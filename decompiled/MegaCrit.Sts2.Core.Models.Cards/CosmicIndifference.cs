using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class CosmicIndifference : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(6m, ValueProp.Move));

	public CosmicIndifference()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
		PileType.Discard.GetPile(base.Owner);
		CardModel cardModel = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(base.Owner), base.Owner, prefs)).FirstOrDefault();
		bool flag = cardModel != null;
		bool flag2 = flag;
		if (flag2)
		{
			bool flag3;
			switch (cardModel.Pile?.Type)
			{
			case PileType.Draw:
			case PileType.Discard:
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = flag3;
		}
		if (flag2)
		{
			await CardPileCmd.Add(cardModel, PileType.Draw, CardPilePosition.Top);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
