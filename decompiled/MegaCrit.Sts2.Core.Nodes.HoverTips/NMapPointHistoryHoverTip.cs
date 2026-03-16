using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.HoverTips;

[ScriptPath("res://src/Core/Nodes/HoverTips/NMapPointHistoryHoverTip.cs")]
public class NMapPointHistoryHoverTip : MarginContainer
{
	public new class MethodName : MarginContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : MarginContainer.PropertyName
	{
		public static readonly StringName _floorNum = "_floorNum";

		public static readonly StringName _playerId = "_playerId";

		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _playerStats = "_playerStats";

		public static readonly StringName _roomStats = "_roomStats";

		public static readonly StringName _rewardStatsContainer = "_rewardStatsContainer";

		public static readonly StringName _skippedStatsContainer = "_skippedStatsContainer";

		public static readonly StringName _actionStats = "_actionStats";
	}

	public new class SignalName : MarginContainer.SignalName
	{
	}

	private readonly LocString _mapPointRoomStatsLoc = new LocString("run_history", "MAP_POINT_HISTORY.room_stats");

	private readonly LocString _mapPointPlayerStatsLoc = new LocString("run_history", "MAP_POINT_HISTORY.player_stats");

	private readonly LocString _combatStats = new LocString("run_history", "MAP_POINT_HISTORY.combatStats");

	private readonly LocString _floorTitle = new LocString("run_history", "MAP_POINT_HISTORY.header");

	private readonly LocString _chose = new LocString("run_history", "MAP_POINT_HISTORY.chose");

	private readonly LocString _skipped = new LocString("run_history", "MAP_POINT_HISTORY.skipped");

	private readonly LocString _rewardsHeaderLoc = new LocString("run_history", "HISTORY_ENTRY.rewardsHeader");

	private readonly LocString _skippedHeaderLoc = new LocString("run_history", "HISTORY_ENTRY.skippedHeader");

	private readonly LocString _enchanted = new LocString("run_history", "HISTORY_ENTRY.enchanted");

	private readonly LocString _obtained = new LocString("run_history", "HISTORY_ENTRY.obtained");

	private readonly LocString _goldGained = new LocString("run_history", "HISTORY_ENTRY.goldGained");

	private readonly LocString _goldSpent = new LocString("run_history", "HISTORY_ENTRY.goldSpent");

	private readonly LocString _goldLost = new LocString("run_history", "HISTORY_ENTRY.goldLost");

	private readonly LocString _goldStolen = new LocString("run_history", "HISTORY_ENTRY.goldStolen");

	private readonly LocString _used = new LocString("run_history", "HISTORY_ENTRY.used");

	private readonly LocString _removed = new LocString("run_history", "HISTORY_ENTRY.removed");

	private readonly LocString _transformed = new LocString("run_history", "HISTORY_ENTRY.transformed");

	private readonly LocString _upgraded = new LocString("run_history", "HISTORY_ENTRY.upgraded");

	private readonly LocString _downgraded = new LocString("run_history", "HISTORY_ENTRY.downgraded");

	private readonly LocString _damaged = new LocString("run_history", "MAP_POINT_HISTORY.damageTaken");

	private readonly LocString _healed = new LocString("run_history", "MAP_POINT_HISTORY.healed");

	private readonly LocString _maxHpGained = new LocString("run_history", "MAP_POINT_HISTORY.maxHpGained");

	private readonly LocString _maxHpLost = new LocString("run_history", "MAP_POINT_HISTORY.maxHpLost");

	private readonly LocString _turns = new LocString("run_history", "MAP_POINT_HISTORY.turnsTaken");

	private readonly LocString _quests = new LocString("run_history", "MAP_POINT_HISTORY.questCompleted");

	private const string _goldIconPath = "res://images/packed/sprite_fonts/gold_icon.png";

	private const string _cardIconPath = "res://images/packed/sprite_fonts/card_icon.png";

	private const string _chestIconPath = "res://images/packed/sprite_fonts/chest_icon.png";

	private const string _potionIconPath = "res://images/packed/sprite_fonts/potion_icon.png";

	private const string _roomHistoryTipScenePath = "res://scenes/ui/map_point_history_hover_tip.tscn";

	private MapPointHistoryEntry _entry;

	private int _floorNum;

	private ulong _playerId;

	private MegaLabel _titleLabel;

	private RichTextLabel _playerStats;

	private RichTextLabel _roomStats;

	private Control _rewardStatsContainer;

