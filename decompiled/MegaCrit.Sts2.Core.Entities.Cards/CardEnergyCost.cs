using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Encapsulates all energy cost related properties and methods for a CardModel.
/// This class handles base energy costs, local and global modifications, and energy cost calculations.
/// </summary>
public sealed class CardEnergyCost
{
	private readonly CardModel _card;

	private int _base;

	private int _capturedXValue;

	/// <summary>
	/// This card's local cost modifiers.
	/// See <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier" /> for details on how this works.
	/// </summary>
	private List<LocalCostModifier> _localModifiers = new List<LocalCostModifier>();

	/// <summary>
	/// This card's "official" starting energy cost.
	/// This is what would appear on the card if it was printed out on paper.
	/// </summary>
	public int Canonical { get; }

	/// <summary>
	/// Whether this card has an energy cost of X.
	/// X-cost-cards automatically spend all of the player's remaining energy when played, and their effect is
	/// multiplied by the amount spent.
	/// </summary>
	public bool CostsX { get; }

	/// <summary>
	/// Was this card's energy cost just recently upgraded?
	/// This is mainly used to show upgrade preview values in green.
	/// This should be cleared after the upgrade is complete.
	/// </summary>
	public bool WasJustUpgraded { get; private set; }

	/// <summary>
	/// Does this energy cost have any local modifiers?
	/// See <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.Local" /> for details.
	/// </summary>
	public bool HasLocalModifiers => _localModifiers.Count > 0;

	/// <summary>
	/// The amount of energy most recently spent to play this X-cost card.
	/// Used when duplicating X-cost cards, to make sure the duplicates are played with the same value.
	///
	/// WARNING: Only use this for calculations related to energy spent. If you're using this to calculate a
	/// cost-X-energy card's effect, use <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.ResolveEnergyXValue" /> instead, as it will take X-value
	/// modifications (like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.ChemicalX" />) into account.
	/// </summary>
	public int CapturedXValue
	{
		get
		{
			if (!CostsX)
			{
				throw new InvalidOperationException("Only X-cost cards have a captured value.");
			}
			return _capturedXValue;
		}
		set
		{
			_card.AssertMutable();
			if (!CostsX)
			{
				throw new InvalidOperationException("Only X-cost cards have a captured value.");
			}
			_capturedXValue = value;
		}
	}

	public CardEnergyCost(CardModel card, int canonicalCost, bool costsX)
	{
		_card = card;
		CostsX = costsX;
		Canonical = ((!CostsX) ? canonicalCost : 0);
		_base = Canonical;
	}

	/// <summary>
	/// Get this card's energy cost, including the specified modifier types.
	/// See <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers" /> for details on what types are available.
	/// </summary>
	public int GetWithModifiers(CostModifiers modifiers)
	{
		int num = _base;
		if (_card.IsCanonical)
		{
			return num;
		}
		if (_base < 0)
		{
			return num;
		}
		if (CostsX)
		{
			return num;
		}
		if (modifiers.HasFlag(CostModifiers.Local))
		{
			foreach (LocalCostModifier localModifier in _localModifiers)
			{
				num = localModifier.Modify(num);
			}
		}
		if (modifiers.HasFlag(CostModifiers.Global) && _card.CombatState != null)
		{
			num = (int)Hook.ModifyEnergyCostInCombat(_card.CombatState, _card, num);
		}
		return Math.Max(0, num);
	}

	/// <summary>
	/// Get the amount of energy that should be spent to play this card.
	///
	/// * For X-cost cards, this is the amount of energy that its owner has.
	/// * For normal cards, this is the current cost including all modifiers
	///   (see <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.GetWithModifiers(MegaCrit.Sts2.Core.Entities.Cards.CostModifiers)" /> with <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.All" />) clamped to 0.
	///
	/// The game uses this value when actually spending the energy to play the card.
	/// Additionally, this is useful for effects that need to know how much WOULD be spent to play the card without
	/// actually playing it, such as <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Scavenge" />.
	/// </summary>
	public int GetAmountToSpend()
	{
		if (CostsX)
		{
			return _card.Owner.PlayerCombatState?.Energy ?? 0;
		}
		return Math.Max(0, GetWithModifiers(CostModifiers.All));
	}

