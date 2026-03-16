using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Map;

public class MapPoint : IComparable<MapPoint>
{
	private readonly List<AbstractModel> _quests = new List<AbstractModel>();

	public readonly HashSet<MapPoint> parents;

	public MapCoord coord;

	public bool CanBeModified { get; set; } = true;

	public MapPointType PointType { get; set; }

	public IReadOnlyList<AbstractModel> Quests => _quests;

	public HashSet<MapPoint> Children { get; }

	public event Action? NodeMarkedChanged;

	private bool Equals(MapPoint other)
	{
		return coord.Equals(other.coord);
	}

	public int CompareTo(MapPoint? other)
	{
		if (other == null)
		{
			return 1;
		}
		return coord.CompareTo(other.coord);
	}

	public MapPoint(int col, int row)
	{
		coord.row = row;
		coord.col = col;
		parents = new HashSet<MapPoint>();
		Children = new HashSet<MapPoint>();
	}

	public void AddChildPoint(MapPoint child)
	{
		Children.Add(child);
		child.parents.Add(this);
	}

	public void RemoveChildPoint(MapPoint child)
	{
		Children.Remove(child);
		child.parents.Remove(this);
	}

	public override string ToString()
	{
		return $"Point[{coord.col},{coord.row}]";
	}

	public MapPoint? LeftChild()
	{
		foreach (MapPoint child in Children)
		{
			if (child.IsAdjacentLeft(this))
			{
				return child;
			}
		}
		return null;
	}

	public MapPoint? RightChild()
	{
		foreach (MapPoint child in Children)
		{
			if (child.IsAdjacentRight(this))
			{
				return child;
			}
		}
		return null;
	}

	public bool IsAdjacentLeft(MapPoint sibling)
	{
		return coord.col - 1 == sibling.coord.col;
	}

	public bool IsAdjacentRight(MapPoint sibling)
	{
		return coord.col + 1 == sibling.coord.col;
	}

	public bool IsToTheLeft(MapPoint sibling)
	{
		return coord.col < sibling.coord.col;
	}

	public bool IsToTheRight(MapPoint sibling)
	{
		return coord.col > sibling.coord.col;
	}

	public bool IsInTheSameRow(MapPoint sibling)
	{
		return sibling.coord.row == coord.row;
	}

	public MapPoint? GetFirstCommonDescendant(MapPoint b)
	{
		if (Equals(b))
		{
			return b;
		}
		HashSet<MapPoint> allDescendants = GetAllDescendants();
		Queue<MapPoint> queue = new Queue<MapPoint>();
		queue.Enqueue(b);
		while (queue.Count > 0)
		{
			b = queue.Dequeue();
			if (allDescendants.Contains(b))
			{
				return b;
			}
			foreach (MapPoint child in b.Children)
			{
				queue.Enqueue(child);
			}
		}
		return null;
	}

	private HashSet<MapPoint> GetAllDescendants()
	{
		HashSet<MapPoint> hashSet = new HashSet<MapPoint>();
		Queue<MapPoint> queue = new Queue<MapPoint>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			MapPoint mapPoint = queue.Dequeue();
			if (!hashSet.Add(mapPoint))
			{
				continue;
			}
			foreach (MapPoint child in mapPoint.Children)
			{
				queue.Enqueue(child);
			}
		}
		return hashSet;
	}

	public MapPoint? GetCommonAncestor(MapPoint b)
	{
		if (coord.Equals(b.coord))
		{
			return b;
		}
		HashSet<MapPoint> allAncestors = GetAllAncestors();
		Queue<MapPoint> queue = new Queue<MapPoint>();
		queue.Enqueue(b);
		while (queue.Count > 0)
		{
			b = queue.Dequeue();
			if (allAncestors.Contains(b))
			{
				return b;
			}
			foreach (MapPoint parent in b.parents)
			{
				queue.Enqueue(parent);
			}
		}
		return null;
	}

	public int GetLastJunctionLength()
	{
		Queue<MapPoint> queue = new Queue<MapPoint>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			MapPoint mapPoint = queue.Dequeue();
			if (mapPoint.Children.Count > 1)
			{
				return coord.row - mapPoint.coord.row;
			}
			foreach (MapPoint parent in mapPoint.parents)
			{
				queue.Enqueue(parent);
			}
		}
		return coord.row;
	}

	public HashSet<MapPoint> GetAllAncestors()
	{
		HashSet<MapPoint> hashSet = new HashSet<MapPoint>();
		Queue<MapPoint> queue = new Queue<MapPoint>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			MapPoint mapPoint = queue.Dequeue();
			if (!hashSet.Add(mapPoint))
			{
				continue;
			}
			foreach (MapPoint parent in mapPoint.parents)
			{
				queue.Enqueue(parent);
			}
		}
		return hashSet;
	}

	public IEnumerable<MapPoint> BFS_FindPath(MapPoint target)
	{
		Queue<MapPoint> queue = new Queue<MapPoint>();
		Dictionary<MapPoint, MapPoint> dictionary = new Dictionary<MapPoint, MapPoint>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			MapPoint mapPoint = queue.Dequeue();
			if (mapPoint.Equals(target))
			{
				return BuildPath(dictionary, target);
			}
			foreach (MapPoint child in mapPoint.Children)
			{
				if (!dictionary.ContainsKey(child))
				{
					dictionary[child] = mapPoint;
					queue.Enqueue(child);
				}
			}
		}
		return new List<MapPoint>();
	}

	private IEnumerable<MapPoint> BuildPath(IReadOnlyDictionary<MapPoint, MapPoint> parentPoint, MapPoint target)
	{
		List<MapPoint> list = new List<MapPoint>();
		MapPoint mapPoint = target;
		while (!Equals(mapPoint))
		{
			list.Add(mapPoint);
			mapPoint = parentPoint[mapPoint];
		}
		list.Add(this);
		list.Reverse();
		return list;
	}

	public bool IsDescendantPathSame(MapPoint? other)
	{
		if (other == null)
		{
			return false;
		}
		MapPoint firstCommonDescendant = GetFirstCommonDescendant(other);
		if (firstCommonDescendant == null)
		{
			return false;
		}
		MapPointType[] array = (from n in BFS_FindPath(firstCommonDescendant)
			where n.PointType != MapPointType.Unassigned
			select n.PointType).ToArray();
		MapPointType[] array2 = (from n in other.BFS_FindPath(firstCommonDescendant)
			where n.PointType != MapPointType.Unassigned
			select n.PointType).ToArray();
		if (array.Length != array2.Length)
		{
			return false;
		}
		for (int num = 0; num < array.Length; num++)
		{
			if (array[num] != array2[num])
			{
				return false;
			}
		}
		return true;
	}

	public void AddQuest(AbstractModel model)
	{
		_quests.Add(model);
		this.NodeMarkedChanged?.Invoke();
	}

	public void RemoveQuest(AbstractModel model)
	{
		_quests.Remove(model);
		this.NodeMarkedChanged?.Invoke();
	}
}
