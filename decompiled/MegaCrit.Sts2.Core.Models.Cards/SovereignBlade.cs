using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SovereignBlade : CardModel
{
	private const int _baseDamage = 10;

	private const string _sovereignBladeSfx = "event:/sfx/characters/regent/regent_sovereign_blade";

	private bool _createdThroughForge;

	private decimal _currentDamage = 10m;

	private decimal _currentRepeats = 1m;

	public override TargetType TargetType
	{
		get
		{
			if (!HasSeekingEdge)
			{
				return TargetType.AnyEnemy;
			}
			return TargetType.AllEnemies;
		}
	}

	protected override IEnumerable<string> ExtraRunAssetPaths => NSovereignBladeVfx.AssetPaths;

	private decimal CurrentDamage
	{
		get
		{
			return _currentDamage;
		}
		set
		{
			AssertMutable();
			_currentDamage = value;
		}
	}

	private decimal CurrentRepeats
	{
		get
		{
			return _currentRepeats;
		}
		set
		{
			AssertMutable();
			_currentRepeats = value;
		}
	}

	public bool CreatedThroughForge
	{
		get
		{
			return _createdThroughForge;
		}
		set
		{
			AssertMutable();
			_createdThroughForge = value;
		}
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Retain);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[5]
	{
		new DamageVar(10m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("SeekingEdgeAmount").WithMultiplier((CardModel card, Creature? _) => (card != null && card.IsMutable && card.Owner != null) ? card.Owner.Creature.GetPowerAmount<SeekingEdgePower>() : 0),
		new RepeatVar(1)
	});

	private bool HasSeekingEdge
	{
		get
		{
			if (CombatManager.Instance.IsInProgress)
			{
				return base.Owner.Creature.HasPower<SeekingEdgePower>();
			}
			return false;
		}
	}

	public SovereignBlade()
		: base(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		AttackCommand attack = DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(base.DynamicVars.Repeat.IntValue)
			.WithAttackerAnim("Cast", base.Owner.Character.AttackAnimDelay)
			.WithAttackerFx(null, "event:/sfx/characters/regent/regent_sovereign_blade");
		if (HasSeekingEdge)
		{
			attack = attack.TargetingAllOpponents(base.CombatState).BeforeDamage(delegate
			{
				NSovereignBladeVfx vfxNode = GetVfxNode(base.Owner, this);
				IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
				if (hittableEnemies.Count > 0)
				{
					NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(hittableEnemies[0]);
					if (vfxNode != null && nCreature != null)
					{
						vfxNode.Attack(nCreature.VfxSpawnPosition);
					}
				}
				return Task.CompletedTask;
			}).WithHitFx("vfx/vfx_giant_horizontal_slash", null, "slash_attack.mp3");
		}
		else
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			attack = attack.Targeting(cardPlay.Target).BeforeDamage(delegate
			{
				NSovereignBladeVfx vfxNode = GetVfxNode(base.Owner, this);
				NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
				if (vfxNode != null && nCreature != null)
				{
					vfxNode.Attack(nCreature.VfxSpawnPosition);
				}
				return Task.CompletedTask;
			}).WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t))
				.WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t));
		}
		await attack.Execute(choiceContext);
		ParryPower power = base.Owner.Creature.GetPower<ParryPower>();
		if (power != null)
		{
			await power.AfterSovereignBladePlayed(base.Owner.Creature, attack.Results);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		CreatedThroughForge = false;
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue = CurrentDamage;
		base.DynamicVars.Repeat.BaseValue = CurrentRepeats;
	}

	public override void AfterTransformedFrom()
	{
		RemoveSovereignBladeNode();
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (card != this)
		{
			return Task.CompletedTask;
		}
		if ((!CreatedThroughForge && oldPileType == PileType.None) || oldPileType == PileType.Exhaust)
		{
			ForgeCmd.PlayCombatRoomForgeVfx(base.Owner, this);
		}
		if (card.Pile.Type == PileType.Exhaust)
		{
			RemoveSovereignBladeNode();
		}
		return Task.CompletedTask;
	}

	public void AddDamage(decimal amount)
	{
		base.DynamicVars.Damage.BaseValue += amount;
		CurrentDamage = base.DynamicVars.Damage.BaseValue;
	}

	public void SetRepeats(decimal amount)
	{
		base.DynamicVars.Repeat.BaseValue = amount;
		CurrentRepeats = base.DynamicVars.Repeat.BaseValue;
	}

	public static NSovereignBladeVfx? GetVfxNode(Player player, CardModel card)
	{
		CardModel originalCard = card.DupeOf ?? card;
		return (NCombatRoom.Instance?.GetCreatureNode(player.Creature))?.GetChildren().OfType<NSovereignBladeVfx>().FirstOrDefault((NSovereignBladeVfx b) => b.Card == originalCard);
	}

	private void RemoveSovereignBladeNode()
	{
		GetVfxNode(base.Owner, this)?.RemoveSovereignBlade();
	}
}
