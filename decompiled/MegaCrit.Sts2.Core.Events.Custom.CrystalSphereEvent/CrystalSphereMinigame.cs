using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;

public class CrystalSphereMinigame
{
	public enum CrystalSphereToolType
	{
		None,
		Small,
		Big
	}

	private const int _defaultWidth = 11;

	private const int _defaultHeight = 11;

	private int _divinationCount;

	private readonly TaskCompletionSource _completionSource = new TaskCompletionSource();

	private readonly Player _owner;

	public CrystalSphereCell[,] cells;

	/// <summary>
	/// Set of items on the grid
	/// </summary>
	private readonly List<CrystalSphereItem> _items = new List<CrystalSphereItem>();

	/// <summary>
	/// Items revealed by the player.
	/// Rewards for these items are given to the player once the minigame is completed
	/// </summary>
	private readonly List<CrystalSphereItem> _revealed = new List<CrystalSphereItem>();

	public Rng Rng { get; private set; }

	public int DivinationCount
	{
		get
		{
			return _divinationCount;
		}
		set
		{
			_divinationCount = value;
			this.DivinationCountChanged?.Invoke();
		}
	}

	public Vector2I GridSize => new Vector2I(cells.GetLength(0), cells.GetLength(1));

	public bool IsFinished => DivinationCount == 0;

	public bool PlacedAllItems { get; private set; }

	public CrystalSphereToolType CrystalSphereTool { get; private set; }

	public IReadOnlyList<CrystalSphereItem> Items => _items;

	/// <summary>
	/// The cell that we are currently hovered over.
	/// </summary>
	private CrystalSphereCell? HoveredCell { get; set; }

	/// <summary>
	/// The cells that are highlighted on the board. This is separate from HoveredCell
	/// because we can highlight multiple cells at once depending on the tool
	/// </summary>
	private List<CrystalSphereCell> HighlightedCells { get; set; } = new List<CrystalSphereCell>();

	public event Action? DivinationCountChanged;

	public event Action? Finished;

