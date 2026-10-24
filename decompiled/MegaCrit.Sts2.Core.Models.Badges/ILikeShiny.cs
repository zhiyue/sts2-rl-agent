using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Have 25 or more relics on death (win or lose).
/// </summary>
public class ILikeShiny : Badge
{
	private const int _relicRequirement = 25;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Have 25 or more relics on death (win or lose).
	/// </summary>
	public ILikeShiny(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "ILIKESHINY", requiresWin: false, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return _localPlayer.Relics.Count >= 25;
	}
}
