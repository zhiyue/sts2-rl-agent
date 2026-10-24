using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Commands.Builders;

/// <summary>
/// A context for grouping multiple damage calls as a single attack for hook purposes.
/// Use with C#'s `await using` statement to automatically call BeforeAttack and AfterAttack hooks.
/// </summary>
public sealed class AttackContext : IAsyncDisposable
{
	private readonly ICombatState _combatState;

	private readonly PlayerChoiceContext _choiceContext;

	private readonly AttackCommand _attackCommand;

	private bool _disposed;

	/// <summary>
	/// Private constructor. Use <see cref="M:MegaCrit.Sts2.Core.Commands.Builders.AttackContext.CreateAsync(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.CardModel)" /> to create instances.
	/// </summary>
	private AttackContext(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel cardSource)
	{
		_combatState = combatState;
		_choiceContext = choiceContext;
		_attackCommand = new AttackCommand(0m).FromCard(cardSource).TargetingAllOpponents(combatState);
	}

	/// <summary>
	/// Creates a new AttackContext and calls the BeforeAttack hook.
	/// </summary>
	/// <param name="combatState">The current combat state</param>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="cardSource">The card that is the source of this attack</param>
	/// <returns>An initialized AttackContext ready for use with await using</returns>
	public static async Task<AttackContext> CreateAsync(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel cardSource)
	{
		AttackContext context = new AttackContext(combatState, choiceContext, cardSource);
		await Hook.BeforeAttack(combatState, context._attackCommand);
		return context;
	}

	/// <summary>
	/// Add to the list of hits that have been done within the context of this attack.
	/// </summary>
	/// <param name="results">DamageResults from the hit.</param>
	public void AddHit(IEnumerable<DamageResult> results)
	{
		_attackCommand.IncrementHitsInternal();
		_attackCommand.AddResultsInternal(results);
	}

	/// <summary>
	/// Disposes the context and calls the AfterAttack hook.
	/// This is automatically called when exiting an await using block.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		try
		{
			await Hook.AfterAttack(_combatState, _choiceContext, _attackCommand);
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
	}
}
