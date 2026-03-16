using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NInputSettingsPanel.cs")]
public class NInputSettingsPanel : NSettingsPanel
{
	public new class MethodName : NSettingsPanel.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnViewportSizeChange = "OnViewportSizeChange";

		public new static readonly StringName OnVisibilityChange = "OnVisibilityChange";

		public static readonly StringName SetAsListeningEntry = "SetAsListeningEntry";

		public new static readonly StringName _UnhandledKeyInput = "_UnhandledKeyInput";

		public new static readonly StringName _Input = "_Input";
	}

	public new class PropertyName : NSettingsPanel.PropertyName
	{
		public new static readonly StringName _minPadding = "_minPadding";

		public static readonly StringName _listeningEntry = "_listeningEntry";

		public static readonly StringName _resetToDefaultButton = "_resetToDefaultButton";

		public static readonly StringName _commandHeader = "_commandHeader";

		public static readonly StringName _keyboardHeader = "_keyboardHeader";

		public static readonly StringName _controllerHeader = "_controllerHeader";

		public static readonly StringName _steamInputPrompt = "_steamInputPrompt";
	}

	public new class SignalName : NSettingsPanel.SignalName
	{
	}

	private float _minPadding = 50f;

	private NInputSettingsEntry? _listeningEntry;

	private NButton _resetToDefaultButton;

	private MegaRichTextLabel _commandHeader;

	private MegaRichTextLabel _keyboardHeader;

	private MegaRichTextLabel _controllerHeader;

	private MegaRichTextLabel _steamInputPrompt;

	public override void _Ready()
	{
		base._Ready();
		_resetToDefaultButton = GetNode<NButton>("%ResetToDefaultButton");
		_commandHeader = GetNode<MegaRichTextLabel>("%CommandHeader");
		_keyboardHeader = GetNode<MegaRichTextLabel>("%KeyboardHeader");
		_controllerHeader = GetNode<MegaRichTextLabel>("%ControllerHeader");
		_steamInputPrompt = GetNode<MegaRichTextLabel>("%SteamInputPrompt");
		_resetToDefaultButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
		{
			NInputManager.Instance.ResetToDefaults();
		}));
		GetViewport().Connect(Viewport.SignalName.SizeChanged, Callable.From(OnViewportSizeChange));
		_commandHeader.Text = new LocString("settings_ui", "INPUT_SETTINGS.COMMAND_HEADER").GetFormattedText();
		_keyboardHeader.Text = new LocString("settings_ui", "INPUT_SETTINGS.KEYBOARD_HEADER").GetFormattedText();
		_controllerHeader.Text = new LocString("settings_ui", "INPUT_SETTINGS.CONTROLLER_HEADER").GetFormattedText();
		_steamInputPrompt.Text = new LocString("settings_ui", "INPUT_SETTINGS.STEAM_INPUT_DETECTED").GetFormattedText();
		IReadOnlyList<StringName> readOnlyList = NInputManager.remappableControllerInputs.Concat(NInputManager.remappableKeyboardInputs).Distinct().ToList();
		List<NInputSettingsEntry> list = base.Content.GetChildren().OfType<NInputSettingsEntry>().ToList();
		foreach (StringName item in readOnlyList)
		{
			NInputSettingsEntry entry = NInputSettingsEntry.Create(item);
			entry.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
			{
				SetAsListeningEntry(entry);
			}));
			base.Content.AddChildSafely(entry);
			list.Add(entry);
		}
		for (int num = 0; num < list.Count; num++)
		{
			list[num].FocusNeighborLeft = list[num].GetPath();
			list[num].FocusNeighborRight = list[num].GetPath();
			list[num].FocusNeighborTop = ((num > 0) ? list[num - 1].GetPath() : list[num].GetPath());
			list[num].FocusNeighborBottom = ((num < list.Count - 1) ? list[num + 1].GetPath() : list[num].GetPath());
		}
		_resetToDefaultButton.FocusNeighborLeft = _resetToDefaultButton.GetPath();
		_resetToDefaultButton.FocusNeighborRight = _resetToDefaultButton.GetPath();
		_resetToDefaultButton.FocusNeighborTop = _resetToDefaultButton.GetPath();
		_resetToDefaultButton.FocusNeighborBottom = list[0].GetPath();
		list[0].FocusNeighborTop = _resetToDefaultButton.GetPath();
		_firstControl = base.Content.GetChildren().OfType<NInputSettingsEntry>().First();
	}

	private async Task RefreshSize()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Vector2 size = GetParent<Control>().Size;
		Vector2 minimumSize = base.Content.GetMinimumSize();
		if (minimumSize.Y + _minPadding >= size.Y)
		{
			base.Size = new Vector2(base.Content.Size.X, minimumSize.Y + size.Y * 0.4f);
		}
	}

	private void OnViewportSizeChange()
	{
		TaskHelper.RunSafely(RefreshSize());
	}

	protected override void OnVisibilityChange()
	{
		base.OnVisibilityChange();
		_listeningEntry = null;
		_steamInputPrompt.Visible = !NControllerManager.Instance.ShouldAllowControllerRebinding;
		TaskHelper.RunSafely(RefreshSize());
	}

	private void SetAsListeningEntry(NInputSettingsEntry entry)
	{
		_listeningEntry = entry;
	}

	public override void _UnhandledKeyInput(InputEvent inputEvent)
	{
		if (_listeningEntry != null && NInputManager.remappableKeyboardInputs.Contains(_listeningEntry.InputName) && inputEvent is InputEventKey inputEventKey)
		{
			NInputManager.Instance.ModifyShortcutKey(_listeningEntry.InputName, inputEventKey.Keycode);
			GetViewport()?.SetInputAsHandled();
			_listeningEntry = null;
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (_listeningEntry == null)
		{
			return;
		}
		StringName[] allControllerInputs = Controller.AllControllerInputs;
		foreach (StringName stringName in allControllerInputs)
		{
			if (inputEvent.IsActionReleased(stringName))
			{
				if (NInputManager.remappableControllerInputs.Contains(_listeningEntry.InputName) && NControllerManager.Instance.ShouldAllowControllerRebinding)
				{
					NInputManager.Instance.ModifyControllerButton(_listeningEntry.InputName, stringName);
				}
				GetViewport()?.SetInputAsHandled();
				_listeningEntry = null;
				break;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnViewportSizeChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAsListeningEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._UnhandledKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName.OnViewportSizeChange && args.Count == 0)
		{
			OnViewportSizeChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChange && args.Count == 0)
		{
			OnVisibilityChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAsListeningEntry && args.Count == 1)
		{
			SetAsListeningEntry(VariantUtils.ConvertTo<NInputSettingsEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledKeyInput && args.Count == 1)
		{
			_UnhandledKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName.OnViewportSizeChange)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChange)
		{
			return true;
		}
		if (method == MethodName.SetAsListeningEntry)
		{
			return true;
		}
		if (method == MethodName._UnhandledKeyInput)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._minPadding)
		{
			_minPadding = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._listeningEntry)
		{
			_listeningEntry = VariantUtils.ConvertTo<NInputSettingsEntry>(in value);
			return true;
		}
		if (name == PropertyName._resetToDefaultButton)
		{
			_resetToDefaultButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._commandHeader)
		{
			_commandHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._keyboardHeader)
		{
			_keyboardHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._controllerHeader)
		{
			_controllerHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._steamInputPrompt)
		{
			_steamInputPrompt = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._minPadding)
		{
			value = VariantUtils.CreateFrom(in _minPadding);
			return true;
		}
		if (name == PropertyName._listeningEntry)
		{
			value = VariantUtils.CreateFrom(in _listeningEntry);
			return true;
		}
		if (name == PropertyName._resetToDefaultButton)
		{
			value = VariantUtils.CreateFrom(in _resetToDefaultButton);
			return true;
		}
		if (name == PropertyName._commandHeader)
		{
			value = VariantUtils.CreateFrom(in _commandHeader);
			return true;
		}
		if (name == PropertyName._keyboardHeader)
		{
			value = VariantUtils.CreateFrom(in _keyboardHeader);
			return true;
		}
		if (name == PropertyName._controllerHeader)
		{
			value = VariantUtils.CreateFrom(in _controllerHeader);
			return true;
		}
		if (name == PropertyName._steamInputPrompt)
		{
			value = VariantUtils.CreateFrom(in _steamInputPrompt);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minPadding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._listeningEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._resetToDefaultButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._commandHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyboardHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamInputPrompt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._minPadding, Variant.From(in _minPadding));
		info.AddProperty(PropertyName._listeningEntry, Variant.From(in _listeningEntry));
		info.AddProperty(PropertyName._resetToDefaultButton, Variant.From(in _resetToDefaultButton));
		info.AddProperty(PropertyName._commandHeader, Variant.From(in _commandHeader));
		info.AddProperty(PropertyName._keyboardHeader, Variant.From(in _keyboardHeader));
		info.AddProperty(PropertyName._controllerHeader, Variant.From(in _controllerHeader));
		info.AddProperty(PropertyName._steamInputPrompt, Variant.From(in _steamInputPrompt));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._minPadding, out var value))
		{
			_minPadding = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._listeningEntry, out var value2))
		{
			_listeningEntry = value2.As<NInputSettingsEntry>();
		}
		if (info.TryGetProperty(PropertyName._resetToDefaultButton, out var value3))
		{
			_resetToDefaultButton = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._commandHeader, out var value4))
		{
			_commandHeader = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._keyboardHeader, out var value5))
		{
			_keyboardHeader = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._controllerHeader, out var value6))
		{
			_controllerHeader = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._steamInputPrompt, out var value7))
		{
			_steamInputPrompt = value7.As<MegaRichTextLabel>();
		}
	}
}
