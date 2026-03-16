using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BurningSticks : RelicModel
{
	private bool _wasUsedThisCombat;

	public override RelicRarity Rarity => RelicRarity.Shop;

	private bool WasUsedThisCombat
	{
		get
		{
			return _wasUsedThisCombat;
		}
		set
		{
			AssertMutable();
			_wasUsedThisCombat = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		WasUsedThisCombat = false;
		base.Status = RelicStatus.Active;
		return Task.CompletedTask;
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner == base.Owner && !WasUsedThisCombat && card.Type == CardType.Skill)
		{
			Flash();
			CardModel card2 = card.CreateClone();
			await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, addedByPlayer: true);
			base.Status = RelicStatus.Normal;
			WasUsedThisCombat = true;
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		WasUsedThisCombat = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
