using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using MegaCrit.Sts2.Core.Models.Badges;
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
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NClickableControl.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'DiscoveredAnyEpochs' method.
		/// </summary>
		public static readonly StringName DiscoveredAnyEpochs = "DiscoveredAnyEpochs";

		/// <summary>
		/// Cached name for the 'InitializeBannerAndQuote' method.
		/// </summary>
		public static readonly StringName InitializeBannerAndQuote = "InitializeBannerAndQuote";

		/// <summary>
		/// Cached name for the 'OpenSummaryScreen' method.
		/// </summary>
		public static readonly StringName OpenSummaryScreen = "OpenSummaryScreen";

		/// <summary>
		/// Cached name for the 'AddScoreLine' method.
		/// </summary>
		public static readonly StringName AddScoreLine = "AddScoreLine";

		/// <summary>
		/// Cached name for the 'PlayUnlockSfx' method.
		/// </summary>
		public static readonly StringName PlayUnlockSfx = "PlayUnlockSfx";

		/// <summary>
		/// Cached name for the 'TweenScore' method.
		/// </summary>
		public static readonly StringName TweenScore = "TweenScore";

		/// <summary>
		/// Cached name for the 'GetScoreThreshold' method.
		/// </summary>
		public static readonly StringName GetScoreThreshold = "GetScoreThreshold";

		/// <summary>
		/// Cached name for the 'ShowLeaderboard' method.
		/// </summary>
		public static readonly StringName ShowLeaderboard = "ShowLeaderboard";

		/// <summary>
		/// Cached name for the 'HideSummary' method.
		/// </summary>
		public static readonly StringName HideSummary = "HideSummary";

		/// <summary>
		/// Cached name for the 'OpenRunHistoryScreen' method.
		/// </summary>
		public static readonly StringName OpenRunHistoryScreen = "OpenRunHistoryScreen";

		/// <summary>
		/// Cached name for the 'OnMainMenuButtonPressed' method.
		/// </summary>
		public static readonly StringName OnMainMenuButtonPressed = "OnMainMenuButtonPressed";

		/// <summary>
		/// Cached name for the 'OpenTimeline' method.
		/// </summary>
		public static readonly StringName OpenTimeline = "OpenTimeline";

		/// <summary>
		/// Cached name for the 'ReturnToMainMenu' method.
		/// </summary>
		public static readonly StringName ReturnToMainMenu = "ReturnToMainMenu";

		/// <summary>
		/// Cached name for the 'AfterOverlayOpened' method.
		/// </summary>
		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		/// <summary>
		/// Cached name for the 'MoveCreaturesToDifferentLayerAndDisableUi' method.
		/// </summary>
		public static readonly StringName MoveCreaturesToDifferentLayerAndDisableUi = "MoveCreaturesToDifferentLayerAndDisableUi";

		/// <summary>
		/// Cached name for the 'UpdateBackstopMaterial' method.
		/// </summary>
		public static readonly StringName UpdateBackstopMaterial = "UpdateBackstopMaterial";

		/// <summary>
		/// Cached name for the 'AfterOverlayClosed' method.
		/// </summary>
		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		/// <summary>
		/// Cached name for the 'AfterOverlayShown' method.
		/// </summary>
		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		/// <summary>
		/// Cached name for the 'AfterOverlayHidden' method.
		/// </summary>
		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";

		/// <summary>
		/// Cached name for the 'GetAscensionMulti' method.
		/// </summary>
		public static readonly StringName GetAscensionMulti = "GetAscensionMulti";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NClickableControl.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ScreenType' property.
		/// </summary>
		public static readonly StringName ScreenType = "ScreenType";

		/// <summary>
		/// Cached name for the 'UseSharedBackstop' property.
		/// </summary>
		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		/// <summary>
		/// Cached name for the 'FocusedControlFromTopBar' property.
		/// </summary>
		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_continueButton' field.
		/// </summary>
		public static readonly StringName _continueButton = "_continueButton";

		/// <summary>
		/// Cached name for the '_viewRunButton' field.
		/// </summary>
		public static readonly StringName _viewRunButton = "_viewRunButton";

		/// <summary>
		/// Cached name for the '_mainMenuButton' field.
		/// </summary>
		public static readonly StringName _mainMenuButton = "_mainMenuButton";

		/// <summary>
		/// Cached name for the '_leaderboardButton' field.
		/// </summary>
		public static readonly StringName _leaderboardButton = "_leaderboardButton";

		/// <summary>
		/// Cached name for the '_badgeContainer' field.
		/// </summary>
		public static readonly StringName _badgeContainer = "_badgeContainer";

		/// <summary>
		/// Cached name for the '_scoreLineContainer' field.
		/// </summary>
		public static readonly StringName _scoreLineContainer = "_scoreLineContainer";

		/// <summary>
		/// Cached name for the '_scoreBar' field.
		/// </summary>
		public static readonly StringName _scoreBar = "_scoreBar";

		/// <summary>
		/// Cached name for the '_scoreFg' field.
		/// </summary>
		public static readonly StringName _scoreFg = "_scoreFg";

		/// <summary>
		/// Cached name for the '_scoreProgress' field.
		/// </summary>
		public static readonly StringName _scoreProgress = "_scoreProgress";

		/// <summary>
		/// Cached name for the '_unlocksRemaining' field.
		/// </summary>
		public static readonly StringName _unlocksRemaining = "_unlocksRemaining";

		/// <summary>
		/// Cached name for the '_score' field.
		/// </summary>
		public static readonly StringName _score = "_score";

		/// <summary>
		/// Cached name for the '_scoreThreshold' field.
		/// </summary>
		public static readonly StringName _scoreThreshold = "_scoreThreshold";

		/// <summary>
		/// Cached name for the '_scoreUnlockedEpochId' field.
		/// </summary>
		public static readonly StringName _scoreUnlockedEpochId = "_scoreUnlockedEpochId";

		/// <summary>
		/// Cached name for the '_leaderboard' field.
		/// </summary>
		public static readonly StringName _leaderboard = "_leaderboard";

		/// <summary>
		/// Cached name for the '_creatureContainer' field.
		/// </summary>
		public static readonly StringName _creatureContainer = "_creatureContainer";

		/// <summary>
		/// Cached name for the '_summaryContainer' field.
		/// </summary>
		public static readonly StringName _summaryContainer = "_summaryContainer";

		/// <summary>
		/// Cached name for the '_fullBlackBackstop' field.
		/// </summary>
		public static readonly StringName _fullBlackBackstop = "_fullBlackBackstop";

		/// <summary>
		/// Cached name for the '_summaryBackstop' field.
		/// </summary>
		public static readonly StringName _summaryBackstop = "_summaryBackstop";

		/// <summary>
		/// Cached name for the '_backstop' field.
		/// </summary>
		public static readonly StringName _backstop = "_backstop";

		/// <summary>
		/// Cached name for the '_banner' field.
		/// </summary>
		public static readonly StringName _banner = "_banner";

		/// <summary>
		/// Cached name for the '_deathQuote' field.
		/// </summary>
		public static readonly StringName _deathQuote = "_deathQuote";

		/// <summary>
		/// Cached name for the '_victoryDamageLabel' field.
		/// </summary>
		public static readonly StringName _victoryDamageLabel = "_victoryDamageLabel";

		/// <summary>
		/// Cached name for the '_uiNode' field.
		/// </summary>
		public static readonly StringName _uiNode = "_uiNode";

		/// <summary>
		/// Cached name for the '_screenshakeContainer' field.
		/// </summary>
		public static readonly StringName _screenshakeContainer = "_screenshakeContainer";

		/// <summary>
		/// Cached name for the '_discoveryLabel' field.
		/// </summary>
		public static readonly StringName _discoveryLabel = "_discoveryLabel";

		/// <summary>
		/// Cached name for the '_encounterQuote' field.
		/// </summary>
		public static readonly StringName _encounterQuote = "_encounterQuote";

		/// <summary>
		/// Cached name for the '_isAnimatingSummary' field.
		/// </summary>
		public static readonly StringName _isAnimatingSummary = "_isAnimatingSummary";

		/// <summary>
		/// Cached name for the '_backstopMaterial' field.
		/// </summary>
		public static readonly StringName _backstopMaterial = "_backstopMaterial";

		/// <summary>
		/// Cached name for the '_quoteTween' field.
		/// </summary>
		public static readonly StringName _quoteTween = "_quoteTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly StringName _threshold = new StringName("threshold");

	private RunState _runState;

	private SerializableRun _serializableRun;

	private RunHistory _history;

	private Player _localPlayer;

	private NGameOverContinueButton _continueButton;

	private NViewRunButton _viewRunButton;

	private NReturnToMainMenuButton _mainMenuButton;

	private NGameOverContinueButton _leaderboardButton;

	private Control _badgeContainer;

	private GridContainer _scoreLineContainer;

	private readonly List<NScoreLine> _scoreLines = new List<NScoreLine>();

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

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	private static string ScenePath => SceneHelper.GetScenePath("screens/game_over_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NetScreenType ScreenType => NetScreenType.GameOver;

	public bool UseSharedBackstop => false;

	public Control? FocusedControlFromTopBar
	{
		get
		{
			if (_badgeContainer.GetChildCount() > 0)
			{
				return _badgeContainer.GetChild<NBadge>(0);
			}
			if (_summaryContainer.DefaultFocusedControl != null)
			{
				return _summaryContainer.DefaultFocusedControl;
			}
			return this;
		}
	}

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
		_scoreLineContainer = GetNode<GridContainer>("%ScoreLineContainer");
		_badgeContainer = GetNode<Control>("%BadgeContainer");
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
		_leaderboardButton.Disable();
		_viewRunButton.Disable();
		_mainMenuButton.Disable();
		_leaderboard.Visible = false;
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
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
			_banner.label.SetTextAutoSize(new LocString("game_over_screen", "BANNER.falseWin").GetRawText());
			_deathQuote.Text = string.Empty;
			long personalArchitectDamage = StatsManager.GetPersonalArchitectDamage();
			long? globalArchitectDamage = StatsManager.GetGlobalArchitectDamage();
			StringBuilder stringBuilder = new StringBuilder();
			LocString locString;
			if (globalArchitectDamage.HasValue)
			{
				locString = new LocString("game_over_screen", "VICTORY_DAMAGE");
				locString.Add("TotalDamage", globalArchitectDamage.Value);
			}
			else
			{
				locString = new LocString("game_over_screen", "VICTORY_DAMAGE_LOCAL");
			}
			locString.Add("PlayerDamage", _score);
			locString.Add("PersonalDamage", personalArchitectDamage);
			stringBuilder.Append(locString.GetFormattedText());
			int ascensionLevel = _runState.AscensionLevel;
			if (ascensionLevel < 10 && ascensionLevel > 0 && _runState.AscensionLevel >= _localPlayer.MaxAscensionWhenRunStarted && _runState.GameMode == GameMode.Standard)
			{
				stringBuilder.Append("\n\n");
				LocString locString2 = new LocString("game_over_screen", "VICTORY_UNLOCKED_ASCENSION");
				locString2.Add("AscensionLevel", _runState.AscensionLevel + 1);
				stringBuilder.Append(locString2.GetFormattedText());
			}
			_victoryDamageLabel.Text = stringBuilder.ToString();
		}
		else
		{
			LocTable table = LocManager.Instance.GetTable("game_over_screen");
			IReadOnlyList<LocString> locStringsWithPrefix = table.GetLocStringsWithPrefix("BANNER.lose");
			_banner.label.SetTextAutoSize(Rng.Chaotic.NextItem(locStringsWithPrefix).GetRawText());
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
			if (!(await _quoteTween.AwaitFinished(this)))
			{
				return;
			}
			_deathQuote.Text = _encounterQuote;
			_quoteTween.Kill();
			await Cmd.Wait(1f, _cts.Token);
		}
		if (this.IsValid())
		{
			_quoteTween?.Kill();
			_quoteTween = CreateTween().SetParallel();
			if (_history.Win)
			{
				_quoteTween.TweenProperty(_victoryDamageLabel, "visible_ratio", 1f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				_quoteTween.TweenProperty(_victoryDamageLabel, "modulate:a", 1f, 2.0);
				await _quoteTween.AwaitFinished(this);
			}
			else
			{
				_quoteTween.TweenProperty(_deathQuote, "position:y", 156f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
					.From(90f);
				_quoteTween.TweenProperty(_deathQuote, "modulate:a", 1f, 1.5);
			}
		}
	}

	/// <summary>
	/// Create an instance of this screen.
	/// Null if we're in test mode.
	/// </summary>
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

	/// <summary>
	/// Called when the player continues to the score/badge breakdown.
	/// </summary>
	/// <param name="_"></param>
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

	/// <summary>
	/// Animates the second page of our Game Over screen
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	private async Task AnimateRunSummary()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_banner, "position:y", _banner.Position.Y - 32f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_summaryContainer.Visible = true;
		await AnimateScoreLines();
		await AnimateBadges();
		await AnimateScoreBar();
		await AnimateDiscoveries();
		for (int i = 0; i < _badgeContainer.GetChildCount(); i++)
		{
			_badgeContainer.GetChild<NBadge>(i).FocusNeighborLeft = ((i > 0) ? _badgeContainer.GetChild<NBadge>(i - 1).GetPath() : _badgeContainer.GetChild<NBadge>(_badgeContainer.GetChildCount() - 1).GetPath());
			_badgeContainer.GetChild<NBadge>(i).FocusNeighborRight = ((i < _badgeContainer.GetChildCount() - 1) ? _badgeContainer.GetChild<NBadge>(i + 1).GetPath() : _badgeContainer.GetChild<NBadge>(0).GetPath());
			_badgeContainer.GetChild<NBadge>(i).FocusNeighborTop = _badgeContainer.GetChild<NBadge>(i).GetPath();
			_badgeContainer.GetChild<NBadge>(i).FocusNeighborBottom = ((_summaryContainer.DefaultFocusedControl != null) ? _summaryContainer.DefaultFocusedControl.GetPath() : _badgeContainer.GetChild<NBadge>(i).GetPath());
		}
		_summaryContainer.SetControllerNav((_badgeContainer.GetChildCount() > 0) ? _badgeContainer.GetChild<Control>(0) : null);
		if (_badgeContainer.GetChildCount() > 0)
		{
			base.FocusNeighborBottom = _badgeContainer.GetChild<NBadge>(0).GetPath();
			base.FocusNeighborTop = _badgeContainer.GetChild<NBadge>(0).GetPath();
			base.FocusNeighborLeft = _badgeContainer.GetChild<NBadge>(0).GetPath();
			base.FocusNeighborRight = _badgeContainer.GetChild<NBadge>(0).GetPath();
		}
		else if (_summaryContainer.DefaultFocusedControl != null)
		{
			base.FocusNeighborBottom = _summaryContainer.DefaultFocusedControl.GetPath();
			base.FocusNeighborTop = _summaryContainer.DefaultFocusedControl.GetPath();
			base.FocusNeighborLeft = _summaryContainer.DefaultFocusedControl.GetPath();
			base.FocusNeighborRight = _summaryContainer.DefaultFocusedControl.GetPath();
		}
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

	private async Task AnimateScoreLines()
	{
		_scoreLines.Clear();
		AddScoreLine("SCORE_LINE.floorsClimbed", "FloorCount", _runState.TotalFloor, $"+{ScoreUtility.GetScoreForFloor(_serializableRun.MapPointHistory)}", "res://images/ui/game_over_screen/score_floor.png");
		int amount = _serializableRun.MapPointHistory.SelectMany((List<MapPointHistoryEntry> actEntries) => actEntries).Sum((MapPointHistoryEntry e) => e.GetEntry(_localPlayer.NetId).GoldGained);
		AddScoreLine("SCORE_LINE.goldGained", "GoldAmount", amount, $"+{ScoreUtility.GetScoreForGoldGained(_serializableRun.MapPointHistory, _serializableRun.Players.Count)}", "res://images/ui/game_over_screen/score_gold.png");
		int elitesKilledCount = ScoreUtility.GetElitesKilledCount(_serializableRun.MapPointHistory);
		if (elitesKilledCount > 0)
		{
			AddScoreLine("SCORE_LINE.elitesKilled", "EliteCount", elitesKilledCount, $"+{ScoreUtility.GetScoreForElitesKilled(elitesKilledCount)}", "res://images/ui/game_over_screen/score_elite.png");
		}
		int bossesSlainCount = ScoreUtility.GetBossesSlainCount(_serializableRun.MapPointHistory, _history.Win);
		if (bossesSlainCount > 0)
		{
			AddScoreLine("SCORE_LINE.bossesSlain", "BossCount", bossesSlainCount, $"+{ScoreUtility.GetScoreForBossesSlain(bossesSlainCount)}", "res://images/ui/game_over_screen/score_boss.png");
		}
		int ascension = _history.Ascension;
		if (ascension > 0)
		{
			AddScoreLine("SCORE_LINE.ascension", "AscensionLevel", ascension, "x" + GetAscensionMulti(ascension), "res://images/ui/game_over_screen/score_ascension.png");
		}
		foreach (NScoreLine scoreLine in _scoreLines)
		{
			await scoreLine.AnimateIn();
		}
		await Cmd.Wait(0.5f, _cts.Token);
	}

	private async Task AnimateBadges()
	{
		List<Badge> badges = ScoreUtility.GetBadges(_serializableRun, _localPlayer.NetId, _history.Win);
		foreach (Badge item in badges)
		{
			_badgeContainer.AddChildSafely(NBadge.Create(item));
		}
		if (!_serializableRun.GameMode.AreAchievementsAndEpochsLocked())
		{
			SaveBadgesToProgress(badges);
		}
		await Cmd.Wait(0.25f, _cts.Token);
		foreach (NBadge item2 in _badgeContainer.GetChildren().OfType<NBadge>())
		{
			await item2.AnimateIn();
		}
		await Cmd.Wait(0.5f, _cts.Token);
	}

	/// <summary>
	/// Based on the badges you get, save them to the progress file.
	/// </summary>
	private void SaveBadgesToProgress(List<Badge> badgesToSave)
	{
		CharacterStats characterStats = SaveManager.Instance.Progress.CharacterStats[_localPlayer.Character.Id];
		foreach (Badge badge in badgesToSave)
		{
			BadgeStats badgeStats = characterStats.Badges.FirstOrDefault((BadgeStats b) => b.Id.Equals(badge.Id) && b.Rarity == badge.Rarity);
			if (badgeStats == null)
			{
				BadgeStats item = new BadgeStats
				{
					Id = badge.Id,
					Count = 1,
					Rarity = badge.Rarity
				};
				characterStats.Badges.Add(item);
				Log.Info("You got a new badge: " + badge.Id);
			}
			else
			{
				badgeStats.Count++;
				Log.Info($"You got badge: {badge.Id} again. Now you have {badgeStats.Count}!");
			}
		}
	}

	/// <summary>
	/// Helper function to create score lines, add child, and place it in a list to bulk animate later.
	/// </summary>
	private void AddScoreLine(string locEntryKey, string? locAmountKey = null, int amount = 0, string scoreLabel = "ERROR", string? iconPath = null)
	{
		LocString locString = new LocString("game_over_screen", locEntryKey);
		if (locAmountKey != null)
		{
			locString.Add(locAmountKey, amount);
		}
		Texture2D icon = ((iconPath == null) ? null : PreloadManager.Cache.GetTexture2D(iconPath));
		NScoreLine nScoreLine = NScoreLine.Create(locString.GetFormattedText(), scoreLabel, icon);
		_scoreLineContainer.AddChildSafely(nScoreLine);
		_scoreLines.Add(nScoreLine);
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
			if (!(await scoreTween.AwaitFinished(this)))
			{
				return;
			}
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
				if (!(await scoreTween.AwaitFinished(this)))
				{
					return;
				}
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
					if (!(await scoreTween.AwaitFinished(this)))
					{
						return;
					}
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
					if (!(await scoreTween.AwaitFinished(this)))
					{
						return;
					}
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

	/// <summary>
	/// Lookup table of score thresholds based on the total number of unlocks the player has completed.
	/// Must match: <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.GetEpochIdForUnlock" />.
	/// </summary>
	/// <param name="unlocksRemaining"></param>
	/// <returns></returns>
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
		_banner.ChangeText(new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.title").GetRawText());
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
		await _summaryContainer.AnimateInDiscoveries(_runState, _cts.Token);
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

	/// <summary>
	/// We move the Creature nodes so they render above the game over screen backstop!
	/// Drama!
	/// </summary>
	private void MoveCreaturesToDifferentLayerAndDisableUi()
	{
		List<NCreatureVisuals> list = new List<NCreatureVisuals>();
		List<NCreature> list2;
		if (NCombatRoom.Instance != null)
		{
			if (NCombatRoom.Instance.Mode == CombatRoomMode.ActiveCombat)
			{
				NCombatRoom.Instance.Ui.AnimOut();
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
				nCreatureVisuals.SpineAnimation.SetAnimation("die", loop: false);
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
				nCreatureVisuals2.SpineAnimation.SetAnimation("die", loop: false);
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

	/// <summary>
	/// Called when the Game Over screen appears (immediately upon Death/Victory)
	/// </summary>
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
		if (await backstopTween.AwaitFinished(this))
		{
			_banner.AnimateIn();
			backstopTween.Kill();
			Tween tween = CreateTween();
			tween.TweenProperty(_uiNode, "modulate:a", 1f, 0.25);
			if (await tween.AwaitFinished(this))
			{
				TaskHelper.RunSafely(AnimateInQuote());
				_continueButton.Enable();
			}
		}
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

	/// <summary>
	/// Little helper function that creates the x1.1 sort of string for the Score Line.
	/// </summary>
	/// <param name="ascension"></param>
	/// <returns></returns>
	private string GetAscensionMulti(int ascension)
	{
		int value = ascension / 10 + 1;
		int num = ascension % 10;
		if (num != 0)
		{
			return $"{value}.{num}";
		}
		return value.ToString();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(22);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DiscoveredAnyEpochs, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeBannerAndQuote, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenSummaryScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddScoreLine, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "locEntryKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "locAmountKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "scoreLabel", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
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
		list.Add(new MethodInfo(MethodName.GetAscensionMulti, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "ascension", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.AddScoreLine && args.Count == 5)
		{
			AddScoreLine(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<int>(in args[2]), VariantUtils.ConvertTo<string>(in args[3]), VariantUtils.ConvertTo<string>(in args[4]));
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
		if (method == MethodName.GetAscensionMulti && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetAscensionMulti(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
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
		if (method == MethodName.AddScoreLine)
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
		if (method == MethodName.GetAscensionMulti)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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
			_badgeContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scoreLineContainer)
		{
			_scoreLineContainer = VariantUtils.ConvertTo<GridContainer>(in value);
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

	/// <inheritdoc />
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
		Control from;
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
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
		if (name == PropertyName._scoreLineContainer)
		{
			value = VariantUtils.CreateFrom(in _scoreLineContainer);
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._continueButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewRunButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainMenuButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboardButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._badgeContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreLineContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._continueButton, Variant.From(in _continueButton));
		info.AddProperty(PropertyName._viewRunButton, Variant.From(in _viewRunButton));
		info.AddProperty(PropertyName._mainMenuButton, Variant.From(in _mainMenuButton));
		info.AddProperty(PropertyName._leaderboardButton, Variant.From(in _leaderboardButton));
		info.AddProperty(PropertyName._badgeContainer, Variant.From(in _badgeContainer));
		info.AddProperty(PropertyName._scoreLineContainer, Variant.From(in _scoreLineContainer));
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

	/// <inheritdoc />
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
			_badgeContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scoreLineContainer, out var value6))
		{
			_scoreLineContainer = value6.As<GridContainer>();
		}
		if (info.TryGetProperty(PropertyName._scoreBar, out var value7))
		{
			_scoreBar = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scoreFg, out var value8))
		{
			_scoreFg = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scoreProgress, out var value9))
		{
			_scoreProgress = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._unlocksRemaining, out var value10))
		{
			_unlocksRemaining = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._score, out var value11))
		{
			_score = value11.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scoreThreshold, out var value12))
		{
			_scoreThreshold = value12.As<int>();
		}
		if (info.TryGetProperty(PropertyName._scoreUnlockedEpochId, out var value13))
		{
			_scoreUnlockedEpochId = value13.As<string>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value14))
		{
			_leaderboard = value14.As<NDailyRunLeaderboard>();
		}
		if (info.TryGetProperty(PropertyName._creatureContainer, out var value15))
		{
			_creatureContainer = value15.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._summaryContainer, out var value16))
		{
			_summaryContainer = value16.As<NRunSummary>();
		}
		if (info.TryGetProperty(PropertyName._fullBlackBackstop, out var value17))
		{
			_fullBlackBackstop = value17.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._summaryBackstop, out var value18))
		{
			_summaryBackstop = value18.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value19))
		{
			_backstop = value19.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value20))
		{
			_banner = value20.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._deathQuote, out var value21))
		{
			_deathQuote = value21.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._victoryDamageLabel, out var value22))
		{
			_victoryDamageLabel = value22.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._uiNode, out var value23))
		{
			_uiNode = value23.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._screenshakeContainer, out var value24))
		{
			_screenshakeContainer = value24.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveryLabel, out var value25))
		{
			_discoveryLabel = value25.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._encounterQuote, out var value26))
		{
			_encounterQuote = value26.As<string>();
		}
		if (info.TryGetProperty(PropertyName._isAnimatingSummary, out var value27))
		{
			_isAnimatingSummary = value27.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._backstopMaterial, out var value28))
		{
			_backstopMaterial = value28.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._quoteTween, out var value29))
		{
			_quoteTween = value29.As<Tween>();
		}
	}
}
