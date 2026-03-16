using System;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class BoolVar : DynamicVar
{
	public bool BoolVal
	{
		get
		{
			return Convert.ToBoolean(base.BaseValue);
		}
		set
		{
			base.BaseValue = Convert.ToDecimal(value);
		}
	}

	public BoolVar(string name)
		: base(name, 0m)
	{
	}

	public BoolVar(string name, bool value)
		: base(name, Convert.ToDecimal(value))
	{
	}
}
