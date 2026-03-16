using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

public static class NodeUtil
{
	public static bool IsDescendant(Node parent, Node candidate)
	{
		for (Node parent2 = candidate.GetParent(); parent2 != null; parent2 = parent2.GetParent())
		{
			if (parent2 == parent)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsValid(this Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid(node))
		{
			return !node.IsQueuedForDeletion();
		}
		return false;
	}

	public static void TryGrabFocus(this Control control)
	{
		if (NControllerManager.Instance.IsUsingController)
		{
			if (control.IsVisibleInTree())
			{
				control.GrabFocus();
			}
			else
			{
				Callable.From(control.GrabFocus).CallDeferred();
			}
		}
	}

	public static T? GetAncestorOfType<T>(this Node node)
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is T)
			{
				return (T)(object)((parent is T) ? parent : null);
			}
		}
		return default(T);
	}

	public static IEnumerable<T> GetChildrenRecursive<T>(this Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			foreach (T item in child.GetChildrenRecursive<T>())
			{
				yield return item;
			}
			if (child is T)
			{
				yield return (T)(object)((child is T) ? child : null);
			}
		}
	}
}
