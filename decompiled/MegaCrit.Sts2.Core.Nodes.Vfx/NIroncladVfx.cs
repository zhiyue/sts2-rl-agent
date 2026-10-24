using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NIroncladVfx.cs")]
public class NIroncladVfx : Node
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
		/// Cached name for the 'OnAnimationEvent' method.
		/// </summary>
		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		/// <summary>
		/// Cached name for the 'OnHeavySlash' method.
		/// </summary>
		public static readonly StringName OnHeavySlash = "OnHeavySlash";

		/// <summary>
		/// Cached name for the 'OnAttackSlash' method.
		/// </summary>
		public static readonly StringName OnAttackSlash = "OnAttackSlash";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_slashStepBase' field.
		/// </summary>
		public static readonly StringName _slashStepBase = "_slashStepBase";

		/// <summary>
		/// Cached name for the '_slashShaderMat' field.
		/// </summary>
		public static readonly StringName _slashShaderMat = "_slashShaderMat";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

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

	private static readonly StringName _step = new StringName("step");

	private Vector2 _slashStepBase;

	private ShaderMaterial? _slashShaderMat;

	private Tween _tween;

	private Node2D _parent;

	private MegaSprite _megaSprite;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_slashShaderMat = new MegaSlotNode(_parent.GetNode("SlashVfxSlot")).GetNormalMaterial() as ShaderMaterial;
		_slashStepBase = (Vector2)_slashShaderMat.GetShaderParameter(_step);
		_megaSprite = new MegaSprite(_parent);
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "heavy_slash_start"))
		{
			if (eventName == "attack_slash_start")
			{
				OnAttackSlash();
			}
		}
		else
		{
			OnHeavySlash();
		}
	}

	private void OnHeavySlash()
	{
		_slashShaderMat?.SetShaderParameter(_step, _slashStepBase);
		_tween?.Kill();
		_tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Cubic);
		Vector2 vector = new Vector2(1f, 1.02f);
		_tween.TweenProperty(_slashShaderMat, "shader_parameter/step", vector, 0.3499999940395355);
	}

	private void OnAttackSlash()
	{
		_slashShaderMat?.SetShaderParameter(_step, _slashStepBase);
		_tween?.Kill();
		_tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		Vector2 vector = new Vector2(1f, 1.02f);
		_tween.TweenInterval(0.15000000596046448);
		_tween.TweenProperty(_slashShaderMat, "shader_parameter/step", vector, 0.20000000298023224);
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
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
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHeavySlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAttackSlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHeavySlash && args.Count == 0)
		{
			OnHeavySlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAttackSlash && args.Count == 0)
		{
			OnAttackSlash();
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.OnHeavySlash)
		{
			return true;
		}
		if (method == MethodName.OnAttackSlash)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._slashStepBase)
		{
			_slashStepBase = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._slashShaderMat)
		{
			_slashShaderMat = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._slashStepBase)
		{
			value = VariantUtils.CreateFrom(in _slashStepBase);
			return true;
		}
		if (name == PropertyName._slashShaderMat)
		{
			value = VariantUtils.CreateFrom(in _slashShaderMat);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._slashStepBase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashShaderMat, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._slashStepBase, Variant.From(in _slashStepBase));
		info.AddProperty(PropertyName._slashShaderMat, Variant.From(in _slashShaderMat));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._slashStepBase, out var value))
		{
			_slashStepBase = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._slashShaderMat, out var value2))
		{
			_slashShaderMat = value2.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value4))
		{
			_parent = value4.As<Node2D>();
		}
	}
}
