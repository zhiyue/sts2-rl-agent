using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class DynamicVarSet : IReadOnlyDictionary<string, DynamicVar>, IEnumerable<KeyValuePair<string, DynamicVar>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, DynamicVar>>
{
	private readonly Dictionary<string, DynamicVar> _vars = new Dictionary<string, DynamicVar>();

	public int Count => _vars.Count;

	public DynamicVar this[string key] => _vars[key];

	public IEnumerable<string> Keys => _vars.Keys;

	public IEnumerable<DynamicVar> Values => _vars.Values;

	public BlockVar Block => (BlockVar)_vars["Block"];

	public CalculatedBlockVar CalculatedBlock => (CalculatedBlockVar)_vars["CalculatedBlock"];

	public CalculatedDamageVar CalculatedDamage => (CalculatedDamageVar)_vars["CalculatedDamage"];

	public CalculationBaseVar CalculationBase => (CalculationBaseVar)_vars["CalculationBase"];

	public CalculationExtraVar CalculationExtra => (CalculationExtraVar)_vars["CalculationExtra"];

	public CardsVar Cards => (CardsVar)_vars["Cards"];

	public DamageVar Damage => (DamageVar)_vars["Damage"];

	public PowerVar<DexterityPower> Dexterity => (PowerVar<DexterityPower>)_vars["DexterityPower"];

	public PowerVar<DoomPower> Doom => (PowerVar<DoomPower>)_vars["DoomPower"];

	public EnergyVar Energy => (EnergyVar)_vars["Energy"];

	public ExtraDamageVar ExtraDamage => (ExtraDamageVar)_vars["ExtraDamage"];

	public ForgeVar Forge => (ForgeVar)_vars["Forge"];

	public GoldVar Gold => (GoldVar)_vars["Gold"];

	public HealVar Heal => (HealVar)_vars["Heal"];

	public HpLossVar HpLoss => (HpLossVar)_vars["HpLoss"];

	public MaxHpVar MaxHp => (MaxHpVar)_vars["MaxHp"];

	public OstyDamageVar OstyDamage => (OstyDamageVar)_vars["OstyDamage"];

	public PowerVar<PoisonPower> Poison => (PowerVar<PoisonPower>)_vars["PoisonPower"];

	public RepeatVar Repeat => (RepeatVar)_vars["Repeat"];

	public StarsVar Stars => (StarsVar)_vars["Stars"];

	public PowerVar<StrengthPower> Strength => (PowerVar<StrengthPower>)_vars["StrengthPower"];

	public SummonVar Summon => (SummonVar)_vars["Summon"];

	public PowerVar<VulnerablePower> Vulnerable => (PowerVar<VulnerablePower>)_vars["VulnerablePower"];

	public PowerVar<WeakPower> Weak => (PowerVar<WeakPower>)_vars["WeakPower"];

	public IEnumerator<KeyValuePair<string, DynamicVar>> GetEnumerator()
	{
		return _vars.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool ContainsKey(string key)
	{
		return _vars.ContainsKey(key);
	}

	public bool TryGetValue(string key, [MaybeNullWhen(false)] out DynamicVar value)
	{
		return _vars.TryGetValue(key, out value);
	}

	public DynamicVarSet(IEnumerable<DynamicVar> vars)
	{
		foreach (DynamicVar var in vars)
		{
			if (_vars.ContainsKey(var.Name))
			{
				throw new ArgumentException(StringHelper.CompactText("DynamicVarSet contains duplicate key '" + var.Name + "'. If you're using two of the same variable\ntype (like 2 BlockVars for Halt), please specify a name for each one instead of using the default."));
			}
			_vars[var.Name] = var;
		}
	}

	public void InitializeWithOwner(AbstractModel model)
	{
		foreach (DynamicVar value in Values)
		{
			value.SetOwner(model);
		}
	}

	public void AddTo(LocString str)
	{
		using IEnumerator<KeyValuePair<string, DynamicVar>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			str.Add(enumerator.Current.Value);
		}
	}

	public void ClearPreview()
	{
		foreach (DynamicVar value in Values)
		{
			value.ResetToBase();
		}
	}

	public void FinalizeUpgrade()
	{
		foreach (DynamicVar value in Values)
		{
			value.FinalizeUpgrade();
		}
	}

	public void RecalculateForUpgradeOrEnchant()
	{
		foreach (CalculatedVar item in Values.OfType<CalculatedVar>())
		{
			item.RecalculateForUpgradeOrEnchant();
		}
	}

	public DynamicVarSet Clone(AbstractModel model)
	{
		DynamicVarSet dynamicVarSet = new DynamicVarSet(Values.Select((DynamicVar v) => v.Clone()));
		dynamicVarSet.InitializeWithOwner(model);
		return dynamicVarSet;
	}
}
