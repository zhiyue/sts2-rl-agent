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

	/// <summary>
	/// Ethereal is granted globally for as long as this power exists, gated on the card being afflicted with
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Afflictions.Hexed" />. When this power is removed, it stops contributing here automatically, so any Ethereal from
	/// another source (e.g. <see cref="T:MegaCrit.Sts2.Core.Models.Relics.MusicBox" />) is left untouched.
	/// </summary>
	public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
	{
		if (card.Owner != base.Owner.Player)
		{
			return false;
		}
		if (!(card.Affliction is Hexed))
		{
			return false;
		}
		return keywords.Add(CardKeyword.Ethereal);
	}

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
			if (allCard.Affliction is Hexed)
			{
				CardCmd.ClearAffliction(allCard);
			}
		}
		return Task.CompletedTask;
	}

	private async Task Afflict(CardModel card)
	{
		if (card.Affliction == null)
		{
			await CardCmd.Afflict<Hexed>(card, base.Amount);
		}
	}
}
