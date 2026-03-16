using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct NetDeckCard : IPacketSerializable
{
	public uint DeckIndex { get; private set; }

	public static NetDeckCard FromModel(CardModel card)
	{
		if (!card.IsMutable)
		{
			throw new InvalidOperationException("Immutable instances of CardModel should not be serialized through NetDeckCard! Send the ModelId instead.");
		}
		if (card.Pile == null)
		{
			throw new InvalidOperationException("Cannot use NetDeckCard to serialize a card without a pile!");
		}
		if (card.Pile.Type != PileType.Deck)
		{
			throw new InvalidOperationException($"Cannot use {"NetDeckCard"} to serialize card {card} that is in a non-deck pile!");
		}
		return new NetDeckCard
		{
			DeckIndex = (uint)card.Pile.Cards.IndexOf(card)
		};
	}

	public readonly CardModel ToCardModel(Player player)
	{
		return player.Deck.Cards[(int)DeckIndex];
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(DeckIndex, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		DeckIndex = reader.ReadUInt(16);
	}

	public override string ToString()
	{
		return $"{"NetDeckCard"} {DeckIndex}";
	}
}
