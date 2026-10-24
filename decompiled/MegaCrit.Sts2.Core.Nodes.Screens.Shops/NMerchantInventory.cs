using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

/// <summary>
/// Manages the shop items for a shop.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantInventory.cs")]
public class NMerchantInventory : Control, IScreenContext
{
	[Signal]
	public delegate void InventoryClosedEventHandler();

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
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'Open' method.
		/// </summary>
		public static readonly StringName Open = "Open";

		/// <summary>
		/// Cached name for the 'SubscribeToEntries' method.
		/// </summary>
		public static readonly StringName SubscribeToEntries = "SubscribeToEntries";

		/// <summary>
		/// Cached name for the 'Close' method.
		/// </summary>
		public static readonly StringName Close = "Close";

		/// <summary>
		/// Cached name for the 'OnCardRemovalUsed' method.
		/// </summary>
		public static readonly StringName OnCardRemovalUsed = "OnCardRemovalUsed";

		/// <summary>
		/// Cached name for the 'UpdateNavigation' method.
		/// </summary>
		public static readonly StringName UpdateNavigation = "UpdateNavigation";

		/// <summary>
		/// Cached name for the 'UpdateHorizontalNavigation' method.
		/// </summary>
		public static readonly StringName UpdateHorizontalNavigation = "UpdateHorizontalNavigation";

		/// <summary>
		/// Cached name for the 'UpdateVerticalNavigation' method.
		/// </summary>
		public static readonly StringName UpdateVerticalNavigation = "UpdateVerticalNavigation";

		/// <summary>
		/// Cached name for the 'BlockInput' method.
		/// </summary>
		public static readonly StringName BlockInput = "BlockInput";

		/// <summary>
		/// Cached name for the 'UnblockInput' method.
		/// </summary>
		public static readonly StringName UnblockInput = "UnblockInput";

		/// <summary>
		/// Cached name for the 'OnActiveScreenUpdated' method.
		/// </summary>
		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsOpen' property.
		/// </summary>
		public static readonly StringName IsOpen = "IsOpen";

		/// <summary>
		/// Cached name for the 'MerchantHand' property.
		/// </summary>
		public static readonly StringName MerchantHand = "MerchantHand";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_characterCardContainer' field.
		/// </summary>
		public static readonly StringName _characterCardContainer = "_characterCardContainer";

		/// <summary>
		/// Cached name for the '_colorlessCardContainer' field.
		/// </summary>
		public static readonly StringName _colorlessCardContainer = "_colorlessCardContainer";

		/// <summary>
		/// Cached name for the '_relicContainer' field.
		/// </summary>
		public static readonly StringName _relicContainer = "_relicContainer";

		/// <summary>
		/// Cached name for the '_potionContainer' field.
		/// </summary>
		public static readonly StringName _potionContainer = "_potionContainer";

		/// <summary>
		/// Cached name for the '_cardRemovalNode' field.
		/// </summary>
		public static readonly StringName _cardRemovalNode = "_cardRemovalNode";

		/// <summary>
		/// Cached name for the '_backButton' field.
		/// </summary>
		public static readonly StringName _backButton = "_backButton";

		/// <summary>
		/// Cached name for the '_merchantDialogue' field.
		/// </summary>
		public static readonly StringName _merchantDialogue = "_merchantDialogue";

		/// <summary>
		/// Cached name for the '_inventoryTween' field.
		/// </summary>
		public static readonly StringName _inventoryTween = "_inventoryTween";

		/// <summary>
		/// Cached name for the '_slotsContainer' field.
		/// </summary>
		public static readonly StringName _slotsContainer = "_slotsContainer";

		/// <summary>
		/// Cached name for the '_backstop' field.
		/// </summary>
		public static readonly StringName _backstop = "_backstop";

		/// <summary>
		/// Cached name for the '_inputBlocker' field.
		/// </summary>
		public static readonly StringName _inputBlocker = "_inputBlocker";

		/// <summary>
		/// Cached name for the '_isInputBlocked' field.
		/// </summary>
		public static readonly StringName _isInputBlocked = "_isInputBlocked";

