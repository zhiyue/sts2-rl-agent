using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;
using MegaCrit.Sts2.Core.Nodes.Screens.ProfileScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMainMenuSubmenuStack.cs")]
public class NMainMenuSubmenuStack : NSubmenuStack
{
	public new class MethodName : NSubmenuStack.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : NSubmenuStack.PropertyName
	{
		public static readonly StringName _settingsScreenScene = "_settingsScreenScene";

		public static readonly StringName _characterSelectScreenScene = "_characterSelectScreenScene";

		public static readonly StringName _singleplayerSubmenu = "_singleplayerSubmenu";

		public static readonly StringName _multiplayerSubmenu = "_multiplayerSubmenu";

		public static readonly StringName _multiplayerHostSubmenu = "_multiplayerHostSubmenu";

		public static readonly StringName _joinFriendSubmenu = "_joinFriendSubmenu";

		public static readonly StringName _characterSelectSubmenu = "_characterSelectSubmenu";

		public static readonly StringName _loadMultiplayerSubmenu = "_loadMultiplayerSubmenu";

		public static readonly StringName _compendiumSubmenu = "_compendiumSubmenu";

		public static readonly StringName _bestiarySubmenu = "_bestiarySubmenu";

		public static readonly StringName _relicCollectionSubmenu = "_relicCollectionSubmenu";

		public static readonly StringName _potionLabSubmenu = "_potionLabSubmenu";

		public static readonly StringName _cardLibrarySubmenu = "_cardLibrarySubmenu";

		public static readonly StringName _runHistorySubmenu = "_runHistorySubmenu";

		public static readonly StringName _statsScreen = "_statsScreen";

		public static readonly StringName _timelineScreen = "_timelineScreen";

		public static readonly StringName _settingsScreen = "_settingsScreen";

		public static readonly StringName _dailyScreen = "_dailyScreen";

		public static readonly StringName _dailyLoadScreen = "_dailyLoadScreen";

		public static readonly StringName _customRunScreen = "_customRunScreen";

		public static readonly StringName _customRunLoadScreen = "_customRunLoadScreen";

		public static readonly StringName _moddingScreen = "_moddingScreen";

		public static readonly StringName _profileScreen = "_profileScreen";
	}

	public new class SignalName : NSubmenuStack.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private PackedScene _settingsScreenScene;

	[Export(PropertyHint.None, "")]
	private PackedScene _characterSelectScreenScene;

	private NSingleplayerSubmenu? _singleplayerSubmenu;

	private NMultiplayerSubmenu? _multiplayerSubmenu;

	private NMultiplayerHostSubmenu? _multiplayerHostSubmenu;

	private NJoinFriendScreen? _joinFriendSubmenu;

	private NCharacterSelectScreen? _characterSelectSubmenu;

	private NMultiplayerLoadGameScreen? _loadMultiplayerSubmenu;

	private NCompendiumSubmenu? _compendiumSubmenu;

	private NBestiary? _bestiarySubmenu;

	private NRelicCollection? _relicCollectionSubmenu;

	private NPotionLab? _potionLabSubmenu;

	private NCardLibrary? _cardLibrarySubmenu;

	private NRunHistory? _runHistorySubmenu;

	private NStatsScreen? _statsScreen;

	private NTimelineScreen? _timelineScreen;

	private NSettingsScreen? _settingsScreen;

	private NDailyRunScreen? _dailyScreen;

	private NDailyRunLoadScreen? _dailyLoadScreen;

	private NCustomRunScreen? _customRunScreen;

	private NCustomRunLoadScreen? _customRunLoadScreen;

	private NModdingScreen? _moddingScreen;

	private NProfileScreen? _profileScreen;

