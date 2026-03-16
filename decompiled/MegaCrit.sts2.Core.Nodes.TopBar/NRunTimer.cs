using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NRunTimer.cs")]
public class NRunTimer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName DeferredInit = "DeferredInit";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName RefreshVisibility = "RefreshVisibility";

		public static readonly StringName ToggleTimer = "ToggleTimer";

		public static readonly StringName OnTimerTimeout = "OnTimerTimeout";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _timerLabel = "_timerLabel";

		public static readonly StringName _timer = "_timer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaLabel _timerLabel;

	private Timer _timer;

	public override void _Ready()
	{
		_timerLabel = GetNode<MegaLabel>("TimerLabel");
		ToggleTimer(on: false);
		CallDeferred("DeferredInit");
	}

	private void DeferredInit()
	{
		NMapScreen.Instance.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(RefreshVisibility));
		NCapstoneContainer.Instance.Connect(NCapstoneContainer.SignalName.Changed, Callable.From(RefreshVisibility));
		_timer = new Timer();
		_timer.WaitTime = 1.0;
		_timer.Autostart = false;
		_timer.Connect(Timer.SignalName.Timeout, Callable.From(OnTimerTimeout));
		this.AddChildSafely(_timer);
		_timer.Start();
	}

	public override void _ExitTree()
	{
		_timer.Stop();
	}

	public void RefreshVisibility()
	{
		if (SaveManager.Instance.PrefsSave.ShowRunTimer)
		{
			ToggleTimer(on: true);
		}
		else
		{
			ToggleTimer(NCapstoneContainer.Instance.InUse || NMapScreen.Instance.Visible);
		}
	}

	private void ToggleTimer(bool on)
	{
		base.Visible = on;
	}

	private void OnTimerTimeout()
	{
		if (!RunManager.Instance.IsGameOver)
		{
			_timerLabel.SetTextAutoSize(TimeFormatting.Format(RunManager.Instance.RunTime));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DeferredInit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleTimer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "on", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnTimerTimeout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DeferredInit && args.Count == 0)
		{
			DeferredInit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshVisibility && args.Count == 0)
		{
			RefreshVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleTimer && args.Count == 1)
		{
			ToggleTimer(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTimerTimeout && args.Count == 0)
		{
			OnTimerTimeout();
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
		if (method == MethodName.DeferredInit)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.RefreshVisibility)
		{
			return true;
		}
		if (method == MethodName.ToggleTimer)
		{
			return true;
		}
		if (method == MethodName.OnTimerTimeout)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._timerLabel)
		{
			_timerLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<Timer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._timerLabel)
		{
			value = VariantUtils.CreateFrom(in _timerLabel);
			return true;
		}
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._timerLabel, Variant.From(in _timerLabel));
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._timerLabel, out var value))
		{
			_timerLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._timer, out var value2))
		{
			_timer = value2.As<Timer>();
		}
	}
}
