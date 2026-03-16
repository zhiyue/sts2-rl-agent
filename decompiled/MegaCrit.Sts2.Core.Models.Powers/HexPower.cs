using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HexPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Hexed>(base.Amount);

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		foreach (CardModel allCard in base.Owner.Player.PlayerCombatState.AllCards)
		{
			await Afflict(allCard);
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner == base.Owner.Player)
		{
			await Afflict(card);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented && creature == base.Applier)
		{
			await PowerCmd.Remove(this);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		foreach (CardModel allCard in base.Owner.Player.PlayerCombatState.AllCards)
		{
			if (allCard.Affliction is Hexed hexed)
			{
				if (hexed.AppliedEthereal)
				{
					CardCmd.RemoveKeyword(allCard, CardKeyword.Ethereal);
				}
				CardCmd.ClearAffliction(allCard);
			}
		}
		return Task.CompletedTask;
	}

	private async Task Afflict(CardModel card)
	{
		if (card.Affliction == null)
		{
			Hexed hexed = await CardCmd.Afflict<Hexed>(card, base.Amount);
			if (hexed != null && !card.Keywords.Contains(CardKeyword.Ethereal))
			{
				CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
				hexed.AppliedEthereal = true;
			}
		}
	}
}
