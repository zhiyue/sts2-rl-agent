using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Doormaker : MonsterModel
{
	private const string _doormakerTrackName = "queen_progress";

	private readonly string[] _visualsPaths = new string[3] { "monsters/beta/door_maker_placeholder_2.png", "monsters/beta/door_maker_placeholder_3.png", "monsters/beta/door_maker_placeholder_4.png" };

	private static readonly List<string> _whatIsItLines;

	private int _timesGotBackIn;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 512, 489);

	public override int MaxInitialHp => MinInitialHp;

	private int LaserBeamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 34, 31);

	private int GetBackInMoveDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 45, 40);

	private int StrengthScale => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int DoorHpScale => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 25, 20);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public int TimesGotBackIn
	{
		get
		{
			return _timesGotBackIn;
		}
		private set
		{
			AssertMutable();
			_timesGotBackIn = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("WHAT_IS_IT_MOVE", WhatIsItMove, new StunIntent());
		MoveState moveState2 = new MoveState("BEAM_MOVE", BeamMove, new SingleAttackIntent(LaserBeamDamage));
		MoveState moveState3 = new MoveState("GET_BACK_IN_MOVE", GetBackInMove, new SingleAttackIntent(GetBackInMoveDamage), new BuffIntent(), new EscapeIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState3;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task WhatIsItMove(IReadOnlyList<Creature> targets)
	{
		Creature creature = base.Creature.CombatState.GetTeammatesOf(base.Creature).FirstOrDefault((Creature c) => c.Monster is Door);
		if (creature == null)
		{
			goto IL_0065;
		}
		if (creature.IsDead)
		{
			DoorRevivalPower? power = creature.GetPower<DoorRevivalPower>();
			if (power == null || !power.IsHalfDead)
			{
				goto IL_0065;
			}
		}
		TalkCmd.Play(MonsterModel.L10NMonsterLookup(base.Rng.NextItem(_whatIsItLines)), base.Creature);
		goto IL_00b2;
		IL_0065:
		TalkCmd.Play(MonsterModel.L10NMonsterLookup("DOORMAKER.moves.WHAT_IS_IT.deadDoorSpeakLine"), base.Creature);
		goto IL_00b2;
		IL_00b2:
		return Task.CompletedTask;
	}

	private async Task BeamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LaserBeamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task GetBackInMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(GetBackInMoveDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		if (base.Creature.IsDead)
		{
			return;
		}
		await PowerCmd.Apply<StrengthPower>(base.Creature, 5m, base.Creature, null);
		TimesGotBackIn++;
		Creature door = base.Creature.CombatState.GetTeammatesOf(base.Creature).FirstOrDefault((Creature c) => c.Monster is Door);
		if (door != null)
		{
			DoorRevivalPower power = door.GetPower<DoorRevivalPower>();
			if (power != null)
			{
				await power.DoRevive();
			}
			await PowerCmd.SetAmount<StrengthPower>(door, StrengthScale * TimesGotBackIn, base.Creature, null);
			await CreatureCmd.SetMaxAndCurrentHp(door, door.MaxHp + DoorHpScale * TimesGotBackIn);
			await Cmd.Wait(0.25f);
			await AnimOut();
			CombatManager.Instance.RemoveCreature(base.Creature);
			base.CombatState.RemoveCreature(base.Creature, unattach: false);
			await Cmd.Wait(0.25f);
		}
		else
		{
			await AnimOut();
			await CreatureCmd.Escape(base.Creature, removeCreatureNode: false);
		}
	}

	private async Task RemoveCreatureWhenDone(NCreature creatureNode, Tween tween)
	{
		await tween.ToSignal(tween, Tween.SignalName.Finished);
		NCombatRoom.Instance?.RemoveCreatureNode(creatureNode);
		creatureNode.QueueFreeSafely();
	}

	private async Task AnimOut()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(base.Creature);
		if (nCreature != null)
		{
			Vector2 scale = nCreature.Visuals.Body.Scale;
			nCreature.ToggleIsInteractable(on: false);
			Tween tween = nCreature.CreateTween();
			tween.TweenProperty(nCreature.Visuals.Body, "scale", scale * 0.5f, 1.2000000476837158).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.Parallel().TweenProperty(nCreature.Visuals.Body, "modulate", Colors.Black, 0.25).From(Colors.White);
			tween.Parallel().TweenProperty(nCreature.Visuals.Body, "modulate", Colors.Transparent, 0.25).From(Colors.Black)
				.SetDelay(0.25);
			TaskHelper.RunSafely(RemoveCreatureWhenDone(nCreature, tween));
			await Cmd.CustomScaledWait(0.2f, 0.6f);
		}
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != base.Creature)
		{
			return Task.CompletedTask;
		}
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 5f);
		return Task.CompletedTask;
	}

	public async Task AnimIn()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(base.Creature);
		NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 1f);
		if (nCreature != null)
		{
			Vector2 scale = nCreature.Visuals.Body.Scale;
			((Sprite2D)nCreature.Visuals.Body).Texture = PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath(_visualsPaths[TimesGotBackIn % _visualsPaths.Length]));
			Tween tween = nCreature.CreateTween();
			tween.TweenProperty(nCreature.Visuals.Body, "scale", scale, 1.2000000476837158).From(scale * 0.5f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);
			tween.Parallel().TweenProperty(nCreature.Visuals.Body, "modulate", Colors.Black, 0.25).From(Colors.Transparent);
			tween.Parallel().TweenProperty(nCreature.Visuals.Body, "modulate", Colors.White, 0.25).From(Colors.Black)
				.SetDelay(0.25);
			TaskHelper.RunSafely(PlayLineAfterAnimIn(tween));
			await Cmd.CustomScaledWait(0.2f, 0.6f);
		}
	}

	private async Task PlayLineAfterAnimIn(Tween tween)
	{
		await tween.ToSignal(tween, Tween.SignalName.Finished);
		if (base.Creature.CombatState == null)
		{
			return;
		}
		Creature creature = base.Creature.CombatState.GetTeammatesOf(base.Creature).FirstOrDefault((Creature c) => c.Monster is Door);
		int num;
		if (creature != null)
		{
			if (creature.IsDead)
			{
				DoorRevivalPower? power = creature.GetPower<DoorRevivalPower>();
				num = ((power == null || !power.IsHalfDead) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		LocString line = ((num != 0) ? MonsterModel.L10NMonsterLookup("DOORMAKER.moves.WHAT_IS_IT.deadDoorSpeakLine") : ((TimesGotBackIn != 0) ? MonsterModel.L10NMonsterLookup(base.Rng.NextItem(_whatIsItLines)) : MonsterModel.L10NMonsterLookup("DOORMAKER.moves.WHAT_IS_IT.speakLineInitial")));
		TalkCmd.Play(line, base.Creature);
	}

	static Doormaker()
	{
		int num = 2;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "DOORMAKER.moves.WHAT_IS_IT.speakLine1";
		num2++;
		span[num2] = "DOORMAKER.moves.WHAT_IS_IT.speakLine2";
		_whatIsItLines = list;
	}
}
