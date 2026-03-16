using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NRunSubmenuStack.cs")]
public class NRunSubmenuStack : NSubmenuStack
{
	public new class MethodName : NSubmenuStack.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : NSubmenuStack.PropertyName
	{
		public static readonly StringName _settingsScreenScene = "_settingsScreenScene";

		public static readonly StringName _pauseMenuScene = "_pauseMenuScene";

		public static readonly StringName _statsScreenScene = "_statsScreenScene";

		public static readonly StringName _runHistoryScreenScene = "_runHistoryScreenScene";

		public static readonly StringName _compendiumSubmenu = "_compendiumSubmenu";

		public static readonly StringName _bestiarySubmenu = "_bestiarySubmenu";

		public static readonly StringName _relicCollectionSubmenu = "_relicCollectionSubmenu";

		public static readonly StringName _potionLabSubmenu = "_potionLabSubmenu";

		public static readonly StringName _cardLibrarySubmenu = "_cardLibrarySubmenu";

		public static readonly StringName _runHistoryScreen = "_runHistoryScreen";

		public static readonly StringName _settingsScreen = "_settingsScreen";

		public static readonly StringName _statsScreen = "_statsScreen";

		public static readonly StringName _pauseMenu = "_pauseMenu";
	}

	public new class SignalName : NSubmenuStack.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _settingsScreenScene;

	[Export(PropertyHint.None, "")]
	private PackedScene _pauseMenuScene;

	[Export(PropertyHint.None, "")]
	private PackedScene _statsScreenScene;

	[Export(PropertyHint.None, "")]
	private PackedScene _runHistoryScreenScene;

	private NCompendiumSubmenu? _compendiumSubmenu;

	private NBestiary? _bestiarySubmenu;

	private NRelicCollection? _relicCollectionSubmenu;

	private NPotionLab? _potionLabSubmenu;

	private NCardLibrary? _cardLibrarySubmenu;

	private NRunHistory? _runHistoryScreen;

	private NSettingsScreen? _settingsScreen;

	private NStatsScreen? _statsScreen;

	private NPauseMenu? _pauseMenu;

	public override void _Ready()
	{
		GetSubmenuType<NSettingsScreen>();
	}

	public override T PushSubmenuType<T>()
	{
		return (T)PushSubmenuType(typeof(T));
	}

	public override T GetSubmenuType<T>()
	{
		return (T)GetSubmenuType(typeof(T));
	}

	public override NSubmenu PushSubmenuType(Type type)
	{
		NSubmenu submenuType = GetSubmenuType(type);
		Push(submenuType);
		return submenuType;
	}

