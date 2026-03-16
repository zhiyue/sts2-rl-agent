using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class MadScience : CardModel
{
	public const int attackDamage = 12;

	public const int skillBlock = 8;

	public const string sappingWeakKey = "SappingWeak";

	public const int sappingWeakValue = 2;

	public const string sappingVulnerableKey = "SappingVulnerable";

	public const int sappingVulnerableValue = 2;

	public const string violenceHitsKey = "ViolenceHits";

	public const int violenceHitsValue = 3;

	public const string chokingDamageKey = "ChokingDamage";

	public const int chokingDamageValue = 6;

	public const string energizedEnergyKey = "EnergizedEnergy";

	public const int energizedEnergyValue = 2;

	public const string wisdomCardsKey = "WisdomCards";

	public const int wisdomCardsValue = 3;

	public const string expertiseStrengthKey = "ExpertiseStrength";

	public const int expertiseStrengthValue = 2;

	public const string expertiseDexterityKey = "ExpertiseDexterity";

	public const int expertiseDexterityValue = 2;

	public const string curiousReductionKey = "CuriousReduction";

	public const int curiousReductionValue = 1;

	private CardType _tinkerTimeType;

	private TinkerTime.RiderEffect _tinkerTimeRider;

	private CardModel? _mockedChaosCard;

	public override string PortraitPath => GetPortraitPath(TinkerTimeType);

	public override string BetaPortraitPath => CardModel.MissingPortraitPath;

	public override string[] AllPortraitPaths => new string[3]
	{
		GetPortraitPath(CardType.Attack),
		GetPortraitPath(CardType.Skill),
		GetPortraitPath(CardType.Power)
	};

	public override CardType Type => TinkerTimeType;

	public override TargetType TargetType
	{
		get
		{
			if (TinkerTimeType != CardType.Attack)
			{
				return TargetType.Self;
			}
			return TargetType.AnyEnemy;
		}
	}

	public override bool GainsBlock => TinkerTimeType == CardType.Skill;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[11]
	{
		new DamageVar(12m, ValueProp.Move),
		new BlockVar(8m, ValueProp.Move),
		new PowerVar<WeakPower>("SappingWeak", 2m),
		new PowerVar<VulnerablePower>("SappingVulnerable", 2m),
		new DynamicVar("ViolenceHits", 3m),
		new PowerVar<StranglePower>("ChokingDamage", 6m),
		new EnergyVar("EnergizedEnergy", 2),
		new CardsVar("WisdomCards", 3),
		new PowerVar<StrengthPower>("ExpertiseStrength", 2m),
		new PowerVar<DexterityPower>("ExpertiseDexterity", 2m),
		new DynamicVar("CuriousReduction", 1m)
	});

	[SavedProperty(SerializationCondition.AlwaysSave, -1)]
	public CardType TinkerTimeType
	{
		get
		{
			return _tinkerTimeType;
		}
		set
		{
			AssertMutable();
			_tinkerTimeType = value;
		}
	}

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public TinkerTime.RiderEffect TinkerTimeRider
	{
		get
		{
			return _tinkerTimeRider;
		}
		set
		{
			AssertMutable();
			_tinkerTimeRider = value;
		}
	}

	private CardModel? MockedChaosCard
	{
		get
		{
			return _mockedChaosCard;
		}
		set
		{
			AssertMutable();
			_mockedChaosCard = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			if (TinkerTimeRider != TinkerTime.RiderEffect.None)
			{
				return TinkerTime.GetRiderHoverTips(TinkerTimeRider);
			}
			return Array.Empty<IHoverTip>();
		}
	}

	public MadScience()
		: base(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy, shouldShowInCardLibrary: false)
	{
	}

	private string GetPortraitPath(CardType cardType)
	{
		return ImageHelper.GetImagePath("atlases/card_atlas.sprites/event/" + GetPortraitFilename(cardType) + ".tres");
	}

	private string GetPortraitFilename(CardType cardType)
	{
		return cardType switch
		{
			CardType.Attack => "mad_science_attack", 
			CardType.Skill => "mad_science_skill", 
			CardType.Power => "mad_science_power", 
			_ => throw new InvalidOperationException($"Mad Science is invalid type {TinkerTimeType}."), 
		};
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (TargetType == TargetType.AnyEnemy && cardPlay.Target == null)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		}
		switch (TinkerTimeType)
		{
		case CardType.Attack:
			await ExecuteAttack(choiceContext, cardPlay.Target);
			break;
		case CardType.Skill:
			await ExecuteSkill(cardPlay);
			break;
		case CardType.Power:
			await ExecutePower();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		TinkerTime.RiderEffect tinkerTimeRider = TinkerTimeRider;
		if ((tinkerTimeRider == TinkerTime.RiderEffect.Sapping || (uint)(tinkerTimeRider - 3) <= 3u) ? true : false)
		{
			await ExecuteRider(TinkerTimeRider, cardPlay.Target, choiceContext);
		}
	}

	private async Task ExecuteAttack(PlayerChoiceContext choiceContext, Creature target)
	{
		int hits = ((TinkerTimeRider != TinkerTime.RiderEffect.Violence) ? 1 : base.DynamicVars["ViolenceHits"].IntValue);
		for (int i = 0; i < hits; i++)
		{
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}
	}

	private async Task ExecuteSkill(CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	private async Task ExecutePower()
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		switch (TinkerTimeRider)
		{
		case TinkerTime.RiderEffect.Expertise:
			await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars["ExpertiseStrength"].BaseValue, base.Owner.Creature, this);
			await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, base.DynamicVars["ExpertiseDexterity"].BaseValue, base.Owner.Creature, this);
			break;
		case TinkerTime.RiderEffect.Curious:
			await PowerCmd.Apply<CuriousPower>(base.Owner.Creature, base.DynamicVars["CuriousReduction"].BaseValue, base.Owner.Creature, this);
			break;
		case TinkerTime.RiderEffect.Improvement:
			await PowerCmd.Apply<ImprovementPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
			break;
		}
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		description.Add("CardType", TinkerTimeType.ToString());
		description.Add("HasRider", TinkerTimeRider != TinkerTime.RiderEffect.None);
		TinkerTime.RiderEffect[] values = Enum.GetValues<TinkerTime.RiderEffect>();
		for (int i = 0; i < values.Length; i++)
		{
			TinkerTime.RiderEffect riderEffect = values[i];
			description.Add(riderEffect.ToString(), TinkerTimeRider == riderEffect);
		}
	}

	private async Task ExecuteRider(TinkerTime.RiderEffect rider, Creature? target, PlayerChoiceContext choiceContext)
	{
		switch (rider)
		{
		case TinkerTime.RiderEffect.Sapping:
			await PowerCmd.Apply<WeakPower>(target, base.DynamicVars["SappingWeak"].BaseValue, base.Owner.Creature, this);
			await PowerCmd.Apply<VulnerablePower>(target, base.DynamicVars["SappingVulnerable"].BaseValue, base.Owner.Creature, this);
			break;
		case TinkerTime.RiderEffect.Choking:
			await PowerCmd.Apply<StranglePower>(target, base.DynamicVars["ChokingDamage"].BaseValue, base.Owner.Creature, this);
			break;
		case TinkerTime.RiderEffect.Energized:
			await PlayerCmd.GainEnergy(base.DynamicVars["EnergizedEnergy"].IntValue, base.Owner);
			break;
		case TinkerTime.RiderEffect.Wisdom:
			await CardPileCmd.Draw(choiceContext, base.DynamicVars["WisdomCards"].IntValue, base.Owner);
			break;
		case TinkerTime.RiderEffect.Chaos:
		{
			CardModel cardModel = ((MockedChaosCard == null) ? CardFactory.GetDistinctForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint), 1, base.Owner.RunState.Rng.CombatCardGeneration).First() : MockedChaosCard);
			cardModel.EnergyCost.SetThisTurnOrUntilPlayed(0);
			await CardPileCmd.Add(cardModel, PileType.Hand);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("rider", rider, null);
		}
	}

	public void MockChaosCard(CardModel card)
	{
		AssertMutable();
		card.AssertMutable();
		MockedChaosCard = card;
	}
}
