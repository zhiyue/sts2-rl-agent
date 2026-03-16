using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

[GenerateSubtypes]
public interface INetAction : IPacketSerializable
{
	GameAction ToGameAction(Player player);
}
