using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NRewardsScreen.cs")]
public class NRewardsScreen : Control, IOverlayScreen, IScreenContext
{
	[Signal]
	public delegate void CompletedEventHandler();

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RewardCollectedFrom = "RewardCollectedFrom";

		public static readonly StringName RewardSkippedFrom = "RewardSkippedFrom";

		public static readonly StringName UpdateScreenState = "UpdateScreenState";

		public static readonly StringName RemoveButton = "RemoveButton";

		public static readonly StringName OnProceedButtonPressed = "OnProceedButtonPressed";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName TryEnableProceedButton = "TryEnableProceedButton";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateScrollPosition = "UpdateScrollPosition";

		public static readonly StringName HideWaitingForPlayersScreen = "HideWaitingForPlayersScreen";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CanScroll = "CanScroll";

		public static readonly StringName ScrollLimitBottom = "ScrollLimitBottom";

		public static readonly StringName IsComplete = "IsComplete";

		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _rewardsContainer = "_rewardsContainer";

		public static readonly StringName _scrollbar = "_scrollbar";

		public static readonly StringName _headerLabel = "_headerLabel";

		public static readonly StringName _rewardContainerMask = "_rewardContainerMask";

		public static readonly StringName _waitingForOtherPlayersOverlay = "_waitingForOtherPlayersOverlay";

		public static readonly StringName _rewardsWindow = "_rewardsWindow";

		public static readonly StringName _targetDragPos = "_targetDragPos";

		public static readonly StringName _scrollbarPressed = "_scrollbarPressed";

		public static readonly StringName _fadeTween = "_fadeTween";

		public static readonly StringName _lastRewardFocused = "_lastRewardFocused";

