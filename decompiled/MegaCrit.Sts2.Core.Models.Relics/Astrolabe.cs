using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Astrolabe : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	public override async Task AfterObtained()
	{
		List<CardModel> list = (await CardSelectCmd.FromDeckForTransformation(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			CardModel cardModel = CardFactory.CreateRandomCardForTransform(item, isInCombat: false, base.Owner.RunState.Rng.Niche);
			CardCmd.Upgrade(cardModel);
			await CardCmd.Transform(item, cardModel);
		}
	}
}
