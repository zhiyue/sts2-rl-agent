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
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'PointAtTarget' method.
		/// </summary>
		public static readonly StringName PointAtTarget = "PointAtTarget";

		/// <summary>
		/// Cached name for the 'StopPointing' method.
		/// </summary>
		public static readonly StringName StopPointing = "StopPointing";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_startPos' field.
		/// </summary>
		public static readonly StringName _startPos = "_startPos";

		/// <summary>
		/// Cached name for the '_targetPos' field.
		/// </summary>
		public static readonly StringName _targetPos = "_targetPos";

		/// <summary>
		/// Cached name for the '_targetNode' field.
		/// </summary>
		public static readonly StringName _targetNode = "_targetNode";

		/// <summary>
		/// Cached name for the '_targetOffset' field.
		/// </summary>
		public static readonly StringName _targetOffset = "_targetOffset";

		/// <summary>
		/// Cached name for the '_noise' field.
		/// </summary>
		public static readonly StringName _noise = "_noise";

		/// <summary>
		/// Cached name for the '_time' field.
		/// </summary>
		public static readonly StringName _time = "_time";

		/// <summary>
		/// Cached name for the '_rug' field.
		/// </summary>
		public static readonly StringName _rug = "_rug";

		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private Vector2 _startPos;

	private Vector2 _targetPos;

	private Control? _targetNode;

	private Vector2 _targetOffset;

	private MegaBone? _bone;

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
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			_bone = _animController.GetSkeleton()?.FindBone("rotate_me");
			animState.SetAnimation("default");
		});
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_stopPointingToken?.Cancel();
	}

	public override void _Process(double delta)
	{
		if (_targetNode != null && GodotObject.IsInstanceValid(_targetNode))
		{
			_targetPos = _targetNode.GlobalPosition + _targetOffset;
		}
		_time += (float)delta;
		float x = _noise.GetNoise1D(_time * 0.1f) + 0.4f;
		float y = _noise.GetNoise1D((_time + 0.25f) * 0.1f) - 0.5f;
		_parent.GlobalPosition = _parent.GlobalPosition.Lerp(_targetPos + new Vector2(x, y) * 100f, (float)delta * 4f);
		_bone?.SetRotation(Mathf.Lerp(-10f, 10f, (_parent.Position.X - _rug.Size.X * 0.5f - 50f) * 0.01f));
	}

	public void PointAtTarget(Control target, Vector2 offset)
	{
		_stopPointingToken?.Cancel();
		_targetNode = target;
		_targetOffset = offset;
	}

	public void StopPointing(float lingerTime)
	{
		_targetNode = null;
		_stopPointingToken?.Cancel();
		_stopPointingToken = new CancellationTokenSource();
		TaskHelper.RunSafely(WaitAndReturn(_stopPointingToken, lingerTime));
	}

	private async Task WaitAndReturn(CancellationTokenSource cancelToken, float lingerTime)
	{
		float num = 0f;
		while (num < lingerTime)
		{
			if (cancelToken.IsCancellationRequested || !this.IsValid() || !IsInsideTree())
			{
				return;
			}
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		_targetPos = _startPos;
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
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PointAtTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Vector2, "offset", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopPointing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "lingerTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.PointAtTarget && args.Count == 2)
		{
			PointAtTarget(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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
		if (name == PropertyName._targetNode)
		{
			_targetNode = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._targetOffset)
		{
			_targetOffset = VariantUtils.ConvertTo<Vector2>(in value);
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

	/// <inheritdoc />
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
		if (name == PropertyName._targetNode)
		{
			value = VariantUtils.CreateFrom(in _targetNode);
			return true;
		}
		if (name == PropertyName._targetOffset)
		{
			value = VariantUtils.CreateFrom(in _targetOffset);
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rug, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._startPos, Variant.From(in _startPos));
		info.AddProperty(PropertyName._targetPos, Variant.From(in _targetPos));
		info.AddProperty(PropertyName._targetNode, Variant.From(in _targetNode));
		info.AddProperty(PropertyName._targetOffset, Variant.From(in _targetOffset));
		info.AddProperty(PropertyName._noise, Variant.From(in _noise));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._rug, Variant.From(in _rug));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	/// <inheritdoc />
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
		if (info.TryGetProperty(PropertyName._targetNode, out var value3))
		{
			_targetNode = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._targetOffset, out var value4))
		{
			_targetOffset = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._noise, out var value5))
		{
			_noise = value5.As<FastNoiseLite>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value6))
		{
			_time = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rug, out var value7))
		{
			_rug = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value8))
		{
			_parent = value8.As<Node2D>();
		}
	}
}
