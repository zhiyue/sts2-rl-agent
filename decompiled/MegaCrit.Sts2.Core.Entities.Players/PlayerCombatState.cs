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

	public IReadOnlyList<Creature> Pets => _pets;

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
		Energy = (int)Math.Max((decimal)Energy - amount, 0m);
	}

	public void GainEnergy(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Energy = (int)Math.Max((decimal)Energy + amount, 0m);
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
			throw new InvalidOperationException("Player does not have pet " + pet.Name);
		}
		if (Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(pet.CombatState, pet))
		{
			pet.Died -= OnPetDied;
			_pets.Remove(pet);
		}
	}
}
