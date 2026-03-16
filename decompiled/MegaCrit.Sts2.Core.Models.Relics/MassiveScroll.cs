using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MassiveScroll : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		IEnumerable<CardModel> customCardPool = from c in ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(base.Owner.RunState.UnlockState, base.Owner.RunState.CardMultiplayerConstraint).Concat(base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.RunState.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
			where c.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly
			select c;
		CardCreationOptions options = new CardCreationOptions(customCardPool, CardCreationSource.Other, CardRarityOddsType.RegularEncounter);
		List<CardModel> options2 = (from r in CardFactory.CreateForReward(base.Owner, 3, options)
			select r.Card).ToList();
		CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options2, base.Owner, canSkip: true);
		if (chosenCard != null)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosenCard, PileType.Deck));
		}
		foreach (CardModel item in options2)
		{
			if (item != chosenCard)
			{
				base.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(base.Owner.NetId).CardChoices.Add(new CardChoiceHistoryEntry(item, wasPicked: false));
			}
		}
	}
}
