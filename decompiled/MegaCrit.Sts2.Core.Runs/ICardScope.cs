using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// A scope in which cards exist.
/// </summary>
public interface ICardScope
{
	/// <summary>
	/// Create a card within this scope.
	/// </summary>
	/// <param name="owner">Owner of the new card.</param>
	/// <typeparam name="T">Type of the new card.</typeparam>
	/// <returns>Newly-created card.</returns>
	T CreateCard<T>(Player owner) where T : CardModel;

	/// <summary>
	/// Create a card within this scope, based on a canonical card.
	/// </summary>
	/// <param name="canonicalCard">Canonical version of the new card.</param>
	/// <param name="owner">Owner of the new card.</param>
	/// <returns>Newly-created card.</returns>
	CardModel CreateCard(CardModel canonicalCard, Player owner);

	/// <summary>
	/// WARNING: If you're specifically intending to create a clone in combat for an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.DualWield" />,
	/// you should use <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.CreateClone" /> instead.
	/// Create a clone of a mutable card within this scope.
	/// The card must already have an owner.
	/// </summary>
	CardModel CloneCard(CardModel mutableCard);

	/// <summary>
	/// Add a mutable card to this scope.
	/// The card must not have an owner yet.
	/// </summary>
	/// <param name="mutableCard">Mutable card to add.</param>
	/// <param name="owner">The player who should own this card.</param>
	void AddCard(CardModel mutableCard, Player owner);

	/// <summary>
	/// Remove a card from this scope.
	/// Be careful with this! Outside of tests, cards should really not be moving around between scopes.
	/// </summary>
	void RemoveCard(CardModel card);

	/// <summary>
	/// THIS IS TEMPORARY AND SHOULD ONLY BE USED IN TESTS AND DEV CONSOLE COMMANDS.
	/// </summary>
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
