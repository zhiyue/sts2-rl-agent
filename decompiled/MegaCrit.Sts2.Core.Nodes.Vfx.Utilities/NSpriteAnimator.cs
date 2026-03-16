using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NSpriteAnimator.cs")]
public class NSpriteAnimator : Sprite2D
{
	public new class MethodName : Sprite2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Sprite2D.PropertyName
	{
		public static readonly StringName _frames = "_frames";

		public static readonly StringName _fps = "_fps";

		public static readonly StringName _loop = "_loop";

		public static readonly StringName _randomizeRotation = "_randomizeRotation";

		public static readonly StringName _rotationRange = "_rotationRange";
	}

	public new class SignalName : Sprite2D.SignalName
	{
	}

	[ExportGroup("Animation Settings", "")]
	[Export(PropertyHint.None, "")]
	private Texture2D[] _frames;

	[Export(PropertyHint.None, "")]
	private float _fps = 15f;

	[Export(PropertyHint.None, "")]
	private bool _loop;

	[ExportGroup("Rotation Settings", "")]
	[Export(PropertyHint.None, "")]
	private bool _randomizeRotation;

	[Export(PropertyHint.None, "")]
	private Vector2 _rotationRange;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	public override void _Ready()
	{
		if (_randomizeRotation)
		{
			base.RotationDegrees = new System.Random().Next((int)_rotationRange.X, (int)_rotationRange.Y);
		}
		TaskHelper.RunSafely(PlayAnimation());
	}

	public override void _ExitTree()
	{
		_cancelToken.Cancel();
	}

	private async Task PlayAnimation()
	{
		int i = 0;
		int interval = Mathf.RoundToInt(1000f / _fps);
		while (!_cancelToken.IsCancellationRequested)
		{
			base.Texture = _frames[i];
			i++;
			if (_loop)
			{
				i %= _frames.Length;
			}
			await Task.Delay(interval, _cancelToken.Token);
			if (_frames.Length <= i)
			{
				break;
			}
		}
		if (!_cancelToken.IsCancellationRequested)
		{
			this.QueueFreeSafely();
		}
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
		if (name == PropertyName._frames)
		{
			_frames = VariantUtils.ConvertToSystemArrayOfGodotObject<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._fps)
		{
			_fps = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._loop)
		{
			_loop = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._randomizeRotation)
		{
			_randomizeRotation = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._rotationRange)
		{
			_rotationRange = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._frames)
		{
			GodotObject[] frames = _frames;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(frames);
			return true;
		}
		if (name == PropertyName._fps)
		{
			value = VariantUtils.CreateFrom(in _fps);
			return true;
		}
		if (name == PropertyName._loop)
		{
			value = VariantUtils.CreateFrom(in _loop);
			return true;
		}
		if (name == PropertyName._randomizeRotation)
		{
			value = VariantUtils.CreateFrom(in _randomizeRotation);
			return true;
		}
		if (name == PropertyName._rotationRange)
		{
			value = VariantUtils.CreateFrom(in _rotationRange);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Nil, "Animation Settings", PropertyHint.None, "", PropertyUsageFlags.Group, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._frames, PropertyHint.TypeString, "24/17:Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._fps, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._loop, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Nil, "Rotation Settings", PropertyHint.None, "", PropertyUsageFlags.Group, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._randomizeRotation, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._rotationRange, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName frames = PropertyName._frames;
		GodotObject[] frames2 = _frames;
		info.AddProperty(frames, Variant.CreateFrom(frames2));
		info.AddProperty(PropertyName._fps, Variant.From(in _fps));
		info.AddProperty(PropertyName._loop, Variant.From(in _loop));
		info.AddProperty(PropertyName._randomizeRotation, Variant.From(in _randomizeRotation));
		info.AddProperty(PropertyName._rotationRange, Variant.From(in _rotationRange));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._frames, out var value))
		{
			_frames = value.AsGodotObjectArray<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._fps, out var value2))
		{
			_fps = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._loop, out var value3))
		{
			_loop = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._randomizeRotation, out var value4))
		{
			_randomizeRotation = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._rotationRange, out var value5))
		{
			_rotationRange = value5.As<Vector2>();
		}
	}
}
