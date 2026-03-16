using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class Reflections : EventModel
{
	public override void OnRoomEnter()
	{
		NEventRoom.Instance?.VfxContainer?.AddChildSafely(NMirrorVfx.Create());
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, TouchAMirror, "REFLECTIONS.pages.INITIAL.options.TOUCH_A_MIRROR"),
			new EventOption(this, Shatter, "REFLECTIONS.pages.INITIAL.options.SHATTER", HoverTipFactory.FromCardWithCardHoverTips<BadLuck>())
		});
	}

	private async Task TouchAMirror()
	{
		List<CardModel> upgradedCards = base.Owner.Deck.Cards.Where((CardModel c) => c.IsUpgraded).ToList();
		for (int i = 0; i < 2; i++)
		{
			if (upgradedCards.Count <= 0)
			{
				break;
			}
			CardModel cardModel = base.Rng.NextItem(upgradedCards);
			upgradedCards.Remove(cardModel);
			CardCmd.Downgrade(cardModel);
			CardCmd.Preview(cardModel, 1.2f, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.3f, 0.5f);
		}
		List<CardModel> upgradableCards = base.Owner.Deck.Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		for (int i = 0; i < 4; i++)
		{
			if (upgradableCards.Count <= 0)
			{
				break;
			}
			CardModel cardModel2 = base.Rng.NextItem(upgradableCards);
			upgradableCards.Remove(cardModel2);
			CardCmd.Upgrade(cardModel2, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.3f, 0.5f);
		}
		await Cmd.CustomScaledWait(0.6f, 1.2f);
		SetEventFinished(L10NLookup("REFLECTIONS.pages.TOUCH_A_MIRROR.description"));
	}

	private async Task Shatter()
	{
		int originalDeckSize = base.Owner.Deck.Cards.Count;
		for (int i = 0; i < originalDeckSize; i++)
		{
			CardModel card = base.Owner.RunState.CloneCard(base.Owner.Deck.Cards[i]);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1.2f, CardPreviewStyle.MessyLayout);
			await Cmd.CustomScaledWait(0.1f, 0.2f);
		}
		await Cmd.CustomScaledWait(0.6f, 1.2f);
		await CardPileCmd.AddCurseToDeck<BadLuck>(base.Owner);
		SetEventFinished(L10NLookup("REFLECTIONS.pages.SHATTER.description"));
	}
}
