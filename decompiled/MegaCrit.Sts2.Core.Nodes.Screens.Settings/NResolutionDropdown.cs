using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NResolutionDropdown.cs")]
public class NResolutionDropdown : NSettingsDropdown
{
	public new class MethodName : NSettingsDropdown.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshCurrentlySelectedResolution = "RefreshCurrentlySelectedResolution";

		public static readonly StringName PopulateDropdownItems = "PopulateDropdownItems";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName RefreshEnabled = "RefreshEnabled";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";

		public static readonly StringName DoesResolutionFit = "DoesResolutionFit";
	}

	public new class PropertyName : NSettingsDropdown.PropertyName
	{
		public static readonly StringName _dropdownItemScene = "_dropdownItemScene";

		public static readonly StringName _arrow = "_arrow";
	}

	public new class SignalName : NSettingsDropdown.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _dropdownItemScene;

	private Control _arrow;

	private static Vector2I _currentResolution;

	public static NResolutionDropdown? Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_arrow = GetNode<Control>("%Arrow");
		NGame.Instance.Connect(NGame.SignalName.WindowChange, Callable.From<bool>(OnWindowChange));
		RefreshEnabled();
		RefreshCurrentlySelectedResolution();
		PopulateDropdownItems();
	}

	public void RefreshCurrentlySelectedResolution()
	{
		if (base.IsEnabled)
		{
			_currentResolution = DisplayServer.WindowGetSize();
			_currentOptionLabel.SetTextAutoSize($"{_currentResolution.X} x {_currentResolution.Y}");
		}
	}

	public void PopulateDropdownItems()
	{
		ClearDropdownItems();
		Vector2I boundaryResolution = DisplayServer.ScreenGetSize(SaveManager.Instance.SettingsSave.TargetDisplay);
		foreach (Vector2I resolutionWhite in GetResolutionWhiteList())
		{
			if (DoesResolutionFit(resolutionWhite, boundaryResolution))
			{
				NResolutionDropdownItem nResolutionDropdownItem = _dropdownItemScene.Instantiate<NResolutionDropdownItem>(PackedScene.GenEditState.Disabled);
				_dropdownItems.AddChildSafely(nResolutionDropdownItem);
				nResolutionDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
				nResolutionDropdownItem.Init(resolutionWhite);
			}
		}
		_dropdownItems.GetParent<NDropdownContainer>().RefreshLayout();
	}

	private void OnWindowChange(bool isAutoAspectRatio)
	{
		RefreshEnabled();
		RefreshCurrentlySelectedResolution();
	}

	private void RefreshEnabled()
	{
		if (SaveManager.Instance.SettingsSave.Fullscreen || PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen())
		{
			Disable();
		}
		else
		{
			Enable();
		}
	}

	protected override void OnEnable()
	{
		_currentOptionLabel.Modulate = StsColors.gold;
		_arrow.Visible = true;
		RefreshCurrentlySelectedResolution();
	}

	protected override void OnDisable()
	{
		_currentOptionLabel.SetTextAutoSize("N/A");
		_currentOptionLabel.Modulate = StsColors.gray;
		_arrow.Visible = false;
	}

	private void OnDropdownItemSelected(NDropdownItem nDropdownItem)
	{
		NResolutionDropdownItem nResolutionDropdownItem = (NResolutionDropdownItem)nDropdownItem;
		if (!(nResolutionDropdownItem.resolution == _currentResolution))
		{
			CloseDropdown();
			SaveManager.Instance.SettingsSave.WindowPosition = DisplayServer.WindowGetPosition() - DisplayServer.ScreenGetPosition(SaveManager.Instance.SettingsSave.TargetDisplay);
			SaveManager.Instance.SettingsSave.WindowSize = nResolutionDropdownItem.resolution;
			Log.Info($"Setting window size to {nResolutionDropdownItem.resolution} from dropdown");
			NGame.Instance.ApplyDisplaySettings();
		}
	}

	private static bool DoesResolutionFit(Vector2I resolution, Vector2I boundaryResolution)
	{
		if (resolution.X <= boundaryResolution.X)
		{
			return resolution.Y <= boundaryResolution.Y;
		}
		return false;
	}

	private static List<Vector2I> GetResolutionWhiteList()
	{
		int num = 26;
		List<Vector2I> list = new List<Vector2I>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Vector2I> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new Vector2I(1024, 768);
		num2++;
		span[num2] = new Vector2I(1152, 864);
		num2++;
		span[num2] = new Vector2I(1280, 720);
		num2++;
		span[num2] = new Vector2I(1280, 800);
		num2++;
		span[num2] = new Vector2I(1280, 960);
		num2++;
		span[num2] = new Vector2I(1366, 768);
		num2++;
		span[num2] = new Vector2I(1400, 1050);
		num2++;
		span[num2] = new Vector2I(1440, 900);
		num2++;
		span[num2] = new Vector2I(1440, 1080);
		num2++;
		span[num2] = new Vector2I(1600, 900);
		num2++;
		span[num2] = new Vector2I(1600, 1200);
		num2++;
		span[num2] = new Vector2I(1680, 1050);
		num2++;
		span[num2] = new Vector2I(1856, 1392);
		num2++;
		span[num2] = new Vector2I(1920, 1080);
		num2++;
		span[num2] = new Vector2I(1920, 1200);
		num2++;
		span[num2] = new Vector2I(1920, 1440);
		num2++;
		span[num2] = new Vector2I(2048, 1536);
		num2++;
		span[num2] = new Vector2I(2560, 1080);
		num2++;
		span[num2] = new Vector2I(2560, 1440);
		num2++;
		span[num2] = new Vector2I(2560, 1600);
		num2++;
		span[num2] = new Vector2I(3200, 1800);
		num2++;
		span[num2] = new Vector2I(3440, 1440);
		num2++;
		span[num2] = new Vector2I(3840, 1600);
		num2++;
		span[num2] = new Vector2I(3840, 2160);
		num2++;
		span[num2] = new Vector2I(3840, 2400);
		num2++;
		span[num2] = new Vector2I(7680, 4320);
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshCurrentlySelectedResolution, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PopulateDropdownItems, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isAutoAspectRatio", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "nDropdownItem", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoesResolutionFit, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2I, "resolution", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2I, "boundaryResolution", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.RefreshCurrentlySelectedResolution && args.Count == 0)
		{
			RefreshCurrentlySelectedResolution();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PopulateDropdownItems && args.Count == 0)
		{
			PopulateDropdownItems();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 1)
		{
			OnWindowChange(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshEnabled && args.Count == 0)
		{
			RefreshEnabled();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected && args.Count == 1)
		{
			OnDropdownItemSelected(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoesResolutionFit && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(DoesResolutionFit(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DoesResolutionFit && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(DoesResolutionFit(VariantUtils.ConvertTo<Vector2I>(in args[0]), VariantUtils.ConvertTo<Vector2I>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
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
		if (method == MethodName.RefreshCurrentlySelectedResolution)
		{
			return true;
		}
		if (method == MethodName.PopulateDropdownItems)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.RefreshEnabled)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected)
		{
			return true;
		}
		if (method == MethodName.DoesResolutionFit)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._dropdownItemScene)
		{
			_dropdownItemScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			_arrow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._dropdownItemScene)
		{
			value = VariantUtils.CreateFrom(in _dropdownItemScene);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			value = VariantUtils.CreateFrom(in _arrow);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItemScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownItemScene, Variant.From(in _dropdownItemScene));
		info.AddProperty(PropertyName._arrow, Variant.From(in _arrow));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownItemScene, out var value))
		{
			_dropdownItemScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._arrow, out var value2))
		{
			_arrow = value2.As<Control>();
		}
	}
}
