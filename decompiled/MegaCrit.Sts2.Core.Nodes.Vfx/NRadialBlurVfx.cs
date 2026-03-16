using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NRadialBlurVfx.cs")]
public class NRadialBlurVfx : BackBufferCopy
{
	public new class MethodName : BackBufferCopy.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Activate = "Activate";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : BackBufferCopy.PropertyName
	{
		public static readonly StringName _blurShader = "_blurShader";

		public static readonly StringName _vfxPosition = "_vfxPosition";

		public static readonly StringName _rect = "_rect";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : BackBufferCopy.SignalName
	{
	}

	private static readonly StringName _blurCenterx = new StringName("blur_center:x");

	private ShaderMaterial _blurShader;

	private VfxPosition _vfxPosition;

	private Control _rect;

	private Tween? _tween;

	public override void _Ready()
	{
		_rect = GetNode<Control>("Rect");
		_blurShader = (ShaderMaterial)_rect.GetMaterial();
	}

	public void Activate(VfxPosition vfxPosition = VfxPosition.Center)
	{
		if (!TestMode.IsOn && !base.Visible)
		{
			base.Visible = true;
			_rect.SetDeferred(Control.PropertyName.Size, GetViewportRect().Size);
			switch (vfxPosition)
			{
			case VfxPosition.Left:
				_blurShader.SetShaderParameter(_blurCenterx, 0.3f);
				break;
			case VfxPosition.Right:
				_blurShader.SetShaderParameter(_blurCenterx, 0.6f);
				break;
			default:
				_blurShader.SetShaderParameter(_blurCenterx, 0.45f);
				break;
			}
			TaskHelper.RunSafely(Animate());
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task Animate()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_blurShader, "shader_parameter/blur_power", 0.005f, 0.10000000149011612);
		_tween.Chain();
		_tween.TweenProperty(_blurShader, "shader_parameter/blur_power", 0f, 0.8999999761581421).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Cubic);
		await ToSignal(_tween, Tween.SignalName.Finished);
		base.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Activate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "vfxPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.Activate && args.Count == 1)
		{
			Activate(VariantUtils.ConvertTo<VfxPosition>(in args[0]));
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
		if (method == MethodName.Activate)
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
		if (name == PropertyName._blurShader)
		{
			_blurShader = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._vfxPosition)
		{
			_vfxPosition = VariantUtils.ConvertTo<VfxPosition>(in value);
			return true;
		}
		if (name == PropertyName._rect)
		{
			_rect = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._blurShader)
		{
			value = VariantUtils.CreateFrom(in _blurShader);
			return true;
		}
		if (name == PropertyName._vfxPosition)
		{
			value = VariantUtils.CreateFrom(in _vfxPosition);
			return true;
		}
		if (name == PropertyName._rect)
		{
			value = VariantUtils.CreateFrom(in _rect);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blurShader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._blurShader, Variant.From(in _blurShader));
		info.AddProperty(PropertyName._vfxPosition, Variant.From(in _vfxPosition));
		info.AddProperty(PropertyName._rect, Variant.From(in _rect));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._blurShader, out var value))
		{
			_blurShader = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._vfxPosition, out var value2))
		{
			_vfxPosition = value2.As<VfxPosition>();
		}
		if (info.TryGetProperty(PropertyName._rect, out var value3))
		{
			_rect = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
