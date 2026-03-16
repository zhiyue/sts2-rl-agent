using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Runs;

public interface ICardScope
{
	T CreateCard<T>(Player owner) where T : CardModel;

	CardModel CreateCard(CardModel canonicalCard, Player owner);

	CardModel CloneCard(CardModel mutableCard);

	void AddCard(CardModel mutableCard, Player owner);

	void RemoveCard(CardModel card);

	static ICardScope DebugOnlyGet(CardScope scope)
	{
		return scope switch
		{
			CardScope.Run => RunManager.Instance.DebugOnlyGetState(), 
			CardScope.Combat => CombatManager.Instance.DebugOnlyGetState(), 
			_ => throw new ArgumentOutOfRangeException("scope", scope, null), 
		};
	}
}
