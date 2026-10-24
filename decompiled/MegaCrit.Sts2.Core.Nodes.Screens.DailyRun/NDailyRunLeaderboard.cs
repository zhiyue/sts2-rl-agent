using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunLeaderboard.cs")]
public class NDailyRunLeaderboard : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ToggleGlobalRank' method.
		/// </summary>
		public static readonly StringName ToggleGlobalRank = "ToggleGlobalRank";

		/// <summary>
		/// Cached name for the 'SetLocalizedText' method.
		/// </summary>
		public static readonly StringName SetLocalizedText = "SetLocalizedText";

		/// <summary>
		/// Cached name for the 'Cleanup' method.
		/// </summary>
		public static readonly StringName Cleanup = "Cleanup";

		/// <summary>
		/// Cached name for the 'ChangePage' method.
		/// </summary>
		public static readonly StringName ChangePage = "ChangePage";

		/// <summary>
		/// Cached name for the 'SetPage' method.
		/// </summary>
		public static readonly StringName SetPage = "SetPage";

		/// <summary>
		/// Cached name for the 'NavigateGlobalRank' method.
		/// </summary>
		public static readonly StringName NavigateGlobalRank = "NavigateGlobalRank";

		/// <summary>
		/// Cached name for the 'ClearEntries' method.
		/// </summary>
		public static readonly StringName ClearEntries = "ClearEntries";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_paginator' field.
		/// </summary>
		public static readonly StringName _paginator = "_paginator";

		/// <summary>
		/// Cached name for the '_scoreContainer' field.
		/// </summary>
		public static readonly StringName _scoreContainer = "_scoreContainer";

		/// <summary>
		/// Cached name for the '_leftArrow' field.
		/// </summary>
		public static readonly StringName _leftArrow = "_leftArrow";

		/// <summary>
		/// Cached name for the '_rightArrow' field.
		/// </summary>
		public static readonly StringName _rightArrow = "_rightArrow";

		/// <summary>
		/// Cached name for the '_loadingIndicator' field.
		/// </summary>
		public static readonly StringName _loadingIndicator = "_loadingIndicator";

		/// <summary>
		/// Cached name for the '_noScoresIndicator' field.
		/// </summary>
		public static readonly StringName _noScoresIndicator = "_noScoresIndicator";

		/// <summary>
		/// Cached name for the '_noFriendsIndicator' field.
		/// </summary>
		public static readonly StringName _noFriendsIndicator = "_noFriendsIndicator";

		/// <summary>
		/// Cached name for the '_noScoreUploadIndicator' field.
		/// </summary>
		public static readonly StringName _noScoreUploadIndicator = "_noScoreUploadIndicator";

		/// <summary>
		/// Cached name for the '_separators' field.
		/// </summary>
		public static readonly StringName _separators = "_separators";

		/// <summary>
		/// Cached name for the '_globalTickbox' field.
		/// </summary>
		public static readonly StringName _globalTickbox = "_globalTickbox";

		/// <summary>
		/// Cached name for the '_currentPage' field.
		/// </summary>
		public static readonly StringName _currentPage = "_currentPage";

		/// <summary>
		/// Cached name for the '_currentIndex' field.
		/// </summary>
		public static readonly StringName _currentIndex = "_currentIndex";

		/// <summary>
		/// Cached name for the '_hasNegativeScore' field.
		/// </summary>
		public static readonly StringName _hasNegativeScore = "_hasNegativeScore";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_leaderboard");

	private const int _maxEntries = 10;

	private NLeaderboardDayPaginator _paginator;

	private VBoxContainer _scoreContainer;

	private NLeaderboardPageArrow _leftArrow;

	private NLeaderboardPageArrow _rightArrow;

	private MegaRichTextLabel _loadingIndicator;

	private MegaLabel _noScoresIndicator;

	private MegaLabel _noFriendsIndicator;

	private Control? _noScoreUploadIndicator;

	private Control _separators;

	private NGlobalRankTickbox _globalTickbox;

	private const int _defaultGlobalIndex = -4;

	private int _currentPage;

	private int _currentIndex = -4;

	private DateTimeOffset _todaysDailyTime;

	private DateTimeOffset _leaderboardTime;

	private readonly List<ulong> _playersInRun = new List<ulong>();

	private bool _hasNegativeScore;

	private CancellationTokenSource? _loadCts;

	private static readonly LocString _titleLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.title");

	private static readonly LocString _scoreLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.noScore");

	private static readonly LocString _fetchingScoreLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.fetchingScores");

	private static readonly LocString _friendsLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.noFriends");

	public static string[] AssetPaths => new string[1] { _scenePath };

	public override void _Ready()
	{
		_paginator = GetNode<NLeaderboardDayPaginator>("Paginator");
		_scoreContainer = GetNodeOrNull<VBoxContainer>("%ScoreContainer") ?? GetNodeOrNull<VBoxContainer>("%LeaderboardScoreContainer") ?? throw new InvalidOperationException("Couldn't find score container");
		_leftArrow = GetNode<NLeaderboardPageArrow>("%LeftArrow");
		_rightArrow = GetNode<NLeaderboardPageArrow>("%RightArrow");
		_loadingIndicator = GetNode<MegaRichTextLabel>("%LoadingText");
		_noScoresIndicator = GetNode<MegaLabel>("%NoScoresIndicator");
		_noFriendsIndicator = GetNode<MegaLabel>("%NoFriendsIndicator");
		_noScoreUploadIndicator = GetNodeOrNull<Control>("%ScoreWarning");
		_separators = GetNode<Control>("%Separators");
		_globalTickbox = GetNode<NGlobalRankTickbox>("%GlobalTickbox");
		_globalTickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleGlobalRank));
		_globalTickbox.SetLabel(new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.globalTickbox").GetRawText());
		CallDeferred(MethodName.SetLocalizedText);
		_loadingIndicator.SetTextAutoSize(_fetchingScoreLoc.GetFormattedText());
		_rightArrow.Connect(delegate
		{
			ChangePage(_currentIndex += 10);
		});
		_leftArrow.Connect(delegate
		{
			ChangePage(_currentIndex -= 10);
		});
	}

	private void ToggleGlobalRank(NTickbox tickbox)
	{
		if (tickbox.IsTicked)
		{
			_currentIndex = -4;
			TaskHelper.RunSafely(LoadLeaderboard(_leaderboardTime, _currentPage, LeaderboardQueryType.AroundUser));
		}
		else
		{
			SetPage(0);
		}
	}

	private void SetLocalizedText()
	{
		GetNodeOrNull<MegaLabel>("%Title")?.SetTextAutoSize(_titleLoc.GetRawText());
		_noScoresIndicator.SetTextAutoSize(_scoreLoc.GetRawText());
		_noFriendsIndicator.SetTextAutoSize(_friendsLoc.GetRawText());
	}

	public void Cleanup()
	{
		_loadCts?.Cancel();
		_loadCts = null;
		_leftArrow.Visible = false;
		_rightArrow.Visible = false;
		_loadingIndicator.Visible = false;
		_noScoresIndicator.Visible = false;
		_noFriendsIndicator.Visible = false;
		_paginator.Visible = false;
		_globalTickbox.Visible = false;
		_currentIndex = -4;
		if (_noScoreUploadIndicator != null)
		{
			_noScoreUploadIndicator.Visible = false;
		}
		ClearEntries();
	}

	/// <summary>
	/// Does first-time initialization with the given day. Should only be called when the page loads and when a new
	/// player joins the run.
	/// </summary>
	public void Initialize(DateTimeOffset dateTime, IEnumerable<ulong> playersInRun, bool allowPagination)
	{
		_playersInRun.Clear();
		_playersInRun.AddRange(playersInRun);
		_paginator.Initialize(this, dateTime, allowPagination);
		_paginator.Visible = true;
		_todaysDailyTime = dateTime;
		SetDay(dateTime);
	}

	/// <summary>
	/// Called by the paginator when the day in the paginator changes.
	/// </summary>
	public void SetDay(DateTimeOffset dateTime)
	{
		_leaderboardTime = dateTime;
		if (!_globalTickbox.IsTicked)
		{
			SetPage(0);
		}
		else
		{
			TaskHelper.RunSafely(LoadLeaderboard(_leaderboardTime, _currentPage, LeaderboardQueryType.AroundUser));
		}
	}

	private void ChangePage(int _ = 0)
	{
		NavigateGlobalRank();
	}

	private void SetPage(int page)
	{
		_currentPage = page;
		TaskHelper.RunSafely(LoadLeaderboard(_leaderboardTime, _currentPage, LeaderboardQueryType.FriendsOnly));
	}

	/// <summary>
	/// Called when we press left/right arrows when viewing Global Rank.
	/// Lets other players view the scores and names of other players ranked above/below them.
	/// </summary>
	private void NavigateGlobalRank()
	{
		TaskHelper.RunSafely(LoadLeaderboard(_leaderboardTime, _currentPage, LeaderboardQueryType.AroundUser));
	}

	private async Task LoadLeaderboard(DateTimeOffset dateTime, int page, LeaderboardQueryType queryType)
	{
		if (_loadCts != null)
		{
			await _loadCts.CancelAsync();
		}
		_loadCts = new CancellationTokenSource();
		CancellationToken ct = _loadCts.Token;
		ClearEntries();
		_rightArrow.Disable();
		_leftArrow.Disable();
		_paginator.Disable();
		_noFriendsIndicator.Visible = false;
		_noScoresIndicator.Visible = false;
		_loadingIndicator.Visible = true;
		_separators.Visible = false;
		_globalTickbox.Visible = false;
		try
		{
			string leaderboardName = DailyRunUtility.GetLeaderboardName(dateTime, _playersInRun.Count);
			DateTimeOffset dateTime2 = dateTime - TimeSpan.FromDays(1);
			DateTimeOffset rightLeaderboardTime = dateTime + TimeSpan.FromDays(1);
			Task<ILeaderboardHandle?> mainTask = LeaderboardManager.GetLeaderboard(leaderboardName, ct);
			Task<ILeaderboardHandle?> leftTask = LeaderboardManager.GetLeaderboard(DailyRunUtility.GetLeaderboardName(dateTime2, _playersInRun.Count), ct);
			Task<ILeaderboardHandle?> rightTask = LeaderboardManager.GetLeaderboard(DailyRunUtility.GetLeaderboardName(rightLeaderboardTime, _playersInRun.Count), ct);
			global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>> buffer = default(global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 0) = mainTask;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 1) = leftTask;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 2) = rightTask;
			await Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(in buffer, 3));
			ILeaderboardHandle handle = await mainTask;
			if (ct.IsCancellationRequested)
			{
				return;
			}
			if (handle != null)
			{
				switch (queryType)
				{
				case LeaderboardQueryType.FriendsOnly:
					await QueryFriendScores(handle, page, ct);
					break;
				case LeaderboardQueryType.AroundUser:
					await QueryGlobalScores(handle, ct);
					break;
				}
			}
			else
			{
				_noScoresIndicator.Visible = true;
				_leftArrow.Visible = false;
				_rightArrow.Visible = false;
				_separators.Visible = false;
				_globalTickbox.Visible = false;
			}
			bool hasLeftLeaderboard = await leftTask != null;
			bool rightArrowEnabled = await rightTask != null || rightLeaderboardTime == _todaysDailyTime;
			_currentPage = page;
			_paginator.Enable(hasLeftLeaderboard, rightArrowEnabled);
			_loadingIndicator.Visible = false;
			if (_noScoreUploadIndicator != null && _todaysDailyTime == dateTime)
			{
				bool flag = await DailyRunUtility.ShouldUploadScore(handle, _playersInRun, ct);
				if (!ct.IsCancellationRequested)
				{
					_noScoreUploadIndicator.Visible = !flag;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private async Task QueryFriendScores(ILeaderboardHandle handle, int page, CancellationToken ct)
	{
		List<LeaderboardEntry> list = await LeaderboardManager.QueryLeaderboard(handle, LeaderboardQueryType.FriendsOnly, page * 10, 10, ct);
		if (!ct.IsCancellationRequested)
		{
			if (list.Count > 0)
			{
				_noScoresIndicator.Visible = false;
				_globalTickbox.Visible = true;
				_separators.Visible = true;
			}
			else
			{
				_noScoresIndicator.Visible = true;
				_globalTickbox.Visible = false;
				_separators.Visible = false;
			}
			FillEntries(list);
			_leftArrow.Visible = false;
			_rightArrow.Visible = false;
		}
	}

	private async Task QueryGlobalScores(ILeaderboardHandle handle, CancellationToken ct)
	{
		List<LeaderboardEntry> list = await LeaderboardManager.QueryLeaderboard(handle, LeaderboardQueryType.AroundUser, _currentIndex, 10, ct);
		if (ct.IsCancellationRequested)
		{
			return;
		}
		if (list.Count > 0)
		{
			_noScoresIndicator.Visible = false;
			_globalTickbox.Visible = true;
			_separators.Visible = true;
		}
		else
		{
			_noScoresIndicator.Visible = true;
			_globalTickbox.Visible = false;
			_separators.Visible = false;
		}
		FillEntries(list);
		if (list.Count > 0)
		{
			int leaderboardEntryCount = LeaderboardManager.GetLeaderboardEntryCount(handle);
			_leftArrow.Visible = true;
			_rightArrow.Visible = true;
			if (list[0].rank > 0)
			{
				_leftArrow.Enable();
			}
			else
			{
				_leftArrow.Disable();
			}
			if (list[list.Count - 1].rank < leaderboardEntryCount - 1 && !_hasNegativeScore)
			{
				_rightArrow.Enable();
			}
			else
			{
				_rightArrow.Disable();
			}
		}
	}

	private void FillEntries(List<LeaderboardEntry> entries)
	{
		if (entries.Count == 0)
		{
			return;
		}
		_hasNegativeScore = false;
		NDailyRunLeaderboardHeader child = NDailyRunLeaderboardHeader.Create();
		_scoreContainer.AddChildSafely(child);
		NDailyRunLeaderboardSeparator child2 = NDailyRunLeaderboardSeparator.Create();
		_scoreContainer.AddChildSafely(child2);
		ulong localPlayerId = PlatformUtil.GetLocalPlayerId(LeaderboardManager.CurrentPlatform);
		foreach (LeaderboardEntry entry in entries)
		{
			if (entry.score < 0)
			{
				_hasNegativeScore = true;
				continue;
			}
			_scoreContainer.AddChildSafely(NDailyRunLeaderboardRow.Create(entry, entry.userIds.Contains(localPlayerId)));
			_scoreContainer.AddChildSafely(NDailyRunLeaderboardSeparator.Create());
		}
	}

	private void ClearEntries()
	{
		_noScoresIndicator.Visible = false;
		_noFriendsIndicator.Visible = false;
		foreach (Node child in _scoreContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
	}

	public override void _ExitTree()
	{
		_loadCts?.Cancel();
		_loadCts = null;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleGlobalRank, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetLocalizedText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ChangePage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetPage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "page", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.NavigateGlobalRank, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ToggleGlobalRank && args.Count == 1)
		{
			ToggleGlobalRank(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLocalizedText && args.Count == 0)
		{
			SetLocalizedText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cleanup && args.Count == 0)
		{
			Cleanup();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ChangePage && args.Count == 1)
		{
			ChangePage(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPage && args.Count == 1)
		{
			SetPage(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NavigateGlobalRank && args.Count == 0)
		{
			NavigateGlobalRank();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearEntries && args.Count == 0)
		{
			ClearEntries();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
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
		if (method == MethodName.ToggleGlobalRank)
		{
			return true;
		}
		if (method == MethodName.SetLocalizedText)
		{
			return true;
		}
		if (method == MethodName.Cleanup)
		{
			return true;
		}
		if (method == MethodName.ChangePage)
		{
			return true;
		}
		if (method == MethodName.SetPage)
		{
			return true;
		}
		if (method == MethodName.NavigateGlobalRank)
		{
			return true;
		}
		if (method == MethodName.ClearEntries)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._paginator)
		{
			_paginator = VariantUtils.ConvertTo<NLeaderboardDayPaginator>(in value);
			return true;
		}
		if (name == PropertyName._scoreContainer)
		{
			_scoreContainer = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			_leftArrow = VariantUtils.ConvertTo<NLeaderboardPageArrow>(in value);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			_rightArrow = VariantUtils.ConvertTo<NLeaderboardPageArrow>(in value);
			return true;
		}
		if (name == PropertyName._loadingIndicator)
		{
			_loadingIndicator = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._noScoresIndicator)
		{
			_noScoresIndicator = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._noFriendsIndicator)
		{
			_noFriendsIndicator = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._noScoreUploadIndicator)
		{
			_noScoreUploadIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._separators)
		{
			_separators = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._globalTickbox)
		{
			_globalTickbox = VariantUtils.ConvertTo<NGlobalRankTickbox>(in value);
			return true;
		}
		if (name == PropertyName._currentPage)
		{
			_currentPage = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._currentIndex)
		{
			_currentIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._hasNegativeScore)
		{
			_hasNegativeScore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._paginator)
		{
			value = VariantUtils.CreateFrom(in _paginator);
			return true;
		}
		if (name == PropertyName._scoreContainer)
		{
			value = VariantUtils.CreateFrom(in _scoreContainer);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			value = VariantUtils.CreateFrom(in _leftArrow);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			value = VariantUtils.CreateFrom(in _rightArrow);
			return true;
		}
		if (name == PropertyName._loadingIndicator)
		{
			value = VariantUtils.CreateFrom(in _loadingIndicator);
			return true;
		}
		if (name == PropertyName._noScoresIndicator)
		{
			value = VariantUtils.CreateFrom(in _noScoresIndicator);
			return true;
		}
		if (name == PropertyName._noFriendsIndicator)
		{
			value = VariantUtils.CreateFrom(in _noFriendsIndicator);
			return true;
		}
		if (name == PropertyName._noScoreUploadIndicator)
		{
			value = VariantUtils.CreateFrom(in _noScoreUploadIndicator);
			return true;
		}
		if (name == PropertyName._separators)
		{
			value = VariantUtils.CreateFrom(in _separators);
			return true;
		}
		if (name == PropertyName._globalTickbox)
		{
			value = VariantUtils.CreateFrom(in _globalTickbox);
			return true;
		}
		if (name == PropertyName._currentPage)
		{
			value = VariantUtils.CreateFrom(in _currentPage);
			return true;
		}
		if (name == PropertyName._currentIndex)
		{
			value = VariantUtils.CreateFrom(in _currentIndex);
			return true;
		}
		if (name == PropertyName._hasNegativeScore)
		{
			value = VariantUtils.CreateFrom(in _hasNegativeScore);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._paginator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noScoresIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noFriendsIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noScoreUploadIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._separators, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._globalTickbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentPage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hasNegativeScore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._paginator, Variant.From(in _paginator));
		info.AddProperty(PropertyName._scoreContainer, Variant.From(in _scoreContainer));
		info.AddProperty(PropertyName._leftArrow, Variant.From(in _leftArrow));
		info.AddProperty(PropertyName._rightArrow, Variant.From(in _rightArrow));
		info.AddProperty(PropertyName._loadingIndicator, Variant.From(in _loadingIndicator));
		info.AddProperty(PropertyName._noScoresIndicator, Variant.From(in _noScoresIndicator));
		info.AddProperty(PropertyName._noFriendsIndicator, Variant.From(in _noFriendsIndicator));
		info.AddProperty(PropertyName._noScoreUploadIndicator, Variant.From(in _noScoreUploadIndicator));
		info.AddProperty(PropertyName._separators, Variant.From(in _separators));
		info.AddProperty(PropertyName._globalTickbox, Variant.From(in _globalTickbox));
		info.AddProperty(PropertyName._currentPage, Variant.From(in _currentPage));
		info.AddProperty(PropertyName._currentIndex, Variant.From(in _currentIndex));
		info.AddProperty(PropertyName._hasNegativeScore, Variant.From(in _hasNegativeScore));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._paginator, out var value))
		{
			_paginator = value.As<NLeaderboardDayPaginator>();
		}
		if (info.TryGetProperty(PropertyName._scoreContainer, out var value2))
		{
			_scoreContainer = value2.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._leftArrow, out var value3))
		{
			_leftArrow = value3.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._rightArrow, out var value4))
		{
			_rightArrow = value4.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._loadingIndicator, out var value5))
		{
			_loadingIndicator = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._noScoresIndicator, out var value6))
		{
			_noScoresIndicator = value6.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._noFriendsIndicator, out var value7))
		{
			_noFriendsIndicator = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._noScoreUploadIndicator, out var value8))
		{
			_noScoreUploadIndicator = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._separators, out var value9))
		{
			_separators = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._globalTickbox, out var value10))
		{
			_globalTickbox = value10.As<NGlobalRankTickbox>();
		}
		if (info.TryGetProperty(PropertyName._currentPage, out var value11))
		{
			_currentPage = value11.As<int>();
		}
		if (info.TryGetProperty(PropertyName._currentIndex, out var value12))
		{
			_currentIndex = value12.As<int>();
		}
		if (info.TryGetProperty(PropertyName._hasNegativeScore, out var value13))
		{
			_hasNegativeScore = value13.As<bool>();
		}
	}
}
