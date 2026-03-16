using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class BlessingOfTheForge : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		NCombatRoom.Instance?.PlaySplashVfx(base.Owner.Creature, new Color("e06e58"));
		foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards)
		{
			if (card.IsUpgradable)
			{
				CardCmd.Upgrade(card);
			}
		}
		return Task.CompletedTask;
	}
}
