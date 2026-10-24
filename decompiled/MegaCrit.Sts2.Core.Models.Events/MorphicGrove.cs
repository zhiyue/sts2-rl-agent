using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class MorphicGrove : EventModel
{
	private const int _transformCount = 2;

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(5m));

	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.All((Player p) => p.Gold >= 100 && p.Deck.Cards.Count((CardModel c) => c.IsTransformable) >= 2);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Group, "MORPHIC_GROVE.pages.INITIAL.options.GROUP", HoverTipFactory.Static(StaticHoverTip.Transform)),
			new EventOption(this, Loner, "MORPHIC_GROVE.pages.INITIAL.options.LONER")
		});
	}

	private async Task Loner()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
		SetEventFinished(L10NLookup("MORPHIC_GROVE.pages.LONER.description"));
	}

	private async Task Group()
	{
		await PlayerCmd.LoseGold(base.Owner.Gold, base.Owner, GoldLossType.Stolen);
		List<CardModel> list = (await CardSelectCmd.FromDeckForTransformation(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 2), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			await CardCmd.TransformToRandom(item, base.Rng, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("MORPHIC_GROVE.pages.GROUP.description"));
	}
}
