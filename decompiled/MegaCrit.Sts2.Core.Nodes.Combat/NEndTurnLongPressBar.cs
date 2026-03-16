using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NEndTurnLongPressBar.cs")]
public class NEndTurnLongPressBar : ColorRect
{
	public new class MethodName : ColorRect.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Init = "Init";

		public static readonly StringName StartPress = "StartPress";

		public static readonly StringName CancelPress = "CancelPress";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName RecalculateBar = "RecalculateBar";
	}

	public new class PropertyName : ColorRect.PropertyName
	{
		public static readonly StringName _outline = "_outline";

		public static readonly StringName _pressTimer = "_pressTimer";

		public static readonly StringName _isPressed = "_isPressed";

		public static readonly StringName _endTurnButton = "_endTurnButton";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _enabled = "_enabled";
	}

	public new class SignalName : ColorRect.SignalName
	{
	}

	private Control _outline;

	private double _pressTimer;

	private const double _longPressDuration = 0.5;

	private bool _isPressed;

	private const float _targetWidth = 204f;

	private NEndTurnButton _endTurnButton;

	private Tween? _tween;

	private bool _enabled = true;

	public override void _Ready()
	{
		_outline = GetNode<Control>("%BarOutline");
	}

	public void Init(NEndTurnButton endTurnButton)
	{
		_endTurnButton = endTurnButton;
	}

	public void StartPress()
	{
		_isPressed = true;
	}

	public void CancelPress()
	{
		_isPressed = false;
	}

	public override void _Process(double delta)
	{
		if (!_enabled)
		{
			return;
		}
		if (_isPressed)
		{
			_pressTimer += delta;
			if (_pressTimer > 0.5)
			{
				_enabled = false;
				base.Size = new Vector2(204f, 6f);
				_pressTimer = 0.0;
				_endTurnButton.CallReleaseLogic();
				TaskHelper.RunSafely(PlayAnim());
			}
			else
			{
				RecalculateBar();
			}
		}
		else if (_pressTimer > 0.0)
		{
			_pressTimer -= delta;
			if (_pressTimer < 0.0)
			{
				_pressTimer = 0.0;
				Color modulate = base.Modulate;
				modulate.A = 0f;
				base.Modulate = modulate;
			}
			else
			{
				RecalculateBar();
			}
		}
	}

	private void RecalculateBar()
	{
		float num = (float)(_pressTimer / 0.5);
		base.Size = new Vector2(num * 204f, 6f);
		base.Color = new Color(num * 2.5f, 0.6f + num, 0.6f);
		Color modulate = base.Modulate;
		modulate.A = Ease.CubicOut(num * 0.75f);
		base.Modulate = modulate;
	}

	private async Task PlayAnim()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.25);
		await ToSignal(_tween, Tween.SignalName.Finished);
		base.Color = new Color(1f, 0.85f, 0.36f);
		_isPressed = false;
		_enabled = true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "endTurnButton", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RecalculateBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Init && args.Count == 1)
		{
			Init(VariantUtils.ConvertTo<NEndTurnButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartPress && args.Count == 0)
		{
			StartPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelPress && args.Count == 0)
		{
			CancelPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RecalculateBar && args.Count == 0)
		{
			RecalculateBar();
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
		if (method == MethodName.Init)
		{
			return true;
		}
		if (method == MethodName.StartPress)
		{
			return true;
		}
		if (method == MethodName.CancelPress)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.RecalculateBar)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._pressTimer)
		{
			_pressTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			_isPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._endTurnButton)
		{
			_endTurnButton = VariantUtils.ConvertTo<NEndTurnButton>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._enabled)
		{
			_enabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._pressTimer)
		{
			value = VariantUtils.CreateFrom(in _pressTimer);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			value = VariantUtils.CreateFrom(in _isPressed);
			return true;
		}
		if (name == PropertyName._endTurnButton)
		{
			value = VariantUtils.CreateFrom(in _endTurnButton);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._enabled)
		{
			value = VariantUtils.CreateFrom(in _enabled);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._pressTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._endTurnButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._enabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._pressTimer, Variant.From(in _pressTimer));
		info.AddProperty(PropertyName._isPressed, Variant.From(in _isPressed));
		info.AddProperty(PropertyName._endTurnButton, Variant.From(in _endTurnButton));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._enabled, Variant.From(in _enabled));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outline, out var value))
		{
			_outline = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._pressTimer, out var value2))
		{
			_pressTimer = value2.As<double>();
		}
		if (info.TryGetProperty(PropertyName._isPressed, out var value3))
		{
			_isPressed = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._endTurnButton, out var value4))
		{
			_endTurnButton = value4.As<NEndTurnButton>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._enabled, out var value6))
		{
			_enabled = value6.As<bool>();
		}
	}
}
