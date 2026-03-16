using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NCardGrid.cs")]
public class NCardGrid : Control
{
	[Signal]
	public delegate void HolderPressedEventHandler(NCardHolder card);

	[Signal]
	public delegate void HolderAltPressedEventHandler(NCardHolder card);

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateScrollLimitBottom = "UpdateScrollLimitBottom";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName SetScrollPosition = "SetScrollPosition";

		public static readonly StringName SetCanScroll = "SetCanScroll";

		public static readonly StringName InsetForTopBar = "InsetForTopBar";

		public static readonly StringName ProcessMouseEvent = "ProcessMouseEvent";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		public static readonly StringName UpdateScrollPosition = "UpdateScrollPosition";

		public static readonly StringName ClearGrid = "ClearGrid";

		public static readonly StringName CalculateRowsNeeded = "CalculateRowsNeeded";

		public static readonly StringName InitGrid = "InitGrid";

		public static readonly StringName GetContainedCardsSize = "GetContainedCardsSize";

		public static readonly StringName ReflowColumns = "ReflowColumns";

		public static readonly StringName UpdateGridPositions = "UpdateGridPositions";

		public static readonly StringName OnHolderPressed = "OnHolderPressed";

		public static readonly StringName OnHolderAltPressed = "OnHolderAltPressed";

		public static readonly StringName GetTotalRowCount = "GetTotalRowCount";

		public static readonly StringName AllocateCardHolders = "AllocateCardHolders";

		public static readonly StringName ReallocateAll = "ReallocateAll";

		public static readonly StringName UpdateGridNavigation = "UpdateGridNavigation";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CanScroll = "CanScroll";

		public static readonly StringName DisplayedRows = "DisplayedRows";

		public static readonly StringName Columns = "Columns";

		public static readonly StringName CardPadding = "CardPadding";

		public static readonly StringName IsCardLibrary = "IsCardLibrary";

		public static readonly StringName ScrollLimitBottom = "ScrollLimitBottom";

		public static readonly StringName ScrollLimitTop = "ScrollLimitTop";

		public static readonly StringName IsAnimatingOut = "IsAnimatingOut";

		public static readonly StringName IsShowingUpgrades = "IsShowingUpgrades";

		public static readonly StringName YOffset = "YOffset";

		public static readonly StringName CenterGrid = "CenterGrid";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _startDrag = "_startDrag";

		public static readonly StringName _targetDrag = "_targetDrag";

		public static readonly StringName _isDragging = "_isDragging";

		public static readonly StringName _scrollingEnabled = "_scrollingEnabled";

		public static readonly StringName _scrollContainer = "_scrollContainer";

		public static readonly StringName _scrollbarPressed = "_scrollbarPressed";

		public static readonly StringName _scrollbar = "_scrollbar";

		public static readonly StringName _slidingWindowCardIndex = "_slidingWindowCardIndex";

		public static readonly StringName _pileType = "_pileType";

		public static readonly StringName _cardSize = "_cardSize";

		public static readonly StringName _cardsAnimatingOutForSetCards = "_cardsAnimatingOutForSetCards";

		public static readonly StringName _isShowingUpgrades = "_isShowingUpgrades";

		public static readonly StringName _lastFocusedHolder = "_lastFocusedHolder";

		public static readonly StringName _needsReinit = "_needsReinit";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName HolderPressed = "HolderPressed";

