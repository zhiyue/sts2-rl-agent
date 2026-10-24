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
		/// Cached name for the 'ConnectSignals' method.
		/// </summary>
		public static readonly StringName ConnectSignals = "ConnectSignals";

		/// <summary>
		/// Cached name for the 'HideBackButtonImmediately' method.
		/// </summary>
		public static readonly StringName HideBackButtonImmediately = "HideBackButtonImmediately";

		/// <summary>
		/// Cached name for the 'SetStack' method.
		/// </summary>
		public static readonly StringName SetStack = "SetStack";

		/// <summary>
		/// Cached name for the 'OnScreenVisibilityChange' method.
		/// </summary>
		public static readonly StringName OnScreenVisibilityChange = "OnScreenVisibilityChange";

		/// <summary>
		/// Cached name for the 'OnSubmenuShown' method.
		/// </summary>
		public static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		/// <summary>
		/// Cached name for the 'OnSubmenuHidden' method.
		/// </summary>
		public static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the '_backButton' field.
		/// </summary>
		public static readonly StringName _backButton = "_backButton";

		/// <summary>
		/// Cached name for the '_stack' field.
		/// </summary>
		public static readonly StringName _stack = "_stack";

		/// <summary>
		/// Cached name for the '_lastFocusedControl' field.
		/// </summary>
		public static readonly StringName _lastFocusedControl = "_lastFocusedControl";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private NBackButton _backButton;

	protected NSubmenuStack _stack;

	/// <summary>
	/// The last control node we were focusing on when this submenu was hidden (usually when another submenu is opened above it)
	/// </summary>
	protected Control? _lastFocusedControl;

	public Control? DefaultFocusedControl => _lastFocusedControl ?? InitialFocusedControl;

	/// <summary>
	/// The initial control the submenu is focused on when it is first opened
	/// </summary>
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

	/// <summary>
	/// Used to override submenu back button behaviors (see: Timeline)
	/// </summary>
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

	/// <summary>
	/// Called when this submenu is newly pushed onto the stack OR when it is re-shown because the submenu above it has
	/// been popped from the stack.
	/// </summary>
	protected virtual void OnSubmenuShown()
	{
	}

	/// <summary>
	/// Called when this submenu has just been popped from the stack OR when it is hidden because a new submenu has just
	/// been pushed onto the stack.
	/// </summary>
	protected virtual void OnSubmenuHidden()
	{
	}

	/// <summary>
	/// Called only when this submenu is newly pushed on to a stack.
	/// </summary>
	public virtual void OnSubmenuOpened()
	{
	}

	/// <summary>
	/// Called only when this submenu is popped from the stack it was on.
	/// </summary>
	public virtual void OnSubmenuClosed()
	{
		_lastFocusedControl = null;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._stack, Variant.From(in _stack));
		info.AddProperty(PropertyName._lastFocusedControl, Variant.From(in _lastFocusedControl));
	}

	/// <inheritdoc />
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
