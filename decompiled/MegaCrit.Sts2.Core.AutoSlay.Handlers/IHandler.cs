using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

public interface IHandler
{
	TimeSpan Timeout { get; }

	Task HandleAsync(Rng random, CancellationToken ct);
}
