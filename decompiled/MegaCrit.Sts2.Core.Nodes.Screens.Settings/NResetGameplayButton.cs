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

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NResetGameplayButton.cs")]
public class NResetGameplayButton : NSettingsButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSettingsButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSettingsButton.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
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
		if (!(await nGenericPopup.WaitForConfirmation(new LocString("settings_ui", "RESET_GAMEPLAY_CONFIRMATION.body"), new LocString("settings_ui", "RESET_CONFIRMATION.header"), new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), new LocString("main_menu_ui", "GENERIC_POPUP.confirm"))))
		{
			return;
		}
		Log.Info("Player reset general settings");
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		settingsSave.LimitFpsInBackground = true;
		settingsSave.SkipIntroLogo = false;
		PrefsSave prefsSave = SaveManager.Instance.PrefsSave;
		prefsSave.ScreenShakeOptionIndex = 2;
		prefsSave.FastMode = FastModeType.Normal;
		prefsSave.ShowRunTimer = false;
		prefsSave.ShowCardIndices = false;
		prefsSave.PhobiaMode = false;
		prefsSave.IsLongPressEnabled = false;
		prefsSave.UploadData = true;
		prefsSave.TextEffectsEnabled = true;
		prefsSave.ShowMultiplayerDrawings = true;
		NGame.Instance?.EmitSignal(NGame.SignalName.PhobiaModeToggled);
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
