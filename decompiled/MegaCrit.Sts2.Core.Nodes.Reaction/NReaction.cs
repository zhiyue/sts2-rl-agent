using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Reaction;

[ScriptPath("res://src/Core/Nodes/Reaction/NReaction.cs")]
public class NReaction : TextureRect
{
	public new class MethodName : TextureRect.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName BeginAnim = "BeginAnim";

		public static readonly StringName TypeToTexture = "TypeToTexture";

		public static readonly StringName TextureToType = "TextureToType";
	}

	public new class PropertyName : TextureRect.PropertyName
	{
		public static readonly StringName Type = "Type";
	}

	public new class SignalName : TextureRect.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/reaction");

	private const string _exclamationPath = "res://images/ui/emote/exclaim.png";

	private const string _skullPath = "res://images/ui/emote/skull.png";

	private const string _thumbDownPath = "res://images/ui/emote/thumb_down.png";

	private const string _sadSlimePath = "res://images/ui/emote/slime_sad.png";

	private const string _questionMarkPath = "res://images/ui/emote/question.png";

	private const string _heartPath = "res://images/ui/emote/heart.png";

	private const string _thumbUpPath = "res://images/ui/emote/thumb_up.png";

	private const string _happyCultistPath = "res://images/ui/emote/happy_cultist.png";

	public ReactionType Type => TextureToType(base.Texture);

	public static NReaction Create(Texture2D reactionTexture)
	{
		NReaction nReaction = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NReaction>(PackedScene.GenEditState.Disabled);
		nReaction.Texture = reactionTexture;
		return nReaction;
	}

	public static NReaction Create(ReactionType type)
	{
		return Create(TypeToTexture(type));
	}

	public void BeginAnim()
	{
		TaskHelper.RunSafely(DoAnim());
	}

	private async Task DoAnim()
	{
		NReaction nReaction = this;
		Color modulate = base.Modulate;
		modulate.A = 0f;
		nReaction.Modulate = modulate;
		float num = Rng.Chaotic.NextFloat(40f, 60f);
		float deg = Rng.Chaotic.NextFloat(-30f, 30f);
		Vector2 vector = base.Position + Vector2.Up.Rotated(Mathf.DegToRad(deg)) * num;
		Tween tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty(this, "position", vector, 0.30000001192092896).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(this, "modulate:a", 1f, 0.30000001192092896).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.SetParallel(parallel: false);
		tween.TweenProperty(this, "modulate:a", 0f, 0.20000000298023224).SetDelay(0.6000000238418579).SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Expo);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	private static Texture2D TypeToTexture(ReactionType type)
	{
		AssetCache cache = PreloadManager.Cache;
		return cache.GetTexture2D(type switch
		{
			ReactionType.Exclamation => "res://images/ui/emote/exclaim.png", 
			ReactionType.Skull => "res://images/ui/emote/skull.png", 
			ReactionType.ThumbDown => "res://images/ui/emote/thumb_down.png", 
			ReactionType.SadSlime => "res://images/ui/emote/slime_sad.png", 
			ReactionType.QuestionMark => "res://images/ui/emote/question.png", 
			ReactionType.Heart => "res://images/ui/emote/heart.png", 
			ReactionType.ThumbUp => "res://images/ui/emote/thumb_up.png", 
			ReactionType.HappyCultist => "res://images/ui/emote/happy_cultist.png", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		});
	}

	private static ReactionType TextureToType(Texture2D texture)
	{
		return texture.ResourcePath switch
		{
			"res://images/ui/emote/exclaim.png" => ReactionType.Exclamation, 
			"res://images/ui/emote/skull.png" => ReactionType.Skull, 
			"res://images/ui/emote/thumb_down.png" => ReactionType.ThumbDown, 
			"res://images/ui/emote/slime_sad.png" => ReactionType.SadSlime, 
			"res://images/ui/emote/question.png" => ReactionType.QuestionMark, 
			"res://images/ui/emote/heart.png" => ReactionType.Heart, 
			"res://images/ui/emote/thumb_up.png" => ReactionType.ThumbUp, 
			"res://images/ui/emote/happy_cultist.png" => ReactionType.HappyCultist, 
			_ => throw new ArgumentOutOfRangeException("texture", texture, null), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("TextureRect"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "reactionTexture", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.BeginAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TypeToTexture, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TextureToType, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "texture", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NReaction>(Create(VariantUtils.ConvertTo<Texture2D>(in args[0])));
			return true;
		}
		if (method == MethodName.BeginAnim && args.Count == 0)
		{
			BeginAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TypeToTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(TypeToTexture(VariantUtils.ConvertTo<ReactionType>(in args[0])));
			return true;
		}
		if (method == MethodName.TextureToType && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ReactionType>(TextureToType(VariantUtils.ConvertTo<Texture2D>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NReaction>(Create(VariantUtils.ConvertTo<Texture2D>(in args[0])));
			return true;
		}
		if (method == MethodName.TypeToTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(TypeToTexture(VariantUtils.ConvertTo<ReactionType>(in args[0])));
			return true;
		}
		if (method == MethodName.TextureToType && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ReactionType>(TextureToType(VariantUtils.ConvertTo<Texture2D>(in args[0])));
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
		if (method == MethodName.BeginAnim)
		{
			return true;
		}
		if (method == MethodName.TypeToTexture)
		{
			return true;
		}
		if (method == MethodName.TextureToType)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Type)
		{
			value = VariantUtils.CreateFrom<ReactionType>(Type);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Type, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
