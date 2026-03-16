using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsFlesh : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override int DisplayAmount => base.Owner.Creature.CombatState?.RoundNumber ?? 1;

	public override bool ShowCounter
	{
		get
		{
			if (CombatManager.Instance.IsInProgress)
			{
				return base.Status == RelicStatus.Normal;
			}
			return false;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override Task BeforeCombatStart()
	{
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side && combatState.RoundNumber >= 3)
		{
			base.Status = RelicStatus.Active;
			InvokeDisplayAmountChanged();
			Flash();
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
