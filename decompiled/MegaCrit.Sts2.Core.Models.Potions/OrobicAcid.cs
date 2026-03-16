using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class OrobicAcid : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		List<CardModel> list = new List<CardModel>();
		list.AddRange(CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Attack
			select c, 1, base.Owner.RunState.Rng.CombatCardGeneration));
		list.AddRange(CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Skill
			select c, 1, base.Owner.RunState.Rng.CombatCardGeneration));
		list.AddRange(CardFactory.GetDistinctForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Power
			select c, 1, base.Owner.RunState.Rng.CombatCardGeneration));
		foreach (CardModel item in list)
		{
			item.SetToFreeThisTurn();
		}
		await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, addedByPlayer: true);
	}
}
