using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NBolasVfx.cs")]
public class NBolasVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName FollowCurve = "FollowCurve";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _bola2 = "_bola2";

		public static readonly StringName _bola3 = "_bola3";

		public static readonly StringName _startPosition = "_startPosition";

		public static readonly StringName _controlPosition = "_controlPosition";

		public static readonly StringName _endPosition = "_endPosition";

		public static readonly StringName _rotationSpeed = "_rotationSpeed";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/cards/bolas_vfx");

	private Node2D _bola2;

	private Node2D _bola3;

	private Vector2 _startPosition;

	private Vector2 _controlPosition;

	private Vector2 _endPosition;

	private float _rotationSpeed = 30f;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NBolasVfx? Create(Creature owner, Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (target.IsDead)
		{
			return null;
		}
		NBolasVfx nBolasVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBolasVfx>(PackedScene.GenEditState.Disabled);
		nBolasVfx.GlobalPosition = NCombatRoom.Instance.GetCreatureNode(owner).VfxSpawnPosition;
		nBolasVfx._startPosition = nBolasVfx.GlobalPosition;
		nBolasVfx._endPosition = NCombatRoom.Instance.GetCreatureNode(target).VfxSpawnPosition;
		float y = Mathf.Min(nBolasVfx._startPosition.Y, nBolasVfx._endPosition.Y) - Rng.Chaotic.NextFloat(400f, 500f);
		nBolasVfx._controlPosition = new Vector2((nBolasVfx._startPosition.X + nBolasVfx._endPosition.X) * 0.5f, y);
		return nBolasVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(FlyBolasFly());
		_bola2 = GetNode<Node2D>("Bola2");
		_bola3 = GetNode<Node2D>("Bola3");
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task FlyBolasFly()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.25).From(0f);
		_tween.TweenMethod(Callable.From<float>(FollowCurve), 0f, 1f, 0.6000000238418579).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_tween.Chain();
		_tween.TweenInterval(0.15000000596046448);
		_tween.TweenProperty(this, "modulate:a", 0f, 0.15000000596046448);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	private void FollowCurve(float progressPercent)
	{
		base.GlobalPosition = MathHelper.BezierCurve(_startPosition, _endPosition, _controlPosition, progressPercent);
	}

	public override void _Process(double delta)
	{
		float num = (float)delta;
		base.Rotation += num * (0f - _rotationSpeed);
		_rotationSpeed -= num * 12f;
		_bola2.Position -= new Vector2(150f * num, 0f);
		_bola3.Position -= new Vector2(-150f * num, 0f);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FollowCurve, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "progressPercent", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.FollowCurve && args.Count == 1)
		{
			FollowCurve(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.FollowCurve)
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
		if (name == PropertyName._bola2)
		{
			_bola2 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._bola3)
		{
			_bola3 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._startPosition)
		{
			_startPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._controlPosition)
		{
			_controlPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._endPosition)
		{
			_endPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._rotationSpeed)
		{
			_rotationSpeed = VariantUtils.ConvertTo<float>(in value);
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
		if (name == PropertyName._bola2)
		{
			value = VariantUtils.CreateFrom(in _bola2);
			return true;
		}
		if (name == PropertyName._bola3)
		{
			value = VariantUtils.CreateFrom(in _bola3);
			return true;
		}
		if (name == PropertyName._startPosition)
		{
			value = VariantUtils.CreateFrom(in _startPosition);
			return true;
		}
		if (name == PropertyName._controlPosition)
		{
			value = VariantUtils.CreateFrom(in _controlPosition);
			return true;
		}
		if (name == PropertyName._endPosition)
		{
			value = VariantUtils.CreateFrom(in _endPosition);
			return true;
		}
		if (name == PropertyName._rotationSpeed)
		{
			value = VariantUtils.CreateFrom(in _rotationSpeed);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bola2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bola3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._controlPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._endPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rotationSpeed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bola2, Variant.From(in _bola2));
		info.AddProperty(PropertyName._bola3, Variant.From(in _bola3));
		info.AddProperty(PropertyName._startPosition, Variant.From(in _startPosition));
		info.AddProperty(PropertyName._controlPosition, Variant.From(in _controlPosition));
		info.AddProperty(PropertyName._endPosition, Variant.From(in _endPosition));
		info.AddProperty(PropertyName._rotationSpeed, Variant.From(in _rotationSpeed));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bola2, out var value))
		{
			_bola2 = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._bola3, out var value2))
		{
			_bola3 = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._startPosition, out var value3))
		{
			_startPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._controlPosition, out var value4))
		{
			_controlPosition = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._endPosition, out var value5))
		{
			_endPosition = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._rotationSpeed, out var value6))
		{
			_rotationSpeed = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value7))
		{
			_tween = value7.As<Tween>();
		}
	}
}
