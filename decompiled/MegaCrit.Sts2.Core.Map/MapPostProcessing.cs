using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Map;

/// <summary>
/// These helper methods make the map more clean looking and less cluttered together which should help with some of the
/// complaints. Some might look like there's a lot of looping, but I profiled them and all these public static methods
/// run together take 0 ms on my computer.
/// </summary>
public static class MapPostProcessing
{
	/// <summary>
	/// Creates a copy of the given grid that is “centered” by shifting
	/// all the nodes left or right if two of the leftmost or rightmost columns are empty.
	/// The MapPoint objects are not modified; they are simply placed into different cells.
	/// </summary>
	/// <param name="grid">The original grid to be centered.</param>
	/// <returns>A grid (modified in place) with nodes moved to center the map.</returns>
	public static MapPoint?[,] CenterGrid(MapPoint?[,] grid)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		bool flag = IsColumnEmpty(grid, 0) && IsColumnEmpty(grid, 1);
		bool flag2 = IsColumnEmpty(grid, length - 1) && IsColumnEmpty(grid, length - 2);
		int num = 0;
		if (flag && !flag2)
		{
			num = -1;
		}
		else if (!flag && flag2)
		{
			num = 1;
		}
		if (num == 0)
		{
			return grid;
		}
		if (num > 0)
		{
			for (int i = 0; i < length2; i++)
			{
				for (int num2 = length - 1; num2 >= 0; num2--)
				{
					MapPoint mapPoint = grid[num2, i];
					grid[num2, i] = null;
					int num3 = num2 + num;
					if (num3 < length)
					{
						grid[num3, i] = mapPoint;
						if (mapPoint != null)
						{
							mapPoint.coord.col = num3;
						}
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < length2; j++)
			{
				for (int k = 0; k < length; k++)
				{
					MapPoint mapPoint2 = grid[k, j];
					grid[k, j] = null;
					int num4 = k + num;
					if (num4 >= 0)
					{
						grid[num4, j] = mapPoint2;
						if (mapPoint2 != null)
						{
							mapPoint2.coord.col = num4;
						}
					}
				}
			}
		}
		return grid;
	}

	/// <summary>
	/// This method straightens out the path if there's a single line
	/// </summary>
	public static MapPoint?[,] StraightenPaths(MapPoint?[,] grid)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length; j++)
			{
				MapPoint mapPoint = grid[j, i];
				if (mapPoint == null || mapPoint.parents.Count != 1 || mapPoint.Children.Count != 1)
				{
					continue;
				}
				MapPoint mapPoint2 = mapPoint.parents.First();
				MapPoint mapPoint3 = mapPoint.Children.First();
				bool flag = mapPoint.coord.col < mapPoint3.coord.col && mapPoint.coord.col < mapPoint2.coord.col;
				bool flag2 = mapPoint.coord.col > mapPoint3.coord.col && mapPoint.coord.col > mapPoint2.coord.col;
				if (flag && j < length - 1)
				{
					int num = j + 1;
					if (grid[num, i] != null)
					{
						continue;
					}
					mapPoint.coord.col = num;
					grid[j, i] = null;
					grid[num, i] = mapPoint;
				}
				if (flag2 && j > 0)
				{
					int num2 = j - 1;
					if (grid[num2, i] == null)
					{
						mapPoint.coord.col = num2;
						grid[j, i] = null;
						grid[num2, i] = mapPoint;
					}
				}
			}
		}
		return grid;
	}

	/// <summary>
	/// Returns true if every cell in the given column is null.
	/// </summary>
	/// <param name="grid">The grid to check.</param>
	/// <param name="col">The column index.</param>
	/// <returns>True if the column is empty; otherwise, false.</returns>
	private static bool IsColumnEmpty(MapPoint?[,] grid, int col)
	{
		int length = grid.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			if (grid[col, i] != null)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Returns a set of column indices allowed for a neighbor at the specified column.
	/// Allowed positions are the same column, one column to the left, and one column to the right.
	/// </summary>
	/// <param name="column">The reference column.</param>
	/// <param name="totalColumns">Total number of columns in the grid.</param>
	/// <returns>A set of allowed column indices.</returns>
	private static HashSet<int> GetNeighborAllowedPositions(int column, int totalColumns)
	{
		HashSet<int> hashSet = new HashSet<int>(3);
		for (int i = -1; i <= 1; i++)
		{
			int num = column + i;
			if (num >= 0 && num < totalColumns)
			{
				hashSet.Add(num);
			}
		}
		return hashSet;
	}

	/// <summary>
	/// Computes the set of allowed column indices for a given node based on its parents' and children’s positions.
	/// For each parent or child, the node must be in the same column or one column to the left or right.
	/// The final allowed set is the intersection of these constraints.
	/// </summary>
	/// <param name="node">The MapPoint to evaluate.</param>
	/// <param name="totalColumns">Total number of columns in the grid.</param>
	/// <returns>A set of allowed column indices.</returns>
	private static HashSet<int> GetAllowedPositions(MapPoint node, int totalColumns)
	{
		HashSet<int> hashSet = new HashSet<int>(Enumerable.Range(0, totalColumns));
		foreach (MapPoint parent in node.parents)
		{
			hashSet.IntersectWith(GetNeighborAllowedPositions(parent.coord.col, totalColumns));
		}
		foreach (MapPoint child in node.Children)
		{
			hashSet.IntersectWith(GetNeighborAllowedPositions(child.coord.col, totalColumns));
		}
		return hashSet;
	}

	/// <summary>
	/// Spreads adjacent MapPoints in the grid by moving nodes to positions that maximize the gap between neighbors,
	/// subject to connectivity constraints.
	/// </summary>
	/// <param name="grid">The grid containing MapPoints.</param>
	/// <returns>The transformed grid.</returns>
	public static MapPoint?[,] SpreadAdjacentMapPoints(MapPoint?[,] grid)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		for (int i = 0; i < length2; i++)
		{
			List<MapPoint> list = new List<MapPoint>(length2);
			for (int j = 0; j < length; j++)
			{
				MapPoint mapPoint = grid[j, i];
				if (mapPoint != null)
				{
					list.Add(mapPoint);
				}
			}
			bool flag;
			do
			{
				flag = false;
				foreach (MapPoint item in list)
				{
					int col = item.coord.col;
					HashSet<int> allowedPositions = GetAllowedPositions(item, length);
					int num = ComputeGap(col, list, item);
					int num2 = col;
					int num3 = num;
					foreach (int item2 in allowedPositions)
					{
						if (item2 != col && (grid[item2, i] == null || grid[item2, i] == item))
						{
							int num4 = ComputeGap(item2, list, item);
							if (num4 > num3)
							{
								num2 = item2;
								num3 = num4;
							}
						}
					}
					if (num2 != col)
					{
						grid[col, i] = null;
						grid[num2, i] = item;
						item.coord.col = num2;
						flag = true;
					}
				}
			}
			while (flag);
		}
		return grid;
	}

	/// <summary>
	/// Computes the minimal horizontal gap from a candidate column to any other node in the same row.
	/// If there are no other nodes in the row, returns int.MaxValue.
	/// </summary>
	/// <param name="candidateCol">The candidate column for the node.</param>
	/// <param name="rowNodes">All nodes in the row.</param>
	/// <param name="currentNode">The node being moved.</param>
	/// <returns>The minimum column distance to any other node.</returns>
	private static int ComputeGap(int candidateCol, List<MapPoint> rowNodes, MapPoint currentNode)
	{
		int num = int.MaxValue;
		foreach (MapPoint rowNode in rowNodes)
		{
			if (rowNode != currentNode)
			{
				num = Math.Min(num, Math.Abs(candidateCol - rowNode.coord.col));
			}
		}
		if (num != int.MaxValue)
		{
			return num;
		}
		return int.MaxValue;
	}
}
