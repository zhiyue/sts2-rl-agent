using System;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

/// <summary>
/// Sent from the host to clients to indicate that a GameAction has been enqueued.
/// This is one of the most important messages in the game. The game assumes that:
/// - These messages are sent and received reliably
/// - These messages are received in the order that they are sent
/// - These messages are ordered correctly with ResumeActionAfterPlayerChoiceMessages
/// </summary>
public struct ActionEnqueuedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public ulong playerId;

	public RunLocation location;

	public INetAction action;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

	public bool ShouldBuffer => true;

	public RunLocation Location => location;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(playerId);
		writer.Write(location);
		writer.WriteByte((byte)action.ToId());
		writer.Write(action);
	}

	public void Deserialize(PacketReader reader)
	{
		playerId = reader.ReadULong();
		location = reader.Read<RunLocation>();
		int num = reader.ReadByte();
		if (!ActionTypes.TryGetActionType(num, out Type type))
		{
			throw new InvalidOperationException($"Received net action of type {num} that does not map to any type!");
		}
		action = (INetAction)Activator.CreateInstance(type);
		action.Deserialize(reader);
	}

	public override string ToString()
	{
		string value = "";
		if (action is NetPlayCardAction netPlayCardAction)
		{
			CardModel cardModel = netPlayCardAction.card.ToCardModelOrNull();
			value = ((cardModel == null) ? $"(Card ID {netPlayCardAction.card.CombatCardIndex} not found in database!)" : ("(card: " + cardModel.Title + ")"));
		}
		return $"ActionEnqueuedMessage PlayerID: {playerId} Action: {action} Source Location: {location} {value}";
	}
}
