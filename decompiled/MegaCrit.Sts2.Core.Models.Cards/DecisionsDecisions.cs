using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DecisionsDecisions : CardModel
{
	public override int CanonicalStarCost => 6;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(3),
		new RepeatVar(3)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public DecisionsDecisions()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner);
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
		cardSelectorPrefs.PretendCardsCanBePlayed = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		CardModel card = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, (CardModel c) => c.Type == CardType.Skill && !c.Keywords.Contains(CardKeyword.Unplayable), this)).FirstOrDefault();
		if (card != null)
		{
			for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
			{
				await CardCmd.AutoPlay(choiceContext, card, null);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(2m);
	}
}
