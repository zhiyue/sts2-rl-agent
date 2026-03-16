using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantSlot.cs")]
public abstract class NMerchantSlot : Control
{
	[Signal]
	public delegate void HoveredEventHandler(NMerchantSlot slot);

	[Signal]
	public delegate void UnhoveredEventHandler(NMerchantSlot slot);

	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Initialize = "Initialize";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName OnMousePressed = "OnMousePressed";

		public static readonly StringName OnMouseReleased = "OnMouseReleased";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName TriggerMerchantHandToPointHere = "TriggerMerchantHandToPointHere";

		public static readonly StringName OnPreview = "OnPreview";

		public static readonly StringName CreateHoverTip = "CreateHoverTip";

		public static readonly StringName ClearHoverTip = "ClearHoverTip";

		public static readonly StringName OnMerchantHandHovered = "OnMerchantHandHovered";

		public static readonly StringName OnMerchantHandUnhovered = "OnMerchantHandUnhovered";

		public static readonly StringName OnPurchaseFailed = "OnPurchaseFailed";

		public static readonly StringName UpdateVisual = "UpdateVisual";

		public static readonly StringName WiggleAnimation = "WiggleAnimation";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Hitbox = "Hitbox";

		public static readonly StringName Visual = "Visual";

		public static readonly StringName _isHovered = "_isHovered";

		public static readonly StringName _hitbox = "_hitbox";

		public static readonly StringName _costLabel = "_costLabel";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _purchaseFailedTween = "_purchaseFailedTween";

		public static readonly StringName _merchantRug = "_merchantRug";

		public static readonly StringName _ignoreMouseRelease = "_ignoreMouseRelease";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName Hovered = "Hovered";

