using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class PowerVar<T> : DynamicVar where T : PowerModel
{
	public PowerVar(decimal powerAmount)
		: base(typeof(T).Name, powerAmount)
	{
	}

	public PowerVar(string name, decimal powerAmount)
		: base(name, powerAmount)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		if (runGlobalHooks)
		{
			base.PreviewValue = Hook.ModifyPowerAmountGiven(card.CombatState, ModelDb.Power<T>(), card.Owner.Creature, base.BaseValue, target, card, out IEnumerable<AbstractModel> _);
		}
	}
}
