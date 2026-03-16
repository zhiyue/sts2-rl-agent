using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NSettingsScreen.cs")]
public class NSettingsScreen : NSubmenu
{
	[Signal]
	public delegate void SettingsClosedEventHandler();

	[Signal]
	public delegate void SettingsOpenedEventHandler();

	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName SetIsInRun = "SetIsInRun";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnSettingsTabChanged = "OnSettingsTabChanged";

		public static readonly StringName LocalizeLabels = "LocalizeLabels";

		public static readonly StringName OpenModdingScreen = "OpenModdingScreen";

		public static readonly StringName OpenFeedbackScreen = "OpenFeedbackScreen";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public new static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _settingsTabManager = "_settingsTabManager";

		public static readonly StringName _feedbackScreenButton = "_feedbackScreenButton";

		public static readonly StringName _moddingScreenButton = "_moddingScreenButton";

		public static readonly StringName _toast = "_toast";

		public static readonly StringName _isInRun = "_isInRun";
	}

	public new class SignalName : NSubmenu.SignalName
	{
		public static readonly StringName SettingsClosed = "SettingsClosed";

		public static readonly StringName SettingsOpened = "SettingsOpened";
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/settings_screen");

	private NSettingsTabManager _settingsTabManager;

	private NOpenFeedbackScreenButton _feedbackScreenButton;

	private NOpenModdingScreenButton _moddingScreenButton;

	private NSettingsToast _toast;

	private bool _isInRun;

	public static readonly Vector2 settingTipsOffset = new Vector2(1012f, -60f);

	private SettingsClosedEventHandler backing_SettingsClosed;

	private SettingsOpenedEventHandler backing_SettingsOpened;

	public static string[] AssetPaths => new string[2] { _scenePath, "res://images/ui/language_warning.png" };

	protected override Control? InitialFocusedControl => _settingsTabManager.DefaultFocusedControl;

	public event SettingsClosedEventHandler SettingsClosed
	{
		add
		{
			backing_SettingsClosed = (SettingsClosedEventHandler)Delegate.Combine(backing_SettingsClosed, value);
		}
		remove
		{
			backing_SettingsClosed = (SettingsClosedEventHandler)Delegate.Remove(backing_SettingsClosed, value);
		}
	}

	public event SettingsOpenedEventHandler SettingsOpened
	{
		add
		{
			backing_SettingsOpened = (SettingsOpenedEventHandler)Delegate.Combine(backing_SettingsOpened, value);
		}
		remove
		{
			backing_SettingsOpened = (SettingsOpenedEventHandler)Delegate.Remove(backing_SettingsOpened, value);
		}
	}

	public void SetIsInRun(bool isInRun)
	{
		_isInRun = isInRun;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_settingsTabManager = GetNode<NSettingsTabManager>("%SettingsTabManager");
		_feedbackScreenButton = GetNode<NOpenFeedbackScreenButton>("%FeedbackButton");
		_moddingScreenButton = GetNode<NOpenModdingScreenButton>("%ModdingButton");
		_toast = GetNode<NSettingsToast>("%Toast");
		LocalizeLabels();
		base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
		_settingsTabManager.Connect(NSettingsTabManager.SignalName.TabChanged, Callable.From(OnSettingsTabChanged));
		_moddingScreenButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenModdingScreen));
		_feedbackScreenButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)OpenFeedbackScreen));
		if (SaveManager.Instance.SettingsSave.ModSettings != null && ModManager.AllMods.Count > 0)
		{
			GetNode<Control>("%Modding").Visible = true;
			GetNode<Control>("%ModdingDivider").Visible = true;
		}
		if (PlatformUtil.GetSupportedWindowMode() == SupportedWindowMode.FullscreenOnly)
		{
			GetNode<Control>("%Fullscreen").Visible = false;
			GetNode<Control>("%FullscreenDivider").Visible = false;
		}
		if (RunManager.Instance.IsInProgress)
		{
			GetNode<Node>("%LanguageLine").GetNode<MegaRichTextLabel>("Label").Modulate = StsColors.gray;
			NLanguageDropdown node = GetNode<NLanguageDropdown>("%LanguageDropdown");
			node.Modulate = StsColors.gray;
			node.Disable();
			_moddingScreenButton.Disable();
		}
	}

	public void ShowToast(LocString locString)
	{
		_toast.Show(locString);
	}

	private void OnSettingsTabChanged()
	{
	}

	private void LocalizeLabels()
	{
		Node content = GetNode<NSettingsPanel>("%GeneralSettings").Content;
		LocHelper(content.GetNode<Node>("LanguageLine"), new LocString("settings_ui", _isInRun ? "LANGUAGE_IN_RUN" : "LANGUAGE"));
		LocHelper(content.GetNode<Node>("FastMode"), new LocString("settings_ui", "FASTMODE"));
		LocHelper(content.GetNode<Node>("Screenshake"), new LocString("settings_ui", "SCREENSHAKE"));
		LocHelper(content.GetNode<Node>("CommonTooltips"), new LocString("settings_ui", "COMMON_TOOLTIPS"));
		LocHelper(content.GetNode<Node>("ShowRunTimer"), new LocString("settings_ui", "SHOW_RUN_TIMER_HEADER"));
		LocHelper(content.GetNode<Node>("ShowHandCardCount"), new LocString("settings_ui", "SHOW_HAND_CARD_COUNT_HEADER"));
		LocHelper(content.GetNode<Node>("LongPressConfirmations"), new LocString("settings_ui", "LONG_PRESS_CONFIRMATION_HEADER"));
		LocHelper(content.GetNode<Node>("SkipIntroLogo"), new LocString("settings_ui", "SKIP_INTRO_LOGO_HEADER"));
		LocHelper(content.GetNode<Node>("LimitFpsInBackground"), new LocString("settings_ui", "LIMIT_FPS_IN_BACKGROUND_HEADER"));
		LocHelper(content.GetNode<Node>("UploadGameplayData"), new LocString("settings_ui", "UPLOAD_GAMEPLAY_DATA"));
		LocHelper(content.GetNode<Node>("TextEffects"), new LocString("settings_ui", "TEXT_EFFECTS"));
		LocHelper(content.GetNode<Node>("SendFeedback"), new LocString("settings_ui", "SEND_FEEDBACK"));
		LocHelper(content.GetNode<Node>("ResetTutorials"), new LocString("settings_ui", "TUTORIAL_RESET"));
		LocHelper(content.GetNode<Node>("Credits"), new LocString("settings_ui", "CREDITS"));
		LocHelper(content.GetNode<Node>("ResetGameplay"), new LocString("settings_ui", "RESET_DEFAULT"));
		Node content2 = GetNode<NSettingsPanel>("%GraphicsSettings").Content;
		LocHelper(content2.GetNode<Node>("Fullscreen"), new LocString("settings_ui", "FULLSCREEN"));
		LocHelper(content2.GetNode<Node>("DisplaySelection"), new LocString("settings_ui", "DISPLAY_SELECTION"));
		LocHelper(content2.GetNode<Node>("WindowedResolution"), new LocString("settings_ui", "RESOLUTION"));
		LocHelper(content2.GetNode<Node>("AspectRatio"), new LocString("settings_ui", "ASPECT_RATIO"));
		LocHelper(content2.GetNode<Node>("WindowResizing"), new LocString("settings_ui", "WINDOW_RESIZING"));
		LocHelper(content2.GetNode<Node>("VSync"), new LocString("settings_ui", "VSYNC"));
		LocHelper(content2.GetNode<Node>("MaxFps"), new LocString("settings_ui", "FPS_CAP"));
		LocHelper(content2.GetNode<Node>("Msaa"), new LocString("settings_ui", "MSAA"));
		LocHelper(content2.GetNode<Node>("ResetGraphics"), new LocString("settings_ui", "RESET_DEFAULT"));
		Node content3 = GetNode<NSettingsPanel>("%SoundSettings").Content;
		LocHelper(content3.GetNode<Node>("MasterVolume"), new LocString("settings_ui", "MASTER_VOLUME"));
		LocHelper(content3.GetNode<Node>("BgmVolume"), new LocString("settings_ui", "MUSIC_VOLUME"));
		LocHelper(content3.GetNode<Node>("SfxVolume"), new LocString("settings_ui", "SFX_VOLUME"));
		LocHelper(content3.GetNode<Node>("AmbienceVolume"), new LocString("settings_ui", "AMBIENCE_VOLUME"));
		LocHelper(content3.GetNode<Node>("MuteIfBackground"), new LocString("settings_ui", "BACKGROUND_MUTE"));
	}

	private static void LocHelper(Node settingsLineNode, LocString locString)
	{
		settingsLineNode.GetNode<MegaRichTextLabel>("Label").Text = locString.GetFormattedText();
	}

	private void OpenModdingScreen(NButton _)
	{
		_stack.PushSubmenuType<NModdingScreen>();
	}

	private void OpenFeedbackScreen(NButton _)
	{
		_lastFocusedControl = _feedbackScreenButton;
		TaskHelper.RunSafely(OpenFeedbackScreen());
	}

	public async Task OpenFeedbackScreen()
	{
		Log.Info("Opening feedback screen");
		base.Visible = false;
		NGame.Instance.MainMenu?.DisableBackstopInstantly();
		NCapstoneContainer.Instance?.DisableBackstopInstantly();
		NRun.Instance?.GlobalUi.RelicInventory.ShowImmediately();
		NRun.Instance?.GlobalUi.MultiplayerPlayerContainer.ShowImmediately();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Image image = GetViewport().GetTexture().GetImage();
		base.Visible = true;
		NRun.Instance?.GlobalUi.RelicInventory.HideImmediately();
		NRun.Instance?.GlobalUi.MultiplayerPlayerContainer.HideImmediately();
		NGame.Instance.MainMenu?.EnableBackstopInstantly();
		NCapstoneContainer.Instance?.EnableBackstopInstantly();
		NSendFeedbackScreen feedbackScreen = NGame.Instance.FeedbackScreen;
		feedbackScreen.SetScreenshot(image);
		NGame.Instance.FeedbackScreen.Open();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		base.ProcessMode = ProcessModeEnum.Inherit;
		EmitSignal(SignalName.SettingsOpened);
		_settingsTabManager.ResetTabs();
		_settingsTabManager.Enable();
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		base.ProcessMode = ProcessModeEnum.Disabled;
		SaveManager.Instance.SaveSettings();
		SaveManager.Instance.SavePrefsFile();
		EmitSignal(SignalName.SettingsClosed);
		_settingsTabManager.Disable();
	}

	protected override void OnSubmenuHidden()
	{
		base.OnSubmenuClosed();
		base.ProcessMode = ProcessModeEnum.Disabled;
		EmitSignal(SignalName.SettingsClosed);
	}

	protected override void OnSubmenuShown()
	{
		base.OnSubmenuShown();
		base.ProcessMode = ProcessModeEnum.Inherit;
		EmitSignal(SignalName.SettingsOpened);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName.SetIsInRun, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isInRun", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSettingsTabChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LocalizeLabels, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenModdingScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenFeedbackScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetIsInRun && args.Count == 1)
		{
			SetIsInRun(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSettingsTabChanged && args.Count == 0)
		{
			OnSettingsTabChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LocalizeLabels && args.Count == 0)
		{
			LocalizeLabels();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenModdingScreen && args.Count == 1)
		{
			OpenModdingScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenFeedbackScreen && args.Count == 1)
		{
			OpenFeedbackScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetIsInRun)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnSettingsTabChanged)
		{
			return true;
		}
		if (method == MethodName.LocalizeLabels)
		{
			return true;
		}
		if (method == MethodName.OpenModdingScreen)
		{
			return true;
		}
		if (method == MethodName.OpenFeedbackScreen)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._settingsTabManager)
		{
			_settingsTabManager = VariantUtils.ConvertTo<NSettingsTabManager>(in value);
			return true;
		}
		if (name == PropertyName._feedbackScreenButton)
		{
			_feedbackScreenButton = VariantUtils.ConvertTo<NOpenFeedbackScreenButton>(in value);
			return true;
		}
		if (name == PropertyName._moddingScreenButton)
		{
			_moddingScreenButton = VariantUtils.ConvertTo<NOpenModdingScreenButton>(in value);
			return true;
		}
		if (name == PropertyName._toast)
		{
			_toast = VariantUtils.ConvertTo<NSettingsToast>(in value);
			return true;
		}
		if (name == PropertyName._isInRun)
		{
			_isInRun = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._settingsTabManager)
		{
			value = VariantUtils.CreateFrom(in _settingsTabManager);
			return true;
		}
		if (name == PropertyName._feedbackScreenButton)
		{
			value = VariantUtils.CreateFrom(in _feedbackScreenButton);
			return true;
		}
		if (name == PropertyName._moddingScreenButton)
		{
			value = VariantUtils.CreateFrom(in _moddingScreenButton);
			return true;
		}
		if (name == PropertyName._toast)
		{
			value = VariantUtils.CreateFrom(in _toast);
			return true;
		}
		if (name == PropertyName._isInRun)
		{
			value = VariantUtils.CreateFrom(in _isInRun);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsTabManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._feedbackScreenButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moddingScreenButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._toast, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInRun, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._settingsTabManager, Variant.From(in _settingsTabManager));
		info.AddProperty(PropertyName._feedbackScreenButton, Variant.From(in _feedbackScreenButton));
		info.AddProperty(PropertyName._moddingScreenButton, Variant.From(in _moddingScreenButton));
		info.AddProperty(PropertyName._toast, Variant.From(in _toast));
		info.AddProperty(PropertyName._isInRun, Variant.From(in _isInRun));
		info.AddSignalEventDelegate(SignalName.SettingsClosed, backing_SettingsClosed);
		info.AddSignalEventDelegate(SignalName.SettingsOpened, backing_SettingsOpened);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._settingsTabManager, out var value))
		{
			_settingsTabManager = value.As<NSettingsTabManager>();
		}
		if (info.TryGetProperty(PropertyName._feedbackScreenButton, out var value2))
		{
			_feedbackScreenButton = value2.As<NOpenFeedbackScreenButton>();
		}
		if (info.TryGetProperty(PropertyName._moddingScreenButton, out var value3))
		{
			_moddingScreenButton = value3.As<NOpenModdingScreenButton>();
		}
		if (info.TryGetProperty(PropertyName._toast, out var value4))
		{
			_toast = value4.As<NSettingsToast>();
		}
		if (info.TryGetProperty(PropertyName._isInRun, out var value5))
		{
			_isInRun = value5.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<SettingsClosedEventHandler>(SignalName.SettingsClosed, out var value6))
		{
			backing_SettingsClosed = value6;
		}
		if (info.TryGetSignalEventDelegate<SettingsOpenedEventHandler>(SignalName.SettingsOpened, out var value7))
		{
			backing_SettingsOpened = value7;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.SettingsClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.SettingsOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalSettingsClosed()
	{
		EmitSignal(SignalName.SettingsClosed);
	}

	protected void EmitSignalSettingsOpened()
	{
		EmitSignal(SignalName.SettingsOpened);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.SettingsClosed && args.Count == 0)
		{
			backing_SettingsClosed?.Invoke();
		}
		else if (signal == SignalName.SettingsOpened && args.Count == 0)
		{
			backing_SettingsOpened?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.SettingsClosed)
		{
			return true;
		}
		if (signal == SignalName.SettingsOpened)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