	public override NSubmenu GetSubmenuType(Type type)
	{
		if (type == typeof(NCompendiumSubmenu))
		{
			if (_compendiumSubmenu == null)
			{
				_compendiumSubmenu = NCompendiumSubmenu.Create();
				_compendiumSubmenu.Visible = false;
				this.AddChildSafely(_compendiumSubmenu);
			}
			return _compendiumSubmenu;
		}
		if (type == typeof(NBestiary))
		{
			if (_bestiarySubmenu == null)
			{
				_bestiarySubmenu = NBestiary.Create();
				_bestiarySubmenu.Visible = false;
				this.AddChildSafely(_bestiarySubmenu);
			}
			return _bestiarySubmenu;
		}
		if (type == typeof(NRelicCollection))
		{
			if (_relicCollectionSubmenu == null)
			{
				_relicCollectionSubmenu = NRelicCollection.Create();
				_relicCollectionSubmenu.Visible = false;
				this.AddChildSafely(_relicCollectionSubmenu);
			}
			return _relicCollectionSubmenu;
		}
		if (type == typeof(NPotionLab))
		{
			if (_potionLabSubmenu == null)
			{
				_potionLabSubmenu = NPotionLab.Create();
				_potionLabSubmenu.Visible = false;
				this.AddChildSafely(_potionLabSubmenu);
			}
			return _potionLabSubmenu;
		}
		if (type == typeof(NCardLibrary))
		{
			if (_cardLibrarySubmenu == null)
			{
				_cardLibrarySubmenu = NCardLibrary.Create();
				_cardLibrarySubmenu.Visible = false;
				this.AddChildSafely(_cardLibrarySubmenu);
			}
			return _cardLibrarySubmenu;
		}
		if (type == typeof(NRunHistory))
		{
			if (_runHistoryScreen == null)
			{
				_runHistoryScreen = _runHistoryScreenScene.Instantiate<NRunHistory>(PackedScene.GenEditState.Disabled);
				_runHistoryScreen.Visible = false;
				this.AddChildSafely(_runHistoryScreen);
			}
			return _runHistoryScreen;
		}
		if (type == typeof(NSettingsScreen))
		{
			if (_settingsScreen == null)
			{
				_settingsScreen = _settingsScreenScene.Instantiate<NSettingsScreen>(PackedScene.GenEditState.Disabled);
				_settingsScreen.SetIsInRun(isInRun: true);
				_settingsScreen.Visible = false;
				this.AddChildSafely(_settingsScreen);
			}
			return _settingsScreen;
		}
		if (type == typeof(NStatsScreen))
		{
			if (_statsScreen == null)
			{
				_statsScreen = _statsScreenScene.Instantiate<NStatsScreen>(PackedScene.GenEditState.Disabled);
				_statsScreen.Visible = false;
				this.AddChildSafely(_statsScreen);
			}
			return _statsScreen;
		}
		if (type == typeof(NPauseMenu))
		{
			if (_pauseMenu == null)
			{
				_pauseMenu = _pauseMenuScene.Instantiate<NPauseMenu>(PackedScene.GenEditState.Disabled);
				_pauseMenu.Visible = false;
				this.AddChildSafely(_pauseMenu);
			}
			return _pauseMenu;
		}
		throw new ArgumentException($"No such submenu of type {type} in run");
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._settingsScreenScene)
		{
			_settingsScreenScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._pauseMenuScene)
		{
			_pauseMenuScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._statsScreenScene)
		{
			_statsScreenScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._runHistoryScreenScene)
		{
			_runHistoryScreenScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._compendiumSubmenu)
		{
			_compendiumSubmenu = VariantUtils.ConvertTo<NCompendiumSubmenu>(in value);
			return true;
		}
		if (name == PropertyName._bestiarySubmenu)
		{
			_bestiarySubmenu = VariantUtils.ConvertTo<NBestiary>(in value);
			return true;
		}
		if (name == PropertyName._relicCollectionSubmenu)
		{
			_relicCollectionSubmenu = VariantUtils.ConvertTo<NRelicCollection>(in value);
			return true;
		}
		if (name == PropertyName._potionLabSubmenu)
		{
			_potionLabSubmenu = VariantUtils.ConvertTo<NPotionLab>(in value);
			return true;
		}
		if (name == PropertyName._cardLibrarySubmenu)
		{
			_cardLibrarySubmenu = VariantUtils.ConvertTo<NCardLibrary>(in value);
			return true;
		}
		if (name == PropertyName._runHistoryScreen)
		{
			_runHistoryScreen = VariantUtils.ConvertTo<NRunHistory>(in value);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			_settingsScreen = VariantUtils.ConvertTo<NSettingsScreen>(in value);
			return true;
		}
		if (name == PropertyName._statsScreen)
		{
			_statsScreen = VariantUtils.ConvertTo<NStatsScreen>(in value);
			return true;
		}
		if (name == PropertyName._pauseMenu)
		{
			_pauseMenu = VariantUtils.ConvertTo<NPauseMenu>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._settingsScreenScene)
		{
			value = VariantUtils.CreateFrom(in _settingsScreenScene);
			return true;
		}
		if (name == PropertyName._pauseMenuScene)
		{
			value = VariantUtils.CreateFrom(in _pauseMenuScene);
			return true;
		}
		if (name == PropertyName._statsScreenScene)
		{
			value = VariantUtils.CreateFrom(in _statsScreenScene);
			return true;
		}
		if (name == PropertyName._runHistoryScreenScene)
		{
			value = VariantUtils.CreateFrom(in _runHistoryScreenScene);
			return true;
		}
		if (name == PropertyName._compendiumSubmenu)
		{
			value = VariantUtils.CreateFrom(in _compendiumSubmenu);
			return true;
		}
		if (name == PropertyName._bestiarySubmenu)
		{
			value = VariantUtils.CreateFrom(in _bestiarySubmenu);
			return true;
		}
		if (name == PropertyName._relicCollectionSubmenu)
		{
			value = VariantUtils.CreateFrom(in _relicCollectionSubmenu);
			return true;
		}
		if (name == PropertyName._potionLabSubmenu)
		{
			value = VariantUtils.CreateFrom(in _potionLabSubmenu);
			return true;
		}
		if (name == PropertyName._cardLibrarySubmenu)
		{
			value = VariantUtils.CreateFrom(in _cardLibrarySubmenu);
			return true;
		}
		if (name == PropertyName._runHistoryScreen)
		{
			value = VariantUtils.CreateFrom(in _runHistoryScreen);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			value = VariantUtils.CreateFrom(in _settingsScreen);
			return true;
		}
		if (name == PropertyName._statsScreen)
		{
			value = VariantUtils.CreateFrom(in _statsScreen);
			return true;
		}
		if (name == PropertyName._pauseMenu)
		{
			value = VariantUtils.CreateFrom(in _pauseMenu);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreenScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pauseMenuScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsScreenScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runHistoryScreenScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._compendiumSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiarySubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicCollectionSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionLabSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardLibrarySubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runHistoryScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pauseMenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._settingsScreenScene, Variant.From(in _settingsScreenScene));
		info.AddProperty(PropertyName._pauseMenuScene, Variant.From(in _pauseMenuScene));
		info.AddProperty(PropertyName._statsScreenScene, Variant.From(in _statsScreenScene));
		info.AddProperty(PropertyName._runHistoryScreenScene, Variant.From(in _runHistoryScreenScene));
		info.AddProperty(PropertyName._compendiumSubmenu, Variant.From(in _compendiumSubmenu));
		info.AddProperty(PropertyName._bestiarySubmenu, Variant.From(in _bestiarySubmenu));
		info.AddProperty(PropertyName._relicCollectionSubmenu, Variant.From(in _relicCollectionSubmenu));
		info.AddProperty(PropertyName._potionLabSubmenu, Variant.From(in _potionLabSubmenu));
		info.AddProperty(PropertyName._cardLibrarySubmenu, Variant.From(in _cardLibrarySubmenu));
		info.AddProperty(PropertyName._runHistoryScreen, Variant.From(in _runHistoryScreen));
		info.AddProperty(PropertyName._settingsScreen, Variant.From(in _settingsScreen));
		info.AddProperty(PropertyName._statsScreen, Variant.From(in _statsScreen));
		info.AddProperty(PropertyName._pauseMenu, Variant.From(in _pauseMenu));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._settingsScreenScene, out var value))
		{
			_settingsScreenScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._pauseMenuScene, out var value2))
		{
			_pauseMenuScene = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._statsScreenScene, out var value3))
		{
			_statsScreenScene = value3.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._runHistoryScreenScene, out var value4))
		{
			_runHistoryScreenScene = value4.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._compendiumSubmenu, out var value5))
		{
			_compendiumSubmenu = value5.As<NCompendiumSubmenu>();
		}
		if (info.TryGetProperty(PropertyName._bestiarySubmenu, out var value6))
		{
			_bestiarySubmenu = value6.As<NBestiary>();
		}
		if (info.TryGetProperty(PropertyName._relicCollectionSubmenu, out var value7))
		{
			_relicCollectionSubmenu = value7.As<NRelicCollection>();
		}
		if (info.TryGetProperty(PropertyName._potionLabSubmenu, out var value8))
		{
			_potionLabSubmenu = value8.As<NPotionLab>();
		}
		if (info.TryGetProperty(PropertyName._cardLibrarySubmenu, out var value9))
		{
			_cardLibrarySubmenu = value9.As<NCardLibrary>();
		}
		if (info.TryGetProperty(PropertyName._runHistoryScreen, out var value10))
		{
			_runHistoryScreen = value10.As<NRunHistory>();
		}
		if (info.TryGetProperty(PropertyName._settingsScreen, out var value11))
		{
			_settingsScreen = value11.As<NSettingsScreen>();
		}
		if (info.TryGetProperty(PropertyName._statsScreen, out var value12))
		{
			_statsScreen = value12.As<NStatsScreen>();
		}
		if (info.TryGetProperty(PropertyName._pauseMenu, out var value13))
		{
			_pauseMenu = value13.As<NPauseMenu>();
		}
	}
}
