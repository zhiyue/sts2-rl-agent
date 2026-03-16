using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NUpgradePreview.cs")]
public class NUpgradePreview : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Reload = "Reload";

		public static readonly StringName RemoveExistingCards = "RemoveExistingCards";

		public static readonly StringName ReturnCard = "ReturnCard";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName SelectedCardPosition = "SelectedCardPosition";

		public static readonly StringName _before = "_before";

		public static readonly StringName _after = "_after";

		public static readonly StringName _arrows = "_arrows";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _before;

	private Control _after;

	private Control _arrows;

	private CardModel? _card;

	public Vector2 SelectedCardPosition => _before.GlobalPosition;

	public CardModel? Card
	{
		get
		{
			return _card;
		}
		set
		{
			_card = value;
			Reload();
		}
	}

	public override void _Ready()
	{
		_before = GetNode<Control>("%Before");
		_after = GetNode<Control>("%After");
		_arrows = GetNode<Control>("Arrows");
	}

	private void Reload()
	{
		RemoveExistingCards();
		_arrows.Visible = Card != null;
		if (Card != null)
		{
			NPlayerHand nPlayerHand = NCombatRoom.Instance?.Ui.Hand;
			NPreviewCardHolder nPreviewCardHolder = NPreviewCardHolder.Create(NCard.Create(Card), nPlayerHand == null, nPlayerHand != null);
			_before.AddChildSafely(nPreviewCardHolder);
			nPreviewCardHolder.FocusMode = FocusModeEnum.All;
			nPreviewCardHolder.CardNode.UpdateVisuals(Card.Pile.Type, CardPreviewMode.Normal);
			if (nPlayerHand != null)
			{
				nPreviewCardHolder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(ReturnCard));
			}
			CardModel cardModel = Card.CardScope.CloneCard(Card);
			cardModel.UpgradeInternal();
			cardModel.UpgradePreviewType = ((!Card.Pile.IsCombatPile) ? CardUpgradePreviewType.Deck : CardUpgradePreviewType.Combat);
			NPreviewCardHolder nPreviewCardHolder2 = NPreviewCardHolder.Create(NCard.Create(cardModel), showHoverTips: true, scaleOnHover: false);
			nPreviewCardHolder2.FocusMode = FocusModeEnum.None;
			_after.AddChildSafely(nPreviewCardHolder2);
			nPreviewCardHolder2.CardNode.ShowUpgradePreview();
		}
	}

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

	private void ReturnCard(NCardHolder holder)
	{
		holder.Pressed -= ReturnCard;
		NCombatRoom.Instance?.Ui.Hand.DeselectCard(holder.CardNode);
		Card = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveExistingCards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReturnCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveExistingCards && args.Count == 0)
		{
			RemoveExistingCards();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReturnCard && args.Count == 1)
		{
			ReturnCard(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.RemoveExistingCards)
		{
			return true;
		}
		if (method == MethodName.ReturnCard)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SelectedCardPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(SelectedCardPosition);
			return true;
		}
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._before, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._after, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrows, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.SelectedCardPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._before, Variant.From(in _before));
		info.AddProperty(PropertyName._after, Variant.From(in _after));
		info.AddProperty(PropertyName._arrows, Variant.From(in _arrows));
	}

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
