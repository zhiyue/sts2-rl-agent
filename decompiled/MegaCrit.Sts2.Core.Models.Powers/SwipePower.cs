using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SwipePower : PowerModel
{
	private CardModel? _stolenCard;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool IsInstanced => true;

	public CardModel? StolenCard
	{
		get
		{
			return _stolenCard;
		}
		set
		{
			AssertMutable();
			_stolenCard = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			if (StolenCard == null)
			{
				return Array.Empty<IHoverTip>();
			}
			return new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard(StolenCard));
		}
	}

	public override Task BeforeDeath(Creature target)
	{
		if (base.Owner != target)
		{
			return Task.CompletedTask;
		}
		if (StolenCard?.DeckVersion == null)
		{
			return Task.CompletedTask;
		}
		IRunState runState = base.CombatState.RunState;
		runState.AddCard(StolenCard.DeckVersion, base.Target.Player);
		SpecialCardReward specialCardReward = new SpecialCardReward(StolenCard.DeckVersion, base.Target.Player);
		specialCardReward.SetCustomDescriptionEncounterSource(ModelDb.Encounter<ThievingHopperWeak>().Id);
		((CombatRoom)runState.CurrentRoom).AddExtraReward(base.Target.Player, specialCardReward);
		return Task.CompletedTask;
	}

	public async Task Steal(CardModel card)
	{
		base.Target = card.Owner.Creature;
		StolenCard = card;
		if (card.DeckVersion != null)
		{
			await CardPileCmd.RemoveFromDeck(card.DeckVersion, showPreview: false);
		}
	}
}
