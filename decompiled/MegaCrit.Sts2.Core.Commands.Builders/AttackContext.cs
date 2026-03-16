using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Commands.Builders;

public sealed class AttackContext : IAsyncDisposable
{
	private readonly CombatState _combatState;

	private readonly AttackCommand _attackCommand;

	private bool _disposed;

	private AttackContext(CombatState combatState, CardModel cardSource)
	{
		_combatState = combatState;
		_attackCommand = new AttackCommand(0m).FromCard(cardSource).TargetingAllOpponents(combatState);
	}

	public static async Task<AttackContext> CreateAsync(CombatState combatState, CardModel cardSource)
	{
		AttackContext context = new AttackContext(combatState, cardSource);
		await Hook.BeforeAttack(combatState, context._attackCommand);
		return context;
	}

	public void AddHit(IEnumerable<DamageResult> results)
	{
		_attackCommand.IncrementHitsInternal();
		_attackCommand.AddResultsInternal(results);
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		try
		{
			await Hook.AfterAttack(_combatState, _attackCommand);
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
	}
}
