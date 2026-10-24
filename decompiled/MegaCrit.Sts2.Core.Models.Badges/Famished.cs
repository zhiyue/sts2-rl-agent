using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Won with Max HP reduced to less than half of your starting HP.
/// </summary>
public class Famished : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Won with Max HP reduced to less than half of your starting HP.
	/// </summary>
	public Famished(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "FAMISHED", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		int num = (SaveUtil.CharacterOrDeprecated(_localPlayer.CharacterId).StartingHp + 1) / 2;
		return _localPlayer.MaxHp < num;
	}
}
