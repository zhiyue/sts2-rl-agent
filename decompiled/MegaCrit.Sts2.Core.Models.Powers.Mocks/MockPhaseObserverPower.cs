using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

/// <summary>
/// Test-only power that records <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.Phase" /> at the moment each turn lifecycle hook fires.
/// Tests should call <see cref="M:MegaCrit.Sts2.Core.Models.Powers.Mocks.MockPhaseObserverPower.ResetObservations" /> before each scenario and read <see cref="P:MegaCrit.Sts2.Core.Models.Powers.Mocks.MockPhaseObserverPower.Observations" /> after.
/// </summary>
public sealed class MockPhaseObserverPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public static List<(string Hook, PlayerTurnPhase Phase)> Observations { get; } = new List<(string, PlayerTurnPhase)>();

	/// <summary>
	/// Optional callback invoked during <see cref="M:MegaCrit.Sts2.Core.Models.Powers.Mocks.MockPhaseObserverPower.Record(System.String,MegaCrit.Sts2.Core.Entities.Players.Player)" />. Useful for multiplayer tests that need to
	/// inspect other players' phases at the moment a hook fires.
	/// </summary>
	public static Action<string, Player>? OnRecordCallback { get; set; }

	public static void ResetObservations()
	{
		Observations.Clear();
		OnRecordCallback = null;
	}

	public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		Record("BeforeHandDraw", player);
		return Task.CompletedTask;
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		Record("AfterPlayerTurnStart", player);
		return Task.CompletedTask;
	}

	public override Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		Record("AfterAutoPrePlayPhaseEntered", player);
		return Task.CompletedTask;
	}

	public override Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		Record("AfterAutoPostPlayPhaseEntered", player);
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		Record("BeforeSideTurnEnd", base.Owner.Player);
		return Task.CompletedTask;
	}

	public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		Record("BeforeFlush", player);
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		Record("AfterSideTurnEnd", base.Owner.Player);
		return Task.CompletedTask;
	}

	private void Record(string hook, Player player)
	{
		Observations.Add((hook, player.PlayerCombatState.Phase));
		OnRecordCallback?.Invoke(hook, player);
	}
}
