using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DualWield : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(1));

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<IroncladCardPool>();

	public DualWield()
		: base(1, CardType.Skill, CardRarity.Event, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel selection = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, player: base.Owner, filter: delegate(CardModel c)
		{
			CardType type = c.Type;
			return (type == CardType.Attack || type == CardType.Power) ? true : false;
		}, source: this)).FirstOrDefault();
		if (selection != null)
		{
			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				CardModel card = selection.CreateClone();
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
