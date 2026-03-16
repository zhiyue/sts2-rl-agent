using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ChoicesParadox : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(5));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Retain));

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner || base.Owner.Creature.CombatState.RoundNumber != 1)
		{
			return;
		}
		Flash();
		List<CardModel> list = CardFactory.GetDistinctForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint), base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
		foreach (CardModel item in list)
		{
			CardCmd.ApplyKeyword(item, CardKeyword.Retain);
		}
		foreach (CardModel item2 in await CardSelectCmd.FromSimpleGrid(choiceContext, list, base.Owner, new CardSelectorPrefs(RelicModel.L10NLookup("CHOICES_PARADOX.selectionScreenPrompt"), 1)))
		{
			await CardPileCmd.AddGeneratedCardToCombat(item2, PileType.Hand, addedByPlayer: true);
		}
	}
}
