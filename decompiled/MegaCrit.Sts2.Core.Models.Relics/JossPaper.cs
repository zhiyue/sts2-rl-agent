using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class JossPaper : RelicModel
{
	private const string _exhaustAmountKey = "ExhaustAmount";

	private bool _isActivating;

	private int _cardsExhausted;

	private int _etherealCount;

	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CardsExhausted;
			}
			return base.DynamicVars["ExhaustAmount"].IntValue;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
	public int CardsExhausted
	{
		get
		{
			return _cardsExhausted;
		}
		set
		{
			AssertMutable();
			_cardsExhausted = value;
			base.Status = (((decimal)_cardsExhausted == base.DynamicVars["ExhaustAmount"].BaseValue - 1m) ? RelicStatus.Active : RelicStatus.Normal);
			InvokeDisplayAmountChanged();
		}
	}

	private int EtherealCount
	{
		get
		{
			return _etherealCount;
		}
		set
		{
			AssertMutable();
			_etherealCount = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("ExhaustAmount", 5m),
		new CardsVar(1)
	});

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner == base.Owner)
		{
			if (causedByEthereal)
			{
				EtherealCount++;
				return;
			}
			CardsExhausted++;
			await DrawIfThresholdMet(choiceContext);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player)
		{
			CardsExhausted += EtherealCount;
			EtherealCount = 0;
			await DrawIfThresholdMet(choiceContext);
		}
	}

	private async Task DrawIfThresholdMet(PlayerChoiceContext choiceContext)
	{
		if (!((decimal)CardsExhausted < base.DynamicVars["ExhaustAmount"].BaseValue))
		{
			TaskHelper.RunSafely(DoActivateVisuals());
			await CardPileCmd.Draw(choiceContext, (int)((decimal)CardsExhausted / base.DynamicVars["ExhaustAmount"].BaseValue), base.Owner);
			CardsExhausted %= base.DynamicVars["ExhaustAmount"].IntValue;
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
