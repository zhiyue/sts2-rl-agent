using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class Bugslayer : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("Card1", ModelDb.Card<Exterminate>().Title),
		new StringVar("Card2", ModelDb.Card<Squash>().Title)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Extermination, "BUGSLAYER.pages.INITIAL.options.EXTERMINATION", HoverTipFactory.FromCardWithCardHoverTips<Exterminate>()),
			new EventOption(this, Squash, "BUGSLAYER.pages.INITIAL.options.SQUASH", HoverTipFactory.FromCardWithCardHoverTips<Squash>())
		});
	}

	private async Task Extermination()
	{
		await AddAndPreview<Exterminate>(L10NLookup("BUGSLAYER.pages.EXTERMINATION.description"));
	}

	private async Task Squash()
	{
		await AddAndPreview<Squash>(L10NLookup("BUGSLAYER.pages.SQUASH.description"));
	}

	private async Task AddAndPreview<T>(LocString loc) where T : CardModel
	{
		CardModel card = base.Owner.RunState.CreateCard<T>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
		SetEventFinished(loc);
	}
}