	private Control _skippedStatsContainer;

	private List<RichTextLabel> _rewardRows;

	private List<RichTextLabel> _skippedRows;

	private RichTextLabel _actionStats;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/ui/map_point_history_hover_tip.tscn");

	public static NMapPointHistoryHoverTip Create(int floorNum, ulong playerId, MapPointHistoryEntry historyEntry)
	{
		NMapPointHistoryHoverTip nMapPointHistoryHoverTip = PreloadManager.Cache.GetScene("res://scenes/ui/map_point_history_hover_tip.tscn").Instantiate<NMapPointHistoryHoverTip>(PackedScene.GenEditState.Disabled);
		nMapPointHistoryHoverTip._entry = historyEntry;
		nMapPointHistoryHoverTip._floorNum = floorNum;
		nMapPointHistoryHoverTip._playerId = playerId;
		return nMapPointHistoryHoverTip;
	}

	public override void _Ready()
	{
		_titleLabel = GetNode<MegaLabel>("%Title");
		_playerStats = GetNode<RichTextLabel>("%PlayerStats");
		_roomStats = GetNode<RichTextLabel>("%RoomStats");
		_actionStats = GetNode<RichTextLabel>("%CardStats");
		_rewardStatsContainer = GetNode<Control>("%RewardStats");
		_skippedStatsContainer = GetNode<Control>("%SkippedStats");
		_rewardRows = GetNode<Control>("%RewardRows").GetChildren().OfType<RichTextLabel>().ToList();
		_skippedRows = GetNode<Control>("%SkippedRows").GetChildren().OfType<RichTextLabel>().ToList();
		_rewardStatsContainer.GetNode<MegaLabel>("Header").SetTextAutoSize(_rewardsHeaderLoc.GetFormattedText());
		_skippedStatsContainer.GetNode<MegaLabel>("Header").SetTextAutoSize(_skippedHeaderLoc.GetFormattedText());
		_floorTitle.Add("FloorNum", _floorNum);
		_titleLabel.SetTextAutoSize(_floorTitle.GetFormattedText());
		string text = _entry.MapPointType switch
		{
			MapPointType.Shop => "ROOM_MERCHANT", 
			MapPointType.Treasure => "ROOM_TREASURE", 
			MapPointType.RestSite => "ROOM_REST", 
			MapPointType.Monster => "ROOM_ENEMY", 
			MapPointType.Elite => "ROOM_ELITE", 
			MapPointType.Boss => "ROOM_BOSS", 
			MapPointType.Ancient => "ROOM_ANCIENT", 
			_ => null, 
		};
		if (_entry.MapPointType == MapPointType.Unknown)
		{
			text = _entry.Rooms.First().RoomType switch
			{
				RoomType.Monster => "ROOM_UNKNOWN_ENEMY", 
				RoomType.Treasure => "ROOM_UNKNOWN_TREASURE", 
				RoomType.Shop => "ROOM_UNKNOWN_MERCHANT", 
				RoomType.Elite => "ROOM_UNKNOWN_ELITE", 
				RoomType.Event => "ROOM_EVENT", 
				_ => null, 
			};
		}
		_mapPointRoomStatsLoc.Add("MapPointType", new LocString("static_hover_tips", text + ".title"));
		switch (_entry.Rooms.First().RoomType)
		{
		case RoomType.Event:
			_mapPointRoomStatsLoc.Add("ModelTitle", SaveUtil.EventOrDeprecated(_entry.Rooms.First().ModelId).Title);
			break;
		case RoomType.Treasure:
		case RoomType.Shop:
		case RoomType.RestSite:
			_mapPointRoomStatsLoc.Add("ModelTitle", "");
			break;
		default:
			_mapPointRoomStatsLoc.Add("ModelTitle", SaveUtil.EncounterOrDeprecated(_entry.Rooms.First().ModelId).Title);
			break;
		}
		_roomStats.Text = _mapPointRoomStatsLoc.GetFormattedText();
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = _entry.PlayerStats.FirstOrDefault((PlayerMapPointHistoryEntry e) => e.PlayerId == _playerId);
		if (playerMapPointHistoryEntry == null)
		{
			throw new InvalidOperationException($"Player with ID {_playerId} not found in player stats for this run history!");
		}
		_mapPointPlayerStatsLoc.Add("HP", playerMapPointHistoryEntry.CurrentHp);
		_mapPointPlayerStatsLoc.Add("MaxHP", playerMapPointHistoryEntry.MaxHp);
		_mapPointPlayerStatsLoc.Add("Gold", playerMapPointHistoryEntry.CurrentGold);
		_playerStats.Text = _mapPointPlayerStatsLoc.GetFormattedText();
		PopulateActionStats(playerMapPointHistoryEntry);
		PopulateRewardAndSkippedEntries(playerMapPointHistoryEntry);
	}

