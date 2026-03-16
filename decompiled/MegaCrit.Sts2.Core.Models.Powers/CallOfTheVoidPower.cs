using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CallOfTheVoidPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player != base.Owner.Player)
		{
			return;
		}
		IReadOnlyList<CardModel> readOnlyList = base.Owner.Player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(delegate(CardModel c)
		{
			CardRarity rarity = c.Rarity;
			bool flag = ((rarity == CardRarity.Basic || rarity == CardRarity.Ancient) ? true : false);
			return !flag;
		}).ToList();
		if (readOnlyList.Count > 0)
		{
			CardModel[] array = new CardModel[base.Amount];
			Rng combatCardGeneration = base.Owner.Player.RunState.Rng.CombatCardGeneration;
			for (int num = 0; num < base.Amount; num++)
			{
				CardCmd.ApplyKeyword(array[num] = CardFactory.GetDistinctForCombat(player, readOnlyList, 1, combatCardGeneration).First(), CardKeyword.Ethereal);
			}
			Flash();
			await CardPileCmd.AddGeneratedCardsToCombat(array, PileType.Hand, addedByPlayer: true);
		}
	}
}