		public static readonly StringName Unhovered = "Unhovered";
	}

	private bool _isHovered;

	private static readonly Vector2 _hoverScale = Vector2.One * 0.8f;

	private static readonly Vector2 _smallScale = Vector2.One * 0.65f;

	protected NClickableControl _hitbox;

	protected MegaLabel _costLabel;

	private Tween? _hoverTween;

	private Tween? _purchaseFailedTween;

	private NMerchantInventory? _merchantRug;

	private bool _ignoreMouseRelease;

	private float? _originalVisualPosition;

	private HoveredEventHandler backing_Hovered;

	private UnhoveredEventHandler backing_Unhovered;

	public NClickableControl Hitbox => _hitbox;

	public abstract MerchantEntry Entry { get; }

	protected abstract CanvasItem Visual { get; }

	protected Player? Player => _merchantRug?.Inventory?.Player;

	public event HoveredEventHandler Hovered
	{
		add
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Combine(backing_Hovered, value);
		}
		remove
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Remove(backing_Hovered, value);
		}
	}

	public event UnhoveredEventHandler Unhovered
	{
		add
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Combine(backing_Unhovered, value);
		}
		remove
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Remove(backing_Unhovered, value);
		}
	}

	public void Initialize(NMerchantInventory rug)
	{
		_merchantRug = rug;
		Player.GoldChanged += UpdateVisual;
		Connect(SignalName.Hovered, Callable.From<NMerchantSlot>(OnMerchantHandHovered));
		Connect(SignalName.Unhovered, Callable.From<NMerchantSlot>(OnMerchantHandUnhovered));
	}

	public override void _Ready()
	{
		if (GetType() != typeof(NMerchantSlot))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected virtual void ConnectSignals()
	{
		_hitbox = GetNode<NClickableControl>("%Hitbox");
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		_hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
		_hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
		_hitbox.Connect(NClickableControl.SignalName.MousePressed, Callable.From<InputEvent>(OnMousePressed));
		_hitbox.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(OnMouseReleased));
		_costLabel = GetNode<MegaLabel>("%CostLabel");
	}

	public override void _ExitTree()
	{
		Disconnect(SignalName.Unhovered, Callable.From<NMerchantSlot>(OnMerchantHandUnhovered));
		_hitbox.Disconnect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
		Disconnect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		_hoverTween?.Kill();
		if (Player != null)
		{
			Player.GoldChanged -= UpdateVisual;
		}
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(MegaInput.select))
		{
			TaskHelper.RunSafely(OnReleased());
		}
		else if (inputEvent.IsActionReleased(MegaInput.accept))
		{
			OnPreview();
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnMousePressed(InputEvent inputEvent)
	{
		_ignoreMouseRelease = false;
	}

	private void OnMouseReleased(InputEvent inputEvent)
	{
		if (_isHovered && !_ignoreMouseRelease && inputEvent is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				TaskHelper.RunSafely(OnReleased());
			}
			else
			{
				OnPreview();
			}
		}
	}

	private void OnFocus()
	{
		_isHovered = true;
		_hoverTween?.Kill();
		base.Scale = _hoverScale;
		CreateHoverTip();
		EmitSignal(SignalName.Hovered, this);
	}

	private void OnUnfocus()
	{
		_isHovered = false;
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(this, "scale", _smallScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		ClearHoverTip();
		EmitSignal(SignalName.Unhovered, this);
	}

	private async Task OnReleased()
	{
		ClearHoverTip();
		await OnTryPurchase(_merchantRug?.Inventory);
		MerchantEntry entry = Entry;
		if (entry != null && entry.IsStocked && _isHovered)
		{
			CreateHoverTip();
		}
	}

	protected abstract Task OnTryPurchase(MerchantInventory? inventory);

	protected void TriggerMerchantHandToPointHere()
	{
		_merchantRug?.MerchantHand.PointAtTarget(base.GlobalPosition);
		_merchantRug?.MerchantHand.StopPointing(2f);
	}

	protected virtual void OnPreview()
	{
	}

	protected abstract void CreateHoverTip();

	protected void ClearHoverTip()
	{
		NHoverTipSet.Remove(this);
	}

	private void OnMerchantHandHovered(NMerchantSlot _)
	{
		_merchantRug?.MerchantHand.PointAtTarget(base.GlobalPosition);
	}

	private void OnMerchantHandUnhovered(NMerchantSlot _)
	{
		_merchantRug?.MerchantHand.StopPointing(2f);
	}

	protected void OnPurchaseFailed(PurchaseStatus status)
	{
		if (status == PurchaseStatus.Success)
		{
			return;
		}
		if (!_originalVisualPosition.HasValue)
		{
			if (Visual is Node2D node2D)
			{
				_originalVisualPosition = node2D.Position.X;
			}
			else if (Visual is Control control)
			{
				_originalVisualPosition = control.Position.X;
			}
		}
		_purchaseFailedTween?.Kill();
		_purchaseFailedTween = CreateTween();
		_purchaseFailedTween.TweenMethod(Callable.From<float>(WiggleAnimation), 0f, 2f, 0.4000000059604645).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		SfxCmd.Play("event:/sfx/npcs/merchant/merchant_dissapointment");
	}

	protected virtual void UpdateVisual()
	{
		if (Entry.IsStocked)
		{
			_costLabel.SetTextAutoSize(Entry.Cost.ToString());
		}
	}

	private void WiggleAnimation(float progress)
	{
		if (Visual is Node2D node2D)
		{
			Vector2 position = node2D.Position;
			position.X = _originalVisualPosition.Value + (float)Math.Sin(progress * (float)Math.PI * 2f) * 10f;
			node2D.Position = position;
		}
		else if (Visual is Control control)
		{
			Vector2 position = control.Position;
			position.X = _originalVisualPosition.Value + (float)Math.Sin(progress * (float)Math.PI * 2f) * 10f;
			control.Position = position;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "rug", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMousePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TriggerMerchantHandToPointHere, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMerchantHandHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMerchantHandUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPurchaseFailed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.WiggleAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "progress", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Initialize && args.Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<NMerchantInventory>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMousePressed && args.Count == 1)
		{
			OnMousePressed(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseReleased && args.Count == 1)
		{
			OnMouseReleased(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TriggerMerchantHandToPointHere && args.Count == 0)
		{
			TriggerMerchantHandToPointHere();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPreview && args.Count == 0)
		{
			OnPreview();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateHoverTip && args.Count == 0)
		{
			CreateHoverTip();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearHoverTip && args.Count == 0)
		{
			ClearHoverTip();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMerchantHandHovered && args.Count == 1)
		{
			OnMerchantHandHovered(VariantUtils.ConvertTo<NMerchantSlot>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMerchantHandUnhovered && args.Count == 1)
		{
			OnMerchantHandUnhovered(VariantUtils.ConvertTo<NMerchantSlot>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPurchaseFailed && args.Count == 1)
		{
			OnPurchaseFailed(VariantUtils.ConvertTo<PurchaseStatus>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVisual && args.Count == 0)
		{
			UpdateVisual();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WiggleAnimation && args.Count == 1)
		{
			WiggleAnimation(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.OnMousePressed)
		{
			return true;
		}
		if (method == MethodName.OnMouseReleased)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.TriggerMerchantHandToPointHere)
		{
			return true;
		}
		if (method == MethodName.OnPreview)
		{
			return true;
		}
		if (method == MethodName.CreateHoverTip)
		{
			return true;
		}
		if (method == MethodName.ClearHoverTip)
		{
			return true;
		}
		if (method == MethodName.OnMerchantHandHovered)
		{
			return true;
		}
		if (method == MethodName.OnMerchantHandUnhovered)
		{
			return true;
		}
		if (method == MethodName.OnPurchaseFailed)
		{
			return true;
		}
		if (method == MethodName.UpdateVisual)
		{
			return true;
		}
		if (method == MethodName.WiggleAnimation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			_hitbox = VariantUtils.ConvertTo<NClickableControl>(in value);
			return true;
		}
		if (name == PropertyName._costLabel)
		{
			_costLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._purchaseFailedTween)
		{
			_purchaseFailedTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._merchantRug)
		{
			_merchantRug = VariantUtils.ConvertTo<NMerchantInventory>(in value);
			return true;
		}
		if (name == PropertyName._ignoreMouseRelease)
		{
			_ignoreMouseRelease = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			value = VariantUtils.CreateFrom<NClickableControl>(Hitbox);
			return true;
		}
		if (name == PropertyName.Visual)
		{
			value = VariantUtils.CreateFrom<CanvasItem>(Visual);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			value = VariantUtils.CreateFrom(in _hitbox);
			return true;
		}
		if (name == PropertyName._costLabel)
		{
			value = VariantUtils.CreateFrom(in _costLabel);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._purchaseFailedTween)
		{
			value = VariantUtils.CreateFrom(in _purchaseFailedTween);
			return true;
		}
		if (name == PropertyName._merchantRug)
		{
			value = VariantUtils.CreateFrom(in _merchantRug);
			return true;
		}
		if (name == PropertyName._ignoreMouseRelease)
		{
			value = VariantUtils.CreateFrom(in _ignoreMouseRelease);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._costLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._purchaseFailedTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._merchantRug, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._ignoreMouseRelease, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._hitbox, Variant.From(in _hitbox));
		info.AddProperty(PropertyName._costLabel, Variant.From(in _costLabel));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._purchaseFailedTween, Variant.From(in _purchaseFailedTween));
		info.AddProperty(PropertyName._merchantRug, Variant.From(in _merchantRug));
		info.AddProperty(PropertyName._ignoreMouseRelease, Variant.From(in _ignoreMouseRelease));
		info.AddSignalEventDelegate(SignalName.Hovered, backing_Hovered);
		info.AddSignalEventDelegate(SignalName.Unhovered, backing_Unhovered);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._isHovered, out var value))
		{
			_isHovered = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hitbox, out var value2))
		{
			_hitbox = value2.As<NClickableControl>();
		}
		if (info.TryGetProperty(PropertyName._costLabel, out var value3))
		{
			_costLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value4))
		{
			_hoverTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._purchaseFailedTween, out var value5))
		{
			_purchaseFailedTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._merchantRug, out var value6))
		{
			_merchantRug = value6.As<NMerchantInventory>();
		}
		if (info.TryGetProperty(PropertyName._ignoreMouseRelease, out var value7))
		{
			_ignoreMouseRelease = value7.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<HoveredEventHandler>(SignalName.Hovered, out var value8))
		{
			backing_Hovered = value8;
		}
		if (info.TryGetSignalEventDelegate<UnhoveredEventHandler>(SignalName.Unhovered, out var value9))
		{
			backing_Unhovered = value9;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Hovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "slot", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.Unhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "slot", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHovered(NMerchantSlot slot)
	{
		EmitSignal(SignalName.Hovered, slot);
	}

	protected void EmitSignalUnhovered(NMerchantSlot slot)
	{
		EmitSignal(SignalName.Unhovered, slot);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Hovered && args.Count == 1)
		{
			backing_Hovered?.Invoke(VariantUtils.ConvertTo<NMerchantSlot>(in args[0]));
		}
		else if (signal == SignalName.Unhovered && args.Count == 1)
		{
			backing_Unhovered?.Invoke(VariantUtils.ConvertTo<NMerchantSlot>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Hovered)
		{
			return true;
		}
		if (signal == SignalName.Unhovered)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
