using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NBgGroundSpikeVfx.cs")]
public class NBgGroundSpikeVfx : Sprite2D
{
	public new class MethodName : Sprite2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetColor = "SetColor";

		public static readonly StringName AdjustStartPosition = "AdjustStartPosition";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Sprite2D.PropertyName
	{
		public static readonly StringName _startPosition = "_startPosition";

		public static readonly StringName _movingRight = "_movingRight";

		public static readonly StringName _vfxColor = "_vfxColor";

		public static readonly StringName _velocity = "_velocity";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Sprite2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/bg_ground_spike_vfx.tscn";

	protected Vector2 _startPosition;

	protected bool _movingRight = true;

	protected VfxColor _vfxColor;

	private Vector2 _velocity;

	private Tween? _tween;

	public static NBgGroundSpikeVfx? Create(Vector2 position, bool movingRight = true, VfxColor vfxColor = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBgGroundSpikeVfx nBgGroundSpikeVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/bg_ground_spike_vfx.tscn").Instantiate<NBgGroundSpikeVfx>(PackedScene.GenEditState.Disabled);
		nBgGroundSpikeVfx._startPosition = position;
		nBgGroundSpikeVfx._movingRight = movingRight;
		nBgGroundSpikeVfx._vfxColor = vfxColor;
		return nBgGroundSpikeVfx;
	}

	public override void _Ready()
	{
		base.Skew = (_movingRight ? (Rng.Chaotic.NextFloat(15f, 30f) * 0.0174533f) : (Rng.Chaotic.NextFloat(-30f, -15f) * 0.0174533f));
		float num = Rng.Chaotic.NextFloat(0.5f, 1.5f);
		base.Scale = new Vector2(Rng.Chaotic.NextFloat(0.8f, 1.2f), Rng.Chaotic.NextFloat(0.8f, 2f)) * num;
		AdjustStartPosition();
		base.GlobalPosition = _startPosition;
		_velocity = new Vector2(_movingRight ? Rng.Chaotic.NextFloat(50f, 250f) : Rng.Chaotic.NextFloat(-250f, -50f), Rng.Chaotic.NextFloat(-5f, 5f)) / num;
		SetColor();
		TaskHelper.RunSafely(Animate());
	}

	private void SetColor()
	{
		switch (_vfxColor)
		{
		case VfxColor.Red:
			base.Modulate = new Color(1f, Rng.Chaotic.NextFloat(0.2f, 0.8f), Rng.Chaotic.NextFloat(0f, 0.2f), 0.5f);
			break;
		case VfxColor.Purple:
			base.Modulate = new Color(Rng.Chaotic.NextFloat(0f, 0.2f), Rng.Chaotic.NextFloat(0.2f, 0.8f), 1f, 0.5f);
			break;
		case VfxColor.White:
		{
			float num3 = Rng.Chaotic.NextFloat(0.2f, 0.8f);
			base.Modulate = new Color(num3, num3, num3, 0.5f);
			break;
		}
		case VfxColor.Cyan:
		{
			float num2 = Rng.Chaotic.NextFloat(0.6f, 1f);
			base.Modulate = new Color(0.2f, num2, num2, 0.5f);
			break;
		}
		case VfxColor.Gold:
		{
			float num = Rng.Chaotic.NextFloat(0.6f, 1f);
			base.Modulate = new Color(num, num, 0.2f);
			break;
		}
		default:
			Log.Error("Color: " + _vfxColor.ToString() + " not implemented");
			throw new ArgumentOutOfRangeException();
		}
	}

	protected virtual void AdjustStartPosition()
	{
		_startPosition += new Vector2(_movingRight ? Rng.Chaotic.NextFloat(40f, 160f) : Rng.Chaotic.NextFloat(-160f, -40f), Rng.Chaotic.NextFloat(-96f, -10f));
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task Animate()
	{
		float num = Rng.Chaotic.NextFloat(0.25f, 1f);
		_tween = CreateTween().SetParallel();
		_tween.TweenInterval(Rng.Chaotic.NextFloat(0.01f, 0.2f));
		_tween.Chain();
		_tween.TweenProperty(this, "skew", _movingRight ? (Rng.Chaotic.NextFloat(30f, 60f) * 0.0174533f) : (Rng.Chaotic.NextFloat(-60f, -30f) * 0.0174533f), num).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "scale", base.Scale * Rng.Chaotic.NextFloat(0.1f, 0.5f), num).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "modulate:a", 0f, num).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public override void _Process(double delta)
	{
		base.Position += _velocity * (float)delta;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Sprite2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "movingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "vfxColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AdjustStartPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NBgGroundSpikeVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<VfxColor>(in args[2])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetColor && args.Count == 0)
		{
			SetColor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AdjustStartPosition && args.Count == 0)
		{
			AdjustStartPosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NBgGroundSpikeVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<VfxColor>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetColor)
		{
			return true;
		}
		if (method == MethodName.AdjustStartPosition)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._startPosition)
		{
			_startPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._movingRight)
		{
			_movingRight = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			_vfxColor = VariantUtils.ConvertTo<VfxColor>(in value);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			_velocity = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._startPosition)
		{
			value = VariantUtils.CreateFrom(in _startPosition);
			return true;
		}
		if (name == PropertyName._movingRight)
		{
			value = VariantUtils.CreateFrom(in _movingRight);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			value = VariantUtils.CreateFrom(in _vfxColor);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			value = VariantUtils.CreateFrom(in _velocity);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._movingRight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._velocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._startPosition, Variant.From(in _startPosition));
		info.AddProperty(PropertyName._movingRight, Variant.From(in _movingRight));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
		info.AddProperty(PropertyName._velocity, Variant.From(in _velocity));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._startPosition, out var value))
		{
			_startPosition = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._movingRight, out var value2))
		{
			_movingRight = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value3))
		{
			_vfxColor = value3.As<VfxColor>();
		}
		if (info.TryGetProperty(PropertyName._velocity, out var value4))
		{
			_velocity = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
	}
}
