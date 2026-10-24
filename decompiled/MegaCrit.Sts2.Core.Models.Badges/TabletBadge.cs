using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Won with 1 Max HP.
/// </summary>
public class TabletBadge : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Gold;

	/// <summary>
	/// Won with 1 Max HP.
	/// </summary>
	public TabletBadge(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "TABLET", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return _localPlayer.MaxHp == 1;
	}
}