		public static readonly StringName _isTerminal = "_isTerminal";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName Completed = "Completed";
	}

	private const float _scrollLimitTop = 35f;

	private const int _scrollbarThreshold = 400;

	private IRunState _runState;

	private NProceedButton _proceedButton;

	private Control _rewardsContainer;

	private NScrollbar _scrollbar;

	private MegaLabel _headerLabel;

	private Control _rewardContainerMask;

	private Control _waitingForOtherPlayersOverlay;

	private Control _rewardsWindow;

	private Vector2 _targetDragPos;

	private bool _scrollbarPressed;

	private Tween? _fadeTween;

	private readonly List<Control> _rewardButtons = new List<Control>();

	private readonly List<Control> _skippedRewardButtons = new List<Control>();

	private Control? _lastRewardFocused;

	private bool _isTerminal;

	private readonly TaskCompletionSource _closedCompletionSource = new TaskCompletionSource();

	private static readonly LocString _waitingLoc = new LocString("gameplay_ui", "MULTIPLAYER_WAITING");

	private CompletedEventHandler backing_Completed;

	private bool CanScroll => _rewardsContainer.Size.Y >= 400f;

	private float ScrollLimitBottom => 35f - _rewardsContainer.Size.Y + 400f;

	private static string ScenePath => SceneHelper.GetScenePath("screens/rewards_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public Task ClosedTask => _closedCompletionSource.Task;

	public bool IsComplete { get; private set; }

	public NetScreenType ScreenType => NetScreenType.Rewards;

	public bool UseSharedBackstop => true;

	public Control DefaultFocusedControl
	{
		get
		{
			if (_rewardButtons.Count == 0)
			{
				return _rewardsContainer;
			}
			return _lastRewardFocused ?? _rewardButtons[0];
		}
	}

	public Control FocusedControlFromTopBar
	{
		get
		{
			if (_rewardButtons.Count <= 0)
			{
				return _rewardsContainer;
			}
			return _rewardButtons[0];
		}
	}

	public event CompletedEventHandler Completed
	{
		add
		{
			backing_Completed = (CompletedEventHandler)Delegate.Combine(backing_Completed, value);
		}
		remove
		{
			backing_Completed = (CompletedEventHandler)Delegate.Remove(backing_Completed, value);
		}
	}

	public static NRewardsScreen ShowScreen(bool isTerminal, IRunState runState)
	{
		NRewardsScreen nRewardsScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NRewardsScreen>(PackedScene.GenEditState.Disabled);
		nRewardsScreen._isTerminal = isTerminal;
		nRewardsScreen._runState = runState;
		NOverlayStack.Instance.Push(nRewardsScreen);
		return nRewardsScreen;
	}

	public override void _Ready()
	{
		_proceedButton = GetNode<NProceedButton>("ProceedButton");
		_rewardContainerMask = GetNode<Control>("%RewardContainerMask");
		_rewardsContainer = GetNode<Control>("%RewardsContainer");
		_scrollbar = GetNode<NScrollbar>("%Scrollbar");
		_headerLabel = GetNode<MegaLabel>("%HeaderLabel");
		_waitingForOtherPlayersOverlay = GetNode<Control>("%WaitingForOtherPlayers");
		_waitingForOtherPlayersOverlay.GetNode<MegaLabel>("Label").SetTextAutoSize(_waitingLoc.GetRawText());
		_rewardsWindow = GetNode<Control>("Rewards");
		_rewardsWindow.Modulate = StsColors.transparentBlack;
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnProceedButtonPressed));
		_proceedButton.SetPulseState(isPulsing: false);
		TryEnableProceedButton();
		_proceedButton.UpdateText(NProceedButton.SkipLoc);
		NDebugAudioManager.Instance?.Play("victory.mp3");
		_scrollbar.Connect(NScrollbar.SignalName.MousePressed, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = true;
		}));
		_scrollbar.Connect(NScrollbar.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = false;
		}));
		_targetDragPos = new Vector2(_rewardsContainer.Position.X, 35f);
		if (_runState.CurrentRoom is CombatRoom { GoldProportion: <1f })
		{
			_headerLabel.SetTextAutoSize(new LocString("gameplay_ui", "COMBAT_REWARD_HEADER_MUGGED").GetFormattedText());
		}
		else
		{
			_headerLabel.SetTextAutoSize(new LocString("gameplay_ui", "COMBAT_REWARD_HEADER_LOOT").GetFormattedText());
		}
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
		_rewardsContainer.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			DefaultFocusedControl.TryGrabFocus();
		}));
		ActiveScreenContext.Instance.Update();
	}

	public void SetRewards(IEnumerable<Reward> rewards)
	{
		foreach (Control rewardButton in _rewardButtons)
		{
			RemoveButton(rewardButton);
		}
		List<Reward> list = rewards.ToList();
		_rewardButtons.Clear();
		foreach (Reward item in list)
		{
			Control option;
			if (item is LinkedRewardSet linkedReward)
			{
				option = NLinkedRewardSet.Create(linkedReward, this);
				option.Connect(NLinkedRewardSet.SignalName.RewardClaimed, Callable.From<NLinkedRewardSet>(RewardCollectedFrom));
			}
			else
			{
				option = NRewardButton.Create(item, this);
				option.Connect(NRewardButton.SignalName.RewardClaimed, Callable.From<NRewardButton>(RewardCollectedFrom));
				option.Connect(NRewardButton.SignalName.RewardSkipped, Callable.From<NRewardButton>(RewardSkippedFrom));
				option.Connect(Control.SignalName.FocusEntered, Callable.From(() => _lastRewardFocused = option));
			}
			item.MarkContentAsSeen();
			_rewardButtons.Add(option);
			_rewardsContainer.AddChildSafely(option);
		}
		UpdateScreenState();
		if (list.Count == 0)
		{
			TryEnableProceedButton();
		}
		if (_rewardsContainer.HasFocus())
		{
			DefaultFocusedControl.TryGrabFocus();
		}
		TaskHelper.RunSafely(RelicFtueCheck());
	}

	private async Task RelicFtueCheck()
	{
		if (SaveManager.Instance.SeenFtue("obtain_relic_ftue"))
		{
			return;
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		foreach (NRewardButton item in _rewardButtons.OfType<NRewardButton>())
		{
			if (item.Reward is RelicReward)
			{
				NModalContainer.Instance.Add(NRelicRewardFtue.Create(item));
				SaveManager.Instance.MarkFtueAsComplete("obtain_relic_ftue");
				break;
			}
		}
	}

	public void RewardCollectedFrom(Control button)
	{
		int a = _rewardButtons.IndexOf(button);
		RemoveButton(button);
		_lastRewardFocused = ((_rewardButtons.Count > 0) ? _rewardButtons[Mathf.Min(a, _rewardButtons.Count - 1)] : null);
		UpdateScreenState();
		if (_rewardButtons.Count > 0 || _isTerminal)
		{
			TryEnableProceedButton();
			if (!_rewardButtons.Except(_skippedRewardButtons).Any())
			{
				_proceedButton.SetPulseState(isPulsing: true);
			}
		}
	}

	public void RewardSkippedFrom(Control button)
	{
		_skippedRewardButtons.Add(button);
		if (!_rewardButtons.Except(_skippedRewardButtons).Any())
		{
			_proceedButton.SetPulseState(isPulsing: true);
		}
	}

	private void UpdateScreenState()
	{
		if (_rewardButtons.Count == 0)
		{
			if (_isTerminal)
			{
				_fadeTween?.Kill();
				_fadeTween = CreateTween().SetParallel();
				_fadeTween.TweenProperty(GetNode<Control>("Rewards"), "modulate:a", 0f, 0.25);
				NOverlayStack.Instance.HideBackstop();
				_proceedButton.UpdateText(NProceedButton.ProceedLoc);
				_proceedButton.SetPulseState(isPulsing: true);
				_rewardsContainer.FocusMode = FocusModeEnum.None;
				IsComplete = true;
				EmitSignal(SignalName.Completed);
			}
			else
			{
				NOverlayStack.Instance.Remove(this);
			}
		}
		_rewardsContainer.ResetSize();
		_scrollbar.Visible = CanScroll;
		_scrollbar.MouseFilter = (MouseFilterEnum)(CanScroll ? 0 : 2);
		if (!CanScroll)
		{
			_targetDragPos.Y = 35f;
		}
		for (int i = 0; i < _rewardButtons.Count; i++)
		{
			_rewardButtons[i].FocusNeighborLeft = _rewardButtons[i].GetPath();
			_rewardButtons[i].FocusNeighborRight = _rewardButtons[i].GetPath();
			_rewardButtons[i].FocusNeighborTop = ((i > 0) ? _rewardButtons[i - 1].GetPath() : _rewardButtons[i].GetPath());
			_rewardButtons[i].FocusNeighborBottom = ((i < _rewardButtons.Count - 1) ? _rewardButtons[i + 1].GetPath() : _rewardButtons[i].GetPath());
		}
	}

	private void RemoveButton(Control button)
	{
		button.GetParent().RemoveChildSafely(button);
		button.QueueFreeSafely();
		int a = _rewardButtons.IndexOf(button);
		_rewardButtons.Remove(button);
		if (_rewardButtons.Count > 0)
		{
			a = Mathf.Min(a, _rewardButtons.Count - 1);
			_rewardButtons[a].TryGrabFocus();
		}
		else if (_rewardButtons.Contains(GetViewport().GuiGetFocusOwner()))
		{
			ActiveScreenContext.Instance.Update();
		}
	}

	private void OnProceedButtonPressed(NButton _)
	{
		if (RunManager.Instance.debugAfterCombatRewardsOverride != null && _isTerminal)
		{
			RunManager.Instance.debugAfterCombatRewardsOverride?.Invoke();
		}
		else if (_isTerminal && (_runState.CurrentRoom.RoomType == RoomType.Boss || _runState.CurrentRoom.IsVictoryRoom))
		{
			if (_runState.Map.SecondBossMapPoint != null && _runState.CurrentMapCoord == _runState.Map.BossMapPoint.coord)
			{
				TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen());
				return;
			}
			_proceedButton.Disable();
			if (RunManager.Instance.ActChangeSynchronizer.IsWaitingForOtherPlayers())
			{
				_waitingForOtherPlayersOverlay.Visible = true;
			}
			RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
		}
		else if (_isTerminal)
		{
			if (_proceedButton.IsSkip)
			{
				if (TestMode.IsOn || SaveManager.Instance.SeenFtue("combat_reward_ftue"))
				{
					TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen());
				}
				else
				{
					TaskHelper.RunSafely(RewardFtueCheck());
				}
				return;
			}
			if (_runState.ActFloor > 4)
			{
				SaveManager.Instance.MarkFtueAsComplete("combat_reward_ftue");
			}
			TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen());
		}
		else
		{
			NOverlayStack.Instance.Remove(this);
		}
	}

	public void AfterOverlayOpened()
	{
	}

	public void AfterOverlayClosed()
	{
		if (RunManager.Instance.IsInProgress && !RunManager.Instance.IsCleaningUp)
		{
			foreach (NRewardButton item in _rewardsContainer.GetChildren().OfType<NRewardButton>())
			{
				item.Reward?.OnSkipped();
			}
			foreach (NLinkedRewardSet item2 in _rewardsContainer.GetChildren().OfType<NLinkedRewardSet>())
			{
				item2.LinkedRewardSet.OnSkipped();
			}
			_closedCompletionSource.SetResult();
		}
		_proceedButton.Disable();
		this.QueueFreeSafely();
	}

	private void TryEnableProceedButton()
	{
		if (Hook.ShouldProceedToNextMapPoint(_runState) && !_proceedButton.IsEnabled)
		{
			if (_isTerminal && _rewardButtons.Count == 0)
			{
				NOverlayStack.Instance.HideBackstop();
			}
			_proceedButton.Enable();
		}
	}

	public void AfterOverlayShown()
	{
		TryEnableProceedButton();
		if (!IsComplete)
		{
			_fadeTween?.FastForwardToCompletion();
			_fadeTween = CreateTween().SetParallel();
			_fadeTween.TweenProperty(_rewardsWindow, "modulate", Colors.White, 0.5);
			_fadeTween.TweenProperty(_rewardsWindow, "position:y", _rewardsWindow.Position.Y, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
				.From(_rewardsWindow.Position.Y + 100f);
		}
	}

	public void AfterOverlayHidden()
	{
		_proceedButton.Disable();
		if (!IsComplete)
		{
			_fadeTween?.FastForwardToCompletion();
			_fadeTween = CreateTween();
			_fadeTween.TweenProperty(_rewardsWindow, "modulate:a", 0, 0.25);
		}
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (IsVisibleInTree() && CanScroll)
		{
			ProcessScrollEvent(inputEvent);
		}
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		_targetDragPos += new Vector2(0f, ScrollHelper.GetDragForScrollEvent(inputEvent));
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (IsVisibleInTree() && CanScroll && NControllerManager.Instance.IsUsingController && _rewardButtons.Contains(focusedControl))
		{
			float value = 0f - focusedControl.Position.Y + _rewardContainerMask.Size.Y * 0.5f;
			value = Mathf.Clamp(value, ScrollLimitBottom, 35f);
			_targetDragPos = new Vector2(_targetDragPos.X, value);
		}
	}

	public override void _Process(double delta)
	{
		if (IsVisibleInTree())
		{
			UpdateScrollPosition(delta);
		}
	}

	private void UpdateScrollPosition(double delta)
	{
		if (!_rewardsContainer.Position.IsEqualApprox(_targetDragPos))
		{
			_rewardsContainer.Position = _rewardsContainer.Position.Lerp(_targetDragPos, Mathf.Clamp((float)delta * 15f, 0f, 1f));
			if (_rewardsContainer.Position.DistanceTo(_targetDragPos) < 0.5f)
			{
				_rewardsContainer.Position = _targetDragPos;
			}
			if (!_scrollbarPressed && CanScroll)
			{
				_scrollbar.SetValueWithoutAnimation(Mathf.Clamp(_rewardsContainer.Position.Y / ScrollLimitBottom, 0f, 1f) * 100f);
			}
		}
		if (_scrollbarPressed)
		{
			_targetDragPos.Y = Mathf.Lerp(35f, ScrollLimitBottom, (float)_scrollbar.Value * 0.01f);
		}
		if (_targetDragPos.Y < Mathf.Min(ScrollLimitBottom, 0f))
		{
			_targetDragPos.Y = Mathf.Lerp(_targetDragPos.Y, ScrollLimitBottom, (float)delta * 12f);
		}
		else if (_targetDragPos.Y > Mathf.Max(ScrollLimitBottom, 0f))
		{
			_targetDragPos.Y = Mathf.Lerp(_targetDragPos.Y, 35f, (float)delta * 12f);
		}
	}

	private async Task RewardFtueCheck()
	{
		_proceedButton.Hide();
		NCombatRewardFtue nCombatRewardFtue = NCombatRewardFtue.Create(_rewardsContainer);
		NModalContainer.Instance.Add(nCombatRewardFtue);
		SaveManager.Instance.MarkFtueAsComplete("combat_reward_ftue");
		await nCombatRewardFtue.WaitForPlayerToConfirm();
		_proceedButton.Show();
	}

	public void HideWaitingForPlayersScreen()
	{
		_waitingForOtherPlayersOverlay.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(17);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RewardCollectedFrom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RewardSkippedFrom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScreenState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryEnableProceedButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessScrollEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessGuiFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusedControl", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideWaitingForPlayersScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RewardCollectedFrom && args.Count == 1)
		{
			RewardCollectedFrom(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RewardSkippedFrom && args.Count == 1)
		{
			RewardSkippedFrom(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScreenState && args.Count == 0)
		{
			UpdateScreenState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveButton && args.Count == 1)
		{
			RemoveButton(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed && args.Count == 1)
		{
			OnProceedButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryEnableProceedButton && args.Count == 0)
		{
			TryEnableProceedButton();
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
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessScrollEvent && args.Count == 1)
		{
			ProcessScrollEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessGuiFocus && args.Count == 1)
		{
			ProcessGuiFocus(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScrollPosition && args.Count == 1)
		{
			UpdateScrollPosition(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideWaitingForPlayersScreen && args.Count == 0)
		{
			HideWaitingForPlayersScreen();
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
		if (method == MethodName.RewardCollectedFrom)
		{
			return true;
		}
		if (method == MethodName.RewardSkippedFrom)
		{
			return true;
		}
		if (method == MethodName.UpdateScreenState)
		{
			return true;
		}
		if (method == MethodName.RemoveButton)
		{
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.TryEnableProceedButton)
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
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.ProcessScrollEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessGuiFocus)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateScrollPosition)
		{
			return true;
		}
		if (method == MethodName.HideWaitingForPlayersScreen)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsComplete)
		{
			IsComplete = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._rewardsContainer)
		{
			_rewardsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scrollbar)
		{
			_scrollbar = VariantUtils.ConvertTo<NScrollbar>(in value);
			return true;
		}
		if (name == PropertyName._headerLabel)
		{
			_headerLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._rewardContainerMask)
		{
			_rewardContainerMask = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._waitingForOtherPlayersOverlay)
		{
			_waitingForOtherPlayersOverlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._rewardsWindow)
		{
			_rewardsWindow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			_targetDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			_scrollbarPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			_fadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._lastRewardFocused)
		{
			_lastRewardFocused = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isTerminal)
		{
			_isTerminal = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.CanScroll)
		{
			from = CanScroll;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ScrollLimitBottom)
		{
			value = VariantUtils.CreateFrom<float>(ScrollLimitBottom);
			return true;
		}
		if (name == PropertyName.IsComplete)
		{
			from = IsComplete;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			from = UseSharedBackstop;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		Control from2;
		if (name == PropertyName.DefaultFocusedControl)
		{
			from2 = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from2 = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._rewardsContainer)
		{
			value = VariantUtils.CreateFrom(in _rewardsContainer);
			return true;
		}
		if (name == PropertyName._scrollbar)
		{
			value = VariantUtils.CreateFrom(in _scrollbar);
			return true;
		}
		if (name == PropertyName._headerLabel)
		{
			value = VariantUtils.CreateFrom(in _headerLabel);
			return true;
		}
		if (name == PropertyName._rewardContainerMask)
		{
			value = VariantUtils.CreateFrom(in _rewardContainerMask);
			return true;
		}
		if (name == PropertyName._waitingForOtherPlayersOverlay)
		{
			value = VariantUtils.CreateFrom(in _waitingForOtherPlayersOverlay);
			return true;
		}
		if (name == PropertyName._rewardsWindow)
		{
			value = VariantUtils.CreateFrom(in _rewardsWindow);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			value = VariantUtils.CreateFrom(in _targetDragPos);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			value = VariantUtils.CreateFrom(in _scrollbarPressed);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			value = VariantUtils.CreateFrom(in _fadeTween);
			return true;
		}
		if (name == PropertyName._lastRewardFocused)
		{
			value = VariantUtils.CreateFrom(in _lastRewardFocused);
			return true;
		}
		if (name == PropertyName._isTerminal)
		{
			value = VariantUtils.CreateFrom(in _isTerminal);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollbar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardContainerMask, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._waitingForOtherPlayersOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardsWindow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CanScroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scrollbarPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastRewardFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isTerminal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsComplete, Variant.From<bool>(IsComplete));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._rewardsContainer, Variant.From(in _rewardsContainer));
		info.AddProperty(PropertyName._scrollbar, Variant.From(in _scrollbar));
		info.AddProperty(PropertyName._headerLabel, Variant.From(in _headerLabel));
		info.AddProperty(PropertyName._rewardContainerMask, Variant.From(in _rewardContainerMask));
		info.AddProperty(PropertyName._waitingForOtherPlayersOverlay, Variant.From(in _waitingForOtherPlayersOverlay));
		info.AddProperty(PropertyName._rewardsWindow, Variant.From(in _rewardsWindow));
		info.AddProperty(PropertyName._targetDragPos, Variant.From(in _targetDragPos));
		info.AddProperty(PropertyName._scrollbarPressed, Variant.From(in _scrollbarPressed));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
		info.AddProperty(PropertyName._lastRewardFocused, Variant.From(in _lastRewardFocused));
		info.AddProperty(PropertyName._isTerminal, Variant.From(in _isTerminal));
		info.AddSignalEventDelegate(SignalName.Completed, backing_Completed);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsComplete, out var value))
		{
			IsComplete = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value2))
		{
			_proceedButton = value2.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._rewardsContainer, out var value3))
		{
			_rewardsContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scrollbar, out var value4))
		{
			_scrollbar = value4.As<NScrollbar>();
		}
		if (info.TryGetProperty(PropertyName._headerLabel, out var value5))
		{
			_headerLabel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._rewardContainerMask, out var value6))
		{
			_rewardContainerMask = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._waitingForOtherPlayersOverlay, out var value7))
		{
			_waitingForOtherPlayersOverlay = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._rewardsWindow, out var value8))
		{
			_rewardsWindow = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPos, out var value9))
		{
			_targetDragPos = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarPressed, out var value10))
		{
			_scrollbarPressed = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value11))
		{
			_fadeTween = value11.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._lastRewardFocused, out var value12))
		{
			_lastRewardFocused = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isTerminal, out var value13))
		{
			_isTerminal = value13.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<CompletedEventHandler>(SignalName.Completed, out var value14))
		{
			backing_Completed = value14;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Completed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalCompleted()
	{
		EmitSignal(SignalName.Completed);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Completed && args.Count == 0)
		{
			backing_Completed?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Completed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
