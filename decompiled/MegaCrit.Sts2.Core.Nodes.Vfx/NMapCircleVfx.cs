using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NMapCircleVfx.cs")]
public class NMapCircleVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Create = "Create";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _image = "_image";

		public static readonly StringName _playAnim = "_playAnim";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private TextureRect _image;

	private const string _path = "res://scenes/vfx/map_circle_vfx.tscn";

	private static readonly string[] _textures = new string[5] { "res://images/atlases/compressed.sprites/map/map_circle_0.tres", "res://images/atlases/compressed.sprites/map/map_circle_1.tres", "res://images/atlases/compressed.sprites/map/map_circle_2.tres", "res://images/atlases/compressed.sprites/map/map_circle_3.tres", "res://images/atlases/compressed.sprites/map/map_circle_4.tres" };

	private const double _animInterval = 1.0 / 24.0;

	private bool _playAnim;

	public static IEnumerable<string> AssetPaths => _textures.Append("res://scenes/vfx/map_circle_vfx.tscn");

	public override void _Ready()
	{
		_image = GetNode<TextureRect>("TextureRect");
		_image.Texture = PreloadManager.Cache.GetTexture2D(_textures[0]);
		base.RotationDegrees = Rng.Chaotic.NextFloat(360f);
		Vector2 vector = Vector2.One * Rng.Chaotic.NextFloat(0.85f, 0.9f);
		if (_playAnim)
		{
			Tween tween = CreateTween().SetParallel();
			tween.TweenProperty(this, "scale", vector, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(vector * 2f);
			tween.TweenProperty(this, "modulate:a", 0.95f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(0f);
			TaskHelper.RunSafely(AnimateSprite());
		}
		else
		{
			base.Scale = vector;
			base.Modulate = new Color(base.Modulate, 0.95f);
			string path = _textures.Last();
			_image.Texture = PreloadManager.Cache.GetTexture2D(path);
		}
	}

	private async Task AnimateSprite()
	{
		string[] textures = _textures;
		foreach (string path in textures)
		{
			_image.Texture = PreloadManager.Cache.GetTexture2D(path);
			SceneTreeTimer source = GetTree().CreateTimer(1.0 / 24.0);
			await ToSignal(source, SceneTreeTimer.SignalName.Timeout);
		}
	}

	public static NMapCircleVfx? Create(bool playAnim)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NMapCircleVfx nMapCircleVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/map_circle_vfx.tscn").Instantiate<NMapCircleVfx>(PackedScene.GenEditState.Disabled);
		nMapCircleVfx._playAnim = playAnim;
		return nMapCircleVfx;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "playAnim", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMapCircleVfx>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMapCircleVfx>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._playAnim)
		{
			_playAnim = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._playAnim)
		{
			value = VariantUtils.CreateFrom(in _playAnim);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._playAnim, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._playAnim, Variant.From(in _playAnim));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._image, out var value))
		{
			_image = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._playAnim, out var value2))
		{
			_playAnim = value2.As<bool>();
		}
	}
}
