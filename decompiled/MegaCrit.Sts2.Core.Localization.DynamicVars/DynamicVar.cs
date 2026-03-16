using System;
using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TextEffects;
using SmartFormat.Core.Extensions;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class DynamicVar : IConvertible
{
	protected AbstractModel? _owner;

	private decimal _baseValue;

	public string Name { get; }

	public decimal BaseValue
	{
		get
		{
			return _baseValue;
		}
		set
		{
			_baseValue = value;
			ResetToBase();
		}
	}

	public decimal EnchantedValue { get; protected set; }

	public decimal PreviewValue { get; set; }

	public bool WasJustUpgraded { get; protected set; }

	public int IntValue => (int)BaseValue;

	public DynamicVar(string name, decimal baseValue)
	{
		Name = name;
		BaseValue = baseValue;
		ResetToBase();
	}

	public void ResetToBase()
	{
		EnchantedValue = BaseValue;
		PreviewValue = BaseValue;
	}

	public virtual void SetOwner(AbstractModel owner)
	{
		_owner = owner;
	}

	public virtual void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
	}

	public void UpgradeValueBy(decimal addend)
	{
		BaseValue += addend;
		WasJustUpgraded = true;
	}

	public void FinalizeUpgrade()
	{
		WasJustUpgraded = false;
	}

	public DynamicVar Clone()
	{
		DynamicVar dynamicVar = (DynamicVar)MemberwiseClone();
		dynamicVar.ResetToBase();
		return dynamicVar;
	}

	public string ToHighlightedString(bool inverse)
	{
		int value = (int)PreviewValue;
		int value2 = (int)EnchantedValue;
		return StsTextUtilities.HighlightChangeText(baseComparison: WasJustUpgraded ? 1 : ((!inverse) ? value.CompareTo(value2) : value2.CompareTo(value)), text: value.ToString(CultureInfo.InvariantCulture));
	}

	public override string ToString()
	{
		return IntValue.ToString();
	}

	public object GetSourceValue(ISelectorInfo selector)
	{
		return GetBaseValueForIConvertible();
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Object;
	}

	public bool ToBoolean(IFormatProvider? provider)
	{
		throw new InvalidCastException($"Cannot convert {BaseValue} to Boolean");
	}

	public byte ToByte(IFormatProvider? provider)
	{
		return Convert.ToByte(GetBaseValueForIConvertible(), provider);
	}

	public char ToChar(IFormatProvider? provider)
	{
		throw new InvalidCastException($"Cannot convert {BaseValue} to Char");
	}

	public DateTime ToDateTime(IFormatProvider? provider)
	{
		throw new InvalidCastException($"Cannot convert {BaseValue} to DateTime");
	}

	public decimal ToDecimal(IFormatProvider? provider)
	{
		return GetBaseValueForIConvertible();
	}

	public double ToDouble(IFormatProvider? provider)
	{
		return Convert.ToDouble(GetBaseValueForIConvertible(), provider);
	}

	public short ToInt16(IFormatProvider? provider)
	{
		return Convert.ToInt16(GetBaseValueForIConvertible(), provider);
	}

	public int ToInt32(IFormatProvider? provider)
	{
		return Convert.ToInt32(GetBaseValueForIConvertible(), provider);
	}

	public long ToInt64(IFormatProvider? provider)
	{
		return Convert.ToInt64(GetBaseValueForIConvertible(), provider);
	}

	public sbyte ToSByte(IFormatProvider? provider)
	{
		return Convert.ToSByte(GetBaseValueForIConvertible(), provider);
	}

	public float ToSingle(IFormatProvider? provider)
	{
		return Convert.ToSingle(GetBaseValueForIConvertible(), provider);
	}

	public string ToString(IFormatProvider? provider)
	{
		return GetBaseValueForIConvertible().ToString(provider);
	}

	public object ToType(Type conversionType, IFormatProvider? provider)
	{
		return Convert.ChangeType(GetBaseValueForIConvertible(), conversionType, provider);
	}

	public ushort ToUInt16(IFormatProvider? provider)
	{
		return Convert.ToUInt16(GetBaseValueForIConvertible(), provider);
	}

	public uint ToUInt32(IFormatProvider? provider)
	{
		return Convert.ToUInt32(GetBaseValueForIConvertible(), provider);
	}

	public ulong ToUInt64(IFormatProvider? provider)
	{
		return Convert.ToUInt64(GetBaseValueForIConvertible(), provider);
	}

	protected virtual decimal GetBaseValueForIConvertible()
	{
		return BaseValue;
	}
}
