using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// The disconnection confirmation popup to prevent the player from accidentally leaving a run in progress.
/// Renders above the capstone screens (above top bar).
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NDisconnectConfirmPopup.cs")]
public class NDisconnectConfirmPopup : Control, IScreenContext
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'OnYesButtonPressed' method.
		/// </summary>
		public static readonly StringName OnYesButtonPressed = "OnYesButtonPressed";

		/// <summary>
		/// Cached name for the 'OnNoButtonPressed' method.
		/// </summary>
		public static readonly StringName OnNoButtonPressed = "OnNoButtonPressed";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_header' field.
		/// </summary>
		public static readonly StringName _header = "_header";

		/// <summary>
		/// Cached name for the '_description' field.
		/// </summary>
		public static readonly StringName _description = "_description";

		/// <summary>
		/// Cached name for the '_noButton' field.
		/// </summary>
		public static readonly StringName _noButton = "_noButton";

		/// <summary>
		/// Cached name for the '_yesButton' field.
		/// </summary>
		public static readonly StringName _yesButton = "_yesButton";

		/// <summary>
		/// Cached name for the '_mainMenuNode' field.
		/// </summary>
		public static readonly StringName _mainMenuNode = "_mainMenuNode";

		/// <summary>
		/// Cached name for the '_verticalPopup' field.
		/// </summary>
		public static readonly StringName _verticalPopup = "_verticalPopup";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private MegaLabel _header;

	private MegaRichTextLabel _description;

	private NButton _noButton;

	private NButton _yesButton;

	private NMainMenu? _mainMenuNode;

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/disconnect_confirm_popup");

	private NVerticalPopup _verticalPopup;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public Control? DefaultFocusedControl => null;

	public override void _Ready()
	{
		_verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		_verticalPopup.SetText(new LocString("settings_ui", "DISCONNECT_CONFIRMATION.header"), new LocString("settings_ui", "DISCONNECT_CONFIRMATION.body"));
		_verticalPopup.InitNoButton(new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), OnNoButtonPressed);
		_verticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), OnYesButtonPressed);
	}

	public static NDisconnectConfirmPopup? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDisconnectConfirmPopup>(PackedScene.GenEditState.Disabled);
	}

	private void OnYesButtonPressed(NButton _)
	{
		RunManager.Instance.NetService.Disconnect(NetError.Quit);
		this.QueueFreeSafely();
	}

	private void OnNoButtonPressed(NButton _)
	{
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.OnYesButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnNoButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDisconnectConfirmPopup>(Create());
			return true;
		}
		if (method == MethodName.OnYesButtonPressed && args.Count == 1)
		{
			OnYesButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnNoButtonPressed && args.Count == 1)
		{
			OnNoButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NDisconnectConfirmPopup>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.OnYesButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnNoButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._header)
		{
			_header = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._noButton)
		{
			_noButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._yesButton)
		{
			_yesButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._mainMenuNode)
		{
			_mainMenuNode = VariantUtils.ConvertTo<NMainMenu>(in value);
			return true;
		}
		if (name == PropertyName._verticalPopup)
		{
			_verticalPopup = VariantUtils.ConvertTo<NVerticalPopup>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._header)
		{
			value = VariantUtils.CreateFrom(in _header);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._noButton)
		{
			value = VariantUtils.CreateFrom(in _noButton);
			return true;
		}
		if (name == PropertyName._yesButton)
		{
			value = VariantUtils.CreateFrom(in _yesButton);
			return true;
		}
		if (name == PropertyName._mainMenuNode)
		{
			value = VariantUtils.CreateFrom(in _mainMenuNode);
			return true;
		}
		if (name == PropertyName._verticalPopup)
		{
			value = VariantUtils.CreateFrom(in _verticalPopup);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._header, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._yesButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainMenuNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._header, Variant.From(in _header));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._noButton, Variant.From(in _noButton));
		info.AddProperty(PropertyName._yesButton, Variant.From(in _yesButton));
		info.AddProperty(PropertyName._mainMenuNode, Variant.From(in _mainMenuNode));
		info.AddProperty(PropertyName._verticalPopup, Variant.From(in _verticalPopup));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._header, out var value))
		{
			_header = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value2))
		{
			_description = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._noButton, out var value3))
		{
			_noButton = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._yesButton, out var value4))
		{
			_yesButton = value4.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._mainMenuNode, out var value5))
		{
			_mainMenuNode = value5.As<NMainMenu>();
		}
		if (info.TryGetProperty(PropertyName._verticalPopup, out var value6))
		{
			_verticalPopup = value6.As<NVerticalPopup>();
		}
	}
}
