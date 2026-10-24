using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Won with a deck of 40, 60, or 100 or more cards.
/// </summary>
public class BigDeck : Badge
{
	public override BadgeRarity Rarity
	{
		get
		{
			int count = _localPlayer.Deck.Count;
			if (count >= 60)
			{
				if (count >= 100)
				{
					return BadgeRarity.Gold;
				}
				return BadgeRarity.Silver;
			}
			if (count >= 40)
			{
				return BadgeRarity.Bronze;
			}
			return BadgeRarity.None;
		}
	}

	/// <summary>
	/// Won with a deck of 40, 60, or 100 or more cards.
	/// </summary>
	public BigDeck(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "BIG_DECK", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		return Rarity != BadgeRarity.None;
	}
}
