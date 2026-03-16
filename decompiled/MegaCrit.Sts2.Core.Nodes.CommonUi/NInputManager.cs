using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NInputManager.cs")]
public class NInputManager : Node
{
	[Signal]
	public delegate void InputReboundEventHandler();

	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _UnhandledKeyInput = "_UnhandledKeyInput";

		public static readonly StringName ProcessDebugKeyInput = "ProcessDebugKeyInput";

		public static readonly StringName ProcessShortcutKeyInput = "ProcessShortcutKeyInput";

		public new static readonly StringName _UnhandledInput = "_UnhandledInput";

		public static readonly StringName GetShortcutKey = "GetShortcutKey";

		public static readonly StringName GetHotkeyIcon = "GetHotkeyIcon";

		public static readonly StringName ModifyShortcutKey = "ModifyShortcutKey";

		public static readonly StringName ModifyControllerButton = "ModifyControllerButton";

		public static readonly StringName ResetToDefaults = "ResetToDefaults";

		public static readonly StringName ResetToDefaultControllerMapping = "ResetToDefaultControllerMapping";

		public static readonly StringName OnControllerTypeChanged = "OnControllerTypeChanged";

		public static readonly StringName SaveControllerInputMapping = "SaveControllerInputMapping";

		public static readonly StringName SaveKeyboardInputMapping = "SaveKeyboardInputMapping";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName ControllerManager = "ControllerManager";
	}

	public new class SignalName : Node.SignalName
	{
		public static readonly StringName InputRebound = "InputRebound";
	}

	private readonly Dictionary<Key, StringName> _debugInputMap = new Dictionary<Key, StringName>
	{
		{
			Key.Key1,
			DebugHotkey.hideTopBar
		},
		{
			Key.Key2,
			DebugHotkey.hideIntents
		},
		{
			Key.Key3,
			DebugHotkey.hideCombatUi
		},
		{
			Key.Key4,
			DebugHotkey.hidePlayContainer
		},
		{
			Key.Key5,
			DebugHotkey.hideHand
		},
		{
			Key.Key6,
			DebugHotkey.hideHpBars
		},
		{
			Key.Key7,
			DebugHotkey.hideTextVfx
		},
		{
			Key.Key8,
			DebugHotkey.hideTargetingUi
		},
		{
			Key.Key9,
			DebugHotkey.slowRewards
		},
		{
			Key.Key0,
			DebugHotkey.hideVersionInfo
		},
		{
			Key.Minus,
			DebugHotkey.speedDown
		},
		{
			Key.Equal,
			DebugHotkey.speedUp
		},
		{
			Key.F1,
			DebugHotkey.hideRestSite
		},
		{
			Key.F3,
			DebugHotkey.hideEventUi
		},
		{
			Key.F4,
			DebugHotkey.hideProceedButton
		},
		{
			Key.F5,
			DebugHotkey.hideHoverTips
		},
		{
			Key.F6,
			DebugHotkey.hideMpCursors
		},
		{
			Key.F7,
			DebugHotkey.hideMpTargeting
		},
		{
			Key.F9,
			DebugHotkey.hideMpIntents
		},
		{
			Key.F10,
			DebugHotkey.hideMpHealthBars
		},
		{
			Key.U,
			DebugHotkey.unlockCharacters
		}
	};

	public static readonly IReadOnlyList<StringName> remappableKeyboardInputs = new List<StringName>
	{
		MegaInput.select,
		MegaInput.cancel,
		MegaInput.viewMap,
		MegaInput.viewDeckAndTabLeft,
		MegaInput.viewDrawPile,
		MegaInput.viewDiscardPile,
		MegaInput.viewExhaustPileAndTabRight,
		MegaInput.accept,
		MegaInput.peek,
		MegaInput.up,
		MegaInput.down,
		MegaInput.left,
		MegaInput.right,
		MegaInput.selectCard1,
		MegaInput.selectCard2,
		MegaInput.selectCard3,
		MegaInput.selectCard4,
		MegaInput.selectCard5,
		MegaInput.selectCard6,
		MegaInput.selectCard7,
		MegaInput.selectCard8,
		MegaInput.selectCard9,
		MegaInput.selectCard10,
		MegaInput.releaseCard
	};

	public static readonly IReadOnlyList<StringName> remappableControllerInputs = new List<StringName>
	{
		MegaInput.select,
		MegaInput.cancel,
		MegaInput.viewMap,
		MegaInput.topPanel,
		MegaInput.viewDeckAndTabLeft,
		MegaInput.viewDrawPile,
		MegaInput.viewDiscardPile,
		MegaInput.viewExhaustPileAndTabRight,
		MegaInput.accept,
		MegaInput.peek,
		MegaInput.up,
		MegaInput.down,
		MegaInput.left,
		MegaInput.right
	};

	private Dictionary<StringName, Key> _keyboardInputMap = new Dictionary<StringName, Key>();

	private Dictionary<StringName, StringName> _controllerInputMap = new Dictionary<StringName, StringName>();

	private InputReboundEventHandler backing_InputRebound;

	public static NInputManager? Instance
	{
		get
		{
			if (NGame.Instance == null)
			{
				return null;
			}
			return NGame.Instance.InputManager;
		}
	}

	private static Dictionary<StringName, Key> DefaultKeyboardInputMap => new Dictionary<StringName, Key>
	{
		{
			MegaInput.accept,
			Key.E
		},
		{
			MegaInput.select,
			Key.Enter
		},
		{
			MegaInput.viewDiscardPile,
			Key.S
		},
		{
			MegaInput.viewDeckAndTabLeft,
			Key.D
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			Key.X
		},
		{
			MegaInput.viewDrawPile,
			Key.A
		},
		{
			MegaInput.viewMap,
			Key.M
		},
		{
			MegaInput.cancel,
			Key.Escape
		},
		{
			MegaInput.peek,
			Key.Space
		},
		{
			MegaInput.up,
			Key.Up
		},
		{
			MegaInput.down,
			Key.Down
		},
		{
			MegaInput.left,
			Key.Left
		},
		{
			MegaInput.right,
			Key.Right
		},
		{
			MegaInput.pauseAndBack,
			Key.Escape
		},
		{
			MegaInput.selectCard1,
			Key.Key1
		},
		{
			MegaInput.selectCard2,
			Key.Key2
		},
		{
			MegaInput.selectCard3,
			Key.Key3
		},
		{
			MegaInput.selectCard4,
			Key.Key4
		},
		{
			MegaInput.selectCard5,
			Key.Key5
		},
		{
			MegaInput.selectCard6,
			Key.Key6
		},
		{
			MegaInput.selectCard7,
			Key.Key7
		},
		{
			MegaInput.selectCard8,
			Key.Key8
		},
		{
			MegaInput.selectCard9,
			Key.Key9
		},
		{
			MegaInput.selectCard10,
			Key.Key0
		},
		{
			MegaInput.releaseCard,
			Key.Down
		}
	};

	public NControllerManager ControllerManager { get; private set; }

	public event InputReboundEventHandler InputRebound
	{
		add
		{
			backing_InputRebound = (InputReboundEventHandler)Delegate.Combine(backing_InputRebound, value);
		}
		remove
		{
			backing_InputRebound = (InputReboundEventHandler)Delegate.Remove(backing_InputRebound, value);
		}
	}

	public override void _EnterTree()
	{
		ControllerManager = GetNode<NControllerManager>("%ControllerManager");
	}

	public override void _Ready()
	{
		ControllerManager.Connect(NControllerManager.SignalName.ControllerTypeChanged, Callable.From(OnControllerTypeChanged));
		TaskHelper.RunSafely(Init());
	}

	private async Task Init()
	{
		await ControllerManager.Init();
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (settingsSave.KeyboardMapping.Count > 0)
		{
			_keyboardInputMap = new Dictionary<StringName, Key>();
			foreach (KeyValuePair<string, string> item in settingsSave.KeyboardMapping)
			{
				_keyboardInputMap.Add(item.Key, Enum.Parse<Key>(item.Value));
			}
		}
		else
		{
			_keyboardInputMap = DefaultKeyboardInputMap;
			SaveKeyboardInputMapping();
		}
		if (settingsSave.ControllerMapping.Count > 0 && settingsSave.ControllerMappingType == ControllerManager.ControllerMappingType)
		{
			_controllerInputMap = new Dictionary<StringName, StringName>();
			{
				foreach (KeyValuePair<string, string> item2 in settingsSave.ControllerMapping)
				{
					_controllerInputMap.Add(item2.Key, item2.Value);
				}
				return;
			}
		}
		_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
		SaveControllerInputMapping();
	}

	public override void _UnhandledKeyInput(InputEvent inputEvent)
	{
		ProcessShortcutKeyInput(inputEvent);
		ProcessDebugKeyInput(inputEvent);
	}

	private void ProcessDebugKeyInput(InputEvent inputEvent)
	{
		if (!(inputEvent is InputEventKey inputEventKey) || NDevConsole.Instance.Visible || !NGame.IsTrailerMode)
		{
			return;
		}
		foreach (KeyValuePair<Key, StringName> item in _debugInputMap)
		{
			if (inputEventKey.Keycode == item.Key)
			{
				InputEventAction inputEventAction = new InputEventAction
				{
					Action = item.Value,
					Pressed = inputEvent.IsPressed()
				};
				Input.ParseInputEvent(inputEventAction);
			}
		}
	}

	private void ProcessShortcutKeyInput(InputEvent inputEvent)
	{
		if (NGame.Instance.Transition.InTransition || !(inputEvent is InputEventKey inputEventKey))
		{
			return;
		}
		foreach (KeyValuePair<StringName, Key> item in _keyboardInputMap)
		{
			if (inputEventKey.Keycode == item.Value && !inputEvent.IsEcho())
			{
				InputEventAction inputEventAction = new InputEventAction
				{
					Action = item.Key,
					Pressed = inputEvent.IsPressed()
				};
				Input.ParseInputEvent(inputEventAction);
			}
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (NGame.Instance.Transition.InTransition)
		{
			return;
		}
		foreach (KeyValuePair<StringName, StringName> item in _controllerInputMap)
		{
			if (inputEvent.IsActionPressed(item.Value))
			{
				InputEventAction inputEventAction = new InputEventAction
				{
					Action = item.Key,
					Pressed = true
				};
				Input.ParseInputEvent(inputEventAction);
			}
			else if (inputEvent.IsActionReleased(item.Value))
			{
				InputEventAction inputEventAction2 = new InputEventAction
				{
					Action = item.Key,
					Pressed = false
				};
				Input.ParseInputEvent(inputEventAction2);
			}
		}
	}

	public Key GetShortcutKey(StringName input)
	{
		return _keyboardInputMap[input];
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		if (_controllerInputMap.TryGetValue(hotkey, out StringName value))
		{
			return ControllerManager.GetHotkeyIcon(value);
		}
		return null;
	}

	public void ModifyShortcutKey(StringName input, Key shortcutKey)
	{
		KeyValuePair<StringName, Key> keyValuePair = _keyboardInputMap.FirstOrDefault<KeyValuePair<StringName, Key>>((KeyValuePair<StringName, Key> kvp) => kvp.Value == shortcutKey && remappableKeyboardInputs.Contains(kvp.Key));
		if (keyValuePair.Key != null)
		{
			Key value = _keyboardInputMap[input];
			_keyboardInputMap[keyValuePair.Key] = value;
		}
		_keyboardInputMap[input] = shortcutKey;
		SaveKeyboardInputMapping();
		EmitSignalInputRebound();
	}

	public void ModifyControllerButton(StringName input, StringName controllerInput)
	{
		KeyValuePair<StringName, StringName> keyValuePair = _controllerInputMap.FirstOrDefault<KeyValuePair<StringName, StringName>>((KeyValuePair<StringName, StringName> kvp) => kvp.Value == controllerInput && remappableControllerInputs.Contains(kvp.Key));
		if (keyValuePair.Key != null)
		{
			StringName value = _controllerInputMap[input];
			_controllerInputMap[keyValuePair.Key] = value;
		}
		_controllerInputMap[input] = controllerInput;
		SaveControllerInputMapping();
		EmitSignalInputRebound();
	}

	public void ResetToDefaults()
	{
		_keyboardInputMap = DefaultKeyboardInputMap;
		_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
		SaveControllerInputMapping();
		SaveKeyboardInputMapping();
		EmitSignalInputRebound();
	}

	public void ResetToDefaultControllerMapping()
	{
		_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
		SaveControllerInputMapping();
		EmitSignalInputRebound();
	}

	private void OnControllerTypeChanged()
	{
		if (ControllerManager.ControllerMappingType != SaveManager.Instance.SettingsSave.ControllerMappingType)
		{
			_controllerInputMap = ControllerManager.GetDefaultControllerInputMap;
			SaveControllerInputMapping();
			EmitSignalInputRebound();
		}
	}

	private void SaveControllerInputMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<StringName, StringName> item in _controllerInputMap)
		{
			dictionary.Add(item.Key.ToString(), item.Value.ToString());
		}
		SaveManager.Instance.SettingsSave.ControllerMappingType = ControllerManager.ControllerMappingType;
		SaveManager.Instance.SettingsSave.ControllerMapping = dictionary;
		SaveManager.Instance.SaveSettings();
	}

	private void SaveKeyboardInputMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<StringName, Key> item in _keyboardInputMap)
		{
			dictionary.Add(item.Key.ToString(), item.Value.ToString());
		}
		SaveManager.Instance.SettingsSave.KeyboardMapping = dictionary;
		SaveManager.Instance.SaveSettings();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._UnhandledKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessDebugKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessShortcutKeyInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetShortcutKey, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetHotkeyIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "hotkey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModifyShortcutKey, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "shortcutKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModifyControllerButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "input", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.StringName, "controllerInput", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ResetToDefaults, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetToDefaultControllerMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SaveControllerInputMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SaveKeyboardInputMapping, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledKeyInput && args.Count == 1)
		{
			_UnhandledKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessDebugKeyInput && args.Count == 1)
		{
			ProcessDebugKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessShortcutKeyInput && args.Count == 1)
		{
			ProcessShortcutKeyInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetShortcutKey && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Key>(GetShortcutKey(VariantUtils.ConvertTo<StringName>(in args[0])));
			return true;
		}
		if (method == MethodName.GetHotkeyIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetHotkeyIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ModifyShortcutKey && args.Count == 2)
		{
			ModifyShortcutKey(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<Key>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModifyControllerButton && args.Count == 2)
		{
			ModifyControllerButton(VariantUtils.ConvertTo<StringName>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetToDefaults && args.Count == 0)
		{
			ResetToDefaults();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetToDefaultControllerMapping && args.Count == 0)
		{
			ResetToDefaultControllerMapping();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged && args.Count == 0)
		{
			OnControllerTypeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveControllerInputMapping && args.Count == 0)
		{
			SaveControllerInputMapping();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SaveKeyboardInputMapping && args.Count == 0)
		{
			SaveKeyboardInputMapping();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._UnhandledKeyInput)
		{
			return true;
		}
		if (method == MethodName.ProcessDebugKeyInput)
		{
			return true;
		}
		if (method == MethodName.ProcessShortcutKeyInput)
		{
			return true;
		}
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		if (method == MethodName.GetShortcutKey)
		{
			return true;
		}
		if (method == MethodName.GetHotkeyIcon)
		{
			return true;
		}
		if (method == MethodName.ModifyShortcutKey)
		{
			return true;
		}
		if (method == MethodName.ModifyControllerButton)
		{
			return true;
		}
		if (method == MethodName.ResetToDefaults)
		{
			return true;
		}
		if (method == MethodName.ResetToDefaultControllerMapping)
		{
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged)
		{
			return true;
		}
		if (method == MethodName.SaveControllerInputMapping)
		{
			return true;
		}
		if (method == MethodName.SaveKeyboardInputMapping)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ControllerManager)
		{
			ControllerManager = VariantUtils.ConvertTo<NControllerManager>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ControllerManager)
		{
			value = VariantUtils.CreateFrom<NControllerManager>(ControllerManager);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ControllerManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ControllerManager, Variant.From<NControllerManager>(ControllerManager));
		info.AddSignalEventDelegate(SignalName.InputRebound, backing_InputRebound);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ControllerManager, out var value))
		{
			ControllerManager = value.As<NControllerManager>();
		}
		if (info.TryGetSignalEventDelegate<InputReboundEventHandler>(SignalName.InputRebound, out var value2))
		{
			backing_InputRebound = value2;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.InputRebound, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalInputRebound()
	{
		EmitSignal(SignalName.InputRebound);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.InputRebound && args.Count == 0)
		{
			backing_InputRebound?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.InputRebound)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
