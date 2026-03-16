using System;
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

[ScriptPath("res://src/Core/Nodes/Debug/NDebugInfoLabelManager.cs")]
public class NDebugInfoLabelManager : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName UpdateText = "UpdateText";

		public new static readonly StringName _Input = "_Input";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName isMainMenu = "isMainMenu";

		public static readonly StringName _releaseInfo = "_releaseInfo";

		public static readonly StringName _moddedWarning = "_moddedWarning";

		public static readonly StringName _seed = "_seed";

		public static readonly StringName _runningModded = "_runningModded";
	}

	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	public bool isMainMenu;

	private MegaLabel _releaseInfo;

	private MegaLabel _moddedWarning;

	private MegaLabel? _seed;

	private bool _runningModded;

	public override void _Ready()
	{
		_releaseInfo = GetNode<MegaLabel>("%ReleaseInfo");
		_moddedWarning = GetNode<MegaLabel>("%ModdedWarning");
		_seed = GetNodeOrNull<MegaLabel>("%DebugSeed");
		_runningModded = ModManager.LoadedMods.Count > 0;
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
		string text = DateTime.Now.ToString("yyyy-MM-dd");
		string text2 = releaseInfo?.Version ?? commitId ?? "NONE";
		if (isMainMenu)
		{
			_releaseInfo.Text = text2 + "\n" + text;
		}
		else
		{
			_releaseInfo.Text = $"[{text2}] ({text})";
		}
		_moddedWarning.Visible = _runningModded;
		if (_runningModded)
		{
			bool flag = ModManager.LoadedMods.Any((Mod m) => !(m.assemblyLoadedSuccessfully ?? true));
			if (isMainMenu)
			{
				LocString locString = new LocString("main_menu_ui", "MODDED_WARNING");
				locString.Add("count", ModManager.LoadedMods.Count);
				locString.Add("hasError", flag);
				_moddedWarning.SetTextAutoSize(locString.GetFormattedText());
			}
			else
			{
				_moddedWarning.SetTextAutoSize($"MODDED ({ModManager.LoadedMods.Count})");
			}
			if (flag)
			{
				_moddedWarning.Modulate = StsColors.redGlow;
			}
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideVersionInfo))
		{
			_releaseInfo.Visible = !_releaseInfo.Visible;
			_moddedWarning.Visible = _runningModded && !_moddedWarning.Visible;
			_seed?.SetVisible(!_seed.Visible);
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_releaseInfo.Visible ? "Show Version Info" : "Hide Version Info"));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "commitId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName.UpdateText && args.Count == 1)
		{
			UpdateText(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName._Input)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

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
		if (name == PropertyName._runningModded)
		{
			_runningModded = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

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
		if (name == PropertyName._runningModded)
		{
			value = VariantUtils.CreateFrom(in _runningModded);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.isMainMenu, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._releaseInfo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moddedWarning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._runningModded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.isMainMenu, Variant.From(in isMainMenu));
		info.AddProperty(PropertyName._releaseInfo, Variant.From(in _releaseInfo));
		info.AddProperty(PropertyName._moddedWarning, Variant.From(in _moddedWarning));
		info.AddProperty(PropertyName._seed, Variant.From(in _seed));
		info.AddProperty(PropertyName._runningModded, Variant.From(in _runningModded));
	}

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
		if (info.TryGetProperty(PropertyName._runningModded, out var value5))
		{
			_runningModded = value5.As<bool>();
		}
	}
}
