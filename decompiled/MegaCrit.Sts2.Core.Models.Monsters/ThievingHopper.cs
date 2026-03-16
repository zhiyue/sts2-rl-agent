using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class ThievingHopper : MonsterModel
{
	private static readonly Func<CardModel, bool>[] _stealPriorities = new Func<CardModel, bool>[4]
	{
		(CardModel c) => !(c.Enchantment is Imbued) && c.Rarity == CardRarity.Uncommon,
		delegate(CardModel c)
		{
			bool flag = !(c.Enchantment is Imbued);
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3;
				switch (c.Rarity)
				{
				case CardRarity.Common:
				case CardRarity.Rare:
				case CardRarity.Event:
					flag3 = true;
					break;
				default:
					flag3 = false;
					break;
				}
				flag2 = flag3;
			}
			return flag2;
		},
		delegate(CardModel c)
		{
			bool flag = !(c.Enchantment is Imbued);
			bool flag2 = flag;
			if (flag2)
			{
				CardRarity rarity = c.Rarity;
				bool flag3 = ((rarity == CardRarity.Basic || rarity == CardRarity.Quest) ? true : false);
				flag2 = flag3;
			}
			return flag2;
		},
		(CardModel c) => c.Rarity == CardRarity.Ancient || c.Enchantment is Imbued
	};

	public const string stunTrigger = "StunTrigger";

	private bool _isHovering;

	private const string _fleeTrigger = "Flee";

	private const string _hoverTrigger = "Hover";

	private const string _stealTrigger = "Steal";

	private const string _escapeMoveId = "ESCAPE_MOVE";

	private const string _stealSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_steal";

	private const string _takeOffSfx = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_take_off";

	public const string hoverLoop = "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 84, 79);

	public override int MaxInitialHp => MinInitialHp;

	public bool IsHovering
	{
		get
		{
			return _isHovering;
		}
		set
		{
			AssertMutable();
			_isHovering = value;
		}
	}

	protected override string AttackSfx
	{
		get
		{
			if (!IsHovering)
			{
				return "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack";
			}
			return "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack_hover";
		}
	}

	private string FleeSfx
	{
		get
		{
			if (!IsHovering)
			{
				return "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_flee";
			}
			return "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_flee_hover";
		}
	}

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override string TakeDamageSfx
	{
		get
		{
			if (!IsHovering)
			{
				return base.TakeDamageSfx;
			}
			return "event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hurt_hover";
		}
	}

	private int TheftDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

	private int HatTrickDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 21);

	private int NabDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<EscapeArtistPower>(base.Creature, 5m, base.Creature, null);
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop");
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("THIEVERY_MOVE", ThieveryMove, new SingleAttackIntent(TheftDamage), new CardDebuffIntent());
		MoveState moveState2 = new MoveState("NAB_MOVE", NabMove, new SingleAttackIntent(NabDamage));
		MoveState moveState3 = new MoveState("HAT_TRICK_MOVE", HatTrickMove, new SingleAttackIntent(HatTrickDamage));
		MoveState moveState4 = new MoveState("FLUTTER_MOVE", FlutterMove, new BuffIntent());
		MoveState moveState5 = new MoveState("ESCAPE_MOVE", EscapeMove, new EscapeIntent());
		moveState.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState5;
		moveState5.FollowUpState = moveState5;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(moveState5);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ThieveryMove(IReadOnlyList<Creature> targets)
	{
		NCreature creatureNode = NCombatRoom.Instance?.GetCreatureNode(base.Creature);
		if (creatureNode != null)
		{
			Creature creature = LocalContext.GetMe(targets) ?? targets.First();
			NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(creature);
			Node2D specialNode = creatureNode.GetSpecialNode<Node2D>("Visuals/SpineBoneNode");
			if (specialNode != null)
			{
				specialNode.Position = Vector2.Right * (creatureNode2.GlobalPosition.X - creatureNode.GlobalPosition.X);
			}
		}
		await CreatureCmd.TriggerAnim(base.Creature, "Steal", 0.25f);
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_steal");
		List<CardModel> cardsToSteal = new List<CardModel>();
		foreach (Creature target in targets)
		{
			List<CardModel> list = (from c in CardPile.GetCards(target.Player ?? target.PetOwner, PileType.Draw, PileType.Discard)
				where c.DeckVersion != null
				select c).ToList();
			IEnumerable<CardModel> items = list;
			Func<CardModel, bool>[] stealPriorities = _stealPriorities;
			foreach (Func<CardModel, bool> predicate in stealPriorities)
			{
				IEnumerable<CardModel> enumerable = list.Where(predicate);
				if (enumerable.Any())
				{
					items = enumerable;
					break;
				}
			}
			CardModel cardToSteal = base.RunRng.CombatCardGeneration.NextItem(items);
			await CardPileCmd.RemoveFromCombat(cardToSteal, isBeingPlayed: false);
			cardsToSteal.Add(cardToSteal);
		}
		await Cmd.Wait(0.6f);
		foreach (CardModel item in cardsToSteal)
		{
			if (creatureNode != null && LocalContext.IsMine(item))
			{
				Marker2D specialNode2 = creatureNode.GetSpecialNode<Marker2D>("%StolenCardPos");
				if (specialNode2 != null)
				{
					NCard nCard = NCard.Create(item);
					specialNode2.AddChildSafely(nCard);
					nCard.Position += nCard.Size * 0.5f;
					nCard.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
				}
			}
			SwipePower swipe = (SwipePower)ModelDb.Power<SwipePower>().ToMutable();
			await swipe.Steal(item);
			await PowerCmd.Apply(swipe, base.Creature, 1m, base.Creature, null);
		}
		await DamageCmd.Attack(TheftDamage).FromMonster(this).WithNoAttackerAnim()
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task NabMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(NabDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task HatTrickMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HatTrickDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task FlutterMove(IReadOnlyList<Creature> targets)
	{
		IsHovering = true;
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_take_off");
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop");
		await CreatureCmd.TriggerAnim(base.Creature, "Hover", 0f);
		await Cmd.Wait(1.25f);
		await PowerCmd.Apply<FlutterPower>(base.Creature, 5m, base.Creature, null);
	}

	private async Task EscapeMove(IReadOnlyList<Creature> targets)
	{
		NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.ToggleIsInteractable(on: false);
		SfxCmd.Play(FleeSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Flee", 0.85f);
		if (IsHovering)
		{
			SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop");
			IsHovering = false;
		}
		await Cmd.Wait(1.5f);
		await CreatureCmd.Escape(base.Creature);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true)
		{
			BoundsContainer = "GroundedBounds"
		};
		AnimState state = new AnimState("flee");
		AnimState state2 = new AnimState("flee_hover");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("hurt_hover");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("attack_hover");
		AnimState state3 = new AnimState("die");
		AnimState animState6 = new AnimState("take_off");
		AnimState nextState = new AnimState("hover_loop", isLooping: true)
		{
			BoundsContainer = "FlyingBounds"
		};
		AnimState animState7 = new AnimState("steal");
		animState6.NextState = nextState;
		animState7.NextState = animState;
		animState2.NextState = animState;
		animState3.NextState = nextState;
		animState4.NextState = animState;
		animState5.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("StunTrigger", animState);
		creatureAnimator.AddAnyState("Hover", animState6);
		creatureAnimator.AddAnyState("Steal", animState7);
		creatureAnimator.AddAnyState("Dead", state3);
		creatureAnimator.AddAnyState("Hit", animState3, () => IsHovering);
		creatureAnimator.AddAnyState("Hit", animState2, () => !IsHovering);
		creatureAnimator.AddAnyState("Attack", animState5, () => IsHovering);
		creatureAnimator.AddAnyState("Attack", animState4, () => !IsHovering);
		creatureAnimator.AddAnyState("Flee", state2, () => IsHovering);
		creatureAnimator.AddAnyState("Flee", state, () => !IsHovering);
		return creatureAnimator;
	}
}
