using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SandCastle : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(6));

	public override Task AfterObtained()
	{
		IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c?.IsUpgradable ?? false).ToList().StableShuffle(base.Owner.RunState.Rng.Niche)
			.Take(base.DynamicVars.Cards.IntValue);
		NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);
		foreach (CardModel item in enumerable)
		{
			CardCmd.Upgrade(item, CardPreviewStyle.GridLayout);
		}
		return Task.CompletedTask;
	}
}
