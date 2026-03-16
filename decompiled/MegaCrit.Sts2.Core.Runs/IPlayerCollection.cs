using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Runs;

public interface IPlayerCollection
{
	IReadOnlyList<Player> Players { get; }

	int GetPlayerSlotIndex(Player player);

	Player? GetPlayer(ulong netId);
}
