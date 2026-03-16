using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardFlyVfx.cs")]
public class NCardFlyVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnCardExitedTree = "OnCardExitedTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _card = "_card";

		public static readonly StringName _trailPath = "_trailPath";

		public static readonly StringName _vfx = "_vfx";

		public static readonly StringName _fadeOutTween = "_fadeOutTween";

		public static readonly StringName _vfxFading = "_vfxFading";

		public static readonly StringName _isAddingToPile = "_isAddingToPile";

		public static readonly StringName _startPos = "_startPos";

		public static readonly StringName _endPos = "_endPos";

		public static readonly StringName _controlPointOffset = "_controlPointOffset";

		public static readonly StringName _duration = "_duration";

		public static readonly StringName _speed = "_speed";

		public static readonly StringName _accel = "_accel";

		public static readonly StringName _arcDir = "_arcDir";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private NCard _card;

	private string _trailPath;

	private NCardTrailVfx? _vfx;

	private Tween? _fadeOutTween;

	private bool _vfxFading;

	private bool _isAddingToPile;

	private Vector2 _startPos;

	private Vector2 _endPos;

	private float _controlPointOffset;

	private float _duration;

	private float _speed;

	private float _accel;

	private float _arcDir;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_card_fly");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public TaskCompletionSource? SwooshAwayCompletion { get; private set; }

	public static NCardFlyVfx? Create(NCard card, Vector2 end, bool isAddingToPile, string trailPath)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardFlyVfx nCardFlyVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCardFlyVfx>(PackedScene.GenEditState.Disabled);
		nCardFlyVfx._startPos = card.GlobalPosition;
		nCardFlyVfx._endPos = end;
		nCardFlyVfx._card = card;
		nCardFlyVfx._isAddingToPile = isAddingToPile;
		nCardFlyVfx._trailPath = trailPath;
		return nCardFlyVfx;
	}

	public override void _Ready()
	{
		_vfx = NCardTrailVfx.Create(_card, _trailPath);
		if (_vfx != null)
		{
			GetParent().AddChildSafely(_vfx);
		}
		_controlPointOffset = Rng.Chaotic.NextFloat(100f, 400f);
		_speed = Rng.Chaotic.NextFloat(1.1f, 1.25f);
		_accel = Rng.Chaotic.NextFloat(2f, 2.5f);
		_arcDir = ((_endPos.Y < GetViewportRect().Size.Y * 0.5f) ? (-500f) : (500f + _controlPointOffset));
		_duration = Rng.Chaotic.NextFloat(1f, 1.75f);
		_card.Connect(Node.SignalName.TreeExited, Callable.From(OnCardExitedTree));
		if (NCombatUi.IsDebugHidingPlayContainer)
		{
			_card.Modulate = Colors.Transparent;
			_card.Visible = false;
			base.Visible = false;
		}
		TaskHelper.RunSafely(PlayAnim());
	}

	public override void _ExitTree()
	{
		_fadeOutTween?.Kill();
		_cancelToken.Cancel();
		_cancelToken.Dispose();
	}

	private void OnCardExitedTree()
	{
		try
		{
			_vfx?.QueueFreeSafely();
		}
		catch (ObjectDisposedException)
		{
		}
		SwooshAwayCompletion?.TrySetResult();
		this.QueueFreeSafely();
	}

	private async Task PlayAnim()
	{
		SwooshAwayCompletion = new TaskCompletionSource();
		float time = 0f;
		while (time / _duration <= 1f)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (_cancelToken.IsCancellationRequested)
			{
				SwooshAwayCompletion?.SetResult();
				return;
			}
			float num = (float)GetProcessDeltaTime();
			time += _speed * num;
			_speed += _accel * num;
			Vector2 c = _startPos + (_endPos - _startPos) * 0.5f;
			c.Y -= _arcDir;
			Vector2 vector = MathHelper.BezierCurve(_startPos, _endPos, c, (time + 0.05f) / _duration);
			_card.GlobalPosition = MathHelper.BezierCurve(_startPos, _endPos, c, time / _duration);
			float num2 = (vector - _card.GlobalPosition).Angle() + (float)Math.PI / 2f;
			Node parent = _card.GetParent();
			if (parent is Control control)
			{
				num2 -= control.Rotation;
			}
			else if (parent is Node2D node2D)
			{
				num2 -= node2D.Rotation;
			}
			_card.Rotation = Mathf.LerpAngle(_card.Rotation, num2, num * 12f);
			_card.Body.Modulate = Colors.White.Lerp(Colors.Black, Mathf.Clamp(time * 3f / _duration, 0f, 1f));
			_card.Body.Scale = Vector2.One * Mathf.Lerp(1f, 0.1f, Mathf.Clamp(time * 3f / _duration, 0f, 1f));
		}
		_card.GlobalPosition = _endPos;
		if (_isAddingToPile)
		{
			_card.Model.Pile?.InvokeCardAddFinished();
		}
		time = 0f;
		while (time / _duration <= 1f)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (_cancelToken.IsCancellationRequested)
			{
				SwooshAwayCompletion?.SetResult();
				return;
			}
			float num3 = (float)GetProcessDeltaTime();
			time += _speed * num3;
			if (time / _duration > 0.25f && !_vfxFading)
			{
				if (_vfx != null)
				{
					TaskHelper.RunSafely(_vfx.FadeOut());
				}
				_vfxFading = true;
			}
			_card.Body.Scale = Vector2.One * Mathf.Max(Mathf.Lerp(0.1f, -0.15f, time / _duration), 0f);
		}
		SwooshAwayCompletion?.SetResult();
		_card.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Vector2, "end", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "isAddingToPile", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "trailPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardExitedTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NCardFlyVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<string>(in args[3])));
			return true;
		}
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
		if (method == MethodName.OnCardExitedTree && args.Count == 0)
		{
			OnCardExitedTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NCardFlyVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<string>(in args[3])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnCardExitedTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._card)
		{
			_card = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._trailPath)
		{
			_trailPath = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
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
		if (name == PropertyName._isAddingToPile)
		{
			_isAddingToPile = VariantUtils.ConvertTo<bool>(in value);
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
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._card)
		{
			value = VariantUtils.CreateFrom(in _card);
			return true;
		}
		if (name == PropertyName._trailPath)
		{
			value = VariantUtils.CreateFrom(in _trailPath);
			return true;
		}
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
		if (name == PropertyName._isAddingToPile)
		{
			value = VariantUtils.CreateFrom(in _isAddingToPile);
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
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._card, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._trailPath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._vfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeOutTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._vfxFading, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAddingToPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._endPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._controlPointOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._accel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._arcDir, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._card, Variant.From(in _card));
		info.AddProperty(PropertyName._trailPath, Variant.From(in _trailPath));
		info.AddProperty(PropertyName._vfx, Variant.From(in _vfx));
		info.AddProperty(PropertyName._fadeOutTween, Variant.From(in _fadeOutTween));
		info.AddProperty(PropertyName._vfxFading, Variant.From(in _vfxFading));
		info.AddProperty(PropertyName._isAddingToPile, Variant.From(in _isAddingToPile));
		info.AddProperty(PropertyName._startPos, Variant.From(in _startPos));
		info.AddProperty(PropertyName._endPos, Variant.From(in _endPos));
		info.AddProperty(PropertyName._controlPointOffset, Variant.From(in _controlPointOffset));
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._speed, Variant.From(in _speed));
		info.AddProperty(PropertyName._accel, Variant.From(in _accel));
		info.AddProperty(PropertyName._arcDir, Variant.From(in _arcDir));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._card, out var value))
		{
			_card = value.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._trailPath, out var value2))
		{
			_trailPath = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._vfx, out var value3))
		{
			_vfx = value3.As<NCardTrailVfx>();
		}
		if (info.TryGetProperty(PropertyName._fadeOutTween, out var value4))
		{
			_fadeOutTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._vfxFading, out var value5))
		{
			_vfxFading = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isAddingToPile, out var value6))
		{
			_isAddingToPile = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._startPos, out var value7))
		{
			_startPos = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._endPos, out var value8))
		{
			_endPos = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._controlPointOffset, out var value9))
		{
			_controlPointOffset = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._duration, out var value10))
		{
			_duration = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._speed, out var value11))
		{
			_speed = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._accel, out var value12))
		{
			_accel = value12.As<float>();
		}
		if (info.TryGetProperty(PropertyName._arcDir, out var value13))
		{
			_arcDir = value13.As<float>();
		}
	}
}
