using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NInputSettingsEntry.cs")]
public class NInputSettingsEntry : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateInput = "UpdateInput";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName InputName = "InputName";

		public static readonly StringName _bg = "_bg";

		public static readonly StringName _inputLabel = "_inputLabel";

		public static readonly StringName _keyBindingLabel = "_keyBindingLabel";

		public static readonly StringName _controllerBindingIcon = "_controllerBindingIcon";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly Dictionary<StringName, string> _commandToLocTitle = new Dictionary<StringName, string>
	{
		{
			MegaInput.accept,
			"endTurn"
		},
		{
			MegaInput.select,
			"confirmCard"
		},
		{
			MegaInput.viewDiscardPile,
			"viewDiscard"
		},
		{
			MegaInput.viewDrawPile,
			"viewDraw"
		},
		{
			MegaInput.viewDeckAndTabLeft,
			"viewDeck"
		},
		{
			MegaInput.viewExhaustPileAndTabRight,
			"viewExhaust"
		},
		{
			MegaInput.viewMap,
			"viewMap"
		},
		{
			MegaInput.cancel,
			"cancel"
		},
		{
			MegaInput.peek,
			"peek"
		},
		{
			MegaInput.up,
			"up"
		},
		{
			MegaInput.topPanel,
			"topPanel"
		},
		{
			MegaInput.down,
			"down"
		},
		{
			MegaInput.left,
			"left"
		},
		{
			MegaInput.right,
			"right"
		},
		{
			MegaInput.selectCard1,
			"selectCard1"
		},
		{
			MegaInput.selectCard2,
			"selectCard2"
		},
		{
			MegaInput.selectCard3,
			"selectCard3"
		},
		{
			MegaInput.selectCard4,
			"selectCard4"
		},
		{
			MegaInput.selectCard5,
			"selectCard5"
		},
		{
			MegaInput.selectCard6,
			"selectCard6"
		},
		{
			MegaInput.selectCard7,
			"selectCard7"
		},
		{
			MegaInput.selectCard8,
			"selectCard8"
		},
		{
			MegaInput.selectCard9,
			"selectCard9"
		},
		{
			MegaInput.selectCard10,
			"selectCard10"
		},
		{
			MegaInput.releaseCard,
			"releaseCard"
		}
	};

	private const string _scenePath = "res://scenes/screens/settings_screen/input_settings_entry.tscn";

	private Control _bg;

	private MegaRichTextLabel _inputLabel;

	private MegaRichTextLabel _keyBindingLabel;

	private TextureRect _controllerBindingIcon;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/screens/settings_screen/input_settings_entry.tscn");

	public StringName InputName { get; private set; }

	public static NInputSettingsEntry Create(string commandName)
	{
		NInputSettingsEntry nInputSettingsEntry = ResourceLoader.Load<PackedScene>("res://scenes/screens/settings_screen/input_settings_entry.tscn", null, ResourceLoader.CacheMode.Reuse).Instantiate<NInputSettingsEntry>(PackedScene.GenEditState.Disabled);
		nInputSettingsEntry.InputName = commandName;
		return nInputSettingsEntry;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_inputLabel = GetNode<MegaRichTextLabel>("%InputLabel");
		_keyBindingLabel = GetNode<MegaRichTextLabel>("%KeyBindingInputLabel");
		_controllerBindingIcon = GetNode<TextureRect>("%ControllerBindingIcon");
		_bg = GetNode<Control>("%Bg");
		string text = _commandToLocTitle[InputName];
		_inputLabel.Text = new LocString("settings_ui", "INPUT_SETTINGS.INPUT_TITLE." + text).GetFormattedText();
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateInput));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateInput));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateInput));
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(UpdateInput));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		NInputManager.Instance.Disconnect(NInputManager.SignalName.InputRebound, Callable.From(UpdateInput));
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateInput));
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateInput));
		Disconnect(CanvasItem.SignalName.VisibilityChanged, Callable.From(UpdateInput));
	}

	private void UpdateInput()
	{
		if (IsVisibleInTree())
		{
			if (NInputManager.remappableKeyboardInputs.Contains(InputName))
			{
				_keyBindingLabel.Text = NInputManager.Instance.GetShortcutKey(InputName).ToString();
			}
			else
			{
				_keyBindingLabel.Text = "";
			}
			if (NInputManager.remappableControllerInputs.Contains(InputName))
			{
				_controllerBindingIcon.Texture = NInputManager.Instance.GetHotkeyIcon(InputName);
			}
			_controllerBindingIcon.Modulate = (NControllerManager.Instance.ShouldAllowControllerRebinding ? Colors.White : new Color(1f, 1f, 1f, 0.15f));
		}
	}

	protected override void OnFocus()
	{
		_bg.Visible = true;
	}

	protected override void OnUnfocus()
	{
		_bg.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "commandName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NInputSettingsEntry>(Create(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateInput && args.Count == 0)
		{
			UpdateInput();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NInputSettingsEntry>(Create(VariantUtils.ConvertTo<string>(in args[0])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.UpdateInput)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InputName)
		{
			InputName = VariantUtils.ConvertTo<StringName>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._inputLabel)
		{
			_inputLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._keyBindingLabel)
		{
			_keyBindingLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._controllerBindingIcon)
		{
			_controllerBindingIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InputName)
		{
			value = VariantUtils.CreateFrom<StringName>(InputName);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		if (name == PropertyName._inputLabel)
		{
			value = VariantUtils.CreateFrom(in _inputLabel);
			return true;
		}
		if (name == PropertyName._keyBindingLabel)
		{
			value = VariantUtils.CreateFrom(in _keyBindingLabel);
			return true;
		}
		if (name == PropertyName._controllerBindingIcon)
		{
			value = VariantUtils.CreateFrom(in _controllerBindingIcon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.StringName, PropertyName.InputName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._keyBindingLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerBindingIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.InputName, Variant.From<StringName>(InputName));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
		info.AddProperty(PropertyName._inputLabel, Variant.From(in _inputLabel));
		info.AddProperty(PropertyName._keyBindingLabel, Variant.From(in _keyBindingLabel));
		info.AddProperty(PropertyName._controllerBindingIcon, Variant.From(in _controllerBindingIcon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InputName, out var value))
		{
			InputName = value.As<StringName>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value2))
		{
			_bg = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._inputLabel, out var value3))
		{
			_inputLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._keyBindingLabel, out var value4))
		{
			_keyBindingLabel = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._controllerBindingIcon, out var value5))
		{
			_controllerBindingIcon = value5.As<TextureRect>();
		}
	}
}
