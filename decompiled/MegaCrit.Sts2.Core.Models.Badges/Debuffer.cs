using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

public class Debuffer : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	public Debuffer(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "DEBUFFER", requiresWin: false, multiplayerOnly: true)
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
			if (serializablePlayer == null || player.ExtraFields.DebuffsApplied > serializablePlayer.ExtraFields.DebuffsApplied)
			{
				serializablePlayer = player;
			}
		}
		return serializablePlayer?.NetId == _localPlayer.NetId;
	}
}
