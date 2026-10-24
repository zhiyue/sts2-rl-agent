using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// The player who dealt the most damage in a multiplayer game gets this badge.
/// </summary>
public class DamageLeader : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// The player who dealt the most damage in a multiplayer game gets this badge.
	/// </summary>
	public DamageLeader(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "DAMAGE_LEADER", requiresWin: false, multiplayerOnly: true)
	{
	}

	public override bool IsObtained()
	{
		if (_run.MapPointHistory.Count < 1 || _run.MapPointHistory[0].Count < 5)
		{
			return false;
		}
		SerializablePlayer serializablePlayer = null;
		foreach (SerializablePlayer player in _run.Players)
		{
			if (serializablePlayer == null || player.ExtraFields.DamageDealt > serializablePlayer.ExtraFields.DamageDealt)
			{
				serializablePlayer = player;
			}
		}
		return serializablePlayer?.NetId == _localPlayer.NetId;
	}
}
