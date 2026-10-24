using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Won with 5 or more Curses.
/// </summary>
public class Curses : Badge
{
	private const int _curseRequirement = 5;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Won with 5 or more Curses.
	/// </summary>
	public Curses(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "CURSES", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		int num = 0;
		foreach (SerializableCard item in _localPlayer.Deck)
		{
			if (SaveUtil.CardOrDeprecated(item.Id).Type == CardType.Curse)
			{
				num++;
			}
		}
		return num >= 5;
	}
}
