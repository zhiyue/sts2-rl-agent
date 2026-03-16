using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TangledPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		IEnumerable<CardModel> enumerable = base.Owner.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Type == CardType.Attack);
		foreach (CardModel item in enumerable)
		{
			await CardCmd.Afflict<Entangled>(item, 1m);
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner == base.Owner.Player && card.Affliction == null && card.Type == CardType.Attack)
		{
			await CardCmd.Afflict<Entangled>(card, 1m);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			Flash();
			await PowerCmd.Remove(this);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		IEnumerable<CardModel> enumerable = oldOwner.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Affliction is Entangled);
		foreach (CardModel item in enumerable)
		{
			CardCmd.ClearAffliction(item);
		}
		return Task.CompletedTask;
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		if (!(card.Affliction is Entangled) || card.Owner != base.Owner.Player)
		{
			modifiedCost = originalCost;
			return false;
		}
		modifiedCost = originalCost + (decimal)base.Amount;
		return true;
	}
}
