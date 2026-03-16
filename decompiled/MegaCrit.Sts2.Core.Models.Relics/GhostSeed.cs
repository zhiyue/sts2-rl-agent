using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GhostSeed : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (!CanAffect(card))
		{
			return Task.CompletedTask;
		}
		CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
		return Task.CompletedTask;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		IEnumerable<CardModel> allCards = base.Owner.PlayerCombatState.AllCards;
		foreach (CardModel item in allCards)
		{
			if (CanAffect(item))
			{
				CardCmd.ApplyKeyword(item, CardKeyword.Ethereal);
			}
		}
		return Task.CompletedTask;
	}

	public bool CanAffect(CardModel card)
	{
		if (card.Rarity == CardRarity.Basic && (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend)))
		{
			return !card.Keywords.Contains(CardKeyword.Ethereal);
		}
		return false;
	}
}
