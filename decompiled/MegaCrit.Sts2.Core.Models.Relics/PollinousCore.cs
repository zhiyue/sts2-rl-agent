using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PollinousCore : RelicModel
{
	private const string _turnsKey = "Turns";

	private bool _isActivating;

	private int _turnsSeen;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return TurnsSeen;
			}
			return base.DynamicVars["Turns"].IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(2),
		new DynamicVar("Turns", 4m)
	});

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
			UpdateDisplay();
		}
	}

	[SavedProperty]
	public int TurnsSeen
	{
		get
		{
			return _turnsSeen;
		}
		set
		{
			AssertMutable();
			_turnsSeen = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			base.Status = RelicStatus.Normal;
		}
		else
		{
			int intValue = base.DynamicVars["Turns"].IntValue;
			base.Status = ((TurnsSeen == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		TurnsSeen++;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		if (TurnsSeen != base.DynamicVars["Turns"].IntValue)
		{
			return count;
		}
		return count + base.DynamicVars.Cards.BaseValue;
	}

	public override Task AfterModifyingHandDraw()
	{
		TurnsSeen = 0;
		TaskHelper.RunSafely(DoActivateVisuals());
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
