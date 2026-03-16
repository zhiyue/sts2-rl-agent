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

public sealed class BlessedAntler : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new EnergyVar(1),
		new CardsVar(3)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.ForEnergy(this),
		HoverTipFactory.FromCard<Dazed>()
	});

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		return amount + (decimal)base.DynamicVars.Energy.IntValue;
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			Flash();
			List<CardModel> list = new List<CardModel>();
			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				list.Add(combatState.CreateCard<Dazed>(base.Owner));
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
			await Cmd.Wait(3f);
		}
	}
}