		/// <summary>
		/// Cached name for the '_lastSlot' field.
		/// </summary>
		public static readonly StringName _lastSlot = "_lastSlot";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'InventoryClosed' signal.
		/// </summary>
		public static readonly StringName InventoryClosed = "InventoryClosed";
	}

	private const float _openPosition = 80f;

	private const float _closedPosition = -1000f;

	private Control? _characterCardContainer;

	private Control? _colorlessCardContainer;

	protected Control? _relicContainer;

	private Control? _potionContainer;

	private NMerchantCardRemoval? _cardRemovalNode;

	private NBackButton _backButton;

	private NMerchantDialogue _merchantDialogue;

	private Tween? _inventoryTween;

	private Control _slotsContainer;

	private ColorRect _backstop;

	private Control _inputBlocker;

	private bool _isInputBlocked;

	private NMerchantSlot? _lastSlot;

	private InventoryClosedEventHandler backing_InventoryClosed;

	public MerchantInventory? Inventory { get; private set; }

	public bool IsOpen { get; private set; }

	public NMerchantHand MerchantHand { get; private set; }

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_isInputBlocked)
			{
				return _inputBlocker;
			}
			NMerchantSlot lastSlot = _lastSlot;
			if (lastSlot != null)
			{
				MerchantEntry entry = lastSlot.Entry;
				if (entry != null && entry.IsStocked)
				{
					return _lastSlot;
				}
			}
			return GetAllSlots().FirstOrDefault((NMerchantSlot s) => s.Entry.IsStocked);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantInventory.InventoryClosedEventHandler" />
	public event InventoryClosedEventHandler InventoryClosed
	{
		add
		{
			backing_InventoryClosed = (InventoryClosedEventHandler)Delegate.Combine(backing_InventoryClosed, value);
		}
		remove
		{
			backing_InventoryClosed = (InventoryClosedEventHandler)Delegate.Remove(backing_InventoryClosed, value);
		}
	}

	public override void _Ready()
	{
		_merchantDialogue = GetNode<NMerchantDialogue>("%Dialogue");
		_merchantDialogue.Modulate = Colors.Transparent;
		_slotsContainer = GetNode<Control>("%SlotsContainer");
		_slotsContainer.Position = new Vector2(_slotsContainer.Position.X, -1000f);
		_backstop = GetNode<ColorRect>("Backstop");
		MerchantHand = GetNode<NMerchantHand>("%MerchantHand");
		_characterCardContainer = GetNodeOrNull<Control>("%CharacterCards");
		_colorlessCardContainer = GetNodeOrNull<Control>("%ColorlessCards");
		_relicContainer = GetNodeOrNull<Control>("%Relics");
		_potionContainer = GetNodeOrNull<Control>("%Potions");
		_cardRemovalNode = GetNodeOrNull<NMerchantCardRemoval>("%MerchantCardRemoval");
		_backButton = GetNode<NBackButton>("%BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			Close();
		}));
		_backButton.Disable();
		_inputBlocker = GetNode<Control>("%InputBlocker");
		NGame.Instance.SetScreenShakeTarget(this);
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
		SubscribeToEntries();
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
		if (Inventory == null)
		{
			return;
		}
		foreach (MerchantEntry allEntry in Inventory.AllEntries)
		{
			allEntry.PurchaseCompleted -= OnPurchaseCompleted;
			allEntry.PurchaseFailed -= _merchantDialogue.ShowForPurchaseAttempt;
		}
	}

	public void Initialize(MerchantInventory inventory, MerchantDialogueSet dialogue)
	{
		if (Inventory != null)
		{
			throw new InvalidOperationException("Merchant inventory already populated.");
		}
		Inventory = inventory;
		for (int i = 0; i < Inventory.CharacterCardEntries.Count; i++)
		{
			NMerchantCard child = _characterCardContainer.GetChild<NMerchantCard>(i);
			child.Initialize(this);
			child.FillSlot(Inventory.CharacterCardEntries[i]);
		}
		for (int j = 0; j < Inventory.ColorlessCardEntries.Count; j++)
		{
			NMerchantCard child2 = _colorlessCardContainer.GetChild<NMerchantCard>(j);
			child2.Initialize(this);
			child2.FillSlot(Inventory.ColorlessCardEntries[j]);
		}
		for (int k = 0; k < Inventory.RelicEntries.Count; k++)
		{
			NMerchantRelic child3 = _relicContainer.GetChild<NMerchantRelic>(k);
			child3.Initialize(this);
			child3.FillSlot(Inventory.RelicEntries[k]);
		}
		for (int l = 0; l < Inventory.PotionEntries.Count; l++)
		{
			NMerchantPotion child4 = _potionContainer.GetChild<NMerchantPotion>(l);
			child4.Initialize(this);
			child4.FillSlot(Inventory.PotionEntries[l]);
		}
		if (Inventory.CardRemovalEntry != null)
		{
			_cardRemovalNode.Initialize(this);
			_cardRemovalNode.FillSlot(Inventory.CardRemovalEntry);
		}
		SubscribeToEntries();
		UpdateNavigation();
		foreach (NMerchantSlot slot in GetAllSlots())
		{
			slot.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
			{
				_lastSlot = slot;
			}));
		}
		_merchantDialogue.Initialize(dialogue);
	}

	public void Open()
	{
		if (!SaveManager.Instance.SeenFtue("merchant_ftue"))
		{
			SaveManager.Instance.MarkFtueAsComplete("merchant_ftue");
		}
		TaskHelper.RunSafely(DoOpenAnimation());
		base.MouseFilter = MouseFilterEnum.Stop;
		if (!_isInputBlocked)
		{
			_backButton.Enable();
		}
		foreach (NMerchantCard cardSlot in GetCardSlots())
		{
			cardSlot.OnInventoryOpened();
		}
		SfxCmd.Play("event:/sfx/npcs/merchant/merchant_welcome");
		IsOpen = true;
		ActiveScreenContext.Instance.Update();
		_merchantDialogue.ShowOnInventoryOpen();
	}

	private void SubscribeToEntries()
	{
		if (!IsNodeReady() || Inventory == null)
		{
			return;
		}
		foreach (MerchantEntry allEntry in Inventory.AllEntries)
		{
			allEntry.PurchaseCompleted += OnPurchaseCompleted;
			allEntry.PurchaseFailed += _merchantDialogue.ShowForPurchaseAttempt;
		}
	}

	private async Task DoOpenAnimation()
	{
		_inventoryTween?.Kill();
		_inventoryTween = CreateTween().SetParallel();
		_inventoryTween.TweenProperty(_backstop, "modulate:a", 0.8f, 1.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine)
			.FromCurrent();
		_inventoryTween.TweenProperty(_slotsContainer, "position:y", 80f, 0.699999988079071).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint)
			.FromCurrent();
		await _inventoryTween.AwaitFinished(this);
	}

	private void Close()
	{
		MerchantHand.StopPointing(0f);
		_inventoryTween?.Kill();
		_inventoryTween = CreateTween().SetParallel();
		_inventoryTween.TweenProperty(_backstop, "modulate:a", 0f, 0.800000011920929).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine)
			.FromCurrent();
		_inventoryTween.TweenProperty(_slotsContainer, "position:y", -1000f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.FromCurrent();
		base.MouseFilter = MouseFilterEnum.Ignore;
		_backButton.Disable();
		_lastSlot = null;
		IsOpen = false;
		ActiveScreenContext.Instance.Update();
		EmitSignalInventoryClosed();
	}

	private void OnPurchaseCompleted(PurchaseStatus status, MerchantEntry entry)
	{
		UpdateNavigation();
		NMerchantSlot lastSlot = GetAllSlots().FirstOrDefault((NMerchantSlot s) => s.Entry == entry);
		if (lastSlot != null && !_isInputBlocked)
		{
			(from s in GetAllSlots()
				where s.Visible && s != lastSlot
				select s).MinBy((NMerchantSlot s) => (s.GlobalPosition - lastSlot.GlobalPosition).Length())?.TryGrabFocus();
		}
		SfxCmd.Play("event:/sfx/npcs/merchant/merchant_thank_yous");
		_merchantDialogue.ShowForPurchaseAttempt(status);
	}

	public void OnCardRemovalUsed()
	{
		_cardRemovalNode.OnCardRemovalUsed();
	}

	public IEnumerable<NMerchantSlot> GetAllSlots()
	{
		List<NMerchantSlot> list = new List<NMerchantSlot>();
		list.AddRange(GetCardSlots());
		if (_relicContainer != null)
		{
			list.AddRange(_relicContainer.GetChildren().OfType<NMerchantRelic>());
		}
		if (_potionContainer != null)
		{
			list.AddRange(_potionContainer.GetChildren().OfType<NMerchantPotion>());
		}
		if (_cardRemovalNode != null)
		{
			list.Add(_cardRemovalNode);
		}
		return list;
	}

	private IEnumerable<NMerchantCard> GetCardSlots()
	{
		return new IEnumerable<Node>[2]
		{
			_characterCardContainer?.GetChildren() ?? new Array<Node>(),
			_colorlessCardContainer?.GetChildren() ?? new Array<Node>()
		}.SelectMany((IEnumerable<Node> n) => n).OfType<NMerchantCard>();
	}

	protected virtual void UpdateNavigation()
	{
		UpdateHorizontalNavigation();
		UpdateVerticalNavigation();
	}

	private void UpdateHorizontalNavigation()
	{
		List<NMerchantSlot> source = (from c in _characterCardContainer?.GetChildren().OfType<NMerchantSlot>()
			where c.Visible
			select c).ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list = (from c in _colorlessCardContainer?.GetChildren().OfType<NMerchantSlot>()
			where c.Visible
			select c).ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list2 = (from c in _relicContainer?.GetChildren().OfType<NMerchantSlot>()
			where c.Visible
			select c).ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list3 = (from c in _potionContainer?.GetChildren().OfType<NMerchantSlot>()
			where c.Visible
			select c).ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list4 = source.ToList();
		IEnumerable<NMerchantSlot> first = list.Concat(list2);
		IEnumerable<NMerchantSlot> second;
		if (_cardRemovalNode == null)
		{
			IEnumerable<NMerchantSlot> enumerable = System.Array.Empty<NMerchantSlot>();
			second = enumerable;
		}
		else
		{
			IEnumerable<NMerchantSlot> enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<NMerchantSlot>(_cardRemovalNode);
			second = enumerable;
		}
		List<NMerchantSlot> list5 = first.Concat(second).ToList();
		IEnumerable<NMerchantSlot> first2 = list.Concat(list3);
		IEnumerable<NMerchantSlot> second2;
		if (_cardRemovalNode == null)
		{
			IEnumerable<NMerchantSlot> enumerable = System.Array.Empty<NMerchantSlot>();
			second2 = enumerable;
		}
		else
		{
			IEnumerable<NMerchantSlot> enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<NMerchantSlot>(_cardRemovalNode);
			second2 = enumerable;
		}
		List<NMerchantSlot> list6 = first2.Concat(second2).ToList();
		for (int num = 0; num < list4.Count; num++)
		{
			list4[num].FocusNeighborLeft = ((num > 0) ? list4[num - 1].GetPath() : list4[num].GetPath());
			list4[num].FocusNeighborRight = ((num < list4.Count - 1) ? list4[num + 1].GetPath() : list4[num].GetPath());
		}
		for (int num2 = 0; num2 < list6.Count; num2++)
		{
			list6[num2].FocusNeighborLeft = ((num2 > 0) ? list6[num2 - 1].GetPath() : list6[num2].GetPath());
			list6[num2].FocusNeighborRight = ((num2 < list6.Count - 1) ? list6[num2 + 1].GetPath() : list6[num2].GetPath());
		}
		for (int num3 = 0; num3 < list5.Count; num3++)
		{
			list5[num3].FocusNeighborLeft = ((num3 > 0) ? list5[num3 - 1].GetPath() : list5[num3].GetPath());
			list5[num3].FocusNeighborRight = ((num3 < list5.Count - 1) ? list5[num3 + 1].GetPath() : list5[num3].GetPath());
		}
		if (list2.Count == 0 && list3.Count > 0)
		{
			if (_cardRemovalNode != null)
			{
				_cardRemovalNode.FocusNeighborLeft = list3.Last().GetPath();
			}
			if (list.Count > 0)
			{
				list.Last().FocusNeighborRight = list3.First().GetPath();
			}
		}
	}

	private void UpdateVerticalNavigation()
	{
		List<NMerchantSlot> source = _characterCardContainer?.GetChildren().OfType<NMerchantSlot>().ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> first = _colorlessCardContainer?.GetChildren().OfType<NMerchantSlot>().ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> second = _relicContainer?.GetChildren().OfType<NMerchantSlot>().ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> second2 = _potionContainer?.GetChildren().OfType<NMerchantSlot>().ToList() ?? new List<NMerchantSlot>();
		List<NMerchantSlot> list = source.ToList();
		IEnumerable<NMerchantSlot> first2 = first.Concat(second);
		IEnumerable<NMerchantSlot> second3;
		if (_cardRemovalNode == null)
		{
			IEnumerable<NMerchantSlot> enumerable = System.Array.Empty<NMerchantSlot>();
			second3 = enumerable;
		}
		else
		{
			IEnumerable<NMerchantSlot> enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<NMerchantSlot>(_cardRemovalNode);
			second3 = enumerable;
		}
		List<NMerchantSlot> list2 = first2.Concat(second3).ToList();
		IEnumerable<NMerchantSlot> first3 = first.Concat(second2);
		IEnumerable<NMerchantSlot> second4;
		if (_cardRemovalNode == null)
		{
			IEnumerable<NMerchantSlot> enumerable = System.Array.Empty<NMerchantSlot>();
			second4 = enumerable;
		}
		else
		{
			IEnumerable<NMerchantSlot> enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<NMerchantSlot>(_cardRemovalNode);
			second4 = enumerable;
		}
		List<NMerchantSlot> list3 = first3.Concat(second4).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			list[i].FocusNeighborTop = list[i].GetPath();
			if (list2.Count > 0)
			{
				Control closestStockedSlot = GetClosestStockedSlot(i, list2);
				if (closestStockedSlot != null)
				{
					list[i].FocusNeighborBottom = closestStockedSlot.GetPath();
					continue;
				}
			}
			Control closestStockedSlot2 = GetClosestStockedSlot(i, list3);
			if (closestStockedSlot2 != null)
			{
				list[i].FocusNeighborBottom = closestStockedSlot2.GetPath();
			}
			else
			{
				list[i].FocusNeighborBottom = list[i].GetPath();
			}
		}
		for (int j = 0; j < list3.Count; list3[j].FocusNeighborBottom = list3[j].GetPath(), j++)
		{
			if (list2.Count > 0)
			{
				Control closestStockedSlot3 = GetClosestStockedSlot(j, list2);
				if (closestStockedSlot3 != null)
				{
					list3[j].FocusNeighborTop = closestStockedSlot3.GetPath();
					continue;
				}
			}
			Control closestStockedSlot4 = GetClosestStockedSlot(j, list);
			if (closestStockedSlot4 != null)
			{
				list3[j].FocusNeighborTop = closestStockedSlot4.GetPath();
			}
			else
			{
				list3[j].FocusNeighborTop = list3[j].GetPath();
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			if (list.Count > 0)
			{
				Control closestStockedSlot5 = GetClosestStockedSlot(k, list);
				if (closestStockedSlot5 != null)
				{
					list2[k].FocusNeighborTop = closestStockedSlot5.GetPath();
					goto IL_02bb;
				}
			}
			list2[k].FocusNeighborTop = list2[k].GetPath();
			goto IL_02bb;
			IL_02bb:
			if (list3.Count > 0)
			{
				Control closestStockedSlot6 = GetClosestStockedSlot(k, list3);
				if (closestStockedSlot6 != null)
				{
					list2[k].FocusNeighborBottom = closestStockedSlot6.GetPath();
					continue;
				}
			}
			list2[k].FocusNeighborBottom = list2[k].GetPath();
		}
	}

	public void BlockInput()
	{
		_isInputBlocked = true;
		_inputBlocker.MouseFilter = MouseFilterEnum.Stop;
		NHotkeyManager.Instance.AddBlockingScreen(_inputBlocker);
		if (_backButton.IsEnabled)
		{
			_backButton.Disable();
		}
	}

	public void UnblockInput()
	{
		_isInputBlocked = false;
		_inputBlocker.MouseFilter = MouseFilterEnum.Ignore;
		NHotkeyManager.Instance.RemoveBlockingScreen(_inputBlocker);
		if (!_backButton.IsEnabled)
		{
			_backButton.Enable();
		}
	}

	private Control? GetClosestStockedSlot(int idx, List<NMerchantSlot> row)
	{
		NMerchantSlot nMerchantSlot = row[Math.Min(idx, row.Count - 1)];
		if (nMerchantSlot.Entry.IsStocked)
		{
			return nMerchantSlot;
		}
		int num = row.IndexOf(nMerchantSlot);
		int num2 = num - 1;
		int num3 = num + 1;
		while (num2 >= 0 || num3 < row.Count)
		{
			if (num3 < row.Count)
			{
				if (row[num3].Entry.IsStocked)
				{
					return row[num3];
				}
				num3++;
			}
			if (num2 >= 0)
			{
				if (row[num2].Entry.IsStocked)
				{
					return row[num2];
				}
				num2--;
			}
		}
		return null;
	}

	private void OnActiveScreenUpdated()
	{
		this.UpdateControllerNavEnabled();
		if (ActiveScreenContext.Instance.IsCurrent(this))
		{
			if (_characterCardContainer != null && NControllerManager.Instance.IsUsingController && _inventoryTween != null && _inventoryTween.IsRunning())
			{
				float num = 80f - _slotsContainer.Position.Y;
				MerchantHand.PointAtTarget(_characterCardContainer.GetChild<NMerchantCard>(0), Vector2.Down * num);
			}
			if (!_isInputBlocked)
			{
				_backButton.Enable();
			}
		}
		else
		{
			_backButton.Disable();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SubscribeToEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardRemovalUsed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHorizontalNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVerticalNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BlockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnblockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SubscribeToEntries && args.Count == 0)
		{
			SubscribeToEntries();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardRemovalUsed && args.Count == 0)
		{
			OnCardRemovalUsed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHorizontalNavigation && args.Count == 0)
		{
			UpdateHorizontalNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVerticalNavigation && args.Count == 0)
		{
			UpdateVerticalNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlockInput && args.Count == 0)
		{
			BlockInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnblockInput && args.Count == 0)
		{
			UnblockInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated && args.Count == 0)
		{
			OnActiveScreenUpdated();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.SubscribeToEntries)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.OnCardRemovalUsed)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		if (method == MethodName.UpdateHorizontalNavigation)
		{
			return true;
		}
		if (method == MethodName.UpdateVerticalNavigation)
		{
			return true;
		}
		if (method == MethodName.BlockInput)
		{
			return true;
		}
		if (method == MethodName.UnblockInput)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsOpen)
		{
			IsOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.MerchantHand)
		{
			MerchantHand = VariantUtils.ConvertTo<NMerchantHand>(in value);
			return true;
		}
		if (name == PropertyName._characterCardContainer)
		{
			_characterCardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._colorlessCardContainer)
		{
			_colorlessCardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._relicContainer)
		{
			_relicContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			_potionContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cardRemovalNode)
		{
			_cardRemovalNode = VariantUtils.ConvertTo<NMerchantCardRemoval>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._merchantDialogue)
		{
			_merchantDialogue = VariantUtils.ConvertTo<NMerchantDialogue>(in value);
			return true;
		}
		if (name == PropertyName._inventoryTween)
		{
			_inventoryTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._slotsContainer)
		{
			_slotsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			_inputBlocker = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isInputBlocked)
		{
			_isInputBlocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lastSlot)
		{
			_lastSlot = VariantUtils.ConvertTo<NMerchantSlot>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsOpen)
		{
			value = VariantUtils.CreateFrom<bool>(IsOpen);
			return true;
		}
		if (name == PropertyName.MerchantHand)
		{
			value = VariantUtils.CreateFrom<NMerchantHand>(MerchantHand);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._characterCardContainer)
		{
			value = VariantUtils.CreateFrom(in _characterCardContainer);
			return true;
		}
		if (name == PropertyName._colorlessCardContainer)
		{
			value = VariantUtils.CreateFrom(in _colorlessCardContainer);
			return true;
		}
		if (name == PropertyName._relicContainer)
		{
			value = VariantUtils.CreateFrom(in _relicContainer);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			value = VariantUtils.CreateFrom(in _potionContainer);
			return true;
		}
		if (name == PropertyName._cardRemovalNode)
		{
			value = VariantUtils.CreateFrom(in _cardRemovalNode);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._merchantDialogue)
		{
			value = VariantUtils.CreateFrom(in _merchantDialogue);
			return true;
		}
		if (name == PropertyName._inventoryTween)
		{
			value = VariantUtils.CreateFrom(in _inventoryTween);
			return true;
		}
		if (name == PropertyName._slotsContainer)
		{
			value = VariantUtils.CreateFrom(in _slotsContainer);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			value = VariantUtils.CreateFrom(in _inputBlocker);
			return true;
		}
		if (name == PropertyName._isInputBlocked)
		{
			value = VariantUtils.CreateFrom(in _isInputBlocked);
			return true;
		}
		if (name == PropertyName._lastSlot)
		{
			value = VariantUtils.CreateFrom(in _lastSlot);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterCardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._colorlessCardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardRemovalNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._merchantDialogue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inventoryTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slotsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputBlocker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInputBlocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastSlot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MerchantHand, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsOpen, Variant.From<bool>(IsOpen));
		info.AddProperty(PropertyName.MerchantHand, Variant.From<NMerchantHand>(MerchantHand));
		info.AddProperty(PropertyName._characterCardContainer, Variant.From(in _characterCardContainer));
		info.AddProperty(PropertyName._colorlessCardContainer, Variant.From(in _colorlessCardContainer));
		info.AddProperty(PropertyName._relicContainer, Variant.From(in _relicContainer));
		info.AddProperty(PropertyName._potionContainer, Variant.From(in _potionContainer));
		info.AddProperty(PropertyName._cardRemovalNode, Variant.From(in _cardRemovalNode));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._merchantDialogue, Variant.From(in _merchantDialogue));
		info.AddProperty(PropertyName._inventoryTween, Variant.From(in _inventoryTween));
		info.AddProperty(PropertyName._slotsContainer, Variant.From(in _slotsContainer));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._inputBlocker, Variant.From(in _inputBlocker));
		info.AddProperty(PropertyName._isInputBlocked, Variant.From(in _isInputBlocked));
		info.AddProperty(PropertyName._lastSlot, Variant.From(in _lastSlot));
		info.AddSignalEventDelegate(SignalName.InventoryClosed, backing_InventoryClosed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsOpen, out var value))
		{
			IsOpen = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.MerchantHand, out var value2))
		{
			MerchantHand = value2.As<NMerchantHand>();
		}
		if (info.TryGetProperty(PropertyName._characterCardContainer, out var value3))
		{
			_characterCardContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._colorlessCardContainer, out var value4))
		{
			_colorlessCardContainer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._relicContainer, out var value5))
		{
			_relicContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._potionContainer, out var value6))
		{
			_potionContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cardRemovalNode, out var value7))
		{
			_cardRemovalNode = value7.As<NMerchantCardRemoval>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value8))
		{
			_backButton = value8.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._merchantDialogue, out var value9))
		{
			_merchantDialogue = value9.As<NMerchantDialogue>();
		}
		if (info.TryGetProperty(PropertyName._inventoryTween, out var value10))
		{
			_inventoryTween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._slotsContainer, out var value11))
		{
			_slotsContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value12))
		{
			_backstop = value12.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._inputBlocker, out var value13))
		{
			_inputBlocker = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isInputBlocked, out var value14))
		{
			_isInputBlocked = value14.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lastSlot, out var value15))
		{
			_lastSlot = value15.As<NMerchantSlot>();
		}
		if (info.TryGetSignalEventDelegate<InventoryClosedEventHandler>(SignalName.InventoryClosed, out var value16))
		{
			backing_InventoryClosed = value16;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.InventoryClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalInventoryClosed()
	{
		EmitSignal(SignalName.InventoryClosed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.InventoryClosed && args.Count == 0)
		{
			backing_InventoryClosed?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.InventoryClosed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
