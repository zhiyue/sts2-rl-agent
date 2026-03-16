using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPlayerHand.cs")]
public class NPlayerHand : Control
{
	[Signal]
	public delegate void ModeChangedEventHandler();

	public enum Mode
	{
		None,
		Play,
		SimpleSelect,
		UpgradeSelect
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName IsAwaitingPlay = "IsAwaitingPlay";

		public static readonly StringName Add = "Add";

		public static readonly StringName AddCardHolder = "AddCardHolder";

		public static readonly StringName RemoveCardHolder = "RemoveCardHolder";

		public static readonly StringName OnHolderFocused = "OnHolderFocused";

		public static readonly StringName OnHolderUnfocused = "OnHolderUnfocused";

		public static readonly StringName CancelAllCardPlay = "CancelAllCardPlay";

		public static readonly StringName ReturnHolderToHand = "ReturnHolderToHand";

		public static readonly StringName ForceRefreshCardIndices = "ForceRefreshCardIndices";

		public static readonly StringName RefreshLayout = "RefreshLayout";

		public static readonly StringName OnPeekButtonToggled = "OnPeekButtonToggled";

		public static readonly StringName UpdateSelectModeCardVisibility = "UpdateSelectModeCardVisibility";

		public static readonly StringName CancelHandSelectionIfNecessary = "CancelHandSelectionIfNecessary";

		public static readonly StringName OnHolderPressed = "OnHolderPressed";

		public static readonly StringName CanPlayCards = "CanPlayCards";

		public static readonly StringName AreCardActionsAllowed = "AreCardActionsAllowed";

		public static readonly StringName StartCardPlay = "StartCardPlay";

		public static readonly StringName SelectCardInSimpleMode = "SelectCardInSimpleMode";

		public static readonly StringName SelectCardInUpgradeMode = "SelectCardInUpgradeMode";

		public static readonly StringName DeselectCard = "DeselectCard";

		public static readonly StringName OnSelectModeConfirmButtonPressed = "OnSelectModeConfirmButtonPressed";

		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";

		public static readonly StringName RefreshSelectModeConfirmButton = "RefreshSelectModeConfirmButton";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName AnimOut = "AnimOut";

		public static readonly StringName AnimDisable = "AnimDisable";

		public static readonly StringName AnimEnable = "AnimEnable";

		public static readonly StringName FlashPlayableHolders = "FlashPlayableHolders";

		public static readonly StringName OnCardSelected = "OnCardSelected";

		public static readonly StringName OnCardDeselected = "OnCardDeselected";

		public static readonly StringName UpdateSelectedCardContainer = "UpdateSelectedCardContainer";

		public static readonly StringName EnableControllerNavigation = "EnableControllerNavigation";

		public static readonly StringName DisableControllerNavigation = "DisableControllerNavigation";

		public new static readonly StringName _UnhandledInput = "_UnhandledInput";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CardHolderContainer = "CardHolderContainer";

		public static readonly StringName PeekButton = "PeekButton";

		public static readonly StringName InCardPlay = "InCardPlay";

		public static readonly StringName IsInCardSelection = "IsInCardSelection";

		public static readonly StringName CurrentMode = "CurrentMode";

		public static readonly StringName HasDraggedHolder = "HasDraggedHolder";

		public static readonly StringName FocusedHolder = "FocusedHolder";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _selectCardShortcuts = "_selectCardShortcuts";

		public static readonly StringName _selectModeBackstop = "_selectModeBackstop";

		public static readonly StringName _upgradePreviewContainer = "_upgradePreviewContainer";

		public static readonly StringName _selectedHandCardContainer = "_selectedHandCardContainer";

		public static readonly StringName _upgradePreview = "_upgradePreview";

		public static readonly StringName _selectModeConfirmButton = "_selectModeConfirmButton";

		public static readonly StringName _selectionHeader = "_selectionHeader";

		public static readonly StringName _currentCardPlay = "_currentCardPlay";

		public static readonly StringName _currentMode = "_currentMode";

		public static readonly StringName _draggedHolderIndex = "_draggedHolderIndex";

		public static readonly StringName _lastFocusedHolderIdx = "_lastFocusedHolderIdx";

		public static readonly StringName _animEnableTween = "_animEnableTween";

		public static readonly StringName _isDisabled = "_isDisabled";

		public static readonly StringName _animInTween = "_animInTween";

		public static readonly StringName _animOutTween = "_animOutTween";

		public static readonly StringName _selectedCardScaleTween = "_selectedCardScaleTween";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName ModeChanged = "ModeChanged";
	}

	private StringName[] _selectCardShortcuts = new StringName[10]
	{
		MegaInput.selectCard1,
		MegaInput.selectCard2,
		MegaInput.selectCard3,
		MegaInput.selectCard4,
		MegaInput.selectCard5,
		MegaInput.selectCard6,
		MegaInput.selectCard7,
		MegaInput.selectCard8,
		MegaInput.selectCard9,
		MegaInput.selectCard10
	};

	private Control _selectModeBackstop;

	private readonly List<CardModel> _selectedCards = new List<CardModel>();

	private CardSelectorPrefs _prefs;

	private TaskCompletionSource<IEnumerable<CardModel>>? _selectionCompletionSource;

	private Control _upgradePreviewContainer;

	private NSelectedHandCardContainer _selectedHandCardContainer;

	private NUpgradePreview _upgradePreview;

	private NConfirmButton _selectModeConfirmButton;

	private MegaRichTextLabel _selectionHeader;

	private NCardPlay? _currentCardPlay;

	private CombatState? _combatState;

	private Mode _currentMode = Mode.Play;

	private Func<CardModel, bool>? _currentSelectionFilter;

	private int _draggedHolderIndex = -1;

	private int _lastFocusedHolderIdx = -1;

	private readonly Dictionary<NHandCardHolder, int> _holdersAwaitingQueue = new Dictionary<NHandCardHolder, int>();

	private Tween? _animEnableTween;

	private bool _isDisabled;

	private const double _enableDisableDuration = 0.2;

	private static readonly Vector2 _disablePosition = new Vector2(0f, 100f);

	private static readonly Color _disableModulate = StsColors.gray;

	private Tween? _animInTween;

	private Tween? _animOutTween;

	private Tween? _selectedCardScaleTween;

	private const float _showHideAnimDuration = 0.8f;

