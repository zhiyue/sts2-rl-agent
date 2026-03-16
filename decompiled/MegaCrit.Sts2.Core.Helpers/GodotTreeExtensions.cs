using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Pooling;

namespace MegaCrit.Sts2.Core.Helpers;

public static class GodotTreeExtensions
{
	public static void AddChildSafely(this Node parent, Node? child)
	{
		if (child != null)
		{
			if (NGame.IsMainThread())
			{
				parent.AddChild(child, forceReadableName: false, Node.InternalMode.Disabled);
				return;
			}
			parent.CallDeferred(Node.MethodName.AddChild, child);
		}
	}

	public static void RemoveChildSafely(this Node parent, Node? child)
	{
		if (child != null)
		{
			if (NGame.IsMainThread())
			{
				parent.RemoveChild(child);
				return;
			}
			parent.CallDeferred(Node.MethodName.RemoveChild, child);
		}
	}

	public static void QueueFreeSafely(this Node node)
	{
		if (!GodotObject.IsInstanceValid(node))
		{
			return;
		}
		IPoolable poolable = node as IPoolable;
		if (poolable != null)
		{
			node.GetParent()?.RemoveChildSafely(node);
			Callable.From(delegate
			{
				NodePool.Free(poolable);
			}).CallDeferred();
		}
		else
		{
			node.QueueFreeSafelyNoPool();
		}
	}

	public static void QueueFreeSafelyNoPool(this Node node)
	{
		if (NGame.IsMainThread())
		{
			node.QueueFree();
		}
		else
		{
			node.CallDeferred(Node.MethodName.QueueFree);
		}
	}

	public static void FreeChildren(this Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			child.QueueFreeSafely();
		}
	}
}
