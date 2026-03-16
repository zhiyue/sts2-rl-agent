using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BiiigHug : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Soot>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(4));

	public override async Task AfterObtained()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
	}

	public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
	{
		if (shuffler == base.Owner)
		{
			CardModel soot = shuffler.Creature.CombatState.CreateCard<Soot>(base.Owner);
			await CardPileCmd.AddGeneratedCardToCombat(soot, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
			Flash();
			CardCmd.Preview(soot, 0.75f);
			await Cmd.Wait(1f);
		}
	}
}
