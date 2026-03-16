using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public interface IClientConnectionInitializer
{
	Task<NetErrorInfo?> Connect(NetClientGameService gameService, CancellationToken cancelToken = default(CancellationToken));
}
