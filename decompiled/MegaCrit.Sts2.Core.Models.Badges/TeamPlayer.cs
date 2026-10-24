using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

public class TeamPlayer : Badge
{
	public override BadgeRarity Rarity => BadgeRarity.Silver;

	public TeamPlayer(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "TEAM_PLAYER", requiresWin: false, multiplayerOnly: true)
	{
	}

	public override bool IsObtained()
	{
		int num = 0;
		foreach (SerializableCard item in _localPlayer.Deck)
		{
			if (SaveUtil.CardOrDeprecated(item.Id).MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly)
			{
				num++;
			}
		}
		return num >= 3;
	}
}
