using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

/// <summary>
/// Node class that manages the ordering of all overlay screens.
/// Any overlays added to the stack will sit on top of previously opened overlays,
/// Only the top overlay is considered active
/// overlays below it will not be active again until the overlays  on top of it are closed
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Overlays/NOverlayStack.cs")]
public class NOverlayStack : Control
{
	[Signal]
	public delegate void ChangedEventHandler();

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
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'Clear' method.
		/// </summary>
		public static readonly StringName Clear = "Clear";

		/// <summary>
		/// Cached name for the 'HideOverlays' method.
		/// </summary>
		public static readonly StringName HideOverlays = "HideOverlays";

		/// <summary>
		/// Cached name for the 'ShowOverlays' method.
		/// </summary>
		public static readonly StringName ShowOverlays = "ShowOverlays";

		/// <summary>
		/// Cached name for the 'ShowBackstop' method.
		/// </summary>
		public static readonly StringName ShowBackstop = "ShowBackstop";

		/// <summary>
		/// Cached name for the 'HideBackstop' method.
		/// </summary>
		public static readonly StringName HideBackstop = "HideBackstop";

		/// <summary>
		/// Cached name for the 'OnActiveScreenChanged' method.
		/// </summary>
		public static readonly StringName OnActiveScreenChanged = "OnActiveScreenChanged";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ScreenCount' property.
		/// </summary>
		public static readonly StringName ScreenCount = "ScreenCount";

		/// <summary>
		/// Cached name for the '_backstop' field.
		/// </summary>
		public static readonly StringName _backstop = "_backstop";

