using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NCapstoneSubmenuStack.cs")]
public class NCapstoneSubmenuStack : Control, ICapstoneScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName ShowScreen = "ShowScreen";

		public static readonly StringName GetCapstoneSubmenuType = "GetCapstoneSubmenuType";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnSubmenuStackChanged = "OnSubmenuStackChanged";

		public static readonly StringName AfterCapstoneOpened = "AfterCapstoneOpened";

		public static readonly StringName AfterCapstoneClosed = "AfterCapstoneClosed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Type = "Type";

		public static readonly StringName Stack = "Stack";

		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static string ScenePath => SceneHelper.GetScenePath("screens/capstone_submenu_stack");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public CapstoneSubmenuType Type { get; private set; }

	public NRunSubmenuStack Stack { get; private set; }

	public NetScreenType ScreenType => GetCapstoneSubmenuType();

	public bool UseSharedBackstop => true;

	public Control? DefaultFocusedControl => Stack.Peek()?.DefaultFocusedControl;

	public NSubmenu ShowScreen(CapstoneSubmenuType type)
	{
		while (Stack.Peek() != null)
		{
			Stack.Pop();
		}
		Type type2 = type switch
		{
			CapstoneSubmenuType.Compendium => typeof(NCompendiumSubmenu), 
			CapstoneSubmenuType.Feedback => typeof(NSendFeedbackScreen), 
			CapstoneSubmenuType.PauseMenu => typeof(NPauseMenu), 
			CapstoneSubmenuType.Settings => typeof(NSettingsScreen), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
		NSubmenu result = Stack.PushSubmenuType(type2);
		Type = type;
		NCapstoneContainer.Instance.Open(this);
		return result;
	}

	private NetScreenType GetCapstoneSubmenuType()
	{
		return Type switch
		{
			CapstoneSubmenuType.Compendium => NetScreenType.Compendium, 
			CapstoneSubmenuType.Feedback => NetScreenType.Feedback, 
			CapstoneSubmenuType.PauseMenu => NetScreenType.PauseMenu, 
			CapstoneSubmenuType.Settings => NetScreenType.Settings, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public override void _Ready()
	{
		Stack = GetNode<NRunSubmenuStack>("%Submenus");
		Stack.Connect(NSubmenuStack.SignalName.StackModified, Callable.From(OnSubmenuStackChanged));
	}

	private void OnSubmenuStackChanged()
	{
		if (Stack.Peek() == null && NCapstoneContainer.Instance.CurrentCapstoneScreen == this)
		{
			NCapstoneContainer.Instance.Close();
		}
	}

	public void AfterCapstoneOpened()
	{
		NGlobalUi globalUi = NRun.Instance.GlobalUi;
		globalUi.TopBar.AnimHide();
		globalUi.RelicInventory.AnimHide();
		globalUi.MultiplayerPlayerContainer.AnimHide();
		SfxCmd.Play("event:/sfx/ui/pause_open");
		globalUi.MoveChild(globalUi.AboveTopBarVfxContainer, globalUi.CapstoneContainer.GetIndex());
		globalUi.MoveChild(globalUi.CardPreviewContainer, globalUi.CapstoneContainer.GetIndex());
		globalUi.MoveChild(globalUi.MessyCardPreviewContainer, globalUi.CapstoneContainer.GetIndex());
		base.Visible = true;
	}

	public void AfterCapstoneClosed()
	{
		while (Stack.Peek() != null)
		{
			Stack.Pop();
		}
		SfxCmd.Play("event:/sfx/ui/pause_close");
		NGlobalUi globalUi = NRun.Instance.GlobalUi;
		globalUi.TopBar.AnimShow();
		globalUi.RelicInventory.AnimShow();
		globalUi.MultiplayerPlayerContainer.AnimShow();
		globalUi.MoveChild(globalUi.AboveTopBarVfxContainer, globalUi.TopBar.GetIndex() + 1);
		globalUi.MoveChild(globalUi.CardPreviewContainer, globalUi.TopBar.GetIndex() + 1);
		globalUi.MoveChild(globalUi.MessyCardPreviewContainer, globalUi.TopBar.GetIndex() + 1);
		base.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.ShowScreen, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCapstoneSubmenuType, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuStackChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ShowScreen && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NSubmenu>(ShowScreen(VariantUtils.ConvertTo<CapstoneSubmenuType>(in args[0])));
			return true;
		}
		if (method == MethodName.GetCapstoneSubmenuType && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NetScreenType>(GetCapstoneSubmenuType());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuStackChanged && args.Count == 0)
		{
			OnSubmenuStackChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened && args.Count == 0)
		{
			AfterCapstoneOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed && args.Count == 0)
		{
			AfterCapstoneClosed();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ShowScreen)
		{
			return true;
		}
		if (method == MethodName.GetCapstoneSubmenuType)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuStackChanged)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Type)
		{
			Type = VariantUtils.ConvertTo<CapstoneSubmenuType>(in value);
			return true;
		}
		if (name == PropertyName.Stack)
		{
			Stack = VariantUtils.ConvertTo<NRunSubmenuStack>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Type)
		{
			value = VariantUtils.CreateFrom<CapstoneSubmenuType>(Type);
			return true;
		}
		if (name == PropertyName.Stack)
		{
			value = VariantUtils.CreateFrom<NRunSubmenuStack>(Stack);
			return true;
		}
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Type, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Stack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Type, Variant.From<CapstoneSubmenuType>(Type));
		info.AddProperty(PropertyName.Stack, Variant.From<NRunSubmenuStack>(Stack));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Type, out var value))
		{
			Type = value.As<CapstoneSubmenuType>();
		}
		if (info.TryGetProperty(PropertyName.Stack, out var value2))
		{
			Stack = value2.As<NRunSubmenuStack>();
		}
	}
}
