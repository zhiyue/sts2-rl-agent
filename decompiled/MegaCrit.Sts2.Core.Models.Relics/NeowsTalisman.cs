using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class NeowsTalisman : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	public override Task AfterObtained()
	{
		List<CardModel> source = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.Rarity == CardRarity.Basic).ToList();
		IEnumerable<CardModel> enumerable = new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[2]
		{
			source.LastOrDefault((CardModel c) => c.Tags.Contains(CardTag.Strike)),
			source.LastOrDefault((CardModel c) => c.Tags.Contains(CardTag.Defend))
		});
		foreach (CardModel item in enumerable)
		{
			if (item != null)
			{
				CardCmd.Upgrade(item);
			}
		}
		return Task.CompletedTask;
	}
}
