using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FakeHappyFlower : RelicModel
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

	public override int MerchantCost => 50;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new EnergyVar(1),
		new DynamicVar("Turns", 5m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

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
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
			TurnsSeen = (TurnsSeen + 1) % base.DynamicVars["Turns"].IntValue;
			base.Status = ((TurnsSeen == base.DynamicVars["Turns"].IntValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
			if (TurnsSeen == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			}
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
