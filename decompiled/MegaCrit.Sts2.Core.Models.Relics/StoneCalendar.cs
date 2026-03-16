using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class StoneCalendar : RelicModel
{
	private const string _damageTurnKey = "DamageTurn";

	private bool _isActivating;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter => DisplayAmount > -1;

	public override int DisplayAmount
	{
		get
		{
			if (!CombatManager.Instance.IsInProgress)
			{
				return -1;
			}
			if (base.IsCanonical)
			{
				return -1;
			}
			int intValue = base.DynamicVars["DamageTurn"].IntValue;
			if (IsActivating)
			{
				return intValue;
			}
			int roundNumber = base.Owner.Creature.CombatState.RoundNumber;
			if (roundNumber >= intValue)
			{
				return -1;
			}
			return roundNumber;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			InvokeDisplayAmountChanged();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(52m, ValueProp.Unpowered),
		new DynamicVar("DamageTurn", 7m)
	});

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (combatState.RoundNumber == base.DynamicVars["DamageTurn"].IntValue)
		{
			base.Status = RelicStatus.Active;
		}
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Creature.Side)
		{
			int intValue = base.DynamicVars["DamageTurn"].IntValue;
			int roundNumber = base.Owner.Creature.CombatState.RoundNumber;
			base.Status = RelicStatus.Normal;
			if (roundNumber == intValue)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
				InvokeDisplayAmountChanged();
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
