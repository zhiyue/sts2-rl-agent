using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TinkerTime : EventModel
{
	public enum RiderEffect
	{
		None,
		Sapping,
		Violence,
		Choking,
		Energized,
		Wisdom,
		Chaos,
		Expertise,
		Curious,
		Improvement
	}

	private CardType _chosenCardType;

	private CardType ChosenCardType
	{
		get
		{
			return _chosenCardType;
		}
		set
		{
			AssertMutable();
			_chosenCardType = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[12]
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
		new DynamicVar("CuriousReduction", 1m),
		new EnergyVar("energyPrefix", 1)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, ChooseCardType, "TINKER_TIME.pages.INITIAL.options.CHOOSE_CARD_TYPE"));
	}

	private Task ChooseCardType()
	{
		IEnumerable<EventOption> collection = new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
		{
			new EventOption(this, Attack, "TINKER_TIME.pages.CHOOSE_CARD_TYPE.options.ATTACK", GetCardTypeHoverTip(CardType.Attack)),
			new EventOption(this, Skill, "TINKER_TIME.pages.CHOOSE_CARD_TYPE.options.SKILL", GetCardTypeHoverTip(CardType.Skill)),
			new EventOption(this, Power, "TINKER_TIME.pages.CHOOSE_CARD_TYPE.options.POWER", GetCardTypeHoverTip(CardType.Power))
		});
		SetEventState(L10NLookup("TINKER_TIME.pages.CHOOSE_CARD_TYPE.description"), collection.TakeRandom(2, base.Rng));
		return Task.CompletedTask;
	}

	private CardHoverTip GetCardTypeHoverTip(CardType cardType)
	{
		MadScience madScience = base.Owner.RunState.CreateCard<MadScience>(base.Owner);
		madScience.TinkerTimeType = cardType;
		madScience.TinkerTimeRider = RiderEffect.None;
		return new CardHoverTip(madScience);
	}

	private Task Attack()
	{
		ChosenCardType = CardType.Attack;
		return ChooseRiderEffect();
	}

	private Task Skill()
	{
		ChosenCardType = CardType.Skill;
		return ChooseRiderEffect();
	}

	private Task Power()
	{
		ChosenCardType = CardType.Power;
		return ChooseRiderEffect();
	}

	private Task ChooseRiderEffect()
	{
		List<RiderEffect> riders = (ChosenCardType switch
		{
			CardType.Attack => new global::_003C_003Ez__ReadOnlyArray<RiderEffect>(new RiderEffect[3]
			{
				RiderEffect.Sapping,
				RiderEffect.Violence,
				RiderEffect.Choking
			}), 
			CardType.Skill => new global::_003C_003Ez__ReadOnlyArray<RiderEffect>(new RiderEffect[3]
			{
				RiderEffect.Energized,
				RiderEffect.Wisdom,
				RiderEffect.Chaos
			}), 
			CardType.Power => new global::_003C_003Ez__ReadOnlyArray<RiderEffect>(new RiderEffect[3]
			{
				RiderEffect.Expertise,
				RiderEffect.Curious,
				RiderEffect.Improvement
			}), 
			_ => throw new ArgumentOutOfRangeException(), 
		}).TakeRandom(2, base.Rng).ToList();
		SetEventState(L10NLookup("TINKER_TIME.pages.CHOOSE_RIDER.description"), new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, () => RiderChosen(riders[0]), GetRiderLocKey(riders[0]), GetRiderHoverTip(riders[0])),
			new EventOption(this, () => RiderChosen(riders[1]), GetRiderLocKey(riders[1]), GetRiderHoverTip(riders[1]))
		}));
		return Task.CompletedTask;
	}

	private CardHoverTip GetRiderHoverTip(RiderEffect rider)
	{
		MadScience madScience = base.Owner.RunState.CreateCard<MadScience>(base.Owner);
		madScience.TinkerTimeType = ChosenCardType;
		madScience.TinkerTimeRider = rider;
		return new CardHoverTip(madScience);
	}

	private async Task RiderChosen(RiderEffect rider)
	{
		MadScience madScience = base.Owner.RunState.CreateCard<MadScience>(base.Owner);
		madScience.TinkerTimeType = ChosenCardType;
		madScience.TinkerTimeRider = rider;
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(madScience, PileType.Deck), 3f);
		SetEventFinished(L10NLookup("TINKER_TIME.pages.DONE.description"));
	}

	private static string GetRiderLocKey(RiderEffect rider)
	{
		return rider switch
		{
			RiderEffect.None => throw new ArgumentOutOfRangeException("rider", rider, "None is not a valid rider"), 
			RiderEffect.Sapping => "TINKER_TIME.pages.CHOOSE_RIDER.options.SAPPING", 
			RiderEffect.Violence => "TINKER_TIME.pages.CHOOSE_RIDER.options.VIOLENCE", 
			RiderEffect.Choking => "TINKER_TIME.pages.CHOOSE_RIDER.options.CHOKING", 
			RiderEffect.Energized => "TINKER_TIME.pages.CHOOSE_RIDER.options.ENERGIZED", 
			RiderEffect.Wisdom => "TINKER_TIME.pages.CHOOSE_RIDER.options.WISDOM", 
			RiderEffect.Chaos => "TINKER_TIME.pages.CHOOSE_RIDER.options.CHAOS", 
			RiderEffect.Expertise => "TINKER_TIME.pages.CHOOSE_RIDER.options.EXPERTISE", 
			RiderEffect.Curious => "TINKER_TIME.pages.CHOOSE_RIDER.options.CURIOUS", 
			RiderEffect.Improvement => "TINKER_TIME.pages.CHOOSE_RIDER.options.IMPROVEMENT", 
			_ => throw new ArgumentOutOfRangeException("rider", rider, null), 
		};
	}

	public static IHoverTip[] GetRiderHoverTips(RiderEffect rider)
	{
		return rider switch
		{
			RiderEffect.None => Array.Empty<IHoverTip>(), 
			RiderEffect.Sapping => new IHoverTip[2]
			{
				HoverTipFactory.FromPower<WeakPower>(),
				HoverTipFactory.FromPower<VulnerablePower>()
			}, 
			RiderEffect.Violence => Array.Empty<IHoverTip>(), 
			RiderEffect.Choking => new IHoverTip[1] { HoverTipFactory.FromPower<StranglePower>() }, 
			RiderEffect.Energized => Array.Empty<IHoverTip>(), 
			RiderEffect.Wisdom => Array.Empty<IHoverTip>(), 
			RiderEffect.Chaos => Array.Empty<IHoverTip>(), 
			RiderEffect.Expertise => new IHoverTip[2]
			{
				HoverTipFactory.FromPower<StrengthPower>(),
				HoverTipFactory.FromPower<DexterityPower>()
			}, 
			RiderEffect.Curious => Array.Empty<IHoverTip>(), 
			RiderEffect.Improvement => Array.Empty<IHoverTip>(), 
			_ => throw new ArgumentOutOfRangeException("rider", rider, null), 
		};
	}
}
