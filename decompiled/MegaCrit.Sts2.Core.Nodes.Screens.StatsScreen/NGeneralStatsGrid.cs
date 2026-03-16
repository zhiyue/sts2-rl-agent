using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NGeneralStatsGrid.cs")]
public class NGeneralStatsGrid : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName CreateSection = "CreateSection";

		public static readonly StringName LoadStats = "LoadStats";

		public static readonly StringName SetupHoverTips = "SetupHoverTips";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _gridContainer = "_gridContainer";

		public static readonly StringName _achievementsEntry = "_achievementsEntry";

		public static readonly StringName _playtimeEntry = "_playtimeEntry";

		public static readonly StringName _cardsEntry = "_cardsEntry";

		public static readonly StringName _winLossEntry = "_winLossEntry";

		public static readonly StringName _monsterEntry = "_monsterEntry";

		public static readonly StringName _relicEntry = "_relicEntry";

		public static readonly StringName _potionEntry = "_potionEntry";

		public static readonly StringName _eventsEntry = "_eventsEntry";

		public static readonly StringName _streakEntry = "_streakEntry";

		public static readonly StringName _characterStatContainer = "_characterStatContainer";

		public static readonly StringName _screenTween = "_screenTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _achievementsIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_achievements.tres");

	private static readonly string _playtimeIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_clock.tres");

	private static readonly string _cardsIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_cards.tres");

	private static readonly string _winLossIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_swords.tres");

	private static readonly string _monsterIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_monsters.tres");

	private static readonly string _ancientsIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_ancients.tres");

	private static readonly string _relicIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_chest.tres");

	private static readonly string _potionIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_potions_seen.tres");

	private static readonly string _eventsIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_questionmark.tres");

	private static readonly string _streakIconPath = ImageHelper.GetImagePath("atlases/stats_screen_atlas.sprites/stats_chain.tres");

	private Node _gridContainer;

	private NStatEntry _achievementsEntry;

	private NStatEntry _playtimeEntry;

	private NStatEntry _cardsEntry;

	private NStatEntry _winLossEntry;

	private NStatEntry _monsterEntry;

	private NStatEntry _relicEntry;

	private NStatEntry _potionEntry;

	private NStatEntry _eventsEntry;

	private NStatEntry _streakEntry;

	private Control _characterStatContainer;

	private Tween? _screenTween;

	public static string[] AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_achievementsIconPath);
			list.Add(_playtimeIconPath);
			list.Add(_cardsIconPath);
			list.Add(_winLossIconPath);
			list.Add(_monsterIconPath);
			list.Add(_ancientsIconPath);
			list.Add(_relicIconPath);
			list.Add(_potionIconPath);
			list.Add(_eventsIconPath);
			list.Add(_streakIconPath);
			list.AddRange(NCharacterStats.AssetPaths);
			return list.ToArray();
		}
	}

	private static HoverTip PlaytimeTip => new HoverTip(new LocString("stats_screen", "TIP_PLAYTIME.header"), new LocString("stats_screen", "TIP_PLAYTIME.description"));

	private static HoverTip WinsLossesTip => new HoverTip(new LocString("stats_screen", "TIP_WIN_LOSS.header"), new LocString("stats_screen", "TIP_WIN_LOSS.description"));

	public Control DefaultFocusedControl => _achievementsEntry;

	public override void _Ready()
	{
		_gridContainer = GetNode<Control>("%GridContainer");
		_characterStatContainer = GetNode<Control>("%CharacterStatsContainer");
		_achievementsEntry = CreateSection(_achievementsIconPath);
		_playtimeEntry = CreateSection(_playtimeIconPath);
		_cardsEntry = CreateSection(_cardsIconPath);
		_winLossEntry = CreateSection(_winLossIconPath);
		_monsterEntry = CreateSection(_monsterIconPath);
		_relicEntry = CreateSection(_relicIconPath);
		_potionEntry = CreateSection(_potionIconPath);
		_eventsEntry = CreateSection(_eventsIconPath);
		_streakEntry = CreateSection(_streakIconPath);
		SetupHoverTips();
	}

	private NStatEntry CreateSection(string imgUrl)
	{
		NStatEntry nStatEntry = NStatEntry.Create(imgUrl);
		_gridContainer.AddChildSafely(nStatEntry);
		return nStatEntry;
	}

	public void LoadStats()
	{
		ProgressState progressSave = SaveManager.Instance.Progress;
		SaveManager instance = SaveManager.Instance;
		LocString locString = new LocString("stats_screen", "ENTRY_ACHIEVEMENTS.top");
		int denominator = AchievementsUtil.TotalAchievementCount();
		int numerator = AchievementsUtil.UnlockedAchievementCount();
		locString.Add("Amount", StringHelper.RatioFormat(numerator, denominator));
		_achievementsEntry.SetTopText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_ACHIEVEMENTS.bottom");
		int numerator2 = progressSave.Epochs.Count((SerializableEpoch epoch) => epoch.State >= EpochState.Revealed);
		if (EpochModel.AllEpochIds.All((string id) => progressSave.Epochs.Any((SerializableEpoch epoch) => epoch.Id == id)))
		{
			int count = progressSave.Epochs.Count;
			locString.Add("Amount", StringHelper.RatioFormat(numerator2, count));
		}
		else
		{
			locString.Add("Amount", StringHelper.RatioFormat(numerator2.ToString(), "??"));
		}
		_achievementsEntry.SetBottomText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_PLAYTIME.top");
		locString.Add("Playtime", TimeFormatting.Format(progressSave.TotalPlaytime));
		_playtimeEntry.SetTopText(locString.GetFormattedText());
		if (progressSave.Wins > 0)
		{
			locString = new LocString("stats_screen", "ENTRY_PLAYTIME.bottom");
			locString.Add("FastestWin", TimeFormatting.Format(progressSave.FastestVictory));
			_playtimeEntry.SetBottomText(locString.GetFormattedText());
		}
		locString = new LocString("stats_screen", "ENTRY_CARDS.top");
		locString.Add("Amount", StringHelper.RatioFormat(instance.GetTotalUnlockedCards(), SaveManager.GetUnlockableCardCount()));
		_cardsEntry.SetTopText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_CARDS.bottom");
		locString.Add("Amount", StringHelper.RatioFormat(progressSave.DiscoveredCards.Count, ModelDb.AllCards.Count()));
		_cardsEntry.SetBottomText(locString.GetFormattedText());
		int aggregateAscensionProgress = instance.GetAggregateAscensionProgress();
		if (aggregateAscensionProgress > 0)
		{
			locString = new LocString("stats_screen", "ENTRY_WIN_LOSS.top");
			locString.Add("Amount", StringHelper.RatioFormat(aggregateAscensionProgress, SaveManager.GetAggregateAscensionCount()));
			_winLossEntry.SetTopText(locString.GetFormattedText() ?? "");
		}
		locString = new LocString("stats_screen", "ENTRY_WIN_LOSS.bottom");
		locString.Add("Wins", progressSave.Wins);
		locString.Add("Losses", progressSave.Losses);
		_winLossEntry.SetBottomText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_MONSTER.top");
		locString.Add("Amount", StringHelper.Radix(instance.GetTotalKills()));
		_monsterEntry.SetTopText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_MONSTER.bottom");
		locString.Add("Amount", StringHelper.RatioFormat(instance.Progress.EnemyStats.Count, ModelDb.Monsters.Count()));
		_monsterEntry.SetBottomText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_RELIC.top");
		locString.Add("Amount", StringHelper.RatioFormat(instance.GetTotalUnlockedRelics(), SaveManager.GetUnlockableRelicCount()));
		_relicEntry.SetTopText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_RELIC.bottom");
		locString.Add("Amount", StringHelper.RatioFormat(progressSave.DiscoveredRelics.Count, ModelDb.AllRelics.Count()));
		_relicEntry.SetBottomText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_POTION.top");
		locString.Add("Amount", StringHelper.RatioFormat(instance.GetTotalUnlockedPotions(), SaveManager.GetUnlockablePotionCount()));
		_potionEntry.SetTopText(locString.GetFormattedText() ?? "");
		locString = new LocString("stats_screen", "ENTRY_POTION.bottom");
		locString.Add("Amount", ModelDb.AllPotions.Count());
		_potionEntry.SetBottomText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_EVENTS.top");
		locString.Add("Amount", "N/A");
		_eventsEntry.SetTopText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_EVENTS.bottom");
		locString.Add("Amount", StringHelper.RatioFormat(progressSave.DiscoveredEvents.Count, ModelDb.AllEvents.Count()));
		_eventsEntry.SetBottomText(locString.GetFormattedText());
		locString = new LocString("stats_screen", "ENTRY_STREAK.top");
		locString.Add("Amount", progressSave.BestWinStreak);
		_streakEntry.SetTopText(locString.GetFormattedText());
		if (aggregateAscensionProgress > 99999999)
		{
			locString = new LocString("stats_screen", "ENTRY_STREAK.bottom");
			locString.Add("Amount", 5m);
			_streakEntry.SetBottomText("[red]" + locString.GetFormattedText() + "[/red]");
		}
		_characterStatContainer.FreeChildren();
		CreateCharacterSection(progressSave, ModelDb.Character<Ironclad>().Id);
		CreateCharacterSection(progressSave, ModelDb.Character<Silent>().Id);
		CreateCharacterSection(progressSave, ModelDb.Character<Regent>().Id);
		CreateCharacterSection(progressSave, ModelDb.Character<Necrobinder>().Id);
		CreateCharacterSection(progressSave, ModelDb.Character<Defect>().Id);
	}

	private void CreateCharacterSection(ProgressState progressSave, ModelId id)
	{
		CharacterStats statsForCharacter = progressSave.GetStatsForCharacter(id);
		if (statsForCharacter != null)
		{
			NCharacterStats child = NCharacterStats.Create(statsForCharacter);
			_characterStatContainer.AddChildSafely(child);
		}
	}

	private void SetupHoverTips()
	{
		_playtimeEntry.SetHoverTip(PlaytimeTip);
		int aggregateAscensionProgress = SaveManager.Instance.GetAggregateAscensionProgress();
		if (aggregateAscensionProgress > 0)
		{
			_winLossEntry.SetHoverTip(WinsLossesTip);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateSection, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "imgUrl", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.LoadStats, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetupHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.CreateSection && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NStatEntry>(CreateSection(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.LoadStats && args.Count == 0)
		{
			LoadStats();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupHoverTips && args.Count == 0)
		{
			SetupHoverTips();
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
		if (method == MethodName.CreateSection)
		{
			return true;
		}
		if (method == MethodName.LoadStats)
		{
			return true;
		}
		if (method == MethodName.SetupHoverTips)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._gridContainer)
		{
			_gridContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._achievementsEntry)
		{
			_achievementsEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._playtimeEntry)
		{
			_playtimeEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._cardsEntry)
		{
			_cardsEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._winLossEntry)
		{
			_winLossEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._monsterEntry)
		{
			_monsterEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._relicEntry)
		{
			_relicEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._potionEntry)
		{
			_potionEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._eventsEntry)
		{
			_eventsEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._streakEntry)
		{
			_streakEntry = VariantUtils.ConvertTo<NStatEntry>(in value);
			return true;
		}
		if (name == PropertyName._characterStatContainer)
		{
			_characterStatContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			_screenTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._gridContainer)
		{
			value = VariantUtils.CreateFrom(in _gridContainer);
			return true;
		}
		if (name == PropertyName._achievementsEntry)
		{
			value = VariantUtils.CreateFrom(in _achievementsEntry);
			return true;
		}
		if (name == PropertyName._playtimeEntry)
		{
			value = VariantUtils.CreateFrom(in _playtimeEntry);
			return true;
		}
		if (name == PropertyName._cardsEntry)
		{
			value = VariantUtils.CreateFrom(in _cardsEntry);
			return true;
		}
		if (name == PropertyName._winLossEntry)
		{
			value = VariantUtils.CreateFrom(in _winLossEntry);
			return true;
		}
		if (name == PropertyName._monsterEntry)
		{
			value = VariantUtils.CreateFrom(in _monsterEntry);
			return true;
		}
		if (name == PropertyName._relicEntry)
		{
			value = VariantUtils.CreateFrom(in _relicEntry);
			return true;
		}
		if (name == PropertyName._potionEntry)
		{
			value = VariantUtils.CreateFrom(in _potionEntry);
			return true;
		}
		if (name == PropertyName._eventsEntry)
		{
			value = VariantUtils.CreateFrom(in _eventsEntry);
			return true;
		}
		if (name == PropertyName._streakEntry)
		{
			value = VariantUtils.CreateFrom(in _streakEntry);
			return true;
		}
		if (name == PropertyName._characterStatContainer)
		{
			value = VariantUtils.CreateFrom(in _characterStatContainer);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			value = VariantUtils.CreateFrom(in _screenTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gridContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._achievementsEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playtimeEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardsEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._winLossEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eventsEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._streakEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterStatContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._gridContainer, Variant.From(in _gridContainer));
		info.AddProperty(PropertyName._achievementsEntry, Variant.From(in _achievementsEntry));
		info.AddProperty(PropertyName._playtimeEntry, Variant.From(in _playtimeEntry));
		info.AddProperty(PropertyName._cardsEntry, Variant.From(in _cardsEntry));
		info.AddProperty(PropertyName._winLossEntry, Variant.From(in _winLossEntry));
		info.AddProperty(PropertyName._monsterEntry, Variant.From(in _monsterEntry));
		info.AddProperty(PropertyName._relicEntry, Variant.From(in _relicEntry));
		info.AddProperty(PropertyName._potionEntry, Variant.From(in _potionEntry));
		info.AddProperty(PropertyName._eventsEntry, Variant.From(in _eventsEntry));
		info.AddProperty(PropertyName._streakEntry, Variant.From(in _streakEntry));
		info.AddProperty(PropertyName._characterStatContainer, Variant.From(in _characterStatContainer));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._gridContainer, out var value))
		{
			_gridContainer = value.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._achievementsEntry, out var value2))
		{
			_achievementsEntry = value2.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._playtimeEntry, out var value3))
		{
			_playtimeEntry = value3.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._cardsEntry, out var value4))
		{
			_cardsEntry = value4.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._winLossEntry, out var value5))
		{
			_winLossEntry = value5.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._monsterEntry, out var value6))
		{
			_monsterEntry = value6.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._relicEntry, out var value7))
		{
			_relicEntry = value7.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._potionEntry, out var value8))
		{
			_potionEntry = value8.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._eventsEntry, out var value9))
		{
			_eventsEntry = value9.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._streakEntry, out var value10))
		{
			_streakEntry = value10.As<NStatEntry>();
		}
		if (info.TryGetProperty(PropertyName._characterStatContainer, out var value11))
		{
			_characterStatContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value12))
		{
			_screenTween = value12.As<Tween>();
		}
	}
}
