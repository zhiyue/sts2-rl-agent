using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Bookmark : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Retain));

	public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		if (player != base.Owner)
		{
			return Task.CompletedTask;
		}
		List<CardModel> list = retainedCards.Where((CardModel c) => !c.EnergyCost.CostsX && c.EnergyCost.GetWithModifiers(CostModifiers.Local) > 0).ToList();
		if (list.Count == 0)
		{
			return Task.CompletedTask;
		}
		Flash();
		base.Owner.RunState.Rng.CombatCardSelection.NextItem(list)?.EnergyCost.AddUntilPlayed(-1);
		return Task.CompletedTask;
	}
}
