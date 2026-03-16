using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public sealed class CardEnergyCost
{
	private readonly CardModel _card;

	private int _base;

	private int _capturedXValue;

	private List<LocalCostModifier> _localModifiers = new List<LocalCostModifier>();

	public int Canonical { get; }

	public bool CostsX { get; }

	public bool WasJustUpgraded { get; private set; }

	public bool HasLocalModifiers => _localModifiers.Count > 0;

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

	public int GetAmountToSpend()
	{
		if (CostsX)
		{
			return _card.Owner.PlayerCombatState?.Energy ?? 0;
		}
		return Math.Max(0, GetWithModifiers(CostModifiers.All));
	}

	public int GetResolved()
	{
		if (CostsX)
		{
			return CapturedXValue;
		}
		return Math.Max(0, GetWithModifiers(CostModifiers.All));
	}

	public void SetUntilPlayed(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	public void SetThisTurnOrUntilPlayed(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	public void SetThisTurn(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfTurn, reduceOnly));
		}
	}

	public void SetThisCombat(int cost, bool reduceOnly = false)
	{
		if (cost != 0 || Canonical >= 0)
		{
			_localModifiers.Add(new LocalCostModifier(cost, LocalCostType.Absolute, LocalCostModifierExpiration.EndOfCombat, reduceOnly));
		}
	}

	public void AddUntilPlayed(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	public void AddThisTurnOrUntilPlayed(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfTurn | LocalCostModifierExpiration.WhenPlayed, reduceOnly));
		}
	}

	public void AddThisTurn(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfTurn, reduceOnly));
		}
	}

	public void AddThisCombat(int amount, bool reduceOnly = false)
	{
		if (amount != 0)
		{
			_localModifiers.Add(new LocalCostModifier(amount, LocalCostType.Relative, LocalCostModifierExpiration.EndOfCombat, reduceOnly));
		}
	}

	public bool EndOfTurnCleanup()
	{
		_card.AssertMutable();
		return _localModifiers.RemoveAll((LocalCostModifier m) => m.Expiration.HasFlag(LocalCostModifierExpiration.EndOfTurn)) > 0;
	}

	public bool AfterCardPlayedCleanup()
	{
		_card.AssertMutable();
		return _localModifiers.RemoveAll((LocalCostModifier m) => m.Expiration.HasFlag(LocalCostModifierExpiration.WhenPlayed)) > 0;
	}

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

	public void FinalizeUpgrade()
	{
		_card.AssertMutable();
		WasJustUpgraded = false;
	}

	public void ResetForDowngrade()
	{
		_card.AssertMutable();
		_base = Canonical;
		_card.InvokeEnergyCostChanged();
	}

	public void SetCustomBaseCost(int newBaseCost)
	{
		_card.AssertMutable();
		_base = newBaseCost;
		_card.InvokeEnergyCostChanged();
	}

	public CardEnergyCost Clone(CardModel newCard)
	{
		return new CardEnergyCost(newCard, newCard.EnergyCost.Canonical, newCard.EnergyCost.CostsX)
		{
			_base = _base,
			_capturedXValue = _capturedXValue,
			WasJustUpgraded = WasJustUpgraded,
			_localModifiers = _localModifiers.ToList()
		};
	}
}
