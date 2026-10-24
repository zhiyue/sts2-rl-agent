using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Combat;

public class NullCombatState : ICombatState
{
	public IRunState RunState => NullRunState.Instance;

	public IReadOnlyList<Creature> Allies { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Creature> Enemies { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Creature> Creatures { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Creature> PlayerCreatures { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Player> Players { get; } = Array.Empty<Player>();

	public IReadOnlyList<ModifierModel> Modifiers { get; } = Array.Empty<ModifierModel>();

	public MultiplayerScalingModel? MultiplayerScalingModel => null;

	public int RoundNumber { get; set; } = 1;

	public CombatSide CurrentSide { get; set; }

	public EncounterModel? Encounter => null;

	public IReadOnlyList<Creature> EscapedCreatures => Array.Empty<Creature>();

	public IReadOnlyList<Creature> CreaturesOnCurrentSide { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Creature> HittableEnemies { get; } = Array.Empty<Creature>();

	public event Action<ICombatState>? CreaturesChanged;

	public T CreateCard<T>(Player owner) where T : CardModel
	{
		throw new NotImplementedException();
	}

	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		throw new NotImplementedException();
	}

	public CardModel CloneCard(CardModel mutableCard)
	{
		throw new NotImplementedException();
	}

	public void AddCard(CardModel card, Player owner)
	{
	}

	public void RemoveCard(CardModel card)
	{
	}

	public bool ContainsCard(CardModel card)
	{
		return false;
	}

	public void AddPlayer(Player player)
	{
	}

	public Creature CreateCreature(MonsterModel monster, CombatSide side, string? slot)
	{
		return new Creature(monster, side, slot);
	}

	public void CreatureEscaped(Creature creature)
	{
	}

	public void RemoveCreature(Creature creature, bool unattach = true)
	{
	}

	public bool ContainsCreature(Creature creature)
	{
		return false;
	}

	public bool ContainsMonster<T>() where T : MonsterModel
	{
		return false;
	}

	public Creature? GetCreature(uint? combatId)
	{
		return null;
	}

	public Task<Creature?> GetCreatureAsync(uint? combatId, double timeoutSec)
	{
		return Task.FromResult<Creature>(null);
	}

	public IReadOnlyList<Creature> GetCreaturesOnSide(CombatSide side)
	{
		return CreaturesOnCurrentSide;
	}

	public IReadOnlyList<Creature> GetOpponentsOf(Creature creature)
	{
		return Array.Empty<Creature>();
	}

	public IReadOnlyList<Creature> GetTeammatesOf(Creature creature)
	{
		return Array.Empty<Creature>();
	}

	public Player? GetPlayer(ulong playerId)
	{
		return null;
	}

	public IEnumerable<AbstractModel> IterateHookListeners()
	{
		return Array.Empty<AbstractModel>();
	}

	public void SortEnemiesBySlotName()
	{
	}

	public void SetEnemyIndex(Creature creature, int index)
	{
	}

	public void AddCreature(Creature creature)
	{
	}

	public bool IsLiveCombat()
	{
		return false;
	}
}
