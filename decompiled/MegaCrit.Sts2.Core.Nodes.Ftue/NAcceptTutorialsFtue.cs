using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Ftue;

[ScriptPath("res://src/Core/Nodes/Ftue/NAcceptTutorialsFtue.cs")]
public class NAcceptTutorialsFtue : NFtue
{
	public new class MethodName : NFtue.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName NoTutorials = "NoTutorials";

		public static readonly StringName YesTutorials = "YesTutorials";
	}

	public new class PropertyName : NFtue.PropertyName
	{
		public static readonly StringName _charSelectScreen = "_charSelectScreen";

		public static readonly StringName _verticalPopup = "_verticalPopup";
	}

	public new class SignalName : NFtue.SignalName
	{
	}

	public const string id = "accept_tutorials_ftue";

	private static readonly string _scenePath = SceneHelper.GetScenePath("ftue/accept_tutorials_ftue");

	private NCharacterSelectScreen _charSelectScreen;

	private NVerticalPopup _verticalPopup;

	private Action _onFinished;

	public override void _Ready()
	{
		_verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		_verticalPopup.SetText(new LocString("main_menu_ui", "ENABLE_TUTORIALS.title"), new LocString("main_menu_ui", "ENABLE_TUTORIALS.description"));
		_verticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), YesTutorials);
		_verticalPopup.InitNoButton(new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), NoTutorials);
	}

	public static NAcceptTutorialsFtue? Create(NCharacterSelectScreen charSelectScreen, Action onFinished)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NAcceptTutorialsFtue nAcceptTutorialsFtue = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NAcceptTutorialsFtue>(PackedScene.GenEditState.Disabled);
		nAcceptTutorialsFtue._charSelectScreen = charSelectScreen;
		nAcceptTutorialsFtue._onFinished = onFinished;
		return nAcceptTutorialsFtue;
	}

	private void NoTutorials(NButton _)
	{
		SaveManager.Instance.MarkFtueAsComplete("accept_tutorials_ftue");
		SaveManager.Instance.SetFtuesEnabled(enabled: false);
		_onFinished();
		CloseFtue();
	}

	private void YesTutorials(NButton _)
	{
		SaveManager.Instance.MarkFtueAsComplete("accept_tutorials_ftue");
		_onFinished();
		CloseFtue();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.NoTutorials, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.YesTutorials, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.NoTutorials && args.Count == 1)
		{
			NoTutorials(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.YesTutorials && args.Count == 1)
		{
			YesTutorials(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName.NoTutorials)
		{
			return true;
		}
		if (method == MethodName.YesTutorials)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._charSelectScreen)
		{
			_charSelectScreen = VariantUtils.ConvertTo<NCharacterSelectScreen>(in value);
			return true;
		}
		if (name == PropertyName._verticalPopup)
		{
			_verticalPopup = VariantUtils.ConvertTo<NVerticalPopup>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._charSelectScreen)
		{
			value = VariantUtils.CreateFrom(in _charSelectScreen);
			return true;
		}
		if (name == PropertyName._verticalPopup)
		{
			value = VariantUtils.CreateFrom(in _verticalPopup);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._charSelectScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._charSelectScreen, Variant.From(in _charSelectScreen));
		info.AddProperty(PropertyName._verticalPopup, Variant.From(in _verticalPopup));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._charSelectScreen, out var value))
		{
			_charSelectScreen = value.As<NCharacterSelectScreen>();
		}
		if (info.TryGetProperty(PropertyName._verticalPopup, out var value2))
		{
			_verticalPopup = value2.As<NVerticalPopup>();
		}
	}
}
