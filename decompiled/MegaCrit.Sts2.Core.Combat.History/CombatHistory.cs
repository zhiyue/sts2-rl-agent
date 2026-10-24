using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Combat.History;

/// <summary>
/// A history of most events that have happened in combat.
/// Each entry should be logged immediately after the event occurs, but before its AfterX hook is executed.
/// For example, a CardPlayedEntry is logged immediately after the card is played, but before AfterCardPlayed is
/// executed.
/// </summary>
public class CombatHistory
{
	private readonly List<CombatHistoryEntry> _entries = new List<CombatHistoryEntry>();

	public IEnumerable<CombatHistoryEntry> Entries => _entries;

	public IEnumerable<CardPlayStartedEntry> CardPlaysStarted => Entries.OfType<CardPlayStartedEntry>();

	public IEnumerable<CardPlayFinishedEntry> CardPlaysFinished => Entries.OfType<CardPlayFinishedEntry>();

	public event Action? Changed;

	public void Clear()
	{
		_entries.Clear();
		this.Changed?.Invoke();
	}

	public void CardPlayStarted(ICombatState combatState, CardPlay cardPlay)
	{
		Add(combatState, new CardPlayStartedEntry(cardPlay, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CardPlayFinished(ICombatState combatState, CardPlay cardPlay)
	{
		Add(combatState, new CardPlayFinishedEntry(cardPlay, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CardAfflicted(ICombatState combatState, CardModel card, AfflictionModel affliction)
	{
		Add(combatState, new CardAfflictedEntry(card, affliction, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CardDiscarded(ICombatState combatState, CardModel card)
	{
		Add(combatState, new CardDiscardedEntry(card, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CardDrawn(ICombatState combatState, CardModel card, bool fromHandDraw)
	{
		Add(combatState, new CardDrawnEntry(card, combatState.RoundNumber, combatState.CurrentSide, fromHandDraw, this, combatState.Players));
	}

	public void CardExhausted(ICombatState combatState, CardModel card)
	{
		Add(combatState, new CardExhaustedEntry(card, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CardGenerated(ICombatState combatState, CardModel card, Player? creator)
	{
		Add(combatState, new CardGeneratedEntry(card, creator, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void CreatureAttacked(ICombatState combatState, Creature attacker, IReadOnlyList<DamageResult> damageResults)
	{
		Add(combatState, new CreatureAttackedEntry(attacker, damageResults, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void DamageReceived(ICombatState combatState, Creature receiver, Creature? dealer, DamageResult result, CardModel? cardSource)
	{
		Add(combatState, new DamageReceivedEntry(result, receiver, dealer, cardSource, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void BlockGained(ICombatState combatState, Creature receiver, int amount, ValueProp props, CardPlay? cardPlay)
	{
		Add(combatState, new BlockGainedEntry(amount, props, cardPlay, receiver, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void EnergySpent(ICombatState combatState, int amount, Player player)
	{
		Add(combatState, new EnergySpentEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void MonsterPerformedMove(ICombatState combatState, MonsterModel monster, MoveState move, IEnumerable<Creature>? targets)
	{
		Add(combatState, new MonsterPerformedMoveEntry(monster, move, targets, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void OrbChanneled(ICombatState combatState, OrbModel orb)
	{
		Add(combatState, new OrbChanneledEntry(orb, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void PotionUsed(ICombatState combatState, PotionModel potion, Creature? target)
	{
		Add(combatState, new PotionUsedEntry(potion, target, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void PowerReceived(ICombatState combatState, PowerModel power, decimal amount, Creature? applier)
	{
		Add(combatState, new PowerReceivedEntry(power, amount, applier, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void StarsModified(ICombatState combatState, int amount, Player player)
	{
		Add(combatState, new StarsModifiedEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	public void Summoned(ICombatState combatState, int amount, Player player)
	{
		Add(combatState, new SummonedEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this, combatState.Players));
	}

	private void Add(ICombatState combatState, CombatHistoryEntry entry)
	{
		if (combatState.IsLiveCombat())
		{
			_entries.Add(entry);
			this.Changed?.Invoke();
		}
	}
}
