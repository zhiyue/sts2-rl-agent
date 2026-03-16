using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;

[ScriptPath("res://src/Core/Nodes/Screens/PauseMenu/NPauseMenu.cs")]
public class NPauseMenu : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshLabels = "RefreshLabels";

		public static readonly StringName OnBackOrResumeButtonPressed = "OnBackOrResumeButtonPressed";

		public static readonly StringName OnSettingsButtonPressed = "OnSettingsButtonPressed";

		public static readonly StringName OnCompendiumButtonPressed = "OnCompendiumButtonPressed";

		public static readonly StringName OnGiveUpButtonPressed = "OnGiveUpButtonPressed";

		public static readonly StringName OnDisconnectButtonPressed = "OnDisconnectButtonPressed";

		public static readonly StringName OnSaveAndQuitButtonPressed = "OnSaveAndQuitButtonPressed";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName Buttons = "Buttons";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName ScreenType = "ScreenType";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _buttonContainer = "_buttonContainer";

		public static readonly StringName _resumeButton = "_resumeButton";

		public static readonly StringName _settingsButton = "_settingsButton";

		public static readonly StringName _compendiumButton = "_compendiumButton";

		public static readonly StringName _giveUpButton = "_giveUpButton";

		public static readonly StringName _disconnectButton = "_disconnectButton";

		public static readonly StringName _saveAndQuitButton = "_saveAndQuitButton";

		public static readonly StringName _pausedLabel = "_pausedLabel";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly LocString _pausedLoc = new LocString("gameplay_ui", "PAUSE_MENU.PAUSED");

	private static readonly LocString _resumeLoc = new LocString("gameplay_ui", "PAUSE_MENU.RESUME");

	private static readonly LocString _settingsLoc = new LocString("gameplay_ui", "PAUSE_MENU.SETTINGS");

	private static readonly LocString _compendiumLoc = new LocString("gameplay_ui", "PAUSE_MENU.COMPENDIUM");

	private static readonly LocString _giveUpLoc = new LocString("gameplay_ui", "PAUSE_MENU.GIVE_UP");

	private static readonly LocString _disconnectLoc = new LocString("gameplay_ui", "PAUSE_MENU.DISCONNECT");

	private static readonly LocString _saveAndQuitLoc = new LocString("gameplay_ui", "PAUSE_MENU.SAVE_AND_QUIT");

	private NBackButton _backButton;

	private Control _buttonContainer;

	private NPauseMenuButton _resumeButton;

	private NPauseMenuButton _settingsButton;

	private NPauseMenuButton _compendiumButton;

	private NPauseMenuButton _giveUpButton;

	private NPauseMenuButton _disconnectButton;

	private NPauseMenuButton _saveAndQuitButton;

	private MegaLabel _pausedLabel;

	private IRunState _runState;

	protected override Control InitialFocusedControl => _resumeButton;

	private NPauseMenuButton[] Buttons => new NPauseMenuButton[6] { _resumeButton, _settingsButton, _compendiumButton, _giveUpButton, _disconnectButton, _saveAndQuitButton };

	public bool UseSharedBackstop => true;

	public NetScreenType ScreenType => NetScreenType.PauseMenu;

	public override void _Ready()
	{
		ConnectSignals();
		_buttonContainer = GetNode<Control>("%ButtonContainer");
		_resumeButton = _buttonContainer.GetNode<NPauseMenuButton>("Resume");
		_settingsButton = _buttonContainer.GetNode<NPauseMenuButton>("Settings");
		_compendiumButton = _buttonContainer.GetNode<NPauseMenuButton>("Compendium");
		_giveUpButton = _buttonContainer.GetNode<NPauseMenuButton>("GiveUp");
		_disconnectButton = _buttonContainer.GetNode<NPauseMenuButton>("Disconnect");
		_saveAndQuitButton = _buttonContainer.GetNode<NPauseMenuButton>("SaveAndQuit");
		_backButton = GetNode<NBackButton>("%BackButton");
		_pausedLabel = GetNode<MegaLabel>("%PausedText/Label");
		RefreshLabels();
		_resumeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackOrResumeButtonPressed));
		_settingsButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSettingsButtonPressed));
		_compendiumButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCompendiumButtonPressed));
		_giveUpButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnGiveUpButtonPressed));
		_disconnectButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnDisconnectButtonPressed));
		_saveAndQuitButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSaveAndQuitButtonPressed));
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackOrResumeButtonPressed));
		_backButton.Disable();
		_giveUpButton.Visible = RunManager.Instance.NetService.Type != NetGameType.Client;
		_saveAndQuitButton.Visible = RunManager.Instance.NetService.Type != NetGameType.Client;
		_disconnectButton.Visible = RunManager.Instance.NetService.Type == NetGameType.Client;
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].FocusNeighborLeft = Buttons[i].GetPath();
			Buttons[i].FocusNeighborRight = Buttons[i].GetPath();
			Buttons[i].FocusNeighborTop = ((i > 0) ? Buttons[i - 1].GetPath() : Buttons[i].GetPath());
			Buttons[i].FocusNeighborBottom = ((i < Buttons.Length - 1) ? Buttons[i + 1].GetPath() : Buttons[i].GetPath());
		}
	}

	private void RefreshLabels()
	{
		_pausedLabel.SetTextAutoSize(_pausedLoc.GetFormattedText());
		_resumeButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_resumeLoc.GetFormattedText());
		_settingsButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_settingsLoc.GetFormattedText());
		_compendiumButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_compendiumLoc.GetFormattedText());
		_giveUpButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_giveUpLoc.GetFormattedText());
		_disconnectButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_disconnectLoc.GetFormattedText());
		_saveAndQuitButton.GetNode<MegaLabel>("Label").SetTextAutoSize(_saveAndQuitLoc.GetFormattedText());
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
		if (!RunManager.Instance.IsInProgress || _runState.IsGameOver)
		{
			_giveUpButton.Disable();
		}
		else
		{
			_giveUpButton.Enable();
		}
		_compendiumButton.Visible = !NGame.IsReleaseGame() || SaveManager.Instance.IsCompendiumAvailable();
	}

	private void OnBackOrResumeButtonPressed(NButton _)
	{
		SfxCmd.Play("event:/sfx/ui/map/map_close");
		NCapstoneContainer.Instance.Close();
		NRun.Instance.GlobalUi.TopBar.Pause.ToggleAnimState();
	}

	private void OnSettingsButtonPressed(NButton _)
	{
		_stack.PushSubmenuType<NSettingsScreen>();
	}

	private void OnCompendiumButtonPressed(NButton _)
	{
		NCompendiumSubmenu submenuType = _stack.GetSubmenuType<NCompendiumSubmenu>();
		submenuType.Initialize(_runState);
		_stack.Push(submenuType);
	}

	private void OnGiveUpButtonPressed(NButton _)
	{
		NModalContainer.Instance.Add(NAbandonRunConfirmPopup.Create(null));
	}

	private void OnDisconnectButtonPressed(NButton _)
	{
		if (RunManager.Instance.NetService.IsConnected)
		{
			NModalContainer.Instance.Add(NDisconnectConfirmPopup.Create());
		}
		else
		{
			TaskHelper.RunSafely(NGame.Instance.ReturnToMainMenuAfterRun());
		}
	}

	private void OnSaveAndQuitButtonPressed(NButton _)
	{
		TaskHelper.RunSafely(CloseToMenu());
	}

	private async Task CloseToMenu()
	{
		_resumeButton.Disable();
		_settingsButton.Disable();
		_compendiumButton.Disable();
		_giveUpButton.Disable();
		_disconnectButton.Disable();
		_saveAndQuitButton.Disable();
		_backButton.Disable();
		RunManager.Instance.ActionQueueSet.Reset();
		NRunMusicController.Instance.StopMusic();
		await NGame.Instance.ReturnToMainMenu();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		NHotkeyManager.Instance.AddBlockingScreen(this);
	}

	public override void OnSubmenuClosed()
	{
		_backButton.Disable();
		base.Visible = false;
		NHotkeyManager.Instance.RemoveBlockingScreen(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshLabels, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBackOrResumeButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSettingsButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCompendiumButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnGiveUpButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDisconnectButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSaveAndQuitButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RefreshLabels && args.Count == 0)
		{
			RefreshLabels();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBackOrResumeButtonPressed && args.Count == 1)
		{
			OnBackOrResumeButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSettingsButtonPressed && args.Count == 1)
		{
			OnSettingsButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCompendiumButtonPressed && args.Count == 1)
		{
			OnCompendiumButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGiveUpButtonPressed && args.Count == 1)
		{
			OnGiveUpButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisconnectButtonPressed && args.Count == 1)
		{
			OnDisconnectButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSaveAndQuitButtonPressed && args.Count == 1)
		{
			OnSaveAndQuitButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.RefreshLabels)
		{
			return true;
		}
		if (method == MethodName.OnBackOrResumeButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnSettingsButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnCompendiumButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnGiveUpButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnDisconnectButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnSaveAndQuitButtonPressed)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._buttonContainer)
		{
			_buttonContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._resumeButton)
		{
			_resumeButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._settingsButton)
		{
			_settingsButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._compendiumButton)
		{
			_compendiumButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._giveUpButton)
		{
			_giveUpButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._disconnectButton)
		{
			_disconnectButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._saveAndQuitButton)
		{
			_saveAndQuitButton = VariantUtils.ConvertTo<NPauseMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._pausedLabel)
		{
			_pausedLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
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
		if (name == PropertyName.Buttons)
		{
			GodotObject[] buttons = Buttons;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(buttons);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._buttonContainer)
		{
			value = VariantUtils.CreateFrom(in _buttonContainer);
			return true;
		}
		if (name == PropertyName._resumeButton)
		{
			value = VariantUtils.CreateFrom(in _resumeButton);
			return true;
		}
		if (name == PropertyName._settingsButton)
		{
			value = VariantUtils.CreateFrom(in _settingsButton);
			return true;
		}
		if (name == PropertyName._compendiumButton)
		{
			value = VariantUtils.CreateFrom(in _compendiumButton);
			return true;
		}
		if (name == PropertyName._giveUpButton)
		{
			value = VariantUtils.CreateFrom(in _giveUpButton);
			return true;
		}
		if (name == PropertyName._disconnectButton)
		{
			value = VariantUtils.CreateFrom(in _disconnectButton);
			return true;
		}
		if (name == PropertyName._saveAndQuitButton)
		{
			value = VariantUtils.CreateFrom(in _saveAndQuitButton);
			return true;
		}
		if (name == PropertyName._pausedLabel)
		{
			value = VariantUtils.CreateFrom(in _pausedLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._resumeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._compendiumButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._giveUpButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._disconnectButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._saveAndQuitButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pausedLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName.Buttons, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._buttonContainer, Variant.From(in _buttonContainer));
		info.AddProperty(PropertyName._resumeButton, Variant.From(in _resumeButton));
		info.AddProperty(PropertyName._settingsButton, Variant.From(in _settingsButton));
		info.AddProperty(PropertyName._compendiumButton, Variant.From(in _compendiumButton));
		info.AddProperty(PropertyName._giveUpButton, Variant.From(in _giveUpButton));
		info.AddProperty(PropertyName._disconnectButton, Variant.From(in _disconnectButton));
		info.AddProperty(PropertyName._saveAndQuitButton, Variant.From(in _saveAndQuitButton));
		info.AddProperty(PropertyName._pausedLabel, Variant.From(in _pausedLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backButton, out var value))
		{
			_backButton = value.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._buttonContainer, out var value2))
		{
			_buttonContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._resumeButton, out var value3))
		{
			_resumeButton = value3.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._settingsButton, out var value4))
		{
			_settingsButton = value4.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._compendiumButton, out var value5))
		{
			_compendiumButton = value5.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._giveUpButton, out var value6))
		{
			_giveUpButton = value6.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._disconnectButton, out var value7))
		{
			_disconnectButton = value7.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._saveAndQuitButton, out var value8))
		{
			_saveAndQuitButton = value8.As<NPauseMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._pausedLabel, out var value9))
		{
			_pausedLabel = value9.As<MegaLabel>();
		}
	}
}
