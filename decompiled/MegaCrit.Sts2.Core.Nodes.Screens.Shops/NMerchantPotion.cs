using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantPotion.cs")]
public class NMerchantPotion : NMerchantSlot
{
	public new class MethodName : NMerchantSlot.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName UpdateVisual = "UpdateVisual";

		public new static readonly StringName CreateHoverTip = "CreateHoverTip";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NMerchantSlot.PropertyName
	{
		public new static readonly StringName Visual = "Visual";

		public static readonly StringName _potionHolder = "_potionHolder";

		public static readonly StringName _potionNode = "_potionNode";

		public static readonly StringName _potionNodePosition = "_potionNodePosition";
	}

	public new class SignalName : NMerchantSlot.SignalName
	{
	}

	private Control _potionHolder;

	private NPotion? _potionNode;

	private MerchantPotionEntry _potionEntry;

	private PotionModel? _potion;

	private Vector2 _potionNodePosition;

	public override MerchantEntry Entry => _potionEntry;

	protected override CanvasItem Visual => _potionHolder;

	public override void _Ready()
	{
		ConnectSignals();
		_potionHolder = GetNode<Control>("%PotionHolder");
	}

	public void FillSlot(MerchantPotionEntry potionEntry)
	{
		_potionEntry = potionEntry;
		_potion = potionEntry.Model;
		_potionEntry.EntryUpdated += UpdateVisual;
		_potionEntry.PurchaseFailed += base.OnPurchaseFailed;
		_potionEntry.PurchaseCompleted += OnSuccessfulPurchase;
		UpdateVisual();
	}

	protected override void UpdateVisual()
	{
		base.UpdateVisual();
		if (_potionEntry.Model == null)
		{
			base.Visible = false;
			base.MouseFilter = MouseFilterEnum.Ignore;
			if (_potionNode != null)
			{
				_potionNode.QueueFreeSafely();
				_potionNode = null;
			}
			ClearHoverTip();
			return;
		}
		if (_potionNode != null && _potionNode.Model != _potionEntry.Model)
		{
			_potionNode.QueueFreeSafely();
			_potionNode = null;
		}
		if (_potionNode == null)
		{
			_potionNode = NPotion.Create(_potionEntry.Model);
			_potionHolder.AddChildSafely(_potionNode);
			_potionNode.Position = Vector2.Zero;
		}
		_potionNodePosition = _potionNode.GlobalPosition;
		_costLabel.SetTextAutoSize(_potionEntry.Cost.ToString());
		_costLabel.Modulate = (_potionEntry.EnoughGold ? StsColors.cream : StsColors.red);
	}

	protected override async Task OnTryPurchase(MerchantInventory? inventory)
	{
		await _potionEntry.OnTryPurchaseWrapper(inventory);
	}

	protected void OnSuccessfulPurchase(PurchaseStatus _, MerchantEntry __)
	{
		TriggerMerchantHandToPointHere();
		NRun.Instance?.GlobalUi.TopBar.PotionContainer.AnimatePotion(_potion, _potionNodePosition);
		UpdateVisual();
		_potion = _potionEntry.Model;
	}

	protected override void CreateHoverTip()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _potionNode.Model.HoverTips);
		nHoverTipSet.GlobalPosition = base.GlobalPosition;
		if (base.GlobalPosition.X > GetViewport().GetVisibleRect().Size.X * 0.5f)
		{
			nHoverTipSet.SetAlignment(this, HoverTipAlignment.Left);
			nHoverTipSet.GlobalPosition -= base.Size * 0.5f * base.Scale;
		}
		else
		{
			nHoverTipSet.GlobalPosition += Vector2.Right * base.Size.X * 0.5f * base.Scale + Vector2.Up * base.Size.Y * 0.5f * base.Scale;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_potionEntry.EntryUpdated -= UpdateVisual;
		_potionEntry.PurchaseFailed -= base.OnPurchaseFailed;
		_potionEntry.PurchaseCompleted -= OnSuccessfulPurchase;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._potionHolder)
		{
			_potionHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._potionNode)
		{
			_potionNode = VariantUtils.ConvertTo<NPotion>(in value);
			return true;
		}
		if (name == PropertyName._potionNodePosition)
		{
			_potionNodePosition = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._potionHolder)
		{
			value = VariantUtils.CreateFrom(in _potionHolder);
			return true;
		}
		if (name == PropertyName._potionNode)
		{
			value = VariantUtils.CreateFrom(in _potionNode);
			return true;
		}
		if (name == PropertyName._potionNodePosition)
		{
			value = VariantUtils.CreateFrom(in _potionNodePosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._potionNodePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._potionHolder, Variant.From(in _potionHolder));
		info.AddProperty(PropertyName._potionNode, Variant.From(in _potionNode));
		info.AddProperty(PropertyName._potionNodePosition, Variant.From(in _potionNodePosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._potionHolder, out var value))
		{
			_potionHolder = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._potionNode, out var value2))
		{
			_potionNode = value2.As<NPotion>();
		}
		if (info.TryGetProperty(PropertyName._potionNodePosition, out var value3))
		{
			_potionNodePosition = value3.As<Vector2>();
		}
	}
}
