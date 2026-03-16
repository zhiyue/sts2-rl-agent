using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NHorizontalLinesVfx.cs")]
public class NHorizontalLinesVfx : GpuParticles2D
{
	public new class MethodName : GpuParticles2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : GpuParticles2D.PropertyName
	{
		public static readonly StringName _tween = "_tween";

		public static readonly StringName _duration = "_duration";

		public static readonly StringName _mat = "_mat";

		public static readonly StringName _isMovingRight = "_isMovingRight";
	}

	public new class SignalName : GpuParticles2D.SignalName
	{
	}

	private Tween? _tween;

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/whole_screen/horizontal_lines_vfx");

	private double _duration;

	private ParticleProcessMaterial _mat;

	private bool _isMovingRight;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NHorizontalLinesVfx? Create(Color color, double duration = 2.0, bool movingRightwards = true)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NHorizontalLinesVfx nHorizontalLinesVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NHorizontalLinesVfx>(PackedScene.GenEditState.Disabled);
		nHorizontalLinesVfx._duration = Mathf.Max(1.0, duration);
		nHorizontalLinesVfx._mat = (ParticleProcessMaterial)nHorizontalLinesVfx.ProcessMaterial;
		nHorizontalLinesVfx._mat.Color = color;
		nHorizontalLinesVfx._isMovingRight = movingRightwards;
		return nHorizontalLinesVfx;
	}

	public override void _Ready()
	{
		Control parent = GetParent<Control>();
		_mat.EmissionShapeOffset = new Vector3(-500f, parent.Size.Y * 0.5f, 0f);
		_mat.EmissionShapeScale = new Vector3(200f, parent.Size.Y * 0.5f, 1f);
		if (!_isMovingRight)
		{
			base.RotationDegrees = 180f;
			base.Position = new Vector2(parent.Size.X, parent.Size.Y);
		}
		TaskHelper.RunSafely(PlayAnim());
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task PlayAnim()
	{
		base.Modulate = StsColors.transparentWhite;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.45);
		_tween.Chain();
		_tween.TweenInterval(_duration - 0.9);
		_tween.Chain();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.45);
		await ToSignal(_tween, Tween.SignalName.Finished);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("GPUParticles2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "movingRightwards", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NHorizontalLinesVfx>(Create(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NHorizontalLinesVfx>(Create(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._duration)
		{
			_duration = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._mat)
		{
			_mat = VariantUtils.ConvertTo<ParticleProcessMaterial>(in value);
			return true;
		}
		if (name == PropertyName._isMovingRight)
		{
			_isMovingRight = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._mat)
		{
			value = VariantUtils.CreateFrom(in _mat);
			return true;
		}
		if (name == PropertyName._isMovingRight)
		{
			value = VariantUtils.CreateFrom(in _isMovingRight);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isMovingRight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._mat, Variant.From(in _mat));
		info.AddProperty(PropertyName._isMovingRight, Variant.From(in _isMovingRight));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._duration, out var value2))
		{
			_duration = value2.As<double>();
		}
		if (info.TryGetProperty(PropertyName._mat, out var value3))
		{
			_mat = value3.As<ParticleProcessMaterial>();
		}
		if (info.TryGetProperty(PropertyName._isMovingRight, out var value4))
		{
			_isMovingRight = value4.As<bool>();
		}
	}
}
