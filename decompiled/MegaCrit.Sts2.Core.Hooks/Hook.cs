using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Hooks;

/// <summary>
/// A static class containing all of the gameplay hooks.
/// </summary>
public static class Hook
{
	/// <summary>
	/// Iterates combat hook listeners, but yields nothing if combat is over or ending
	/// (CombatManager.IsOverOrEnding) when the dispatch begins. Most combat hooks should use this
	/// instead of <see cref="M:MegaCrit.Sts2.Core.Combat.ICombatState.IterateHookListeners" /> directly, so a hook dispatched
	/// after combat has started ending fires for no one (for example after a Strike that Hellraiser
	/// plays automatically kills the last enemy while cards are still being drawn).
	///
	/// The check is evaluated once, when enumeration begins, not per listener. A dispatch that
	/// begins while combat is live therefore runs every listener even if one of them ends combat
	/// partway through. That is intentional: combat teardown is deferred to the next safe point
	/// (CheckWinCondition), so the state stays intact for the rest of the dispatch, and rechecking
	/// per listener would drop the remaining ones in listener order (for example a Joss Paper
	/// increment that should still count when Charon's Ashes lands the killing blow on an exhaust).
	///
	/// Combat setup (CombatManager.IsStarting) is exempt: IsInProgress is still false then, so
	/// IsOverOrEnding is true, but hooks that run during setup, such as the initial deck shuffle
	/// (ModifyShuffleOrder with isInitialShuffle true), must still reach listeners.
	///
	/// A few hooks intentionally call <see cref="M:MegaCrit.Sts2.Core.Combat.ICombatState.IterateHookListeners" /> directly
	/// because they are part of the kill, death, or combat end sequence itself, where yielding
	/// nothing would break that sequence. Each such hook documents the reason in its own summary.
	/// </summary>
	private static IEnumerable<AbstractModel> IterateCombatHookListeners(ICombatState combatState)
	{
		if (CombatManager.Instance.IsOverOrEnding && !CombatManager.Instance.IsStarting)
		{
			yield break;
		}
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			yield return item;
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterActEntered" />.
	/// </summary>
	public static async Task AfterActEntered(IRunState runState)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterActEntered();
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeAttack(MegaCrit.Sts2.Core.Commands.Builders.AttackCommand)" />.
	/// </summary>
	public static async Task BeforeAttack(ICombatState combatState, AttackCommand command)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.BeforeAttack(command);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterAttack(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Commands.Builders.AttackCommand)" />.
	/// </summary>
	public static async Task AfterAttack(ICombatState combatState, PlayerChoiceContext choiceContext, AttackCommand command)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterAttack(choiceContext, command);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterBlockBroken(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it fires from the same
	/// damage event that ends combat (the killing hit), so it must still resolve for that hit.
	/// </summary>
	public static async Task AfterBlockBroken(ICombatState combatState, Creature creature)
	{
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			await model.AfterBlockBroken(creature);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterBlockCleared(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task AfterBlockCleared(ICombatState combatState, Creature creature)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterBlockCleared(creature);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeBlockGained(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task BeforeBlockGained(ICombatState combatState, Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.BeforeBlockGained(creature, amount, props, cardSource);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterBlockGained(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterBlockGained(ICombatState combatState, Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterBlockGained(creature, amount, props, cardSource);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeCardAutoPlayed(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Entities.Cards.AutoPlayType)" />.
	/// </summary>
	public static async Task BeforeCardAutoPlayed(ICombatState combatState, CardModel card, Creature? target, AutoPlayType type)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.BeforeCardAutoPlayed(card, target, type);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardChangedPiles(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.PileType,MegaCrit.Sts2.Core.Models.AbstractModel)" />.
	/// </summary>
	public static async Task AfterCardChangedPiles(IRunState runState, ICombatState? combatState, CardModel card, PileType oldPile, AbstractModel? clonedBy)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCardChangedPiles(card, oldPile, clonedBy);
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCardChangedPilesLate(card, oldPile, clonedBy);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardDiscarded(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterCardDiscarded(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel card)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterCardDiscarded(choiceContext, card);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardDrawn(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.CardModel,System.Boolean)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterCardDrawn(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterCardDrawnEarly(choiceContext, card, fromHandDraw);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterCardDrawn(choiceContext, card, fromHandDraw);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardEnteredCombat(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterCardEnteredCombat(ICombatState combatState, CardModel card)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterCardEnteredCombat(card);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardExhausted(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.CardModel,System.Boolean)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterCardExhausted(ICombatState combatState, PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterCardExhausted(choiceContext, card, causedByEthereal);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardGeneratedForCombat(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterCardGeneratedForCombat(ICombatState combatState, CardModel card, Player? creator)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterCardGeneratedForCombat(card, creator);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeCardPlayed(MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	/// </summary>
	public static async Task BeforeCardPlayed(ICombatState combatState, CardPlay cardPlay)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.BeforeCardPlayed(cardPlay);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCardPlayed(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it completes
	/// resolution of the card that caused the kill.
	/// </summary>
	public static async Task AfterCardPlayed(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			choiceContext.PushModel(model);
			await model.AfterCardPlayed(choiceContext, cardPlay);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			choiceContext.PushModel(model);
			await model.AfterCardPlayedLate(choiceContext, cardPlay);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeCardRemoved(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task BeforeCardRemoved(IRunState runState, CardModel card)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.BeforeCardRemoved(card);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeCombatStart" />.
	/// </summary>
	public static async Task BeforeCombatStart(IRunState runState, ICombatState? combatState)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.BeforeCombatStart();
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.BeforeCombatStartLate();
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom)" />.
	/// </summary>
	public static async Task AfterCombatEnd(IRunState runState, ICombatState? combatState, CombatRoom room)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCombatEnd(room);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCombatVictory(MegaCrit.Sts2.Core.Rooms.CombatRoom)" />.
	/// </summary>
	public static async Task AfterCombatVictory(IRunState runState, ICombatState? combatState, CombatRoom room)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCombatVictoryEarly(room);
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCombatVictory(room);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCreatureAddedToCombat(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it only fires for
	/// creatures added during combat (creatures present when combat starts take a different path,
	/// before IsInProgress is set), and a creature added while combat is ending can affect whether
	/// it should actually end, so that decision is not overridden here.
	/// </summary>
	public static async Task AfterCreatureAddedToCombat(ICombatState combatState, Creature creature)
	{
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			await model.AfterCreatureAddedToCombat(creature);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterCurrentHpChanged(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal)" />.
	/// </summary>
	public static async Task AfterCurrentHpChanged(IRunState runState, ICombatState? combatState, Creature creature, decimal delta)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterCurrentHpChanged(creature, delta);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterDamageGiven(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Entities.Creatures.DamageResult,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it fires from the same
	/// damage event that ends combat (the killing hit), so it must still resolve for that hit.
	/// </summary>
	public static async Task AfterDamageGiven(PlayerChoiceContext choiceContext, ICombatState combatState, Creature? dealer, DamageResult results, ValueProp props, Creature target, CardModel? cardSource)
	{
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			choiceContext.PushModel(model);
			await model.AfterDamageGiven(choiceContext, dealer, results, props, target, cardSource);
			choiceContext.PopModel(model);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeDamageReceived(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, IRunState runState, ICombatState? combatState, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.BeforeDamageReceived(choiceContext, target, amount, props, dealer, cardSource);
			choiceContext.PopModel(model);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterDamageReceived(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Entities.Creatures.DamageResult,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterDamageReceived(PlayerChoiceContext choiceContext, IRunState runState, ICombatState? combatState, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
			choiceContext.PopModel(model);
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterDamageReceivedLate(choiceContext, target, result, props, dealer, cardSource);
			choiceContext.PopModel(model);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeDeath(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task BeforeDeath(IRunState runState, ICombatState? combatState, Creature creature)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.BeforeDeath(creature);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterDeath(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Boolean,System.Single)" />.
	/// </summary>
	public static async Task AfterDeath(IRunState runState, ICombatState? combatState, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(model, netId.Value, creature.CombatState, GameActionType.Combat);
			Task task = model.AfterDeath(hookPlayerChoiceContext, creature, wasRemovalPrevented, deathAnimLength);
			await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterGoldGained(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterGoldGained(IRunState runState, Player player)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterGoldGained(player);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterDiedToDoom(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it runs during death
	/// resolution, which proceeds while combat is ending.
	/// </summary>
	public static async Task AfterDiedToDoom(ICombatState combatState, IReadOnlyList<Creature> creatures)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		foreach (AbstractModel model in combatState.IterateHookListeners())
		{
			HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(model, netId.Value, combatState, GameActionType.Combat);
			Task task = model.AfterDiedToDoom(hookPlayerChoiceContext, creatures);
			await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterEnergyReset(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterEnergyReset(ICombatState combatState, Player player)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterEnergyReset(player);
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterEnergyResetLate(player);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterEnergySpent(MegaCrit.Sts2.Core.Models.CardModel,System.Int32)" />.
	/// </summary>
	public static async Task AfterEnergySpent(ICombatState combatState, CardModel card, int amount)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterEnergySpent(card, amount);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeFlush(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task BeforeFlush(ICombatState combatState, Player player)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		List<Task> tasksToAwait = new List<Task>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, netId.Value, player.Creature.CombatState, GameActionType.Combat);
			Task task = item.BeforeFlush(playerChoiceContext, player);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item2, netId.Value, player.Creature.CombatState, GameActionType.Combat);
			Task task2 = item2.BeforeFlushLate(playerChoiceContext, player);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		await Task.WhenAll(tasksToAwait);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterFlush(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IReadOnlyCollection{MegaCrit.Sts2.Core.Models.CardModel},System.Collections.Generic.IReadOnlyCollection{MegaCrit.Sts2.Core.Models.CardModel})" />.
	/// </summary>
	public static async Task AfterFlush(ICombatState combatState, Player player, PlayerChoiceContext playerChoiceContext, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.AfterFlush(playerChoiceContext, player, flushedCards, retainedCards);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterForge(System.Decimal,MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Models.AbstractModel)" />.
	/// </summary>
	public static async Task AfterForge(ICombatState combatState, decimal amount, Player forger, AbstractModel? source)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterForge(amount, forger, source);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeHandDraw(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Combat.ICombatState)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task BeforeHandDraw(ICombatState combatState, Player player, PlayerChoiceContext playerChoiceContext)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.BeforeHandDraw(player, playerChoiceContext, combatState);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.BeforeHandDrawLate(player, playerChoiceContext, combatState);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterHandEmptied(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterHandEmptied(ICombatState combatState, PlayerChoiceContext choiceContext, Player player)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterHandEmptied(choiceContext, player);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterItemPurchased(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Entities.Merchant.MerchantEntry,System.Int32)" />.
	/// </summary>
	public static async Task AfterItemPurchased(IRunState runState, Player player, MerchantEntry itemPurchased, int goldSpent)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterItemPurchased(player, itemPurchased, goldSpent);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterMapGenerated(MegaCrit.Sts2.Core.Map.ActMap,System.Int32)" />.
	/// </summary>
	public static async Task AfterMapGenerated(IRunState runState, ActMap map, int actIndex)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterMapGenerated(map, actIndex);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingBlockAmount(System.Decimal,MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	/// </summary>
	public static async Task AfterModifyingBlockAmount(ICombatState combatState, decimal modifiedBlock, CardModel? cardSource, CardPlay? cardPlay, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingBlockAmount(modifiedBlock, cardSource, cardPlay);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingCardPlayCount(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterModifyingCardPlayCount(ICombatState combatState, CardModel card, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingCardPlayCount(card);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingCardRewardOptions" />.
	/// </summary>
	public static async Task AfterModifyingCardRewardOptions(IRunState runState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(null))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingCardRewardOptions();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingDamageAmount(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterModifyingDamageAmount(IRunState runState, ICombatState? combatState, CardModel? cardSource, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingDamageAmount(cardSource);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingEnergyGain" />.
	/// </summary>
	public static async Task AfterModifyingEnergyGain(ICombatState combatState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingEnergyGain();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingGoldGained(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// </summary>
	public static async Task AfterModifyingGoldGained(IRunState runState, ICombatState? combatState, IEnumerable<AbstractModel> modifiers, Player player, decimal amount)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingGoldGained(player, amount);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingHandDraw" />.
	/// </summary>
	public static async Task AfterModifyingHandDraw(ICombatState combatState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingHandDraw();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingHpLostBeforeOsty" />.
	/// </summary>
	public static async Task AfterModifyingHpLostBeforeOsty(IRunState runState, ICombatState? combatState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingHpLostBeforeOsty();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingHpLostAfterOsty" />.
	/// </summary>
	public static async Task AfterModifyingHpLostAfterOsty(IRunState runState, ICombatState? combatState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingHpLostAfterOsty();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingOrbPassiveTriggerCount(MegaCrit.Sts2.Core.Models.OrbModel)" />.
	/// </summary>
	public static async Task AfterModifyingOrbPassiveTriggerCount(ICombatState combatState, OrbModel orb, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingOrbPassiveTriggerCount(orb);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingPowerAmountGiven(MegaCrit.Sts2.Core.Models.PowerModel)" />.
	/// </summary>
	public static async Task AfterModifyingPowerAmountGiven(ICombatState combatState, IEnumerable<AbstractModel> modifiers, PowerModel modifiedPower)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingPowerAmountGiven(modifiedPower);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingPowerAmountReceived(MegaCrit.Sts2.Core.Models.PowerModel)" />.
	/// </summary>
	public static async Task AfterModifyingPowerAmountReceived(ICombatState combatState, IEnumerable<AbstractModel> modifiers, PowerModel modifiedPower)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingPowerAmountReceived(modifiedPower);
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterModifyingRewards" />.
	/// </summary>
	public static async Task AfterModifyingRewards(IRunState runState, IEnumerable<AbstractModel> modifiers)
	{
		foreach (AbstractModel modifier in runState.IterateHookListeners(null))
		{
			if (modifiers.Contains(modifier))
			{
				await modifier.AfterModifyingRewards();
				modifier.InvokeExecutionFinished();
			}
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterOrbChanneled(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Models.OrbModel)" />.
	/// </summary>
	public static async Task AfterOrbChanneled(ICombatState combatState, PlayerChoiceContext choiceContext, Player player, OrbModel orb)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterOrbChanneled(choiceContext, player, orb);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterOrbEvoked(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.OrbModel,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />.
	/// </summary>
	public static async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, ICombatState combatState, OrbModel orb, IEnumerable<Creature> targets)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterOrbEvoked(choiceContext, orb, targets);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterOstyRevived(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task AfterOstyRevived(ICombatState combatState, Creature osty)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterOstyRevived(osty);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPlayerTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterPlayerTurnStart(ICombatState combatState, PlayerChoiceContext choiceContext, Player player)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterPlayerTurnStartEarly(choiceContext, player);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterPlayerTurnStart(choiceContext, player);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterPlayerTurnStartLate(choiceContext, player);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterAutoPostPlayPhaseEntered(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterAutoPostPlayPhaseEntered(HookPlayerChoiceContext playerChoiceContext, ICombatState combatState, Player player)
	{
		if (!LocalContext.NetId.HasValue)
		{
			return;
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.AfterAutoPostPlayPhaseEntered(playerChoiceContext, player);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterAutoPrePlayPhaseEntered(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterAutoPrePlayPhaseEntered(HookPlayerChoiceContext playerChoiceContext, ICombatState combatState, Player player)
	{
		if (!LocalContext.NetId.HasValue)
		{
			return;
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.AfterAutoPrePlayPhaseEnteredEarly(playerChoiceContext, player);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.AfterAutoPrePlayPhaseEntered(playerChoiceContext, player);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			playerChoiceContext.PushModel(model);
			await model.AfterAutoPrePlayPhaseEnteredLate(playerChoiceContext, player);
			model.InvokeExecutionFinished();
			playerChoiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPotionDiscarded(MegaCrit.Sts2.Core.Models.PotionModel)" />.
	/// </summary>
	public static async Task AfterPotionDiscarded(IRunState runState, ICombatState? combatState, PotionModel potion)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterPotionDiscarded(potion);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPotionProcured(MegaCrit.Sts2.Core.Models.PotionModel)" />.
	/// </summary>
	public static async Task AfterPotionProcured(IRunState runState, ICombatState? combatState, PotionModel potion)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterPotionProcured(potion);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforePotionUsed(MegaCrit.Sts2.Core.Models.PotionModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task BeforePotionUsed(IRunState runState, ICombatState? combatState, PotionModel potion, Creature? target)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.BeforePotionUsed(potion, target);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPotionUsed(MegaCrit.Sts2.Core.Models.PotionModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task AfterPotionUsed(IRunState runState, ICombatState? combatState, PotionModel potion, Creature? target)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(combatState))
		{
			await model.AfterPotionUsed(potion, target);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforePowerAmountChanged(MegaCrit.Sts2.Core.Models.PowerModel,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task BeforePowerAmountChanged(ICombatState combatState, PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
	{
		foreach (AbstractModel modifier in IterateCombatHookListeners(combatState))
		{
			await modifier.BeforePowerAmountChanged(power, amount, target, applier, cardSource);
			modifier.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPowerAmountChanged(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Models.PowerModel,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static async Task AfterPowerAmountChanged(ICombatState combatState, PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPreventingBlockClear(MegaCrit.Sts2.Core.Models.AbstractModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task AfterPreventingBlockClear(ICombatState combatState, AbstractModel preventer, Creature creature)
	{
		if (IterateCombatHookListeners(combatState).Contains(preventer))
		{
			await preventer.AfterPreventingBlockClear(preventer, creature);
			preventer.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPreventingDeath(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static async Task AfterPreventingDeath(IRunState runState, ICombatState? combatState, AbstractModel preventer, Creature creature)
	{
		if (runState.IterateHookListeners(combatState).Contains(preventer))
		{
			await preventer.AfterPreventingDeath(creature);
			preventer.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPreventingDraw" />.
	/// </summary>
	public static async Task AfterPreventingDraw(ICombatState combatState, AbstractModel modifier)
	{
		if (IterateCombatHookListeners(combatState).Contains(modifier))
		{
			await modifier.AfterPreventingDraw();
			modifier.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRestSiteHeal(MegaCrit.Sts2.Core.Entities.Players.Player,System.Boolean)" />.
	/// </summary>
	public static async Task AfterRestSiteHeal(IRunState runState, Player player, bool isMimicked)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterRestSiteHeal(player, isMimicked);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRestSiteSmith(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterRestSiteSmith(IRunState runState, Player player)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterRestSiteSmith(player);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRewardTaken(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Rewards.Reward)" />.
	/// </summary>
	public static async Task AfterRewardTaken(IRunState runState, Player player, Reward reward)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterRewardTaken(player, reward);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeRoomEntered(MegaCrit.Sts2.Core.Rooms.AbstractRoom)" />.
	/// </summary>
	public static async Task BeforeRoomEntered(IRunState runState, AbstractRoom room)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.BeforeRoomEntered(room);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRoomEntered(MegaCrit.Sts2.Core.Rooms.AbstractRoom)" />.
	/// </summary>
	public static async Task AfterRoomEntered(IRunState runState, AbstractRoom room)
	{
		foreach (AbstractModel model in runState.IterateHookListeners(null))
		{
			await model.AfterRoomEntered(room);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterShuffle(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterShuffle(ICombatState combatState, PlayerChoiceContext choiceContext, Player shuffler)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterShuffle(choiceContext, shuffler);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeSideTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" />.
	/// </summary>
	public static async Task BeforeSideTurnStart(ICombatState combatState, CombatSide side, IReadOnlyList<Creature> participants)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(model, netId.Value, combatState, GameActionType.Combat);
			Task task = model.BeforeSideTurnStart(hookPlayerChoiceContext, side, participants, combatState);
			await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" />.
	/// </summary>
	public static async Task AfterSideTurnStart(ICombatState combatState, CombatSide side, IReadOnlyList<Creature> participants)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterSideTurnStart(side, participants, combatState);
			model.InvokeExecutionFinished();
		}
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterSideTurnStartLate(side, participants, combatState);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterStarsGained(System.Int32,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterStarsGained(ICombatState combatState, int amount, Player gainer)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterStarsGained(amount, gainer);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterStarsSpent(System.Int32,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterStarsSpent(ICombatState combatState, int amount, Player spender)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterStarsSpent(amount, spender);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSummon(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// This takes a player choice context as an argument because it needs to block combat flow if a player choice is
	/// encountered.
	/// </summary>
	public static async Task AfterSummon(ICombatState combatState, PlayerChoiceContext choiceContext, Player summoner, decimal amount)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			choiceContext.PushModel(model);
			await model.AfterSummon(choiceContext, summoner, amount);
			model.InvokeExecutionFinished();
			choiceContext.PopModel(model);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterTakingExtraTurn(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static async Task AfterTakingExtraTurn(ICombatState combatState, Player player)
	{
		foreach (AbstractModel model in IterateCombatHookListeners(combatState))
		{
			await model.AfterTakingExtraTurn(player);
			model.InvokeExecutionFinished();
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.BeforeSideTurnEnd(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />.
	/// </summary>
	public static async Task BeforeTurnEnd(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		List<Task> tasksToAwait = new List<Task>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, netId.Value, combatState, GameActionType.Combat);
			Task task = item.BeforeSideTurnEndVeryEarly(playerChoiceContext, side, participants);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item2, netId.Value, combatState, GameActionType.Combat);
			Task task2 = item2.BeforeSideTurnEndEarly(playerChoiceContext, side, participants);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		foreach (AbstractModel item3 in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item3, netId.Value, combatState, GameActionType.Combat);
			Task task3 = item3.BeforeSideTurnEnd(playerChoiceContext, side, participants);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task3);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		await Task.WhenAll(tasksToAwait);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnEnd(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" />.
	/// </summary>
	public static async Task AfterTurnEnd(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
	{
		ulong? netId = LocalContext.NetId;
		if (!netId.HasValue)
		{
			return;
		}
		List<Task> tasksToAwait = new List<Task>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, netId.Value, combatState, GameActionType.Combat);
			Task task = item.AfterSideTurnEnd(playerChoiceContext, side, participants);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		await Task.WhenAll(tasksToAwait);
		tasksToAwait.Clear();
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item2, netId.Value, combatState, GameActionType.Combat);
			Task task2 = item2.AfterSideTurnEndLate(playerChoiceContext, side, participants);
			await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
			tasksToAwait.Add(playerChoiceContext.WaitForCompletion());
		}
		await Task.WhenAll(tasksToAwait);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyAttackHitCount(MegaCrit.Sts2.Core.Commands.Builders.AttackCommand,System.Int32)" />.
	/// </summary>
	public static decimal ModifyAttackHitCount(ICombatState combatState, AttackCommand attackCommand, int originalHitCount)
	{
		int num = originalHitCount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			num = item.ModifyAttackHitCount(attackCommand, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyBlockAdditive(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> and <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyBlockMultiplicative(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	/// </summary>
	public static decimal ModifyBlock(ICombatState combatState, Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay, out IEnumerable<AbstractModel> modifiers)
	{
		List<AbstractModel> list = new List<AbstractModel>();
		decimal num = block;
		if (cardSource != null && cardSource.Enchantment != null)
		{
			EnchantmentModel enchantment = cardSource.Enchantment;
			num += enchantment.EnchantBlockAdditive(num);
			num *= enchantment.EnchantBlockMultiplicative(num);
		}
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			decimal num2 = item.ModifyBlockAdditive(target, num, props, cardSource, cardPlay);
			num += num2;
			if (num2 != 0m)
			{
				list.Add(item);
			}
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			decimal num3 = item2.ModifyBlockMultiplicative(target, num, props, cardSource, cardPlay);
			num *= num3;
			if (num3 != 1m)
			{
				list.Add(item2);
			}
		}
		modifiers = list;
		return Math.Max(0m, num);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyCardBeingAddedToDeck(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Models.CardModel@)" />.
	/// </summary>
	public static CardModel ModifyCardBeingAddedToDeck(IRunState runState, CardModel card, out List<AbstractModel> modifyingModels)
	{
		modifyingModels = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.TryModifyCardBeingAddedToDeck(card, out CardModel newCard) && newCard != null)
			{
				modifyingModels.Add(item);
				card = newCard;
			}
			item.InvokeExecutionFinished();
		}
		foreach (AbstractModel item2 in runState.IterateHookListeners(null))
		{
			if (item2.TryModifyCardBeingAddedToDeckLate(card, out CardModel newCard2) && newCard2 != null)
			{
				modifyingModels.Add(item2);
				card = newCard2;
			}
			item2.InvokeExecutionFinished();
		}
		return card;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardPlayCount(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Int32)" />.
	/// </summary>
	public static int ModifyCardPlayCount(ICombatState combatState, CardModel card, int playCount, Creature? target, out List<AbstractModel> modifyingModels)
	{
		modifyingModels = new List<AbstractModel>();
		int num = playCount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			int num2 = num;
			num = item.ModifyCardPlayCount(card, target, num);
			if (num != num2)
			{
				modifyingModels.Add(item);
			}
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardPlayResultPileTypeAndPosition(MegaCrit.Sts2.Core.Models.CardModel,System.Boolean,MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo,MegaCrit.Sts2.Core.Entities.Cards.PileType,MegaCrit.Sts2.Core.Entities.Cards.CardPilePosition)" />.
	/// </summary>
	public static (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(ICombatState combatState, CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position, out IEnumerable<AbstractModel> modifiers)
	{
		PileType pileType2 = pileType;
		CardPilePosition cardPilePosition = position;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			PileType pileType3 = pileType2;
			CardPilePosition cardPilePosition2 = cardPilePosition;
			(pileType2, cardPilePosition) = item.ModifyCardPlayResultPileTypeAndPosition(card, isAutoPlay, resources, pileType2, cardPilePosition);
			if (pileType3 != pileType2 || cardPilePosition2 != cardPilePosition)
			{
				list.Add(item);
			}
		}
		modifiers = list;
		return (pileType2, cardPilePosition);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyCardRewardAlternatives(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Rewards.CardReward,System.Collections.Generic.List{MegaCrit.Sts2.Core.Entities.CardRewardAlternatives.CardRewardAlternative})" />.
	/// </summary>
	public static IEnumerable<AbstractModel> ModifyCardRewardAlternatives(IRunState runState, Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
	{
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.TryModifyCardRewardAlternatives(player, cardReward, alternatives))
			{
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardRewardCreationOptions(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Runs.CardCreationOptions)" />.
	/// </summary>
	public static CardCreationOptions ModifyCardRewardCreationOptions(IRunState runState, Player player, CardCreationOptions options)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			options = item.ModifyCardRewardCreationOptions(player, options);
		}
		foreach (AbstractModel item2 in runState.IterateHookListeners(null))
		{
			options = item2.ModifyCardRewardCreationOptionsLate(player, options);
		}
		return options;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyCardRewardOptions(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.List{MegaCrit.Sts2.Core.Entities.Cards.CardCreationResult},MegaCrit.Sts2.Core.Runs.CardCreationOptions)" />.
	/// </summary>
	public static bool TryModifyCardRewardOptions(IRunState runState, Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions, out List<AbstractModel> modifiers)
	{
		bool flag = false;
		modifiers = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			bool flag2 = item.TryModifyCardRewardOptions(player, cardRewardOptions, creationOptions);
			flag = flag || flag2;
			if (flag2)
			{
				modifiers.Add(item);
			}
		}
		foreach (AbstractModel item2 in runState.IterateHookListeners(null))
		{
			bool flag3 = item2.TryModifyCardRewardOptionsLate(player, cardRewardOptions, creationOptions);
			flag = flag || flag3;
			if (flag3)
			{
				modifiers.Add(item2);
			}
		}
		return flag;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardRewardUpgradeOdds(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Models.CardModel,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyCardRewardUpgradeOdds(IRunState runState, Player player, CardModel card, decimal originalOdds)
	{
		decimal num = originalOdds;
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			num = item.ModifyCardRewardUpgradeOdds(player, card, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageAdditive(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static decimal ModifyDamage(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType modifyDamageHookType, CardPreviewMode previewMode, out IEnumerable<AbstractModel> modifiers)
	{
		List<AbstractModel> modifiers2 = new List<AbstractModel>();
		decimal num = damage;
		if (cardSource != null && cardSource.Enchantment != null)
		{
			if (modifyDamageHookType.HasFlag(ModifyDamageHookType.Additive))
			{
				num += cardSource.Enchantment.EnchantDamageAdditive(num, props);
			}
			if (modifyDamageHookType.HasFlag(ModifyDamageHookType.Multiplicative))
			{
				num *= cardSource.Enchantment.EnchantDamageMultiplicative(num, props);
			}
		}
		bool flag = target == null && previewMode == CardPreviewMode.MultiCreatureTargeting;
		bool flag2 = flag;
		bool flag3;
		if (flag2)
		{
			if (cardSource != null)
			{
				TargetType targetType = cardSource.TargetType;
				if ((uint)(targetType - 3) <= 1u)
				{
					CardPile pile = cardSource.Pile;
					if (pile != null)
					{
						PileType type = pile.Type;
						if (type == PileType.Hand || type == PileType.Play)
						{
							flag3 = true;
							goto IL_00bb;
						}
					}
				}
			}
			flag3 = false;
			goto IL_00bb;
		}
		goto IL_00bf;
		IL_00bf:
		bool flag4 = flag2;
		bool flag5 = false;
		if (flag4)
		{
			bool flag6 = true;
			decimal? num2 = null;
			foreach (Creature item in combatState?.HittableEnemies ?? Array.Empty<Creature>())
			{
				List<AbstractModel> modifiers3;
				decimal num3 = ModifyDamageInternal(runState, combatState, item, dealer, num, props, cardSource, modifyDamageHookType, out modifiers3);
				if (!num2.HasValue)
				{
					num2 = num3;
				}
				else if ((int)num3 != (int)num2.Value)
				{
					flag6 = false;
					break;
				}
				modifiers2.AddRange(modifiers3);
			}
			if (num2.HasValue && flag6)
			{
				flag5 = true;
				num = num2.Value;
				modifiers2 = modifiers2.Distinct().ToList();
			}
			else
			{
				modifiers2.Clear();
			}
		}
		if (!flag4 || !flag5)
		{
			num = ModifyDamageInternal(runState, combatState, target, dealer, num, props, cardSource, modifyDamageHookType, out modifiers2);
		}
		modifiers = modifiers2;
		return Math.Max(0m, num);
		IL_00bb:
		flag2 = flag3;
		goto IL_00bf;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyEnergyCostInCombat(MegaCrit.Sts2.Core.Models.CardModel,System.Decimal,System.Decimal@)" />.
	/// </summary>
	public static decimal ModifyEnergyCostInCombat(ICombatState combatState, CardModel card, decimal originalCost)
	{
		if (originalCost < 0m)
		{
			return originalCost;
		}
		decimal modifiedCost = originalCost;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			item.TryModifyEnergyCostInCombat(card, modifiedCost, out modifiedCost);
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			item2.TryModifyEnergyCostInCombatLate(card, modifiedCost, out modifiedCost);
		}
		return modifiedCost;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyKeywordsInCombat(MegaCrit.Sts2.Core.Models.CardModel,System.Collections.Generic.ISet{MegaCrit.Sts2.Core.Entities.Cards.CardKeyword})" />.
	/// </summary>
	public static void ModifyKeywordsInCombat(ICombatState combatState, CardModel card, ISet<CardKeyword> keywords)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			item.TryModifyKeywordsInCombat(card, keywords);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyEnergyGain(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyEnergyGain(ICombatState combatState, Player player, decimal originalAmount, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = originalAmount;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			decimal num2 = num;
			num = item.ModifyEnergyGain(player, num);
			if ((int)num2 != (int)num)
			{
				list.Add(item);
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyGoldGained(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyGoldGained(IRunState runState, ICombatState? combatState, decimal amount, Player player, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = amount;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(combatState))
		{
			decimal num2 = num;
			num = item.ModifyGoldGained(player, num);
			if ((int)num2 != (int)num)
			{
				list.Add(item);
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyExtraRestSiteHealText(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Localization.LocString})" />.
	/// </summary>
	public static IReadOnlyList<LocString> ModifyExtraRestSiteHealText(IRunState runState, Player player, IReadOnlyList<LocString> extraText)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			extraText = item.ModifyExtraRestSiteHealText(player, extraText);
		}
		return extraText;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyGeneratedMap(MegaCrit.Sts2.Core.Runs.IRunState,MegaCrit.Sts2.Core.Map.ActMap,System.Int32)" />.
	/// </summary>
	public static ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			map = item.ModifyGeneratedMap(runState, map, actIndex);
			item.InvokeExecutionFinished();
		}
		return ModifyGeneratedMapLate(runState, map, actIndex);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyGeneratedMapLate(MegaCrit.Sts2.Core.Runs.IRunState,MegaCrit.Sts2.Core.Map.ActMap,System.Int32)" />.
	/// </summary>
	public static ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			map = item.ModifyGeneratedMapLate(runState, map, actIndex);
			item.InvokeExecutionFinished();
		}
		return map;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHandDraw(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyHandDraw(ICombatState combatState, Player player, decimal originalCardCount, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = originalCardCount;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			decimal num2 = num;
			num = item.ModifyHandDraw(player, num);
			if ((int)num2 != (int)num)
			{
				list.Add(item);
			}
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			decimal num3 = num;
			num = item2.ModifyHandDrawLate(player, num);
			if ((int)num3 != (int)num)
			{
				list.Add(item2);
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// Run the requested HP-loss-modification hook phases on <paramref name="target" />.
	/// In CreatureCmd.Damage the two phases are invoked separately because damage redirection (Osty) sits between them
	/// and may change the target. Callers that have no redirection step (e.g. damage previews) should pass
	/// <see cref="F:MegaCrit.Sts2.Core.Hooks.HpLossHookPhase.All" />.
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostBeforeOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostAfterOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static decimal ModifyHpLost(IRunState runState, ICombatState? combatState, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, HpLossHookPhase phases, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = amount;
		List<AbstractModel> list = new List<AbstractModel>();
		if (phases.HasFlag(HpLossHookPhase.BeforeOsty))
		{
			foreach (AbstractModel item in runState.IterateHookListeners(combatState))
			{
				decimal d = num;
				num = item.ModifyHpLostBeforeOsty(target, num, props, dealer, cardSource);
				if (decimal.Truncate(d) != decimal.Truncate(num))
				{
					list.Add(item);
				}
			}
			foreach (AbstractModel item2 in runState.IterateHookListeners(combatState))
			{
				decimal d2 = num;
				num = item2.ModifyHpLostBeforeOstyLate(target, num, props, dealer, cardSource);
				if (decimal.Truncate(d2) != decimal.Truncate(num))
				{
					list.Add(item2);
				}
			}
		}
		if (phases.HasFlag(HpLossHookPhase.AfterOsty))
		{
			foreach (AbstractModel item3 in runState.IterateHookListeners(combatState))
			{
				decimal d3 = num;
				num = item3.ModifyHpLostAfterOsty(target, num, props, dealer, cardSource);
				if (decimal.Truncate(d3) != decimal.Truncate(num))
				{
					list.Add(item3);
				}
			}
			foreach (AbstractModel item4 in runState.IterateHookListeners(combatState))
			{
				decimal d4 = num;
				num = item4.ModifyHpLostAfterOstyLate(target, num, props, dealer, cardSource);
				if (decimal.Truncate(d4) != decimal.Truncate(num))
				{
					list.Add(item4);
				}
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyMaxEnergy(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyMaxEnergy(ICombatState combatState, Player player, decimal amount)
	{
		decimal num = amount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			num = item.ModifyMaxEnergy(player, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyMerchantCardCreationResults(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.List{MegaCrit.Sts2.Core.Entities.Cards.CardCreationResult})" />.
	/// </summary>
	public static void ModifyMerchantCardCreationResults(IRunState runState, Player player, List<CardCreationResult> cards)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			item.ModifyMerchantCardCreationResults(player, cards);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyMerchantCardPool(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Models.CardModel})" />.
	/// </summary>
	public static IEnumerable<CardModel> ModifyMerchantCardPool(IRunState runState, Player player, IEnumerable<CardModel> options)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			options = item.ModifyMerchantCardPool(player, options);
		}
		return options;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyMerchantCardPool(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Models.CardModel})" />.
	/// </summary>
	public static CardRarity ModifyMerchantCardRarity(IRunState runState, Player player, CardRarity rarity)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			rarity = item.ModifyMerchantCardRarity(player, rarity);
		}
		return rarity;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyMerchantPrice(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Entities.Merchant.MerchantEntry,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyMerchantPrice(IRunState runState, Player player, MerchantEntry entry, decimal result)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			result = item.ModifyMerchantPrice(player, entry, result);
		}
		return result;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyNextEvent(MegaCrit.Sts2.Core.Models.EventModel)" />.
	/// </summary>
	public static EventModel ModifyNextEvent(IRunState runState, EventModel currentEvent)
	{
		EventModel eventModel = currentEvent;
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			eventModel = item.ModifyNextEvent(eventModel);
		}
		return eventModel;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyOddsIncreaseForUnrolledRoomType(MegaCrit.Sts2.Core.Rooms.RoomType,System.Single)" />.
	/// </summary>
	public static float ModifyOddsIncreaseForUnrolledRoomType(IRunState runState, RoomType roomType, float oddsIncrease)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			oddsIncrease = item.ModifyOddsIncreaseForUnrolledRoomType(roomType, oddsIncrease);
		}
		return oddsIncrease;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyOrbPassiveTriggerCounts(MegaCrit.Sts2.Core.Models.OrbModel,System.Int32)" />.
	/// </summary>
	public static int ModifyOrbPassiveTriggerCount(ICombatState combatState, OrbModel orb, int triggerCount, out List<AbstractModel> modifyingModels)
	{
		modifyingModels = new List<AbstractModel>();
		int num = triggerCount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			int num2 = num;
			num = item.ModifyOrbPassiveTriggerCounts(orb, num);
			if (num != num2)
			{
				modifyingModels.Add(item);
			}
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyOrbValue(MegaCrit.Sts2.Core.Models.OrbModel,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyOrbValue(ICombatState combatState, OrbModel orb, decimal amount)
	{
		decimal num = amount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			num = item.ModifyOrbValue(orb, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyPowerAmountGivenAdditive(MegaCrit.Sts2.Core.Models.PowerModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyPowerAmountGivenMultiplicative(MegaCrit.Sts2.Core.Models.PowerModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static decimal ModifyPowerAmountGiven(ICombatState combatState, PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = amount;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			decimal num2 = item.ModifyPowerAmountGivenAdditive(power, giver, num, target, cardSource);
			num += num2;
			if (num2 != 0m)
			{
				list.Add(item);
			}
		}
		foreach (AbstractModel item2 in IterateCombatHookListeners(combatState))
		{
			decimal num3 = item2.ModifyPowerAmountGivenMultiplicative(power, giver, num, target, cardSource);
			num *= num3;
			if (num3 != 1m)
			{
				list.Add(item2);
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyPowerAmountReceived(MegaCrit.Sts2.Core.Models.PowerModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal@)" />.
	/// </summary>
	public static decimal ModifyPowerAmountReceived(ICombatState combatState, PowerModel canonicalPower, Creature target, decimal amount, Creature? giver, out IEnumerable<AbstractModel> modifiers)
	{
		decimal num = amount;
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (item.TryModifyPowerAmountReceived(canonicalPower, target, num, giver, out var modifiedAmount))
			{
				num = modifiedAmount;
				list.Add(item);
			}
		}
		modifiers = list;
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyRestSiteHealAmount(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal)" />.
	/// </summary>
	public static decimal ModifyRestSiteHealAmount(IRunState runState, Creature creature, decimal amount)
	{
		decimal num = amount;
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			num = item.ModifyRestSiteHealAmount(creature, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyRestSiteOptions(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.ICollection{MegaCrit.Sts2.Core.Entities.RestSite.RestSiteOption})" />.
	/// </summary>
	public static IEnumerable<AbstractModel> ModifyRestSiteOptions(IRunState runState, Player player, ICollection<RestSiteOption> options)
	{
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.TryModifyRestSiteOptions(player, options))
			{
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyRestSiteHealRewards(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.List{MegaCrit.Sts2.Core.Rewards.Reward},System.Boolean)" />.
	/// </summary>
	public static IEnumerable<AbstractModel> ModifyRestSiteHealRewards(IRunState runState, Player player, List<Reward> rewards, bool isMimicked)
	{
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.TryModifyRestSiteHealRewards(player, rewards, isMimicked))
			{
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyRewards(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.List{MegaCrit.Sts2.Core.Rewards.Reward},MegaCrit.Sts2.Core.Rooms.AbstractRoom)" />.
	/// </summary>
	public static IEnumerable<AbstractModel> ModifyRewards(IRunState runState, Player player, List<Reward> rewards, AbstractRoom? room)
	{
		List<AbstractModel> list = new List<AbstractModel>();
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.TryModifyRewards(player, rewards, room))
			{
				list.Add(item);
			}
		}
		foreach (AbstractModel item2 in runState.IterateHookListeners(null))
		{
			if (item2.TryModifyRewardsLate(player, rewards, room))
			{
				list.Add(item2);
			}
		}
		return list;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyShuffleOrder(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.List{MegaCrit.Sts2.Core.Models.CardModel},System.Boolean)" />.
	/// </summary>
	public static void ModifyShuffleOrder(ICombatState combatState, Player player, List<CardModel> cards, bool isInitialShuffle)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			item.ModifyShuffleOrder(player, cards, isInitialShuffle);
		}
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyStarCost(MegaCrit.Sts2.Core.Models.CardModel,System.Decimal,System.Decimal@)" />.
	/// </summary>
	public static decimal ModifyStarCost(ICombatState combatState, CardModel card, decimal originalCost)
	{
		if (originalCost < 0m)
		{
			return originalCost;
		}
		decimal modifiedCost = originalCost;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			item.TryModifyStarCost(card, modifiedCost, out modifiedCost);
		}
		return modifiedCost;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifySummonAmount(MegaCrit.Sts2.Core.Entities.Players.Player,System.Decimal,MegaCrit.Sts2.Core.Models.AbstractModel)" />.
	/// </summary>
	public static decimal ModifySummonAmount(ICombatState combatState, Player summoner, decimal amount, AbstractModel? source)
	{
		decimal num = amount;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			num = item.ModifySummonAmount(summoner, num, source);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyUnblockedDamageTarget(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it runs during damage
	/// resolution, which proceeds while combat is ending.
	/// </summary>
	public static Creature ModifyUnblockedDamageTarget(ICombatState combatState, Creature originalTarget, decimal amount, ValueProp props, Creature? dealer)
	{
		Creature creature = originalTarget;
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			creature = item.ModifyUnblockedDamageTarget(creature, amount, props, dealer);
		}
		return creature;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyUnknownMapPointRoomTypes(System.Collections.Generic.IReadOnlySet{MegaCrit.Sts2.Core.Rooms.RoomType})" />.
	/// </summary>
	public static IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IRunState runState, IReadOnlySet<RoomType> roomTypes)
	{
		IReadOnlySet<RoomType> readOnlySet = new HashSet<RoomType>(roomTypes);
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			readOnlySet = item.ModifyUnknownMapPointRoomTypes(readOnlySet);
		}
		return readOnlySet;
	}

	public static int ModifyXValue(ICombatState combatState, CardModel card, int originalValue)
	{
		int num = originalValue;
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			num = item.ModifyXValue(card, num);
		}
		return num;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAddToDeck(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static bool ShouldAddToDeck(IRunState runState, CardModel card, out AbstractModel? preventer)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldAddToDeck(card))
			{
				preventer = item;
				return false;
			}
		}
		preventer = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAfflict(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Models.AfflictionModel)" />.
	/// </summary>
	public static bool ShouldAfflict(ICombatState combatState, CardModel card, AfflictionModel affliction)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldAfflict(card, affliction))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowAncient(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Models.AncientEventModel)" />.
	/// </summary>
	public static bool ShouldAllowAncient(IRunState runState, Player player, AncientEventModel ancient)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldAllowAncient(player, ancient))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowHitting(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static bool ShouldAllowHitting(ICombatState combatState, Creature creature)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldAllowHitting(creature))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowMerchantCardRemoval(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldAllowMerchantCardRemoval(IRunState runState, Player player)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldAllowMerchantCardRemoval(player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowSelectingMoreCardRewards(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Rewards.CardReward)" />.
	/// </summary>
	public static bool ShouldAllowSelectingMoreCardRewards(IRunState runState, Player player, CardReward reward)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.ShouldAllowSelectingMoreCardRewards(player, reward))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowTargeting(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static bool ShouldAllowTargeting(ICombatState combatState, Creature target, out AbstractModel? preventer)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldAllowTargeting(target))
			{
				preventer = item;
				return false;
			}
		}
		preventer = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldClearBlock(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static bool ShouldClearBlock(ICombatState combatState, Creature creature, out AbstractModel? preventer)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldClearBlock(creature))
			{
				preventer = item;
				return false;
			}
		}
		preventer = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldCreatureBeRemovedFromCombatAfterDeath(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it is a predicate that
	/// drives the decision of whether a creature is removed after death, so suppressing it while
	/// combat is ending would drop the votes it collects.
	/// </summary>
	public static bool ShouldCreatureBeRemovedFromCombatAfterDeath(ICombatState combatState, Creature creature)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (!item.ShouldCreatureBeRemovedFromCombatAfterDeath(creature))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldDie(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" />.
	/// </summary>
	public static bool ShouldDie(IRunState runState, ICombatState? combatState, Creature creature, out AbstractModel? preventer)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(combatState))
		{
			if (!item.ShouldDie(creature))
			{
				preventer = item;
				return false;
			}
		}
		foreach (AbstractModel item2 in runState.IterateHookListeners(combatState))
		{
			if (!item2.ShouldDieLate(creature))
			{
				preventer = item2;
				return false;
			}
		}
		preventer = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldDisableRemainingRestSiteOptions(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldDisableRemainingRestSiteOptions(IRunState runState, Player player)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldDisableRemainingRestSiteOptions(player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldDraw(MegaCrit.Sts2.Core.Entities.Players.Player,System.Boolean)" />.
	/// </summary>
	public static bool ShouldDraw(ICombatState combatState, Player player, bool fromHandDraw, out AbstractModel? modifier)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldDraw(player, fromHandDraw))
			{
				modifier = item;
				return false;
			}
		}
		modifier = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldEtherealTrigger(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public static bool ShouldEtherealTrigger(ICombatState combatState, CardModel card)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldEtherealTrigger(card))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldFlush(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldFlush(ICombatState combatState, Player player)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldFlush(player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldGenerateTreasure(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldGenerateTreasure(IRunState runState, Player player)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldGenerateTreasure(player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldGainStars(System.Decimal,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldGainStars(ICombatState combatState, decimal amount, Player player)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldGainStars(amount, player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldPayExcessEnergyCostWithStars(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldPayExcessEnergyCostWithStars(ICombatState combatState, Player player)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (item.ShouldPayExcessEnergyCostWithStars(player))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldPlay(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.AutoPlayType)" />.
	/// </summary>
	public static bool ShouldPlay(ICombatState combatState, CardModel card, out AbstractModel? preventer, AutoPlayType autoPlayType)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldPlay(card, autoPlayType))
			{
				preventer = item;
				return false;
			}
		}
		preventer = null;
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldPlayerResetEnergy(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldPlayerResetEnergy(ICombatState combatState, Player player)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (!item.ShouldPlayerResetEnergy(player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldProceedToNextMapPoint" />.
	/// </summary>
	public static bool ShouldProceedToNextMapPoint(IRunState runState)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (!item.ShouldProceedToNextMapPoint())
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldProcurePotion(MegaCrit.Sts2.Core.Models.PotionModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldProcurePotion(IRunState runState, ICombatState? combatState, PotionModel potion, Player player)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(combatState))
		{
			if (!item.ShouldProcurePotion(potion, player))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldRefillMerchantEntry(MegaCrit.Sts2.Core.Entities.Merchant.MerchantEntry,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldRefillMerchantEntry(IRunState runState, MerchantEntry entry, Player player)
	{
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			if (item.ShouldRefillMerchantEntry(entry, player))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldStopCombatFromEnding" />.
	///
	/// Dispatched directly, not through the IterateCombatHookListeners guard: it is a predicate that
	/// drives the decision of whether combat ends, so suppressing it while combat is ending would
	/// drop the votes it collects.
	/// </summary>
	public static bool ShouldStopCombatFromEnding(ICombatState combatState)
	{
		foreach (AbstractModel item in combatState.IterateHookListeners())
		{
			if (item.ShouldStopCombatFromEnding())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldTakeExtraTurn(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public static bool ShouldTakeExtraTurn(ICombatState combatState, Player player)
	{
		foreach (AbstractModel item in IterateCombatHookListeners(combatState))
		{
			if (item.ShouldTakeExtraTurn(player))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldForcePotionReward(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Rooms.RoomType)" />.
	/// </summary>
	public static bool ShouldForcePotionReward(IRunState runState, Player player, RoomType roomType)
	{
		bool flag = false;
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			flag = flag || item.ShouldForcePotionReward(player, roomType);
		}
		return flag;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ShouldAllowFreeTravel" />.
	/// </summary>
	public static bool ShouldAllowFreeTravel(IRunState runState)
	{
		bool flag = false;
		foreach (AbstractModel item in runState.IterateHookListeners(null))
		{
			flag = flag || item.ShouldAllowFreeTravel();
		}
		return flag;
	}

	public static bool ShouldPowerBeRemovedOnDeath(PowerModel power)
	{
		if (power.Owner.CombatState == null)
		{
			return true;
		}
		foreach (AbstractModel item in power.CombatState.IterateHookListeners())
		{
			if (!item.ShouldPowerBeRemovedOnDeath(power))
			{
				return false;
			}
		}
		return true;
	}

	private static decimal ModifyDamageInternal(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType modifyDamageHookType, out List<AbstractModel> modifiers)
	{
		decimal num = damage;
		List<AbstractModel> list = new List<AbstractModel>();
		if (modifyDamageHookType.HasFlag(ModifyDamageHookType.Additive))
		{
			foreach (AbstractModel item in runState.IterateHookListeners(combatState))
			{
				decimal num2 = item.ModifyDamageAdditive(target, num, props, dealer, cardSource);
				num += num2;
				if (num2 != 0m)
				{
					list.Add(item);
				}
			}
		}
		if (modifyDamageHookType.HasFlag(ModifyDamageHookType.Multiplicative))
		{
			foreach (AbstractModel item2 in runState.IterateHookListeners(combatState))
			{
				decimal num3 = item2.ModifyDamageMultiplicative(target, num, props, dealer, cardSource);
				num *= num3;
				if (num3 != 1m)
				{
					list.Add(item2);
				}
			}
		}
		if (modifyDamageHookType.HasFlag(ModifyDamageHookType.Cap))
		{
			decimal num4 = decimal.MaxValue;
			foreach (AbstractModel item3 in runState.IterateHookListeners(combatState))
			{
				decimal num5 = item3.ModifyDamageCap(target, props, dealer, cardSource);
				if (num5 < num4)
				{
					num4 = num5;
					if (num > num5)
					{
						num = num5;
						list.Add(item3);
					}
				}
			}
		}
		modifiers = list;
		return num;
	}
}