	private static readonly Vector2 _showPosition = Vector2.Zero;

	private static readonly Vector2 _hidePosition = new Vector2(0f, 500f);

	private ModeChangedEventHandler backing_ModeChanged;

	public static NPlayerHand? Instance => NCombatRoom.Instance?.Ui.Hand;

	public Control CardHolderContainer { get; private set; }

	public NPeekButton PeekButton { get; private set; }

	public bool InCardPlay
	{
		get
		{
			if (_currentCardPlay != null)
			{
				return GodotObject.IsInstanceValid(_currentCardPlay);
			}
			return false;
		}
	}

	public bool IsInCardSelection
	{
		get
		{
			Mode currentMode = CurrentMode;
			if ((uint)(currentMode - 2) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	public Mode CurrentMode
	{
		get
		{
			return _currentMode;
		}
		private set
		{
			_currentMode = value;
			EmitSignal(SignalName.ModeChanged);
		}
	}

	private bool HasDraggedHolder => _draggedHolderIndex >= 0;

	public Func<CardModel, bool>? SelectModeGoldGlowOverride => _prefs.ShouldGlowGold;

	public NHandCardHolder? FocusedHolder { get; private set; }

	public IReadOnlyList<NHandCardHolder> ActiveHolders => Holders.Where((NHandCardHolder child) => child.Visible).ToList();

	private IReadOnlyList<NHandCardHolder> Holders => CardHolderContainer.GetChildren().OfType<NHandCardHolder>().ToList();

	public Control DefaultFocusedControl
	{
		get
		{
			if (ActiveHolders.Count > 0)
			{
				if (_lastFocusedHolderIdx >= 0)
				{
					return ActiveHolders[Mathf.Clamp(_lastFocusedHolderIdx, 0, ActiveHolders.Count - 1)];
				}
				return ActiveHolders[ActiveHolders.Count / 2];
			}
			return CardHolderContainer;
		}
	}

	public event ModeChangedEventHandler ModeChanged
	{
		add
		{
			backing_ModeChanged = (ModeChangedEventHandler)Delegate.Combine(backing_ModeChanged, value);
		}
		remove
		{
			backing_ModeChanged = (ModeChangedEventHandler)Delegate.Remove(backing_ModeChanged, value);
		}
	}

	public override void _Ready()
	{
		_selectModeBackstop = GetNode<Control>("%SelectModeBackstop");
		CardHolderContainer = GetNode<Control>("%CardHolderContainer");
		_upgradePreviewContainer = GetNode<Control>("%UpgradePreviewContainer");
		_selectModeConfirmButton = GetNode<NConfirmButton>("%SelectModeConfirmButton");
		_upgradePreview = GetNode<NUpgradePreview>("%UpgradePreview");
		_selectionHeader = GetNode<MegaRichTextLabel>("%SelectionHeader");
		_selectionHeader.Visible = false;
		_selectedHandCardContainer = GetNode<NSelectedHandCardContainer>("%SelectedHandCardContainer");
		_selectedHandCardContainer.Hand = this;
		_selectModeConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSelectModeConfirmButtonPressed));
		_selectModeConfirmButton.Disable();
		_selectedHandCardContainer.Connect(Node.SignalName.ChildExitingTree, Callable.From<Node>(OnCardDeselected));
		_selectedHandCardContainer.Connect(Node.SignalName.ChildEnteredTree, Callable.From<Node>(OnCardSelected));
		PeekButton = GetNode<NPeekButton>("%PeekButton");
		PeekButton.Disable();
		PeekButton.AddTargets(_selectModeBackstop, _upgradePreviewContainer, _selectModeConfirmButton, _selectionHeader, _selectedHandCardContainer);
		PeekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(OnPeekButtonToggled));
		CardHolderContainer.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			if (ActiveHolders.Count > 0)
			{
				DefaultFocusedControl.TryGrabFocus();
			}
		}));
		CardHolderContainer.FocusNeighborBottom = CardHolderContainer.GetPath();
		CardHolderContainer.FocusNeighborLeft = CardHolderContainer.GetPath();
		CardHolderContainer.FocusNeighborRight = CardHolderContainer.GetPath();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.PlayerActionsDisabledChanged += OnPlayerActionsDisabledChanged;
		CombatManager.Instance.PlayerUnendedTurn += OnPlayerUnendedTurn;
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		CombatManager.Instance.CombatEnded += OnCombatEnded;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		TaskCompletionSource<IEnumerable<CardModel>> selectionCompletionSource = _selectionCompletionSource;
		if (selectionCompletionSource != null)
		{
			Task<IEnumerable<CardModel>> task = selectionCompletionSource.Task;
			if (task != null && !task.IsCompleted)
			{
				_selectionCompletionSource.SetResult(Array.Empty<CardModel>());
			}
		}
		CombatManager.Instance.PlayerActionsDisabledChanged -= OnPlayerActionsDisabledChanged;
		CombatManager.Instance.PlayerUnendedTurn -= OnPlayerUnendedTurn;
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
	}

	public NCard? GetCard(CardModel card)
	{
		return GetCardHolder(card)?.CardNode;
	}

	public bool IsAwaitingPlay(NHandCardHolder? holder)
	{
		if (holder != null)
		{
			return _holdersAwaitingQueue.ContainsKey(holder);
		}
		return false;
	}

	public NCardHolder? GetCardHolder(CardModel card)
	{
		return ((IEnumerable<NCardHolder>)Holders).Concat((IEnumerable<NCardHolder>)_selectedHandCardContainer.Holders).Concat(_holdersAwaitingQueue.Keys).FirstOrDefault((NCardHolder h) => h.CardNode != null && h.CardNode.Model == card);
	}

	public NHandCardHolder Add(NCard card, int index = -1)
	{
		Vector2 globalPosition = card.GlobalPosition;
		NHandCardHolder nHandCardHolder = NHandCardHolder.Create(card, this);
		AddCardHolder(nHandCardHolder, index);
		nHandCardHolder.GlobalPosition = globalPosition;
		RefreshLayout();
		return nHandCardHolder;
	}

	public void Remove(CardModel card)
	{
		NCardHolder cardHolder = GetCardHolder(card);
		if (cardHolder == null)
		{
			throw new InvalidOperationException($"No holder for card {card.Id}");
		}
		if (InCardPlay && card == _currentCardPlay.Holder.CardModel)
		{
			_currentCardPlay.CancelPlayCard();
		}
		RemoveCardHolder(cardHolder);
	}

	private void AddCardHolder(NHandCardHolder holder, int index)
	{
		CardHolderContainer.AddChildSafely(holder);
		if (index >= 0)
		{
			CardHolderContainer.MoveChild(holder, index);
		}
		holder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(OnHolderPressed));
		holder.Connect(NHandCardHolder.SignalName.HolderMouseClicked, Callable.From<NCardHolder>(OnHolderPressed));
		holder.Connect(NHandCardHolder.SignalName.HolderFocused, Callable.From<NHandCardHolder>(OnHolderFocused));
		holder.Connect(NHandCardHolder.SignalName.HolderUnfocused, Callable.From<NHandCardHolder>(OnHolderUnfocused));
		RefreshLayout();
		if (CardHolderContainer.HasFocus())
		{
			holder.TryGrabFocus();
		}
	}

	public void RemoveCardHolder(NCardHolder holder)
	{
		if (holder is NHandCardHolder key)
		{
			_holdersAwaitingQueue.Remove(key);
		}
		if (InCardPlay && _currentCardPlay.Holder == holder)
		{
			_currentCardPlay.CancelPlayCard();
		}
		bool flag = holder.HasFocus();
		holder.Clear();
		holder.GetParent().RemoveChildSafely(holder);
		holder.QueueFreeSafely();
		RefreshLayout();
		if (flag)
		{
			DefaultFocusedControl.TryGrabFocus();
		}
	}

	private void OnHolderFocused(NHandCardHolder holder)
	{
		FocusedHolder = holder;
		_lastFocusedHolderIdx = holder.GetIndex();
		RunManager.Instance.HoveredModelTracker.OnLocalCardHovered(FocusedHolder.CardModel);
		RefreshLayout();
	}

	private void OnHolderUnfocused(NHandCardHolder holder)
	{
		FocusedHolder = null;
		RunManager.Instance.HoveredModelTracker.OnLocalCardUnhovered();
		RefreshLayout();
	}

	public void TryCancelCardPlay(CardModel card)
	{
		NCardHolder cardHolder = GetCardHolder(card);
		if (cardHolder is NHandCardHolder nHandCardHolder && IsAwaitingPlay(nHandCardHolder))
		{
			ReturnHolderToHand(nHandCardHolder);
			nHandCardHolder.UpdateCard();
			if (InCardPlay && _currentCardPlay.Holder == nHandCardHolder)
			{
				_currentCardPlay.CancelPlayCard();
			}
			else
			{
				RefreshLayout();
			}
		}
	}

	public void CancelAllCardPlay()
	{
		if (InCardPlay)
		{
			_currentCardPlay.CancelPlayCard();
		}
		foreach (NHandCardHolder item in _holdersAwaitingQueue.Keys.ToList())
		{
			ReturnHolderToHand(item);
		}
	}

	private void ReturnHolderToHand(NHandCardHolder holder)
	{
		if (IsAwaitingPlay(holder))
		{
			int num = _holdersAwaitingQueue[holder];
			_holdersAwaitingQueue.Remove(holder);
			holder.Reparent(CardHolderContainer);
			if (num >= 0)
			{
				CardHolderContainer.MoveChild(holder, num);
			}
			holder.SetDefaultTargets();
		}
	}

	public void ForceRefreshCardIndices()
	{
		RefreshLayout();
	}

	private void RefreshLayout()
	{
		int count = ActiveHolders.Count;
		if (count <= 0)
		{
			return;
		}
		int handSize = count;
		Vector2 scale = HandPosHelper.GetScale(count);
		int num = -1;
		if (FocusedHolder != null)
		{
			num = ActiveHolders.IndexOf<NHandCardHolder>(FocusedHolder);
		}
		for (int i = 0; i < count; i++)
		{
			int cardIndex = i;
			Vector2 position = HandPosHelper.GetPosition(handSize, cardIndex);
			if (num > -1)
			{
				float num2 = Mathf.Lerp(100f, 0f, Mathf.Min(1f, (float)Mathf.Abs(num - i) / 4f));
				position += Vector2.Left * Mathf.Sign(num - i) * num2;
			}
			NHandCardHolder nHandCardHolder = ActiveHolders[i];
			if (num == i)
			{
				nHandCardHolder.SetAngleInstantly(0f);
				nHandCardHolder.SetScaleInstantly(Vector2.One);
				position.Y = (0f - nHandCardHolder.Hitbox.Size.Y) * 0.5f + 2f;
				if (_isDisabled)
				{
					position -= _disablePosition;
				}
				nHandCardHolder.Position = new Vector2(nHandCardHolder.Position.X, position.Y);
				nHandCardHolder.SetTargetPosition(position);
			}
			else
			{
				nHandCardHolder.SetTargetPosition(position);
				nHandCardHolder.SetTargetScale(scale);
				nHandCardHolder.SetTargetAngle(HandPosHelper.GetAngle(handSize, cardIndex));
			}
			nHandCardHolder.Hitbox.MouseFilter = (MouseFilterEnum)(HasDraggedHolder ? 2 : 0);
			NodePath path;
			if (i <= 0)
			{
				IReadOnlyList<NHandCardHolder> activeHolders = ActiveHolders;
				path = activeHolders[activeHolders.Count - 1].GetPath();
			}
			else
			{
				path = ActiveHolders[i - 1].GetPath();
			}
			nHandCardHolder.FocusNeighborLeft = path;
			nHandCardHolder.FocusNeighborRight = ((i < ActiveHolders.Count - 1) ? ActiveHolders[i + 1].GetPath() : ActiveHolders[0].GetPath());
			nHandCardHolder.FocusNeighborBottom = nHandCardHolder.GetPath();
			if (HasDraggedHolder && i >= _draggedHolderIndex)
			{
				nHandCardHolder.SetIndexLabel(i + 2);
			}
			else
			{
				nHandCardHolder.SetIndexLabel(i + 1);
			}
		}
	}

	private void OnPlayerUnendedTurn(Player player)
	{
		UpdateHandDisabledState(player.Creature.CombatState);
	}

	private void OnPlayerActionsDisabledChanged(CombatState state)
	{
		UpdateHandDisabledState(state);
	}

	private void UpdateHandDisabledState(CombatState state)
	{
		Player me = LocalContext.GetMe(state);
		bool flag = CombatManager.Instance.PlayerActionsDisabled;
		if (!flag && CombatManager.Instance.PlayersTakingExtraTurn.Count > 0 && me != null && !CombatManager.Instance.PlayersTakingExtraTurn.Contains(me))
		{
			flag = true;
		}
		if (flag)
		{
			if (me == null || !state.Players.Except(new global::_003C_003Ez__ReadOnlySingleElementList<Player>(me)).All(CombatManager.Instance.IsPlayerReadyToEndTurn))
			{
				AnimDisable();
			}
		}
		else
		{
			AnimEnable();
		}
	}

	private void OnCombatStateChanged(CombatState state)
	{
		_combatState = state;
		foreach (NHandCardHolder holder in Holders)
		{
			holder.UpdateCard();
		}
		foreach (NHandCardHolder key in _holdersAwaitingQueue.Keys)
		{
			key.UpdateCard();
		}
		foreach (NSelectedHandCardHolder holder2 in _selectedHandCardContainer.Holders)
		{
			holder2.CardNode?.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
		}
		UpdateHandDisabledState(state);
	}

	private void OnCombatEnded(CombatRoom _)
	{
		CancelAllCardPlay();
		CardHolderContainer.FocusMode = FocusModeEnum.None;
	}

	private void OnPeekButtonToggled(NPeekButton button)
	{
		if (button.IsPeeking)
		{
			NCombatRoom.Instance.EnableControllerNavigation();
		}
		else
		{
			NCombatRoom.Instance.RestrictControllerNavigation(Array.Empty<Control>());
			EnableControllerNavigation();
		}
		UpdateSelectModeCardVisibility();
		ActiveScreenContext.Instance.Update();
	}

	public async Task<IEnumerable<CardModel>> SelectCards(CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel? source, Mode mode = Mode.SimpleSelect)
	{
		CancelAllCardPlay();
		_selectModeBackstop.Visible = true;
		_selectModeBackstop.MouseFilter = MouseFilterEnum.Stop;
		Control selectModeBackstop = _selectModeBackstop;
		Color selfModulate = _selectModeBackstop.SelfModulate;
		selfModulate.A = 0f;
		selectModeBackstop.SelfModulate = selfModulate;
		Tween tween = CreateTween();
		tween.TweenProperty(_selectModeBackstop, "self_modulate:a", 1f, 0.20000000298023224);
		bool wasDisabled = _isDisabled;
		if (_isDisabled)
		{
			AnimEnable();
		}
		CurrentMode = mode;
		_currentSelectionFilter = filter;
		NCombatRoom.Instance.RestrictControllerNavigation(Array.Empty<Control>());
		NCombatUi ui = NCombatRoom.Instance.Ui;
		ui.OnHandSelectModeEntered();
		EnableControllerNavigation();
		_prefs = prefs;
		_selectionCompletionSource = new TaskCompletionSource<IEnumerable<CardModel>>();
		_selectionHeader.Visible = true;
		_selectionHeader.Text = "[center]" + prefs.Prompt.GetFormattedText() + "[/center]";
		PeekButton.Enable();
		UpdateSelectModeCardVisibility();
		RefreshSelectModeConfirmButton();
		IEnumerable<CardModel> result = await _selectionCompletionSource.Task;
		tween.Kill();
		AfterCardsSelected(source);
		if (wasDisabled)
		{
			AnimDisable();
		}
		return result;
	}

	private void UpdateSelectModeCardVisibility()
	{
		if (CurrentMode != Mode.SimpleSelect && CurrentMode != Mode.UpgradeSelect)
		{
			throw new InvalidOperationException("Can only be used when we are selecting a card");
		}
		foreach (NHandCardHolder holder in Holders)
		{
			if (holder.CardNode != null)
			{
				if (PeekButton.IsPeeking)
				{
					holder.Visible = true;
					holder.CardNode.SetPretendCardCanBePlayed(pretendCardCanBePlayed: false);
					holder.CardNode.SetForceUnpoweredPreview(forceUnpoweredPreview: false);
				}
				else
				{
					holder.Visible = _currentSelectionFilter?.Invoke(holder.CardNode.Model) ?? true;
					holder.CardNode.SetPretendCardCanBePlayed(_prefs.PretendCardsCanBePlayed);
					holder.CardNode.SetForceUnpoweredPreview(_prefs.UnpoweredPreviews);
				}
				holder.UpdateCard();
			}
		}
		RefreshLayout();
	}

	private void AfterCardsSelected(AbstractModel? source)
	{
		_selectedCards.Clear();
		foreach (NHandCardHolder holder in Holders)
		{
			holder.InSelectMode = false;
			holder.Visible = true;
			holder.CardNode?.SetPretendCardCanBePlayed(pretendCardCanBePlayed: false);
			holder.CardNode?.SetForceUnpoweredPreview(forceUnpoweredPreview: false);
			holder.UpdateCard();
		}
		RefreshLayout();
		_selectModeBackstop.Visible = false;
		_selectModeBackstop.MouseFilter = MouseFilterEnum.Ignore;
		Tween tween = CreateTween();
		tween.TweenProperty(_selectModeBackstop, "self_modulate:a", 0f, 0.20000000298023224);
		_selectModeConfirmButton.Disable();
		_upgradePreviewContainer.Visible = false;
		_selectionHeader.Visible = false;
		PeekButton.Disable();
		_prefs = default(CardSelectorPrefs);
		CurrentMode = Mode.Play;
		_currentSelectionFilter = null;
		NCombatRoom.Instance.Ui.OnHandSelectModeExited();
		if (source != null)
		{
			source.ExecutionFinished += OnSelectModeSourceFinished;
		}
		else
		{
			OnSelectModeSourceFinished(null);
		}
	}

	private void CancelHandSelectionIfNecessary()
	{
		if (IsInCardSelection && _selectionCompletionSource != null)
		{
			_selectionCompletionSource.SetCanceled();
			AfterCardsSelected(null);
		}
	}

	private void OnHolderPressed(NCardHolder holder)
	{
		if (PeekButton.IsPeeking)
		{
			PeekButton.Wiggle();
			return;
		}
		NHandCardHolder nHandCardHolder = (NHandCardHolder)holder;
		if (nHandCardHolder.CardNode == null || !CombatManager.Instance.IsInProgress || NOverlayStack.Instance.ScreenCount > 0)
		{
			return;
		}
		switch (CurrentMode)
		{
		case Mode.Play:
			if (CanPlayCards())
			{
				StartCardPlay(nHandCardHolder, startedViaShortcut: false);
			}
			break;
		case Mode.SimpleSelect:
			SelectCardInSimpleMode(nHandCardHolder);
			break;
		case Mode.UpgradeSelect:
			SelectCardInUpgradeMode(nHandCardHolder);
			break;
		default:
			throw new ArgumentOutOfRangeException("CurrentMode");
		case Mode.None:
			break;
		}
	}

	private bool CanPlayCards()
	{
		if (!InCardPlay)
		{
			return AreCardActionsAllowed();
		}
		return false;
	}

	private bool AreCardActionsAllowed()
	{
		if (CombatManager.Instance.PlayersTakingExtraTurn.Count > 0 && _combatState != null)
		{
			Player me = LocalContext.GetMe(_combatState);
			if (me == null || !CombatManager.Instance.PlayersTakingExtraTurn.Contains(me))
			{
				return false;
			}
		}
		if (!CombatManager.Instance.PlayerActionsDisabled)
		{
			return !PeekButton.IsPeeking;
		}
		return false;
	}

	private void StartCardPlay(NHandCardHolder holder, bool startedViaShortcut)
	{
		_draggedHolderIndex = holder.GetIndex();
		_holdersAwaitingQueue.Add(holder, _draggedHolderIndex);
		holder.Reparent(this);
		holder.BeginDrag();
		_currentCardPlay = (NControllerManager.Instance.IsUsingController ? ((NCardPlay)NControllerCardPlay.Create(holder)) : ((NCardPlay)NMouseCardPlay.Create(holder, _selectCardShortcuts[_draggedHolderIndex], startedViaShortcut)));
		this.AddChildSafely(_currentCardPlay);
		_currentCardPlay.Connect(NCardPlay.SignalName.Finished, Callable.From(delegate(bool success)
		{
			RunManager.Instance.HoveredModelTracker.OnLocalCardDeselected();
			if (!success)
			{
				ReturnHolderToHand(holder);
			}
			_draggedHolderIndex = -1;
			RefreshLayout();
		}));
		RunManager.Instance.HoveredModelTracker.OnLocalCardSelected(holder.CardNode.Model);
		_currentCardPlay.Start();
		RefreshLayout();
		holder.SetIndexLabel(_draggedHolderIndex + 1);
	}

	private void SelectCardInSimpleMode(NHandCardHolder holder)
	{
		if (_selectedCards.Count >= _prefs.MaxSelect)
		{
			_selectedHandCardContainer.DeselectCard(_selectedCards.Last());
		}
		_selectedCards.Add(holder.CardNode.Model);
		_selectedHandCardContainer.Add(holder);
		RemoveCardHolder(holder);
		RefreshSelectModeConfirmButton();
	}

	private void SelectCardInUpgradeMode(NHandCardHolder holder)
	{
		CardModel model = holder.CardNode.Model;
		if (_selectedCards.Count != 0)
		{
			NCard nCard = NCard.Create(_selectedCards.Last());
			nCard.GlobalPosition = _upgradePreview.SelectedCardPosition;
			DeselectCard(nCard);
		}
		_selectedCards.Add(model);
		_upgradePreviewContainer.Visible = true;
		_upgradePreview.Card = model;
		RemoveCardHolder(holder);
		RefreshSelectModeConfirmButton();
	}

	public void DeselectCard(NCard card)
	{
		if (!IsInCardSelection)
		{
			throw new InvalidOperationException("Only valid when in Select Mode.");
		}
		NHandCardHolder nHandCardHolder = Add(card, PileType.Hand.GetPile(card.Model.Owner).Cards.IndexOf<CardModel>(card.Model));
		nHandCardHolder.InSelectMode = true;
		nHandCardHolder.Visible = true;
		_selectedCards.Remove(card.Model);
		RefreshSelectModeConfirmButton();
		nHandCardHolder.TryGrabFocus();
	}

	private void OnSelectModeConfirmButtonPressed(NButton _)
	{
		_selectionCompletionSource.SetResult(_selectedCards.ToList());
	}

	private void CheckIfSelectionComplete()
	{
		if (_selectedCards.Count >= _prefs.MaxSelect)
		{
			_selectionCompletionSource.SetResult(_selectedCards.ToList());
		}
	}

	private void RefreshSelectModeConfirmButton()
	{
		int count = _selectedCards.Count;
		if (count >= _prefs.MinSelect && count <= _prefs.MaxSelect)
		{
			_selectModeConfirmButton.Enable();
		}
		else
		{
			_selectModeConfirmButton.Disable();
		}
	}

	private void OnSelectModeSourceFinished(AbstractModel? source)
	{
		foreach (NSelectedHandCardHolder item in _selectedHandCardContainer.Holders.ToList())
		{
			NCard cardNode = item.CardNode;
			item.QueueFreeSafely();
			Add(cardNode);
		}
		if (_upgradePreview.Card != null)
		{
			Add(NCard.Create(_upgradePreview.Card));
			_upgradePreview.Card = null;
		}
		if (source != null)
		{
			source.ExecutionFinished -= OnSelectModeSourceFinished;
		}
	}

	public void AnimIn()
	{
		_animOutTween?.Kill();
		_animEnableTween?.Kill();
		_animInTween = CreateTween();
		_animInTween.TweenProperty(this, "position", _showPosition, 0.800000011920929).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void AnimOut()
	{
		CancelHandSelectionIfNecessary();
		_animInTween?.Kill();
		_animEnableTween?.Kill();
		_animOutTween = CreateTween();
		_animOutTween.TweenProperty(this, "position", _hidePosition, 0.800000011920929).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
	}

	private void AnimDisable()
	{
		if (!_isDisabled)
		{
			DisableControllerNavigation();
			_animEnableTween = CreateTween().SetParallel();
			_animEnableTween.TweenProperty(this, "position", _disablePosition, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_animEnableTween.TweenProperty(this, "modulate", _disableModulate, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_isDisabled = true;
		}
	}

	private void AnimEnable()
	{
		if (_isDisabled)
		{
			EnableControllerNavigation();
			DefaultFocusedControl.TryGrabFocus();
			_animEnableTween = CreateTween().SetParallel();
			_animEnableTween.TweenProperty(this, "position", _showPosition, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_animEnableTween.TweenProperty(this, "modulate", Colors.White, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_isDisabled = false;
		}
	}

	public void FlashPlayableHolders()
	{
		foreach (NHandCardHolder holder in Holders)
		{
			if (holder.CardNode != null && holder.CardNode.Model.CanPlay())
			{
				holder.Flash();
			}
		}
	}

	private void OnCardSelected(Node _)
	{
		UpdateSelectedCardContainer(_selectedHandCardContainer.GetChildCount());
	}

	private void OnCardDeselected(Node _)
	{
		UpdateSelectedCardContainer(_selectedHandCardContainer.GetChildCount() - 1);
	}

	private void UpdateSelectedCardContainer(int count)
	{
		float num = 1f;
		float num2 = base.Size.Y * 0.5f;
		if (count > 6)
		{
			num = 0.55f;
			num2 -= 150f;
		}
		else if (count > 3)
		{
			num = 0.8f;
			num2 -= 75f;
		}
		_selectedCardScaleTween?.Kill();
		_selectedCardScaleTween = CreateTween().SetParallel();
		_selectedCardScaleTween.TweenProperty(_selectedHandCardContainer, "position:y", num2, 0.5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
		_selectedCardScaleTween.TweenProperty(_selectedHandCardContainer, "scale", Vector2.One * num, 0.5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
	}

	public void EnableControllerNavigation()
	{
		foreach (NHandCardHolder holder in Holders)
		{
			holder.FocusMode = FocusModeEnum.All;
		}
		if (InCardPlay)
		{
			_currentCardPlay.Holder.FocusMode = FocusModeEnum.All;
		}
	}

	public void DisableControllerNavigation()
	{
		foreach (NHandCardHolder holder in Holders)
		{
			holder.FocusMode = FocusModeEnum.None;
		}
		if (InCardPlay)
		{
			_currentCardPlay.Holder.FocusMode = FocusModeEnum.None;
		}
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if (NControllerManager.Instance.IsUsingController || !ActiveScreenContext.Instance.IsCurrent(NCombatRoom.Instance) || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		List<NHandCardHolder> list = new List<NHandCardHolder>();
		list.AddRange(ActiveHolders);
		if (HasDraggedHolder)
		{
			list.Insert(_draggedHolderIndex, null);
		}
		for (int i = 0; i < _selectCardShortcuts.Length; i++)
		{
			StringName action = _selectCardShortcuts[i];
			if (!input.IsActionPressed(action) || list.Count <= i)
			{
				continue;
			}
			NHandCardHolder nHandCardHolder = list[i];
			if (nHandCardHolder == null)
			{
				continue;
			}
			if (NTargetManager.Instance.IsInSelection)
			{
				NTargetManager.Instance.CancelTargeting();
			}
			switch (CurrentMode)
			{
			case Mode.Play:
				if (AreCardActionsAllowed())
				{
					if (InCardPlay)
					{
						_currentCardPlay.CancelPlayCard();
					}
					StartCardPlay(nHandCardHolder, startedViaShortcut: true);
				}
				break;
			case Mode.SimpleSelect:
				if (!PeekButton.IsPeeking)
				{
					SelectCardInSimpleMode(nHandCardHolder);
				}
				break;
			case Mode.UpgradeSelect:
				if (!PeekButton.IsPeeking)
				{
					SelectCardInUpgradeMode(nHandCardHolder);
				}
				break;
			}
			GetViewport()?.SetInputAsHandled();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(37);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsAwaitingPlay, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Add, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddCardHolder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveCardHolder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHolderFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHolderUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CancelAllCardPlay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReturnHolderToHand, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ForceRefreshCardIndices, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPeekButtonToggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateSelectModeCardVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelHandSelectionIfNecessary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHolderPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CanPlayCards, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AreCardActionsAllowed, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartCardPlay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "startedViaShortcut", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectCardInSimpleMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectCardInUpgradeMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DeselectCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSelectModeConfirmButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckIfSelectionComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshSelectModeConfirmButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FlashPlayableHolders, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCardDeselected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateSelectedCardContainer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "count", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnableControllerNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableControllerNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsAwaitingPlay && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsAwaitingPlay(VariantUtils.ConvertTo<NHandCardHolder>(in args[0])));
			return true;
		}
		if (method == MethodName.Add && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NHandCardHolder>(Add(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.AddCardHolder && args.Count == 2)
		{
			AddCardHolder(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveCardHolder && args.Count == 1)
		{
			RemoveCardHolder(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHolderFocused && args.Count == 1)
		{
			OnHolderFocused(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHolderUnfocused && args.Count == 1)
		{
			OnHolderUnfocused(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelAllCardPlay && args.Count == 0)
		{
			CancelAllCardPlay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReturnHolderToHand && args.Count == 1)
		{
			ReturnHolderToHand(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceRefreshCardIndices && args.Count == 0)
		{
			ForceRefreshCardIndices();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshLayout && args.Count == 0)
		{
			RefreshLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPeekButtonToggled && args.Count == 1)
		{
			OnPeekButtonToggled(VariantUtils.ConvertTo<NPeekButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSelectModeCardVisibility && args.Count == 0)
		{
			UpdateSelectModeCardVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelHandSelectionIfNecessary && args.Count == 0)
		{
			CancelHandSelectionIfNecessary();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHolderPressed && args.Count == 1)
		{
			OnHolderPressed(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanPlayCards && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanPlayCards());
			return true;
		}
		if (method == MethodName.AreCardActionsAllowed && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(AreCardActionsAllowed());
			return true;
		}
		if (method == MethodName.StartCardPlay && args.Count == 2)
		{
			StartCardPlay(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectCardInSimpleMode && args.Count == 1)
		{
			SelectCardInSimpleMode(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectCardInUpgradeMode && args.Count == 1)
		{
			SelectCardInUpgradeMode(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DeselectCard && args.Count == 1)
		{
			DeselectCard(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelectModeConfirmButtonPressed && args.Count == 1)
		{
			OnSelectModeConfirmButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete && args.Count == 0)
		{
			CheckIfSelectionComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshSelectModeConfirmButton && args.Count == 0)
		{
			RefreshSelectModeConfirmButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimDisable && args.Count == 0)
		{
			AnimDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimEnable && args.Count == 0)
		{
			AnimEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashPlayableHolders && args.Count == 0)
		{
			FlashPlayableHolders();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardSelected && args.Count == 1)
		{
			OnCardSelected(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardDeselected && args.Count == 1)
		{
			OnCardDeselected(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSelectedCardContainer && args.Count == 1)
		{
			UpdateSelectedCardContainer(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableControllerNavigation && args.Count == 0)
		{
			EnableControllerNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableControllerNavigation && args.Count == 0)
		{
			DisableControllerNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.IsAwaitingPlay)
		{
			return true;
		}
		if (method == MethodName.Add)
		{
			return true;
		}
		if (method == MethodName.AddCardHolder)
		{
			return true;
		}
		if (method == MethodName.RemoveCardHolder)
		{
			return true;
		}
		if (method == MethodName.OnHolderFocused)
		{
			return true;
		}
		if (method == MethodName.OnHolderUnfocused)
		{
			return true;
		}
		if (method == MethodName.CancelAllCardPlay)
		{
			return true;
		}
		if (method == MethodName.ReturnHolderToHand)
		{
			return true;
		}
		if (method == MethodName.ForceRefreshCardIndices)
		{
			return true;
		}
		if (method == MethodName.RefreshLayout)
		{
			return true;
		}
		if (method == MethodName.OnPeekButtonToggled)
		{
			return true;
		}
		if (method == MethodName.UpdateSelectModeCardVisibility)
		{
			return true;
		}
		if (method == MethodName.CancelHandSelectionIfNecessary)
		{
			return true;
		}
		if (method == MethodName.OnHolderPressed)
		{
			return true;
		}
		if (method == MethodName.CanPlayCards)
		{
			return true;
		}
		if (method == MethodName.AreCardActionsAllowed)
		{
			return true;
		}
		if (method == MethodName.StartCardPlay)
		{
			return true;
		}
		if (method == MethodName.SelectCardInSimpleMode)
		{
			return true;
		}
		if (method == MethodName.SelectCardInUpgradeMode)
		{
			return true;
		}
		if (method == MethodName.DeselectCard)
		{
			return true;
		}
		if (method == MethodName.OnSelectModeConfirmButtonPressed)
		{
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete)
		{
			return true;
		}
		if (method == MethodName.RefreshSelectModeConfirmButton)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.AnimDisable)
		{
			return true;
		}
		if (method == MethodName.AnimEnable)
		{
			return true;
		}
		if (method == MethodName.FlashPlayableHolders)
		{
			return true;
		}
		if (method == MethodName.OnCardSelected)
		{
			return true;
		}
		if (method == MethodName.OnCardDeselected)
		{
			return true;
		}
		if (method == MethodName.UpdateSelectedCardContainer)
		{
			return true;
		}
		if (method == MethodName.EnableControllerNavigation)
		{
			return true;
		}
		if (method == MethodName.DisableControllerNavigation)
		{
			return true;
		}
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CardHolderContainer)
		{
			CardHolderContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.PeekButton)
		{
			PeekButton = VariantUtils.ConvertTo<NPeekButton>(in value);
			return true;
		}
		if (name == PropertyName.CurrentMode)
		{
			CurrentMode = VariantUtils.ConvertTo<Mode>(in value);
			return true;
		}
		if (name == PropertyName.FocusedHolder)
		{
			FocusedHolder = VariantUtils.ConvertTo<NHandCardHolder>(in value);
			return true;
		}
		if (name == PropertyName._selectCardShortcuts)
		{
			_selectCardShortcuts = VariantUtils.ConvertTo<StringName[]>(in value);
			return true;
		}
		if (name == PropertyName._selectModeBackstop)
		{
			_selectModeBackstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._upgradePreviewContainer)
		{
			_upgradePreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._selectedHandCardContainer)
		{
			_selectedHandCardContainer = VariantUtils.ConvertTo<NSelectedHandCardContainer>(in value);
			return true;
		}
		if (name == PropertyName._upgradePreview)
		{
			_upgradePreview = VariantUtils.ConvertTo<NUpgradePreview>(in value);
			return true;
		}
		if (name == PropertyName._selectModeConfirmButton)
		{
			_selectModeConfirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._selectionHeader)
		{
			_selectionHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentCardPlay)
		{
			_currentCardPlay = VariantUtils.ConvertTo<NCardPlay>(in value);
			return true;
		}
		if (name == PropertyName._currentMode)
		{
			_currentMode = VariantUtils.ConvertTo<Mode>(in value);
			return true;
		}
		if (name == PropertyName._draggedHolderIndex)
		{
			_draggedHolderIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._lastFocusedHolderIdx)
		{
			_lastFocusedHolderIdx = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._animEnableTween)
		{
			_animEnableTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isDisabled)
		{
			_isDisabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			_animInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._animOutTween)
		{
			_animOutTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._selectedCardScaleTween)
		{
			_selectedCardScaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.CardHolderContainer)
		{
			from = CardHolderContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.PeekButton)
		{
			value = VariantUtils.CreateFrom<NPeekButton>(PeekButton);
			return true;
		}
		bool from2;
		if (name == PropertyName.InCardPlay)
		{
			from2 = InCardPlay;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.IsInCardSelection)
		{
			from2 = IsInCardSelection;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.CurrentMode)
		{
			value = VariantUtils.CreateFrom<Mode>(CurrentMode);
			return true;
		}
		if (name == PropertyName.HasDraggedHolder)
		{
			from2 = HasDraggedHolder;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.FocusedHolder)
		{
			value = VariantUtils.CreateFrom<NHandCardHolder>(FocusedHolder);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._selectCardShortcuts)
		{
			value = VariantUtils.CreateFrom(in _selectCardShortcuts);
			return true;
		}
		if (name == PropertyName._selectModeBackstop)
		{
			value = VariantUtils.CreateFrom(in _selectModeBackstop);
			return true;
		}
		if (name == PropertyName._upgradePreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _upgradePreviewContainer);
			return true;
		}
		if (name == PropertyName._selectedHandCardContainer)
		{
			value = VariantUtils.CreateFrom(in _selectedHandCardContainer);
			return true;
		}
		if (name == PropertyName._upgradePreview)
		{
			value = VariantUtils.CreateFrom(in _upgradePreview);
			return true;
		}
		if (name == PropertyName._selectModeConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _selectModeConfirmButton);
			return true;
		}
		if (name == PropertyName._selectionHeader)
		{
			value = VariantUtils.CreateFrom(in _selectionHeader);
			return true;
		}
		if (name == PropertyName._currentCardPlay)
		{
			value = VariantUtils.CreateFrom(in _currentCardPlay);
			return true;
		}
		if (name == PropertyName._currentMode)
		{
			value = VariantUtils.CreateFrom(in _currentMode);
			return true;
		}
		if (name == PropertyName._draggedHolderIndex)
		{
			value = VariantUtils.CreateFrom(in _draggedHolderIndex);
			return true;
		}
		if (name == PropertyName._lastFocusedHolderIdx)
		{
			value = VariantUtils.CreateFrom(in _lastFocusedHolderIdx);
			return true;
		}
		if (name == PropertyName._animEnableTween)
		{
			value = VariantUtils.CreateFrom(in _animEnableTween);
			return true;
		}
		if (name == PropertyName._isDisabled)
		{
			value = VariantUtils.CreateFrom(in _isDisabled);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			value = VariantUtils.CreateFrom(in _animInTween);
			return true;
		}
		if (name == PropertyName._animOutTween)
		{
			value = VariantUtils.CreateFrom(in _animOutTween);
			return true;
		}
		if (name == PropertyName._selectedCardScaleTween)
		{
			value = VariantUtils.CreateFrom(in _selectedCardScaleTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._selectCardShortcuts, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardHolderContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PeekButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectModeBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upgradePreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedHandCardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upgradePreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectModeConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentCardPlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InCardPlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsInCardSelection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.CurrentMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._draggedHolderIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasDraggedHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastFocusedHolderIdx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animEnableTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDisabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animOutTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedCardScaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CardHolderContainer, Variant.From<Control>(CardHolderContainer));
		info.AddProperty(PropertyName.PeekButton, Variant.From<NPeekButton>(PeekButton));
		info.AddProperty(PropertyName.CurrentMode, Variant.From<Mode>(CurrentMode));
		info.AddProperty(PropertyName.FocusedHolder, Variant.From<NHandCardHolder>(FocusedHolder));
		info.AddProperty(PropertyName._selectCardShortcuts, Variant.From(in _selectCardShortcuts));
		info.AddProperty(PropertyName._selectModeBackstop, Variant.From(in _selectModeBackstop));
		info.AddProperty(PropertyName._upgradePreviewContainer, Variant.From(in _upgradePreviewContainer));
		info.AddProperty(PropertyName._selectedHandCardContainer, Variant.From(in _selectedHandCardContainer));
		info.AddProperty(PropertyName._upgradePreview, Variant.From(in _upgradePreview));
		info.AddProperty(PropertyName._selectModeConfirmButton, Variant.From(in _selectModeConfirmButton));
		info.AddProperty(PropertyName._selectionHeader, Variant.From(in _selectionHeader));
		info.AddProperty(PropertyName._currentCardPlay, Variant.From(in _currentCardPlay));
		info.AddProperty(PropertyName._currentMode, Variant.From(in _currentMode));
		info.AddProperty(PropertyName._draggedHolderIndex, Variant.From(in _draggedHolderIndex));
		info.AddProperty(PropertyName._lastFocusedHolderIdx, Variant.From(in _lastFocusedHolderIdx));
		info.AddProperty(PropertyName._animEnableTween, Variant.From(in _animEnableTween));
		info.AddProperty(PropertyName._isDisabled, Variant.From(in _isDisabled));
		info.AddProperty(PropertyName._animInTween, Variant.From(in _animInTween));
		info.AddProperty(PropertyName._animOutTween, Variant.From(in _animOutTween));
		info.AddProperty(PropertyName._selectedCardScaleTween, Variant.From(in _selectedCardScaleTween));
		info.AddSignalEventDelegate(SignalName.ModeChanged, backing_ModeChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CardHolderContainer, out var value))
		{
			CardHolderContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.PeekButton, out var value2))
		{
			PeekButton = value2.As<NPeekButton>();
		}
		if (info.TryGetProperty(PropertyName.CurrentMode, out var value3))
		{
			CurrentMode = value3.As<Mode>();
		}
		if (info.TryGetProperty(PropertyName.FocusedHolder, out var value4))
		{
			FocusedHolder = value4.As<NHandCardHolder>();
		}
		if (info.TryGetProperty(PropertyName._selectCardShortcuts, out var value5))
		{
			_selectCardShortcuts = value5.As<StringName[]>();
		}
		if (info.TryGetProperty(PropertyName._selectModeBackstop, out var value6))
		{
			_selectModeBackstop = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._upgradePreviewContainer, out var value7))
		{
			_upgradePreviewContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._selectedHandCardContainer, out var value8))
		{
			_selectedHandCardContainer = value8.As<NSelectedHandCardContainer>();
		}
		if (info.TryGetProperty(PropertyName._upgradePreview, out var value9))
		{
			_upgradePreview = value9.As<NUpgradePreview>();
		}
		if (info.TryGetProperty(PropertyName._selectModeConfirmButton, out var value10))
		{
			_selectModeConfirmButton = value10.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._selectionHeader, out var value11))
		{
			_selectionHeader = value11.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentCardPlay, out var value12))
		{
			_currentCardPlay = value12.As<NCardPlay>();
		}
		if (info.TryGetProperty(PropertyName._currentMode, out var value13))
		{
			_currentMode = value13.As<Mode>();
		}
		if (info.TryGetProperty(PropertyName._draggedHolderIndex, out var value14))
		{
			_draggedHolderIndex = value14.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastFocusedHolderIdx, out var value15))
		{
			_lastFocusedHolderIdx = value15.As<int>();
		}
		if (info.TryGetProperty(PropertyName._animEnableTween, out var value16))
		{
			_animEnableTween = value16.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isDisabled, out var value17))
		{
			_isDisabled = value17.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._animInTween, out var value18))
		{
			_animInTween = value18.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._animOutTween, out var value19))
		{
			_animOutTween = value19.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._selectedCardScaleTween, out var value20))
		{
			_selectedCardScaleTween = value20.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ModeChangedEventHandler>(SignalName.ModeChanged, out var value21))
		{
			backing_ModeChanged = value21;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.ModeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalModeChanged()
	{
		EmitSignal(SignalName.ModeChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.ModeChanged && args.Count == 0)
		{
			backing_ModeChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.ModeChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
