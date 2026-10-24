using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Players;

public class PlayerCombatState
{
	private readonly Player _player;

	private readonly List<Creature> _pets = new List<Creature>();

	private CardPile[]? _piles;

	private int _energy;

	private int _stars;

	private PlayerTurnPhase _phase;

	public IReadOnlyList<Creature> Pets => _pets;

	/// <summary>
	/// The turn number that this player is currently on.
	/// This is different from <see cref="P:MegaCrit.Sts2.Core.Combat.ICombatState.RoundNumber" />; if a player takes an extra turn after their
	/// current one, this will be incremented, while the round number will not.
	/// This number can also be different between players in multiplayer. If one player takes an extra turn (due to an
	/// effect like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsEye" />), their turn number will increment, but other players' will not.
	/// This starts at 1, so it should never be 0.
	/// </summary>
	public int TurnNumber { get; private set; } = 1;

	/// <summary>
	/// The current phase of this player's turn. <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.None" /> when combat is not in progress
	/// and during the enemy's turn. See <see cref="T:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase" /> for the meaning of each phase.
	/// Managed by <see cref="T:MegaCrit.Sts2.Core.Combat.CombatManager" /> and <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterAutoPrePlayPhaseEntered(MegaCrit.Sts2.Core.GameActions.Multiplayer.HookPlayerChoiceContext,MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public PlayerTurnPhase Phase
	{
		get
		{
			return _phase;
		}
		set
		{
			if (value != _phase)
			{
				_phase = value;
				this.PlayerTurnPhaseChanged?.Invoke();
			}
		}
	}

	public CardPile Hand { get; } = new CardPile(PileType.Hand);

	public CardPile DrawPile { get; } = new CardPile(PileType.Draw);

	public CardPile DiscardPile { get; } = new CardPile(PileType.Discard);

	public CardPile ExhaustPile { get; } = new CardPile(PileType.Exhaust);

	public CardPile PlayPile { get; } = new CardPile(PileType.Play);

	public IReadOnlyList<CardPile> AllPiles
	{
		get
		{
			if (_piles == null)
			{
				_piles = new CardPile[5] { Hand, DrawPile, DiscardPile, ExhaustPile, PlayPile };
			}
			return _piles;
		}
	}

	public IEnumerable<CardModel> AllCards => AllPiles.SelectMany((CardPile p) => p.Cards);

	public int Energy
	{
		get
		{
			return _energy;
		}
		set
		{
			if (_energy != value)
			{
				int energy = _energy;
				_energy = value;
				this.EnergyChanged?.Invoke(energy, _energy);
			}
		}
	}

	public int MaxEnergy => (int)Hook.ModifyMaxEnergy(_player.Creature.CombatState, _player, _player.MaxEnergy);

	public int Stars
	{
		get
		{
			return _stars;
		}
		set
		{
			if (_stars != value)
			{
				int stars = _stars;
				_stars = value;
				CombatManager.Instance.History.StarsModified(_player.Creature.CombatState, _stars - stars, _player);
				this.StarsChanged?.Invoke(stars, _stars);
			}
		}
	}

	public OrbQueue OrbQueue { get; }

	public event Action? PlayerTurnPhaseChanged;

	public event Action<int, int>? EnergyChanged;

	public event Action<int, int>? StarsChanged;

	public PlayerCombatState(Player player)
	{
		_player = player;
		CombatManager.Instance.StateTracker.Subscribe(this);
		foreach (CardPile allPile in AllPiles)
		{
			CombatManager.Instance.StateTracker.Subscribe(allPile);
		}
		OrbQueue = new OrbQueue(player);
		OrbQueue.Clear();
		OrbQueue.AddCapacity(player.BaseOrbSlotCount);
	}

	public void AfterCombatEnd()
	{
		CombatManager.Instance.StateTracker.Unsubscribe(this);
		foreach (CardPile allPile in AllPiles)
		{
			allPile.Clear();
			CombatManager.Instance.StateTracker.Unsubscribe(allPile);
		}
		_pets.Clear();
	}

	/// <summary>
	/// Increment this player's current turn number.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.TurnNumber" /> for details on turn number versus <see cref="P:MegaCrit.Sts2.Core.Combat.ICombatState.RoundNumber" />.
	/// </summary>
	public void IncrementTurnNumber()
	{
		TurnNumber++;
	}

	public void ResetEnergy()
	{
		Energy = MaxEnergy;
	}

	public void AddMaxEnergyToCurrent()
	{
		Energy += MaxEnergy;
	}

	public void LoseEnergy(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Energy = (int)Math.Clamp((decimal)Energy - amount, 0m, 999999999m);
	}

	public void GainEnergy(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Energy = (int)Math.Clamp((decimal)Energy + amount, 0m, 999999999m);
	}

	public bool HasEnoughResourcesFor(CardModel card, out UnplayableReason reason)
	{
		int num = Math.Max(0, card.EnergyCost.GetWithModifiers(CostModifiers.All));
		int num2 = Math.Max(0, card.GetStarCostWithModifiers());
		if (num > Energy && card.CombatState != null && Hook.ShouldPayExcessEnergyCostWithStars(card.CombatState, _player))
		{
			num2 += (num - Energy) * 2;
			num = Energy;
		}
		reason = UnplayableReason.None;
		if (num > Energy)
		{
			reason |= UnplayableReason.EnergyCostTooHigh;
		}
		if (num2 > Stars)
		{
			reason |= UnplayableReason.StarCostTooHigh;
		}
		return reason == UnplayableReason.None;
	}

	public void LoseStars(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Stars = (int)Math.Max((decimal)Stars - amount, 0m);
	}

	public void GainStars(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Stars = (int)Math.Max((decimal)Stars + amount, 0m);
	}

	/// <summary>
	/// NEVER CALL THIS!
	/// ONLY <see cref="M:MegaCrit.Sts2.Core.Commands.PlayerCmd.AddPet``1(MegaCrit.Sts2.Core.Entities.Players.Player)" /> and save/load stuff should be calling this.
	/// </summary>
	/// <param name="pet">Pet to add.</param>
	public void AddPetInternal(Creature pet)
	{
		pet.Monster.AssertMutable();
		if (!_pets.Contains(pet))
		{
			if (pet.PetOwner != _player)
			{
				pet.PetOwner = _player;
			}
			pet.Died += OnPetDied;
			_pets.Add(pet);
		}
	}

	/// <summary>
	/// Get one of this player's pets.
	/// If the player has multiple of the same type of pet, just get the first one.
	/// If the player has none of this type of pet, returns null.
	/// </summary>
	/// <typeparam name="T">Type of pet to get.</typeparam>
	/// <returns>Matching pet.</returns>
	public Creature? GetPet<T>() where T : MonsterModel
	{
		return Pets.FirstOrDefault((Creature p) => p.Monster is T);
	}

	public void RecalculateCardValues()
	{
		foreach (CardModel allCard in AllCards)
		{
			allCard.Enchantment?.RecalculateValues();
		}
	}

	public void EndOfTurnCleanup()
	{
		foreach (CardModel allCard in AllCards)
		{
			allCard.EndOfTurnCleanup();
		}
	}

	public bool HasCardsToPlay()
	{
		return Hand.Cards.Any((CardModel c) => c.CanPlay());
	}

	private void OnPetDied(Creature pet)
	{
		if (!_pets.Contains(pet))
		{
			throw new InvalidOperationException("Player does not have pet " + pet.LogName);
		}
		if (Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(pet.CombatState, pet))
		{
			pet.Died -= OnPetDied;
			_pets.Remove(pet);
		}
	}
}
