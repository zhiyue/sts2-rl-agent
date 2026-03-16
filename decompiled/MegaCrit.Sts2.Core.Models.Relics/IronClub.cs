using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class IronClub : RelicModel
{
	private bool _isActivating;

	private int _cardsPlayed;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CardsPlayed % base.DynamicVars.Cards.IntValue;
			}
			return base.DynamicVars.Cards.IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(4));

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
			UpdateDisplay();
		}
	}

	[SavedProperty]
	public int CardsPlayed
	{
		get
		{
			return _cardsPlayed;
		}
		set
		{
			AssertMutable();
			_cardsPlayed = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			base.Status = RelicStatus.Normal;
		}
		else
		{
			int intValue = base.DynamicVars.Cards.IntValue;
			base.Status = ((CardsPlayed % intValue == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner)
		{
			CardsPlayed++;
			int intValue = base.DynamicVars.Cards.IntValue;
			if (CombatManager.Instance.IsInProgress && CardsPlayed % intValue == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CardPileCmd.Draw(context, 1m, base.Owner);
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
