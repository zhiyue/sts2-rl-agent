using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VitalSparkPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("AfflictionTitle", ModelDb.Affliction<Tainted>().Title.GetFormattedText()));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Tainted>(base.Amount);

	public override async Task BeforeCombatStart()
	{
		foreach (Creature item in base.Owner.CombatState.Allies.ToList())
		{
			if (!item.IsPlayer)
			{
				continue;
			}
			IEnumerable<CardModel> enumerable = item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Type == CardType.Skill);
			foreach (CardModel item2 in enumerable)
			{
				await CardCmd.Afflict<Tainted>(item2, base.Amount);
			}
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Affliction == null && card.Type == CardType.Skill)
		{
			await CardCmd.Afflict<Tainted>(card, base.Amount);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Affliction is Tainted)
		{
			Flash();
			await PowerCmd.Apply<TaintedPower>(choiceContext, cardPlay.Card.Owner.Creature, base.Amount, null, null);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		if (oldOwner.CombatState == null)
		{
			return Task.CompletedTask;
		}
		foreach (Creature item in oldOwner.CombatState.Allies.ToList())
		{
			if (!item.IsPlayer)
			{
				continue;
			}
			IEnumerable<CardModel> enumerable = item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Affliction is Tainted);
			foreach (CardModel item2 in enumerable)
			{
				CardCmd.ClearAffliction(item2);
			}
		}
		return Task.CompletedTask;
	}

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power != this)
		{
			return Task.CompletedTask;
		}
		foreach (Creature item in base.Owner.CombatState.Allies.ToList())
		{
			if (!item.IsPlayer)
			{
				continue;
			}
			IEnumerable<CardModel> enumerable = item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Affliction is Tainted);
			foreach (CardModel item2 in enumerable)
			{
				item2.Affliction.Amount = base.Amount;
			}
		}
		return Task.CompletedTask;
	}
}
