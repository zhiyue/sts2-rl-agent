using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NCharacterStats.cs")]
public class NCharacterStats : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName LoadStats = "LoadStats";

		public static readonly StringName CreateSection = "CreateSection";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _characterIcon = "_characterIcon";

		public static readonly StringName _statsContainer = "_statsContainer";

		public static readonly StringName _nameLabel = "_nameLabel";

		public static readonly StringName _unlocksLabel = "_unlocksLabel";

		public static readonly StringName _playtimeEntry = "_playtimeEntry";

		public static readonly StringName _winLossEntry = "_winLossEntry";

		public static readonly StringName _streakEntry = "_streakEntry";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly string _playtimeIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_clock.tres");

	private static readonly string _winLossIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_swords.tres");

	private static readonly string _chainIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_chain.tres");

	private CharacterStats _characterStats;

	private Control _characterIcon;

	private Node _statsContainer;

	private MegaLabel _nameLabel;

	private MegaLabel _unlocksLabel;

	private NStatEntry _playtimeEntry;

	private NStatEntry _winLossEntry;

	private NStatEntry _streakEntry;

	public static string[] AssetPaths => new string[4] { ScenePath, _playtimeIconPath, _winLossIconPath, _chainIconPath };

	private static string ScenePath => SceneHelper.GetScenePath("screens/stats_screen/character_stats");

	public static NCharacterStats Create(CharacterStats characterStats)
	{
		NCharacterStats nCharacterStats = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCharacterStats>(PackedScene.GenEditState.Disabled);
		nCharacterStats._characterStats = characterStats;
		return nCharacterStats;
	}

	public override void _Ready()
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(_characterStats.Id);
		_characterIcon = GetNode<Control>("%CharacterIcon");
		_characterIcon.AddChildSafely(byId.Icon);
		_statsContainer = GetNode<Node>("%StatsContainer");
		_playtimeEntry = CreateSection(_playtimeIconPath);
		_winLossEntry = CreateSection(_winLossIconPath);
		_streakEntry = CreateSection(_chainIconPath);
		_nameLabel = GetNode<MegaLabel>("%NameLabel");
		_unlocksLabel = GetNode<MegaLabel>("%UnlocksLabel");
		_nameLabel.SetTextAutoSize(byId.Title.GetRawText());
		_nameLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, byId.NameColor);
		LoadStats();
	}

	private void LoadStats()
	{
		_unlocksLabel.Visible = false;
		LocString locString = new LocString("stats_screen", "ENTRY_CHAR_PLAYTIME.top");
		locString.Add("Playtime", TimeFormatting.Format(_characterStats.Playtime));
		_playtimeEntry.SetTopText(locString.GetFormattedText());
		if (_characterStats.FastestWinTime >= 0)
		{
			locString = new LocString("stats_screen", "ENTRY_CHAR_PLAYTIME.bottom");
			locString.Add("FastestWin", TimeFormatting.Format(_characterStats.FastestWinTime));
			_playtimeEntry.SetBottomText(locString.GetFormattedText());
		}
		locString = new LocString("stats_screen", "ENTRY_CHAR_WIN_LOSS.top");
		if (_characterStats.MaxAscension > 0)
		{
			locString.Add("Amount", _characterStats.MaxAscension);
			_winLossEntry.SetTopText("[red]" + locString.GetFormattedText() + "[/red]");
		}
		locString = new LocString("stats_screen", "ENTRY_CHAR_WIN_LOSS.bottom");
		locString.Add("Wins", _characterStats.TotalWins);
		locString.Add("Losses", _characterStats.TotalLosses);
		_winLossEntry.SetBottomText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_CHAR_STREAK.top");
		locString.Add("Amount", _characterStats.CurrentWinStreak);
		_streakEntry.SetTopText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_CHAR_STREAK.bottom");
		locString.Add("Amount", _characterStats.BestWinStreak);
		_streakEntry.SetBottomText(locString.GetFormattedText());
	}

	private NStatEntry CreateSection(string imgUrl)
	{
		NStatEntry nStatEntry = NStatEntry.Create(imgUrl);
		_statsContainer.AddChildSafely(nStatEntry);
		return nStatEntry;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LoadStats, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateSection, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "imgUrl", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.LoadStats && args.Count == 0)
		{
			LoadStats();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateSection && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NStatEntry>(CreateSection(VariantUtils.ConvertTo<string>(in args[0])));
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
		if (method == MethodName.LoadStats)
		{
			return true;
		}
		if (method == MethodName.CreateSection)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._characterIcon)
		{
			_characterIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._statsContainer)
		{
			_statsContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			_nameLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._unlocksLabel)
		{
			_unlocksLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._playtimeEntry)
		{
			_playtimeEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._winLossEntry)
		{
			_winLossEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._streakEntry)
		{
			_streakEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._characterIcon)
		{
			value = VariantUtils.CreateFrom(in _characterIcon);
			return true;
		}
		if (name == PropertyName._statsContainer)
		{
			value = VariantUtils.CreateFrom(in _statsContainer);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			value = VariantUtils.CreateFrom(in _nameLabel);
			return true;
		}
		if (name == PropertyName._unlocksLabel)
		{
			value = VariantUtils.CreateFrom(in _unlocksLabel);
			return true;
		}
		if (name == PropertyName._playtimeEntry)
		{
			value = VariantUtils.CreateFrom(in _playtimeEntry);
			return true;
		}
		if (name == PropertyName._winLossEntry)
		{
			value = VariantUtils.CreateFrom(in _winLossEntry);
			return true;
		}
		if (name == PropertyName._streakEntry)
		{
			value = VariantUtils.CreateFrom(in _streakEntry);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlocksLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playtimeEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._winLossEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._streakEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._characterIcon, Variant.From(in _characterIcon));
		info.AddProperty(PropertyName._statsContainer, Variant.From(in _statsContainer));
		info.AddProperty(PropertyName._nameLabel, Variant.From(in _nameLabel));
		info.AddProperty(PropertyName._unlocksLabel, Variant.From(in _unlocksLabel));
		info.AddProperty(PropertyName._playtimeEntry, Variant.From(in _playtimeEntry));
		info.AddProperty(PropertyName._winLossEntry, Variant.From(in _winLossEntry));
		info.AddProperty(PropertyName._streakEntry, Variant.From(in _streakEntry));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._characterIcon, out var value))
		{
			_characterIcon = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._statsContainer, out var value2))
		{
			_statsContainer = value2.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._nameLabel, out var value3))
		{
			_nameLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._unlocksLabel, out var value4))
		{
			_unlocksLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._playtimeEntry, out var value5))
		{
			_playtimeEntry = value5.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._winLossEntry, out var value6))
		{
			_winLossEntry = value6.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._streakEntry, out var value7))
		{
			_streakEntry = value7.As<NStatEntry>();
		}
	}
}