	private void PopulateActionStats(PlayerMapPointHistoryEntry playerEntry)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (_entry.MapPointType == MapPointType.Ancient)
		{
			LocString ancientPickedChoiceLoc = playerEntry.GetAncientPickedChoiceLoc();
			if (ancientPickedChoiceLoc != null)
			{
				_chose.Add("Choice", ancientPickedChoiceLoc);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
				handler.AppendLiteral("\t");
				handler.AppendFormatted(_chose.GetFormattedText());
				handler.AppendLiteral("\n");
				stringBuilder3.Append(ref handler);
			}
			foreach (LocString item in playerEntry.GetAncientSkippedChoiceLoc())
			{
				_skipped.Add("Choice", item);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
				handler.AppendLiteral("\t");
				handler.AppendFormatted(_skipped.GetFormattedText());
				handler.AppendLiteral("\n");
				stringBuilder4.Append(ref handler);
			}
		}
		else
		{
			ModelId modelId = _entry.FirstRoomOfType(RoomType.Event)?.ModelId;
			EventModel eventModel = ((modelId != null) ? SaveUtil.EventOrDeprecated(modelId) : null);
			if (eventModel != null)
			{
				foreach (EventOptionHistoryEntry eventChoice in playerEntry.EventChoices)
				{
					eventModel.DynamicVars.AddTo(eventChoice.Title);
					if (eventChoice.Variables != null)
					{
						foreach (KeyValuePair<string, object> variable in eventChoice.Variables)
						{
							eventChoice.Title.AddObj(variable.Key, variable.Value);
						}
					}
					_chose.Add("Choice", eventChoice.Title.GetFormattedText());
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
					handler.AppendLiteral("\t");
					handler.AppendFormatted(_chose.GetFormattedText());
					handler.AppendLiteral("\n");
					stringBuilder5.Append(ref handler);
				}
			}
		}
		foreach (string restSiteChoice in playerEntry.RestSiteChoices)
		{
			LocString locString = new LocString("rest_site_ui", "OPTION_" + restSiteChoice + ".name");
			_chose.Add("Choice", locString.GetFormattedText() ?? "");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_chose.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder6.Append(ref handler);
		}
		MapPointRoomHistoryEntry mapPointRoomHistoryEntry = _entry.Rooms.FirstOrDefault((MapPointRoomHistoryEntry r) => r.RoomType.IsCombatRoom());
		if (playerEntry.MaxHpLost > 0)
		{
			_maxHpLost.Add("HP", playerEntry.MaxHpLost);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_maxHpLost.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder7.Append(ref handler);
		}
		if (playerEntry.DamageTaken > 0 || mapPointRoomHistoryEntry != null)
		{
			_damaged.Add("Damage", playerEntry.DamageTaken);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_damaged.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder8.Append(ref handler);
		}
		if (playerEntry.MaxHpGained > 0)
		{
			_maxHpGained.Add("HP", playerEntry.MaxHpGained);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_maxHpGained.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder9.Append(ref handler);
		}
		if (playerEntry.HpHealed > 0)
		{
			_healed.Add("HP", playerEntry.HpHealed);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_healed.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder10.Append(ref handler);
		}
		if (mapPointRoomHistoryEntry != null)
		{
			_turns.Add("Turns", mapPointRoomHistoryEntry.TurnsTaken);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_turns.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder11.Append(ref handler);
		}
		foreach (ModelId completedQuest in playerEntry.CompletedQuests)
		{
			_quests.Add("Quest", SaveUtil.CardOrDeprecated(completedQuest).Title);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder12 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(_quests.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder12.Append(ref handler);
		}
		foreach (ModelId item2 in playerEntry.PotionUsed)
		{
			_used.Add("Icon", "[img=top]res://images/packed/sprite_fonts/potion_icon.png[/img]");
			_used.Add("Title", SaveUtil.PotionOrDeprecated(item2).Title);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder13 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(_used.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder13.Append(ref handler);
		}
		foreach (ModelId item3 in playerEntry.PotionDiscarded)
		{
			_removed.Add("Icon", "[img=top]res://images/packed/sprite_fonts/potion_icon.png[/img]");
			_removed.Add("Title", SaveUtil.PotionOrDeprecated(item3).Title);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder14 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(_removed.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder14.Append(ref handler);
		}
		if (playerEntry.GoldSpent > 0)
		{
			_goldSpent.Add("Amount", playerEntry.GoldSpent);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder15 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(_goldSpent.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder15.Append(ref handler);
		}
		if (playerEntry.GoldLost > 0)
		{
			_goldLost.Add("Amount", playerEntry.GoldLost);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder16 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(_goldLost.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder16.Append(ref handler);
		}
		if (playerEntry.GoldStolen > 0)
		{
			_goldStolen.Add("Icon", "[img=top]res://images/packed/sprite_fonts/gold_icon.png[/img]");
			_goldStolen.Add("Amount", playerEntry.GoldStolen);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder17 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(_goldStolen.GetFormattedText());
			handler.AppendLiteral("\n");
			stringBuilder17.Append(ref handler);
		}
		_actionStats.Text = stringBuilder.ToString().Trim('\n');
	}

	private void PopulateRewardAndSkippedEntries(PlayerMapPointHistoryEntry playerEntry)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (playerEntry.GoldGained > 0)
		{
			_goldGained.Add("Icon", "[img=top]res://images/packed/sprite_fonts/gold_icon.png[/img]");
			_goldGained.Add("Amount", playerEntry.GoldGained);
			list.Add(_goldGained.GetFormattedText());
		}
		foreach (SerializableCard item in playerEntry.CardsGained)
		{
			_obtained.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_obtained.Add("Title", CardModel.FromSerializable(item).Title);
			list.Add(_obtained.GetFormattedText());
		}
		foreach (CardChoiceHistoryEntry cardChoice in playerEntry.CardChoices)
		{
			if (!cardChoice.wasPicked)
			{
				_obtained.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
				_obtained.Add("Title", CardModel.FromSerializable(cardChoice.Card).Title);
				list2.Add(_obtained.GetFormattedText());
			}
		}
		foreach (ModelChoiceHistoryEntry relicChoice in playerEntry.RelicChoices)
		{
			_obtained.Add("Icon", "[img=top]res://images/packed/sprite_fonts/chest_icon.png[/img]");
			_obtained.Add("Title", SaveUtil.RelicOrDeprecated(relicChoice.choice).Title);
			if (relicChoice.wasPicked)
			{
				list.Add(_obtained.GetFormattedText());
			}
			else
			{
				list2.Add(_obtained.GetFormattedText());
			}
		}
		foreach (ModelChoiceHistoryEntry potionChoice in playerEntry.PotionChoices)
		{
			_obtained.Add("Icon", "[img=top]res://images/packed/sprite_fonts/potion_icon.png[/img]");
			_obtained.Add("Title", SaveUtil.PotionOrDeprecated(potionChoice.choice).Title);
			if (potionChoice.wasPicked)
			{
				list.Add(_obtained.GetFormattedText());
			}
			else
			{
				list2.Add(_obtained.GetFormattedText());
			}
		}
		foreach (SerializableCard item2 in playerEntry.CardsRemoved)
		{
			_removed.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_removed.Add("Title", CardModel.FromSerializable(item2).Title);
			list.Add(_removed.GetFormattedText() ?? "");
		}
		foreach (ModelId item3 in playerEntry.RelicsRemoved)
		{
			_removed.Add("Icon", "[img=top]res://images/packed/sprite_fonts/chest_icon.png[/img]");
			_removed.Add("Title", SaveUtil.RelicOrDeprecated(item3).Title);
			list.Add(_removed.GetFormattedText() ?? "");
		}
		foreach (ModelId upgradedCard in playerEntry.UpgradedCards)
		{
			_upgraded.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_upgraded.Add("Title", SaveUtil.CardOrDeprecated(upgradedCard).Title);
			list.Add(_upgraded.GetFormattedText() ?? "");
		}
		foreach (ModelId downgradedCard in playerEntry.DowngradedCards)
		{
			_downgraded.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_downgraded.Add("Title", SaveUtil.CardOrDeprecated(downgradedCard).Title);
			list.Add(_downgraded.GetFormattedText() ?? "");
		}
		foreach (CardEnchantmentHistoryEntry item4 in playerEntry.CardsEnchanted)
		{
			_enchanted.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_enchanted.Add("Title1", CardModel.FromSerializable(item4.Card).Title);
			_enchanted.Add("Title2", SaveUtil.EnchantmentOrDeprecated(item4.Enchantment).Title);
			list.Add(_enchanted.GetFormattedText() ?? "");
		}
		foreach (CardTransformationHistoryEntry item5 in playerEntry.CardsTransformed)
		{
			_transformed.Add("Icon", "[img=top]res://images/packed/sprite_fonts/card_icon.png[/img]");
			_transformed.Add("Title1", CardModel.FromSerializable(item5.OriginalCard).Title);
			_transformed.Add("Title2", CardModel.FromSerializable(item5.FinalCard).Title);
			list.Add(_transformed.GetFormattedText() ?? "");
		}
		int num = Mathf.Max(5, Mathf.CeilToInt((float)list.Count / 2f));
		_rewardRows[1].Visible = list.Count > 5;
		_rewardStatsContainer.Visible = list.Count > 0;
		for (int i = 0; i < list.Count; i++)
		{
			RichTextLabel richTextLabel = _rewardRows[i / num];
			richTextLabel.Text = richTextLabel.Text + "\t" + list[i] + "\n";
		}
		int num2 = Mathf.Max(5, Mathf.CeilToInt((float)list2.Count / 2f));
		_skippedRows[1].Visible = list2.Count > 5;
		_skippedStatsContainer.Visible = list2.Count > 0;
		for (int j = 0; j < list2.Count; j++)
		{
			RichTextLabel richTextLabel2 = _skippedRows[j / num2];
			richTextLabel2.Text = richTextLabel2.Text + "\t" + list2[j] + "\n";
		}
		foreach (RichTextLabel rewardRow in _rewardRows)
		{
			rewardRow.Text = rewardRow.Text.Trim('\n');
		}
		foreach (RichTextLabel skippedRow in _skippedRows)
		{
			skippedRow.Text = skippedRow.Text.Trim('\n');
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
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
		if (name == PropertyName._floorNum)
		{
			_floorNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._playerId)
		{
			_playerId = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._playerStats)
		{
			_playerStats = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._roomStats)
		{
			_roomStats = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._rewardStatsContainer)
		{
			_rewardStatsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._skippedStatsContainer)
		{
			_skippedStatsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._actionStats)
		{
			_actionStats = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._floorNum)
		{
			value = VariantUtils.CreateFrom(in _floorNum);
			return true;
		}
		if (name == PropertyName._playerId)
		{
			value = VariantUtils.CreateFrom(in _playerId);
			return true;
		}
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
		if (name == PropertyName._playerStats)
		{
			value = VariantUtils.CreateFrom(in _playerStats);
			return true;
		}
		if (name == PropertyName._roomStats)
		{
			value = VariantUtils.CreateFrom(in _roomStats);
			return true;
		}
		if (name == PropertyName._rewardStatsContainer)
		{
			value = VariantUtils.CreateFrom(in _rewardStatsContainer);
			return true;
		}
		if (name == PropertyName._skippedStatsContainer)
		{
			value = VariantUtils.CreateFrom(in _skippedStatsContainer);
			return true;
		}
		if (name == PropertyName._actionStats)
		{
			value = VariantUtils.CreateFrom(in _actionStats);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._floorNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._playerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._roomStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardStatsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._skippedStatsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actionStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._floorNum, Variant.From(in _floorNum));
		info.AddProperty(PropertyName._playerId, Variant.From(in _playerId));
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._playerStats, Variant.From(in _playerStats));
		info.AddProperty(PropertyName._roomStats, Variant.From(in _roomStats));
		info.AddProperty(PropertyName._rewardStatsContainer, Variant.From(in _rewardStatsContainer));
		info.AddProperty(PropertyName._skippedStatsContainer, Variant.From(in _skippedStatsContainer));
		info.AddProperty(PropertyName._actionStats, Variant.From(in _actionStats));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._floorNum, out var value))
		{
			_floorNum = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._playerId, out var value2))
		{
			_playerId = value2.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._titleLabel, out var value3))
		{
			_titleLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._playerStats, out var value4))
		{
			_playerStats = value4.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._roomStats, out var value5))
		{
			_roomStats = value5.As<RichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._rewardStatsContainer, out var value6))
		{
			_rewardStatsContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._skippedStatsContainer, out var value7))
		{
			_skippedStatsContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._actionStats, out var value8))
		{
			_actionStats = value8.As<RichTextLabel>();
		}
	}
}
