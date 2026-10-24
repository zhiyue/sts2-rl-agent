using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Stoke : CardModel
{
	public Stoke()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.ToList();
		int exhaustCount = list.Count;
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		List<CardModel> cards = CardFactory.GetForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint), exhaustCount, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
		if (base.IsUpgraded)
		{
			CardCmd.Upgrade(cards, CardPreviewStyle.None);
		}
		await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, base.Owner);
	}
}