		/// <summary>
		/// Cached name for the '_backstopFade' field.
		/// </summary>
		public static readonly StringName _backstopFade = "_backstopFade";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'Changed' signal.
		/// </summary>
		public static readonly StringName Changed = "Changed";
	}

	private readonly List<IOverlayScreen> _overlays = new List<IOverlayScreen>();

	private Control _backstop;

	private Tween? _backstopFade;

	private ChangedEventHandler backing_Changed;

	public static NOverlayStack? Instance => NRun.Instance?.GlobalUi.Overlays;

	public int ScreenCount => _overlays.Count;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.Overlays.NOverlayStack.ChangedEventHandler" />
	public event ChangedEventHandler Changed
	{
		add
		{
			backing_Changed = (ChangedEventHandler)Delegate.Combine(backing_Changed, value);
		}
		remove
		{
			backing_Changed = (ChangedEventHandler)Delegate.Remove(backing_Changed, value);
		}
	}

	public override void _Ready()
	{
		_backstop = GetNode<Control>("OverlayBackstop");
		_backstop.Modulate = Colors.Transparent;
		_backstop.MouseFilter = MouseFilterEnum.Ignore;
		Callable.From(() => NMapScreen.Instance.Connect(NMapScreen.SignalName.Opened, Callable.From(HideOverlays))).CallDeferred();
		Callable.From(() => NMapScreen.Instance.Connect(NMapScreen.SignalName.Closed, Callable.From(ShowOverlays))).CallDeferred();
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenChanged;
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnActiveScreenChanged;
		Clear();
	}

	/// <summary>
	/// Adds a new screen to the top of the stack.
	/// Hides the previously open screen if there was one.
	/// </summary>
	/// <param name="screen">overlay screen that is being added to the stack.</param>
	public void Push(IOverlayScreen screen)
	{
		Peek()?.AfterOverlayHidden();
		this.AddChildSafely((Node)screen);
		_overlays.Add(screen);
		screen.AfterOverlayOpened();
		screen.AfterOverlayShown();
		_backstop.MouseFilter = MouseFilterEnum.Stop;
		_backstopFade?.Kill();
		this.MoveChildSafely(_backstop, _overlays.IndexOf(screen));
		if (!screen.UseSharedBackstop)
		{
			_backstop.Modulate = Colors.Transparent;
		}
		else if (ScreenCount == 1)
		{
			ShowBackstop();
		}
		else
		{
			_backstop.Modulate = Colors.White;
		}
		ActiveScreenContext.Instance.Update();
		EmitSignal(SignalName.Changed);
	}

	/// <summary>
	/// Removes the specified screen from the stack.
	/// Sets up the previously open screen if there is one and the top screen was removed.
	/// Note: We have this instead of Pop because a screen is responsible for removing itself, and we can't guarantee
	/// that it'll be at the top of the stack when it needs to do this.
	/// </summary>
	/// <param name="screen">Overlay screen to be removed.</param>
	public void Remove(IOverlayScreen screen)
	{
		bool flag = screen == Peek();
		if (flag)
		{
			HideBackstop();
			screen.AfterOverlayHidden();
		}
		screen.AfterOverlayClosed();
		_overlays.Remove(screen);
		if (flag)
		{
			IOverlayScreen overlayScreen = Peek();
			if (overlayScreen != null)
			{
				_backstop.MouseFilter = MouseFilterEnum.Stop;
				this.MoveChildSafely(_backstop, _overlays.IndexOf(overlayScreen));
				if (overlayScreen.UseSharedBackstop)
				{
					_backstop.Modulate = Colors.White;
				}
				else
				{
					HideBackstop();
				}
				overlayScreen.AfterOverlayShown();
			}
			else
			{
				HideBackstop();
			}
		}
		ActiveScreenContext.Instance.Update();
		EmitSignal(SignalName.Changed);
	}

	public void Clear()
	{
		for (IOverlayScreen overlayScreen = Peek(); overlayScreen != null; overlayScreen = Peek())
		{
			Remove(overlayScreen);
		}
	}

	public void HideOverlays()
	{
		_backstop.Modulate = Colors.Transparent;
		Peek()?.AfterOverlayHidden();
	}

	public void ShowOverlays()
	{
		IOverlayScreen overlayScreen = Peek();
		if (overlayScreen != null && !NMapScreen.Instance.IsOpen)
		{
			_backstop.Modulate = (overlayScreen.UseSharedBackstop ? Colors.White : Colors.Transparent);
			overlayScreen.AfterOverlayShown();
		}
	}

	public void ShowBackstop()
	{
		IOverlayScreen? overlayScreen = Peek();
		if (overlayScreen == null || overlayScreen.UseSharedBackstop)
		{
			_backstop.MouseFilter = MouseFilterEnum.Stop;
			_backstopFade?.Kill();
			_backstopFade = CreateTween();
			_backstopFade.TweenProperty(_backstop, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
	}

	public void HideBackstop()
	{
		IOverlayScreen? overlayScreen = Peek();
		if (overlayScreen == null || overlayScreen.UseSharedBackstop)
		{
			_backstop.MouseFilter = MouseFilterEnum.Ignore;
			_backstopFade?.Kill();
			if (ScreenCount <= 1)
			{
				_backstopFade = CreateTween();
				_backstopFade.TweenProperty(_backstop, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			}
			else
			{
				_backstop.Modulate = Colors.Transparent;
			}
		}
	}

	public IOverlayScreen? Peek()
	{
		return _overlays.LastOrDefault();
	}

	private void OnActiveScreenChanged()
	{
		IOverlayScreen overlayScreen = Peek();
		if (overlayScreen != null)
		{
			if (ActiveScreenContext.Instance.IsCurrent(overlayScreen))
			{
				base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Enabled;
			}
			else
			{
				base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Disabled;
			}
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
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Clear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideOverlays, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowOverlays, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Clear && args.Count == 0)
		{
			Clear();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideOverlays && args.Count == 0)
		{
			HideOverlays();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowOverlays && args.Count == 0)
		{
			ShowOverlays();
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
		if (method == MethodName.OnActiveScreenChanged && args.Count == 0)
		{
			OnActiveScreenChanged();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Clear)
		{
			return true;
		}
		if (method == MethodName.HideOverlays)
		{
			return true;
		}
		if (method == MethodName.ShowOverlays)
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
		if (method == MethodName.OnActiveScreenChanged)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backstopFade)
		{
			_backstopFade = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenCount)
		{
			value = VariantUtils.CreateFrom<int>(ScreenCount);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._backstopFade)
		{
			value = VariantUtils.CreateFrom(in _backstopFade);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstopFade, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._backstopFade, Variant.From(in _backstopFade));
		info.AddSignalEventDelegate(SignalName.Changed, backing_Changed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backstop, out var value))
		{
			_backstop = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backstopFade, out var value2))
		{
			_backstopFade = value2.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ChangedEventHandler>(SignalName.Changed, out var value3))
		{
			backing_Changed = value3;
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
		list.Add(new MethodInfo(SignalName.Changed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalChanged()
	{
		EmitSignal(SignalName.Changed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Changed && args.Count == 0)
		{
			backing_Changed?.Invoke();
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
		if (signal == SignalName.Changed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
