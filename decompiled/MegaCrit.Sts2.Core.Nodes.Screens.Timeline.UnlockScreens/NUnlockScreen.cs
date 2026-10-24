using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

/// <summary>
/// Abstract class of the Unlock Screens. Used for general animation and some logistics required for the Timeline Screen.
/// All Unlock Screens in the Timeline must use this!
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockScreen.cs")]
public abstract class NUnlockScreen : Control, IScreenContext
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
		/// Cached name for the 'Open' method.
		/// </summary>
		public static readonly StringName Open = "Open";

		/// <summary>
		/// Cached name for the 'OnScreenPreClose' method.
		/// </summary>
		public static readonly StringName OnScreenPreClose = "OnScreenPreClose";

		/// <summary>
		/// Cached name for the 'OnScreenClose' method.
		/// </summary>
		public static readonly StringName OnScreenClose = "OnScreenClose";
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
		/// Cached name for the '_unlockConfirmButton' field.
		/// </summary>
		public static readonly StringName _unlockConfirmButton = "_unlockConfirmButton";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private NUnlockConfirmButton? _unlockConfirmButton;

	private Tween? _tween;

	public virtual Control? DefaultFocusedControl => null;

	public override void _Ready()
	{
		if (GetType() != typeof(NUnlockScreen))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected void ConnectSignals()
	{
		_unlockConfirmButton = GetNode<NUnlockConfirmButton>("ConfirmButton");
		_unlockConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
		{
			TaskHelper.RunSafely(Close());
		}));
	}

	/// <summary>
	/// Useful for UI stuff because this Node is in the Scene so the positions are accurate.
	/// See: NUnlockEpochScreen where we animate the Epochs on screen open!
	/// </summary>
	public virtual void Open()
	{
		NTimelineScreen.Instance.DisableInput();
		NTimelineScreen.Instance.CurrentUnlockScreen = this;
		_unlockConfirmButton?.Disable();
		_tween?.FastForwardToCompletion();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.5);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			_unlockConfirmButton?.Enable();
		}));
	}

	protected async Task Close()
	{
		Log.Info($"Closing: {base.Name}");
		if (NTimelineScreen.Instance.CurrentUnlockScreen == this)
		{
			NTimelineScreen.Instance.CurrentUnlockScreen = null;
		}
		_tween?.FastForwardToCompletion();
		OnScreenPreClose();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", StsColors.transparentBlack, 1.0);
		if (await _tween.AwaitFinished(this))
		{
			OnScreenClose();
			if (!NTimelineScreen.Instance.IsScreenQueued())
			{
				await NTimelineScreen.Instance.HideBackstopAndShowUi(showBackButton: true);
			}
			else
			{
				NTimelineScreen.Instance.OpenQueuedScreen();
			}
			this.QueueFreeSafely();
		}
	}

	protected virtual void OnScreenPreClose()
	{
	}

	protected virtual void OnScreenClose()
	{
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenPreClose, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenClose, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenPreClose && args.Count == 0)
		{
			OnScreenPreClose();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenClose && args.Count == 0)
		{
			OnScreenClose();
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
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.OnScreenPreClose)
		{
			return true;
		}
		if (method == MethodName.OnScreenClose)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._unlockConfirmButton)
		{
			_unlockConfirmButton = VariantUtils.ConvertTo<NUnlockConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._unlockConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _unlockConfirmButton);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlockConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._unlockConfirmButton, Variant.From(in _unlockConfirmButton));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._unlockConfirmButton, out var value))
		{
			_unlockConfirmButton = value.As<NUnlockConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
	}
}
