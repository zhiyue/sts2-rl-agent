using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.AutoSlay.Helpers;

public static class UiHelper
{
	public static async Task Click(NClickableControl button, int delayMs = 100)
	{
		button.ForceClick();
		await Task.Delay(delayMs);
	}

	public static List<T> FindAll<T>(Node start) where T : Node
	{
		List<T> list = new List<T>();
		if (GodotObject.IsInstanceValid(start))
		{
			FindAllRecursive(start, list);
		}
		return list;
	}

	private static void FindAllRecursive<T>(Node node, List<T> found) where T : Node
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		if (node is T item)
		{
			found.Add(item);
		}
		foreach (Node child in node.GetChildren())
		{
			FindAllRecursive(child, found);
		}
	}

	public static T? FindFirst<T>(Node start) where T : Node
	{
		if (!GodotObject.IsInstanceValid(start))
		{
			return null;
		}
		if (start is T result)
		{
			return result;
		}
		foreach (Node child in start.GetChildren())
		{
			T val = FindFirst<T>(child);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}
}
