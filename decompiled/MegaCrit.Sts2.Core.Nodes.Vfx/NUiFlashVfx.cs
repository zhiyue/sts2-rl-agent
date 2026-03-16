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

[ScriptPath("res://src/Core/Nodes/Vfx/NUiFlashVfx.cs")]
public class NUiFlashVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Create = "Create";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _textureRect = "_textureRect";

		public static readonly StringName _texture = "_texture";

		public static readonly StringName _modulate = "_modulate";

		public static readonly StringName _spriteTween = "_spriteTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/ui_flash_vfx.tscn";

	private TextureRect _textureRect;

	private Texture2D _texture;

	private Color _modulate;

	private Tween? _spriteTween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/ui_flash_vfx.tscn");

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_textureRect.Texture = _texture;
	}

	public async Task StartVfx()
	{
		TextureRect textureRect = _textureRect;
		Color modulate = _modulate;
		modulate.A = 0f;
		textureRect.Modulate = modulate;
		_textureRect.PivotOffset = _textureRect.Size * 0.5f;
		_spriteTween = CreateTween();
		_spriteTween.SetParallel();
		_spriteTween.TweenProperty(_textureRect, "scale", Vector2.One * 1.3f, 0.5);
		_spriteTween.TweenProperty(_textureRect, "modulate:a", 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_spriteTween.TweenProperty(_textureRect, "modulate:a", 0f, 0.25).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine)
			.SetDelay(0.3499999940395355);
		await ToSignal(_spriteTween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public static NUiFlashVfx? Create(Texture2D tex, Color modulate)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NUiFlashVfx nUiFlashVfx = (NUiFlashVfx)PreloadManager.Cache.GetScene("res://scenes/vfx/ui_flash_vfx.tscn").Instantiate(PackedScene.GenEditState.Disabled);
		nUiFlashVfx._texture = tex;
		nUiFlashVfx._modulate = modulate;
		return nUiFlashVfx;
	}

	public override void _ExitTree()
	{
		_spriteTween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tex", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false),
			new PropertyInfo(Variant.Type.Color, "modulate", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NUiFlashVfx>(Create(VariantUtils.ConvertTo<Texture2D>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
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
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NUiFlashVfx>(Create(VariantUtils.ConvertTo<Texture2D>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
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
		if (name == PropertyName._textureRect)
		{
			_textureRect = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._texture)
		{
			_texture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._modulate)
		{
			_modulate = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._spriteTween)
		{
			_spriteTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._textureRect)
		{
			value = VariantUtils.CreateFrom(in _textureRect);
			return true;
		}
		if (name == PropertyName._texture)
		{
			value = VariantUtils.CreateFrom(in _texture);
			return true;
		}
		if (name == PropertyName._modulate)
		{
			value = VariantUtils.CreateFrom(in _modulate);
			return true;
		}
		if (name == PropertyName._spriteTween)
		{
			value = VariantUtils.CreateFrom(in _spriteTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._texture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._modulate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spriteTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._textureRect, Variant.From(in _textureRect));
		info.AddProperty(PropertyName._texture, Variant.From(in _texture));
		info.AddProperty(PropertyName._modulate, Variant.From(in _modulate));
		info.AddProperty(PropertyName._spriteTween, Variant.From(in _spriteTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._textureRect, out var value))
		{
			_textureRect = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._texture, out var value2))
		{
			_texture = value2.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._modulate, out var value3))
		{
			_modulate = value3.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._spriteTween, out var value4))
		{
			_spriteTween = value4.As<Tween>();
		}
	}
}
