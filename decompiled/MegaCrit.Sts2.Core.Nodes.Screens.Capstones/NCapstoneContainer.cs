using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

/// <summary>
/// Node class that manages the current capstone screen.
/// If a new Capstone screen is opened, any previously open capstone screens are destroyed.
/// If a new Capstone screen is opened, any previously open overlay screens are hidden until there are no more capstone screens.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Capstones/NCapstoneContainer.cs")]
public class NCapstoneContainer : Control
{
	[Signal]
	public delegate void ChangedEventHandler();

	[Signal]
	public delegate void CapstoneClosedEventHandler();

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
		/// Cached name for the 'Close' method.
		/// </summary>
		public static readonly StringName Close = "Close";

		/// <summary>
		/// Cached name for the 'CloseInternal' method.
		/// </summary>
		public static readonly StringName CloseInternal = "CloseInternal";

		/// <summary>
		/// Cached name for the 'DisableBackstopInstantly' method.
		/// </summary>
		public static readonly StringName DisableBackstopInstantly = "DisableBackstopInstantly";

		/// <summary>
		/// Cached name for the 'EnableBackstopInstantly' method.
		/// </summary>
		public static readonly StringName EnableBackstopInstantly = "EnableBackstopInstantly";

		/// <summary>
		/// Cached name for the 'CleanUp' method.
		/// </summary>
		public static readonly StringName CleanUp = "CleanUp";

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
		/// Cached name for the 'InUse' property.
		/// </summary>
		public static readonly StringName InUse = "InUse";

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

		/// <summary>
		/// Cached name for the 'CapstoneClosed' signal.
		/// </summary>
		public static readonly StringName CapstoneClosed = "CapstoneClosed";
	}

	private Control _backstop;

	private Tween? _backstopFade;

	private ChangedEventHandler backing_Changed;

	private CapstoneClosedEventHandler backing_CapstoneClosed;

	public ICapstoneScreen? CurrentCapstoneScreen { get; private set; }

	public bool InUse => CurrentCapstoneScreen != null;

	public static NCapstoneContainer? Instance => NRun.Instance?.GlobalUi.CapstoneContainer;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.Capstones.NCapstoneContainer.ChangedEventHandler" />
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

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.Capstones.NCapstoneContainer.CapstoneClosedEventHandler" />
	public event CapstoneClosedEventHandler CapstoneClosed
	{
		add
		{
			backing_CapstoneClosed = (CapstoneClosedEventHandler)Delegate.Combine(backing_CapstoneClosed, value);
		}
		remove
		{
			backing_CapstoneClosed = (CapstoneClosedEventHandler)Delegate.Remove(backing_CapstoneClosed, value);
		}
	}

	public override void _Ready()
	{
		_backstop = GetNode<Control>("CapstoneBackstop");
		_backstop.Modulate = Colors.Transparent;
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenChanged;
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnActiveScreenChanged;
	}

	public void Open(ICapstoneScreen screen)
	{
		NHoverTipSet.Clear();
		bool flag = CurrentCapstoneScreen != null;
		if (flag)
		{
			CloseInternal();
		}
		_backstopFade?.Kill();
		NOverlayStack.Instance.HideOverlays();
		if (!screen.UseSharedBackstop)
		{
			_backstop.Modulate = Colors.Transparent;
		}
		else if (flag || NOverlayStack.Instance.ScreenCount > 0)
		{
			_backstop.Modulate = Colors.White;
		}
		else
		{
			_backstopFade = CreateTween();
			_backstopFade.TweenProperty(_backstop, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
		CurrentCapstoneScreen = screen;
		if (!GetChildren().Contains((Node)screen))
		{
			this.AddChildSafely((Node)screen);
		}
		((Node)screen).ProcessMode = ProcessModeEnum.Inherit;
		screen.AfterCapstoneOpened();
		if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			CombatManager.Instance.Pause();
		}
		ActiveScreenContext.Instance.Update();
		EmitSignal(SignalName.Changed);
	}

	public void Close()
	{
		if (CurrentCapstoneScreen != null)
		{
			CloseInternal();
			ActiveScreenContext.Instance.Update();
			EmitSignal(SignalName.CapstoneClosed);
			EmitSignal(SignalName.Changed);
		}
	}

	private void CloseInternal()
	{
		if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			CombatManager.Instance.Unpause();
		}
		NOverlayStack.Instance.ShowOverlays();
		if (NOverlayStack.Instance.ScreenCount > 0)
		{
			_backstop.Modulate = Colors.Transparent;
		}
		else
		{
			_backstopFade?.Kill();
			_backstopFade = CreateTween();
			_backstopFade.TweenProperty(_backstop, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
		ICapstoneScreen currentCapstoneScreen = CurrentCapstoneScreen;
		CurrentCapstoneScreen = null;
		if (currentCapstoneScreen is Node node)
		{
			node.ProcessMode = ProcessModeEnum.Disabled;
		}
		currentCapstoneScreen?.AfterCapstoneClosed();
		NHoverTipSet.Clear();
	}

	public void DisableBackstopInstantly()
	{
		_backstopFade?.Kill();
		_backstop.Modulate = Colors.Transparent;
	}

	public void EnableBackstopInstantly()
	{
		_backstopFade?.Kill();
		_backstop.Modulate = Colors.White;
	}

	/// <summary>
	/// Called just before the run is disposed.
	/// </summary>
	public void CleanUp()
	{
		if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			CombatManager.Instance.Unpause();
		}
	}

	private void OnActiveScreenChanged()
	{
		if (InUse)
		{
			if (ActiveScreenContext.Instance.IsCurrent(CurrentCapstoneScreen))
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
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CloseInternal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableBackstopInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableBackstopInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CleanUp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseInternal && args.Count == 0)
		{
			CloseInternal();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableBackstopInstantly && args.Count == 0)
		{
			DisableBackstopInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableBackstopInstantly && args.Count == 0)
		{
			EnableBackstopInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CleanUp && args.Count == 0)
		{
			CleanUp();
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
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.CloseInternal)
		{
			return true;
		}
		if (method == MethodName.DisableBackstopInstantly)
		{
			return true;
		}
		if (method == MethodName.EnableBackstopInstantly)
		{
			return true;
		}
		if (method == MethodName.CleanUp)
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
		if (name == PropertyName.InUse)
		{
			value = VariantUtils.CreateFrom<bool>(InUse);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InUse, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
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
		info.AddSignalEventDelegate(SignalName.CapstoneClosed, backing_CapstoneClosed);
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
		if (info.TryGetSignalEventDelegate<CapstoneClosedEventHandler>(SignalName.CapstoneClosed, out var value4))
		{
			backing_CapstoneClosed = value4;
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
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Changed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.CapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalChanged()
	{
		EmitSignal(SignalName.Changed);
	}

	protected void EmitSignalCapstoneClosed()
	{
		EmitSignal(SignalName.CapstoneClosed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Changed && args.Count == 0)
		{
			backing_Changed?.Invoke();
		}
		else if (signal == SignalName.CapstoneClosed && args.Count == 0)
		{
			backing_CapstoneClosed?.Invoke();
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
		if (signal == SignalName.CapstoneClosed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
