using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantRelic.cs")]
public class NMerchantRelic : NMerchantSlot
{
	public new class MethodName : NMerchantSlot.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName UpdateVisual = "UpdateVisual";

		public new static readonly StringName CreateHoverTip = "CreateHoverTip";

		public new static readonly StringName OnPreview = "OnPreview";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NMerchantSlot.PropertyName
	{
		public new static readonly StringName Visual = "Visual";

		public static readonly StringName _iconSize = "_iconSize";

		public static readonly StringName _relicHolder = "_relicHolder";

		public static readonly StringName _relicNode = "_relicNode";

		public static readonly StringName _relicNodePosition = "_relicNodePosition";
	}

	public new class SignalName : NMerchantSlot.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private NRelic.IconSize _iconSize;

	private Control _relicHolder;

	private NRelic? _relicNode;

	private MerchantRelicEntry _relicEntry;

	private RelicModel? _relic;

	private Vector2 _relicNodePosition;

	public override MerchantEntry Entry => _relicEntry;

	protected override CanvasItem Visual => _relicHolder;

	public override void _Ready()
	{
		ConnectSignals();
		_relicHolder = GetNode<Control>("%RelicHolder");
	}

	public void FillSlot(MerchantRelicEntry relicEntry)
	{
		_relicEntry = relicEntry;
		_relic = relicEntry.Model;
		relicEntry.EntryUpdated += UpdateVisual;
		relicEntry.PurchaseFailed += base.OnPurchaseFailed;
		relicEntry.PurchaseCompleted += OnSuccessfulPurchase;
		UpdateVisual();
	}

	protected override void UpdateVisual()
	{
		base.UpdateVisual();
		if (_relicEntry.Model == null)
		{
			base.Visible = false;
			base.MouseFilter = MouseFilterEnum.Ignore;
			if (_relicNode != null)
			{
				_relicNode.QueueFreeSafely();
				_relicNode = null;
			}
			ClearHoverTip();
			return;
		}
		if (_relicNode != null && _relicNode.Model != _relicEntry.Model)
		{
			_relicNode.QueueFreeSafely();
			_relicNode = null;
		}
		if (_relicNode == null)
		{
			_relicNode = NRelic.Create(_relicEntry.Model, _iconSize);
			_relicHolder.AddChildSafely(_relicNode);
			if (_iconSize == NRelic.IconSize.Large)
			{
				_relicNode.Size = new Vector2(128f, 128f);
				_relicNode.Icon.Position -= new Vector2(0f, _costLabel.Size.Y);
			}
			base.Hitbox.Size = _relicNode.Icon.Size;
			base.Hitbox.Scale = _relicHolder.Scale;
			base.Hitbox.GlobalPosition = _relicNode.Icon.GlobalPosition;
		}
		_relicNodePosition = _relicNode.Icon.GlobalPosition;
		_costLabel.SetTextAutoSize(_relicEntry.Cost.ToString());
		_costLabel.Modulate = (_relicEntry.EnoughGold ? StsColors.cream : StsColors.red);
	}

	protected override async Task OnTryPurchase(MerchantInventory? inventory)
	{
		await _relicEntry.OnTryPurchaseWrapper(inventory);
	}

	private void OnSuccessfulPurchase(PurchaseStatus _, MerchantEntry __)
	{
		TriggerMerchantHandToPointHere();
		NRun.Instance?.GlobalUi.RelicInventory.AnimateRelic(_relic, _relicNodePosition);
		UpdateVisual();
		_relic = _relicEntry.Model;
	}

	protected override void CreateHoverTip()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _relicNode.Model.HoverTips);
		nHoverTipSet.GlobalPosition = base.GlobalPosition;
		if (base.GlobalPosition.X > GetViewport().GetVisibleRect().Size.X * 0.5f)
		{
			nHoverTipSet.SetAlignment(this, HoverTipAlignment.Left);
			nHoverTipSet.GlobalPosition -= base.Size * 0.5f * base.Scale;
		}
		else
		{
			nHoverTipSet.SetAlignment(this, HoverTipAlignment.Right);
			nHoverTipSet.GlobalPosition += Vector2.Right * base.Size.X * 0.5f * base.Scale + Vector2.Up * base.Size.Y * 0.5f * base.Scale;
		}
	}

	protected override void OnPreview()
	{
		ClearHoverTip();
		int num = 1;
		List<RelicModel> list = new List<RelicModel>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = _relicNode.Model;
		List<RelicModel> relics = list;
		NGame.Instance.GetInspectRelicScreen().Open(relics, _relicNode.Model);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_relicEntry.EntryUpdated -= UpdateVisual;
		_relicEntry.PurchaseFailed -= base.OnPurchaseFailed;
		_relicEntry.PurchaseCompleted -= OnSuccessfulPurchase;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.UpdateVisual && args.Count == 0)
		{
			UpdateVisual();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateHoverTip && args.Count == 0)
		{
			CreateHoverTip();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPreview && args.Count == 0)
		{
			OnPreview();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.UpdateVisual)
		{
			return true;
		}
		if (method == MethodName.CreateHoverTip)
		{
			return true;
		}
		if (method == MethodName.OnPreview)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._iconSize)
		{
			_iconSize = VariantUtils.ConvertTo<NRelic.IconSize>(in value);
			return true;
		}
		if (name == PropertyName._relicHolder)
		{
			_relicHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._relicNode)
		{
			_relicNode = VariantUtils.ConvertTo<NRelic>(in value);
			return true;
		}
		if (name == PropertyName._relicNodePosition)
		{
			_relicNodePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Visual)
		{
			value = VariantUtils.CreateFrom<CanvasItem>(Visual);
			return true;
		}
		if (name == PropertyName._iconSize)
		{
			value = VariantUtils.CreateFrom(in _iconSize);
			return true;
		}
		if (name == PropertyName._relicHolder)
		{
			value = VariantUtils.CreateFrom(in _relicHolder);
			return true;
		}
		if (name == PropertyName._relicNode)
		{
			value = VariantUtils.CreateFrom(in _relicNode);
			return true;
		}
		if (name == PropertyName._relicNodePosition)
		{
			value = VariantUtils.CreateFrom(in _relicNodePosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._iconSize, PropertyHint.Enum, "Small,Large", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._relicNodePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._iconSize, Variant.From(in _iconSize));
		info.AddProperty(PropertyName._relicHolder, Variant.From(in _relicHolder));
		info.AddProperty(PropertyName._relicNode, Variant.From(in _relicNode));
		info.AddProperty(PropertyName._relicNodePosition, Variant.From(in _relicNodePosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._iconSize, out var value))
		{
			_iconSize = value.As<NRelic.IconSize>();
		}
		if (info.TryGetProperty(PropertyName._relicHolder, out var value2))
		{
			_relicHolder = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._relicNode, out var value3))
		{
			_relicNode = value3.As<NRelic>();
		}
		if (info.TryGetProperty(PropertyName._relicNodePosition, out var value4))
		{
			_relicNodePosition = value4.As<Vector2>();
		}
	}
}
