using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ProfileScreen;

[ScriptPath("res://src/Core/Nodes/Screens/ProfileScreen/NProfileButton.cs")]
public class NProfileButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Initialize = "Initialize";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _title = "_title";

		public static readonly StringName _description = "_description";

		public static readonly StringName _currentProfileIndicator = "_currentProfileIndicator";

		public static readonly StringName _profileIcon = "_profileIcon";

		public static readonly StringName _deleteButton = "_deleteButton";

		public static readonly StringName _background = "_background";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _profileScreen = "_profileScreen";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _profileId = "_profileId";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private MegaRichTextLabel _title;

	private MegaRichTextLabel _description;

	private Control _currentProfileIndicator;

	private NProfileIcon _profileIcon;

	private NDeleteProfileButton _deleteButton;

	private TextureRect _background;

	private ShaderMaterial _hsv;

	private NProfileScreen? _profileScreen;

	private Tween? _tween;

	private int _profileId;

	public static IEnumerable<string> AssetPaths => NProfileIcon.AssetPaths;

	public override void _Ready()
	{
		ConnectSignals();
		_background = GetNode<TextureRect>("%Background");
		_hsv = (ShaderMaterial)_background.Material;
		_title = GetNode<MegaRichTextLabel>("%Title");
		_description = GetNode<MegaRichTextLabel>("%Info");
		_profileIcon = GetNode<NProfileIcon>("%ProfileIcon");
		_currentProfileIndicator = GetNode<Control>("%CurrentProfileIndicator");
	}

	public void Initialize(NProfileScreen profileScreen, int profileId)
	{
		_profileScreen = profileScreen;
		_profileId = profileId;
		LocString locString = new LocString("main_menu_ui", "PROFILE_SCREEN.BUTTON.title");
		locString.Add("Id", profileId);
		_title.Text = locString.GetFormattedText();
		GodotFileIo godotFileIo = new GodotFileIo(UserDataPathProvider.GetProfileScopedPath(profileId, "saves"));
		_profileIcon.SetProfileId(profileId);
		_currentProfileIndicator.Visible = SaveManager.Instance.CurrentProfileId == profileId;
		string path = "progress.save";
		if (NProfileScreen.forceShowProfileAsDeleted == profileId || !godotFileIo.FileExists(path))
		{
			LocString locString2 = new LocString("main_menu_ui", "PROFILE_SCREEN.BUTTON.empty");
			_description.Text = locString2.GetFormattedText();
			return;
		}
		LocString locString3 = new LocString("main_menu_ui", "PROFILE_SCREEN.BUTTON.description");
		if (SaveManager.Instance.CurrentProfileId == profileId)
		{
			locString3.Add("Playtime", TimeFormatting.Format(SaveManager.Instance.Progress.TotalPlaytime));
		}
		else
		{
			string json = godotFileIo.ReadFile(path);
			JsonObject jsonObject = JsonSerializer.Deserialize(json, JsonSerializationUtility.GetTypeInfo<JsonObject>());
			if (jsonObject.TryGetPropertyValue("total_playtime", out JsonNode jsonNode) && jsonNode is JsonValue jsonValue && jsonValue.TryGetValue<long>(out var value))
			{
				locString3.Add("Playtime", TimeFormatting.Format(value));
			}
			else
			{
				locString3.Add("Playtime", "???");
			}
		}
		DateTimeOffset dateTimeOffset = godotFileIo.GetLastModifiedTime(path);
		string path2 = "current_run.save";
		if (godotFileIo.FileExists(path2))
		{
			DateTimeOffset lastModifiedTime = godotFileIo.GetLastModifiedTime(path2);
			dateTimeOffset = ((dateTimeOffset > lastModifiedTime) ? dateTimeOffset : lastModifiedTime);
		}
		string path3 = "current_run_mp.save";
		if (godotFileIo.FileExists(path3))
		{
			DateTimeOffset lastModifiedTime2 = godotFileIo.GetLastModifiedTime(path3);
			dateTimeOffset = ((dateTimeOffset > lastModifiedTime2) ? dateTimeOffset : lastModifiedTime2);
		}
		DateTimeFormatInfo dateTimeFormat = LocManager.Instance.CultureInfo.DateTimeFormat;
		DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, TimeZoneInfo.Local);
		LocString locString4 = new LocString("main_menu_ui", "PROFILE_SCREEN.BUTTON.dateFormat");
		string variable = dateTime.ToString(locString4.GetFormattedText(), dateTimeFormat);
		locString3.Add("LastUpdatedTime", variable);
		_description.Text = locString3.GetFormattedText();
	}

	protected override void OnRelease()
	{
		if (SaveManager.Instance.CurrentProfileId == _profileId)
		{
			NGame.Instance.MainMenu.SubmenuStack.Pop();
		}
		else
		{
			TaskHelper.RunSafely(SwitchToThisProfile());
		}
	}

	private async Task SwitchToThisProfile()
	{
		_profileScreen?.ShowLoading();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		SaveManager.Instance.SwitchProfileId(_profileId);
		ReadSaveResult<PrefsSave> prefsReadResult = SaveManager.Instance.InitPrefsData();
		ReadSaveResult<SerializableProgress> progressReadResult = SaveManager.Instance.InitProgressData();
		NGame.Instance.ReloadMainMenu();
		NGame.Instance.CheckShowSaveFileError(progressReadResult, prefsReadResult, new ReadSaveResult<SettingsSave>(new SettingsSave()));
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One * 1.03f, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1.3f, 0.05);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "profileScreen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Int, "profileId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.Initialize && args.Count == 2)
		{
			Initialize(VariantUtils.ConvertTo<NProfileScreen>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._title)
		{
			_title = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentProfileIndicator)
		{
			_currentProfileIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._profileIcon)
		{
			_profileIcon = VariantUtils.ConvertTo<NProfileIcon>(in value);
			return true;
		}
		if (name == PropertyName._deleteButton)
		{
			_deleteButton = VariantUtils.ConvertTo<NDeleteProfileButton>(in value);
			return true;
		}
		if (name == PropertyName._background)
		{
			_background = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._profileScreen)
		{
			_profileScreen = VariantUtils.ConvertTo<NProfileScreen>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._profileId)
		{
			_profileId = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._title)
		{
			value = VariantUtils.CreateFrom(in _title);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._currentProfileIndicator)
		{
			value = VariantUtils.CreateFrom(in _currentProfileIndicator);
			return true;
		}
		if (name == PropertyName._profileIcon)
		{
			value = VariantUtils.CreateFrom(in _profileIcon);
			return true;
		}
		if (name == PropertyName._deleteButton)
		{
			value = VariantUtils.CreateFrom(in _deleteButton);
			return true;
		}
		if (name == PropertyName._background)
		{
			value = VariantUtils.CreateFrom(in _background);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._profileScreen)
		{
			value = VariantUtils.CreateFrom(in _profileScreen);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._profileId)
		{
			value = VariantUtils.CreateFrom(in _profileId);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._title, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentProfileIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._profileIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deleteButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._profileScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._profileId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._title, Variant.From(in _title));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._currentProfileIndicator, Variant.From(in _currentProfileIndicator));
		info.AddProperty(PropertyName._profileIcon, Variant.From(in _profileIcon));
		info.AddProperty(PropertyName._deleteButton, Variant.From(in _deleteButton));
		info.AddProperty(PropertyName._background, Variant.From(in _background));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._profileScreen, Variant.From(in _profileScreen));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._profileId, Variant.From(in _profileId));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._title, out var value))
		{
			_title = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value2))
		{
			_description = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentProfileIndicator, out var value3))
		{
			_currentProfileIndicator = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._profileIcon, out var value4))
		{
			_profileIcon = value4.As<NProfileIcon>();
		}
		if (info.TryGetProperty(PropertyName._deleteButton, out var value5))
		{
			_deleteButton = value5.As<NDeleteProfileButton>();
		}
		if (info.TryGetProperty(PropertyName._background, out var value6))
		{
			_background = value6.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._profileScreen, out var value8))
		{
			_profileScreen = value8.As<NProfileScreen>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value9))
		{
			_tween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._profileId, out var value10))
		{
			_profileId = value10.As<int>();
		}
	}
}
