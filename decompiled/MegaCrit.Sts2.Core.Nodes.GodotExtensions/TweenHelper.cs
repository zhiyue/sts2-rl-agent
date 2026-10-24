using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

public static class TweenHelper
{
	public static void FastForwardToCompletion(this Tween t)
	{
		t.CustomStep(999999999.0);
	}

	/// <summary>
	/// Awaits the tween's <see cref="F:Godot.Tween.SignalName.Finished" /> signal, automatically cancelling when the
	/// owning node exits the tree. Unlike <c>ToSignal(tween, Tween.SignalName.Finished)</c>, this will not
	/// hang when the tween is killed (e.g. because the node exited the tree and bound tweens stop processing).
	/// </summary>
	/// <remarks>
	/// <c>Tween.Kill()</c> does not emit the <c>finished</c> signal (see <c>scene/animation/tween.cpp</c>).
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission (<c>object.cpp</c>). Since the signal is never emitted, the <c>SignalAwaiterCallable</c>
	/// remains in the tween's signal map, holding a strong GC handle that pins the <c>SignalAwaiter</c> and
	/// the async state machine. The state machine in turn holds the tween reference, forming a reference cycle
	/// across the managed/native boundary that persists for the lifetime of the process.
	/// This method sidesteps that by cancelling via the node's <c>TreeExiting</c> signal, which fires before
	/// the node is freed.
	/// </remarks>
	/// <returns>
	/// <c>true</c> if the tween finished normally and the owner is still alive when control returns to the caller.
	/// <c>false</c> if the owner exited the tree before the tween completed, OR if the owner was freed between the
	/// tween's <c>Finished</c> signal firing and the caller's continuation resuming.
	/// Callers should bail out early when <c>false</c> is returned.
	/// </returns>
	public static async Task<bool> AwaitFinished(this Tween tween, Node owner)
	{
		if (!(await AwaitFinishedInternal(tween, owner)))
		{
			return false;
		}
		return GodotObject.IsInstanceValid(owner) && owner.IsInsideTree();
	}

	private static Task<bool> AwaitFinishedInternal(Tween tween, Node owner)
	{
		if (!tween.IsValid() || !tween.IsRunning())
		{
			return Task.FromResult(result: true);
		}
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		bool resolved = false;
		tween.Finished += OnFinished;
		owner.TreeExiting += OnExiting;
		if (!tween.IsValid() || !tween.IsRunning())
		{
			OnFinished();
		}
		else if (!owner.IsInsideTree())
		{
			OnExiting();
		}
		return tcs.Task;
		void OnExiting()
		{
			if (!resolved)
			{
				resolved = true;
				if (tween.IsValid())
				{
					tween.Finished -= OnFinished;
				}
				if (GodotObject.IsInstanceValid(owner))
				{
					owner.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult(result: false);
			}
		}
		void OnFinished()
		{
			if (!resolved)
			{
				resolved = true;
				if (tween.IsValid())
				{
					tween.Finished -= OnFinished;
				}
				if (GodotObject.IsInstanceValid(owner))
				{
					owner.TreeExiting -= OnExiting;
				}
				tcs.TrySetResult(result: true);
			}
		}
	}

	/// <summary>
	/// Awaits the tween's <see cref="F:Godot.Tween.SignalName.Finished" /> signal with cancellation support.
	/// Unlike <c>ToSignal(tween, Tween.SignalName.Finished)</c>, this will not hang when the tween is killed,
	/// provided the caller cancels the token.
	/// </summary>
	/// <remarks>
	/// <c>Tween.Kill()</c> does not emit the <c>finished</c> signal (see <c>scene/animation/tween.cpp</c>).
	/// <c>ToSignal</c> connects via <c>CONNECT_ONE_SHOT</c>, but one-shot cleanup only runs during signal
	/// emission (<c>object.cpp</c>). Since the signal is never emitted, the <c>SignalAwaiterCallable</c>
	/// remains in the tween's signal map, holding a strong GC handle that pins the <c>SignalAwaiter</c> and
	/// the async state machine. The state machine in turn holds the tween reference, forming a reference cycle
	/// across the managed/native boundary that persists for the lifetime of the process.
	/// This method sidesteps that by using a <see cref="T:System.Threading.Tasks.TaskCompletionSource" /> that can be cancelled externally.
	/// </remarks>
	public static Task AwaitFinished(this Tween tween, CancellationToken ct)
	{
		TaskCompletionSource tcs = new TaskCompletionSource();
		int unsubscribed = 0;
		CancellationTokenRegistration ctr = default(CancellationTokenRegistration);
		tween.Finished += OnFinished;
		if (ct.CanBeCanceled)
		{
			ctr = ct.Register(delegate
			{
				if (Interlocked.Exchange(ref unsubscribed, 1) == 0)
				{
					tween.Finished -= OnFinished;
				}
				tcs.TrySetCanceled(ct);
			});
		}
		return tcs.Task;
		void OnFinished()
		{
			if (Interlocked.Exchange(ref unsubscribed, 1) == 0)
			{
				tween.Finished -= OnFinished;
			}
			ctr.Dispose();
			tcs.TrySetResult();
		}
	}
}
