using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Metamorphosis : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	public Metamorphosis()
		: base(2, CardType.Skill, CardRarity.Event, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.Type == CardType.Attack
			select c, base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration);
		foreach (CardModel item in forCombat)
		{
			item.SetToFreeThisCombat();
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(2m);
	}
}
