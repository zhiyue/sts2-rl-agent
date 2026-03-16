using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public delegate void MessageHandlerDelegate<in T>(T message, ulong senderId) where T : INetMessage;
