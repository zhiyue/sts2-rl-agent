using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class WarHammer : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(4));

	public override Task AfterCombatVictory(CombatRoom room)
	{
		if (room.RoomType != RoomType.Elite)
		{
			return Task.CompletedTask;
		}
		Flash();
		IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable).ToList().StableShuffle(base.Owner.RunState.Rng.Niche)
			.Take(base.DynamicVars.Cards.IntValue);
		foreach (CardModel item in enumerable)
		{
			CardCmd.Upgrade(item);
		}
		return Task.CompletedTask;
	}
}
