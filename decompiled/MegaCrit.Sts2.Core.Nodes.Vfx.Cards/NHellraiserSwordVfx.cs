using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NHellraiserSwordVfx.cs")]
public class NHellraiserSwordVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnTweenFinished = "OnTweenFinished";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _sword = "_sword";

		public static readonly StringName posY = "posY";

		public static readonly StringName targetColor = "targetColor";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _swordStr = new StringName("Sword");

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/cards/vfx_hellraiser/hellraiser_sword_vfx");

	private TextureRect _sword;

	public float posY;

	public Color targetColor;

	public static NHellraiserSwordVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NHellraiserSwordVfx>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		base.Name = _swordStr;
		_sword = GetNode<TextureRect>("TextureRect");
		_sword.FlipH = Rng.Chaotic.NextBool();
		_sword.RotationDegrees = Rng.Chaotic.NextFloat(-20f, 20f);
		_sword.Position = new Vector2(25f, 200f);
		base.Scale = new Vector2(Rng.Chaotic.NextFloat(0.7f, 0.9f), Rng.Chaotic.NextFloat(0.8f, 1.2f)) * Rng.Chaotic.NextFloat(1f, 2f);
		base.Position += new Vector2(Rng.Chaotic.NextGaussianFloat(0f, 1f, -500f, 500f), posY);
		Tween tween = CreateTween().SetParallel();
		tween.TweenInterval(Rng.Chaotic.NextDouble() * 0.8);
		tween.Chain();
		tween.TweenProperty(_sword, "position:y", 80f, 0.25).From(300f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Spring);
		tween.TweenProperty(this, "modulate", targetColor, 0.25).From(Colors.Red);
		tween.Chain().TweenInterval(0.25);
		tween.Chain();
		tween.TweenProperty(_sword, "position:y", 200f, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(this, "modulate:a", 0f, 0.5);
		tween.Chain().TweenCallback(Callable.From(OnTweenFinished));
	}

	private void OnTweenFinished()
	{
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTweenFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NHellraiserSwordVfx>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTweenFinished && args.Count == 0)
		{
			OnTweenFinished();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NHellraiserSwordVfx>(Create());
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
		if (method == MethodName.OnTweenFinished)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._sword)
		{
			_sword = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName.posY)
		{
			posY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName.targetColor)
		{
			targetColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._sword)
		{
			value = VariantUtils.CreateFrom(in _sword);
			return true;
		}
		if (name == PropertyName.posY)
		{
			value = VariantUtils.CreateFrom(in posY);
			return true;
		}
		if (name == PropertyName.targetColor)
		{
			value = VariantUtils.CreateFrom(in targetColor);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sword, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.posY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.targetColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._sword, Variant.From(in _sword));
		info.AddProperty(PropertyName.posY, Variant.From(in posY));
		info.AddProperty(PropertyName.targetColor, Variant.From(in targetColor));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._sword, out var value))
		{
			_sword = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName.posY, out var value2))
		{
			posY = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName.targetColor, out var value3))
		{
			targetColor = value3.As<Color>();
		}
	}
}
