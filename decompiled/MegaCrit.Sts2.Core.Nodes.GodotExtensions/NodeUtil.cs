using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

public static class NodeUtil
{
	/// <summary>
	/// Awaits the next process frame and returns the delta time.
	/// Throws <see cref="T:System.OperationCanceledException" /> if the token is cancelled or the node has exited the tree,
	/// which <see cref="M:MegaCrit.Sts2.Core.Helpers.TaskHelper.RunSafely(System.Threading.Tasks.Task)" /> silently swallows.
	/// </summary>
	public static async Task<float> AwaitProcessFrame(this Node node, CancellationToken ct = default(CancellationToken))
	{
		ct.ThrowIfCancellationRequested();
		SceneTree treeOrNull = node.GetTreeOrNull();
		if (treeOrNull == null)
		{
			throw new TaskCanceledException();
		}
		await treeOrNull.ToSignal(treeOrNull, SceneTree.SignalName.ProcessFrame);
		ct.ThrowIfCancellationRequested();
		if (!node.IsValid() || !node.IsInsideTree())
		{
			throw new TaskCanceledException();
		}
		return (float)node.GetProcessDeltaTime();
	}

	/// <summary>
	/// Awaits the next process frame.
	/// Unlike <see cref="M:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NodeUtil.AwaitProcessFrame(Godot.Node,System.Threading.CancellationToken)" />, this merely returns when the cancellation token is cancelled instead of
	/// throwing. Use this in scenarios where cancellation is frequent and expected, as throwing OperationCancelledException
	/// can be very slow when running in debug mode.
	/// This method itself cancels the cancellation token if the node is removed from the scene tree or becomes invalid.
	/// </summary>
	public static async Task AwaitProcessFrameNonThrowing(this Node node, CancellationTokenSource cts)
	{
		if (cts.IsCancellationRequested)
		{
			return;
		}
		SceneTree treeOrNull = node.GetTreeOrNull();
		if (treeOrNull == null)
		{
			await cts.CancelAsync();
			return;
		}
		await treeOrNull.ToSignal(treeOrNull, SceneTree.SignalName.ProcessFrame);
		if (!cts.IsCancellationRequested && (!node.IsValid() || !node.IsInsideTree()))
		{
			await cts.CancelAsync();
		}
	}

	/// <summary>
	/// Returns the node's <see cref="T:Godot.SceneTree" />, or <c>null</c> if the node is not inside a tree.
	/// Unlike <see cref="M:Godot.Node.GetTree" />, this does not trigger a native error print when the tree is null.
	/// Always prefer this over <see cref="M:Godot.Node.GetTree" /> when the node may not be in the tree.
	/// </summary>
	public static SceneTree? GetTreeOrNull(this Node node)
	{
		if (!node.IsInsideTree())
		{
			return null;
		}
		return node.GetTree();
	}

	/// <summary>
	/// Returns true if candidate is located within parent's subtree.
	/// </summary>
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

	/// <summary>
	/// Returns true if the node is not null, is valid, and is not deleting.
	/// </summary>
	public static bool IsValid(this Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid(node))
		{
			return !node.IsQueuedForDeletion();
		}
		return false;
	}

	/// <summary>
	/// Checks to see if a controller is detected before we grab the focus of control.
	/// </summary>
	public static void TryGrabFocus(this Control control)
	{
		if (!NControllerManager.Instance.IsUsingController)
		{
			return;
		}
		if (control.IsVisibleInTree())
		{
			control.GrabFocus();
			return;
		}
		Callable.From(delegate
		{
			if (control.IsValid() && control.IsInsideTree())
			{
				control.GrabFocus();
			}
		}).CallDeferred();
	}

	/// <summary>
	/// Obtains the nearest ancestor of the given type.
	/// </summary>
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

	/// <summary>
	/// Awaits an arbitrary signal on a <see cref="T:Godot.GodotObject" />, automatically cancelling when the owning node
	/// exits the tree.
	/// </summary>
	/// <remarks>
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission. If the source object is freed without emitting the signal, the <c>SignalAwaiterCallable</c>
	/// and its GC handle linger until the source is destroyed by the garbage collector. This method avoids that
	/// by explicitly disconnecting on both the signal-fired and tree-exiting paths.
	/// </remarks>
	public static Task AwaitSignal(this GodotObject source, StringName signal, Node owner)
	{
		if (!GodotObject.IsInstanceValid(source))
		{
			return Task.CompletedTask;
		}
		TaskCompletionSource tcs = new TaskCompletionSource();
		bool resolved = false;
		Callable callable = default(Callable);
		callable = Callable.From(OnSignal);
		source.Connect(signal, callable);
		owner.TreeExiting += OnExiting;
		return tcs.Task;
		void OnExiting()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source))
				{
					source.Disconnect(signal, callable);
				}
				tcs.TrySetCanceled();
			}
		}
		void OnSignal()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source))
				{
					source.Disconnect(signal, callable);
				}
				if (GodotObject.IsInstanceValid(owner))
				{
					owner.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult();
			}
		}
	}

	/// <summary>
	/// Awaits an arbitrary signal on a <see cref="T:Godot.GodotObject" />, automatically cancelling when the owning node
	/// exits the tree.
	/// </summary>
	/// <remarks>
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission. If the source object is freed without emitting the signal, the <c>SignalAwaiterCallable</c>
	/// and its GC handle linger until the source is destroyed by the garbage collector. This method avoids that
	/// by explicitly disconnecting on both the signal-fired and tree-exiting paths.
	/// </remarks>
	public static Task<T?> AwaitSignal<[MustBeVariant] T>(this GodotObject source, StringName signal, Node owner) where T : class
	{
		if (!GodotObject.IsInstanceValid(source))
		{
			return Task.FromResult<T>(null);
		}
		TaskCompletionSource<T?> tcs = new TaskCompletionSource<T>();
		bool resolved = false;
		Callable callable = default(Callable);
		callable = Callable.From<T>(OnSignal);
		source.Connect(signal, callable);
		owner.TreeExiting += OnExiting;
		return tcs.Task;
		void OnExiting()
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source))
				{
					source.Disconnect(signal, callable);
				}
				tcs.TrySetCanceled();
			}
		}
		void OnSignal(T obj)
		{
			if (!resolved)
			{
				resolved = true;
				if (GodotObject.IsInstanceValid(source))
				{
					source.Disconnect(signal, callable);
				}
				if (GodotObject.IsInstanceValid(owner))
				{
					owner.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult(obj);
			}
		}
	}

	/// <summary>
	/// Obtains all children of a certain type, searching recursively.
	/// </summary>
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
