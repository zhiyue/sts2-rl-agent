using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NResetGraphicsButton.cs")]
public class NResetGraphicsButton : NSettingsButton
{
	public new class MethodName : NSettingsButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnRelease = "OnRelease";
	}

	public new class PropertyName : NSettingsButton.PropertyName
	{
	}

	public new class SignalName : NSettingsButton.SignalName
	{
	}

	public override void _Ready()
	{
		ConnectSignals();
		base.PivotOffset = base.Size * 0.5f;
		GetNode<MegaLabel>("Label").SetTextAutoSize(new LocString("settings_ui", "RESET_SETTINGS_BUTTON").GetFormattedText());
	}

	private async Task ResetSettingsAfterConfirmation()
	{
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		if (!(await nGenericPopup.WaitForConfirmation(new LocString("settings_ui", "RESET_GRAPHICS_CONFIRMATION.body"), new LocString("settings_ui", "RESET_CONFIRMATION.header"), new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), new LocString("main_menu_ui", "GENERIC_POPUP.confirm"))))
		{
			return;
		}
		Log.Info("Player reset graphics settings");
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		settingsSave.FpsLimit = 60;
		settingsSave.WindowPosition = new Vector2I(-1, -1);
		settingsSave.WindowSize = new Vector2I(1920, 1080);
		settingsSave.Fullscreen = true;
		settingsSave.AspectRatioSetting = AspectRatioSetting.SixteenByNine;
		settingsSave.TargetDisplay = -1;
		settingsSave.ResizeWindows = true;
		settingsSave.VSync = VSyncType.Adaptive;
		settingsSave.Msaa = 2;
		NGame.Instance.ApplyDisplaySettings();
		NSettingsPanel ancestorOfType = this.GetAncestorOfType<NSettingsPanel>();
		foreach (IResettableSettingNode item in ancestorOfType.GetChildrenRecursive<IResettableSettingNode>())
		{
			item.SetFromSettings();
		}
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		TaskHelper.RunSafely(ResetSettingsAfterConfirmation());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
