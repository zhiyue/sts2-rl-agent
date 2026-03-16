using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Normality : CardModel
{
	private const int _numOfCardsPerTurn = 3;

	private const string _calculatedCardsKey = "CalculatedCards";

	protected override bool ShouldGlowRedInternal => ShouldPreventCardPlay;

	private bool ShouldPreventCardPlay => CardsPlayedThisTurn >= 3;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(3m),
		new CalculationExtraVar(-1m),
		new CalculatedVar("CalculatedCards").WithMultiplier((CardModel card, Creature? _) => Math.Min(3, ((Normality)card).CardsPlayedThisTurn))
	});

	private int CardsPlayedThisTurn => CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Owner == base.Owner);

	public Normality()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType _)
	{
		if (card.Owner != base.Owner)
		{
			return true;
		}
		CardPile? pile = base.Pile;
		if (pile == null || pile.Type != PileType.Hand)
		{
			return true;
		}
		return !ShouldPreventCardPlay;
	}
}
