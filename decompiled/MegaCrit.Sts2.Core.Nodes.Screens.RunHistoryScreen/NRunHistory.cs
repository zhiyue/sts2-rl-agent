using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NRunHistory.cs")]
public class NRunHistory : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnLeftButtonButtonReleased = "OnLeftButtonButtonReleased";

		public static readonly StringName OnRightButtonButtonReleased = "OnRightButtonButtonReleased";

		public static readonly StringName CanBeShown = "CanBeShown";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public new static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName SelectPlayer = "SelectPlayer";

		public static readonly StringName LoadGoldHpAndPotionInfo = "LoadGoldHpAndPotionInfo";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _screenContents = "_screenContents";

		public static readonly StringName _playerIconContainer = "_playerIconContainer";

		public static readonly StringName _hpLabel = "_hpLabel";

		public static readonly StringName _goldLabel = "_goldLabel";

		public static readonly StringName _potionHolder = "_potionHolder";

		public static readonly StringName _floorLabel = "_floorLabel";

		public static readonly StringName _timeLabel = "_timeLabel";

		public static readonly StringName _dateLabel = "_dateLabel";

		public static readonly StringName _seedLabel = "_seedLabel";

		public static readonly StringName _gameModeLabel = "_gameModeLabel";

		public static readonly StringName _buildLabel = "_buildLabel";

		public static readonly StringName _deathQuoteLabel = "_deathQuoteLabel";

		public static readonly StringName _mapPointHistory = "_mapPointHistory";

		public static readonly StringName _relicHistory = "_relicHistory";

		public static readonly StringName _deckHistory = "_deckHistory";

		public static readonly StringName _outOfDateVisual = "_outOfDateVisual";

		public static readonly StringName _index = "_index";

		public static readonly StringName _prevButton = "_prevButton";

		public static readonly StringName _nextButton = "_nextButton";

		public static readonly StringName _selectedPlayerIcon = "_selectedPlayerIcon";

		public static readonly StringName _screenTween = "_screenTween";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/run_history_screen/run_history");

	public const string locTable = "run_history";

	private static readonly LocString _leftQuote = new LocString("game_over_screen", "ENCOUNTER_QUOTE_LEFT");

	private static readonly LocString _rightQuote = new LocString("game_over_screen", "ENCOUNTER_QUOTE_RIGHT");

	private Control _screenContents;

	private Control _playerIconContainer;

	private MegaLabel _hpLabel;

	private MegaLabel _goldLabel;

	private Control _potionHolder;

	private MegaLabel _floorLabel;

	private MegaLabel _timeLabel;

	private MegaRichTextLabel _dateLabel;

	private MegaRichTextLabel _seedLabel;

	private MegaRichTextLabel _gameModeLabel;

	private MegaRichTextLabel _buildLabel;

	private MegaRichTextLabel _deathQuoteLabel;

	private NMapPointHistory _mapPointHistory;

	private NRelicHistory _relicHistory;

	private NDeckHistory _deckHistory;

	private Control _outOfDateVisual;

	private readonly List<string> _runNames = new List<string>();

	private int _index;

	private RunHistory _history;

	private NRunHistoryArrowButton _prevButton;

	private NRunHistoryArrowButton _nextButton;

	private NRunHistoryPlayerIcon? _selectedPlayerIcon;

	private Tween? _screenTween;

	protected override Control? InitialFocusedControl => null;

	public static string[] AssetPaths => new string[1] { _scenePath };

	public static NRunHistory? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRunHistory>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_screenContents = GetNode<Control>("ScreenContents");
		_playerIconContainer = GetNode<Control>("%PlayerIconContainer");
		_hpLabel = GetNode<MegaLabel>("%HpLabel");
		_goldLabel = GetNode<MegaLabel>("%GoldLabel");
		_potionHolder = GetNode<Control>("%PotionHolders");
		_floorLabel = GetNode<MegaLabel>("%FloorNumLabel");
		_timeLabel = GetNode<MegaLabel>("%RunTimeLabel");
		_dateLabel = GetNode<MegaRichTextLabel>("%DateLabel");
		_seedLabel = GetNode<MegaRichTextLabel>("%SeedLabel");
		_gameModeLabel = GetNode<MegaRichTextLabel>("%GameModeLabel");
		_buildLabel = GetNode<MegaRichTextLabel>("%BuildLabel");
		_deathQuoteLabel = GetNode<MegaRichTextLabel>("%DeathQuoteLabel");
		_mapPointHistory = GetNode<NMapPointHistory>("%MapPointHistory");
		_relicHistory = GetNode<NRelicHistory>("%RelicHistory");
		_deckHistory = GetNode<NDeckHistory>("%DeckHistory");
		_outOfDateVisual = GetNode<Control>("%OutOfDateVisual");
		_prevButton = GetNode<NRunHistoryArrowButton>("LeftArrow");
		_prevButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnLeftButtonButtonReleased));
		_nextButton = GetNode<NRunHistoryArrowButton>("RightArrow");
		_nextButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnRightButtonButtonReleased));
		_prevButton.IsLeft = true;
		_mapPointHistory.SetDeckHistory(_deckHistory);
		_mapPointHistory.SetRelicHistory(_relicHistory);
	}

	private void OnLeftButtonButtonReleased(NButton _)
	{
		TaskHelper.RunSafely(RefreshAndSelectRun(_index + 1));
		_screenTween?.Kill();
		_screenTween = CreateTween().SetParallel();
		_screenTween.TweenProperty(_screenContents, "position", Vector2.Zero, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out)
			.From(Vector2.Zero + new Vector2(-1000f, 0f));
		_screenTween.TweenProperty(_screenContents, "modulate:a", 1f, 0.4).SetTrans(Tween.TransitionType.Linear).From(0f);
	}

	private void OnRightButtonButtonReleased(NButton _)
	{
		TaskHelper.RunSafely(RefreshAndSelectRun(_index - 1));
		_screenTween?.Kill();
		_screenTween = CreateTween().SetParallel();
		_screenTween.TweenProperty(_screenContents, "position", Vector2.Zero, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out)
			.From(Vector2.Zero + new Vector2(1000f, 0f));
		_screenTween.TweenProperty(_screenContents, "modulate:a", 1f, 0.4).SetTrans(Tween.TransitionType.Linear).From(0f);
	}

	public static bool CanBeShown()
	{
		return SaveManager.Instance.GetRunHistoryCount() > 0;
	}

	public override void OnSubmenuOpened()
	{
		_runNames.Clear();
		_runNames.AddRange(SaveManager.Instance.GetAllRunHistoryNames());
		_runNames.Reverse();
		TaskHelper.RunSafely(RefreshAndSelectRun(0));
	}

	protected override void OnSubmenuShown()
	{
		if (!CanBeShown())
		{
			throw new InvalidOperationException("Tried to show run history screen with no runs!");
		}
		_screenTween?.Kill();
		_screenTween = CreateTween();
		_screenTween.TweenProperty(_screenContents, "modulate:a", 1f, 0.4).From(0f);
	}

	protected override void OnSubmenuHidden()
	{
		_screenTween?.Kill();
	}

	private Task RefreshAndSelectRun(int index)
	{
		if (index < 0 || index >= _runNames.Count)
		{
			Log.Error($"Invalid run index {index}, valid range is 0-{_runNames.Count - 1}");
			return Task.CompletedTask;
		}
		_prevButton.Disable();
		_nextButton.Disable();
		_outOfDateVisual.Visible = false;
		try
		{
			ReadSaveResult<RunHistory> readSaveResult = SaveManager.Instance.LoadRunHistory(_runNames[index]);
			if (readSaveResult.Success)
			{
				DisplayRun(readSaveResult.SaveData);
			}
			else
			{
				Log.Error($"Could not load run {_runNames[index]} at index {index}: {readSaveResult.ErrorMessage} ({readSaveResult.Status})");
				_outOfDateVisual.Visible = true;
			}
		}
		catch (Exception value)
		{
			Log.Error($"Exception {value} while loading run at index {index}");
			_outOfDateVisual.Visible = true;
			throw;
		}
		finally
		{
			_index = index;
			if (index < _runNames.Count - 1)
			{
				_prevButton.Enable();
			}
			if (index > 0)
			{
				_nextButton.Enable();
			}
			_prevButton.Visible = index < _runNames.Count - 1;
			_nextButton.Visible = index > 0;
		}
		return Task.CompletedTask;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsVisibleInTree() || NDevConsole.Instance.Visible || !NControllerManager.Instance.IsUsingController)
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		bool flag = ((control is TextEdit || control is LineEdit) ? true : false);
		if (!flag && ActiveScreenContext.Instance.IsCurrent(this))
		{
			Control control2 = GetViewport().GuiGetFocusOwner();
			if ((control2 == null || !IsAncestorOf(control2)) && (inputEvent.IsActionPressed(MegaInput.left) || inputEvent.IsActionPressed(MegaInput.right) || inputEvent.IsActionPressed(MegaInput.up) || inputEvent.IsActionPressed(MegaInput.down) || inputEvent.IsActionPressed(MegaInput.select)))
			{
				GetViewport()?.SetInputAsHandled();
				_mapPointHistory.DefaultFocusedControl?.TryGrabFocus();
			}
		}
	}

	private void DisplayRun(RunHistory history)
	{
		_selectedPlayerIcon?.Deselect();
		_selectedPlayerIcon = null;
		foreach (NRunHistoryPlayerIcon item in _playerIconContainer.GetChildren().OfType<NRunHistoryPlayerIcon>())
		{
			item.QueueFreeSafely();
		}
		_history = history;
		ulong localPlayerId = PlatformUtil.GetLocalPlayerId(history.PlatformType);
		LoadPlayerFloor(history);
		LoadGameModeDetails(history);
		LoadTimeDetails(history);
		_mapPointHistory.LoadHistory(history);
		bool flag = false;
		NRunHistoryPlayerIcon nRunHistoryPlayerIcon = null;
		foreach (RunHistoryPlayer player in history.Players)
		{
			NRunHistoryPlayerIcon playerIcon = PreloadManager.Cache.GetScene(NRunHistoryPlayerIcon.scenePath).Instantiate<NRunHistoryPlayerIcon>(PackedScene.GenEditState.Disabled);
			if (nRunHistoryPlayerIcon == null)
			{
				nRunHistoryPlayerIcon = playerIcon;
			}
			_playerIconContainer.AddChildSafely(playerIcon);
			playerIcon.LoadRun(player, history);
			playerIcon.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
			{
				SelectPlayer(playerIcon);
			}));
			if (player.Id == localPlayerId)
			{
				flag = true;
				SelectPlayer(playerIcon);
			}
		}
		if (!flag)
		{
			if (history.Players.Count > 1)
			{
				Log.Warn($"Local player with ID {localPlayerId} not found in multiplayer run history file! Defaulting to first player");
			}
			SelectPlayer(nRunHistoryPlayerIcon);
		}
	}

	private void SelectPlayer(NRunHistoryPlayerIcon playerIcon)
	{
		_selectedPlayerIcon?.Deselect();
		_selectedPlayerIcon = playerIcon;
		playerIcon.Select();
		if (_history.Players.Count == 1)
		{
			CharacterModel byId = ModelDb.GetById<CharacterModel>(playerIcon.Player.Character);
			Color nameColor = byId.NameColor;
		}
		else
		{
			LocString locString = new LocString("run_history", "PLAYER_NAME");
			locString.Add("PlayerName", PlatformUtil.GetPlayerName(_history.PlatformType, playerIcon.Player.Id));
		}
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		Player player = Player.CreateForNewRun(ModelDb.GetById<CharacterModel>(playerIcon.Player.Character), unlockState, playerIcon.Player.Id);
		LoadGoldHpAndPotionInfo(playerIcon);
		LoadDeathQuote(_history, playerIcon.Player.Character);
		_mapPointHistory.SetPlayer(playerIcon.Player);
		_relicHistory.LoadRelics(player, playerIcon.Player.Relics);
		_deckHistory.LoadDeck(player, playerIcon.Player.Deck);
	}

	private void LoadGoldHpAndPotionInfo(NRunHistoryPlayerIcon icon)
	{
		if (!_history.MapPointHistory.Any())
		{
			CharacterModel byId = ModelDb.GetById<CharacterModel>(icon.Player.Character);
			_hpLabel.SetTextAutoSize($"{byId.StartingHp}/{byId.StartingHp}");
			_goldLabel.SetTextAutoSize($"{byId.StartingGold}");
		}
		else
		{
			MapPointHistoryEntry mapPointHistoryEntry = _history.MapPointHistory.Last().Last();
			PlayerMapPointHistoryEntry playerMapPointHistoryEntry = mapPointHistoryEntry.PlayerStats.First((PlayerMapPointHistoryEntry stat) => stat.PlayerId == icon.Player.Id);
			_hpLabel.SetTextAutoSize($"{playerMapPointHistoryEntry.CurrentHp}/{playerMapPointHistoryEntry.MaxHp}");
			_goldLabel.SetTextAutoSize($"{playerMapPointHistoryEntry.CurrentGold}");
		}
		_potionHolder.FreeChildren();
		RunHistoryPlayer runHistoryPlayer = _history.Players.First((RunHistoryPlayer player) => player.Id == icon.Player.Id);
		List<PotionModel> list = runHistoryPlayer.Potions.Select(PotionModel.FromSerializable).ToList();
		List<NPotionHolder> list2 = new List<NPotionHolder>();
		for (int num = 0; num < runHistoryPlayer.MaxPotionSlotCount; num++)
		{
			NPotionHolder nPotionHolder = NPotionHolder.Create(isUsable: false);
			_potionHolder.AddChildSafely(nPotionHolder);
			list2.Add(nPotionHolder);
		}
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		Player owner = Player.CreateForNewRun(ModelDb.GetById<CharacterModel>(icon.Player.Character), unlockState, icon.Player.Id);
		for (int num2 = 0; num2 < list.Count && num2 < runHistoryPlayer.MaxPotionSlotCount; num2++)
		{
			NPotion nPotion = NPotion.Create(list[num2]);
			nPotion.Model.Owner = owner;
			list2[num2].AddPotion(nPotion);
			nPotion.Position = Vector2.Zero;
		}
	}

	private void LoadPlayerFloor(RunHistory history)
	{
		int value = history.MapPointHistory.Sum((List<MapPointHistoryEntry> rooms) => rooms.Count);
		_floorLabel.SetTextAutoSize($"{value}");
	}

	private void LoadGameModeDetails(RunHistory history)
	{
		LocString locString = new LocString("run_history", "GAME_MODE.title");
		if (history.Players.Count > 1)
		{
			locString.Add("PlayerCount", new LocString("run_history", "PLAYER_COUNT.multiplayer"));
		}
		else
		{
			locString.Add("PlayerCount", new LocString("run_history", "PLAYER_COUNT.singleplayer"));
		}
		switch (history.GameMode)
		{
		case GameMode.Custom:
			locString.Add("GameMode", new LocString("run_history", "GAME_MODE.custom"));
			break;
		case GameMode.Daily:
			locString.Add("GameMode", new LocString("run_history", "GAME_MODE.daily"));
			break;
		case GameMode.Standard:
			locString.Add("GameMode", new LocString("run_history", "GAME_MODE.standard"));
			break;
		default:
			locString.Add("GameMode", new LocString("run_history", "GAME_MODE.unknown"));
			break;
		}
		_gameModeLabel.Text = "[right]" + locString.GetFormattedText() + "[/right]";
	}

	public static GameOverType GetGameOverType(RunHistory history)
	{
		if (history.Win)
		{
			return GameOverType.FalseVictory;
		}
		if (history.WasAbandoned)
		{
			return GameOverType.AbandonedRun;
		}
		if (history.KilledByEncounter != ModelId.none)
		{
			return GameOverType.CombatDeath;
		}
		if (history.KilledByEvent != ModelId.none)
		{
			return GameOverType.EventDeath;
		}
		Log.Warn("How did the game end??");
		return GameOverType.None;
	}

	public static string GetDeathQuote(RunHistory history, ModelId characterId, GameOverType gameOverType)
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(characterId);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_leftQuote.GetRawText());
		Rng rng = new Rng((uint)StringHelper.GetDeterministicHashCode(history.Seed));
		switch (gameOverType)
		{
		case GameOverType.AbandonedRun:
		{
			LocString randomWithPrefix2 = LocString.GetRandomWithPrefix("run_history", "MAP_POINT_HISTORY.abandon", rng);
			byId.AddDetailsTo(randomWithPrefix2);
			stringBuilder.Append(randomWithPrefix2.GetFormattedText());
			break;
		}
		case GameOverType.EventDeath:
		{
			EventModel eventModel;
			try
			{
				eventModel = ModelDb.GetById<EventModel>(history.KilledByEvent);
			}
			catch (ModelNotFoundException)
			{
				eventModel = ModelDb.Event<DeprecatedEvent>();
			}
			LocString locString2 = new LocString("events", eventModel.Id.Entry + ".loss");
			byId.AddDetailsTo(locString2);
			locString2.Add("event", eventModel.Title);
			stringBuilder.Append(locString2.GetFormattedText());
			break;
		}
		case GameOverType.CombatDeath:
		{
			EncounterModel encounterModel = SaveUtil.EncounterOrDeprecated(history.KilledByEncounter);
			LocString lossMessageFor = encounterModel.GetLossMessageFor(byId);
			stringBuilder.Append(lossMessageFor.GetFormattedText());
			break;
		}
		case GameOverType.FalseVictory:
		{
			LocString randomWithPrefix = LocString.GetRandomWithPrefix("run_history", "MAP_POINT_HISTORY.falseVictory", rng);
			byId.AddDetailsTo(randomWithPrefix);
			stringBuilder.Append(randomWithPrefix.GetFormattedText());
			break;
		}
		case GameOverType.None:
		case GameOverType.TrueVictory:
		{
			LocString locString = new LocString("run_history", "MAP_POINT_HISTORY.debug");
			byId.AddDetailsTo(locString);
			stringBuilder.Append(locString.GetFormattedText());
			break;
		}
		default:
			Log.Error("Unimplemented GameOverType: " + gameOverType);
			throw new ArgumentOutOfRangeException("gameOverType", gameOverType, null);
		}
		stringBuilder.Append(_rightQuote.GetRawText());
		return stringBuilder.ToString();
	}

	private void LoadDeathQuote(RunHistory history, ModelId characterId)
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(characterId);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_leftQuote.GetRawText());
		Rng rng = new Rng((uint)StringHelper.GetDeterministicHashCode(history.Seed));
		if (history.Win)
		{
			_deathQuoteLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor, StsColors.green);
			LocString randomWithPrefix = LocString.GetRandomWithPrefix("run_history", "MAP_POINT_HISTORY.falseVictory", rng);
			byId.AddDetailsTo(randomWithPrefix);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
			handler.AppendFormatted(randomWithPrefix.GetFormattedText());
			stringBuilder3.Append(ref handler);
		}
		else if (history.WasAbandoned)
		{
			_deathQuoteLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor, StsColors.red);
			LocString randomWithPrefix2 = LocString.GetRandomWithPrefix("run_history", "MAP_POINT_HISTORY.abandon", rng);
			byId.AddDetailsTo(randomWithPrefix2);
			stringBuilder.Append(randomWithPrefix2.GetFormattedText());
		}
		else if (history.KilledByEncounter != ModelId.none)
		{
			_deathQuoteLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor, StsColors.red);
			EncounterModel encounterModel = SaveUtil.EncounterOrDeprecated(history.KilledByEncounter);
			LocString lossMessageFor = encounterModel.GetLossMessageFor(byId);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
			handler.AppendFormatted(lossMessageFor.GetFormattedText());
			stringBuilder4.Append(ref handler);
		}
		else if (history.KilledByEvent != ModelId.none)
		{
			_deathQuoteLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.defaultColor, StsColors.red);
			EventModel eventModel;
			try
			{
				eventModel = ModelDb.GetById<EventModel>(history.KilledByEvent);
			}
			catch (ModelNotFoundException)
			{
				eventModel = ModelDb.Event<DeprecatedEvent>();
			}
			string text = eventModel.Id.Entry + ".loss";
			LocString locString = ((!LocString.Exists("events", text)) ? new LocString("run_history", "DEFAULT_EVENT_LOSS_MESSAGE") : new LocString("events", text));
			byId.AddDetailsTo(locString);
			locString.Add("event", eventModel.Title);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
			handler.AppendFormatted(locString.GetFormattedText());
			stringBuilder5.Append(ref handler);
		}
		stringBuilder.Append(_rightQuote.GetRawText());
		_deathQuoteLabel.Text = stringBuilder.ToString();
	}

	private void LoadTimeDetails(RunHistory history)
	{
		DateTimeFormatInfo dateTimeFormat = LocManager.Instance.CultureInfo.DateTimeFormat;
		DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(history.StartTime).UtcDateTime, TimeZoneInfo.Local);
		string value = dateTime.ToString("MMMM d, yyyy", dateTimeFormat);
		string value2 = dateTime.ToString("h:mm tt", dateTimeFormat);
		_dateLabel.Text = $"[right][gold]{value}[/gold], [blue]{value2}[/blue][/right]";
		_seedLabel.Text = "[right][gold]Seed[/gold]: " + history.Seed + "[/right]";
		_buildLabel.Text = "[right]" + history.BuildId + "[/right]";
		_timeLabel.SetTextAutoSize(TimeFormatting.Format(history.RunTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnLeftButtonButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRightButtonButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CanBeShown, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectPlayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "playerIcon", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.LoadGoldHpAndPotionInfo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "icon", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NRunHistory>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLeftButtonButtonReleased && args.Count == 1)
		{
			OnLeftButtonButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRightButtonButtonReleased && args.Count == 1)
		{
			OnRightButtonButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanBeShown());
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectPlayer && args.Count == 1)
		{
			SelectPlayer(VariantUtils.ConvertTo<NRunHistoryPlayerIcon>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadGoldHpAndPotionInfo && args.Count == 1)
		{
			LoadGoldHpAndPotionInfo(VariantUtils.ConvertTo<NRunHistoryPlayerIcon>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NRunHistory>(Create());
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanBeShown());
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
		if (method == MethodName.OnLeftButtonButtonReleased)
		{
			return true;
		}
		if (method == MethodName.OnRightButtonButtonReleased)
		{
			return true;
		}
		if (method == MethodName.CanBeShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.SelectPlayer)
		{
			return true;
		}
		if (method == MethodName.LoadGoldHpAndPotionInfo)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._screenContents)
		{
			_screenContents = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			_playerIconContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hpLabel)
		{
			_hpLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._goldLabel)
		{
			_goldLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._potionHolder)
		{
			_potionHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._floorLabel)
		{
			_floorLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._timeLabel)
		{
			_timeLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			_dateLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._seedLabel)
		{
			_seedLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._gameModeLabel)
		{
			_gameModeLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._buildLabel)
		{
			_buildLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._deathQuoteLabel)
		{
			_deathQuoteLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._mapPointHistory)
		{
			_mapPointHistory = VariantUtils.ConvertTo<NMapPointHistory>(in value);
			return true;
		}
		if (name == PropertyName._relicHistory)
		{
			_relicHistory = VariantUtils.ConvertTo<NRelicHistory>(in value);
			return true;
		}
		if (name == PropertyName._deckHistory)
		{
			_deckHistory = VariantUtils.ConvertTo<NDeckHistory>(in value);
			return true;
		}
		if (name == PropertyName._outOfDateVisual)
		{
			_outOfDateVisual = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			_prevButton = VariantUtils.ConvertTo<NRunHistoryArrowButton>(in value);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			_nextButton = VariantUtils.ConvertTo<NRunHistoryArrowButton>(in value);
			return true;
		}
		if (name == PropertyName._selectedPlayerIcon)
		{
			_selectedPlayerIcon = VariantUtils.ConvertTo<NRunHistoryPlayerIcon>(in value);
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
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._screenContents)
		{
			value = VariantUtils.CreateFrom(in _screenContents);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			value = VariantUtils.CreateFrom(in _playerIconContainer);
			return true;
		}
		if (name == PropertyName._hpLabel)
		{
			value = VariantUtils.CreateFrom(in _hpLabel);
			return true;
		}
		if (name == PropertyName._goldLabel)
		{
			value = VariantUtils.CreateFrom(in _goldLabel);
			return true;
		}
		if (name == PropertyName._potionHolder)
		{
			value = VariantUtils.CreateFrom(in _potionHolder);
			return true;
		}
		if (name == PropertyName._floorLabel)
		{
			value = VariantUtils.CreateFrom(in _floorLabel);
			return true;
		}
		if (name == PropertyName._timeLabel)
		{
			value = VariantUtils.CreateFrom(in _timeLabel);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			value = VariantUtils.CreateFrom(in _dateLabel);
			return true;
		}
		if (name == PropertyName._seedLabel)
		{
			value = VariantUtils.CreateFrom(in _seedLabel);
			return true;
		}
		if (name == PropertyName._gameModeLabel)
		{
			value = VariantUtils.CreateFrom(in _gameModeLabel);
			return true;
		}
		if (name == PropertyName._buildLabel)
		{
			value = VariantUtils.CreateFrom(in _buildLabel);
			return true;
		}
		if (name == PropertyName._deathQuoteLabel)
		{
			value = VariantUtils.CreateFrom(in _deathQuoteLabel);
			return true;
		}
		if (name == PropertyName._mapPointHistory)
		{
			value = VariantUtils.CreateFrom(in _mapPointHistory);
			return true;
		}
		if (name == PropertyName._relicHistory)
		{
			value = VariantUtils.CreateFrom(in _relicHistory);
			return true;
		}
		if (name == PropertyName._deckHistory)
		{
			value = VariantUtils.CreateFrom(in _deckHistory);
			return true;
		}
		if (name == PropertyName._outOfDateVisual)
		{
			value = VariantUtils.CreateFrom(in _outOfDateVisual);
			return true;
		}
		if (name == PropertyName._index)
		{
			value = VariantUtils.CreateFrom(in _index);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			value = VariantUtils.CreateFrom(in _prevButton);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			value = VariantUtils.CreateFrom(in _nextButton);
			return true;
		}
		if (name == PropertyName._selectedPlayerIcon)
		{
			value = VariantUtils.CreateFrom(in _selectedPlayerIcon);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerIconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._goldLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._floorLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timeLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seedLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gameModeLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buildLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathQuoteLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapPointHistory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicHistory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deckHistory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outOfDateVisual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._prevButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nextButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedPlayerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._screenContents, Variant.From(in _screenContents));
		info.AddProperty(PropertyName._playerIconContainer, Variant.From(in _playerIconContainer));
		info.AddProperty(PropertyName._hpLabel, Variant.From(in _hpLabel));
		info.AddProperty(PropertyName._goldLabel, Variant.From(in _goldLabel));
		info.AddProperty(PropertyName._potionHolder, Variant.From(in _potionHolder));
		info.AddProperty(PropertyName._floorLabel, Variant.From(in _floorLabel));
		info.AddProperty(PropertyName._timeLabel, Variant.From(in _timeLabel));
		info.AddProperty(PropertyName._dateLabel, Variant.From(in _dateLabel));
		info.AddProperty(PropertyName._seedLabel, Variant.From(in _seedLabel));
		info.AddProperty(PropertyName._gameModeLabel, Variant.From(in _gameModeLabel));
		info.AddProperty(PropertyName._buildLabel, Variant.From(in _buildLabel));
		info.AddProperty(PropertyName._deathQuoteLabel, Variant.From(in _deathQuoteLabel));
		info.AddProperty(PropertyName._mapPointHistory, Variant.From(in _mapPointHistory));
		info.AddProperty(PropertyName._relicHistory, Variant.From(in _relicHistory));
		info.AddProperty(PropertyName._deckHistory, Variant.From(in _deckHistory));
		info.AddProperty(PropertyName._outOfDateVisual, Variant.From(in _outOfDateVisual));
		info.AddProperty(PropertyName._index, Variant.From(in _index));
		info.AddProperty(PropertyName._prevButton, Variant.From(in _prevButton));
		info.AddProperty(PropertyName._nextButton, Variant.From(in _nextButton));
		info.AddProperty(PropertyName._selectedPlayerIcon, Variant.From(in _selectedPlayerIcon));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._screenContents, out var value))
		{
			_screenContents = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._playerIconContainer, out var value2))
		{
			_playerIconContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hpLabel, out var value3))
		{
			_hpLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._goldLabel, out var value4))
		{
			_goldLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._potionHolder, out var value5))
		{
			_potionHolder = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._floorLabel, out var value6))
		{
			_floorLabel = value6.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._timeLabel, out var value7))
		{
			_timeLabel = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._dateLabel, out var value8))
		{
			_dateLabel = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._seedLabel, out var value9))
		{
			_seedLabel = value9.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._gameModeLabel, out var value10))
		{
			_gameModeLabel = value10.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._buildLabel, out var value11))
		{
			_buildLabel = value11.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._deathQuoteLabel, out var value12))
		{
			_deathQuoteLabel = value12.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._mapPointHistory, out var value13))
		{
			_mapPointHistory = value13.As<NMapPointHistory>();
		}
		if (info.TryGetProperty(PropertyName._relicHistory, out var value14))
		{
			_relicHistory = value14.As<NRelicHistory>();
		}
		if (info.TryGetProperty(PropertyName._deckHistory, out var value15))
		{
			_deckHistory = value15.As<NDeckHistory>();
		}
		if (info.TryGetProperty(PropertyName._outOfDateVisual, out var value16))
		{
			_outOfDateVisual = value16.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._index, out var value17))
		{
			_index = value17.As<int>();
		}
		if (info.TryGetProperty(PropertyName._prevButton, out var value18))
		{
			_prevButton = value18.As<NRunHistoryArrowButton>();
		}
		if (info.TryGetProperty(PropertyName._nextButton, out var value19))
		{
			_nextButton = value19.As<NRunHistoryArrowButton>();
		}
		if (info.TryGetProperty(PropertyName._selectedPlayerIcon, out var value20))
		{
			_selectedPlayerIcon = value20.As<NRunHistoryPlayerIcon>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value21))
		{
			_screenTween = value21.As<Tween>();
		}
	}
}
