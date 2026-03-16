using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class GlowwaterPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Event;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	public override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(10));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		NCombatRoom.Instance?.PlaySplashVfx(base.Owner.Creature, new Color("83ebdf"));
		List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.ToList();
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
	}
}
