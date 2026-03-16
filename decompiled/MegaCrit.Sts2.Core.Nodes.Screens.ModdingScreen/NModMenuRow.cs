using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

[ScriptPath("res://src/Core/Nodes/Screens/ModdingScreen/NModMenuRow.cs")]
public class NModMenuRow : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName SetSelected = "SetSelected";

		public static readonly StringName OnTickboxToggled = "OnTickboxToggled";

		public static readonly StringName GetPlatformIcon = "GetPlatformIcon";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _selectionHighlight = "_selectionHighlight";

		public static readonly StringName _tickbox = "_tickbox";

		public static readonly StringName _screen = "_screen";

		public static readonly StringName _isSelected = "_isSelected";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/modding/modding_screen_row");

	private const float _selectedAlpha = 0.25f;

	private Panel _selectionHighlight;

	private NTickbox _tickbox;

	private NModdingScreen _screen;

	private bool _isSelected;

	public Mod? Mod { get; private set; }

	public static NModMenuRow? Create(NModdingScreen screen, Mod mod)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NModMenuRow nModMenuRow = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NModMenuRow>(PackedScene.GenEditState.Disabled);
		nModMenuRow.Mod = mod;
		nModMenuRow._screen = screen;
		return nModMenuRow;
	}

	public override void _Ready()
	{
		if (Mod != null)
		{
			_selectionHighlight = GetNode<Panel>("SelectionHighlight");
			NTickbox node = GetNode<NTickbox>("Tickbox");
			MegaRichTextLabel node2 = GetNode<MegaRichTextLabel>("Title");
			TextureRect node3 = GetNode<TextureRect>("PlatformIcon");
			Panel selectionHighlight = _selectionHighlight;
			Color modulate = _selectionHighlight.Modulate;
			modulate.A = 0f;
			selectionHighlight.Modulate = modulate;
			node.IsTicked = !(SaveManager.Instance.SettingsSave.ModSettings?.IsModDisabled(Mod) ?? false);
			node.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(OnTickboxToggled));
			node2.Text = Mod.manifest?.name ?? Mod.pckName;
			node3.Texture = GetPlatformIcon(Mod.modSource);
			node2.Modulate = (Mod.wasLoaded ? Colors.White : StsColors.gray);
			node3.Modulate = (Mod.wasLoaded ? Colors.White : StsColors.gray);
			ConnectSignals();
		}
	}

	protected override void OnFocus()
	{
		if (!_isSelected)
		{
			Panel selectionHighlight = _selectionHighlight;
			Color darkBlue = StsColors.darkBlue;
			darkBlue.A = 0.25f;
			selectionHighlight.Modulate = darkBlue;
		}
	}

	protected override void OnUnfocus()
	{
		if (!_isSelected)
		{
			_selectionHighlight.Modulate = Colors.Transparent;
		}
	}

	protected override void OnRelease()
	{
		_screen.OnRowSelected(this);
	}

	public void SetSelected(bool isSelected)
	{
		if (_isSelected != isSelected)
		{
			_isSelected = isSelected;
			if (_isSelected)
			{
				Panel selectionHighlight = _selectionHighlight;
				Color blue = StsColors.blue;
				blue.A = 0.25f;
				selectionHighlight.Modulate = blue;
			}
			else if (base.IsFocused)
			{
				Panel selectionHighlight2 = _selectionHighlight;
				Color blue = StsColors.darkBlue;
				blue.A = 0.25f;
				selectionHighlight2.Modulate = blue;
			}
			else
			{
				_selectionHighlight.Modulate = Colors.Transparent;
			}
		}
	}

	private void OnTickboxToggled(NTickbox tickbox)
	{
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (settingsSave.ModSettings == null)
		{
			ModSettings modSettings = (settingsSave.ModSettings = new ModSettings());
		}
		if (tickbox.IsTicked)
		{
			SaveManager.Instance.SettingsSave.ModSettings.DisabledMods.RemoveAll((DisabledMod m) => m.Name == Mod.pckName && m.Source == Mod.modSource);
		}
		else
		{
			SaveManager.Instance.SettingsSave.ModSettings.DisabledMods.Add(new DisabledMod(Mod));
		}
		_screen.OnModEnabledOrDisabled();
	}

	public static Texture2D GetPlatformIcon(ModSource modSource)
	{
		AssetCache cache = PreloadManager.Cache;
		return cache.GetTexture2D(modSource switch
		{
			ModSource.ModsDirectory => ImageHelper.GetImagePath("ui/mods/folder.png"), 
			ModSource.SteamWorkshop => ImageHelper.GetImagePath("ui/mods/steam_logo.png"), 
			_ => throw new ArgumentOutOfRangeException("modSource", modSource, null), 
		});
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isSelected", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnTickboxToggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetPlatformIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "modSource", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSelected && args.Count == 1)
		{
			SetSelected(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTickboxToggled && args.Count == 1)
		{
			OnTickboxToggled(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetPlatformIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetPlatformIcon(VariantUtils.ConvertTo<ModSource>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetPlatformIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetPlatformIcon(VariantUtils.ConvertTo<ModSource>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.SetSelected)
		{
			return true;
		}
		if (method == MethodName.OnTickboxToggled)
		{
			return true;
		}
		if (method == MethodName.GetPlatformIcon)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._selectionHighlight)
		{
			_selectionHighlight = VariantUtils.ConvertTo<Panel>(in value);
			return true;
		}
		if (name == PropertyName._tickbox)
		{
			_tickbox = VariantUtils.ConvertTo<NTickbox>(in value);
			return true;
		}
		if (name == PropertyName._screen)
		{
			_screen = VariantUtils.ConvertTo<NModdingScreen>(in value);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			_isSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._selectionHighlight)
		{
			value = VariantUtils.CreateFrom(in _selectionHighlight);
			return true;
		}
		if (name == PropertyName._tickbox)
		{
			value = VariantUtils.CreateFrom(in _tickbox);
			return true;
		}
		if (name == PropertyName._screen)
		{
			value = VariantUtils.CreateFrom(in _screen);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			value = VariantUtils.CreateFrom(in _isSelected);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionHighlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tickbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._selectionHighlight, Variant.From(in _selectionHighlight));
		info.AddProperty(PropertyName._tickbox, Variant.From(in _tickbox));
		info.AddProperty(PropertyName._screen, Variant.From(in _screen));
		info.AddProperty(PropertyName._isSelected, Variant.From(in _isSelected));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._selectionHighlight, out var value))
		{
			_selectionHighlight = value.As<Panel>();
		}
		if (info.TryGetProperty(PropertyName._tickbox, out var value2))
		{
			_tickbox = value2.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._screen, out var value3))
		{
			_screen = value3.As<NModdingScreen>();
		}
		if (info.TryGetProperty(PropertyName._isSelected, out var value4))
		{
			_isSelected = value4.As<bool>();
		}
	}
}
