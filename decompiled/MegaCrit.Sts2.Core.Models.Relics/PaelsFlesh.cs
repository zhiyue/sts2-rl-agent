using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsFlesh : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override int DisplayAmount => base.Owner.PlayerCombatState?.TurnNumber ?? 1;

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

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		if (player.PlayerCombatState.TurnNumber < 3)
		{
			return amount;
		}
		return amount + base.DynamicVars.Energy.BaseValue;
	}

	public override Task BeforeCombatStart()
	{
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		if (base.Owner.PlayerCombatState.TurnNumber < 3)
		{
			return Task.CompletedTask;
		}
		if (base.Status == RelicStatus.Active)
		{
			return Task.CompletedTask;
		}
		base.Status = RelicStatus.Active;
		InvokeDisplayAmountChanged();
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
