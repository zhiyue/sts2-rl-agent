using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantHand.cs")]
public class NMerchantHand : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName PointAtTarget = "PointAtTarget";

		public static readonly StringName StopPointing = "StopPointing";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _startPos = "_startPos";

		public static readonly StringName _targetPos = "_targetPos";

		public static readonly StringName _noise = "_noise";

		public static readonly StringName _time = "_time";

		public static readonly StringName _rug = "_rug";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private Vector2 _startPos;

	private Vector2 _targetPos;

	private MegaBone _bone;

	private FastNoiseLite _noise;

	private float _time;

	private CancellationTokenSource? _stopPointingToken;

	private Control _rug;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_rug = _parent.GetParent<Control>();
		_startPos = _parent.GlobalPosition;
		_targetPos = _startPos;
		_noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_noise.Frequency = 1f;
		_bone = _animController.GetSkeleton().FindBone("rotate_me");
		_animController.GetAnimationState().SetAnimation("default");
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_stopPointingToken?.Cancel();
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		float x = _noise.GetNoise1D(_time * 0.1f) + 0.4f;
		float y = _noise.GetNoise1D((_time + 0.25f) * 0.1f) - 0.5f;
		_parent.GlobalPosition = _parent.GlobalPosition.Lerp(_targetPos + new Vector2(x, y) * 100f, (float)delta * 4f);
		_bone.SetRotation(Mathf.Lerp(-10f, 10f, (_parent.Position.X - _rug.Size.X * 0.5f - 50f) * 0.01f));
	}

	public void PointAtTarget(Vector2 pos)
	{
		_stopPointingToken?.Cancel();
		_targetPos = pos - Vector2.One * 50f;
	}

	public void StopPointing(float lingerTime)
	{
		_stopPointingToken?.Cancel();
		_stopPointingToken = new CancellationTokenSource();
		TaskHelper.RunSafely(WaitAndReturn(_stopPointingToken, lingerTime));
	}

	private async Task WaitAndReturn(CancellationTokenSource cancelToken, float lingerTime)
	{
		for (float timer = 0f; timer < lingerTime; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested || !this.IsValid() || !IsInsideTree())
			{
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_targetPos = _startPos;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PointAtTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopPointing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "lingerTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PointAtTarget && args.Count == 1)
		{
			PointAtTarget(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopPointing && args.Count == 1)
		{
			StopPointing(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.PointAtTarget)
		{
			return true;
		}
		if (method == MethodName.StopPointing)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._startPos)
		{
			_startPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetPos)
		{
			_targetPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._noise)
		{
			_noise = VariantUtils.ConvertTo<FastNoiseLite>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rug)
		{
			_rug = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._startPos)
		{
			value = VariantUtils.CreateFrom(in _startPos);
			return true;
		}
		if (name == PropertyName._targetPos)
		{
			value = VariantUtils.CreateFrom(in _targetPos);
			return true;
		}
		if (name == PropertyName._noise)
		{
			value = VariantUtils.CreateFrom(in _noise);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		if (name == PropertyName._rug)
		{
			value = VariantUtils.CreateFrom(in _rug);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rug, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._startPos, Variant.From(in _startPos));
		info.AddProperty(PropertyName._targetPos, Variant.From(in _targetPos));
		info.AddProperty(PropertyName._noise, Variant.From(in _noise));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._rug, Variant.From(in _rug));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._startPos, out var value))
		{
			_startPos = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetPos, out var value2))
		{
			_targetPos = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._noise, out var value3))
		{
			_noise = value3.As<FastNoiseLite>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value4))
		{
			_time = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rug, out var value5))
		{
			_rug = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
	}
}
