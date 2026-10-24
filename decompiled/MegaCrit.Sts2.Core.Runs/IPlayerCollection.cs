using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Interface for a collection of players. Primarily used to allow mocking the player collection for testing, so we don't
/// need to create an entire run instance to test parts of the code.
/// </summary>
public interface IPlayerCollection
{
	/// <summary>
	/// Players that are present.
	/// </summary>
	IReadOnlyList<Player> Players { get; }

	/// <returns>The slot index of the player, or -1 if the player is not in Players.</returns>
	int GetPlayerSlotIndex(Player player);

	/// <returns>The player who has player ID playerId, or null if there is no player in Players.</returns>
	Player? GetPlayer(ulong netId);
}
