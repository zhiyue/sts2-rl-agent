using System;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public struct ActionEnqueuedMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public ulong playerId;

	public RunLocation location;

	public INetAction action;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => NetTransferMode.Reliable;

	public LogLevel LogLevel => LogLevel.Debug;

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
