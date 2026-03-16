using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantCardRemoval.cs")]
public class NMerchantCardRemoval : NMerchantSlot
{
	public new class MethodName : NMerchantSlot.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName UpdateVisual = "UpdateVisual";

		public static readonly StringName OnCardRemovalUsed = "OnCardRemovalUsed";

		public new static readonly StringName CreateHoverTip = "CreateHoverTip";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NMerchantSlot.PropertyName
	{
		public new static readonly StringName Visual = "Visual";

		public static readonly StringName _removalVisual = "_removalVisual";

		public static readonly StringName _animator = "_animator";

		public static readonly StringName _costContainer = "_costContainer";

		public static readonly StringName _isUnavailable = "_isUnavailable";
	}

	public new class SignalName : NMerchantSlot.SignalName
	{
	}

	private const string _locTable = "merchant_room";

	private Sprite2D _removalVisual;

	private AnimationPlayer _animator;

	private Control _costContainer;

	private bool _isUnavailable;

	private MerchantCardRemovalEntry _removalEntry;

	private LocString Title => new LocString("merchant_room", "MERCHANT.cardRemovalService.title");

	private LocString Description => new LocString("merchant_room", "MERCHANT.cardRemovalService.description");

	public override MerchantEntry Entry => _removalEntry;

	protected override CanvasItem Visual => _removalVisual;

	public override void _Ready()
	{
		ConnectSignals();
		_removalVisual = GetNode<Sprite2D>("%Visual");
		_animator = GetNode<AnimationPlayer>("%Animation");
		_costContainer = GetNode<Control>("Cost");
	}

	public void FillSlot(MerchantCardRemovalEntry removalEntry)
	{
		_removalEntry = removalEntry;
		_removalEntry.EntryUpdated += UpdateVisual;
		_removalEntry.PurchaseFailed += base.OnPurchaseFailed;
		_removalEntry.PurchaseCompleted += OnSuccessfulPurchase;
		if (!Hook.ShouldAllowMerchantCardRemoval(base.Player.RunState, base.Player))
		{
			_removalEntry.SetUsed();
		}
		UpdateVisual();
	}

	protected override void UpdateVisual()
	{
		base.UpdateVisual();
		if (!_isUnavailable)
		{
			if (_removalEntry.Used)
			{
				_hitbox.MouseFilter = MouseFilterEnum.Ignore;
				_animator.CurrentAnimation = "Used";
				_isUnavailable = true;
				_animator.Play();
				_costLabel.Visible = false;
				_costContainer.Visible = false;
				base.FocusMode = FocusModeEnum.None;
			}
			else
			{
				base.MouseFilter = MouseFilterEnum.Stop;
				_costLabel.Visible = true;
				_costLabel.SetTextAutoSize(_removalEntry.Cost.ToString());
				_costLabel.Modulate = (_removalEntry.EnoughGold ? StsColors.cream : StsColors.red);
				_costContainer.Visible = true;
				base.FocusMode = FocusModeEnum.All;
			}
			ClearHoverTip();
		}
	}

	protected override async Task OnTryPurchase(MerchantInventory? inventory)
	{
		await _removalEntry.OnTryPurchaseWrapper(inventory);
	}

	protected void OnSuccessfulPurchase(PurchaseStatus _, MerchantEntry __)
	{
		TriggerMerchantHandToPointHere();
		UpdateVisual();
	}

	public void OnCardRemovalUsed()
	{
		_removalEntry.SetUsed();
		UpdateVisual();
	}

	protected override void CreateHoverTip()
	{
		LocString title = Title;
		LocString description = Description;
		description.Add("Amount", _removalEntry.CalcPriceIncrease());
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, new HoverTip(title, description));
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
		_removalEntry.EntryUpdated -= UpdateVisual;
		_removalEntry.PurchaseFailed -= base.OnPurchaseFailed;
		_removalEntry.PurchaseCompleted -= OnSuccessfulPurchase;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardRemovalUsed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnCardRemovalUsed && args.Count == 0)
		{
			OnCardRemovalUsed();
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
		if (method == MethodName.OnCardRemovalUsed)
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
		if (name == PropertyName._removalVisual)
		{
			_removalVisual = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._animator)
		{
			_animator = VariantUtils.ConvertTo<AnimationPlayer>(in value);
			return true;
		}
		if (name == PropertyName._costContainer)
		{
			_costContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isUnavailable)
		{
			_isUnavailable = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._removalVisual)
		{
			value = VariantUtils.CreateFrom(in _removalVisual);
			return true;
		}
		if (name == PropertyName._animator)
		{
			value = VariantUtils.CreateFrom(in _animator);
			return true;
		}
		if (name == PropertyName._costContainer)
		{
			value = VariantUtils.CreateFrom(in _costContainer);
			return true;
		}
		if (name == PropertyName._isUnavailable)
		{
			value = VariantUtils.CreateFrom(in _isUnavailable);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._removalVisual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._costContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isUnavailable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._removalVisual, Variant.From(in _removalVisual));
		info.AddProperty(PropertyName._animator, Variant.From(in _animator));
		info.AddProperty(PropertyName._costContainer, Variant.From(in _costContainer));
		info.AddProperty(PropertyName._isUnavailable, Variant.From(in _isUnavailable));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._removalVisual, out var value))
		{
			_removalVisual = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._animator, out var value2))
		{
			_animator = value2.As<AnimationPlayer>();
		}
		if (info.TryGetProperty(PropertyName._costContainer, out var value3))
		{
			_costContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isUnavailable, out var value4))
		{
			_isUnavailable = value4.As<bool>();
		}
	}
}