	public override void _Ready()
	{
		GetSubmenuType<NSettingsScreen>();
		GetSubmenuType<NCharacterSelectScreen>();
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
		if (type == typeof(NSingleplayerSubmenu))
		{
			if (_singleplayerSubmenu == null)
			{
				_singleplayerSubmenu = NSingleplayerSubmenu.Create();
				_singleplayerSubmenu.Visible = false;
				this.AddChildSafely(_singleplayerSubmenu);
			}
			return _singleplayerSubmenu;
		}
		if (type == typeof(NMultiplayerSubmenu))
		{
			if (_multiplayerSubmenu == null)
			{
				_multiplayerSubmenu = NMultiplayerSubmenu.Create();
				_multiplayerSubmenu.Visible = false;
				this.AddChildSafely(_multiplayerSubmenu);
			}
			return _multiplayerSubmenu;
		}
		if (type == typeof(NMultiplayerHostSubmenu))
		{
			if (_multiplayerHostSubmenu == null)
			{
				_multiplayerHostSubmenu = NMultiplayerHostSubmenu.Create();
				_multiplayerHostSubmenu.Visible = false;
				this.AddChildSafely(_multiplayerHostSubmenu);
			}
			return _multiplayerHostSubmenu;
		}
		if (type == typeof(NJoinFriendScreen))
		{
			if (_joinFriendSubmenu == null)
			{
				_joinFriendSubmenu = NJoinFriendScreen.Create();
				_joinFriendSubmenu.Visible = false;
				this.AddChildSafely(_joinFriendSubmenu);
			}
			return _joinFriendSubmenu;
		}
		if (type == typeof(NCharacterSelectScreen))
		{
			if (_characterSelectSubmenu == null)
			{
				_characterSelectSubmenu = _characterSelectScreenScene.Instantiate<NCharacterSelectScreen>(PackedScene.GenEditState.Disabled);
				_characterSelectSubmenu.Visible = false;
				this.AddChildSafely(_characterSelectSubmenu);
			}
			return _characterSelectSubmenu;
		}
		if (type == typeof(NMultiplayerLoadGameScreen))
		{
			if (_loadMultiplayerSubmenu == null)
			{
				_loadMultiplayerSubmenu = NMultiplayerLoadGameScreen.Create();
				_loadMultiplayerSubmenu.Visible = false;
				this.AddChildSafely(_loadMultiplayerSubmenu);
			}
			return _loadMultiplayerSubmenu;
		}
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
		if (type == typeof(NMultiplayerHostSubmenu))
		{
			if (_multiplayerHostSubmenu == null)
			{
				_multiplayerHostSubmenu = NMultiplayerHostSubmenu.Create();
				_multiplayerHostSubmenu.Visible = false;
				this.AddChildSafely(_multiplayerHostSubmenu);
			}
			return _multiplayerHostSubmenu;
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
			if (_runHistorySubmenu == null)
			{
				_runHistorySubmenu = NRunHistory.Create();
				_runHistorySubmenu.Visible = false;
				this.AddChildSafely(_runHistorySubmenu);
			}
			return _runHistorySubmenu;
		}
		if (type == typeof(NStatsScreen))
		{
			if (_statsScreen == null)
			{
				_statsScreen = NStatsScreen.Create();
				_statsScreen.Visible = false;
				this.AddChildSafely(_statsScreen);
			}
			return _statsScreen;
		}
		if (type == typeof(NTimelineScreen))
		{
			if (_timelineScreen == null)
			{
				_timelineScreen = NTimelineScreen.Create();
				_timelineScreen.Visible = false;
				this.AddChildSafely(_timelineScreen);
			}
			return _timelineScreen;
		}
		if (type == typeof(NSettingsScreen))
		{
			if (_settingsScreen == null)
			{
				_settingsScreen = _settingsScreenScene.Instantiate<NSettingsScreen>(PackedScene.GenEditState.Disabled);
				_settingsScreen.SetIsInRun(isInRun: false);
				_settingsScreen.Visible = false;
				this.AddChildSafely(_settingsScreen);
			}
			return _settingsScreen;
		}
		if (type == typeof(NDailyRunScreen))
		{
			if (_dailyScreen == null)
			{
				_dailyScreen = NDailyRunScreen.Create();
				_dailyScreen.Visible = false;
				this.AddChildSafely(_dailyScreen);
			}
			return _dailyScreen;
		}
		if (type == typeof(NDailyRunLoadScreen))
		{
			if (_dailyLoadScreen == null)
			{
				_dailyLoadScreen = NDailyRunLoadScreen.Create();
				_dailyLoadScreen.Visible = false;
				this.AddChildSafely(_dailyLoadScreen);
			}
			return _dailyLoadScreen;
		}
		if (type == typeof(NCustomRunScreen))
		{
			if (_customRunScreen == null)
			{
				_customRunScreen = NCustomRunScreen.Create();
				_customRunScreen.Visible = false;
				this.AddChildSafely(_customRunScreen);
			}
			return _customRunScreen;
		}
		if (type == typeof(NCustomRunLoadScreen))
		{
			if (_customRunLoadScreen == null)
			{
				_customRunLoadScreen = NCustomRunLoadScreen.Create();
				_customRunLoadScreen.Visible = false;
				this.AddChildSafely(_customRunLoadScreen);
			}
			return _customRunLoadScreen;
		}
		if (type == typeof(NModdingScreen))
		{
			if (_moddingScreen == null)
			{
				_moddingScreen = NModdingScreen.Create();
				_moddingScreen.Visible = false;
				this.AddChildSafely(_moddingScreen);
			}
			return _moddingScreen;
		}
		if (type == typeof(NProfileScreen))
		{
			if (_profileScreen == null)
			{
				_profileScreen = NProfileScreen.Create();
				_profileScreen.Visible = false;
				this.AddChildSafely(_profileScreen);
			}
			return _profileScreen;
		}
		throw new ArgumentException($"No such submenu {type} in main menu");
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
		if (name == PropertyName._characterSelectScreenScene)
		{
			_characterSelectScreenScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._singleplayerSubmenu)
		{
			_singleplayerSubmenu = VariantUtils.ConvertTo<NSingleplayerSubmenu>(in value);
			return true;
		}
		if (name == PropertyName._multiplayerSubmenu)
		{
			_multiplayerSubmenu = VariantUtils.ConvertTo<NMultiplayerSubmenu>(in value);
			return true;
		}
		if (name == PropertyName._multiplayerHostSubmenu)
		{
			_multiplayerHostSubmenu = VariantUtils.ConvertTo<NMultiplayerHostSubmenu>(in value);
			return true;
		}
		if (name == PropertyName._joinFriendSubmenu)
		{
			_joinFriendSubmenu = VariantUtils.ConvertTo<NJoinFriendScreen>(in value);
			return true;
		}
		if (name == PropertyName._characterSelectSubmenu)
		{
			_characterSelectSubmenu = VariantUtils.ConvertTo<NCharacterSelectScreen>(in value);
			return true;
		}
		if (name == PropertyName._loadMultiplayerSubmenu)
		{
			_loadMultiplayerSubmenu = VariantUtils.ConvertTo<NMultiplayerLoadGameScreen>(in value);
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
		if (name == PropertyName._runHistorySubmenu)
		{
			_runHistorySubmenu = VariantUtils.ConvertTo<NRunHistory>(in value);
			return true;
		}
		if (name == PropertyName._statsScreen)
		{
			_statsScreen = VariantUtils.ConvertTo<NStatsScreen>(in value);
			return true;
		}
		if (name == PropertyName._timelineScreen)
		{
			_timelineScreen = VariantUtils.ConvertTo<NTimelineScreen>(in value);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			_settingsScreen = VariantUtils.ConvertTo<NSettingsScreen>(in value);
			return true;
		}
		if (name == PropertyName._dailyScreen)
		{
			_dailyScreen = VariantUtils.ConvertTo<NDailyRunScreen>(in value);
			return true;
		}
		if (name == PropertyName._dailyLoadScreen)
		{
			_dailyLoadScreen = VariantUtils.ConvertTo<NDailyRunLoadScreen>(in value);
			return true;
		}
		if (name == PropertyName._customRunScreen)
		{
			_customRunScreen = VariantUtils.ConvertTo<NCustomRunScreen>(in value);
			return true;
		}
		if (name == PropertyName._customRunLoadScreen)
		{
			_customRunLoadScreen = VariantUtils.ConvertTo<NCustomRunLoadScreen>(in value);
			return true;
		}
		if (name == PropertyName._moddingScreen)
		{
			_moddingScreen = VariantUtils.ConvertTo<NModdingScreen>(in value);
			return true;
		}
		if (name == PropertyName._profileScreen)
		{
			_profileScreen = VariantUtils.ConvertTo<NProfileScreen>(in value);
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
		if (name == PropertyName._characterSelectScreenScene)
		{
			value = VariantUtils.CreateFrom(in _characterSelectScreenScene);
			return true;
		}
		if (name == PropertyName._singleplayerSubmenu)
		{
			value = VariantUtils.CreateFrom(in _singleplayerSubmenu);
			return true;
		}
		if (name == PropertyName._multiplayerSubmenu)
		{
			value = VariantUtils.CreateFrom(in _multiplayerSubmenu);
			return true;
		}
		if (name == PropertyName._multiplayerHostSubmenu)
		{
			value = VariantUtils.CreateFrom(in _multiplayerHostSubmenu);
			return true;
		}
		if (name == PropertyName._joinFriendSubmenu)
		{
			value = VariantUtils.CreateFrom(in _joinFriendSubmenu);
			return true;
		}
		if (name == PropertyName._characterSelectSubmenu)
		{
			value = VariantUtils.CreateFrom(in _characterSelectSubmenu);
			return true;
		}
		if (name == PropertyName._loadMultiplayerSubmenu)
		{
			value = VariantUtils.CreateFrom(in _loadMultiplayerSubmenu);
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
		if (name == PropertyName._runHistorySubmenu)
		{
			value = VariantUtils.CreateFrom(in _runHistorySubmenu);
			return true;
		}
		if (name == PropertyName._statsScreen)
		{
			value = VariantUtils.CreateFrom(in _statsScreen);
			return true;
		}
		if (name == PropertyName._timelineScreen)
		{
			value = VariantUtils.CreateFrom(in _timelineScreen);
			return true;
		}
		if (name == PropertyName._settingsScreen)
		{
			value = VariantUtils.CreateFrom(in _settingsScreen);
			return true;
		}
		if (name == PropertyName._dailyScreen)
		{
			value = VariantUtils.CreateFrom(in _dailyScreen);
			return true;
		}
		if (name == PropertyName._dailyLoadScreen)
		{
			value = VariantUtils.CreateFrom(in _dailyLoadScreen);
			return true;
		}
		if (name == PropertyName._customRunScreen)
		{
			value = VariantUtils.CreateFrom(in _customRunScreen);
			return true;
		}
		if (name == PropertyName._customRunLoadScreen)
		{
			value = VariantUtils.CreateFrom(in _customRunLoadScreen);
			return true;
		}
		if (name == PropertyName._moddingScreen)
		{
			value = VariantUtils.CreateFrom(in _moddingScreen);
			return true;
		}
		if (name == PropertyName._profileScreen)
		{
			value = VariantUtils.CreateFrom(in _profileScreen);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreenScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterSelectScreenScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singleplayerSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiplayerSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiplayerHostSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._joinFriendSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterSelectSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadMultiplayerSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._compendiumSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiarySubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicCollectionSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionLabSubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardLibrarySubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runHistorySubmenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timelineScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dailyScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dailyLoadScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._customRunScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._customRunLoadScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moddingScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._profileScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._settingsScreenScene, Variant.From(in _settingsScreenScene));
		info.AddProperty(PropertyName._characterSelectScreenScene, Variant.From(in _characterSelectScreenScene));
		info.AddProperty(PropertyName._singleplayerSubmenu, Variant.From(in _singleplayerSubmenu));
		info.AddProperty(PropertyName._multiplayerSubmenu, Variant.From(in _multiplayerSubmenu));
		info.AddProperty(PropertyName._multiplayerHostSubmenu, Variant.From(in _multiplayerHostSubmenu));
		info.AddProperty(PropertyName._joinFriendSubmenu, Variant.From(in _joinFriendSubmenu));
		info.AddProperty(PropertyName._characterSelectSubmenu, Variant.From(in _characterSelectSubmenu));
		info.AddProperty(PropertyName._loadMultiplayerSubmenu, Variant.From(in _loadMultiplayerSubmenu));
		info.AddProperty(PropertyName._compendiumSubmenu, Variant.From(in _compendiumSubmenu));
		info.AddProperty(PropertyName._bestiarySubmenu, Variant.From(in _bestiarySubmenu));
		info.AddProperty(PropertyName._relicCollectionSubmenu, Variant.From(in _relicCollectionSubmenu));
		info.AddProperty(PropertyName._potionLabSubmenu, Variant.From(in _potionLabSubmenu));
		info.AddProperty(PropertyName._cardLibrarySubmenu, Variant.From(in _cardLibrarySubmenu));
		info.AddProperty(PropertyName._runHistorySubmenu, Variant.From(in _runHistorySubmenu));
		info.AddProperty(PropertyName._statsScreen, Variant.From(in _statsScreen));
		info.AddProperty(PropertyName._timelineScreen, Variant.From(in _timelineScreen));
		info.AddProperty(PropertyName._settingsScreen, Variant.From(in _settingsScreen));
		info.AddProperty(PropertyName._dailyScreen, Variant.From(in _dailyScreen));
		info.AddProperty(PropertyName._dailyLoadScreen, Variant.From(in _dailyLoadScreen));
		info.AddProperty(PropertyName._customRunScreen, Variant.From(in _customRunScreen));
		info.AddProperty(PropertyName._customRunLoadScreen, Variant.From(in _customRunLoadScreen));
		info.AddProperty(PropertyName._moddingScreen, Variant.From(in _moddingScreen));
		info.AddProperty(PropertyName._profileScreen, Variant.From(in _profileScreen));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._settingsScreenScene, out var value))
		{
			_settingsScreenScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._characterSelectScreenScene, out var value2))
		{
			_characterSelectScreenScene = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._singleplayerSubmenu, out var value3))
		{
			_singleplayerSubmenu = value3.As<NSingleplayerSubmenu>();
		}
		if (info.TryGetProperty(PropertyName._multiplayerSubmenu, out var value4))
		{
			_multiplayerSubmenu = value4.As<NMultiplayerSubmenu>();
		}
		if (info.TryGetProperty(PropertyName._multiplayerHostSubmenu, out var value5))
		{
			_multiplayerHostSubmenu = value5.As<NMultiplayerHostSubmenu>();
		}
		if (info.TryGetProperty(PropertyName._joinFriendSubmenu, out var value6))
		{
			_joinFriendSubmenu = value6.As<NJoinFriendScreen>();
		}
		if (info.TryGetProperty(PropertyName._characterSelectSubmenu, out var value7))
		{
			_characterSelectSubmenu = value7.As<NCharacterSelectScreen>();
		}
		if (info.TryGetProperty(PropertyName._loadMultiplayerSubmenu, out var value8))
		{
			_loadMultiplayerSubmenu = value8.As<NMultiplayerLoadGameScreen>();
		}
		if (info.TryGetProperty(PropertyName._compendiumSubmenu, out var value9))
		{
			_compendiumSubmenu = value9.As<NCompendiumSubmenu>();
		}
		if (info.TryGetProperty(PropertyName._bestiarySubmenu, out var value10))
		{
			_bestiarySubmenu = value10.As<NBestiary>();
		}
		if (info.TryGetProperty(PropertyName._relicCollectionSubmenu, out var value11))
		{
			_relicCollectionSubmenu = value11.As<NRelicCollection>();
		}
		if (info.TryGetProperty(PropertyName._potionLabSubmenu, out var value12))
		{
			_potionLabSubmenu = value12.As<NPotionLab>();
		}
		if (info.TryGetProperty(PropertyName._cardLibrarySubmenu, out var value13))
		{
			_cardLibrarySubmenu = value13.As<NCardLibrary>();
		}
		if (info.TryGetProperty(PropertyName._runHistorySubmenu, out var value14))
		{
			_runHistorySubmenu = value14.As<NRunHistory>();
		}
		if (info.TryGetProperty(PropertyName._statsScreen, out var value15))
		{
			_statsScreen = value15.As<NStatsScreen>();
		}
		if (info.TryGetProperty(PropertyName._timelineScreen, out var value16))
		{
			_timelineScreen = value16.As<NTimelineScreen>();
		}
		if (info.TryGetProperty(PropertyName._settingsScreen, out var value17))
		{
			_settingsScreen = value17.As<NSettingsScreen>();
		}
		if (info.TryGetProperty(PropertyName._dailyScreen, out var value18))
		{
			_dailyScreen = value18.As<NDailyRunScreen>();
		}
		if (info.TryGetProperty(PropertyName._dailyLoadScreen, out var value19))
		{
			_dailyLoadScreen = value19.As<NDailyRunLoadScreen>();
		}
		if (info.TryGetProperty(PropertyName._customRunScreen, out var value20))
		{
			_customRunScreen = value20.As<NCustomRunScreen>();
		}
		if (info.TryGetProperty(PropertyName._customRunLoadScreen, out var value21))
		{
			_customRunLoadScreen = value21.As<NCustomRunLoadScreen>();
		}
		if (info.TryGetProperty(PropertyName._moddingScreen, out var value22))
		{
			_moddingScreen = value22.As<NModdingScreen>();
		}
		if (info.TryGetProperty(PropertyName._profileScreen, out var value23))
		{
			_profileScreen = value23.As<NProfileScreen>();
		}
	}
}
