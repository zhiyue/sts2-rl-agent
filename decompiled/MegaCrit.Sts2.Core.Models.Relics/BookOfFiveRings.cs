using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BookOfFiveRings : RelicModel
{
	private bool _isActivating;

	private int _cardsAdded;

	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CardsAddedSinceLastTrigger;
			}
			return base.DynamicVars.Cards.IntValue;
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

	[SavedProperty]
	public int CardsAdded
	{
		get
		{
			return _cardsAdded;
		}
		set
		{
			AssertMutable();
			_cardsAdded = value;
			InvokeDisplayAmountChanged();
		}
	}

	private int CardsAddedSinceLastTrigger => CardsAdded % base.DynamicVars.Cards.IntValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(5),
		new HealVar(15m)
	});

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (base.Owner.Creature.IsDead || card.Owner != base.Owner)
		{
			return;
		}
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck)
		{
			CardsAdded++;
			if (CardsAddedSinceLastTrigger == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
			}
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
