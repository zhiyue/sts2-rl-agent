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
	public static ulong? NetId { get; set; }

	public static Player? GetMe(IPlayerCollection? playerCollection)
	{
		if (!NetId.HasValue || playerCollection == null)
		{
			return null;
		}
		return playerCollection.GetPlayer(NetId.Value) ?? throw new InvalidOperationException("Local player not found in player collection.");
	}

	public static SerializablePlayer? GetMe(SerializableRun? run)
	{
		if (!NetId.HasValue || run == null)
		{
			return null;
		}
		return run.Players.FirstOrDefault((SerializablePlayer p) => p.NetId == NetId.Value) ?? throw new InvalidOperationException("Local player not found in serializable run.");
	}

	public static Player? GetMe(CombatState? combatState)
	{
		if (!NetId.HasValue || combatState == null)
		{
			return null;
		}
		return combatState.GetPlayer(NetId.Value) ?? throw new InvalidOperationException("Local player not found in combat.");
	}

	public static Player? GetMe(IEnumerable<Player> players)
	{
		if (!NetId.HasValue)
		{
			return null;
		}
		return players.FirstOrDefault((Player player) => player.NetId == NetId);
	}

	public static Creature? GetMe(IEnumerable<Creature> creatures)
	{
		if (!NetId.HasValue)
		{
			return null;
		}
		return creatures.FirstOrDefault((Creature creature) => creature.Player?.NetId == NetId);
	}

	public static bool IsMe(Player? player)
	{
		if (player != null && NetId.HasValue)
		{
			return player.NetId == NetId;
		}
		return false;
	}

	public static bool IsMe(Creature? creature)
	{
		return IsMe(creature?.Player);
	}

	public static bool ContainsMe(IEnumerable<Player> players)
	{
		return players.Any(IsMe);
	}

	public static bool ContainsMe(IEnumerable<Creature> creatures)
	{
		return creatures.Any(IsMe);
	}

	public static bool IsMine(CardModel? card)
	{
		if (card != null && card.IsMutable)
		{
			return IsMe(card.Owner);
		}
		return false;
	}

	public static bool IsMine(PotionModel? potion)
	{
		if (potion != null && potion.IsMutable)
		{
			return IsMe(potion.Owner);
		}
		return false;
	}

	public static bool IsMine(RelicModel? relic)
	{
		if (relic != null && relic.IsMutable)
		{
			return IsMe(relic.Owner);
		}
		return false;
	}

	public static bool IsMine(EventModel? eventModel)
	{
		if (eventModel != null && eventModel.IsMutable)
		{
			return IsMe(eventModel.Owner);
		}
		return false;
	}
}
