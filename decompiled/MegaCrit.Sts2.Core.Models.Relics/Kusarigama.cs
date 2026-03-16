using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Kusarigama : RelicModel
{
	private bool _isActivating;

	private int _attacksPlayedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return AttacksPlayedThisTurn % base.DynamicVars.Cards.IntValue;
			}
			return base.DynamicVars.Cards.IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(3),
		new DamageVar(6m, ValueProp.Unpowered)
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

	private int AttacksPlayedThisTurn
	{
		get
		{
			return _attacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_attacksPlayedThisTurn = value;
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
			int intValue = base.DynamicVars.Cards.IntValue;
			base.Status = ((AttacksPlayedThisTurn % intValue == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public override Task BeforeCombatStart()
	{
		AttacksPlayedThisTurn = 0;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		AttacksPlayedThisTurn = 0;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner || !CombatManager.Instance.IsInProgress || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}
		AttacksPlayedThisTurn++;
		int intValue = base.DynamicVars.Cards.IntValue;
		if (AttacksPlayedThisTurn % intValue == 0)
		{
			Creature creature = base.Owner.RunState.Rng.CombatTargets.NextItem(base.Owner.Creature.CombatState.HittableEnemies);
			if (creature != null)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CreatureCmd.Damage(context, creature, base.DynamicVars.Damage, base.Owner.Creature);
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
		IsActivating = false;
		return Task.CompletedTask;
	}
}
