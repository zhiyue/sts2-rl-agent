using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NSettingsScreenPopup.cs")]
public class NSettingsScreenPopup : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnYesButtonPressed = "OnYesButtonPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName VerticalPopup = "VerticalPopup";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _backstop = "_backstop";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/settings_screen_popup");

	private ColorRect _backstop;

	private LocString _header;

	private LocString _description;

	public NVerticalPopup VerticalPopup { get; private set; }

	public Control? DefaultFocusedControl => null;

	public static NSettingsScreenPopup? Create(LocString header, LocString description)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSettingsScreenPopup nSettingsScreenPopup = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSettingsScreenPopup>(PackedScene.GenEditState.Disabled);
		nSettingsScreenPopup._header = header;
		nSettingsScreenPopup._description = description;
		return nSettingsScreenPopup;
	}

	public override void _Ready()
	{
		VerticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		VerticalPopup.InitNoButton(new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), delegate
		{
		});
		VerticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), OnYesButtonPressed);
		VerticalPopup.SetText(_header, _description);
	}

	private void OnYesButtonPressed(NButton _)
	{
		Log.Info("FTUEs have been reset!");
		SaveManager.Instance.ResetFtues();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnYesButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.OnYesButtonPressed && args.Count == 1)
		{
			OnYesButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName.OnYesButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VerticalPopup)
		{
			VerticalPopup = VariantUtils.ConvertTo<NVerticalPopup>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.VerticalPopup)
		{
			value = VariantUtils.CreateFrom<NVerticalPopup>(VerticalPopup);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VerticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.VerticalPopup, Variant.From<NVerticalPopup>(VerticalPopup));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VerticalPopup, out var value))
		{
			VerticalPopup = value.As<NVerticalPopup>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value2))
		{
			_backstop = value2.As<ColorRect>();
		}
	}
}
