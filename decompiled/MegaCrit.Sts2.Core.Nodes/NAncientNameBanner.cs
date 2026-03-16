using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.RichTextTags;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NAncientNameBanner.cs")]
public class NAncientNameBanner : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName UpdateTransform = "UpdateTransform";

		public static readonly StringName UpdateGlyphSpace = "UpdateGlyphSpace";

		public static readonly StringName GetTextCenterGlyphIndex = "GetTextCenterGlyphIndex";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _ancientBannerEffect = "_ancientBannerEffect";

		public static readonly StringName _epithetLabel = "_epithetLabel";

		public static readonly StringName _moveTween = "_moveTween";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaRichTextLabel _titleLabel;

	private RichTextAncientBanner _ancientBannerEffect;

	private MegaLabel _epithetLabel;

	private static readonly string _path = SceneHelper.GetScenePath("ui/ancient_name_banner");

	private AncientEventModel _ancient;

	private Tween? _moveTween;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_path);

	public static NAncientNameBanner? Create(AncientEventModel ancient)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NAncientNameBanner nAncientNameBanner = PreloadManager.Cache.GetScene(_path).Instantiate<NAncientNameBanner>(PackedScene.GenEditState.Disabled);
		nAncientNameBanner._ancient = ancient;
		return nAncientNameBanner;
	}

	public override void _Ready()
	{
		_titleLabel = GetNode<MegaRichTextLabel>("%Title");
		string text = _ancient.Title.GetFormattedText().ToUpper();
		_ancientBannerEffect = new RichTextAncientBanner();
		_ancientBannerEffect.CenterCharacter = GetTextCenterGlyphIndex(text, _titleLabel.GetThemeFont(ThemeConstants.RichTextLabel.normalFont, "RichTextLabel"), _titleLabel.GetThemeFontSize(ThemeConstants.RichTextLabel.normalFontSize, "RichTextLabel"));
		_titleLabel.InstallEffect(_ancientBannerEffect);
		_titleLabel.BbcodeEnabled = true;
		_titleLabel.Text = "[ancient_banner]" + text + "[/ancient_banner]";
		_epithetLabel = GetNode<MegaLabel>("%Epithet");
		_epithetLabel.SetTextAutoSize(_ancient.Epithet.GetFormattedText());
		TaskHelper.RunSafely(AnimateVfx());
	}

	private async Task AnimateVfx()
	{
		_epithetLabel.Position = new Vector2(0f, 18f);
		_epithetLabel.Modulate = Colors.Transparent;
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "position:y", -100f, 4.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
		_tween = CreateTween().SetParallel();
		_tween.TweenMethod(Callable.From<float>(UpdateGlyphSpace), 1f, 0f, 3.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateTransform), 0f, 1f, 3.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring);
		_tween.TweenProperty(_epithetLabel, "position:y", 42f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(1.0);
		_tween.TweenProperty(_epithetLabel, "modulate:a", 1f, 1.0).SetDelay(1.5);
		_tween.Chain();
		_tween.TweenInterval(1.5);
		_tween.Chain();
		_tween.TweenProperty(_titleLabel, "modulate", Colors.Red, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_titleLabel, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_epithetLabel, "modulate", Colors.Red, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_epithetLabel, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		await ToSignal(_tween, Tween.SignalName.Finished);
		_moveTween.Kill();
		_moveTween = CreateTween().SetParallel();
		base.Position = new Vector2(0f, -80f);
		_titleLabel.Modulate = Colors.White;
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
		_titleLabel.VerticalAlignment = VerticalAlignment.Bottom;
		_titleLabel.Position = Vector2.Zero;
		_titleLabel.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.normalFontSize, 54);
		_titleLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.fontOutlineColor, Colors.Transparent);
		_titleLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.fontShadowColor, Colors.Transparent);
		_titleLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor, StsColors.cream);
		_epithetLabel.HorizontalAlignment = HorizontalAlignment.Left;
		_epithetLabel.VerticalAlignment = VerticalAlignment.Bottom;
		_epithetLabel.Modulate = new Color(1f, 1f, 1f, 0f);
		_epithetLabel.AddThemeFontSizeOverride(ThemeConstants.Label.fontSize, 18);
		_epithetLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, Colors.Transparent);
		_epithetLabel.AddThemeColorOverride(ThemeConstants.Label.fontShadowColor, Colors.Transparent);
		_epithetLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, StsColors.cream);
		_moveTween.TweenProperty(_epithetLabel, "modulate:a", 0.5f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
		_moveTween.TweenProperty(this, "position:x", 48f, 2.0).SetEase(Tween.EaseType.Out).From(0)
			.SetTrans(Tween.TransitionType.Circ);
	}

	private void UpdateTransform(float obj)
	{
		_ancientBannerEffect.Rotation = obj;
	}

	private void UpdateGlyphSpace(float spacing)
	{
		_ancientBannerEffect.Spacing = spacing * 1000f;
	}

	private float GetTextCenterGlyphIndex(string text, Font font, int fontSize)
	{
		using TextParagraph textParagraph = new TextParagraph();
		textParagraph.AddString(text, font, fontSize);
		float num = 0f;
		TextServer primaryInterface = TextServerManager.Singleton.GetPrimaryInterface();
		Array<Dictionary> array = primaryInterface.ShapedTextGetGlyphs(textParagraph.GetLineRid(0));
		foreach (Dictionary item in array)
		{
			float num2 = item.GetValueOrDefault("advance").AsSingle();
			num += num2;
		}
		float num3 = 0f;
		int num4 = 0;
		foreach (Dictionary item2 in array)
		{
			float num5 = item2.GetValueOrDefault("advance").AsSingle();
			num3 += num5;
			if (num3 > num * 0.5f)
			{
				return (float)num4 + (num * 0.5f - (num3 - num5)) / num5;
			}
			num4++;
		}
		return 0f;
	}

	public override void _ExitTree()
	{
		_moveTween?.Kill();
		_tween?.Kill();
		_titleLabel.RemoveThemeFontSizeOverride(ThemeConstants.RichTextLabel.normalFontSize);
		_titleLabel.RemoveThemeColorOverride(ThemeConstants.RichTextLabel.fontOutlineColor);
		_titleLabel.RemoveThemeColorOverride(ThemeConstants.RichTextLabel.fontShadowColor);
		_titleLabel.RemoveThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor);
		_epithetLabel.RemoveThemeFontSizeOverride(ThemeConstants.Label.fontSize);
		_epithetLabel.RemoveThemeColorOverride(ThemeConstants.Label.fontOutlineColor);
		_epithetLabel.RemoveThemeColorOverride(ThemeConstants.Label.fontShadowColor);
		_epithetLabel.RemoveThemeColorOverride(ThemeConstants.Label.fontColor);
		base._ExitTree();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTransform, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateGlyphSpace, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "spacing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetTextCenterGlyphIndex, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Object, "font", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Font"), exported: false),
			new PropertyInfo(Variant.Type.Int, "fontSize", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.UpdateTransform && args.Count == 1)
		{
			UpdateTransform(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateGlyphSpace && args.Count == 1)
		{
			UpdateGlyphSpace(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetTextCenterGlyphIndex && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<float>(GetTextCenterGlyphIndex(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Font>(in args[1]), VariantUtils.ConvertTo<int>(in args[2])));
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
		if (method == MethodName.UpdateTransform)
		{
			return true;
		}
		if (method == MethodName.UpdateGlyphSpace)
		{
			return true;
		}
		if (method == MethodName.GetTextCenterGlyphIndex)
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
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._ancientBannerEffect)
		{
			_ancientBannerEffect = VariantUtils.ConvertTo<RichTextAncientBanner>(in value);
			return true;
		}
		if (name == PropertyName._epithetLabel)
		{
			_epithetLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			_moveTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
		if (name == PropertyName._ancientBannerEffect)
		{
			value = VariantUtils.CreateFrom(in _ancientBannerEffect);
			return true;
		}
		if (name == PropertyName._epithetLabel)
		{
			value = VariantUtils.CreateFrom(in _epithetLabel);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			value = VariantUtils.CreateFrom(in _moveTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientBannerEffect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epithetLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._ancientBannerEffect, Variant.From(in _ancientBannerEffect));
		info.AddProperty(PropertyName._epithetLabel, Variant.From(in _epithetLabel));
		info.AddProperty(PropertyName._moveTween, Variant.From(in _moveTween));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._titleLabel, out var value))
		{
			_titleLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._ancientBannerEffect, out var value2))
		{
			_ancientBannerEffect = value2.As<RichTextAncientBanner>();
		}
		if (info.TryGetProperty(PropertyName._epithetLabel, out var value3))
		{
			_epithetLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._moveTween, out var value4))
		{
			_moveTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
	}
}
