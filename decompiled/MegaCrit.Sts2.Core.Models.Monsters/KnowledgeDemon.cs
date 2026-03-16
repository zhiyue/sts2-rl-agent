using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class KnowledgeDemon : MonsterModel
{
	public interface IChoosable
	{
		Task OnChosen();
	}

	private const string _knowledgeDemonCustomTrackName = "knowledge_demon_progress";

	private const string _clapSfx = "event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_clap";

	private const string _flameSfx = "event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_flame";

	private const string _slapSfx = "event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_slap";

	private static readonly LocString _curseOfKnowledgeStartLine = MonsterModel.L10NMonsterLookup("KNOWLEDGE_DEMON.moves.CURSE_OF_KNOWLEDGE.startLine");

	private static readonly LocString _curseOfKnowledgeDoneLine = MonsterModel.L10NMonsterLookup("KNOWLEDGE_DEMON.moves.CURSE_OF_KNOWLEDGE.doneLine");

	private static readonly int[] _disintegrationDamageValues = new int[3] { 6, 7, 8 };

	private static readonly IReadOnlyList<IReadOnlyList<IChoosable>> _curseOfKnowledgeSets = new global::_003C_003Ez__ReadOnlyArray<IReadOnlyList<IChoosable>>(new IReadOnlyList<IChoosable>[3]
	{
		new global::_003C_003Ez__ReadOnlyArray<IChoosable>(new IChoosable[2]
		{
			ModelDb.Card<Disintegration>(),
			ModelDb.Card<MindRot>()
		}),
		new global::_003C_003Ez__ReadOnlyArray<IChoosable>(new IChoosable[2]
		{
			ModelDb.Card<Disintegration>(),
			ModelDb.Card<Sloth>()
		}),
		new global::_003C_003Ez__ReadOnlyArray<IChoosable>(new IChoosable[2]
		{
			ModelDb.Card<Disintegration>(),
			ModelDb.Card<WasteAway>()
		})
	});

	private int _curseOfKnowledgeCounter;

	private const int _knowledgeOverwhelmingRepeat = 3;

	private const int _ponderHeal = 30;

	private bool _isBurnt;

	private const string _mindRotTrigger = "MindRotTrigger";

	private const string _lightAttackTrigger = "LightAttackTrigger";

	private const string _mediumAttackTrigger = "MediumAttackTrigger";

	private const string _heavyAttackTrigger = "HeavyAttackTrigger";

	private const string _healTrigger = "HealTrigger";

	private int CurseOfKnowledgeCounter
	{
		get
		{
			return _curseOfKnowledgeCounter;
		}
		set
		{
			AssertMutable();
			_curseOfKnowledgeCounter = value;
		}
	}

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 399, 379);

	public override int MaxInitialHp => MinInitialHp;

	private int SlapDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 17);

	private int PonderDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);

	private int KnowledgeOverwhelmingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int PonderStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	public bool IsBurnt
	{
		get
		{
			return _isBurnt;
		}
		set
		{
			AssertMutable();
			_isBurnt = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CURSE_OF_KNOWLEDGE_MOVE", CurseOfKnowledge, new DebuffIntent());
		MoveState moveState2 = new MoveState("SLAP_MOVE", SlapMove, new SingleAttackIntent(SlapDamage));
		MoveState moveState3 = new MoveState("KNOWLEDGE_OVERWHELMING_MOVE", KnowledgeOverwhelmingMove, new MultiAttackIntent(KnowledgeOverwhelmingDamage, 3));
		MoveState moveState4 = new MoveState("PONDER_MOVE", PonderMove, new SingleAttackIntent(PonderDamage), new HealIntent(), new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("CurseOfKnowledgeBranch");
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = conditionalBranchState;
		conditionalBranchState.AddState(moveState, () => _curseOfKnowledgeCounter < 3);
		conditionalBranchState.AddState(moveState2, () => _curseOfKnowledgeCounter >= 3);
		list.Add(conditionalBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState4);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task CurseOfKnowledge(IReadOnlyList<Creature> targets)
	{
		if (CurseOfKnowledgeCounter >= _curseOfKnowledgeSets.Count)
		{
			throw new InvalidOperationException($"There are no valid sets at this index {CurseOfKnowledgeCounter}");
		}
		TalkCmd.Play(_curseOfKnowledgeStartLine, base.Creature, 1.0);
		await CreatureCmd.TriggerAnim(base.Creature, "MindRotTrigger", 1f);
		List<Task> list = new List<Task>();
		foreach (Creature target in targets)
		{
			list.Add(ChooseCurse(target));
		}
		await Task.WhenAll(list);
		TalkCmd.Play(_curseOfKnowledgeDoneLine, base.Creature, 1.0);
		CurseOfKnowledgeCounter++;
	}

	private async Task ChooseCurse(Creature target)
	{
		if (target.IsDead)
		{
			return;
		}
		int disintegrationDamage = _disintegrationDamageValues[CurseOfKnowledgeCounter];
		List<CardModel> cards = _curseOfKnowledgeSets[CurseOfKnowledgeCounter].Select(delegate(IChoosable c)
		{
			CardModel cardModel = base.CombatState.CreateCard((CardModel)c, target.Player);
			if (cardModel is Disintegration)
			{
				cardModel.DynamicVars["DisintegrationPower"].BaseValue = disintegrationDamage;
			}
			return cardModel;
		}).ToList();
		await ((IChoosable)(await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, target.Player))).OnChosen();
	}

	private async Task SlapMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlapDamage).FromMonster(this).WithAttackerAnim("MediumAttackTrigger", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_slap")
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
	}

	private async Task KnowledgeOverwhelmingMove(IReadOnlyList<Creature> targets)
	{
		IsBurnt = true;
		NRunMusicController.Instance?.UpdateMusicParameter("knowledge_demon_progress", 2f);
		await DamageCmd.Attack(KnowledgeOverwhelmingDamage).WithHitCount(3).FromMonster(this)
			.WithAttackerAnim("HeavyAttackTrigger", 0.85f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_clap")
			.OnlyPlayAnimOnce()
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
	}

	private async Task PonderMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "HealTrigger", 1.8f);
		NRunMusicController.Instance?.UpdateMusicParameter("knowledge_demon_progress", 1f);
		IsBurnt = false;
		await DamageCmd.Attack(PonderDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		await CreatureCmd.Heal(base.Creature, 30 * base.Creature.CombatState.Players.Count);
		await PowerCmd.Apply<StrengthPower>(base.Creature, PonderStrength, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_light");
		AnimState animState3 = new AnimState("attack_medium");
		AnimState animState4 = new AnimState("attack_heavy");
		AnimState animState5 = new AnimState("brain_rot");
		AnimState animState6 = new AnimState("heal");
		AnimState nextState = new AnimState("burnt_loop", isLooping: true);
		AnimState animState7 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState8 = new AnimState("hurt_burnt");
		AnimState state2 = new AnimState("die_burnt");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = nextState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		animState7.NextState = animState;
		animState8.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("LightAttackTrigger", animState2);
		creatureAnimator.AddAnyState("MediumAttackTrigger", animState3);
		creatureAnimator.AddAnyState("HeavyAttackTrigger", animState4);
		creatureAnimator.AddAnyState("MindRotTrigger", animState5);
		creatureAnimator.AddAnyState("HealTrigger", animState6);
		creatureAnimator.AddAnyState("Dead", state, () => !_isBurnt);
		creatureAnimator.AddAnyState("Hit", animState7, () => !_isBurnt);
		creatureAnimator.AddAnyState("Dead", state2, () => _isBurnt);
		creatureAnimator.AddAnyState("Hit", animState8, () => _isBurnt);
		return creatureAnimator;
	}
}
