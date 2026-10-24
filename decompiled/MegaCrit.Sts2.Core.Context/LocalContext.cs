using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Context;

public static class LocalContext
{
	/// <summary>
	/// The Net ID of the local player.
	/// </summary>
	public static ulong? NetId { get; set; }

	/// <summary>
	/// Get the local player from the specified player collection.
	/// Note that the run state is a player collection.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">Thrown if the local player is not found.</exception>
	public static Player? GetMe(IPlayerCollection? playerCollection)
	{
		if (!NetId.HasValue || playerCollection == null)
		{
			return null;
		}
		return playerCollection.GetPlayer(NetId.Value) ?? throw new InvalidOperationException("Local player not found in player collection.");
	}

	/// <summary>
	/// Get the local player from the specified serializable run.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">Thrown if the local player is not found.</exception>
	public static SerializablePlayer? GetMe(SerializableRun? run)
	{
		if (!NetId.HasValue || run == null)
		{
			return null;
		}
		return run.Players.FirstOrDefault((SerializablePlayer p) => p.NetId == NetId.Value) ?? throw new InvalidOperationException("Local player not found in serializable run.");
	}

	/// <summary>
	/// Get the local player from the specified combat state.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">Thrown if the local player is not found.</exception>
	public static Player? GetMe(ICombatState? combatState)
	{
		if (!NetId.HasValue || combatState == null)
		{
			return null;
		}
		return combatState.GetPlayer(NetId.Value) ?? throw new InvalidOperationException("Local player not found in combat.");
	}

	/// <summary>
	/// Get the local player from the specified list of players.
	/// </summary>
	public static Player? GetMe(IEnumerable<Player> players)
	{
		if (!NetId.HasValue)
		{
			return null;
		}
		return players.FirstOrDefault((Player player) => player.NetId == NetId);
	}

	/// <summary>
	/// Get the local player's creature from the specified list of creatures.
	/// </summary>
	public static Creature? GetMe(IEnumerable<Creature> creatures)
	{
		if (!NetId.HasValue)
		{
			return null;
		}
		return creatures.FirstOrDefault((Creature creature) => creature.Player?.NetId == NetId);
	}

	/// <summary>
	/// Is the specified player the local player?
	/// </summary>
	public static bool IsMe(Player? player)
	{
		if (player != null && NetId.HasValue)
		{
			return player.NetId == NetId;
		}
		return false;
	}

	/// <summary>
	/// Is the specified creature the local player's creature?
	/// </summary>
	public static bool IsMe(Creature? creature)
	{
		return IsMe(creature?.Player);
	}

	/// <summary>
	/// Is the local player in the specified list of players?
	/// </summary>
	public static bool ContainsMe(IEnumerable<Player> players)
	{
		return players.Any(IsMe);
	}

	/// <summary>
	/// Is the local player's creature in the specified list of creatures?
	/// </summary>
	public static bool ContainsMe(IEnumerable<Creature> creatures)
	{
		return creatures.Any(IsMe);
	}

	/// <summary>
	/// Does the specified card belong to the local player?
	/// </summary>
	public static bool IsMine(CardModel? card)
	{
		if (card != null && card.IsMutable)
		{
			return IsMe(card.Owner);
		}
		return false;
	}

	/// <summary>
	/// Does the specified potion belong to the local player?
	/// </summary>
	public static bool IsMine(PotionModel? potion)
	{
		if (potion != null && potion.IsMutable)
		{
			return IsMe(potion.Owner);
		}
		return false;
	}

	/// <summary>
	/// Does the specified relic belong to the local player?
	/// </summary>
	public static bool IsMine(RelicModel? relic)
	{
		if (relic != null && relic.IsMutable)
		{
			return IsMe(relic.Owner);
		}
		return false;
	}

	/// <summary>
	/// Does the specified event belong to the local player?
	/// </summary>
	public static bool IsMine(EventModel? eventModel)
	{
		if (eventModel != null && eventModel.IsMutable)
		{
			return IsMe(eventModel.Owner);
		}
		return false;
	}
}
