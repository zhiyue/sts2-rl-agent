using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Largesse : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public Largesse()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		CardModel cardModel = CardFactory.GetDistinctForCombat(cardPlay.Target.Player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(cardPlay.Target.Player.UnlockState, cardPlay.Target.Player.RunState.CardMultiplayerConstraint), 1, base.Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
		if (cardModel != null)
		{
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(cardModel);
			}
			await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
		}
	}
}
