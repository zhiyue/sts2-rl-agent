using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class LiquidMemories : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		NCombatRoom.Instance?.PlaySplashVfx(base.Owner.Creature, new Color(Colors.Blue));
		CardModel cardModel = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			cardModel.SetToFreeThisTurn();
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}
}
