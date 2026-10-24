using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Represents a transformation of one card in a pile to another card which replaces the original card in the same pile.
/// If Replacement is set, then it will be used as a direct replacement.
/// If ReplacementOptions is set, then a random card will be chosen from one of those options.
/// </summary>
public readonly struct CardTransformation
{
	public CardModel Original { get; }

	public CardModel? Replacement { get; }

	public IEnumerable<CardModel>? ReplacementOptions { get; }

	/// <summary>
	/// Whether the original card is in combat at construction time. Captured here because the card
	/// may be removed from its pile before GetReplacement is called, which would make CombatState null.
	/// </summary>
	public bool IsInCombat { get; }

	public CardTransformation(CardModel original)
	{
		AssertTransformable(original);
		Original = original;
		ReplacementOptions = null;
		Replacement = null;
		IsInCombat = original.CombatState != null;
	}

	public CardTransformation(CardModel original, IEnumerable<CardModel> options)
	{
		AssertTransformable(original);
		Original = original;
		ReplacementOptions = options;
		Replacement = null;
		IsInCombat = original.CombatState != null;
	}

	public CardTransformation(CardModel original, CardModel replacement)
	{
		AssertTransformable(original);
		Original = original;
		Replacement = replacement;
		ReplacementOptions = null;
		IsInCombat = original.CombatState != null;
	}

	public CardModel? GetReplacement(Rng? rng)
	{
		if (Replacement != null)
		{
			return Replacement;
		}
		if (rng == null)
		{
			throw new ArgumentException("RNG must be passed when replacement options is set!");
		}
		if (!Original.IsTransformable)
		{
			return null;
		}
		if (ReplacementOptions == null)
		{
			return CardFactory.CreateRandomCardForTransform(Original, IsInCombat, rng);
		}
		return CardFactory.CreateRandomCardForTransform(Original, ReplacementOptions, IsInCombat, rng);
	}

	public IEnumerable<CardTransformation> Yield()
	{
		yield return this;
	}

	private static void AssertTransformable(CardModel card)
	{
		if (!card.IsTransformable)
		{
			throw new InvalidOperationException("Non-removable cards cannot be transformed!");
		}
	}
}
