using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Deck contains 5+ of the same card
/// </summary>
public class Honed : Badge
{
	private const int _sameCardAmount = 5;

	public override BadgeRarity Rarity => BadgeRarity.Bronze;

	/// <summary>
	/// Deck contains 5+ of the same card
	/// </summary>
	public Honed(SerializableRun run, bool won, ulong playerId)
		: base(run, won, playerId, "HONED", requiresWin: true, multiplayerOnly: false)
	{
	}

	public override bool IsObtained()
	{
		List<SerializableCard> source = _localPlayer.Deck.Where((SerializableCard c) => SaveUtil.CardOrDeprecated(c.Id).Rarity != CardRarity.Basic).ToList();
		return (from c in source
			group c by c.Id).Any((IGrouping<ModelId, SerializableCard> g) => g.Count() >= 5);
	}
}
