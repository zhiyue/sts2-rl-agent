using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunLeaderboard.cs")]
public class NDailyRunLeaderboard : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetLocalizedText = "SetLocalizedText";

		public static readonly StringName Cleanup = "Cleanup";

		public static readonly StringName ChangePage = "ChangePage";

		public static readonly StringName SetPage = "SetPage";

		public static readonly StringName ClearEntries = "ClearEntries";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _paginator = "_paginator";

		public static readonly StringName _scoreContainer = "_scoreContainer";

		public static readonly StringName _leftArrow = "_leftArrow";

		public static readonly StringName _rightArrow = "_rightArrow";

		public static readonly StringName _loadingIndicator = "_loadingIndicator";

		public static readonly StringName _noScoresIndicator = "_noScoresIndicator";

		public static readonly StringName _noFriendsIndicator = "_noFriendsIndicator";

		public static readonly StringName _noScoreUploadIndicator = "_noScoreUploadIndicator";

		public static readonly StringName _currentPage = "_currentPage";

		public static readonly StringName _hasNegativeScore = "_hasNegativeScore";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_leaderboard");

	private const int _maxEntries = 10;

	private MegaLabel _titleLabel;

	private NLeaderboardDayPaginator _paginator;

	private VBoxContainer _scoreContainer;

	private NLeaderboardPageArrow _leftArrow;

	private NLeaderboardPageArrow _rightArrow;

	private MegaRichTextLabel _loadingIndicator;

	private MegaLabel _noScoresIndicator;

	private MegaLabel _noFriendsIndicator;

	private Control? _noScoreUploadIndicator;

	private int _currentPage;

	private DateTimeOffset _todaysDailyTime;

	private DateTimeOffset _leaderboardTime;

	private readonly List<ulong> _playersInRun = new List<ulong>();

	private bool _hasNegativeScore;

	private static readonly LocString _titleLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.title");

	private static readonly LocString _scoreLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.noScore");

	private static readonly LocString _fetchingScoreLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.fetchingScores");

	private static readonly LocString _friendsLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.LEADERBOARDS.noFriends");

	public static string[] AssetPaths => new string[1] { _scenePath };

	public override void _Ready()
	{
		_titleLabel = GetNode<MegaLabel>("%Title");
		_paginator = GetNode<NLeaderboardDayPaginator>("Paginator");
		_scoreContainer = GetNodeOrNull<VBoxContainer>("%ScoreContainer") ?? GetNodeOrNull<VBoxContainer>("%LeaderboardScoreContainer") ?? throw new InvalidOperationException("Couldn't find score container");
		_leftArrow = GetNode<NLeaderboardPageArrow>("%LeftArrow");
		_rightArrow = GetNode<NLeaderboardPageArrow>("%RightArrow");
		_loadingIndicator = GetNode<MegaRichTextLabel>("%LoadingText");
		_noScoresIndicator = GetNode<MegaLabel>("%NoScoresIndicator");
		_noFriendsIndicator = GetNode<MegaLabel>("%NoFriendsIndicator");
		_noScoreUploadIndicator = GetNodeOrNull<Control>("%ScoreWarning");
		CallDeferred(MethodName.SetLocalizedText);
		_loadingIndicator.SetTextAutoSize(_fetchingScoreLoc.GetFormattedText());
		_rightArrow.Connect(delegate
		{
			ChangePage(1);
		});
		_leftArrow.Connect(delegate
		{
			ChangePage(-1);
		});
	}

	private void SetLocalizedText()
	{
		_titleLabel.SetTextAutoSize(_titleLoc.GetFormattedText());
		_noScoresIndicator.SetTextAutoSize(_scoreLoc.GetFormattedText());
		_noFriendsIndicator.SetTextAutoSize(_friendsLoc.GetFormattedText());
	}

	public void Cleanup()
	{
		_leftArrow.Visible = false;
		_rightArrow.Visible = false;
		_loadingIndicator.Visible = false;
		_noScoresIndicator.Visible = false;
		_noFriendsIndicator.Visible = false;
		_paginator.Visible = false;
		if (_noScoreUploadIndicator != null)
		{
			_noScoreUploadIndicator.Visible = false;
		}
		ClearEntries();
	}

	public void Initialize(DateTimeOffset dateTime, IEnumerable<ulong> playersInRun, bool allowPagination)
	{
		_playersInRun.Clear();
		_playersInRun.AddRange(playersInRun);
		_paginator.Initialize(this, dateTime, allowPagination);
		_paginator.Visible = true;
		_todaysDailyTime = dateTime;
		SetDay(dateTime);
	}

	public void SetDay(DateTimeOffset dateTime)
	{
		_leaderboardTime = dateTime;
		SetPage(0);
	}

	private void ChangePage(int increment)
	{
		SetPage(_currentPage + increment);
	}

	private void SetPage(int page)
	{
		_currentPage = page;
		TaskHelper.RunSafely(LoadLeaderboard(_leaderboardTime, _currentPage));
	}

	private async Task LoadLeaderboard(DateTimeOffset dateTime, int page)
	{
		ClearEntries();
		_rightArrow.Disable();
		_leftArrow.Disable();
		_paginator.Disable();
		_noFriendsIndicator.Visible = false;
		_noScoresIndicator.Visible = false;
		_loadingIndicator.Visible = true;
		string leaderboardName = DailyRunUtility.GetLeaderboardName(dateTime, _playersInRun.Count);
		DateTimeOffset dateTime2 = dateTime - TimeSpan.FromDays(1);
		DateTimeOffset rightLeaderboardTime = dateTime + TimeSpan.FromDays(1);
		Task<ILeaderboardHandle?> mainTask = LeaderboardManager.GetLeaderboard(leaderboardName);
		Task<ILeaderboardHandle?> leftTask = LeaderboardManager.GetLeaderboard(DailyRunUtility.GetLeaderboardName(dateTime2, _playersInRun.Count));
		Task<ILeaderboardHandle?> rightTask = LeaderboardManager.GetLeaderboard(DailyRunUtility.GetLeaderboardName(rightLeaderboardTime, _playersInRun.Count));
		global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>> buffer = default(global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 0) = mainTask;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 1) = leftTask;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(ref buffer, 2) = rightTask;
		await Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray3<Task<ILeaderboardHandle>>, Task<ILeaderboardHandle>>(in buffer, 3));
		ILeaderboardHandle handle = await mainTask;
		if (handle != null)
		{
			List<LeaderboardEntry> list = await LeaderboardManager.QueryLeaderboard(handle, LeaderboardQueryType.Global, page * 10, 10);
			_noScoresIndicator.Visible = list.Count <= 0;
			FillEntries(list);
			_leftArrow.Visible = true;
			_rightArrow.Visible = true;
			if (page > 0)
			{
				_leftArrow.Enable();
			}
			else
			{
				_leftArrow.Disable();
			}
			if (page * 10 + 10 < LeaderboardManager.GetLeaderboardEntryCount(handle) && !_hasNegativeScore)
			{
				_rightArrow.Enable();
			}
			else
			{
				_rightArrow.Disable();
			}
		}
		else
		{
			_noScoresIndicator.Visible = true;
			_leftArrow.Visible = false;
			_rightArrow.Visible = false;
		}
		bool hasLeftLeaderboard = await leftTask != null;
		bool rightArrowEnabled = await rightTask != null || rightLeaderboardTime == _todaysDailyTime;
		_currentPage = page;
		_paginator.Enable(hasLeftLeaderboard, rightArrowEnabled);
		_loadingIndicator.Visible = false;
		if (_noScoreUploadIndicator != null && _todaysDailyTime == dateTime)
		{
			bool flag = await DailyRunUtility.ShouldUploadScore(handle, _playersInRun);
			_noScoreUploadIndicator.Visible = !flag;
		}
	}

	private void FillEntries(List<LeaderboardEntry> entries)
	{
		if (entries.Count == 0)
		{
			return;
		}
		_hasNegativeScore = false;
		NDailyRunLeaderboardRow child = NDailyRunLeaderboardRow.CreateHeader();
		_scoreContainer.AddChildSafely(child);
		NDailyRunLeaderboardSeparator child2 = NDailyRunLeaderboardSeparator.Create();
		_scoreContainer.AddChildSafely(child2);
		foreach (LeaderboardEntry entry in entries)
		{
			if (entry.score < 0)
			{
				_hasNegativeScore = true;
				continue;
			}
			_scoreContainer.AddChildSafely(NDailyRunLeaderboardRow.Create(entry));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLocalizedText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ChangePage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "increment", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetPage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "page", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ClearEntries && args.Count == 0)
		{
			ClearEntries();
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
		if (method == MethodName.ClearEntries)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
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
		if (name == PropertyName._currentPage)
		{
			_currentPage = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._hasNegativeScore)
		{
			_hasNegativeScore = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
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
		if (name == PropertyName._currentPage)
		{
			value = VariantUtils.CreateFrom(in _currentPage);
			return true;
		}
		if (name == PropertyName._hasNegativeScore)
		{
			value = VariantUtils.CreateFrom(in _hasNegativeScore);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._paginator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scoreContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noScoresIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noFriendsIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noScoreUploadIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentPage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hasNegativeScore, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._paginator, Variant.From(in _paginator));
		info.AddProperty(PropertyName._scoreContainer, Variant.From(in _scoreContainer));
		info.AddProperty(PropertyName._leftArrow, Variant.From(in _leftArrow));
		info.AddProperty(PropertyName._rightArrow, Variant.From(in _rightArrow));
		info.AddProperty(PropertyName._loadingIndicator, Variant.From(in _loadingIndicator));
		info.AddProperty(PropertyName._noScoresIndicator, Variant.From(in _noScoresIndicator));
		info.AddProperty(PropertyName._noFriendsIndicator, Variant.From(in _noFriendsIndicator));
		info.AddProperty(PropertyName._noScoreUploadIndicator, Variant.From(in _noScoreUploadIndicator));
		info.AddProperty(PropertyName._currentPage, Variant.From(in _currentPage));
		info.AddProperty(PropertyName._hasNegativeScore, Variant.From(in _hasNegativeScore));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._titleLabel, out var value))
		{
			_titleLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._paginator, out var value2))
		{
			_paginator = value2.As<NLeaderboardDayPaginator>();
		}
		if (info.TryGetProperty(PropertyName._scoreContainer, out var value3))
		{
			_scoreContainer = value3.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._leftArrow, out var value4))
		{
			_leftArrow = value4.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._rightArrow, out var value5))
		{
			_rightArrow = value5.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._loadingIndicator, out var value6))
		{
			_loadingIndicator = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._noScoresIndicator, out var value7))
		{
			_noScoresIndicator = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._noFriendsIndicator, out var value8))
		{
			_noFriendsIndicator = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._noScoreUploadIndicator, out var value9))
		{
			_noScoreUploadIndicator = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentPage, out var value10))
		{
			_currentPage = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._hasNegativeScore, out var value11))
		{
			_hasNegativeScore = value11.As<bool>();
		}
	}
}
