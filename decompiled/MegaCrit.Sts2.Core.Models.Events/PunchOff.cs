using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class PunchOff : EventModel
{
	private CancellationTokenSource? _punchCts;

	public override EventLayoutType LayoutType => EventLayoutType.Combat;

	public override EncounterModel CanonicalEncounter => ModelDb.Encounter<PunchOffEventEncounter>();

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(0));

	public override bool IsAllowed(RunState runState)
	{
		return runState.TotalFloor >= 6;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Nab, "PUNCH_OFF.pages.INITIAL.options.NAB", HoverTipFactory.FromCardWithCardHoverTips<Injury>()),
			new EventOption(this, TakeThem, "PUNCH_OFF.pages.INITIAL.options.I_CAN_TAKE_THEM")
		});
	}

	public override Task AfterEventStarted()
	{
		RunManager.Instance.RoomExited += OnRoomExited;
		base.Owner.CanRemovePotions = false;
		_punchCts = new CancellationTokenSource();
		TaskHelper.RunSafely(PunchEachOther());
		return Task.CompletedTask;
	}

	private async Task PunchEachOther()
	{
		Creature leftEnemy = _combatStateForCombatLayout.Enemies[0];
		Creature rightEnemy = _combatStateForCombatLayout.Enemies[1];
		NCreature leftEnemyNode = NCombatRoom.Instance?.GetCreatureNode(leftEnemy);
		if (leftEnemyNode == null)
		{
			return;
		}
		Vector2 originalScale = leftEnemyNode.Scale;
		leftEnemyNode.Scale = new Vector2(0f - originalScale.X, originalScale.Y);
		Control vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
		while (!_punchCts.IsCancellationRequested)
		{
			await CreatureCmd.TriggerAnim(leftEnemy, "Attack", 0f);
			await Cmd.Wait(0.1f);
			VfxCmd.PlayOnCreatureCenter(rightEnemy, "vfx/vfx_attack_blunt");
			vfxContainer?.AddChildSafely(NHitSparkVfx.Create(rightEnemy, requireInteractable: false));
			await CreatureCmd.TriggerAnim(rightEnemy, "Hit", 0f);
			await Cmd.Wait(1.2f);
			if (_punchCts.IsCancellationRequested)
			{
				break;
			}
			await CreatureCmd.TriggerAnim(rightEnemy, "Attack", 0f);
			await Cmd.Wait(0.1f);
			VfxCmd.PlayOnCreatureCenter(leftEnemy, "vfx/vfx_attack_blunt");
			vfxContainer?.AddChildSafely(NHitSparkVfx.Create(leftEnemy, requireInteractable: false));
			await CreatureCmd.TriggerAnim(leftEnemy, "Hit", 0f);
			await Cmd.Wait(1.2f);
		}
		_punchCts = null;
		if (leftEnemyNode.IsValid())
		{
			leftEnemyNode.Scale = originalScale;
		}
	}

	public override void CalculateVars()
	{
		base.DynamicVars.Gold.BaseValue = base.Rng.NextInt(91, 99);
	}

	protected override void OnEventFinished()
	{
		base.Owner.CanRemovePotions = true;
	}

	private async Task Nab()
	{
		await CardPileCmd.AddCurseToDeck<Injury>(base.Owner);
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Strong);
		NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
		await Cmd.CustomScaledWait(0.25f, 0.5f);
		await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
		{
			new RelicReward(base.Owner)
		});
		SetEventFinished(L10NLookup("PUNCH_OFF.pages.NAB.description"));
	}

	private Task TakeThem()
	{
		_punchCts?.Cancel();
		SetEventState(L10NLookup("PUNCH_OFF.pages.I_CAN_TAKE_THEM.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, Fight, "PUNCH_OFF.pages.I_CAN_TAKE_THEM.options.FIGHT")));
		return Task.CompletedTask;
	}

	private Task Fight()
	{
		base.Owner.CanRemovePotions = true;
		EnterCombatWithoutExitingEvent<PunchOffEventEncounter>(new global::_003C_003Ez__ReadOnlyArray<Reward>(new Reward[2]
		{
			new RelicReward(base.Owner),
			new PotionReward(base.Owner)
		}), shouldResumeAfterCombat: false);
		return Task.CompletedTask;
	}

	private void OnRoomExited()
	{
		RunManager.Instance.RoomExited -= OnRoomExited;
		_punchCts?.Cancel();
	}
}
