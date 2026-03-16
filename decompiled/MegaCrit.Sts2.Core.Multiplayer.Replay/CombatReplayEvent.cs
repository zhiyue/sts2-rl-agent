using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Replay;

public struct CombatReplayEvent : IPacketSerializable
{
	public CombatReplayEventType eventType;

	public ulong? playerId;

	public INetAction? action;

	public uint? hookId;

	public uint? actionId;

	public GameActionType? gameActionType;

	public uint? choiceId;

	public NetPlayerChoiceResult? playerChoiceResult;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)eventType, 3);
		switch (eventType)
		{
		case CombatReplayEventType.GameAction:
			writer.WriteULong(playerId.Value);
			writer.WriteByte((byte)action.ToId());
			writer.Write(action);
			break;
		case CombatReplayEventType.HookAction:
			writer.WriteULong(playerId.Value);
			writer.WriteUInt(hookId.Value);
			writer.WriteEnum(gameActionType.Value);
			break;
		case CombatReplayEventType.ResumeAction:
			writer.WriteUInt(actionId.Value);
			break;
		case CombatReplayEventType.PlayerChoice:
			writer.WriteULong(playerId.Value);
			writer.WriteUInt(choiceId.Value);
			writer.Write(playerChoiceResult.Value);
			break;
		default:
			throw new ArgumentOutOfRangeException("eventType");
		}
	}

	public void Deserialize(PacketReader reader)
	{
		eventType = (CombatReplayEventType)reader.ReadInt(3);
		switch (eventType)
		{
		case CombatReplayEventType.GameAction:
		{
			playerId = reader.ReadULong();
			int num = reader.ReadByte();
			if (!ActionTypes.TryGetActionType(num, out Type type))
			{
				throw new InvalidOperationException($"Received net action of type {num} that does not map to any type!");
			}
			action = (INetAction)Activator.CreateInstance(type);
			action.Deserialize(reader);
			break;
		}
		case CombatReplayEventType.HookAction:
			playerId = reader.ReadULong();
			hookId = reader.ReadUInt();
			gameActionType = reader.ReadEnum<GameActionType>();
			break;
		case CombatReplayEventType.ResumeAction:
			actionId = reader.ReadUInt();
			break;
		case CombatReplayEventType.PlayerChoice:
			playerId = reader.ReadULong();
			choiceId = reader.ReadUInt();
			playerChoiceResult = reader.Read<NetPlayerChoiceResult>();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public CombatReplayEvent Anonymized()
	{
		CombatReplayEvent result = this;
		result.playerId = (playerId.HasValue ? new ulong?(IdAnonymizer.Anonymize(playerId.Value)) : ((ulong?)null));
		return result;
	}
}
