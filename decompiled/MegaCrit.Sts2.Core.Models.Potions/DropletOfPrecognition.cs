using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class DropletOfPrecognition : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, (from c in PileType.Draw.GetPile(base.Owner).Cards
			orderby c.Rarity, c.Id
			select c).ToList(), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}
}
