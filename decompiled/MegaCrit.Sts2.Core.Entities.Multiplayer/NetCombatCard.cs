using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public struct NetCombatCard : IPacketSerializable, IEquatable<NetCombatCard>
{
	public uint CombatCardIndex { get; private set; }

	public static NetCombatCard ForTesting(uint index)
	{
		return new NetCombatCard
		{
			CombatCardIndex = index
		};
	}

	public static NetCombatCard FromModel(CardModel card)
	{
		if (!card.IsMutable)
		{
			throw new InvalidOperationException("Immutable instances of CardModel should not be serialized through NetCombatCard! Send the ModelId instead.");
		}
		CardPile pile = card.Pile;
		if (pile != null && !pile.IsCombatPile)
		{
			throw new InvalidOperationException("Cannot use NetCombatCard to serialize a card in a non-combat pile!");
		}
		return new NetCombatCard
		{
			CombatCardIndex = NetCombatCardDb.Instance.GetCardId(card)
		};
	}

	public readonly CardModel ToCardModel()
	{
		return NetCombatCardDb.Instance.GetCard(CombatCardIndex);
	}

	public readonly CardModel? ToCardModelOrNull()
	{
		NetCombatCardDb.Instance.TryGetCard(CombatCardIndex, out CardModel card);
		return card;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(CombatCardIndex, 16);
	}

	public void Deserialize(PacketReader reader)
	{
		CombatCardIndex = reader.ReadUInt(16);
	}

	public bool Equals(NetCombatCard other)
	{
		return CombatCardIndex == other.CombatCardIndex;
	}

	public static bool operator ==(NetCombatCard c1, NetCombatCard c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(NetCombatCard c1, NetCombatCard c2)
	{
		return !c1.Equals(c2);
	}

	public override bool Equals(object? obj)
	{
		if (obj is NetCombatCard other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return CombatCardIndex.GetHashCode();
	}

	public override string ToString()
	{
		return $"{"NetCombatCard"} {CombatCardIndex}";
	}
}
