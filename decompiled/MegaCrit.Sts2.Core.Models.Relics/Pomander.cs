using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pomander : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(1));

	public override async Task AfterObtained()
	{
		List<CardModel> list = (await CardSelectCmd.FromDeckForUpgrade(prefs: new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			CardCmd.Upgrade(item);
		}
	}
}
