using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class AromaOfChaos : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, LetGo, "AROMA_OF_CHAOS.pages.INITIAL.options.LET_GO", HoverTipFactory.Static(StaticHoverTip.Transform)),
			new EventOption(this, MaintainControl, "AROMA_OF_CHAOS.pages.INITIAL.options.MAINTAIN_CONTROL")
		});
	}

	private async Task LetGo()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForTransformation(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.TransformToRandom(cardModel, base.Rng, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("AROMA_OF_CHAOS.pages.LET_GO.description"));
	}

	private async Task MaintainControl()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForUpgrade(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Upgrade(cardModel);
		}
		LocString locString = L10NLookup("AROMA_OF_CHAOS.pages.MAINTAIN_CONTROL.description");
		locString.Add("AromaPrinciple", new LocString("characters", base.Owner.Character.Id.Entry + ".aromaPrinciple"));
		SetEventFinished(locString);
	}
}