	/// <summary>
	/// Get the "resolved" cost of this card. This can mean one of two things:
	///
	/// * For X-cost cards, this is the captured X-cost value (see <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.CapturedXValue" />).
	/// * For normal cards, this is the current cost including all modifiers
	///   (see <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.GetWithModifiers(MegaCrit.Sts2.Core.Entities.Cards.CostModifiers)" /> with <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.All" />) clamped to 0.
	///
	/// This is useful for effects that need to know the card's cost AFTER it was played, such as
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.IntimidatingHelmet" />. For normal cards, these effects just care about the card's current cost
	/// (including all modifiers). For X-cost cards, these effects care about the X-value that was set for the card when
	/// it was played.
	/// </summary>
	public int GetResolved()
	{
		if (CostsX)
		{
			return CapturedXValue;
		}
		return Math.Max(0, GetWithModifiers(CostModifiers.All));
	}

	/// <summary>
	/// Set this cost to the specified amount until the card is played.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Eidolon" /> says "Reduce the cost of all cards in your Discard Pile to 0 until played."
	/// </example>
	/// <param name="cost">New cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void SetUntilPlayed(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	/// <summary>
	/// Set this cost to the specified amount until the end of the current turn OR until the card is played, whichever
	/// comes first.
	/// Note that the text of these effects will just say "this turn"; the "or until played" part is left implicit
	/// because it's wordy and rarely relevant.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.BulletTime" /> says "Reduce the cost of ALL cards in your Hand to 0 this turn."
	/// </example>
	/// <param name="cost">New cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void SetThisTurnOrUntilPlayed(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	/// <summary>
	/// BE CAREFUL USING THIS! You usually want <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.SetThisTurnOrUntilPlayed(System.Int32,System.Boolean)" /> instead.
	/// Set this cost to the specified amount until the end of the current turn.
	/// Note that most effects that say "this turn" really mean "this turn or until played".
	/// This method should only be used for the few effects that should last for multiple plays in the same turn.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Invoke" /> says "This card costs 0 if Osty has attacked this turn."
	/// </example>
	/// <param name="cost">New cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void SetThisTurn(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfTurn, reduceOnly));
		}
	}

	/// <summary>
	/// Set this cost to the specified amount for the rest of the combat.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enlightenment" />+ says "Reduce the cost of ALL cards in your Hand to 1 this combat."
	/// </example>
	/// <param name="cost">New cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void SetThisCombat(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfCombat, reduceOnly));
		}
	}

	/// <summary>
	/// Add the specified amount to this cost until the card is played.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Enchantments.SlumberingEssence" /> says "If this card is in your hand at the end of turn, reduce its cost by 1
	/// until it is played."
	/// </example>
	/// <param name="amount">Amount to add to the cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void AddUntilPlayed(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	/// <summary>
	/// Add the specified amount to this cost until the end of the current turn OR until the card is played, whichever
	/// comes first.
	/// Note that the text of these effects will just say "this turn"; the "or until played" part is left implicit
	/// because it's wordy and rarely relevant.
	/// </summary>
	/// <example>None yet. Update this if we add one!</example>
	/// <param name="amount">Amount to add to the cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void AddThisTurnOrUntilPlayed(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	/// <summary>
	/// BE CAREFUL USING THIS! You usually want <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.AddThisTurnOrUntilPlayed(System.Int32,System.Boolean)" /> instead.
	/// Add the specified amount to this cost until the end of the current turn.
	/// Note that most effects that say "this turn" really mean "this turn or until played".
	/// This method should only be used for the few effects that should last for multiple plays in the same turn.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Pinpoint" /> says "Costs 1 less for each Skill played this turn."
	/// </example>
	/// <param name="amount">Amount to add to the cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void AddThisTurn(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfTurn, reduceOnly));
		}
	}

	/// <summary>
	/// Add the specified amount to this cost for the rest of the combat.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.KinglyKick" /> says "Whenever you draw this card, lower its cost by 1 this combat."
	/// </example>
	/// <param name="amount">Amount to add to the cost.</param>
	/// <param name="reduceOnly">
	/// Whether this modifier should only be included in the cost calculation if it would lower the current cost.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier.IsReduceOnly" /> for details.
	/// </param>
	public void AddThisCombat(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfCombat, reduceOnly));
		}
	}

	/// <summary>
	/// Clear local cost modifiers that should last until the end of the turn.
	/// </summary>
	/// <returns>True if any modifiers were cleared and EnergyCostChanged should be invoked.</returns>
	public bool EndOfTurnCleanup()
	{
		_card.AssertMutable();
		return _localModifiers.RemoveAll((LocalCostModifier m) => m.Expiration.HasFlag(LocalCostModifierExpiration.EndOfTurn)) > 0;
	}

	/// <summary>
	/// Clear local cost modifiers that should last until the card is played.
	/// </summary>
	/// <returns>True if any modifiers were cleared and EnergyCostChanged should be invoked.</returns>
	public bool AfterCardPlayedCleanup()
	{
		_card.AssertMutable();
		return _localModifiers.RemoveAll((LocalCostModifier m) => m.Expiration.HasFlag(LocalCostModifierExpiration.WhenPlayed)) > 0;
	}

	/// <summary>
	/// Upgrade the energy cost of this card by the specified amount.
	/// </summary>
	/// <param name="addend">Amount to add to the current cost (usually negative).</param>
	public void UpgradeBy(int addend)
	{
		_card.AssertMutable();
		if (CostsX || addend == 0)
		{
			return;
		}
		int num = _base;
		int num2 = Math.Max(_base + addend, 0);
		WasJustUpgraded = true;
		if (num2 < num)
		{
			foreach (LocalCostModifier localModifier in _localModifiers)
			{
				if (localModifier.Type == LocalCostType.Absolute && localModifier.Amount > num2)
				{
					localModifier.Amount = num2;
				}
			}
		}
		SetCustomBaseCost(num2);
	}

	/// <summary>
	/// Finalize an upgrade after calling UpgradeEnergyCostBy. This clears out state that is used for displaying an upgrade
	/// preview.
	/// </summary>
	public void FinalizeUpgrade()
	{
		_card.AssertMutable();
		WasJustUpgraded = false;
	}

	/// <summary>
	/// Reset energy cost to base values during downgrade.
	/// </summary>
	public void ResetForDowngrade()
	{
		_card.AssertMutable();
		_base = Canonical;
		_card.InvokeEnergyCostChanged();
	}

	/// <summary>
	/// BE VERY CAREFUL USING THIS!
	/// This is mainly meant for internal usage. The only external usage of this should be in <see cref="T:MegaCrit.Sts2.Core.Models.Cards.MadScience" />.
	/// </summary>
	/// <param name="newBaseCost"></param>
	public void SetCustomBaseCost(int newBaseCost)
	{
		_card.AssertMutable();
		_base = newBaseCost;
		_card.InvokeEnergyCostChanged();
	}

	/// <summary>
	/// Create a deep clone of this EnergyCostInfo for the specified card.
	/// </summary>
	/// <param name="newCard">The card that will own the cloned EnergyCostInfo.</param>
	/// <returns>A deep clone of this EnergyCostInfo.</returns>
	public CardEnergyCost Clone(CardModel newCard)
	{
		List<LocalCostModifier> localModifiers = _localModifiers.Select((LocalCostModifier m) => m.Clone()).ToList();
		return new CardEnergyCost(newCard, newCard.EnergyCost.Canonical, newCard.EnergyCost.CostsX)
		{
			_base = _base,
			_capturedXValue = _capturedXValue,
			WasJustUpgraded = WasJustUpgraded,
			_localModifiers = localModifiers
		};
	}
}
