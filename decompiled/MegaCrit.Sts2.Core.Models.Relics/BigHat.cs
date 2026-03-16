using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BigHat : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			IReadOnlyList<CardModel> readOnlyList = (from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
				where c.Keywords.Contains(CardKeyword.Ethereal)
				select c).ToList();
			if (readOnlyList.Count > 0)
			{
				Flash();
				List<CardModel> cards = CardFactory.GetDistinctForCombat(base.Owner, readOnlyList, base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
				Flash();
				await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true);
			}
		}
	}
}
