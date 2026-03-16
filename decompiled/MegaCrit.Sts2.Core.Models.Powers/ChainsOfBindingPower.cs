using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ChainsOfBindingPower : PowerModel
{
	private class Data
	{
		public bool boundCardPlayed;
	}

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Bound>();

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner == base.Owner.Player && base.CombatState.CurrentSide == base.Owner.Side && ModelDb.Affliction<Bound>().CanAfflict(card))
		{
			int num = CombatManager.Instance.History.Entries.OfType<CardAfflictedEntry>().Count((CardAfflictedEntry e) => e.HappenedThisTurn(base.CombatState) && e.Actor == base.Owner && e.Affliction is Bound);
			if (num < base.Amount)
			{
				await CardCmd.AfflictAndPreview<Bound>(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(card), base.Amount, CardPreviewStyle.None);
			}
		}
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.IsDupe)
		{
			return Task.CompletedTask;
		}
		if (card.Owner.Creature != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!(card.Affliction is Bound))
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().boundCardPlayed = true;
		return Task.CompletedTask;
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return true;
		}
		if (!(card.Affliction is Bound))
		{
			return true;
		}
		return !GetInternalData<Data>().boundCardPlayed;
	}

	public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		GetInternalData<Data>().boundCardPlayed = false;
		IEnumerable<CardModel> enumerable = base.Owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
		foreach (CardModel item in enumerable)
		{
			if (item.Affliction is Bound)
			{
				CardCmd.ClearAffliction(item);
			}
		}
		return Task.CompletedTask;
	}
}
