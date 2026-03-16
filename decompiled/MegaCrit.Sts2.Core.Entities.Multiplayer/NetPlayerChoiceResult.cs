using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Models;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct NetPlayerChoiceResult : IPacketSerializable
{
	public PlayerChoiceType type;

	public List<CardModel>? canonicalCards;

	public List<NetCombatCard>? combatCards;

	public List<NetDeckCard>? deckCards;

	public List<SerializableCard>? mutableCards;

	public ulong? mutableCardOwner;

	public List<int>? indexes;

	public ulong? playerId;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		switch (type)
		{
		case PlayerChoiceType.CanonicalCard:
			writer.WriteInt(canonicalCards.Count);
			{
				foreach (CardModel canonicalCard in canonicalCards)
				{
					writer.WriteModel(canonicalCard);
				}
				break;
			}
		case PlayerChoiceType.CombatCard:
			writer.WriteList(combatCards);
			break;
		case PlayerChoiceType.DeckCard:
			writer.WriteList(deckCards);
			break;
		case PlayerChoiceType.MutableCard:
			writer.WriteList(mutableCards);
			writer.WriteBool(mutableCardOwner.HasValue);
			if (mutableCardOwner.HasValue)
			{
				writer.WriteULong(mutableCardOwner.Value);
			}
			break;
		case PlayerChoiceType.Player:
			writer.WriteBool(playerId.HasValue);
			if (playerId.HasValue)
			{
				writer.WriteULong(playerId.Value);
			}
			break;
		case PlayerChoiceType.Index:
			writer.WriteInt(indexes.Count);
			{
				foreach (int index in indexes)
				{
					writer.WriteInt(index);
				}
				break;
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<PlayerChoiceType>();
		switch (type)
		{
		case PlayerChoiceType.CanonicalCard:
		{
			canonicalCards = new List<CardModel>();
			int num2 = reader.ReadInt();
			for (int j = 0; j < num2; j++)
			{
				canonicalCards.Add(reader.ReadModel<CardModel>());
			}
			break;
		}
		case PlayerChoiceType.CombatCard:
			combatCards = reader.ReadList<NetCombatCard>();
			break;
		case PlayerChoiceType.DeckCard:
			deckCards = reader.ReadList<NetDeckCard>();
			break;
		case PlayerChoiceType.MutableCard:
			mutableCards = reader.ReadList<SerializableCard>();
			if (reader.ReadBool())
			{
				mutableCardOwner = reader.ReadULong();
			}
			break;
		case PlayerChoiceType.Player:
			if (reader.ReadBool())
			{
				playerId = reader.ReadULong();
			}
			break;
		case PlayerChoiceType.Index:
		{
			indexes = new List<int>();
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				indexes.Add(reader.ReadInt());
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToString()
	{
		return type switch
		{
			PlayerChoiceType.CanonicalCard => "NetPlayerChoiceResult canonical " + string.Join(",", canonicalCards ?? new List<CardModel>()), 
			PlayerChoiceType.CombatCard => "NetPlayerChoiceResult combat " + string.Join(",", combatCards ?? new List<NetCombatCard>()), 
			PlayerChoiceType.DeckCard => "NetPlayerChoiceResult deck " + string.Join(",", deckCards ?? new List<NetDeckCard>()), 
			PlayerChoiceType.MutableCard => $"{"NetPlayerChoiceResult"} mutable cards {string.Join(",", mutableCards ?? new List<SerializableCard>())}, owner: {mutableCardOwner}", 
			PlayerChoiceType.Player => $"{"NetPlayerChoiceResult"} player ID {playerId}", 
			PlayerChoiceType.Index => "NetPlayerChoiceResult indexes " + string.Join(",", indexes ?? new List<int>()), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
