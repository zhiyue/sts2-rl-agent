using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardFlyShuffleVfx.cs")]
public class NCardFlyShuffleVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _vfx = "_vfx";

		public static readonly StringName _fadeOutTween = "_fadeOutTween";

		public static readonly StringName _vfxFading = "_vfxFading";

		public static readonly StringName _startPos = "_startPos";

		public static readonly StringName _endPos = "_endPos";

		public static readonly StringName _controlPointOffset = "_controlPointOffset";

		public static readonly StringName _duration = "_duration";

		public static readonly StringName _speed = "_speed";

		public static readonly StringName _accel = "_accel";

		public static readonly StringName _arcDir = "_arcDir";

		public static readonly StringName _trailPath = "_trailPath";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NCardTrailVfx? _vfx;

	private Tween? _fadeOutTween;

	private bool _vfxFading;

	private Vector2 _startPos;

	private Vector2 _endPos;

	private float _controlPointOffset;

	private float _duration;

	private float _speed;

	private float _accel;

	private float _arcDir;

	private string _trailPath;

	private CardPile _targetPile;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_card_shuffle_fly");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NCardFlyShuffleVfx? Create(CardPile startPile, CardPile targetPile, string trailPath)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardFlyShuffleVfx nCardFlyShuffleVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCardFlyShuffleVfx>(PackedScene.GenEditState.Disabled);
		nCardFlyShuffleVfx._startPos = startPile.Type.GetTargetPosition(null);
		nCardFlyShuffleVfx._endPos = targetPile.Type.GetTargetPosition(null);
		nCardFlyShuffleVfx._trailPath = trailPath;
		nCardFlyShuffleVfx._targetPile = targetPile;
		return nCardFlyShuffleVfx;
	}

	public override void _Ready()
	{
		_controlPointOffset = Rng.Chaotic.NextFloat(-300f, 400f);
		_speed = Rng.Chaotic.NextFloat(1.1f, 1.25f);
		_accel = Rng.Chaotic.NextFloat(2f, 2.5f);
		_arcDir = ((_endPos.Y < 540f) ? (-500f) : (500f + _controlPointOffset));
		_duration = Rng.Chaotic.NextFloat(1f, 1.75f);
		_vfx = NCardTrailVfx.Create(this, _trailPath);
		if (_vfx != null)
		{
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(_vfx);
		}
		Node parent = GetParent();
		parent.MoveChild(this, parent.GetChildCount() - 1);
		TaskHelper.RunSafely(PlayAnim());
	}

	private async Task PlayAnim()
	{
		float time = 0f;
		while (time / _duration <= 1f)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (_cancelToken.IsCancellationRequested)
			{
				return;
			}
			float num = (float)GetProcessDeltaTime();
			time += _speed * num;
			_speed += _accel * num;
			Vector2 c = _startPos + (_endPos - _startPos) * 0.5f;
			c.Y -= _arcDir;
			base.GlobalPosition = MathHelper.BezierCurve(_startPos, _endPos, c, time / _duration);
			Vector2 vector = MathHelper.BezierCurve(_startPos, _endPos, c, (time + 0.05f) / _duration);
			base.Rotation = (vector - base.GlobalPosition).Angle() + (float)Math.PI / 2f;
		}
		base.GlobalPosition = _endPos;
		_targetPile.InvokeCardAddFinished();
		time = 0f;
		while (time / _duration <= 1f)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (_cancelToken.IsCancellationRequested)
			{
				return;
			}
			float num2 = (float)GetProcessDeltaTime();
			time += _speed * num2;
			if (time / _duration > 0.25f && !_vfxFading)
			{
				if (_vfx != null)
				{
					TaskHelper.RunSafely(_vfx.FadeOut());
				}
				_vfxFading = true;
			}
			base.Scale = Vector2.One * Mathf.Max(Mathf.Lerp(0.1f, -0.1f, time / _duration), 0f);
		}
		_fadeOutTween = CreateTween();
		_fadeOutTween.TweenProperty(this, "modulate:a", 0f, 0.800000011920929);
		await Task.Delay(800);
		this.QueueFreeSafely();
	}

	public override void _ExitTree()
	{
		_fadeOutTween?.Kill();
		_cancelToken.Cancel();
		_cancelToken.Dispose();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._vfx)
		{
			_vfx = VariantUtils.ConvertTo<NCardTrailVfx>(in value);
			return true;
		}
		if (name == PropertyName._fadeOutTween)
		{
			_fadeOutTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._vfxFading)
		{
			_vfxFading = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._startPos)
		{
			_startPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._endPos)
		{
			_endPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._controlPointOffset)
		{
			_controlPointOffset = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._duration)
		{
			_duration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._speed)
		{
			_speed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._accel)
		{
			_accel = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._arcDir)
		{
			_arcDir = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._trailPath)
		{
			_trailPath = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._vfx)
		{
			value = VariantUtils.CreateFrom(in _vfx);
			return true;
		}
		if (name == PropertyName._fadeOutTween)
		{
			value = VariantUtils.CreateFrom(in _fadeOutTween);
			return true;
		}
		if (name == PropertyName._vfxFading)
		{
			value = VariantUtils.CreateFrom(in _vfxFading);
			return true;
		}
		if (name == PropertyName._startPos)
		{
			value = VariantUtils.CreateFrom(in _startPos);
			return true;
		}
		if (name == PropertyName._endPos)
		{
			value = VariantUtils.CreateFrom(in _endPos);
			return true;
		}
		if (name == PropertyName._controlPointOffset)
		{
			value = VariantUtils.CreateFrom(in _controlPointOffset);
			return true;
		}
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._speed)
		{
			value = VariantUtils.CreateFrom(in _speed);
			return true;
		}
		if (name == PropertyName._accel)
		{
			value = VariantUtils.CreateFrom(in _accel);
			return true;
		}
		if (name == PropertyName._arcDir)
		{
			value = VariantUtils.CreateFrom(in _arcDir);
			return true;
		}
		if (name == PropertyName._trailPath)
		{
			value = VariantUtils.CreateFrom(in _trailPath);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._vfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeOutTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._vfxFading, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._endPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._controlPointOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._accel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._arcDir, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._trailPath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._vfx, Variant.From(in _vfx));
		info.AddProperty(PropertyName._fadeOutTween, Variant.From(in _fadeOutTween));
		info.AddProperty(PropertyName._vfxFading, Variant.From(in _vfxFading));
		info.AddProperty(PropertyName._startPos, Variant.From(in _startPos));
		info.AddProperty(PropertyName._endPos, Variant.From(in _endPos));
		info.AddProperty(PropertyName._controlPointOffset, Variant.From(in _controlPointOffset));
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._speed, Variant.From(in _speed));
		info.AddProperty(PropertyName._accel, Variant.From(in _accel));
		info.AddProperty(PropertyName._arcDir, Variant.From(in _arcDir));
		info.AddProperty(PropertyName._trailPath, Variant.From(in _trailPath));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._vfx, out var value))
		{
			_vfx = value.As<NCardTrailVfx>();
		}
		if (info.TryGetProperty(PropertyName._fadeOutTween, out var value2))
		{
			_fadeOutTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._vfxFading, out var value3))
		{
			_vfxFading = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._startPos, out var value4))
		{
			_startPos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._endPos, out var value5))
		{
			_endPos = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._controlPointOffset, out var value6))
		{
			_controlPointOffset = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._duration, out var value7))
		{
			_duration = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._speed, out var value8))
		{
			_speed = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._accel, out var value9))
		{
			_accel = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._arcDir, out var value10))
		{
			_arcDir = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._trailPath, out var value11))
		{
			_trailPath = value11.As<string>();
		}
	}
}
