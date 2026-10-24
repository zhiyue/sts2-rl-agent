using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RadiantPearl : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(1));

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.ForEnergy(this));
			list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Luminesce>());
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber == 1)
		{
			List<CardModel> list = new List<CardModel>();
			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				list.Add(base.Owner.Creature.CombatState.CreateCard<Luminesce>(base.Owner));
			}
			await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, base.Owner);
		}
	}
}
