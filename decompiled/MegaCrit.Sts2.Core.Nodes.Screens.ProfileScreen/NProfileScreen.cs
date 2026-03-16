using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ProfileScreen;

[ScriptPath("res://src/Core/Nodes/Screens/ProfileScreen/NProfileScreen.cs")]
public class NProfileScreen : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public static readonly StringName ShowLoading = "ShowLoading";

		public static readonly StringName Refresh = "Refresh";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _loadingOverlay = "_loadingOverlay";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	public static int? forceShowProfileAsDeleted;

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/profiles/profile_screen");

	private Control _loadingOverlay;

	private readonly List<NProfileButton> _profileButtons = new List<NProfileButton>();

	private readonly List<NDeleteProfileButton> _deleteButtons = new List<NDeleteProfileButton>();

	public static string[] AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_scenePath);
			list.AddRange(NProfileButton.AssetPaths);
			return list.ToArray();
		}
	}

	protected override Control InitialFocusedControl => _profileButtons[SaveManager.Instance.CurrentProfileId - 1];

	public static NProfileScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NProfileScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_loadingOverlay = GetNode<Control>("%LoadingOverlay");
		GetNode<MegaLabel>("%ChooseProfileMessage").SetTextAutoSize(new LocString("main_menu_ui", "PROFILE_SCREEN.BUTTON.chooseProfileMessage").GetFormattedText());
		_profileButtons.Add(GetNode<NProfileButton>("%ProfileButton1"));
		_profileButtons.Add(GetNode<NProfileButton>("%ProfileButton2"));
		_profileButtons.Add(GetNode<NProfileButton>("%ProfileButton3"));
		_deleteButtons.Add(GetNode<NDeleteProfileButton>("%DeleteProfileButton1"));
		_deleteButtons.Add(GetNode<NDeleteProfileButton>("%DeleteProfileButton2"));
		_deleteButtons.Add(GetNode<NDeleteProfileButton>("%DeleteProfileButton3"));
		if (_profileButtons.Count != 3)
		{
			Log.Error($"There are {_profileButtons.Count} profile buttons, but max profile count in ProfileSaveManager is {3}! This might result in subtle bugs");
		}
		for (int i = 0; i < _profileButtons.Count; i++)
		{
			_profileButtons[i].FocusNeighborTop = _profileButtons[i].GetPath();
			_profileButtons[i].FocusNeighborBottom = _deleteButtons[i].GetPath();
			NProfileButton nProfileButton = _profileButtons[i];
			NodePath path;
			if (i <= 0)
			{
				List<NProfileButton> profileButtons = _profileButtons;
				path = profileButtons[profileButtons.Count - 1].GetPath();
			}
			else
			{
				path = _profileButtons[i - 1].GetPath();
			}
			nProfileButton.FocusNeighborLeft = path;
			_profileButtons[i].FocusNeighborRight = ((i < _profileButtons.Count - 1) ? _profileButtons[i + 1].GetPath() : _profileButtons[0].GetPath());
			_deleteButtons[i].FocusNeighborTop = _profileButtons[i].GetPath();
			_deleteButtons[i].FocusNeighborBottom = _deleteButtons[i].GetPath();
			NDeleteProfileButton nDeleteProfileButton = _deleteButtons[i];
			NodePath path2;
			if (i <= 0)
			{
				List<NDeleteProfileButton> deleteButtons = _deleteButtons;
				path2 = deleteButtons[deleteButtons.Count - 1].GetPath();
			}
			else
			{
				path2 = _deleteButtons[i - 1].GetPath();
			}
			nDeleteProfileButton.FocusNeighborLeft = path2;
			_deleteButtons[i].FocusNeighborRight = ((i < _deleteButtons.Count - 1) ? _deleteButtons[i + 1].GetPath() : _deleteButtons[0].GetPath());
		}
	}

	public override void OnSubmenuOpened()
	{
		Refresh();
	}

	public void ShowLoading()
	{
		_loadingOverlay.Visible = true;
	}

	public void Refresh()
	{
		for (int i = 0; i < _profileButtons.Count; i++)
		{
			_profileButtons[i].Initialize(this, i + 1);
		}
		for (int j = 0; j < _deleteButtons.Count; j++)
		{
			_deleteButtons[j].Initialize(this, j + 1);
		}
		forceShowProfileAsDeleted = null;
		ActiveScreenContext.Instance.Update();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowLoading, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NProfileScreen>(Create());
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
		if (method == MethodName.ShowLoading && args.Count == 0)
		{
			ShowLoading();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Refresh && args.Count == 0)
		{
			Refresh();
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
			ret = VariantUtils.CreateFrom<NProfileScreen>(Create());
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
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.ShowLoading)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._loadingOverlay)
		{
			_loadingOverlay = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._loadingOverlay)
		{
			value = VariantUtils.CreateFrom(in _loadingOverlay);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._loadingOverlay, Variant.From(in _loadingOverlay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._loadingOverlay, out var value))
		{
			_loadingOverlay = value.As<Control>();
		}
	}
}
