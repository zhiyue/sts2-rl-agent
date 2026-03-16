using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Discovery : CardModel
{
	private CardModel? _mockSelectedCard;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Discovery()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel cardModel;
		if (_mockSelectedCard == null)
		{
			List<CardModel> cards = CardFactory.GetDistinctForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint), 3, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
			cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner, canSkip: true);
		}
		else
		{
			cardModel = _mockSelectedCard;
		}
		if (cardModel != null)
		{
			cardModel.EnergyCost.SetThisTurnOrUntilPlayed(0);
			await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}

	public void MockSelectedCard(CardModel card)
	{
		AssertMutable();
		_mockSelectedCard = card;
	}
}
