using System.Collections.Generic;
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

public sealed class GamblersBrew : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		NCombatRoom.Instance?.PlaySplashVfx(base.Owner.Creature, new Color("a19f91"));
		List<CardModel> list = (await CardSelectCmd.FromHandForDiscard(choiceContext, base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 999999999), null, this)).ToList();
		await CardCmd.DiscardAndDraw(choiceContext, list, list.Count);
	}
}
