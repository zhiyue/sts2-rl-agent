using System;
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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public class Metronome : RelicModel
{
	private const string _orbCountKey = "OrbCount";

	private bool _isActivating;

	private int _orbsChanneled;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter
	{
		get
		{
			if (CombatManager.Instance.IsInProgress)
			{
				if (OrbsChanneled >= base.DynamicVars["OrbCount"].IntValue)
				{
					return IsActivating;
				}
				return true;
			}
			return false;
		}
	}

	public override int DisplayAmount => Math.Min(OrbsChanneled, base.DynamicVars["OrbCount"].IntValue);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(30m, ValueProp.Unpowered),
		new DynamicVar("OrbCount", 7m)
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

	private int OrbsChanneled
	{
		get
		{
			return _orbsChanneled;
		}
		set
		{
			AssertMutable();
			_orbsChanneled = value;
			UpdateDisplay();
		}
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		OrbsChanneled = 0;
		UpdateDisplay();
		return Task.CompletedTask;
	}

	public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
	{
		if (player == base.Owner)
		{
			OrbsChanneled++;
			if (OrbsChanneled == base.DynamicVars["OrbCount"].IntValue)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		OrbsChanneled = 0;
		UpdateDisplay();
		return Task.CompletedTask;
	}

	private void UpdateDisplay()
	{
		int intValue = base.DynamicVars["OrbCount"].IntValue;
		if (OrbsChanneled == intValue - 1 && !IsActivating)
		{
			base.Status = RelicStatus.Active;
		}
		else
		{
			base.Status = RelicStatus.Normal;
		}
		InvokeDisplayAmountChanged();
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
