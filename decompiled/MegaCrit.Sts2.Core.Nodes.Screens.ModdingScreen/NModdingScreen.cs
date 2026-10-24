using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

[ScriptPath("res://src/Core/Nodes/Screens/ModdingScreen/NModdingScreen.cs")]
public class NModdingScreen : NSubmenu
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSubmenu.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnGetModsPressed' method.
		/// </summary>
		public static readonly StringName OnGetModsPressed = "OnGetModsPressed";

		/// <summary>
		/// Cached name for the 'OnMakeModsPressed' method.
		/// </summary>
		public static readonly StringName OnMakeModsPressed = "OnMakeModsPressed";

		/// <summary>
		/// Cached name for the 'OnRowSelected' method.
		/// </summary>
		public static readonly StringName OnRowSelected = "OnRowSelected";

		/// <summary>
		/// Cached name for the 'OnModEnabledOrDisabled' method.
		/// </summary>
		public static readonly StringName OnModEnabledOrDisabled = "OnModEnabledOrDisabled";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSubmenu.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the '_modInfoContainer' field.
		/// </summary>
		public static readonly StringName _modInfoContainer = "_modInfoContainer";

		/// <summary>
		/// Cached name for the '_modRowContainer' field.
		/// </summary>
		public static readonly StringName _modRowContainer = "_modRowContainer";

		/// <summary>
		/// Cached name for the '_pendingChangesWarning' field.
		/// </summary>
		public static readonly StringName _pendingChangesWarning = "_pendingChangesWarning";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/modding/modding_screen");

	private NModInfoContainer _modInfoContainer;

	private Control _modRowContainer;

	private Control _pendingChangesWarning;

	protected override Control? InitialFocusedControl => null;

	public static string[] AssetPaths => new string[1] { _scenePath };

	public static NModdingScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NModdingScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_modInfoContainer = GetNode<NModInfoContainer>("%ModInfoContainer");
		_modRowContainer = GetNode<Control>("%ModsScrollContainer/Mask/Content");
		_pendingChangesWarning = GetNode<Control>("%PendingChangesLabel");
		NButton node = GetNode<NButton>("%GetModsButton");
		NButton node2 = GetNode<NButton>("%MakeModsButton");
		foreach (Node child in _modRowContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (Mod mod in ModManager.Mods)
		{
			OnNewModDetected(mod);
		}
		node.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnGetModsPressed));
		node2.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnMakeModsPressed));
		node.GetNode<MegaLabel>("Visuals/Label").SetTextAutoSize(new LocString("settings_ui", "MODDING_SCREEN.GET_MODS_BUTTON").GetFormattedText());
		node2.GetNode<MegaLabel>("Visuals/Label").SetTextAutoSize(new LocString("settings_ui", "MODDING_SCREEN.MAKE_MODS_BUTTON").GetFormattedText());
		GetNode<MegaRichTextLabel>("%InstalledModsTitle").SetTextAutoSize(new LocString("settings_ui", "MODDING_SCREEN.INSTALLED_MODS_TITLE").GetFormattedText());
		GetNode<MegaRichTextLabel>("%PendingChangesLabel").SetTextAutoSize(new LocString("settings_ui", "MODDING_SCREEN.PENDING_CHANGES_WARNING").GetFormattedText());
		node2.Visible = false;
		node.Visible = false;
		_pendingChangesWarning.Visible = false;
		ModManager.OnModDetected += OnNewModDetected;
		ConnectSignals();
	}

	public override void OnSubmenuOpened()
	{
		if (!ModManager.PlayerAgreedToModLoading && ModManager.Mods.Count > 0)
		{
			NModalContainer.Instance.Add(NConfirmModLoadingPopup.Create());
		}
	}

	private void OnGetModsPressed(NButton _)
	{
		PlatformUtil.OpenUrl("https://steamcommunity.com/app/2868840/workshop/");
	}

	private void OnMakeModsPressed(NButton _)
	{
		PlatformUtil.OpenUrl("https://gitlab.com/megacrit/sts2/example-mod/-/wikis/home");
	}

	public void OnRowSelected(NModMenuRow row)
	{
		row.SetSelected(isSelected: true);
		_modInfoContainer.Fill(row.Mod);
		foreach (NModMenuRow item in _modRowContainer.GetChildren().OfType<NModMenuRow>())
		{
			if (item != row)
			{
				item.SetSelected(isSelected: false);
			}
		}
	}

	/// <summary>
	/// Called both during initialization and when a new mod is installed during runtime.
	/// </summary>
	private void OnNewModDetected(Mod mod)
	{
		NModMenuRow child = NModMenuRow.Create(this, mod);
		_modRowContainer.AddChildSafely(child);
		OnModEnabledOrDisabled();
	}

	public void OnModEnabledOrDisabled()
	{
		foreach (Mod mod in ModManager.Mods)
		{
			bool flag = SaveManager.Instance.SettingsSave.ModSettings?.IsModDisabled(mod) ?? false;
			if ((mod.state == ModLoadState.Disabled && !flag) || (mod.state == ModLoadState.Loaded && flag))
			{
				_pendingChangesWarning.Visible = true;
				return;
			}
		}
		_pendingChangesWarning.Visible = false;
	}

	public override void _ExitTree()
	{
		ModManager.OnModDetected -= OnNewModDetected;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnGetModsPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMakeModsPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRowSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "row", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnModEnabledOrDisabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NModdingScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGetModsPressed && args.Count == 1)
		{
			OnGetModsPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMakeModsPressed && args.Count == 1)
		{
			OnMakeModsPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRowSelected && args.Count == 1)
		{
			OnRowSelected(VariantUtils.ConvertTo<NModMenuRow>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnModEnabledOrDisabled && args.Count == 0)
		{
			OnModEnabledOrDisabled();
			ret = default(godot_variant);
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
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NModdingScreen>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
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
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnGetModsPressed)
		{
			return true;
		}
		if (method == MethodName.OnMakeModsPressed)
		{
			return true;
		}
		if (method == MethodName.OnRowSelected)
		{
			return true;
		}
		if (method == MethodName.OnModEnabledOrDisabled)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._modInfoContainer)
		{
			_modInfoContainer = VariantUtils.ConvertTo<NModInfoContainer>(in value);
			return true;
		}
		if (name == PropertyName._modRowContainer)
		{
			_modRowContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._pendingChangesWarning)
		{
			_pendingChangesWarning = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._modInfoContainer)
		{
			value = VariantUtils.CreateFrom(in _modInfoContainer);
			return true;
		}
		if (name == PropertyName._modRowContainer)
		{
			value = VariantUtils.CreateFrom(in _modRowContainer);
			return true;
		}
		if (name == PropertyName._pendingChangesWarning)
		{
			value = VariantUtils.CreateFrom(in _pendingChangesWarning);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modInfoContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modRowContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pendingChangesWarning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._modInfoContainer, Variant.From(in _modInfoContainer));
		info.AddProperty(PropertyName._modRowContainer, Variant.From(in _modRowContainer));
		info.AddProperty(PropertyName._pendingChangesWarning, Variant.From(in _pendingChangesWarning));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._modInfoContainer, out var value))
		{
			_modInfoContainer = value.As<NModInfoContainer>();
		}
		if (info.TryGetProperty(PropertyName._modRowContainer, out var value2))
		{
			_modRowContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._pendingChangesWarning, out var value3))
		{
			_pendingChangesWarning = value3.As<Control>();
		}
	}
}
