using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

[GenerateSubtypes]
public interface INetMessage : IPacketSerializable
{
	bool ShouldBroadcast { get; }

	NetTransferMode Mode { get; }

	LogLevel LogLevel { get; }
}
