using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// THIS BADGE IS CURRENTLY NOT IN USE
/// Deal more than 100 damage with a single card.
/// DESIGN NOTE: Not a great badge because it overvalues a specific archetype.
/// </summary>
public class Whomper : Badge
{
	private const float _damageRequirement = 100f;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// THIS BADGE IS CURRENTLY NOT IN USE
	/// Deal more than 100 damage with a single card.
	/// DESIGN NOTE: Not a great badge because it overvalues a specific archetype.
	/// </summary>
	public Whomper(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "WHOMPER", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return false;
	}
}