		public static readonly StringName HolderAltPressed = "HolderAltPressed";
	}

	private Dictionary<SortingOrders, Func<CardModel, CardModel, int>>? _sortingAlgorithms;

	private float _startDrag;

	private float _targetDrag;

	private bool _isDragging;

	private bool _scrollingEnabled = true;

	private const float _topMargin = 80f;

	private const float _bottomMargin = 320f;

	protected Control _scrollContainer;

	private bool _scrollbarPressed;

	private NScrollbar _scrollbar;

	private int _slidingWindowCardIndex;

	private PileType _pileType;

	protected Vector2 _cardSize;

	protected readonly List<CardModel> _cards = new List<CardModel>();

	protected readonly List<List<NGridCardHolder>> _cardRows = new List<List<NGridCardHolder>>();

	private readonly List<CardModel> _highlightedCards = new List<CardModel>();

	private Task? _animatingOutTask;

	private bool _cardsAnimatingOutForSetCards;

	private CancellationTokenSource? _setCardsCancellation;

	private bool _isShowingUpgrades;

	private NCardHolder? _lastFocusedHolder;

	private readonly List<CardModel> _cardsCache = new List<CardModel>();

	private readonly List<CardModel> _sortedCardsCache = new List<CardModel>();

	private bool _needsReinit;

	private HolderPressedEventHandler backing_HolderPressed;

	private HolderAltPressedEventHandler backing_HolderAltPressed;

	private Dictionary<SortingOrders, Func<CardModel, CardModel, int>> SortingAlgorithms
	{
		get
		{
			Dictionary<SortingOrders, Func<CardModel, CardModel, int>> dictionary = _sortingAlgorithms;
			if (dictionary == null)
			{
				Dictionary<SortingOrders, Func<CardModel, CardModel, int>> obj = new Dictionary<SortingOrders, Func<CardModel, CardModel, int>>
				{
					{
						SortingOrders.RarityAscending,
						(CardModel a, CardModel b) => GetCardRarityComparisonValue(a).CompareTo(GetCardRarityComparisonValue(b))
					},
					{
						SortingOrders.CostAscending,
						(CardModel a, CardModel b) => a.EnergyCost.Canonical.CompareTo(b.EnergyCost.Canonical)
					},
					{
						SortingOrders.TypeAscending,
						(CardModel a, CardModel b) => a.Type.CompareTo(b.Type)
					},
					{
						SortingOrders.AlphabetAscending,
						(CardModel a, CardModel b) => string.Compare(a.Title, b.Title, LocManager.Instance.CultureInfo, CompareOptions.None)
					},
					{
						SortingOrders.RarityDescending,
						(CardModel a, CardModel b) => -GetCardRarityComparisonValue(a).CompareTo(GetCardRarityComparisonValue(b))
					},
					{
						SortingOrders.CostDescending,
						(CardModel a, CardModel b) => -a.EnergyCost.Canonical.CompareTo(b.EnergyCost.Canonical)
					},
					{
						SortingOrders.TypeDescending,
						(CardModel a, CardModel b) => -a.Type.CompareTo(b.Type)
					},
					{
						SortingOrders.AlphabetDescending,
						(CardModel a, CardModel b) => -string.Compare(a.Title, b.Title, LocManager.Instance.CultureInfo, CompareOptions.None)
					},
					{
						SortingOrders.Ascending,
						(CardModel a, CardModel b) => _cards.IndexOf(a).CompareTo(_cards.IndexOf(b))
					},
					{
						SortingOrders.Descending,
						(CardModel a, CardModel b) => -_cards.IndexOf(a).CompareTo(_cards.IndexOf(b))
					}
				};
				Dictionary<SortingOrders, Func<CardModel, CardModel, int>> dictionary2 = obj;
				_sortingAlgorithms = obj;
				dictionary = dictionary2;
			}
			return dictionary;
		}
	}

	private bool CanScroll
	{
		get
		{
			if (_scrollingEnabled)
			{
				return base.Visible;
			}
			return false;
		}
	}

	private int DisplayedRows { get; set; }

	protected int Columns => (int)((_scrollContainer.Size.X + CardPadding) / (_cardSize.X + CardPadding));

	protected float CardPadding => 40f;

	protected virtual bool IsCardLibrary => false;

	private float ScrollLimitBottom
	{
		get
		{
			if (!(base.Size.Y > _scrollContainer.Size.Y))
			{
				return base.Size.Y - _scrollContainer.Size.Y;
			}
			return (base.Size.Y - _scrollContainer.Size.Y) * 0.5f;
		}
	}

	protected float ScrollLimitTop
	{
		get
		{
			if (!(base.Size.Y > _scrollContainer.Size.Y) || !CenterGrid)
			{
				return 0f;
			}
			return (base.Size.Y - _scrollContainer.Size.Y) * 0.5f;
		}
	}

	public IEnumerable<NGridCardHolder> CurrentlyDisplayedCardHolders => _cardRows.SelectMany((List<NGridCardHolder> r) => r);

	public IEnumerable<CardModel> CurrentlyDisplayedCards => CurrentlyDisplayedCardHolders.Select((NGridCardHolder h) => h.CardModel);

	public bool IsAnimatingOut
	{
		get
		{
			Task animatingOutTask = _animatingOutTask;
			if (animatingOutTask != null)
			{
				return !animatingOutTask.IsCompleted;
			}
			return false;
		}
	}

	public bool IsShowingUpgrades
	{
		get
		{
			return _isShowingUpgrades;
		}
		set
		{
			_isShowingUpgrades = value;
			foreach (List<NGridCardHolder> cardRow in _cardRows)
			{
				foreach (NGridCardHolder item in cardRow)
				{
					if ((_isShowingUpgrades || item.CardModel.CanonicalInstance.IsUpgradable) && (!_isShowingUpgrades || item.CardModel.IsUpgradable))
					{
						item.SetIsPreviewingUpgrade(_isShowingUpgrades);
					}
				}
			}
		}
	}

	public int YOffset { get; set; }

	protected virtual bool CenterGrid => true;

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_lastFocusedHolder != null)
			{
				return _lastFocusedHolder;
			}
			if (_cards.Count == 0)
			{
				return null;
			}
			return _cardRows[0][0];
		}
	}

	public Control? FocusedControlFromTopBar
	{
		get
		{
			if (_cards.Count != 0)
			{
				return _cardRows[0][0];
			}
			return null;
		}
	}

	public event HolderPressedEventHandler HolderPressed
	{
		add
		{
			backing_HolderPressed = (HolderPressedEventHandler)Delegate.Combine(backing_HolderPressed, value);
		}
		remove
		{
			backing_HolderPressed = (HolderPressedEventHandler)Delegate.Remove(backing_HolderPressed, value);
		}
	}

	public event HolderAltPressedEventHandler HolderAltPressed
	{
		add
		{
			backing_HolderAltPressed = (HolderAltPressedEventHandler)Delegate.Combine(backing_HolderAltPressed, value);
		}
		remove
		{
			backing_HolderAltPressed = (HolderAltPressedEventHandler)Delegate.Remove(backing_HolderAltPressed, value);
		}
	}

	private int CompareCardVisibility(CardModel a, CardModel b)
	{
		bool flag = GetCardVisibility(a) == ModelVisibility.Locked;
		bool value = GetCardVisibility(b) == ModelVisibility.Locked;
		return flag.CompareTo(value);
	}

	private int GetCardRarityComparisonValue(CardModel a)
	{
		if (a.Rarity <= CardRarity.Ancient)
		{
			return (int)a.Rarity;
		}
		return a.Rarity switch
		{
			CardRarity.Status => 6, 
			CardRarity.Curse => 7, 
			CardRarity.Event => 8, 
			CardRarity.Quest => 9, 
			CardRarity.Token => 10, 
			_ => throw new ArgumentOutOfRangeException("a", a, null), 
		};
	}

	public override void _Ready()
	{
		if (GetType() != typeof(NCardGrid))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected virtual void ConnectSignals()
	{
		_scrollContainer = GetNode<Control>("%ScrollContainer");
		_scrollbar = GetNode<NScrollbar>("Scrollbar");
		_cardSize = NCard.defaultSize * NCardHolder.smallScale;
		_scrollContainer.Connect(CanvasItem.SignalName.ItemRectChanged, Callable.From(UpdateScrollLimitBottom));
		_scrollbar.Visible = false;
		_scrollbar.Connect(NScrollbar.SignalName.MousePressed, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = true;
		}));
		_scrollbar.Connect(NScrollbar.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = false;
		}));
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		GetViewport().Disconnect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
		foreach (List<NGridCardHolder> cardRow in _cardRows)
		{
			foreach (NGridCardHolder item in cardRow)
			{
				item.QueueFreeSafely();
			}
		}
		_cardRows.Clear();
	}

	private void UpdateScrollLimitBottom()
	{
		float num = base.Size.Y + 320f;
		_scrollbar.Visible = _scrollContainer.Size.Y > num && CanScroll;
		_scrollbar.MouseFilter = (MouseFilterEnum)((_scrollContainer.Size.Y > num && CanScroll) ? 0 : 2);
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (IsVisibleInTree())
		{
			ProcessMouseEvent(inputEvent);
			ProcessScrollEvent(inputEvent);
		}
	}

	public override void _Process(double delta)
	{
		if (IsVisibleInTree() && CanScroll)
		{
			UpdateScrollPosition(delta);
			if (_needsReinit)
			{
				InitGrid();
			}
		}
	}

	public override void _Notification(int what)
	{
		base._Notification(what);
		if ((long)what == 40 && IsNodeReady())
		{
			_needsReinit = true;
		}
	}

	public void SetScrollPosition(float scrollY)
	{
		_targetDrag = scrollY;
		_scrollContainer.Position = new Vector2(_scrollContainer.Position.X, scrollY);
	}

	public void SetCanScroll(bool canScroll)
	{
		_scrollingEnabled = canScroll;
		if (!CanScroll)
		{
			_isDragging = false;
		}
	}

	public void InsetForTopBar()
	{
		SetAnchorAndOffset(Side.Top, 0f, 80f);
	}

	private void ProcessMouseEvent(InputEvent inputEvent)
	{
		if (_isDragging && inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			_targetDrag += inputEventMouseMotion.Relative.Y;
		}
		else
		{
			if (!(inputEvent is InputEventMouseButton inputEventMouseButton))
			{
				return;
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (inputEventMouseButton.Pressed)
				{
					_isDragging = true;
					_startDrag = _scrollContainer.Position.Y;
					_targetDrag = _startDrag;
				}
				else
				{
					_isDragging = false;
				}
			}
			else if (!inputEventMouseButton.Pressed)
			{
				_isDragging = false;
			}
		}
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		_targetDrag += ScrollHelper.GetDragForScrollEvent(inputEvent);
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (IsVisibleInTree() && CanScroll && NControllerManager.Instance.IsUsingController && focusedControl.GetParent() == _scrollContainer)
		{
			float value = 0f - focusedControl.Position.Y + base.Size.Y * 0.5f;
			float min = Math.Min(Math.Min(ScrollLimitTop, ScrollLimitBottom), 0f);
			float max = Math.Max(Math.Min(ScrollLimitTop, ScrollLimitBottom), 0f);
			value = Math.Clamp(value, min, max);
			_targetDrag = value;
		}
	}

	private void UpdateScrollPosition(double delta)
	{
		float num = _scrollContainer.Position.Y;
		if (Math.Abs(num - _targetDrag) > 0.1f)
		{
			num = Mathf.Lerp(num, _targetDrag, Mathf.Clamp((float)delta * 15f, 0f, 1f));
			if (Mathf.Abs(num - _targetDrag) < 0.5f)
			{
				num = _targetDrag;
			}
			AllocateCardHolders();
			if (!_scrollbarPressed && CanScroll)
			{
				_scrollbar.SetValueWithoutAnimation(Mathf.Clamp(_scrollContainer.Position.Y / ScrollLimitBottom, 0f, 1f) * 100f);
			}
		}
		if (_scrollbarPressed)
		{
			_targetDrag = Mathf.Lerp(0f, ScrollLimitBottom, (float)_scrollbar.Value / 100f);
			AllocateCardHolders();
		}
		if (!_isDragging)
		{
			if (_targetDrag < Mathf.Min(ScrollLimitBottom, ScrollLimitTop))
			{
				_targetDrag = Mathf.Lerp(_targetDrag, Mathf.Min(ScrollLimitBottom, ScrollLimitTop), (float)delta * 12f);
			}
			else if (_targetDrag > Mathf.Max(ScrollLimitTop, ScrollLimitBottom))
			{
				_targetDrag = Mathf.Lerp(_targetDrag, Mathf.Max(ScrollLimitTop, ScrollLimitBottom), (float)delta * 12f);
			}
		}
		_scrollContainer.Position = new Vector2(_scrollContainer.Position.X, num);
	}

	public void ClearGrid()
	{
		_cardsCache.Clear();
		_cards.Clear();
		TaskHelper.RunSafely(InitGrid(null));
	}

	public void SetCards(IReadOnlyList<CardModel> cardsToDisplay, PileType pileType, List<SortingOrders> sortingPriority, Task? taskToWaitOn = null)
	{
		_cardsCache.Clear();
		_cardsCache.AddRange(cardsToDisplay);
		if (sortingPriority[0] == SortingOrders.Descending)
		{
			_cardsCache.Reverse();
		}
		else if (sortingPriority[0] != SortingOrders.Ascending)
		{
			_cardsCache.Sort(delegate(CardModel x, CardModel y)
			{
				foreach (SortingOrders item in sortingPriority)
				{
					int num = SortingAlgorithms[item](x, y);
					if (num != 0)
					{
						return num;
					}
				}
				return x.Id.CompareTo(y.Id);
			});
		}
		if (IsCardLibrary)
		{
			_sortedCardsCache.Clear();
			_sortedCardsCache.AddRange(_cardsCache.OrderBy((CardModel c) => c, Comparer<CardModel>.Create(CompareCardVisibility)));
			_cardsCache.Clear();
			_cardsCache.AddRange(_sortedCardsCache);
		}
		_cards.Clear();
		_cards.AddRange(_cardsCache);
		_pileType = pileType;
		if (!_cardsAnimatingOutForSetCards)
		{
			TaskHelper.RunSafely(InitGrid(taskToWaitOn));
		}
	}

	public Task AnimateOut()
	{
		_animatingOutTask = AnimateOutInternal();
		return _animatingOutTask;
	}

	private async Task AnimateOutInternal()
	{
		if (!IsCardLibrary)
		{
			return;
		}
		List<NGridCardHolder> list = _cardRows.SelectMany((List<NGridCardHolder> c) => c).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		Tween tween = CreateTween().SetParallel();
		foreach (NGridCardHolder item in list)
		{
			tween.TweenProperty(item, "position:y", item.Position.Y + 40f, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			tween.TweenProperty(item, "modulate:a", 0f, 0.2);
		}
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task AnimateIn()
	{
		if (!IsCardLibrary)
		{
			return;
		}
		List<NGridCardHolder> list = _cardRows.SelectMany((List<NGridCardHolder> c) => c).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		Tween tween = CreateTween().SetParallel();
		for (int num = 0; num < list.Count; num++)
		{
			NGridCardHolder nGridCardHolder = list[num];
			float num2 = (float)num / (float)list.Count * 0.2f;
			float y = nGridCardHolder.Position.Y;
			Vector2 position = nGridCardHolder.Position;
			position.Y = nGridCardHolder.Position.Y + 40f;
			nGridCardHolder.Position = position;
			Color modulate = nGridCardHolder.Modulate;
			modulate.A = 0f;
			nGridCardHolder.Modulate = modulate;
			tween.TweenProperty(nGridCardHolder, "position:y", y, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
				.SetDelay(num2);
			tween.TweenProperty(nGridCardHolder, "modulate:a", 1f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.SetDelay(num2);
		}
		_setCardsCancellation = new CancellationTokenSource();
		while (tween.IsRunning())
		{
			if (_setCardsCancellation.IsCancellationRequested)
			{
				tween.Kill();
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task InitGrid(Task? taskToWaitOn)
	{
		if (_setCardsCancellation != null)
		{
			await _setCardsCancellation.CancelAsync();
		}
		_cardsAnimatingOutForSetCards = true;
		Task animatingOutTask = _animatingOutTask;
		if (animatingOutTask == null || animatingOutTask.IsCompleted)
		{
			await AnimateOut();
		}
		else
		{
			await _animatingOutTask;
		}
		if (taskToWaitOn != null)
		{
			await taskToWaitOn;
		}
		_cardsAnimatingOutForSetCards = false;
		InitGrid();
		SetScrollPosition(ScrollLimitTop);
		await AnimateIn();
	}

	private int CalculateRowsNeeded()
	{
		int a = Mathf.CeilToInt((base.Size.Y + CardPadding) / (_cardSize.Y + CardPadding)) + 2;
		return Mathf.Min(a, GetTotalRowCount());
	}

	protected virtual void InitGrid()
	{
		_scrollContainer.Position = new Vector2(_scrollContainer.Position.X, ScrollLimitTop);
		_slidingWindowCardIndex = 0;
		_scrollbar.Value = 0.0;
		DisplayedRows = CalculateRowsNeeded();
		foreach (List<NGridCardHolder> cardRow in _cardRows)
		{
			foreach (NGridCardHolder item in cardRow)
			{
				item.Name = string.Concat(item.Name, "-OLD");
				item.QueueFreeSafely();
			}
		}
		_cardRows.Clear();
		if (_cards.Count != 0)
		{
			int num = 0;
			for (int i = 0; i < DisplayedRows; i++)
			{
				List<NGridCardHolder> list = new List<NGridCardHolder>();
				for (int j = 0; j < Columns; j++)
				{
					if (num >= _cards.Count)
					{
						break;
					}
					CardModel card = _cards[num];
					NCard nCard = NCard.Create(card, GetCardVisibility(card));
					NGridCardHolder nGridCardHolder = NGridCardHolder.Create(nCard);
					list.Add(nGridCardHolder);
					nGridCardHolder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(OnHolderPressed));
					nGridCardHolder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(OnHolderAltPressed));
					nGridCardHolder.Visible = true;
					nGridCardHolder.MouseFilter = MouseFilterEnum.Pass;
					nGridCardHolder.Scale = nGridCardHolder.SmallScale;
					_scrollContainer.AddChildSafely(nGridCardHolder);
					nCard.UpdateVisuals(_pileType, CardPreviewMode.Normal);
					if (nGridCardHolder.CardModel.CanonicalInstance.IsUpgradable)
					{
						nGridCardHolder.SetIsPreviewingUpgrade(IsShowingUpgrades);
					}
					num++;
				}
				_cardRows.Add(list);
			}
		}
		_scrollContainer.SetDeferred(Control.PropertyName.Size, new Vector2(_scrollContainer.Size.X, GetContainedCardsSize().Y + 80f + 320f + (float)YOffset));
		UpdateGridPositions(0);
		UpdateGridNavigation();
		_needsReinit = false;
	}

	private Vector2 GetContainedCardsSize()
	{
		int totalRowCount = GetTotalRowCount();
		return new Vector2(Columns, totalRowCount) * _cardSize + new Vector2(Columns - 1, totalRowCount - 1) * CardPadding;
	}

	private void ReflowColumns()
	{
		if (_cards.Count != 0)
		{
			InitGrid();
		}
	}

	private void UpdateGridPositions(int index)
	{
		Vector2 vector = new Vector2((_scrollContainer.Size.X - GetContainedCardsSize().X) * 0.5f, (float)YOffset + 80f) + _cardSize * 0.5f;
		foreach (List<NGridCardHolder> cardRow in _cardRows)
		{
			foreach (NGridCardHolder item in cardRow)
			{
				int num = index / Columns;
				int num2 = index % Columns;
				item.Position = vector + new Vector2((float)num2 * (_cardSize.X + CardPadding), (float)num * (_cardSize.Y + CardPadding));
				index++;
			}
		}
	}

	public NGridCardHolder? GetCardHolder(CardModel model)
	{
		return _cardRows.SelectMany((List<NGridCardHolder> row) => row).FirstOrDefault((NGridCardHolder h) => h.CardModel == model);
	}

	public NCard? GetCardNode(CardModel model)
	{
		return GetCardHolder(model)?.CardNode;
	}

	public IEnumerable<NGridCardHolder>? GetTopRowOfCardNodes()
	{
		return _cardRows.FirstOrDefault();
	}

	private void OnHolderPressed(NCardHolder holder)
	{
		_lastFocusedHolder = holder;
		EmitSignal(SignalName.HolderPressed, holder);
	}

	private void OnHolderAltPressed(NCardHolder holder)
	{
		_lastFocusedHolder = holder;
		EmitSignal(SignalName.HolderAltPressed, holder);
	}

	private int GetTotalRowCount()
	{
		return Mathf.CeilToInt((float)_cards.Count / (float)Columns);
	}

	private void AllocateCardHolders()
	{
		if (_cardRows.Count != 0)
		{
			float y = GetViewportRect().Size.Y;
			float num = y;
			List<NGridCardHolder> list = _cardRows[0];
			float y2 = list[0].GlobalPosition.Y;
			List<List<NGridCardHolder>> cardRows = _cardRows;
			List<NGridCardHolder> list2 = cardRows[cardRows.Count - 1];
			float y3 = list2[0].GlobalPosition.Y;
			if (Mathf.Abs(y2 - 0f) > base.Size.Y * 2f)
			{
				ReallocateAll();
			}
			else if (y2 > 0f)
			{
				List<List<NGridCardHolder>> cardRows2 = _cardRows;
				ReallocateAbove(cardRows2[cardRows2.Count - 1]);
			}
			else if (y3 < num)
			{
				ReallocateBelow(_cardRows[0]);
			}
		}
	}

	private void ReallocateAll()
	{
		List<NGridCardHolder> list = _cardRows[0];
		float y = list[0].GlobalPosition.Y;
		float num = y - 0f;
		int num2 = Mathf.RoundToInt(num / (_cardSize.Y + CardPadding));
		int slidingWindowCardIndex = Mathf.Max(0, _slidingWindowCardIndex - Columns * num2);
		_slidingWindowCardIndex = slidingWindowCardIndex;
		int count = _cardRows.Count;
		for (int i = 0; i < count; i++)
		{
			AssignCardsToRow(_cardRows[i], _slidingWindowCardIndex + i * Columns);
		}
		UpdateGridPositions(_slidingWindowCardIndex);
		UpdateGridNavigation();
	}

	private void ReallocateAbove(List<NGridCardHolder> row)
	{
		int num = _slidingWindowCardIndex - Columns;
		if (num < 0)
		{
			return;
		}
		_slidingWindowCardIndex = num;
		_cardRows.RemoveAt(_cardRows.Count - 1);
		AssignCardsToRow(row, _slidingWindowCardIndex);
		_cardRows.Insert(0, row);
		List<NGridCardHolder> list = _cardRows[1];
		float y = list[0].Position.Y;
		foreach (NGridCardHolder item in row)
		{
			Vector2 position = item.Position;
			position.Y = y - _cardSize.Y - CardPadding;
			item.Position = position;
		}
		UpdateGridNavigation();
	}

	private void ReallocateBelow(List<NGridCardHolder> row)
	{
		int num = Columns * DisplayedRows;
		int num2 = _slidingWindowCardIndex + num;
		if (num2 >= _cards.Count)
		{
			return;
		}
		_slidingWindowCardIndex += Columns;
		_cardRows.RemoveAt(0);
		AssignCardsToRow(row, num2);
		_cardRows.Add(row);
		List<List<NGridCardHolder>> cardRows = _cardRows;
		List<NGridCardHolder> list = cardRows[cardRows.Count - 2];
		float y = list[0].Position.Y;
		foreach (NGridCardHolder item in row)
		{
			Vector2 position = item.Position;
			position.Y = y + _cardSize.Y + CardPadding;
			item.Position = position;
		}
		UpdateGridNavigation();
	}

	public void HighlightCard(CardModel card)
	{
		_highlightedCards.Add(card);
		GetCardNode(card)?.CardHighlight.AnimShow();
	}

	public void UnhighlightCard(CardModel card)
	{
		_highlightedCards.Remove(card);
		GetCardNode(card)?.CardHighlight.AnimHide();
	}

	protected virtual void AssignCardsToRow(List<NGridCardHolder> row, int startIndex)
	{
		for (int i = 0; i < row.Count; i++)
		{
			NGridCardHolder nGridCardHolder = row[i];
			if (startIndex + i >= _cards.Count)
			{
				nGridCardHolder.Visible = false;
				continue;
			}
			CardModel cardModel = _cards[startIndex + i];
			nGridCardHolder.ReassignToCard(cardModel, PileType.None, null, GetCardVisibility(cardModel));
			nGridCardHolder.Visible = true;
			if (_highlightedCards.Contains(cardModel))
			{
				nGridCardHolder.CardNode.CardHighlight.AnimShow();
			}
			else
			{
				nGridCardHolder.CardNode.CardHighlight.AnimHide();
			}
			if (_isShowingUpgrades && cardModel.IsUpgradable)
			{
				nGridCardHolder.SetIsPreviewingUpgrade(showUpgradePreview: true);
			}
		}
	}

	protected virtual ModelVisibility GetCardVisibility(CardModel card)
	{
		return ModelVisibility.Visible;
	}

	protected virtual void UpdateGridNavigation()
	{
		for (int i = 0; i < _cardRows.Count; i++)
		{
			for (int j = 0; j < _cardRows[i].Count; j++)
			{
				NCardHolder nCardHolder = _cardRows[i][j];
				nCardHolder.FocusNeighborLeft = ((j > 0) ? _cardRows[i][j - 1].GetPath() : _cardRows[i][_cardRows[i].Count - 1].GetPath());
				nCardHolder.FocusNeighborRight = ((j < _cardRows[i].Count - 1) ? _cardRows[i][j + 1].GetPath() : _cardRows[i][0].GetPath());
				nCardHolder.FocusNeighborTop = ((i > 0) ? _cardRows[i - 1][j].GetPath() : _cardRows[i][j].GetPath());
				nCardHolder.FocusNeighborBottom = ((i < _cardRows.Count - 1 && j < _cardRows[i + 1].Count) ? _cardRows[i + 1][j].GetPath() : _cardRows[i][j].GetPath());
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(27);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollLimitBottom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "scrollY", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetCanScroll, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "canScroll", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InsetForTopBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ProcessMouseEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		list.Add(new MethodInfo(MethodName.UpdateScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CalculateRowsNeeded, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetContainedCardsSize, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReflowColumns, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateGridPositions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHolderPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHolderAltPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetTotalRowCount, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AllocateCardHolders, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReallocateAll, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateGridNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
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
		if (method == MethodName.UpdateScrollLimitBottom && args.Count == 0)
		{
			UpdateScrollLimitBottom();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScrollPosition && args.Count == 1)
		{
			SetScrollPosition(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCanScroll && args.Count == 1)
		{
			SetCanScroll(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InsetForTopBar && args.Count == 0)
		{
			InsetForTopBar();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessMouseEvent && args.Count == 1)
		{
			ProcessMouseEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName.UpdateScrollPosition && args.Count == 1)
		{
			UpdateScrollPosition(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearGrid && args.Count == 0)
		{
			ClearGrid();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CalculateRowsNeeded && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(CalculateRowsNeeded());
			return true;
		}
		if (method == MethodName.InitGrid && args.Count == 0)
		{
			InitGrid();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetContainedCardsSize && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetContainedCardsSize());
			return true;
		}
		if (method == MethodName.ReflowColumns && args.Count == 0)
		{
			ReflowColumns();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateGridPositions && args.Count == 1)
		{
			UpdateGridPositions(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHolderPressed && args.Count == 1)
		{
			OnHolderPressed(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHolderAltPressed && args.Count == 1)
		{
			OnHolderAltPressed(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetTotalRowCount && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<int>(GetTotalRowCount());
			return true;
		}
		if (method == MethodName.AllocateCardHolders && args.Count == 0)
		{
			AllocateCardHolders();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReallocateAll && args.Count == 0)
		{
			ReallocateAll();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateGridNavigation && args.Count == 0)
		{
			UpdateGridNavigation();
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
		if (method == MethodName.ConnectSignals)
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
		if (method == MethodName.UpdateScrollLimitBottom)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.SetScrollPosition)
		{
			return true;
		}
		if (method == MethodName.SetCanScroll)
		{
			return true;
		}
		if (method == MethodName.InsetForTopBar)
		{
			return true;
		}
		if (method == MethodName.ProcessMouseEvent)
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
		if (method == MethodName.UpdateScrollPosition)
		{
			return true;
		}
		if (method == MethodName.ClearGrid)
		{
			return true;
		}
		if (method == MethodName.CalculateRowsNeeded)
		{
			return true;
		}
		if (method == MethodName.InitGrid)
		{
			return true;
		}
		if (method == MethodName.GetContainedCardsSize)
		{
			return true;
		}
		if (method == MethodName.ReflowColumns)
		{
			return true;
		}
		if (method == MethodName.UpdateGridPositions)
		{
			return true;
		}
		if (method == MethodName.OnHolderPressed)
		{
			return true;
		}
		if (method == MethodName.OnHolderAltPressed)
		{
			return true;
		}
		if (method == MethodName.GetTotalRowCount)
		{
			return true;
		}
		if (method == MethodName.AllocateCardHolders)
		{
			return true;
		}
		if (method == MethodName.ReallocateAll)
		{
			return true;
		}
		if (method == MethodName.UpdateGridNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.DisplayedRows)
		{
			DisplayedRows = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.IsShowingUpgrades)
		{
			IsShowingUpgrades = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.YOffset)
		{
			YOffset = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._startDrag)
		{
			_startDrag = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetDrag)
		{
			_targetDrag = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scrollingEnabled)
		{
			_scrollingEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scrollContainer)
		{
			_scrollContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			_scrollbarPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scrollbar)
		{
			_scrollbar = VariantUtils.ConvertTo<NScrollbar>(in value);
			return true;
		}
		if (name == PropertyName._slidingWindowCardIndex)
		{
			_slidingWindowCardIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._pileType)
		{
			_pileType = VariantUtils.ConvertTo<PileType>(in value);
			return true;
		}
		if (name == PropertyName._cardSize)
		{
			_cardSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._cardsAnimatingOutForSetCards)
		{
			_cardsAnimatingOutForSetCards = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isShowingUpgrades)
		{
			_isShowingUpgrades = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lastFocusedHolder)
		{
			_lastFocusedHolder = VariantUtils.ConvertTo<NCardHolder>(in value);
			return true;
		}
		if (name == PropertyName._needsReinit)
		{
			_needsReinit = VariantUtils.ConvertTo<bool>(in value);
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
		int from2;
		if (name == PropertyName.DisplayedRows)
		{
			from2 = DisplayedRows;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.Columns)
		{
			from2 = Columns;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		float from3;
		if (name == PropertyName.CardPadding)
		{
			from3 = CardPadding;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.IsCardLibrary)
		{
			from = IsCardLibrary;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ScrollLimitBottom)
		{
			from3 = ScrollLimitBottom;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.ScrollLimitTop)
		{
			from3 = ScrollLimitTop;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.IsAnimatingOut)
		{
			from = IsAnimatingOut;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsShowingUpgrades)
		{
			from = IsShowingUpgrades;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.YOffset)
		{
			from2 = YOffset;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.CenterGrid)
		{
			from = CenterGrid;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		Control from4;
		if (name == PropertyName.DefaultFocusedControl)
		{
			from4 = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from4 = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		if (name == PropertyName._startDrag)
		{
			value = VariantUtils.CreateFrom(in _startDrag);
			return true;
		}
		if (name == PropertyName._targetDrag)
		{
			value = VariantUtils.CreateFrom(in _targetDrag);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
			return true;
		}
		if (name == PropertyName._scrollingEnabled)
		{
			value = VariantUtils.CreateFrom(in _scrollingEnabled);
			return true;
		}
		if (name == PropertyName._scrollContainer)
		{
			value = VariantUtils.CreateFrom(in _scrollContainer);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			value = VariantUtils.CreateFrom(in _scrollbarPressed);
			return true;
		}
		if (name == PropertyName._scrollbar)
		{
			value = VariantUtils.CreateFrom(in _scrollbar);
			return true;
		}
		if (name == PropertyName._slidingWindowCardIndex)
		{
			value = VariantUtils.CreateFrom(in _slidingWindowCardIndex);
			return true;
		}
		if (name == PropertyName._pileType)
		{
			value = VariantUtils.CreateFrom(in _pileType);
			return true;
		}
		if (name == PropertyName._cardSize)
		{
			value = VariantUtils.CreateFrom(in _cardSize);
			return true;
		}
		if (name == PropertyName._cardsAnimatingOutForSetCards)
		{
			value = VariantUtils.CreateFrom(in _cardsAnimatingOutForSetCards);
			return true;
		}
		if (name == PropertyName._isShowingUpgrades)
		{
			value = VariantUtils.CreateFrom(in _isShowingUpgrades);
			return true;
		}
		if (name == PropertyName._lastFocusedHolder)
		{
			value = VariantUtils.CreateFrom(in _lastFocusedHolder);
			return true;
		}
		if (name == PropertyName._needsReinit)
		{
			value = VariantUtils.CreateFrom(in _needsReinit);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._startDrag, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetDrag, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scrollingEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CanScroll, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.DisplayedRows, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Columns, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.CardPadding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsCardLibrary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollLimitTop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scrollbarPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollbar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._slidingWindowCardIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._pileType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._cardSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._cardsAnimatingOutForSetCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isShowingUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastFocusedHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsAnimatingOut, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._needsReinit, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.YOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CenterGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.DisplayedRows, Variant.From<int>(DisplayedRows));
		info.AddProperty(PropertyName.IsShowingUpgrades, Variant.From<bool>(IsShowingUpgrades));
		info.AddProperty(PropertyName.YOffset, Variant.From<int>(YOffset));
		info.AddProperty(PropertyName._startDrag, Variant.From(in _startDrag));
		info.AddProperty(PropertyName._targetDrag, Variant.From(in _targetDrag));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
		info.AddProperty(PropertyName._scrollingEnabled, Variant.From(in _scrollingEnabled));
		info.AddProperty(PropertyName._scrollContainer, Variant.From(in _scrollContainer));
		info.AddProperty(PropertyName._scrollbarPressed, Variant.From(in _scrollbarPressed));
		info.AddProperty(PropertyName._scrollbar, Variant.From(in _scrollbar));
		info.AddProperty(PropertyName._slidingWindowCardIndex, Variant.From(in _slidingWindowCardIndex));
		info.AddProperty(PropertyName._pileType, Variant.From(in _pileType));
		info.AddProperty(PropertyName._cardSize, Variant.From(in _cardSize));
		info.AddProperty(PropertyName._cardsAnimatingOutForSetCards, Variant.From(in _cardsAnimatingOutForSetCards));
		info.AddProperty(PropertyName._isShowingUpgrades, Variant.From(in _isShowingUpgrades));
		info.AddProperty(PropertyName._lastFocusedHolder, Variant.From(in _lastFocusedHolder));
		info.AddProperty(PropertyName._needsReinit, Variant.From(in _needsReinit));
		info.AddSignalEventDelegate(SignalName.HolderPressed, backing_HolderPressed);
		info.AddSignalEventDelegate(SignalName.HolderAltPressed, backing_HolderAltPressed);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.DisplayedRows, out var value))
		{
			DisplayedRows = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName.IsShowingUpgrades, out var value2))
		{
			IsShowingUpgrades = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.YOffset, out var value3))
		{
			YOffset = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName._startDrag, out var value4))
		{
			_startDrag = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetDrag, out var value5))
		{
			_targetDrag = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value6))
		{
			_isDragging = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scrollingEnabled, out var value7))
		{
			_scrollingEnabled = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scrollContainer, out var value8))
		{
			_scrollContainer = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarPressed, out var value9))
		{
			_scrollbarPressed = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scrollbar, out var value10))
		{
			_scrollbar = value10.As<NScrollbar>();
		}
		if (info.TryGetProperty(PropertyName._slidingWindowCardIndex, out var value11))
		{
			_slidingWindowCardIndex = value11.As<int>();
		}
		if (info.TryGetProperty(PropertyName._pileType, out var value12))
		{
			_pileType = value12.As<PileType>();
		}
		if (info.TryGetProperty(PropertyName._cardSize, out var value13))
		{
			_cardSize = value13.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._cardsAnimatingOutForSetCards, out var value14))
		{
			_cardsAnimatingOutForSetCards = value14.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isShowingUpgrades, out var value15))
		{
			_isShowingUpgrades = value15.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lastFocusedHolder, out var value16))
		{
			_lastFocusedHolder = value16.As<NCardHolder>();
		}
		if (info.TryGetProperty(PropertyName._needsReinit, out var value17))
		{
			_needsReinit = value17.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<HolderPressedEventHandler>(SignalName.HolderPressed, out var value18))
		{
			backing_HolderPressed = value18;
		}
		if (info.TryGetSignalEventDelegate<HolderAltPressedEventHandler>(SignalName.HolderAltPressed, out var value19))
		{
			backing_HolderAltPressed = value19;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.HolderPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.HolderAltPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHolderPressed(NCardHolder card)
	{
		EmitSignal(SignalName.HolderPressed, card);
	}

	protected void EmitSignalHolderAltPressed(NCardHolder card)
	{
		EmitSignal(SignalName.HolderAltPressed, card);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.HolderPressed && args.Count == 1)
		{
			backing_HolderPressed?.Invoke(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
		}
		else if (signal == SignalName.HolderAltPressed && args.Count == 1)
		{
			backing_HolderAltPressed?.Invoke(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.HolderPressed)
		{
			return true;
		}
		if (signal == SignalName.HolderAltPressed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
