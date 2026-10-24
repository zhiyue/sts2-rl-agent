using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class StoneCracker : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			Flash();
			List<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable).ToList().StableShuffle(base.Owner.RunState.Rng.CombatCardSelection)
				.Take(base.DynamicVars.Cards.IntValue)
				.ToList();
			CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
			CardCmd.Preview(cards);
			await Cmd.CustomScaledWait(0.5f, 1f);
		}
	}
}
