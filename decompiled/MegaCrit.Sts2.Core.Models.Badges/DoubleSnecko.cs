using System.Linq;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Obtained both fake and real Snecko Eyes.
/// </summary>
public class DoubleSnecko : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Obtained both fake and real Snecko Eyes.
	/// </summary>
	public DoubleSnecko(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "DOUBLE_SNECKO", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		if (_localPlayer.Relics.Any((SerializableRelic r) => r.Id == ModelDb.Relic<SneckoEye>().Id))
		{
			return _localPlayer.Relics.Any((SerializableRelic r) => r.Id == ModelDb.Relic<FakeSneckoEye>().Id);
		}
		return false;
	}
}