	public CrystalSphereMinigame(Player owner, Rng rng, int divinationCount)
	{
		_owner = owner;
		Rng = rng;
		cells = new CrystalSphereCell[11, 11];
		for (int i = 0; i < 11; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				cells[i, j] = new CrystalSphereCell(i, j);
			}
		}
		int num = 4;
		List<Vector2I> list = new List<Vector2I>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Vector2I> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new Vector2I(0, 0);
		num2++;
		span[num2] = new Vector2I(cells.GetLength(0) - 1, 0);
		num2++;
		span[num2] = new Vector2I(cells.GetLength(0) - 1, cells.GetLength(0) - 1);
		num2++;
		span[num2] = new Vector2I(0, cells.GetLength(0) - 1);
		List<Vector2I> list2 = list;
		for (int k = 0; k < 2; k++)
		{
			List<Vector2I> list3 = list2.Concat(list2.SelectMany((Vector2I c) => GetHorizontalCells(c.X, c.Y))).Concat(list2.SelectMany((Vector2I c) => GetVerticalCells(c.X, c.Y))).ToList();
			list2 = list3;
		}
		foreach (Vector2I item in list2)
		{
			TaskHelper.RunSafely(ClearCell(item.X, item.Y));
		}
		int num3 = 0;
		do
		{
			PlacedAllItems = PopulateItems();
			num3++;
		}
		while (!PlacedAllItems && num3 < 10);
		DivinationCount = divinationCount;
		CrystalSphereTool = CrystalSphereToolType.Big;
	}

	/// <summary>
	/// Ends the minigames early. Used if the event is force closed via save/quit or closing the application
	/// </summary>
	public void ForceMinigameEnd()
	{
		_revealed.Clear();
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.SetCanceled();
		}
	}

	public async Task PlayMinigame()
	{
		if (LocalContext.IsMe(_owner))
		{
			NCrystalSphereScreen.ShowScreen(this);
			await _completionSource.Task;
			await CompleteMinigame();
		}
	}

	private bool PopulateItems()
	{
		bool flag = true;
		CrystalSphereItem crystalSphereItem = new CrystalSphereRelic();
		flag = flag && crystalSphereItem.PlaceItem(this);
		_items.Add(crystalSphereItem);
		for (int i = 0; i < 2; i++)
		{
			CrystalSphereItem crystalSphereItem2 = new CrystalSpherePotion(PotionRarity.Common);
			flag = flag && crystalSphereItem2.PlaceItem(this);
			_items.Add(crystalSphereItem2);
		}
		CrystalSphereItem crystalSphereItem3 = new CrystalSpherePotion(PotionRarity.Rare);
		flag = flag && crystalSphereItem3.PlaceItem(this);
		_items.Add(crystalSphereItem3);
		CrystalSphereItem crystalSphereItem4 = new CrystalSphereCardReward(CardRarity.Common, _owner);
		flag = flag && crystalSphereItem4.PlaceItem(this);
		_items.Add(crystalSphereItem4);
		CrystalSphereItem crystalSphereItem5 = new CrystalSphereCardReward(CardRarity.Uncommon, _owner);
		flag = flag && crystalSphereItem5.PlaceItem(this);
		_items.Add(crystalSphereItem5);
		CrystalSphereItem crystalSphereItem6 = new CrystalSphereCardReward(CardRarity.Rare, _owner);
		flag = flag && crystalSphereItem6.PlaceItem(this);
		_items.Add(crystalSphereItem6);
		CrystalSphereItem crystalSphereItem7 = new CrystalSphereCurse();
		flag = flag && crystalSphereItem7.PlaceItem(this);
		_items.Add(crystalSphereItem7);
		for (int j = 0; j < 5; j++)
		{
			CrystalSphereItem crystalSphereItem8 = new CrystalSphereGold(isBig: false);
			flag = flag && crystalSphereItem8.PlaceItem(this);
			_items.Add(crystalSphereItem8);
		}
		for (int k = 0; k < 2; k++)
		{
			CrystalSphereItem crystalSphereItem9 = new CrystalSphereGold(isBig: true);
			flag = flag && crystalSphereItem9.PlaceItem(this);
			_items.Add(crystalSphereItem9);
		}
		foreach (CrystalSphereItem item in _items)
		{
			item.Revealed += OnItemRevealed;
		}
		return flag;
	}

	private void OnItemRevealed(CrystalSphereItem item)
	{
		_revealed.Add(item);
	}

	public void SetHoveredCell(CrystalSphereCell cell)
	{
		if (HoveredCell != null)
		{
			UnsetHoveredCell();
		}
		HoveredCell = cell;
		HoveredCell.IsHovered = true;
		if (CrystalSphereTool == CrystalSphereToolType.Big)
		{
			HighlightedCells = (from c in GetAdjacentCells(cell.X, cell.Y)
				select cells[c.X, c.Y]).ToList();
		}
		else
		{
			int num = 1;
			List<CrystalSphereCell> list = new List<CrystalSphereCell>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CrystalSphereCell> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = cells[cell.X, cell.Y];
			HighlightedCells = list;
		}
		foreach (CrystalSphereCell highlightedCell in HighlightedCells)
		{
			highlightedCell.IsHighlighted = true;
		}
	}

	public void UnsetHoveredCell()
	{
		foreach (CrystalSphereCell highlightedCell in HighlightedCells)
		{
			highlightedCell.IsHighlighted = false;
			highlightedCell.IsHovered = false;
		}
		HighlightedCells = new List<CrystalSphereCell>();
		HoveredCell = null;
	}

	public void SetTool(CrystalSphereToolType tool)
	{
		CrystalSphereTool = tool;
		if (HoveredCell != null)
		{
			SetHoveredCell(HoveredCell);
		}
	}

	public async Task CellClicked(CrystalSphereCell clickedCell)
	{
		DivinationCount--;
		if (CrystalSphereTool != CrystalSphereToolType.Big)
		{
			await ClearCell(clickedCell.X, clickedCell.Y);
		}
		else
		{
			List<Vector2I> adjacentCells = GetAdjacentCells(clickedCell.X, clickedCell.Y);
			foreach (Vector2I item in adjacentCells)
			{
				await ClearCell(item.X, item.Y);
			}
		}
		if (DivinationCount == 0)
		{
			_completionSource.SetResult();
		}
	}

	/// <summary>
	/// Decreases the FogValue of a CrystalSphereCell at the given x,y coordinate by amount
	/// </summary>
	/// <param name="x">X coordinate the cell we want to affect is at</param>
	/// <param name="y">Y coordinate the cell we want to affect is at</param>
	private async Task ClearCell(int x, int y)
	{
		if (x < 0 || x >= GridSize.X)
		{
			throw new ArgumentException($"[{x},{y}] is not a valid cell on this grid");
		}
		if (y < 0 || y >= GridSize.Y)
		{
			throw new ArgumentException($"[{x},{y}] is not a valid cell on this grid");
		}
		if (!cells[x, y].IsHidden)
		{
			return;
		}
		cells[x, y].IsHidden = false;
		if (cells[x, y].Item != null)
		{
			CrystalSphereItem item = cells[x, y].Item;
			if (AreAllOccupiedCellsClear(item))
			{
				await item.RevealItem(_owner);
			}
		}
	}

	/// <summary>
	/// Returns if all the cells an item occupies are visible
	/// </summary>
	/// <param name="item">The item we are checking is fully visible</param>
	/// <returns></returns>
	private bool AreAllOccupiedCellsClear(CrystalSphereItem item)
	{
		for (int i = 0; i < item.Size.X; i++)
		{
			for (int j = 0; j < item.Size.Y; j++)
			{
				int num = item.Position.X + i;
				int num2 = item.Position.Y + j;
				if (cells[num, num2].IsHidden)
				{
					return false;
				}
			}
		}
		return true;
	}

	private async Task CompleteMinigame()
	{
		await Cmd.Wait(0.75f);
		await RunManager.Instance.OneOffSynchronizer.DoLocalCrystalSphereRewards(_owner, Rng, _revealed);
		this.Finished?.Invoke();
	}

	private List<Vector2I> GetAdjacentCells(int x, int y)
	{
		return GetHorizontalCells(x, y).Concat(GetVerticalCells(x, y)).Concat(GetDiagonalCells(x, y)).Concat(new global::_003C_003Ez__ReadOnlySingleElementList<Vector2I>(new Vector2I(x, y)))
			.ToList();
	}

	private List<Vector2I> GetHorizontalCells(int x, int y)
	{
		List<Vector2I> list = new List<Vector2I>();
		for (int i = -1; i <= 1; i += 2)
		{
			int num = x + i;
			if (num >= 0 && num < 11)
			{
				list.Add(new Vector2I(num, y));
			}
		}
		return list;
	}

	private List<Vector2I> GetVerticalCells(int x, int y)
	{
		List<Vector2I> list = new List<Vector2I>();
		for (int i = -1; i <= 1; i += 2)
		{
			int num = y + i;
			if (num >= 0 && num < 11)
			{
				list.Add(new Vector2I(x, num));
			}
		}
		return list;
	}

	private List<Vector2I> GetDiagonalCells(int x, int y)
	{
		List<Vector2I> list = new List<Vector2I>();
		for (int i = -1; i <= 1; i += 2)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				int num = x + i;
				int num2 = y + j;
				if (num >= 0 && num < 11 && num2 >= 0 && num2 < 11)
				{
					list.Add(new Vector2I(num, num2));
				}
			}
		}
		return list;
	}
}
