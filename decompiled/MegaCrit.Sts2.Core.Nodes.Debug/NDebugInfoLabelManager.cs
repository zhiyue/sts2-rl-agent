using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

/// <summary>
/// Manages label displays for debug info both in main menu and in run.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Debug/NDebugInfoLabelManager.cs")]
public class NDebugInfoLabelManager : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'UpdateText' method.
		/// </summary>
		public static readonly StringName UpdateText = "UpdateText";

		/// <summary>
		/// Cached name for the 'OnModdedWarningHovered' method.
		/// </summary>
		public static readonly StringName OnModdedWarningHovered = "OnModdedWarningHovered";

		/// <summary>
		/// Cached name for the 'OnModdedWarningUnhovered' method.
		/// </summary>
		public static readonly StringName OnModdedWarningUnhovered = "OnModdedWarningUnhovered";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'isMainMenu' field.
		/// </summary>
		public static readonly StringName isMainMenu = "isMainMenu";

		/// <summary>
		/// Cached name for the '_releaseInfo' field.
		/// </summary>
		public static readonly StringName _releaseInfo = "_releaseInfo";

		/// <summary>
		/// Cached name for the '_moddedWarning' field.
		/// </summary>
		public static readonly StringName _moddedWarning = "_moddedWarning";

		/// <summary>
		/// Cached name for the '_seed' field.
		/// </summary>
		public static readonly StringName _seed = "_seed";

		/// <summary>
		/// Cached name for the '_modWarningContainer' field.
		/// </summary>
		public static readonly StringName _modWarningContainer = "_modWarningContainer";

		/// <summary>
		/// Cached name for the '_modWarningLabel' field.
		/// </summary>
		public static readonly StringName _modWarningLabel = "_modWarningLabel";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public bool isMainMenu;

	private MegaLabel _releaseInfo;

	private MegaLabel _moddedWarning;

	private MegaLabel? _seed;

	private Control? _modWarningContainer;

	private MegaRichTextLabel? _modWarningLabel;

	public override void _Ready()
	{
		_releaseInfo = GetNode<MegaLabel>("%ReleaseInfo");
		_moddedWarning = GetNode<MegaLabel>("%ModdedWarning");
		_seed = GetNodeOrNull<MegaLabel>("%DebugSeed");
		_modWarningContainer = GetNodeOrNull<Control>("%ModWarningContainer");
		_modWarningLabel = GetNodeOrNull<MegaRichTextLabel>("%ModWarningLabel");
		_moddedWarning.Connect(Control.SignalName.MouseEntered, Callable.From(OnModdedWarningHovered));
		_moddedWarning.Connect(Control.SignalName.MouseExited, Callable.From(OnModdedWarningUnhovered));
		_modWarningContainer?.SetVisible(visible: false);
		UpdateText(null);
		if (ReleaseInfoManager.Instance.ReleaseInfo == null)
		{
			TaskHelper.RunSafely(SetCommitIdInEditor());
		}
	}

	private async Task SetCommitIdInEditor()
	{
		if (GitHelper.ShortCommitIdTask != null)
		{
			UpdateText(await GitHelper.ShortCommitIdTask);
		}
	}

	private void UpdateText(string? commitId)
	{
		ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
		string text = releaseInfo?.Date.ToString("yyyy.MM.dd") ?? "???";
		string text2 = releaseInfo?.Version ?? commitId ?? "NONE";
		if (isMainMenu)
		{
			_releaseInfo.Text = text2 + "\n" + text;
		}
		else
		{
			_releaseInfo.Text = $"[{text2}] ({text})";
		}
		_moddedWarning.Visible = ModManager.IsRunningModded();
		if (!_moddedWarning.Visible)
		{
			return;
		}
		bool flag = ModManager.Mods.Any(delegate(Mod m)
		{
			if (m.state != ModLoadState.Failed)
			{
				List<LocString>? errors = m.errors;
				if (errors == null)
				{
					return false;
				}
				return errors.Count > 0;
			}
			return true;
		});
		if (isMainMenu)
		{
			LocString locString = new LocString("main_menu_ui", "MODDED_WARNING");
			locString.Add("count", ModManager.GetLoadedMods().Count());
			locString.Add("hasError", flag);
			_moddedWarning.SetTextAutoSize(locString.GetFormattedText());
			LocString[] array = ModManager.Mods.SelectMany((Mod m) => m.errors ?? new List<LocString>()).ToArray();
			if (array.Length != 0)
			{
				_modWarningLabel.Text = string.Join("\n", array.Select((LocString s) => s.GetFormattedText()));
			}
			else
			{
				LocString locString2 = new LocString("main_menu_ui", "MOD_ERROR.NONE");
				locString2.Add("mods", string.Join(", ", ModManager.GetLoadedMods().Select(delegate(Mod m)
				{
					object obj = m.manifest?.name;
					if (obj == null)
					{
						ModManifest? manifest = m.manifest;
						if (manifest == null)
						{
							return (string)null;
						}
						obj = manifest.id;
					}
					return (string)obj;
				})));
				_modWarningLabel.Text = locString2.GetFormattedText();
			}
		}
		else
		{
			_moddedWarning.SetTextAutoSize($"MODDED ({ModManager.GetLoadedMods().Count()})");
		}
		if (flag)
		{
			_moddedWarning.Modulate = StsColors.redGlow;
		}
	}

	private void OnModdedWarningHovered()
	{
		if (_modWarningContainer != null)
		{
			_modWarningContainer.Visible = true;
		}
	}

	private void OnModdedWarningUnhovered()
	{
		if (_modWarningContainer != null)
		{
			_modWarningContainer.Visible = false;
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideVersionInfo))
		{
			_releaseInfo.Visible = !_releaseInfo.Visible;
			_moddedWarning.Visible = ModManager.IsRunningModded() && !_moddedWarning.Visible;
			_seed?.SetVisible(!_seed.Visible);
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_releaseInfo.Visible ? "Show Version Info" : "Hide Version Info"));
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "commitId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnModdedWarningHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnModdedWarningUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName.UpdateText && args.Count == 1)
		{
			UpdateText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnModdedWarningHovered && args.Count == 0)
		{
			OnModdedWarningHovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnModdedWarningUnhovered && args.Count == 0)
		{
			OnModdedWarningUnhovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName.UpdateText)
		{
			return true;
		}
		if (method == MethodName.OnModdedWarningHovered)
		{
			return true;
		}
		if (method == MethodName.OnModdedWarningUnhovered)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.isMainMenu)
		{
			isMainMenu = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._releaseInfo)
		{
			_releaseInfo = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._moddedWarning)
		{
			_moddedWarning = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._seed)
		{
			_seed = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._modWarningContainer)
		{
			_modWarningContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._modWarningLabel)
		{
			_modWarningLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.isMainMenu)
		{
			value = VariantUtils.CreateFrom(in isMainMenu);
			return true;
		}
		if (name == PropertyName._releaseInfo)
		{
			value = VariantUtils.CreateFrom(in _releaseInfo);
			return true;
		}
		if (name == PropertyName._moddedWarning)
		{
			value = VariantUtils.CreateFrom(in _moddedWarning);
			return true;
		}
		if (name == PropertyName._seed)
		{
			value = VariantUtils.CreateFrom(in _seed);
			return true;
		}
		if (name == PropertyName._modWarningContainer)
		{
			value = VariantUtils.CreateFrom(in _modWarningContainer);
			return true;
		}
		if (name == PropertyName._modWarningLabel)
		{
			value = VariantUtils.CreateFrom(in _modWarningLabel);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.isMainMenu, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._releaseInfo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moddedWarning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modWarningContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modWarningLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.isMainMenu, Variant.From(in isMainMenu));
		info.AddProperty(PropertyName._releaseInfo, Variant.From(in _releaseInfo));
		info.AddProperty(PropertyName._moddedWarning, Variant.From(in _moddedWarning));
		info.AddProperty(PropertyName._seed, Variant.From(in _seed));
		info.AddProperty(PropertyName._modWarningContainer, Variant.From(in _modWarningContainer));
		info.AddProperty(PropertyName._modWarningLabel, Variant.From(in _modWarningLabel));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.isMainMenu, out var value))
		{
			isMainMenu = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._releaseInfo, out var value2))
		{
			_releaseInfo = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._moddedWarning, out var value3))
		{
			_moddedWarning = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._seed, out var value4))
		{
			_seed = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._modWarningContainer, out var value5))
		{
			_modWarningContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._modWarningLabel, out var value6))
		{
			_modWarningLabel = value6.As<MegaRichTextLabel>();
		}
	}
}
