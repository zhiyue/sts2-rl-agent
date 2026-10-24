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
	/// <summary>
	/// The model that "owns" this DynamicVar.
	/// </summary>
	protected AbstractModel? _owner;

	private decimal _baseValue;

	private decimal _enchantedValue;

	private decimal _previewValue;

	public string Name { get; }

	/// <summary>
	/// The base value of this DynamicVar.
	/// This represents the actual value of the variable before any updates from external sources.
	/// This is the value that should be used when performing calculations that will modify the game's state.
	/// Other values (like PreviewValue) are for display only, to inform the player about what the value will be after
	/// external modifications.
	/// </summary>
	public decimal BaseValue
	{
		get
		{
			return _baseValue;
		}
		set
		{
			_baseValue = Math.Min(value, 999999999m);
			ResetToBase();
		}
	}

	/// <summary>
	/// The value of this DynamicVar after being modified by a <see cref="T:MegaCrit.Sts2.Core.Models.EnchantmentModel" />, but before any other
	/// hooks have been run.
	/// The vast majority of the time, this will be the same as BaseValue.
	/// Only CardModels with enchantments will have a different EnchantedValue than BaseValue (and even then, only for
	/// enchantments that modify the variable in question).
	/// This exists because, in the eyes of the player, a card's enchantment is a "part of the card", so we need to be
	/// able to display the enchanted value without coloring it like it's being buffed by an effect.
	///
	/// NOTE: If we ever add another effect that needs to be displayed as "part of the card", we should rename this to
	/// be more generic.
	/// </summary>
	public decimal EnchantedValue
	{
		get
		{
			return _enchantedValue;
		}
		set
		{
			_enchantedValue = Math.Min(value, 999999999m);
		}
	}

	/// <summary>
	/// The value that should be displayed to the player when viewing something that uses this DynamicVar.
	/// This will often be the same as BaseValue, but it is modified in places like
	/// <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.UpdateDynamicVarPreview(MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet)" /> to show the player the final value after all modifications.
	/// Do NOT use this value for performing calculations that will modify the game's state. It's for display only.
	/// </summary>
	public decimal PreviewValue
	{
		get
		{
			return _previewValue;
		}
		set
		{
			_previewValue = Math.Min(value, 999999999m);
		}
	}

	/// <summary>
	/// Was this DynamicVar just recently upgraded?
	/// This is mainly used to show upgrade preview values in green.
	/// This should be cleared after the upgrade is complete.
	/// </summary>
	public bool WasJustUpgraded { get; protected set; }

	public int IntValue => (int)BaseValue;

	public DynamicVar(string name, decimal baseValue)
	{
		Name = name;
		BaseValue = baseValue;
		ResetToBase();
	}

	/// <summary>
	/// Reset all values to the base value.
	/// </summary>
	public void ResetToBase()
	{
		EnchantedValue = BaseValue;
		PreviewValue = BaseValue;
	}

	/// <summary>
	/// Set the model that "owns" this DynamicVar.
	/// </summary>
	public virtual void SetOwner(AbstractModel owner)
	{
		_owner = owner;
	}

	/// <summary>
	/// Update this var's preview value based on hooks (i.e. damage, block, and powers).
	/// This is so powers and relic modifications to these values can be reflected in card descriptions.
	/// This should only be called on DynamicVars owned by CardModels.
	/// </summary>
	/// <param name="card">
	/// Card that this var is being displayed on. Usually matches <see cref="F:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar._owner" />, but for DynamicVars on
	/// enchantments, this is the card that the enchantment is on.
	/// </param>
	/// <param name="previewMode">The mode that this preview is being shown in.</param>
	/// <param name="target">Creature who this card is targeting. Null if there is no target</param>
	/// <param name="runGlobalHooks">
	/// Whether the global hooks (defined in <see cref="T:MegaCrit.Sts2.Core.Hooks.Hook" />) should be run.
	/// If false, only <see cref="T:MegaCrit.Sts2.Core.Models.EnchantmentModel" /> hooks will be run.
	/// </param>
	public virtual void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
	}

	/// <summary>
	/// Increase the base value by the specified amount, and mark it as recently upgraded.
	/// </summary>
	public void UpgradeValueBy(decimal addend)
	{
		BaseValue += addend;
		WasJustUpgraded = true;
	}

	/// <summary>
	/// "Finalize" a recent upgrade, so it is no longer considered recently upgraded.
	/// </summary>
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

	/// <summary>
	/// Get a highlighted version of this variable's preview value.
	/// Normally, if the preview value is higher than the base value, color it green. If lower, red. If equal, default.
	/// Sometimes, we do the inverse (green for lower, red for higher), for cases where a lower value is good.
	/// </summary>
	/// <param name="inverse">Whether or not to flip the highlight logic.</param>
	/// <returns>Highlighted preview value.</returns>
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
