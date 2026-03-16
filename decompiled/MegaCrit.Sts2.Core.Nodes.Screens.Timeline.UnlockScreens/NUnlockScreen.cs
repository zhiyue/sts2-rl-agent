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

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockScreen.cs")]
public abstract class NUnlockScreen : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName Open = "Open";

		public static readonly StringName OnScreenPreClose = "OnScreenPreClose";

		public static readonly StringName OnScreenClose = "OnScreenClose";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _unlockConfirmButton = "_unlockConfirmButton";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NUnlockConfirmButton? _unlockConfirmButton;

	private Tween? _tween;

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

	public virtual void Open()
	{
		NTimelineScreen.Instance.DisableInput();
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
		_tween?.FastForwardToCompletion();
		OnScreenPreClose();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", StsColors.transparentBlack, 1.0);
		await ToSignal(_tween, Tween.SignalName.Finished);
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

	protected virtual void OnScreenPreClose()
	{
	}

	protected virtual void OnScreenClose()
	{
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlockConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._unlockConfirmButton, Variant.From(in _unlockConfirmButton));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

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
