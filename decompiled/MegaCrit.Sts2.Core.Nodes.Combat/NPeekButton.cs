using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPeekButton.cs")]
public class NPeekButton : NButton
{
	[Signal]
	public delegate void ToggledEventHandler(NPeekButton peekButton);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public static readonly StringName OnOverlayStackChanged = "OnOverlayStackChanged";

		public static readonly StringName Wiggle = "Wiggle";

		public static readonly StringName AddTargets = "AddTargets";

		public static readonly StringName SetPeeking = "SetPeeking";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName OnCombatRoomReady = "OnCombatRoomReady";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName IsPeeking = "IsPeeking";

		public static readonly StringName CurrentCardMarker = "CurrentCardMarker";

		public static readonly StringName _flash = "_flash";

		public static readonly StringName _visuals = "_visuals";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _wiggleTween = "_wiggleTween";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Toggled = "Toggled";
	}

	private static readonly StringName _pulseStrength = new StringName("pulse_strength");

	private readonly List<Control> _targets = new List<Control>();

	private readonly List<Control> _hiddenTargets = new List<Control>();

	private TextureRect _flash;

	private Control _visuals;

	private IOverlayScreen? _overlayScreenParent;

	private Tween? _hoverTween;

	private Tween? _wiggleTween;

	private ToggledEventHandler backing_Toggled;

	protected override string[] Hotkeys => new string[1] { MegaInput.peek };

	public bool IsPeeking { get; private set; }

	public Marker2D CurrentCardMarker { get; private set; }

	public event ToggledEventHandler Toggled
	{
		add
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Combine(backing_Toggled, value);
		}
		remove
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Remove(backing_Toggled, value);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_flash = GetNode<TextureRect>("%Flash");
		_visuals = GetNode<Control>("%Visuals");
		CurrentCardMarker = GetNode<Marker2D>("%CurrentCardMarker");
		if (NCombatRoom.Instance != null)
		{
			if (NCombatRoom.Instance.IsNodeReady())
			{
				OnCombatRoomReady();
			}
			else
			{
				NCombatRoom.Instance.Connect(Node.SignalName.Ready, Callable.From(OnCombatRoomReady));
			}
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			base.Visible = false;
		}
		for (Node parent = GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is IOverlayScreen overlayScreenParent)
			{
				_overlayScreenParent = overlayScreenParent;
				NOverlayStack.Instance?.Connect(NOverlayStack.SignalName.Changed, Callable.From(OnOverlayStackChanged));
				NCapstoneContainer.Instance?.Connect(NCapstoneContainer.SignalName.Changed, Callable.From(OnOverlayStackChanged));
				break;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.Visible = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Visible = false;
	}

	private void OnOverlayStackChanged()
	{
		if (IsPeeking && _overlayScreenParent != null && NCapstoneContainer.Instance?.CurrentCapstoneScreen == null && _overlayScreenParent == NOverlayStack.Instance?.Peek())
		{
			NOverlayStack.Instance.HideBackstop();
		}
	}

	public void Wiggle()
	{
		_flash.Visible = true;
		TextureRect flash = _flash;
		Color modulate = _flash.Modulate;
		modulate.A = 0f;
		flash.Modulate = modulate;
		_wiggleTween?.Kill();
		_wiggleTween = CreateTween();
		_visuals.RotationDegrees = 0f;
		_wiggleTween.TweenMethod(Callable.From(delegate(float t)
		{
			_visuals.RotationDegrees = 10f * Mathf.Sin(t * 3f) * Mathf.Sin(t * 0.5f);
		}), 0f, (float)Math.PI * 2f, 0.5);
		_wiggleTween.Parallel().TweenMethod(Callable.From(delegate(float t)
		{
			_visuals.Scale = Vector2.One + Vector2.One * 0.15f * Mathf.Sin(t) * Mathf.Sin(t * 0.5f);
		}), 0f, (float)Math.PI, 0.25);
		_wiggleTween.Parallel().TweenProperty(_flash, "modulate:a", 1f, 0.1);
		_wiggleTween.Chain().TweenProperty(_flash, "modulate:a", 0f, 0.3).SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		NDebugAudioManager.Instance.Play("deny.mp3", 0.5f, PitchVariance.Medium);
	}

	public void AddTargets(params Control[] targets)
	{
		_targets.AddRange(targets);
	}

	public void SetPeeking(bool isPeeking)
	{
		if (IsPeeking == isPeeking)
		{
			return;
		}
		IsPeeking = isPeeking;
		if (NOverlayStack.Instance.ScreenCount > 0)
		{
			if (IsPeeking)
			{
				NOverlayStack.Instance.HideBackstop();
			}
			else
			{
				NOverlayStack.Instance.ShowBackstop();
			}
		}
		if (IsPeeking)
		{
			foreach (Control item in _targets.Where((Control t) => t.Visible))
			{
				_hiddenTargets.Add(item);
				item.Visible = false;
			}
		}
		else
		{
			foreach (Control hiddenTarget in _hiddenTargets)
			{
				hiddenTarget.Visible = true;
			}
			_hiddenTargets.Clear();
		}
		((ShaderMaterial)_visuals.Material).SetShaderParameter(_pulseStrength, IsPeeking ? 1 : 0);
		EmitSignal(SignalName.Toggled, this);
	}

	protected override void OnRelease()
	{
		SetPeeking(!IsPeeking);
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_visuals, "scale", Vector2.One, 0.15);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_visuals, "scale", Vector2.One * 0.95f, 0.05);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_visuals, "scale", Vector2.One * 1.1f, 0.05);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_visuals, "scale", Vector2.One, 0.15);
	}

	private void OnCombatRoomReady()
	{
		NCombatRoom.Instance.Ui.OnPeekButtonReady(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnOverlayStackChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Wiggle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Array, "targets", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetPeeking, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isPeeking", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCombatRoomReady, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnOverlayStackChanged && args.Count == 0)
		{
			OnOverlayStackChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Wiggle && args.Count == 0)
		{
			Wiggle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddTargets && args.Count == 1)
		{
			AddTargets(VariantUtils.ConvertToSystemArrayOfGodotObject<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPeeking && args.Count == 1)
		{
			SetPeeking(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
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
		if (method == MethodName.OnCombatRoomReady && args.Count == 0)
		{
			OnCombatRoomReady();
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
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.OnOverlayStackChanged)
		{
			return true;
		}
		if (method == MethodName.Wiggle)
		{
			return true;
		}
		if (method == MethodName.AddTargets)
		{
			return true;
		}
		if (method == MethodName.SetPeeking)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnPress)
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
		if (method == MethodName.OnCombatRoomReady)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsPeeking)
		{
			IsPeeking = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.CurrentCardMarker)
		{
			CurrentCardMarker = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName._flash)
		{
			_flash = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			_visuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._wiggleTween)
		{
			_wiggleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName.IsPeeking)
		{
			value = VariantUtils.CreateFrom<bool>(IsPeeking);
			return true;
		}
		if (name == PropertyName.CurrentCardMarker)
		{
			value = VariantUtils.CreateFrom<Marker2D>(CurrentCardMarker);
			return true;
		}
		if (name == PropertyName._flash)
		{
			value = VariantUtils.CreateFrom(in _flash);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			value = VariantUtils.CreateFrom(in _visuals);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._wiggleTween)
		{
			value = VariantUtils.CreateFrom(in _wiggleTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsPeeking, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._wiggleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CurrentCardMarker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsPeeking, Variant.From<bool>(IsPeeking));
		info.AddProperty(PropertyName.CurrentCardMarker, Variant.From<Marker2D>(CurrentCardMarker));
		info.AddProperty(PropertyName._flash, Variant.From(in _flash));
		info.AddProperty(PropertyName._visuals, Variant.From(in _visuals));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._wiggleTween, Variant.From(in _wiggleTween));
		info.AddSignalEventDelegate(SignalName.Toggled, backing_Toggled);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsPeeking, out var value))
		{
			IsPeeking = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.CurrentCardMarker, out var value2))
		{
			CurrentCardMarker = value2.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName._flash, out var value3))
		{
			_flash = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._visuals, out var value4))
		{
			_visuals = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value5))
		{
			_hoverTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._wiggleTween, out var value6))
		{
			_wiggleTween = value6.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ToggledEventHandler>(SignalName.Toggled, out var value7))
		{
			backing_Toggled = value7;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Toggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "peekButton", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalToggled(NPeekButton peekButton)
	{
		EmitSignal(SignalName.Toggled, peekButton);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Toggled && args.Count == 1)
		{
			backing_Toggled?.Invoke(VariantUtils.ConvertTo<NPeekButton>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Toggled)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
