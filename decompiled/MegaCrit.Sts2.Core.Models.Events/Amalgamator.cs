using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class Amalgamator : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("Card1", ModelDb.Card<UltimateStrike>().Title),
		new StringVar("Card2", ModelDb.Card<UltimateDefend>().Title)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => p.Deck.Cards.Count((CardModel c) => IsValid(CardTag.Strike, c)) >= 2 && p.Deck.Cards.Count((CardModel c) => IsValid(CardTag.Defend, c)) >= 2);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, CombineStrikes, InitialOptionKey("COMBINE_STRIKES"), HoverTipFactory.FromCardWithCardHoverTips<UltimateStrike>()),
			new EventOption(this, CombineDefends, InitialOptionKey("COMBINE_DEFENDS"), HoverTipFactory.FromCardWithCardHoverTips<UltimateDefend>())
		});
	}

	private async Task CombineStrikes()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2), player: base.Owner, filter: (CardModel c) => IsValid(CardTag.Strike, c))).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		NDebugAudioManager.Instance?.Play("card_smith.mp3", 1f, PitchVariance.Small);
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Strong);
		await Task.Delay(300);
		NDebugAudioManager.Instance?.Play("card_smith.mp3", 1f, PitchVariance.Small);
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Strong);
		CardModel card = base.Owner.RunState.CreateCard<UltimateStrike>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("AMALGAMATOR.pages.COMBINE_STRIKES.description"));
	}

	private async Task CombineDefends()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2), player: base.Owner, filter: (CardModel c) => IsValid(CardTag.Defend, c))).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		NDebugAudioManager.Instance?.Play("card_smith.mp3", 1f, PitchVariance.Small);
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Strong);
		await Task.Delay(300);
		NDebugAudioManager.Instance?.Play("card_smith.mp3", 1f, PitchVariance.Small);
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Strong);
		CardModel card = base.Owner.RunState.CreateCard<UltimateDefend>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("AMALGAMATOR.pages.COMBINE_DEFENDS.description"));
	}

	private static bool IsValid(CardTag tag, CardModel card)
	{
		if (card.Tags.Contains(tag))
		{
			if (card != null && card.Rarity == CardRarity.Basic)
			{
				return card.IsRemovable;
			}
			return false;
		}
		return false;
	}
}
