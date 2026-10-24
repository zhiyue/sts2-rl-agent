using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

/// <summary>
/// Base interface for all AutoSlay handlers.
/// </summary>
public interface IHandler
{
	/// <summary>Timeout for this handler.</summary>
	TimeSpan Timeout { get; }

	/// <summary>Executes the handler logic.</summary>
	Task HandleAsync(Rng random, CancellationToken ct);
}
