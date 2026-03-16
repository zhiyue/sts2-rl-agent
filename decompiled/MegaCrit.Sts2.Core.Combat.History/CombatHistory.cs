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

	public void CardPlayStarted(CombatState combatState, CardPlay cardPlay)
	{
		Add(new CardPlayStartedEntry(cardPlay, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CardPlayFinished(CombatState combatState, CardPlay cardPlay)
	{
		Add(new CardPlayFinishedEntry(cardPlay, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CardAfflicted(CombatState combatState, CardModel card, AfflictionModel affliction)
	{
		Add(new CardAfflictedEntry(card, affliction, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CardDiscarded(CombatState combatState, CardModel card)
	{
		Add(new CardDiscardedEntry(card, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CardDrawn(CombatState combatState, CardModel card, bool fromHandDraw)
	{
		Add(new CardDrawnEntry(card, combatState.RoundNumber, combatState.CurrentSide, fromHandDraw, this));
	}

	public void CardExhausted(CombatState combatState, CardModel card)
	{
		Add(new CardExhaustedEntry(card, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CardGenerated(CombatState combatState, CardModel card, bool generatedByPlayer)
	{
		Add(new CardGeneratedEntry(card, generatedByPlayer, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void CreatureAttacked(CombatState combatState, Creature attacker, IReadOnlyList<DamageResult> damageResults)
	{
		Add(new CreatureAttackedEntry(attacker, damageResults, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void DamageReceived(CombatState combatState, Creature receiver, Creature? dealer, DamageResult result, CardModel? cardSource)
	{
		Add(new DamageReceivedEntry(result, receiver, dealer, cardSource, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void BlockGained(CombatState combatState, Creature receiver, int amount, ValueProp props, CardPlay? cardPlay)
	{
		Add(new BlockGainedEntry(amount, props, cardPlay, receiver, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void EnergySpent(CombatState combatState, int amount, Player player)
	{
		Add(new EnergySpentEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void MonsterPerformedMove(CombatState combatState, MonsterModel monster, MoveState move, IEnumerable<Creature>? targets)
	{
		Add(new MonsterPerformedMoveEntry(monster, move, targets, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void OrbChanneled(CombatState combatState, OrbModel orb)
	{
		Add(new OrbChanneledEntry(orb, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void PotionUsed(CombatState combatState, PotionModel potion, Creature? target)
	{
		Add(new PotionUsedEntry(potion, target, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void PowerReceived(CombatState combatState, PowerModel power, decimal amount, Creature? applier)
	{
		Add(new PowerReceivedEntry(power, amount, applier, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void StarsModified(CombatState combatState, int amount, Player player)
	{
		Add(new StarsModifiedEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	public void Summoned(CombatState combatState, int amount, Player player)
	{
		Add(new SummonedEntry(amount, player, combatState.RoundNumber, combatState.CurrentSide, this));
	}

	private void Add(CombatHistoryEntry entry)
	{
		_entries.Add(entry);
		this.Changed?.Invoke();
	}
}
