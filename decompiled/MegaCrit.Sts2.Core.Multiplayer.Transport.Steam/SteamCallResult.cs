using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;

public class SteamCallResult<T> : IDisposable where T : struct
{
	private readonly TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();

	private readonly CallResult<T> _callResult;

	private readonly CancellationTokenRegistration? _cancelTokenRegistration;

	public Task<T> Task => _completionSource.Task;

	public SteamCallResult(SteamAPICall_t call, CancellationToken cancelToken = default(CancellationToken))
	{
		_callResult = CallResult<T>.Create(OnCallResult);
		_callResult.Set(call);
		if (cancelToken.CanBeCanceled)
		{
			_cancelTokenRegistration = cancelToken.Register(Cancel);
		}
	}

	public void Cancel()
	{
		_callResult.Cancel();
		_completionSource.TrySetCanceled();
		_cancelTokenRegistration?.Dispose();
	}

	private void OnCallResult(T result, bool ioError)
	{
		if (ioError)
		{
			_completionSource.SetException(new IOException($"Got IO failure from CallResult of type {typeof(T)}!"));
		}
		else
		{
			_completionSource.SetResult(result);
		}
		_cancelTokenRegistration?.Dispose();
	}

	public void Dispose()
	{
		_callResult.Dispose();
		_cancelTokenRegistration?.Dispose();
	}
}
