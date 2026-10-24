using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Delegate which is called when a message is received.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
/// <param name="message">The message that was received.</param>
/// <param name="senderId">The sender of the message.</param>
public delegate void MessageHandlerDelegate<in T>(T message, ulong senderId) where T : INetMessage;
