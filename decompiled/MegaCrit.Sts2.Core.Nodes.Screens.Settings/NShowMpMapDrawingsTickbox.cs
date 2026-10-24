using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NShowMpMapDrawingsTickbox.cs")]
public class NShowMpMapDrawingsTickbox : NSettingsTickbox, IResettableSettingNode
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSettingsTickbox.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetFromSettings' method.
		/// </summary>
		public static readonly StringName SetFromSettings = "SetFromSettings";

		/// <summary>
		/// Cached name for the 'OnTick' method.
		/// </summary>
		public new static readonly StringName OnTick = "OnTick";

		/// <summary>
		/// Cached name for the 'OnUntick' method.
		/// </summary>
		public new static readonly StringName OnUntick = "OnUntick";

		/// <summary>
		/// Cached name for the 'TryRefreshMapDrawings' method.
		/// </summary>
		public static readonly StringName TryRefreshMapDrawings = "TryRefreshMapDrawings";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSettingsTickbox.PropertyName
	{
		/// <summary>
		/// Cached name for the '_settingsScreen' field.
		/// </summary>
		public new static readonly StringName _settingsScreen = "_settingsScreen";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSettingsTickbox.SignalName
	{
	}

	private NSettingsScreen _settingsScreen;

	public override void _Ready()
	{
		ConnectSignals();
		_settingsScreen = this.GetAncestorOfType<NSettingsScreen>();
		SetFromSettings();
	}

	public void SetFromSettings()
	{
		base.IsTicked = SaveManager.Instance.PrefsSave.ShowMultiplayerDrawings;
	}

	protected override void OnTick()
	{
		_settingsScreen.ShowToast(new LocString("settings_ui", "TOAST_SHOW_MP_DRAWINGS_ON"));
		SaveManager.Instance.PrefsSave.ShowMultiplayerDrawings = true;
		TryRefreshMapDrawings();
	}

	protected override void OnUntick()
	{
		_settingsScreen.ShowToast(new LocString("settings_ui", "TOAST_SHOW_MP_DRAWINGS_OFF"));
		SaveManager.Instance.PrefsSave.ShowMultiplayerDrawings = false;
		TryRefreshMapDrawings();
	}

	private static void TryRefreshMapDrawings()
	{
		if (NMapScreen.Instance != null)
		{
			NMapScreen.Instance.Drawings.UpdateVisibilityFromSettings();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFromSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUntick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryRefreshMapDrawings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
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
		if (method == MethodName.SetFromSettings && args.Count == 0)
		{
			SetFromSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTick && args.Count == 0)
		{
			OnTick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUntick && args.Count == 0)
		{
			OnUntick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryRefreshMapDrawings && args.Count == 0)
		{
			TryRefreshMapDrawings();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.TryRefreshMapDrawings && args.Count == 0)
		{
			TryRefreshMapDrawings();
			ret = default(godot_variant);
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
		if (method == MethodName.SetFromSettings)
		{
			return true;
		}
		if (method == MethodName.OnTick)
		{
			return true;
		}
		if (method == MethodName.OnUntick)
		{
			return true;
		}
		if (method == MethodName.TryRefreshMapDrawings)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._settingsScreen)
		{
			_settingsScreen = VariantUtils.ConvertTo<NSettingsScreen>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._settingsScreen)
		{
			value = VariantUtils.CreateFrom(in _settingsScreen);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._settingsScreen, Variant.From(in _settingsScreen));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._settingsScreen, out var value))
		{
			_settingsScreen = value.As<NSettingsScreen>();
		}
	}
}
