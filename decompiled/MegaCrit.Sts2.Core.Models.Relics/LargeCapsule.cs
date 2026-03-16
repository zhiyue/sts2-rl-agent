using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LargeCapsule : RelicModel
{
	private const string _relicKey = "Relics";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new IntVar("Relics", 2m));

	public override async Task AfterObtained()
	{
		for (int i = 0; i < base.DynamicVars["Relics"].IntValue; i++)
		{
			RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
			await RelicCmd.Obtain(relic, base.Owner);
		}
		List<CardPileAddResult> list = new List<CardPileAddResult>(2);
		List<CardPileAddResult> list2 = list;
		list2.Add(await CardPileCmd.Add(base.Owner.RunState.CreateCard(GetStrikeForCharacter(base.Owner.Character), base.Owner), PileType.Deck));
		List<CardPileAddResult> list3 = list;
		list3.Add(await CardPileCmd.Add(base.Owner.RunState.CreateCard(GetDefendForCharacter(base.Owner.Character), base.Owner), PileType.Deck));
		CardCmd.PreviewCardPileAdd(list, 2f);
	}

	private static CardModel GetStrikeForCharacter(CharacterModel character)
	{
		return character.CardPool.AllCards.First((CardModel c) => c.Rarity == CardRarity.Basic && c.Tags.Contains(CardTag.Strike));
	}

	private static CardModel GetDefendForCharacter(CharacterModel character)
	{
		return character.CardPool.AllCards.First((CardModel c) => c.Rarity == CardRarity.Basic && c.Tags.Contains(CardTag.Defend));
	}
}
