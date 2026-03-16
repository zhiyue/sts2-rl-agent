using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

[ScriptPath("res://src/Core/Nodes/Screens/GameOverScreen/NGameOverScreen.cs")]
public class NGameOverScreen : NClickableControl, IOverlayScreen, IScreenContext
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName DiscoveredAnyEpochs = "DiscoveredAnyEpochs";

		public static readonly StringName InitializeBannerAndQuote = "InitializeBannerAndQuote";

		public static readonly StringName OpenSummaryScreen = "OpenSummaryScreen";

		public static readonly StringName AddBadge = "AddBadge";

		public static readonly StringName PlayUnlockSfx = "PlayUnlockSfx";

		public static readonly StringName TweenScore = "TweenScore";

		public static readonly StringName GetScoreThreshold = "GetScoreThreshold";

		public static readonly StringName ShowLeaderboard = "ShowLeaderboard";

		public static readonly StringName HideSummary = "HideSummary";

		public static readonly StringName OpenRunHistoryScreen = "OpenRunHistoryScreen";

		public static readonly StringName OnMainMenuButtonPressed = "OnMainMenuButtonPressed";

		public static readonly StringName OpenTimeline = "OpenTimeline";

		public static readonly StringName ReturnToMainMenu = "ReturnToMainMenu";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName MoveCreaturesToDifferentLayerAndDisableUi = "MoveCreaturesToDifferentLayerAndDisableUi";

		public static readonly StringName UpdateBackstopMaterial = "UpdateBackstopMaterial";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _continueButton = "_continueButton";

		public static readonly StringName _viewRunButton = "_viewRunButton";

		public static readonly StringName _mainMenuButton = "_mainMenuButton";

		public static readonly StringName _leaderboardButton = "_leaderboardButton";

		public static readonly StringName _badgeContainer = "_badgeContainer";

		public static readonly StringName _scoreBar = "_scoreBar";

		public static readonly StringName _scoreFg = "_scoreFg";

		public static readonly StringName _scoreProgress = "_scoreProgress";

		public static readonly StringName _unlocksRemaining = "_unlocksRemaining";

		public static readonly StringName _score = "_score";

		public static readonly StringName _scoreThreshold = "_scoreThreshold";

		public static readonly StringName _scoreUnlockedEpochId = "_scoreUnlockedEpochId";

		public static readonly StringName _leaderboard = "_leaderboard";

		public static readonly StringName _creatureContainer = "_creatureContainer";

		public static readonly StringName _summaryContainer = "_summaryContainer";

		public static readonly StringName _fullBlackBackstop = "_fullBlackBackstop";

		public static readonly StringName _summaryBackstop = "_summaryBackstop";

		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _deathQuote = "_deathQuote";

		public static readonly StringName _victoryDamageLabel = "_victoryDamageLabel";

		public static readonly StringName _uiNode = "_uiNode";

		public static readonly StringName _screenshakeContainer = "_screenshakeContainer";

		public static readonly StringName _discoveryLabel = "_discoveryLabel";

		public static readonly StringName _encounterQuote = "_encounterQuote";

		public static readonly StringName _isAnimatingSummary = "_isAnimatingSummary";

		public static readonly StringName _backstopMaterial = "_backstopMaterial";

		public static readonly StringName _quoteTween = "_quoteTween";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly StringName _threshold = new StringName("threshold");

	private RunState _runState;

	private SerializableRun _serializableRun;

	private RunHistory _history;

	private Player? _localPlayer;

	private NGameOverContinueButton _continueButton;

	private NViewRunButton _viewRunButton;

	private NReturnToMainMenuButton _mainMenuButton;

	private NGameOverContinueButton _leaderboardButton;

	private GridContainer _badgeContainer;

	private readonly List<NBadge> _badges = new List<NBadge>();

	private Control _scoreBar;

	private Control _scoreFg;

	private MegaLabel _scoreProgress;

	private MegaLabel _unlocksRemaining;

	private int _score;

	private int _scoreThreshold;

	private string? _scoreUnlockedEpochId;

	private NDailyRunLeaderboard _leaderboard;

	private Control _creatureContainer;

	private NRunSummary _summaryContainer;

	private ColorRect _fullBlackBackstop;

	private ColorRect _summaryBackstop;

	private ColorRect _backstop;

	private NCommonBanner _banner;

	private MegaRichTextLabel _deathQuote;

	private MegaRichTextLabel _victoryDamageLabel;

	private Control _uiNode;

	private Control _screenshakeContainer;

	private MegaLabel _discoveryLabel;

	private string _encounterQuote;

	private bool _isAnimatingSummary;

	private ShaderMaterial _backstopMaterial;

	private Tween? _quoteTween;

	private static string ScenePath => SceneHelper.GetScenePath("screens/game_over_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NetScreenType ScreenType => NetScreenType.GameOver;

	public bool UseSharedBackstop => false;

	public Control DefaultFocusedControl => this;

	public override void _Ready()
	{
		bool win = _runState.CurrentRoom?.IsVictoryRoom ?? false;
		_history = RunManager.Instance.History ?? new RunHistory
		{
			Win = win
		};
		_score = ScoreUtility.CalculateScore(_serializableRun, _history.Win);
		_uiNode = GetNode<Control>("%Ui");
		_continueButton = GetNode<NGameOverContinueButton>("%ContinueButton");
		_continueButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenSummaryScreen));
		_continueButton.Disable();
		_viewRunButton = GetNode<NViewRunButton>("%ViewRunButton");
		_viewRunButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenRunHistoryScreen));
		_mainMenuButton = GetNode<NReturnToMainMenuButton>("%MainMenuButton");
		_mainMenuButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnMainMenuButtonPressed));
		_badgeContainer = GetNode<GridContainer>("%BadgeContainer");
		_scoreBar = GetNode<Control>("%ScoreBar");
		_scoreFg = GetNode<Control>("%ScoreFg");
		_scoreProgress = GetNode<MegaLabel>("%ScoreProgress");
		_unlocksRemaining = GetNode<MegaLabel>("%UnlocksRemaining");
		_screenshakeContainer = GetNode<Control>("%ScreenshakeContainer");
		_leaderboardButton = GetNode<NGameOverContinueButton>("%LeaderboardButton");
		_leaderboardButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ShowLeaderboard));
		_creatureContainer = GetNode<Control>("%CreatureContainer");
		_summaryContainer = GetNode<NRunSummary>("%RunSummaryContainer");
		_backstop = GetNode<ColorRect>("%Backstop");
		_fullBlackBackstop = GetNode<ColorRect>("%FullBlackBackstop");
		_backstopMaterial = (ShaderMaterial)_backstop.Material;
		_summaryBackstop = GetNode<ColorRect>("%SummaryBackstop");
		_leaderboard = GetNode<NDailyRunLeaderboard>("%DailyRunLeaderboard");
		_banner = GetNode<NCommonBanner>("%Banner");
		_victoryDamageLabel = GetNode<MegaRichTextLabel>("%VictoryDamageLabel");
		_discoveryLabel = GetNode<MegaLabel>("%DiscoveryLabel");
		_discoveryLabel.SetTextAutoSize(new LocString("game_over_screen", "DISCOVERY_HEADER").GetFormattedText());
		_deathQuote = GetNode<MegaRichTextLabel>("%DeathQuoteLabel");
		InitializeBannerAndQuote();
		ActiveScreenContext.Instance.Update();
		_leaderboardButton.Disable();
		_viewRunButton.Disable();
		_mainMenuButton.Disable();
		_leaderboard.Visible = false;
	}

	private bool DiscoveredAnyEpochs()
	{
		return _localPlayer.DiscoveredEpochs.Count > 0;
	}

	private void InitializeBannerAndQuote()
	{
		ModelId id = _localPlayer.Character.Id;
		if (_history.Win)
		{
			_banner.label.SetTextAutoSize(new LocString("game_over_screen", "BANNER.falseWin").GetFormattedText());
			_deathQuote.Text = string.Empty;
			long personalArchitectDamage = StatsManager.GetPersonalArchitectDamage();
			long? globalArchitectDamage = StatsManager.GetGlobalArchitectDamage();
			StringBuilder stringBuilder = new StringBuilder();
			if (globalArchitectDamage.HasValue)
			{
				LocString locString = new LocString("game_over_screen", "VICTORY_DAMAGE");
				locString.Add("PlayerDamage", _score);
				locString.Add("PersonalDamage", personalArchitectDamage.ToString("N0"));
				locString.Add("TotalDamage", globalArchitectDamage.Value.ToString("N0"));
				stringBuilder.Append(locString.GetFormattedText());
			}
			else
			{
				LocString locString2 = new LocString("game_over_screen", "VICTORY_DAMAGE_LOCAL");
				locString2.Add("PlayerDamage", _score);
				locString2.Add("PersonalDamage", personalArchitectDamage.ToString("N0"));
				stringBuilder.Append(locString2.GetFormattedText());
			}
			int ascensionLevel = _runState.AscensionLevel;
			if (ascensionLevel < 10 && ascensionLevel > 0 && _runState.AscensionLevel >= _localPlayer.MaxAscensionWhenRunStarted)
			{
				stringBuilder.Append("\n\n");
				LocString locString3 = new LocString("game_over_screen", "VICTORY_UNLOCKED_ASCENSION");
				locString3.Add("AscensionLevel", _runState.AscensionLevel + 1);
				stringBuilder.Append(locString3.GetFormattedText());
			}
			_victoryDamageLabel.Text = stringBuilder.ToString();
		}
		else
		{
			LocTable table = LocManager.Instance.GetTable("game_over_screen");
			IReadOnlyList<LocString> locStringsWithPrefix = table.GetLocStringsWithPrefix("BANNER.lose");
			_banner.label.SetTextAutoSize(Rng.Chaotic.NextItem(locStringsWithPrefix).GetFormattedText());
			IReadOnlyList<LocString> locStringsWithPrefix2 = table.GetLocStringsWithPrefix("QUOTES");
			_deathQuote.Text = Rng.Chaotic.NextItem(locStringsWithPrefix2).GetFormattedText();
		}
		_encounterQuote = NRunHistory.GetDeathQuote(_history, id, NRunHistory.GetGameOverType(_history));
	}

	private async Task AnimateInQuote()
	{
		if (_deathQuote.Modulate.A != 0f)
		{
			_quoteTween?.Kill();
			_quoteTween = CreateTween();
			_quoteTween.TweenProperty(_deathQuote, "modulate:a", 0f, 0.25);
			await ToSignal(_quoteTween, Tween.SignalName.Finished);
			if (!this.IsValid())
			{
				return;
			}
			_deathQuote.Text = _encounterQuote;
			_quoteTween.Kill();
			await Cmd.Wait(1f);
		}
		if (this.IsValid())
		{
			_quoteTween?.Kill();
			_quoteTween = CreateTween().SetParallel();
			if (_history.Win)
			{
				_quoteTween.TweenProperty(_victoryDamageLabel, "visible_ratio", 1f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				_quoteTween.TweenProperty(_victoryDamageLabel, "modulate:a", 1f, 2.0);
				await ToSignal(_quoteTween, Tween.SignalName.Finished);
			}
			else
			{
				_quoteTween.TweenProperty(_deathQuote, "position:y", 156f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
					.From(90f);
				_quoteTween.TweenProperty(_deathQuote, "modulate:a", 1f, 1.5);
			}
		}
	}

	public static NGameOverScreen? Create(RunState runState, SerializableRun serializableRun)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NGameOverScreen nGameOverScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NGameOverScreen>(PackedScene.GenEditState.Disabled);
		nGameOverScreen._runState = runState;
		nGameOverScreen._serializableRun = serializableRun;
		nGameOverScreen._localPlayer = LocalContext.GetMe(runState);
		return nGameOverScreen;
	}

	private void OpenSummaryScreen(NButton _)
	{
		_isAnimatingSummary = true;
		_continueButton.Disable();
		_victoryDamageLabel.Visible = false;
		Tween tween = CreateTween();
		tween.TweenProperty(_summaryBackstop, "modulate", Colors.White, 0.5);
		TaskHelper.RunSafely(AnimateInQuote());
		TaskHelper.RunSafely(AnimateRunSummary());
	}

	private async Task AnimateRunSummary()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_banner, "position:y", _banner.Position.Y - 32f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_summaryContainer.Visible = true;
		await AnimateBadges();
		await AnimateScoreBar();
		await AnimateDiscoveries();
		if (_history.GameMode == GameMode.Daily)
		{
			_leaderboard.Initialize(RunManager.Instance.DailyTime.Value, _runState.Players.Select((Player p) => p.NetId), allowPagination: false);
			_leaderboardButton.Visible = true;
			_leaderboardButton.Enable();
			return;
		}
		if (DiscoveredAnyEpochs())
		{
			_mainMenuButton.SetLabelForUnlock();
		}
		_mainMenuButton.Visible = true;
		_mainMenuButton.Enable();
	}

	private async Task AnimateBadges()
	{
		_badges.Clear();
		AddBadge("BADGE.floorsClimbed", "FloorCount", _runState.TotalFloor, "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_floor.tres");
		List<MapPointRoomHistoryEntry> list = _serializableRun.MapPointHistory.SelectMany((List<MapPointHistoryEntry> actEntries) => actEntries).SelectMany((MapPointHistoryEntry e) => e.Rooms).ToList();
		int num = list.Count((MapPointRoomHistoryEntry r) => r.RoomType == RoomType.Elite);
		if (list.Count > 0 && list.Last().RoomType == RoomType.Elite)
		{
			num--;
		}
		AddBadge("BADGE.elitesKilled", "EliteCount", num);
		int amount = _serializableRun.MapPointHistory.SelectMany((List<MapPointHistoryEntry> actEntries) => actEntries).Sum((MapPointHistoryEntry e) => e.GetEntry(_localPlayer.NetId).GoldGained);
		AddBadge("BADGE.goldGained", "GoldAmount", amount);
		if (_localPlayer.Relics.Count >= 25)
		{
			AddBadge("BADGE.iLikeShiny", "RelicCount", _localPlayer.Relics.Count);
		}
		int gold = _localPlayer.Gold;
		if (gold >= 3000)
		{
			AddBadge("BADGE.goldenGod", "GoldAmount", gold);
		}
		else if (gold >= 2000)
		{
			AddBadge("BADGE.scrooge", "GoldAmount", gold);
		}
		else if (gold >= 1000)
		{
			AddBadge("BADGE.miser", "GoldAmount", gold);
		}
		if (_history.Win)
		{
			int count = _localPlayer.Deck.Cards.Count;
			if (count <= 10)
			{
				AddBadge("BADGE.tinyDeck", "DeckSize", count);
			}
			else if (count <= 20)
			{
				AddBadge("BADGE.smallDeck", "DeckSize", count);
			}
			else if (count >= 60)
			{
				AddBadge("BADGE.hugeDeck", "DeckSize", count);
			}
			else if (count >= 40)
			{
				AddBadge("BADGE.bigDeck", "DeckSize", count);
			}
			int startingHp = _localPlayer.Character.StartingHp;
			int maxHp = _localPlayer.Creature.MaxHp;
			int num2 = maxHp - startingHp;
			if ((float)maxHp / (float)startingHp < 0.50001f)
			{
				AddBadge("BADGE.famished", "HpDiff", num2);
			}
			else if (num2 >= 50)
			{
				AddBadge("BADGE.glutton", "HpDiff", num2);
			}
			else if (num2 >= 30)
			{
				AddBadge("BADGE.stuffed", "HpDiff", num2);
			}
			else if (num2 >= 15)
			{
				AddBadge("BADGE.wellFed", "HpDiff", num2);
			}
		}
		_badgeContainer.Columns = ((_badges.Count < 6) ? 1 : 2);
		await Cmd.Wait(0.5f);
		foreach (NBadge badge in _badges)
		{
			await badge.AnimateIn();
		}
	}

	private void AddBadge(string locEntryKey, string? locAmountKey = null, int amount = 0, string? iconPath = null)
	{
		LocString locString = new LocString("game_over_screen", locEntryKey);
		if (locAmountKey != null)
		{
			locString.Add(locAmountKey, amount);
		}
		Texture2D icon = ((iconPath == null) ? null : PreloadManager.Cache.GetTexture2D(iconPath));
		NBadge nBadge = NBadge.Create(locString.GetFormattedText(), icon);
		_badgeContainer.AddChildSafely(nBadge);
		_badges.Add(nBadge);
	}

	private async Task AnimateScoreBar()
	{
		int unlocksRemaining = SaveManager.Instance.GetUnlocksRemaining();
		LocString locString = new LocString("game_over_screen", "SCORE.unlocksRemaining");
		locString.Add("UnlockCount", unlocksRemaining);
		_unlocksRemaining.SetTextAutoSize(locString.GetFormattedText());
		if (unlocksRemaining > 0)
		{
			int currentScore = SaveManager.Instance.GetCurrentScore();
			_scoreThreshold = GetScoreThreshold(unlocksRemaining);
			_scoreProgress.SetTextAutoSize($"[{currentScore}/{_scoreThreshold}]");
			_scoreFg.Scale = new Vector2((float)currentScore / (float)_scoreThreshold, 1f);
			Tween scoreTween = CreateTween();
			scoreTween.TweenProperty(_scoreBar, "modulate:a", 1f, 0.3);
			await scoreTween.ToSignal(scoreTween, Tween.SignalName.Finished);
			if (currentScore + _score >= _scoreThreshold)
			{
				Log.Info("New Unlock, yay!");
				MegaLabel node = GetNode<MegaLabel>("%UnlockText");
				_scoreUnlockedEpochId = SaveManager.Instance.IncrementUnlock();
				currentScore -= _scoreThreshold;
				int newThreshold = GetScoreThreshold(unlocksRemaining - 1);
				string locEntryKey = ((newThreshold == 0) ? "SCORE.unlockedAllMessage" : "SCORE.unlockedEpochMessage");
				node.SetTextAutoSize(new LocString("game_over_screen", locEntryKey).GetFormattedText());
				scoreTween = CreateTween().SetParallel();
				scoreTween.TweenInterval(1.0);
				scoreTween.Chain();
				scoreTween.TweenMethod(Callable.From<int>(TweenScore), currentScore + _scoreThreshold, _scoreThreshold, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				scoreTween.TweenProperty(_scoreFg, "scale:x", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				scoreTween.Chain();
				scoreTween.TweenCallback(Callable.From(PlayUnlockSfx));
				scoreTween.TweenProperty(node, "modulate:a", 1f, 0.25);
				scoreTween.TweenProperty(node, "position:y", -60f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring);
				await scoreTween.ToSignal(scoreTween, Tween.SignalName.Finished);
				if (_scoreUnlockedEpochId != null && !SaveManager.Instance.IsEpochRevealed(_scoreUnlockedEpochId))
				{
					EpochModel epochModel = EpochModel.Get(_scoreUnlockedEpochId);
					SaveManager.Instance.ObtainEpoch(_scoreUnlockedEpochId);
					NGame.Instance.AddChildSafely(NGainEpochVfx.Create(epochModel));
					_localPlayer.DiscoveredEpochs.Add(epochModel.Id);
					LocalContext.GetMe(_serializableRun).DiscoveredEpochs.Add(epochModel.Id);
				}
				LocString locString2 = new LocString("game_over_screen", "SCORE.unlocksRemaining");
				locString2.Add("UnlockCount", unlocksRemaining - 1);
				_unlocksRemaining.SetTextAutoSize(locString2.GetFormattedText());
				_scoreThreshold = newThreshold;
				currentScore += _score;
				if (newThreshold == 0 || currentScore == 0)
				{
					Log.Info("Player has gotten all unlocks or they've overflowed exactly 0");
					SaveManager.Instance.Progress.CurrentScore = 0;
				}
				else if (currentScore >= newThreshold)
				{
					Log.Info("Score is too awesome. Disallow double unlock.");
					scoreTween.Kill();
					scoreTween = CreateTween().SetParallel();
					scoreTween.TweenInterval(0.5);
					scoreTween.Chain();
					scoreTween.TweenMethod(Callable.From<int>(TweenScore), 0, newThreshold * 99 / 100, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
					scoreTween.TweenProperty(_scoreFg, "scale:x", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
						.From(0f);
					await scoreTween.ToSignal(scoreTween, Tween.SignalName.Finished);
					SaveManager.Instance.Progress.CurrentScore = newThreshold - 1;
				}
				else
				{
					Log.Info("Animate overflow score.");
					scoreTween.Kill();
					scoreTween = CreateTween().SetParallel();
					scoreTween.Chain();
					scoreTween.TweenInterval(0.5);
					scoreTween.Chain();
					scoreTween.TweenMethod(Callable.From<int>(TweenScore), 0, currentScore, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
					scoreTween.TweenProperty(_scoreFg, "scale:x", (float)currentScore / (float)newThreshold, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
						.From(0f);
					await scoreTween.ToSignal(scoreTween, Tween.SignalName.Finished);
					SaveManager.Instance.Progress.CurrentScore = currentScore;
				}
			}
			else
			{
				Log.Info("Not enough score to level up");
				scoreTween = CreateTween().SetParallel();
				scoreTween.TweenInterval(0.5);
				scoreTween.TweenMethod(Callable.From<int>(TweenScore), currentScore, currentScore + _score, 1.0);
				scoreTween.TweenProperty(_scoreFg, "scale:x", (float)(currentScore + _score) / (float)_scoreThreshold, 1.0);
				SaveManager.Instance.Progress.CurrentScore += _score;
			}
			SaveManager.Instance.SaveProgressFile();
		}
		else
		{
			Log.Info("This player has all unlocks. No action");
		}
	}

	private void PlayUnlockSfx()
	{
		Log.Info("TODO: Play the ding unlock sfx here pls");
	}

	private void TweenScore(int value)
	{
		_scoreProgress.SetTextAutoSize($"[{value}/{_scoreThreshold}]");
	}

	private int GetScoreThreshold(int unlocksRemaining)
	{
		return (18 - unlocksRemaining) switch
		{
			0 => 200, 
			1 => 500, 
			2 => 750, 
			3 => 1000, 
			4 => 1250, 
			5 => 1500, 
			6 => 1600, 
			7 => 1700, 
			8 => 1800, 
			9 => 1900, 
			10 => 2000, 
			11 => 2100, 
			12 => 2200, 
			13 => 2300, 
			14 => 2400, 
			15 => 2500, 
			16 => 2500, 
			17 => 2500, 
			_ => 0, 
		};
	}

	private void ShowLeaderboard(NButton _)
	{
		Tween tween = CreateTween().SetParallel();
		NDailyRunLeaderboard leaderboard = _leaderboard;
		Color modulate = _leaderboard.Modulate;
		modulate.A = 0f;
		leaderboard.Modulate = modulate;
		tween.TweenProperty(_leaderboard, "modulate:a", 1f, 0.5);
		tween.TweenProperty(_summaryContainer, "modulate:a", 0f, 0.5);
		tween.TweenProperty(_deathQuote, "modulate:a", 0f, 0.5);
		tween.Chain().TweenCallback(Callable.From(HideSummary));
		_leaderboard.Visible = true;
		_leaderboardButton.Disable();
		if (DiscoveredAnyEpochs())
		{
			_mainMenuButton.SetLabelForUnlock();
		}
		_mainMenuButton.Visible = true;
		_mainMenuButton.Enable();
	}

	private void HideSummary()
	{
		_summaryContainer.Visible = false;
		_deathQuote.Visible = false;
	}

	private async Task AnimateDiscoveries()
	{
		await _summaryContainer.AnimateInDiscoveries(_runState);
		_isAnimatingSummary = false;
	}

	private void OpenRunHistoryScreen(NButton _)
	{
		Control child = ResourceLoader.Load<PackedScene>("res://scenes/screens/run_history_screen/run_history_screen_via_game_over_screen.tscn", null, ResourceLoader.CacheMode.Reuse).Instantiate<Control>(PackedScene.GenEditState.Disabled);
		this.AddChildSafely(child);
	}

	private void OnMainMenuButtonPressed(NButton _)
	{
		if (RunManager.Instance.NetService.Type == NetGameType.Host)
		{
			RunManager.Instance.NetService.Disconnect(NetError.QuitGameOver);
		}
		_mainMenuButton.Disable();
		if (DiscoveredAnyEpochs())
		{
			OpenTimeline();
		}
		else
		{
			ReturnToMainMenu();
		}
	}

	private void OpenTimeline()
	{
		TaskHelper.RunSafely(TransitionOutToTimeline());
	}

	private void ReturnToMainMenu()
	{
		TaskHelper.RunSafely(TransitionOutToMainMenu());
	}

	private async Task TransitionOutToTimeline()
	{
		await NGame.Instance.GoToTimelineAfterRun();
	}

	private async Task TransitionOutToMainMenu()
	{
		await NGame.Instance.ReturnToMainMenuAfterRun();
	}

	public void AfterOverlayOpened()
	{
		MoveCreaturesToDifferentLayerAndDisableUi();
		TaskHelper.RunSafely(AnimateIn());
	}

	private void MoveCreaturesToDifferentLayerAndDisableUi()
	{
		List<NCreatureVisuals> list = new List<NCreatureVisuals>();
		List<NCreature> list2;
		if (NCombatRoom.Instance != null)
		{
			if (NCombatRoom.Instance.Mode == CombatRoomMode.ActiveCombat)
			{
				NCombatRoom.Instance.Ui.AnimOut((CombatRoom)_runState.CurrentRoom);
			}
			list2 = NCombatRoom.Instance.CreatureNodes.ToList();
			list = list2.Select((NCreature c) => c.Visuals).ToList();
		}
		else if (NMerchantRoom.Instance != null)
		{
			list2 = new List<NCreature>();
			foreach (NMerchantCharacter playerVisual in NMerchantRoom.Instance.PlayerVisuals)
			{
				playerVisual.PlayAnimation("die");
				playerVisual.Reparent(_creatureContainer);
			}
		}
		else if (NRestSiteRoom.Instance != null)
		{
			list2 = new List<NCreature>();
			list = new List<NCreatureVisuals>();
			foreach (Player player in _runState.Players)
			{
				NCreatureVisuals nCreatureVisuals = player.Creature.CreateVisuals();
				list.Add(nCreatureVisuals);
				_creatureContainer.AddChildSafely(nCreatureVisuals);
				nCreatureVisuals.SpineBody.GetAnimationState().SetAnimation("die", loop: false);
				NRestSiteCharacter characterForPlayer = NRestSiteRoom.Instance.GetCharacterForPlayer(player);
				nCreatureVisuals.GlobalPosition = characterForPlayer.GlobalPosition;
				nCreatureVisuals.Scale = characterForPlayer.Scale;
				characterForPlayer.Visible = false;
				Vector2 vector = new Vector2(100f, 100f);
				nCreatureVisuals.Position += vector * new Vector2(Math.Sign(nCreatureVisuals.Scale.X), Math.Sign(nCreatureVisuals.Scale.Y));
			}
		}
		else
		{
			list2 = new List<NCreature>();
			list = new List<NCreatureVisuals>();
			foreach (Player player2 in _runState.Players)
			{
				NCreatureVisuals nCreatureVisuals2 = player2.Creature.CreateVisuals();
				list.Add(nCreatureVisuals2);
				_creatureContainer.AddChildSafely(nCreatureVisuals2);
				nCreatureVisuals2.SpineBody.GetAnimationState().SetAnimation("die", loop: false);
			}
			float num = Math.Min(250f, (base.Size.X - 200f) / (float)(list.Count - 1));
			float num2 = (float)(list.Count - 1) * (0f - num) * 0.5f;
			foreach (NCreatureVisuals item in list)
			{
				item.Position = _creatureContainer.Size * 0.5f + new Vector2(num2, 200f);
				num2 += num;
			}
		}
		list2.Sort((NCreature c1, NCreature c2) => c1.GetIndex().CompareTo(c2.GetIndex()));
		foreach (NCreature item2 in list2)
		{
			item2.AnimHideIntent();
			item2.AnimDisableUi();
		}
		foreach (NCreatureVisuals item3 in list)
		{
			item3.Reparent(_creatureContainer);
		}
	}

	private async Task AnimateIn()
	{
		Tween backstopTween = CreateTween();
		_uiNode.Modulate = StsColors.transparentWhite;
		if (NEventRoom.Instance != null)
		{
			ColorRect fullBlackBackstop = _fullBlackBackstop;
			Color modulate = _fullBlackBackstop.Modulate;
			modulate.A = 0f;
			fullBlackBackstop.Modulate = modulate;
			_fullBlackBackstop.Visible = true;
			backstopTween.TweenProperty(_fullBlackBackstop, "modulate:a", 1f, 0.2);
			foreach (NCreatureVisuals item in _creatureContainer.GetChildren().OfType<NCreatureVisuals>())
			{
				modulate = item.Modulate;
				modulate.A = 0f;
				item.Modulate = modulate;
				backstopTween.Parallel().TweenProperty(item, "modulate:a", 1f, 0.2);
			}
		}
		Variant shaderParameter = _backstopMaterial.GetShaderParameter(_threshold);
		backstopTween.TweenMethod(Callable.From<float>(UpdateBackstopMaterial), shaderParameter, 1f, 1.5).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(backstopTween, Tween.SignalName.Finished);
		_banner.AnimateIn();
		backstopTween.Kill();
		Tween tween = CreateTween();
		tween.TweenProperty(_uiNode, "modulate:a", 1f, 0.25);
		await ToSignal(tween, Tween.SignalName.Finished);
		TaskHelper.RunSafely(AnimateInQuote());
		_continueButton.Enable();
	}

	private void UpdateBackstopMaterial(float value)
	{
		_backstopMaterial.SetShaderParameter(_threshold, value);
	}

	public void AfterOverlayClosed()
	{
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		NGame.Instance.SetScreenShakeTarget(_screenshakeContainer);
		base.Visible = true;
	}

	public void AfterOverlayHidden()
	{
		base.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(20);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DiscoveredAnyEpochs, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeBannerAndQuote, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenSummaryScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddBadge, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "locEntryKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "locAmountKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "iconPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayUnlockSfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TweenScore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetScoreThreshold, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "unlocksRemaining", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowLeaderboard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideSummary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenRunHistoryScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMainMenuButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenTimeline, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReturnToMainMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MoveCreaturesToDifferentLayerAndDisableUi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateBackstopMaterial, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DiscoveredAnyEpochs && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(DiscoveredAnyEpochs());
			return true;
		}
		if (method == MethodName.InitializeBannerAndQuote && args.Count == 0)
		{
			InitializeBannerAndQuote();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenSummaryScreen && args.Count == 1)
		{
			OpenSummaryScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddBadge && args.Count == 4)
		{
			AddBadge(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<string>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayUnlockSfx && args.Count == 0)
		{
			PlayUnlockSfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenScore && args.Count == 1)
		{
			TweenScore(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetScoreThreshold && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<int>(GetScoreThreshold(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.ShowLeaderboard && args.Count == 1)
		{
			ShowLeaderboard(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideSummary && args.Count == 0)
		{
			HideSummary();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenRunHistoryScreen && args.Count == 1)
		{
			OpenRunHistoryScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMainMenuButtonPressed && args.Count == 1)
		{
			OnMainMenuButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenTimeline && args.Count == 0)
		{
			OpenTimeline();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReturnToMainMenu && args.Count == 0)
		{
			ReturnToMainMenu();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MoveCreaturesToDifferentLayerAndDisableUi && args.Count == 0)
		{
			MoveCreaturesToDifferentLayerAndDisableUi();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateBackstopMaterial && args.Count == 1)
		{
			UpdateBackstopMaterial(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName.DiscoveredAnyEpochs)
		{
			return true;
		}
		if (method == MethodName.InitializeBannerAndQuote)
		{
			return true;
		}
		if (method == MethodName.OpenSummaryScreen)
		{
			return true;
		}
		if (method == MethodName.AddBadge)
		{
			return true;
		}
		if (method == MethodName.PlayUnlockSfx)
		{
			return true;
		}
		if (method == MethodName.TweenScore)
		{
			return true;
		}
		if (method == MethodName.GetScoreThreshold)
		{
			return true;
		}
		if (method == MethodName.ShowLeaderboard)
		{
			return true;
		}
		if (method == MethodName.HideSummary)
		{
			return true;
		}
		if (method == MethodName.OpenRunHistoryScreen)
		{
			return true;
		}
		if (method == MethodName.OnMainMenuButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OpenTimeline)
		{
			return true;
		}
		if (method == MethodName.ReturnToMainMenu)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.MoveCreaturesToDifferentLayerAndDisableUi)
		{
			return true;
		}
		if (method == MethodName.UpdateBackstopMaterial)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._continueButton)
		{
			_continueButton = VariantUtils.ConvertTo<NGameOverContinueButton>(in value);
			return true;
		}
		if (name == PropertyName._viewRunButton)
		{
			_viewRunButton = VariantUtils.ConvertTo<NViewRunButton>(in value);
			return true;
		}
		if (name == PropertyName._mainMenuButton)
		{
			_mainMenuButton = VariantUtils.ConvertTo<NReturnToMainMenuButton>(in value);
			return true;
		}
		if (name == PropertyName._leaderboardButton)
		{
			_leaderboardButton = VariantUtils.ConvertTo<NGameOverContinueButton>(in value);
			return true;
		}
		if (name == PropertyName._badgeContainer)
		{
			_badgeContainer = VariantUtils.ConvertTo<GridContainer>(in value);
			return true;
		}
		if (name == PropertyName._scoreBar)
		{
			_scoreBar = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scoreFg)
		{
			_scoreFg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scoreProgress)
		{
			_scoreProgress = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._unlocksRemaining)
		{
			_unlocksRemaining = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._score)
		{
			_score = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._scoreThreshold)
		{
			_scoreThreshold = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._scoreUnlockedEpochId)
		{
			_scoreUnlockedEpochId = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			_leaderboard = VariantUtils.ConvertTo<NDailyRunLeaderboard>(in value);
			return true;
		}
		if (name == PropertyName._creatureContainer)
		{
			_creatureContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._summaryContainer)
		{
			_summaryContainer = VariantUtils.ConvertTo<NRunSummary>(in value);
			return true;
		}
		if (name == PropertyName._fullBlackBackstop)
		{
			_fullBlackBackstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._summaryBackstop)
		{
			_summaryBackstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._deathQuote)
		{
			_deathQuote = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._victoryDamageLabel)
		{
			_victoryDamageLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._uiNode)
		{
			_uiNode = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._screenshakeContainer)
		{
			_screenshakeContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._discoveryLabel)
		{
			_discoveryLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._encounterQuote)
		{
			_encounterQuote = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._isAnimatingSummary)
		{
			_isAnimatingSummary = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._backstopMaterial)
		{
			_backstopMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._quoteTween)
		{
			_quoteTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		if (name == PropertyName._continueButton)
		{
			value = VariantUtils.CreateFrom(in _continueButton);
			return true;
		}
		if (name == PropertyName._viewRunButton)
		{
			value = VariantUtils.CreateFrom(in _viewRunButton);
			return true;
		}
		if (name == PropertyName._mainMenuButton)
		{
			value = VariantUtils.CreateFrom(in _mainMenuButton);
			return true;
		}
		if (name == PropertyName._leaderboardButton)
		{
			value = VariantUtils.CreateFrom(in _leaderboardButton);
			return true;
		}
		if (name == PropertyName._badgeContainer)
		{
			value = VariantUtils.CreateFrom(in _badgeContainer);
			return true;
		}
		if (name == PropertyName._scoreBar)
		{
			value = VariantUtils.CreateFrom(in _scoreBar);
			return true;
		}
		if (name == PropertyName._scoreFg)
		{
			value = VariantUtils.CreateFrom(in _scoreFg);
			return true;
		}
		if (name == PropertyName._scoreProgress)
		{
			value = VariantUtils.CreateFrom(in _scoreProgress);
			return true;
		}
		if (name == PropertyName._unlocksRemaining)
		{
			value = VariantUtils.CreateFrom(in _unlocksRemaining);
			return true;
		}
		if (name == PropertyName._score)
		{
			value = VariantUtils.CreateFrom(in _score);
			return true;
		}
		if (name == PropertyName._scoreThreshold)
		{
			value = VariantUtils.CreateFrom(in _scoreThreshold);
			return true;
		}
		if (name == PropertyName._scoreUnlockedEpochId)
		{
			value = VariantUtils.CreateFrom(in _scoreUnlockedEpochId);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			value = VariantUtils.CreateFrom(in _leaderboard);
			return true;
		}
		if (name == PropertyName._creatureContainer)
		{
			value = VariantUtils.CreateFrom(in _creatureContainer);
			return true;
		}
		if (name == PropertyName._summaryContainer)
		{
			value = VariantUtils.CreateFrom(in _summaryContainer);
			return true;
		}
		if (name == PropertyName._fullBlackBackstop)
		{
			value = VariantUtils.CreateFrom(in _fullBlackBackstop);
			return true;
		}
		if (name == PropertyName._summaryBackstop)
		{
			value = VariantUtils.CreateFrom(in _summaryBackstop);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._deathQuote)
		{
			value = VariantUtils.CreateFrom(in _deathQuote);
			return true;
		}
		if (name == PropertyName._victoryDamageLabel)
		{
			value = VariantUtils.CreateFrom(in _victoryDamageLabel);
			return true;
		}
		if (name == PropertyName._uiNode)
		{
			value = VariantUtils.CreateFrom(in _uiNode);
			return true;
		}
		if (name == PropertyName._screenshakeContainer)
		{
			value = VariantUtils.CreateFrom(in _screenshakeContainer);
			return true;
		}
		if (name == PropertyName._discoveryLabel)
		{
			value = VariantUtils.CreateFrom(in _discoveryLabel);
			return true;
		}
		if (name == PropertyName._encounterQuote)
		{
			value = VariantUtils.CreateFrom(in _encounterQuote);
			return true;
		}
		if (name == PropertyName._isAnimatingSummary)
		{
			value = VariantUtils.CreateFrom(in _isAnimatingSummary);
			return true;
		}
		if (name == PropertyName._backstopMaterial)
		{
			value = VariantUtils.CreateFrom(in _backstopMaterial);
			return true;
		}
		if (name == PropertyName._quoteTween)
		{
			value = VariantUtils.CreateFrom(in _quoteTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._continueButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewRunButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainMenuButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboardButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._badgeContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreFg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreProgress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlocksRemaining, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._score, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._scoreThreshold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._scoreUnlockedEpochId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._summaryContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fullBlackBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._summaryBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathQuote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._victoryDamageLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uiNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenshakeContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveryLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._encounterQuote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAnimatingSummary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstopMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._quoteTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._continueButton, Variant.From(in _continueButton));
		info.AddProperty(PropertyName._viewRunButton, Variant.From(in _viewRunButton));
		info.AddProperty(PropertyName._mainMenuButton, Variant.From(in _mainMenuButton));
		info.AddProperty(PropertyName._leaderboardButton, Variant.From(in _leaderboardButton));
		info.AddProperty(PropertyName._badgeContainer, Variant.From(in _badgeContainer));
		info.AddProperty(PropertyName._scoreBar, Variant.From(in _scoreBar));
		info.AddProperty(PropertyName._scoreFg, Variant.From(in _scoreFg));
		info.AddProperty(PropertyName._scoreProgress, Variant.From(in _scoreProgress));
		info.AddProperty(PropertyName._unlocksRemaining, Variant.From(in _unlocksRemaining));
		info.AddProperty(PropertyName._score, Variant.From(in _score));
		info.AddProperty(PropertyName._scoreThreshold, Variant.From(in _scoreThreshold));
		info.AddProperty(PropertyName._scoreUnlockedEpochId, Variant.From(in _scoreUnlockedEpochId));
		info.AddProperty(PropertyName._leaderboard, Variant.From(in _leaderboard));
		info.AddProperty(PropertyName._creatureContainer, Variant.From(in _creatureContainer));
		info.AddProperty(PropertyName._summaryContainer, Variant.From(in _summaryContainer));
		info.AddProperty(PropertyName._fullBlackBackstop, Variant.From(in _fullBlackBackstop));
		info.AddProperty(PropertyName._summaryBackstop, Variant.From(in _summaryBackstop));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._deathQuote, Variant.From(in _deathQuote));
		info.AddProperty(PropertyName._victoryDamageLabel, Variant.From(in _victoryDamageLabel));
		info.AddProperty(PropertyName._uiNode, Variant.From(in _uiNode));
		info.AddProperty(PropertyName._screenshakeContainer, Variant.From(in _screenshakeContainer));
		info.AddProperty(PropertyName._discoveryLabel, Variant.From(in _discoveryLabel));
		info.AddProperty(PropertyName._encounterQuote, Variant.From(in _encounterQuote));
		info.AddProperty(PropertyName._isAnimatingSummary, Variant.From(in _isAnimatingSummary));
		info.AddProperty(PropertyName._backstopMaterial, Variant.From(in _backstopMaterial));
		info.AddProperty(PropertyName._quoteTween, Variant.From(in _quoteTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._continueButton, out var value))
		{
			_continueButton = value.As<NGameOverContinueButton>();
		}
		if (info.TryGetProperty(PropertyName._viewRunButton, out var value2))
		{
			_viewRunButton = value2.As<NViewRunButton>();
		}
		if (info.TryGetProperty(PropertyName._mainMenuButton, out var value3))
		{
			_mainMenuButton = value3.As<NReturnToMainMenuButton>();
		}
		if (info.TryGetProperty(PropertyName._leaderboardButton, out var value4))
		{
			_leaderboardButton = value4.As<NGameOverContinueButton>();
		}
		if (info.TryGetProperty(PropertyName._badgeContainer, out var value5))
		{
			_badgeContainer = value5.As<GridContainer>();
		}
		if (info.TryGetProperty(PropertyName._scoreBar, out var value6))
		{
			_scoreBar = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scoreFg, out var value7))
		{
			_scoreFg = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scoreProgress, out var value8))
		{
			_scoreProgress = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._unlocksRemaining, out var value9))
		{
			_unlocksRemaining = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._score, out var value10))
		{
			_score = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scoreThreshold, out var value11))
		{
			_scoreThreshold = value11.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scoreUnlockedEpochId, out var value12))
		{
			_scoreUnlockedEpochId = value12.As<string>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value13))
		{
			_leaderboard = value13.As<NDailyRunLeaderboard>();
		}
		if (info.TryGetProperty(PropertyName._creatureContainer, out var value14))
		{
			_creatureContainer = value14.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._summaryContainer, out var value15))
		{
			_summaryContainer = value15.As<NRunSummary>();
		}
		if (info.TryGetProperty(PropertyName._fullBlackBackstop, out var value16))
		{
			_fullBlackBackstop = value16.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._summaryBackstop, out var value17))
		{
			_summaryBackstop = value17.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value18))
		{
			_backstop = value18.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value19))
		{
			_banner = value19.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._deathQuote, out var value20))
		{
			_deathQuote = value20.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._victoryDamageLabel, out var value21))
		{
			_victoryDamageLabel = value21.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._uiNode, out var value22))
		{
			_uiNode = value22.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._screenshakeContainer, out var value23))
		{
			_screenshakeContainer = value23.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveryLabel, out var value24))
		{
			_discoveryLabel = value24.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._encounterQuote, out var value25))
		{
			_encounterQuote = value25.As<string>();
		}
		if (info.TryGetProperty(PropertyName._isAnimatingSummary, out var value26))
		{
			_isAnimatingSummary = value26.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._backstopMaterial, out var value27))
		{
			_backstopMaterial = value27.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._quoteTween, out var value28))
		{
			_quoteTween = value28.As<Tween>();
		}
	}
}
