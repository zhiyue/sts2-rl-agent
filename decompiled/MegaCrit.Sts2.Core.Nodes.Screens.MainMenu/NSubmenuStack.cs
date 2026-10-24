using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

/// <summary>
/// Manages submenus in the main menu.
/// Also manages specific submenus while in-game (the compendium stack).
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NSubmenuStack.cs")]
public abstract class NSubmenuStack : Control
{
	[Signal]
	public delegate void StackModifiedEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'InitializeForMainMenu' method.
		/// </summary>
		public static readonly StringName InitializeForMainMenu = "InitializeForMainMenu";

		/// <summary>
		/// Cached name for the 'Push' method.
		/// </summary>
		public static readonly StringName Push = "Push";

		/// <summary>
		/// Cached name for the 'Pop' method.
		/// </summary>
		public static readonly StringName Pop = "Pop";

		/// <summary>
		/// Cached name for the 'ShowBackstop' method.
		/// </summary>
		public static readonly StringName ShowBackstop = "ShowBackstop";

		/// <summary>
		/// Cached name for the 'HideBackstop' method.
		/// </summary>
		public static readonly StringName HideBackstop = "HideBackstop";

		/// <summary>
		/// Cached name for the 'Peek' method.
		/// </summary>
		public static readonly StringName Peek = "Peek";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'SubmenusOpen' property.
		/// </summary>
		public static readonly StringName SubmenusOpen = "SubmenusOpen";

		/// <summary>
		/// Cached name for the '_mainMenu' field.
		/// </summary>
		public static readonly StringName _mainMenu = "_mainMenu";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'StackModified' signal.
		/// </summary>
		public static readonly StringName StackModified = "StackModified";
	}

	private readonly Stack<NSubmenu> _submenus = new Stack<NSubmenu>();

	private NMainMenu? _mainMenu;

	private StackModifiedEventHandler backing_StackModified;

	public bool SubmenusOpen => _submenus.Count > 0;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenuStack.StackModifiedEventHandler" />
	public event StackModifiedEventHandler StackModified
	{
		add
		{
			backing_StackModified = (StackModifiedEventHandler)Delegate.Combine(backing_StackModified, value);
		}
		remove
		{
			backing_StackModified = (StackModifiedEventHandler)Delegate.Remove(backing_StackModified, value);
		}
	}

	public void InitializeForMainMenu(NMainMenu mainMenu)
	{
		_mainMenu = mainMenu;
	}

	public abstract T PushSubmenuType<T>() where T : NSubmenu;

	public abstract T GetSubmenuType<T>() where T : NSubmenu;

	public abstract NSubmenu PushSubmenuType(Type type);

	public abstract NSubmenu GetSubmenuType(Type type);

	public void Push(NSubmenu screen)
	{
		if (_submenus.Count > 0)
		{
			NSubmenu nSubmenu = _submenus.Peek();
			nSubmenu.Visible = false;
			nSubmenu.MouseFilter = MouseFilterEnum.Ignore;
		}
		screen.SetStack(this);
		_submenus.Push(screen);
		screen.OnSubmenuOpened();
		screen.Visible = true;
		screen.MouseFilter = MouseFilterEnum.Stop;
		_mainMenu?.EnableBackstop();
		ActiveScreenContext.Instance.Update();
		EmitSignal(SignalName.StackModified);
	}

	public void Pop()
	{
		NSubmenu nSubmenu = _submenus.Pop();
		nSubmenu.Visible = false;
		nSubmenu.MouseFilter = MouseFilterEnum.Ignore;
		nSubmenu.OnSubmenuClosed();
		if (_submenus.Count > 0)
		{
			NSubmenu nSubmenu2 = _submenus.Peek();
			nSubmenu2.Visible = true;
			nSubmenu2.MouseFilter = MouseFilterEnum.Stop;
		}
		else
		{
			HideBackstop();
		}
		ActiveScreenContext.Instance.Update();
		EmitSignal(SignalName.StackModified);
	}

	private void ShowBackstop()
	{
		_mainMenu?.EnableBackstop();
	}

	private void HideBackstop()
	{
		_mainMenu?.DisableBackstop();
	}

	public NSubmenu? Peek()
	{
		if (!_submenus.TryPeek(out NSubmenu result))
		{
			return null;
		}
		return result;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.InitializeForMainMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mainMenu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Push, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "screen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Pop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Peek, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.InitializeForMainMenu && args.Count == 1)
		{
			InitializeForMainMenu(VariantUtils.ConvertTo<NMainMenu>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Push && args.Count == 1)
		{
			Push(VariantUtils.ConvertTo<NSubmenu>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Pop && args.Count == 0)
		{
			Pop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowBackstop && args.Count == 0)
		{
			ShowBackstop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideBackstop && args.Count == 0)
		{
			HideBackstop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Peek && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NSubmenu>(Peek());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.InitializeForMainMenu)
		{
			return true;
		}
		if (method == MethodName.Push)
		{
			return true;
		}
		if (method == MethodName.Pop)
		{
			return true;
		}
		if (method == MethodName.ShowBackstop)
		{
			return true;
		}
		if (method == MethodName.HideBackstop)
		{
			return true;
		}
		if (method == MethodName.Peek)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._mainMenu)
		{
			_mainMenu = VariantUtils.ConvertTo<NMainMenu>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SubmenusOpen)
		{
			value = VariantUtils.CreateFrom<bool>(SubmenusOpen);
			return true;
		}
		if (name == PropertyName._mainMenu)
		{
			value = VariantUtils.CreateFrom(in _mainMenu);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainMenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.SubmenusOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._mainMenu, Variant.From(in _mainMenu));
		info.AddSignalEventDelegate(SignalName.StackModified, backing_StackModified);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._mainMenu, out var value))
		{
			_mainMenu = value.As<NMainMenu>();
		}
		if (info.TryGetSignalEventDelegate<StackModifiedEventHandler>(SignalName.StackModified, out var value2))
		{
			backing_StackModified = value2;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.StackModified, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalStackModified()
	{
		EmitSignal(SignalName.StackModified);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.StackModified && args.Count == 0)
		{
			backing_StackModified?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.StackModified)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
