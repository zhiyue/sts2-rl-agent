using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

/// <summary>
/// Shows a card's before and after for when they get enchanted.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Cards/NEnchantPreview.cs")]
public class NEnchantPreview : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'RemoveExistingCards' method.
		/// </summary>
		public static readonly StringName RemoveExistingCards = "RemoveExistingCards";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_before' field.
		/// </summary>
		public static readonly StringName _before = "_before";

		/// <summary>
		/// Cached name for the '_after' field.
		/// </summary>
		public static readonly StringName _after = "_after";

		/// <summary>
		/// Cached name for the '_arrows' field.
		/// </summary>
		public static readonly StringName _arrows = "_arrows";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Control _before;

	private Control _after;

	private Control _arrows;

	public override void _Ready()
	{
		_before = GetNode<Control>("%Before");
		_after = GetNode<Control>("%After");
		_arrows = GetNode<Control>("Arrows");
	}

	public void Init(CardModel card, EnchantmentModel canonicalEnchantment, int amount)
	{
		canonicalEnchantment.AssertCanonical();
		RemoveExistingCards();
		NPreviewCardHolder nPreviewCardHolder = NPreviewCardHolder.Create(NCard.Create(card), showHoverTips: true, scaleOnHover: false);
		_before.AddChildSafely(nPreviewCardHolder);
		nPreviewCardHolder.CardNode.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
		CardModel cardModel = card.CardScope.CloneCard(card);
		EnchantmentModel enchantmentModel = canonicalEnchantment.ToMutable();
		cardModel.EnchantInternal(enchantmentModel, amount);
		cardModel.IsEnchantmentPreview = true;
		enchantmentModel.ModifyCard();
		NPreviewCardHolder nPreviewCardHolder2 = NPreviewCardHolder.Create(NCard.Create(cardModel), showHoverTips: true, scaleOnHover: false);
		_after.AddChildSafely(nPreviewCardHolder2);
		nPreviewCardHolder2.CardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
	}

	/// <summary>
	/// Helper function to remove any existing preview cards
	/// </summary>
	private void RemoveExistingCards()
	{
		foreach (Node child in _before.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (Node child2 in _after.GetChildren())
		{
			child2.QueueFreeSafely();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveExistingCards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveExistingCards && args.Count == 0)
		{
			RemoveExistingCards();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.RemoveExistingCards)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._before)
		{
			_before = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._after)
		{
			_after = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._arrows)
		{
			_arrows = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._before)
		{
			value = VariantUtils.CreateFrom(in _before);
			return true;
		}
		if (name == PropertyName._after)
		{
			value = VariantUtils.CreateFrom(in _after);
			return true;
		}
		if (name == PropertyName._arrows)
		{
			value = VariantUtils.CreateFrom(in _arrows);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._before, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._after, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrows, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._before, Variant.From(in _before));
		info.AddProperty(PropertyName._after, Variant.From(in _after));
		info.AddProperty(PropertyName._arrows, Variant.From(in _arrows));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._before, out var value))
		{
			_before = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._after, out var value2))
		{
			_after = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._arrows, out var value3))
		{
			_arrows = value3.As<Control>();
		}
	}
}
