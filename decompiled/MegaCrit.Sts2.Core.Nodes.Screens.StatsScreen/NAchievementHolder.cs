using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NAchievementHolder.cs")]
public class NAchievementHolder : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshUnlocked = "RefreshUnlocked";

		public static readonly StringName SetLockVisuals = "SetLockVisuals";

		public static readonly StringName SetDateLabel = "SetDateLabel";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsUnlocked = "IsUnlocked";

		public static readonly StringName _border = "_border";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _lock = "_lock";

		public static readonly StringName _iconHsv = "_iconHsv";

		public static readonly StringName _borderHsv = "_borderHsv";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _date = "_date";

		public static readonly StringName _achievement = "_achievement";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private const string _scenePath = "screens/stats_screen/achievement_holder";

	private TextureRect _border;

	private TextureRect _icon;

	private TextureRect _lock;

	private ShaderMaterial _iconHsv;

	private ShaderMaterial _borderHsv;

	private MegaRichTextLabel _infoLabel;

	private MegaLabel _date;

	private Achievement _achievement;

	private Tween? _tween;

	public bool IsUnlocked { get; private set; }

	public static NAchievementHolder? Create(Achievement achievement)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NAchievementHolder nAchievementHolder = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("screens/stats_screen/achievement_holder")).Instantiate<NAchievementHolder>(PackedScene.GenEditState.Disabled);
		nAchievementHolder._achievement = achievement;
		nAchievementHolder.IsUnlocked = AchievementsUtil.IsUnlocked(achievement);
		return nAchievementHolder;
	}

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_border = GetNode<TextureRect>("%Border");
		_lock = GetNode<TextureRect>("%Lock");
		_borderHsv = (ShaderMaterial)_border.Material;
		_iconHsv = (ShaderMaterial)_icon.Material;
		_infoLabel = GetNode<MegaRichTextLabel>("%InfoText");
		_date = GetNode<MegaLabel>("%DateText");
		RefreshUnlocked();
	}

	public static string GetPathForAchievement(Enum achievement)
	{
		string text = StringHelper.SnakeCase(achievement.ToString()).ToLowerInvariant();
		return ImageHelper.GetImagePath("packed/achievements/unlocked/" + text + ".png");
	}

	public void RefreshUnlocked()
	{
		IsUnlocked = AchievementsUtil.IsUnlocked(_achievement);
		string text = StringHelper.SnakeCase(_achievement.ToString()).ToLowerInvariant();
		_icon.Texture = PreloadManager.Cache.GetTexture2D(GetPathForAchievement(_achievement));
		text = text.ToUpperInvariant();
		if (IsUnlocked)
		{
			_infoLabel.Text = "[b][gold]" + new LocString("achievements", text + ".title").GetRawText() + "[/gold][/b]\n" + new LocString("achievements", text + ".description").GetFormattedText();
		}
		else
		{
			_infoLabel.Text = "[b][red]" + new LocString("achievements", "LOCKED.title").GetRawText() + "[/red][/b]\n" + new LocString("achievements", text + ".description").GetFormattedText();
		}
		SetLockVisuals();
		SetDateLabel();
	}

	private void SetLockVisuals()
	{
		_lock.Visible = !IsUnlocked;
		if (IsUnlocked)
		{
			_borderHsv.SetShaderParameter(_h, 1f);
			_borderHsv.SetShaderParameter(_s, 1f);
			_borderHsv.SetShaderParameter(_v, 1f);
			_iconHsv.SetShaderParameter(_s, 1f);
			_iconHsv.SetShaderParameter(_v, 1f);
		}
		else
		{
			_borderHsv.SetShaderParameter(_h, 0.4f);
			_borderHsv.SetShaderParameter(_s, 0.4f);
			_borderHsv.SetShaderParameter(_v, 0.8f);
			_iconHsv.SetShaderParameter(_s, 0.2f);
			_iconHsv.SetShaderParameter(_v, 0.5f);
		}
	}

	private void SetDateLabel()
	{
		_date.Visible = IsUnlocked;
		if (IsUnlocked)
		{
			if (!SaveManager.Instance.Progress.UnlockedAchievements.TryGetValue(_achievement, out var value))
			{
				_date.Visible = false;
				return;
			}
			DateTimeFormatInfo dateTimeFormat = LocManager.Instance.CultureInfo.DateTimeFormat;
			DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime, TimeZoneInfo.Local);
			LocString locString = new LocString("achievements", "UNLOCK_DATE.text");
			string variable = dateTime.ToString(new LocString("achievements", "UNLOCK_DATE.format").GetRawText(), dateTimeFormat);
			locString.Add("Date", variable);
			_date.SetTextAutoSize(locString.GetFormattedText());
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "achievement", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshUnlocked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLockVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetDateLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAchievementHolder>(Create(VariantUtils.ConvertTo<Achievement>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshUnlocked && args.Count == 0)
		{
			RefreshUnlocked();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLockVisuals && args.Count == 0)
		{
			SetLockVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDateLabel && args.Count == 0)
		{
			SetDateLabel();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAchievementHolder>(Create(VariantUtils.ConvertTo<Achievement>(in args[0])));
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
		if (method == MethodName.RefreshUnlocked)
		{
			return true;
		}
		if (method == MethodName.SetLockVisuals)
		{
			return true;
		}
		if (method == MethodName.SetDateLabel)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsUnlocked)
		{
			IsUnlocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._border)
		{
			_border = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._lock)
		{
			_lock = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconHsv)
		{
			_iconHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._borderHsv)
		{
			_borderHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			_infoLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._date)
		{
			_date = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._achievement)
		{
			_achievement = VariantUtils.ConvertTo<Achievement>(in value);
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
		if (name == PropertyName.IsUnlocked)
		{
			value = VariantUtils.CreateFrom<bool>(IsUnlocked);
			return true;
		}
		if (name == PropertyName._border)
		{
			value = VariantUtils.CreateFrom(in _border);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._lock)
		{
			value = VariantUtils.CreateFrom(in _lock);
			return true;
		}
		if (name == PropertyName._iconHsv)
		{
			value = VariantUtils.CreateFrom(in _iconHsv);
			return true;
		}
		if (name == PropertyName._borderHsv)
		{
			value = VariantUtils.CreateFrom(in _borderHsv);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			value = VariantUtils.CreateFrom(in _infoLabel);
			return true;
		}
		if (name == PropertyName._date)
		{
			value = VariantUtils.CreateFrom(in _date);
			return true;
		}
		if (name == PropertyName._achievement)
		{
			value = VariantUtils.CreateFrom(in _achievement);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._border, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._borderHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._date, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUnlocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._achievement, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsUnlocked, Variant.From<bool>(IsUnlocked));
		info.AddProperty(PropertyName._border, Variant.From(in _border));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._lock, Variant.From(in _lock));
		info.AddProperty(PropertyName._iconHsv, Variant.From(in _iconHsv));
		info.AddProperty(PropertyName._borderHsv, Variant.From(in _borderHsv));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._date, Variant.From(in _date));
		info.AddProperty(PropertyName._achievement, Variant.From(in _achievement));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsUnlocked, out var value))
		{
			IsUnlocked = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._border, out var value2))
		{
			_border = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._lock, out var value4))
		{
			_lock = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconHsv, out var value5))
		{
			_iconHsv = value5.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._borderHsv, out var value6))
		{
			_borderHsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value7))
		{
			_infoLabel = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._date, out var value8))
		{
			_date = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._achievement, out var value9))
		{
			_achievement = value9.As<Achievement>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value10))
		{
			_tween = value10.As<Tween>();
		}
	}
}
