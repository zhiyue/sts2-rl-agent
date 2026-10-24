using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NCardHighlight.cs")]
public class NCardHighlight : TextureRect
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : TextureRect.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'AnimShow' method.
		/// </summary>
		public static readonly StringName AnimShow = "AnimShow";

		/// <summary>
		/// Cached name for the 'AnimHide' method.
		/// </summary>
		public static readonly StringName AnimHide = "AnimHide";

		/// <summary>
		/// Cached name for the 'AnimHideInstantly' method.
		/// </summary>
		public static readonly StringName AnimHideInstantly = "AnimHideInstantly";

		/// <summary>
		/// Cached name for the 'AnimFlash' method.
		/// </summary>
		public static readonly StringName AnimFlash = "AnimFlash";

		/// <summary>
		/// Cached name for the 'GetShaderParameter' method.
		/// </summary>
		public static readonly StringName GetShaderParameter = "GetShaderParameter";

		/// <summary>
		/// Cached name for the 'SetShaderParameter' method.
		/// </summary>
		public static readonly StringName SetShaderParameter = "SetShaderParameter";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : TextureRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_curTween' field.
		/// </summary>
		public static readonly StringName _curTween = "_curTween";

		/// <summary>
		/// Cached name for the '_shaderMaterial' field.
		/// </summary>
		public static readonly StringName _shaderMaterial = "_shaderMaterial";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : TextureRect.SignalName
	{
	}

	private static readonly StringName _shaderParameterWidth = new StringName("width");

	public static readonly Color playableColor = new Color(0f, 0.957f, 0.988f, 0.98f);

	public static readonly Color gold = new Color(1f, 0.784f, 0f, 0.98f);

	public static readonly Color red = new Color(0.83f, 0f, 0.33f, 0.98f);

	private Tween? _curTween;

	private ShaderMaterial _shaderMaterial;

	public override void _Ready()
	{
		_shaderMaterial = (ShaderMaterial)base.Material;
	}

	public void AnimShow()
	{
		_curTween?.Kill();
		_curTween = CreateTween();
		_curTween.TweenMethod(Callable.From<float>(SetShaderParameter), GetShaderParameter(), 0.075f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void AnimHide()
	{
		_curTween?.Kill();
		_curTween = CreateTween();
		_curTween.TweenMethod(Callable.From<float>(SetShaderParameter), GetShaderParameter(), 0.0, 0.5);
	}

	public void AnimHideInstantly()
	{
		_curTween?.Kill();
		SetShaderParameter(0f);
	}

	public void AnimFlash()
	{
		_curTween?.Kill();
		_curTween = CreateTween();
		_curTween.TweenMethod(Callable.From<float>(SetShaderParameter), GetShaderParameter(), 0.15f, 0.1);
		_curTween.TweenMethod(Callable.From<float>(SetShaderParameter), 0.15f, 0.075f, 0.35).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	private float GetShaderParameter()
	{
		return _shaderMaterial.GetShaderParameter(_shaderParameterWidth).AsSingle();
	}

	private void SetShaderParameter(float val)
	{
		_shaderMaterial.SetShaderParameter(_shaderParameterWidth, val);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHideInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimFlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetShaderParameter, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetShaderParameter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "val", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.AnimShow && args.Count == 0)
		{
			AnimShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHide && args.Count == 0)
		{
			AnimHide();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHideInstantly && args.Count == 0)
		{
			AnimHideInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimFlash && args.Count == 0)
		{
			AnimFlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetShaderParameter && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetShaderParameter());
			return true;
		}
		if (method == MethodName.SetShaderParameter && args.Count == 1)
		{
			SetShaderParameter(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.AnimShow)
		{
			return true;
		}
		if (method == MethodName.AnimHide)
		{
			return true;
		}
		if (method == MethodName.AnimHideInstantly)
		{
			return true;
		}
		if (method == MethodName.AnimFlash)
		{
			return true;
		}
		if (method == MethodName.GetShaderParameter)
		{
			return true;
		}
		if (method == MethodName.SetShaderParameter)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._curTween)
		{
			_curTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._shaderMaterial)
		{
			_shaderMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._curTween)
		{
			value = VariantUtils.CreateFrom(in _curTween);
			return true;
		}
		if (name == PropertyName._shaderMaterial)
		{
			value = VariantUtils.CreateFrom(in _shaderMaterial);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._curTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shaderMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._curTween, Variant.From(in _curTween));
		info.AddProperty(PropertyName._shaderMaterial, Variant.From(in _shaderMaterial));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._curTween, out var value))
		{
			_curTween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._shaderMaterial, out var value2))
		{
			_shaderMaterial = value2.As<ShaderMaterial>();
		}
	}
}
