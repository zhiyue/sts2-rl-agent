using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NAbandonRunConfirmPopup.cs")]
public class NAbandonRunConfirmPopup : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Create = "Create";

		public static readonly StringName OnYesButtonPressed = "OnYesButtonPressed";

		public static readonly StringName OnNoButtonPressed = "OnNoButtonPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _verticalPopup = "_verticalPopup";

		public static readonly StringName _mainMenuNode = "_mainMenuNode";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NVerticalPopup _verticalPopup;

	private NMainMenu? _mainMenuNode;

	private Tween? _tween;

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/abandon_run_confirm_popup");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public Control? DefaultFocusedControl => null;

	public override void _Ready()
	{
		_verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		_verticalPopup.SetText(new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.header"), new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.body"));
		_verticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.confirm"), OnYesButtonPressed);
		_verticalPopup.InitNoButton(new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), OnNoButtonPressed);
	}

	public override void _EnterTree()
	{
		base.Modulate = StsColors.transparentBlack;
		float y = base.Position.Y;
		base.Position += new Vector2(0f, 100f);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", Colors.White, 0.1);
		_tween.TweenProperty(this, "position:y", y, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
	}

	public override void _ExitTree()
	{
		_verticalPopup.DisconnectSignals();
		_verticalPopup.DisconnectHotkeys();
	}

	public static NAbandonRunConfirmPopup? Create(NMainMenu? mainMenu)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NAbandonRunConfirmPopup nAbandonRunConfirmPopup = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NAbandonRunConfirmPopup>(PackedScene.GenEditState.Disabled);
		nAbandonRunConfirmPopup._mainMenuNode = mainMenu;
		return nAbandonRunConfirmPopup;
	}

	private void OnYesButtonPressed(NButton _)
	{
		if (_mainMenuNode == null)
		{
			RunManager.Instance.Abandon();
		}
		else
		{
			_mainMenuNode.AbandonRun();
		}
		this.QueueFreeSafely();
	}

	private void OnNoButtonPressed(NButton _)
	{
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mainMenu", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnYesButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnNoButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAbandonRunConfirmPopup>(Create(VariantUtils.ConvertTo<NMainMenu>(in args[0])));
			return true;
		}
		if (method == MethodName.OnYesButtonPressed && args.Count == 1)
		{
			OnYesButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnNoButtonPressed && args.Count == 1)
		{
			OnNoButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAbandonRunConfirmPopup>(Create(VariantUtils.ConvertTo<NMainMenu>(in args[0])));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.OnYesButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnNoButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._verticalPopup)
		{
			_verticalPopup = VariantUtils.ConvertTo<NVerticalPopup>(in value);
			return true;
		}
		if (name == PropertyName._mainMenuNode)
		{
			_mainMenuNode = VariantUtils.ConvertTo<NMainMenu>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._verticalPopup)
		{
			value = VariantUtils.CreateFrom(in _verticalPopup);
			return true;
		}
		if (name == PropertyName._mainMenuNode)
		{
			value = VariantUtils.CreateFrom(in _mainMenuNode);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainMenuNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._verticalPopup, Variant.From(in _verticalPopup));
		info.AddProperty(PropertyName._mainMenuNode, Variant.From(in _mainMenuNode));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._verticalPopup, out var value))
		{
			_verticalPopup = value.As<NVerticalPopup>();
		}
		if (info.TryGetProperty(PropertyName._mainMenuNode, out var value2))
		{
			_mainMenuNode = value2.As<NMainMenu>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
	}
}
