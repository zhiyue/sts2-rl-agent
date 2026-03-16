using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NSubmenu.cs")]
public abstract class NSubmenu : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName HideBackButtonImmediately = "HideBackButtonImmediately";

		public static readonly StringName SetStack = "SetStack";

		public static readonly StringName OnScreenVisibilityChange = "OnScreenVisibilityChange";

		public static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _stack = "_stack";

		public static readonly StringName _lastFocusedControl = "_lastFocusedControl";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NBackButton _backButton;

	protected NSubmenuStack _stack;

	protected Control? _lastFocusedControl;

	public Control? DefaultFocusedControl => _lastFocusedControl ?? InitialFocusedControl;

	protected abstract Control? InitialFocusedControl { get; }

	public override void _Ready()
	{
		if (GetType() != typeof(NSubmenu))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected virtual void ConnectSignals()
	{
		_backButton = GetNode<NBackButton>("BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			_stack.Pop();
		}));
		_backButton.Disable();
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnScreenVisibilityChange));
	}

	public void HideBackButtonImmediately()
	{
		_backButton.Disable();
		_backButton.MoveToHidePosition();
	}

	public void SetStack(NSubmenuStack stack)
	{
		_stack = stack;
	}

	private void OnScreenVisibilityChange()
	{
		if (base.Visible)
		{
			_backButton.MoveToHidePosition();
			_backButton.Enable();
			OnSubmenuShown();
		}
		else
		{
			_lastFocusedControl = GetViewport()?.GuiGetFocusOwner();
			_backButton.Disable();
			OnSubmenuHidden();
		}
	}

	protected virtual void OnSubmenuShown()
	{
	}

	protected virtual void OnSubmenuHidden()
	{
	}

	public virtual void OnSubmenuOpened()
	{
	}

	public virtual void OnSubmenuClosed()
	{
		_lastFocusedControl = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideBackButtonImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetStack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "stack", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnScreenVisibilityChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideBackButtonImmediately && args.Count == 0)
		{
			HideBackButtonImmediately();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetStack && args.Count == 1)
		{
			SetStack(VariantUtils.ConvertTo<NSubmenuStack>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenVisibilityChange && args.Count == 0)
		{
			OnScreenVisibilityChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.HideBackButtonImmediately)
		{
			return true;
		}
		if (method == MethodName.SetStack)
		{
			return true;
		}
		if (method == MethodName.OnScreenVisibilityChange)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._stack)
		{
			_stack = VariantUtils.ConvertTo<NSubmenuStack>(in value);
			return true;
		}
		if (name == PropertyName._lastFocusedControl)
		{
			_lastFocusedControl = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.InitialFocusedControl)
		{
			from = InitialFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._stack)
		{
			value = VariantUtils.CreateFrom(in _stack);
			return true;
		}
		if (name == PropertyName._lastFocusedControl)
		{
			value = VariantUtils.CreateFrom(in _lastFocusedControl);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._stack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._stack, Variant.From(in _stack));
		info.AddProperty(PropertyName._lastFocusedControl, Variant.From(in _lastFocusedControl));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backButton, out var value))
		{
			_backButton = value.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._stack, out var value2))
		{
			_stack = value2.As<NSubmenuStack>();
		}
		if (info.TryGetProperty(PropertyName._lastFocusedControl, out var value3))
		{
			_lastFocusedControl = value3.As<Control>();
		}
	}
}
