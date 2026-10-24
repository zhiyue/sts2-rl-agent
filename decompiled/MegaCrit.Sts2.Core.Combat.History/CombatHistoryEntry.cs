using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat.History;

public abstract class CombatHistoryEntry
{
	/// <summary>
	/// The turn numbers of all the players in combat when this action occurred.
	/// </summary>
	private readonly Dictionary<ulong, int> _playerTurnNumbers = new Dictionary<ulong, int>();

	/// <summary>
	/// Who performed this action?
	/// Even if this was a player-specific action (like drawing a card or gaining energy),
	/// this will be the player's creature.
	/// Null if the action was not performed by any creature, like when a player loses HP from Royal Poison.
	/// </summary>
	public Creature Actor { get; }

	/// <summary>
	/// What round was it when this action occurred?
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.TurnNumber" /> and <see cref="P:MegaCrit.Sts2.Core.Combat.ICombatState.RoundNumber" /> to understand the
	/// difference between the two.
	/// </summary>
	private int RoundNumber { get; }

	/// <summary>
	/// Which side's turn was it when this action occurred?
	///
	/// This will not always be the same as Actor.Side. For example, if you attack a monster with Thorns, the entry for
	/// the Thorns damage will have Actor.Side = Monster (since the monster is the one dealing damage back), but
	/// CurrentSide will be Player (since it's the player's turn).
	/// </summary>
	private CombatSide CurrentSide { get; }

	/// <summary>
	/// The history object that this entry is in.
	/// </summary>
	public CombatHistory History { get; }

	public string HumanReadableString => $"Rd {RoundNumber} ({CurrentSide} turn): {Description}.";

	public abstract string Description { get; }

	protected CombatHistoryEntry(Creature actor, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
	{
		Actor = actor;
		RoundNumber = roundNumber;
		CurrentSide = currentSide;
		History = history;
		foreach (Player player in players)
		{
			_playerTurnNumbers[player.NetId] = player.PlayerCombatState.TurnNumber;
		}
	}

	/// <summary>
	/// Did this action happen during the current turn?
	/// </summary>
	public bool HappenedThisTurn(ICombatState? state)
	{
		if (state == null)
		{
			return false;
		}
		if (RoundNumber != state.RoundNumber)
		{
			return false;
		}
		if (CurrentSide != state.CurrentSide)
		{
			return false;
		}
		foreach (KeyValuePair<ulong, int> playerTurnNumber in _playerTurnNumbers)
		{
			playerTurnNumber.Deconstruct(out var key, out var value);
			ulong playerId = key;
			int num = value;
			Player? player = state.GetPlayer(playerId);
			if (player != null)
			{
				int? num2 = player.PlayerCombatState?.TurnNumber;
				value = num;
				if (num2 == value)
				{
					continue;
				}
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Did this action happen during the specified player's last turn?
	/// </summary>
	public bool HappenedLastPlayerTurn(Player player)
	{
		if (!_playerTurnNumbers.TryGetValue(player.NetId, out var value))
		{
			return false;
		}
		int num = value;
		PlayerCombatState? playerCombatState = player.PlayerCombatState;
		return num == ((playerCombatState != null) ? new int?(playerCombatState.TurnNumber - 1) : ((int?)null));
	}
}
