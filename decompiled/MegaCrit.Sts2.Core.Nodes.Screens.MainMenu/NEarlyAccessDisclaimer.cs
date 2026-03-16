using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NEarlyAccessDisclaimer.cs")]
public class NEarlyAccessDisclaimer : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName UpdateEaDisclaimerDescription = "UpdateEaDisclaimerDescription";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _image = "_image";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Tween? _tween;

	private Control _image;

	public Control? DefaultFocusedControl => null;

	public static NEarlyAccessDisclaimer Create()
	{
		NEarlyAccessDisclaimer nEarlyAccessDisclaimer = ResourceLoader.Load<PackedScene>("res://scenes/screens/main_menu/early_access_disclaimer.tscn", null, ResourceLoader.CacheMode.Reuse).Instantiate<NEarlyAccessDisclaimer>(PackedScene.GenEditState.Disabled);
		NHotkeyManager.Instance?.AddBlockingScreen(nEarlyAccessDisclaimer);
		return nEarlyAccessDisclaimer;
	}

	public override void _Ready()
	{
		string formattedText = new LocString("main_menu_ui", "EARLY_ACCESS_DISCLAIMER.header").GetFormattedText();
		GetNode<MegaLabel>("%Header").SetTextAutoSize(formattedText);
		_image = GetNode<TextureRect>("Image");
		UpdateEaDisclaimerDescription();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateEaDisclaimerDescription));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateEaDisclaimerDescription));
	}

	public async Task CloseScreen()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "modulate:a", 1f, 0.5);
		_tween.TweenProperty(_image, "position:y", _image.Position.Y - 1000f, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
		await ToSignal(_tween, Tween.SignalName.Finished);
		SaveManager.Instance.SettingsSave.SeenEaDisclaimer = true;
		this.QueueFreeSafely();
		NHotkeyManager.Instance?.RemoveBlockingScreen(this);
		NModalContainer.Instance.Clear();
	}

	private void UpdateEaDisclaimerDescription()
	{
		string textAutoSize = ((!NControllerManager.Instance.IsUsingController) ? new LocString("main_menu_ui", "EARLY_ACCESS_DISCLAIMER.description_mkb").GetFormattedText() : new LocString("main_menu_ui", "EARLY_ACCESS_DISCLAIMER.description_controller").GetFormattedText());
		GetNode<MegaRichTextLabel>("%Description").SetTextAutoSize(textAutoSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateEaDisclaimerDescription, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NEarlyAccessDisclaimer>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateEaDisclaimerDescription && args.Count == 0)
		{
			UpdateEaDisclaimerDescription();
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
			ret = VariantUtils.CreateFrom<NEarlyAccessDisclaimer>(Create());
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
		if (method == MethodName.UpdateEaDisclaimerDescription)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value2))
		{
			_image = value2.As<Control>();
		}
	}
}
