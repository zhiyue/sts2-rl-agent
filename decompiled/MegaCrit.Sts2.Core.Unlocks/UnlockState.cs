using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Unlocks;

/// <summary>
/// Represents the state of unlocked content for a given player.
/// The local player can encapsulate this state in their progress save. The real place this class is useful is in
/// multiplayer scenarios, where different players have different unlock states.
///
/// This is used in three places:
///  - In the <see cref="T:MegaCrit.Sts2.Core.Entities.Players.Player" /> class, to query a specific player's unlock state (e.g. for rewards or Attack potion generation)
///  - In the <see cref="T:MegaCrit.Sts2.Core.Runs.RunState" /> class, to query the superset of all players' unlock states (e.g. for ancient generation)
///  - In the <see cref="T:MegaCrit.Sts2.Core.Saves.SaveManager" /> class, to query the local player's unlock state outside of a run (e.g. in the compendium)
///
/// For multiplayer, keep in mind which one is appropriate to use at a given time. Using the third when one of the first
/// two should be used will result in a state divergence.
///
/// In singleplayer, the first two are equivalent, but you should still consider when using the third is appropriate.
/// If you use the third during a run, epochs unlocked between a save and a load may be inappropriately included in the
/// run.
/// </summary>
public class UnlockState
{
	public static readonly UnlockState none = new UnlockState(Array.Empty<string>(), Array.Empty<ModelId>(), 0);

	public static readonly UnlockState all = new UnlockState(EpochModel.AllEpochIds, ModelDb.AllEncounters.Select((EncounterModel e) => e.Id), 999999999);

	/// <summary>
	/// The total set of epochs revealed.
	/// If this is the shared unlock set, this is the superset of all epochs from all players.
	/// </summary>
	private readonly HashSet<string> _unlockedEpochIds;

	/// <summary>
	/// The encounter IDs this player has seen (lost or won).
	/// If this is the shared unlock set, this is the superset of all encounters seen by all players.
	/// </summary>
	private readonly HashSet<ModelId> _encountersSeen;

	/// <summary>
	/// Ths number of runs the player has done.
	/// If this is the shared unlock set, this is the **maximum** number of runs among all players.
	/// </summary>
	public int NumberOfRuns { get; }

	/// <summary>
	/// Returns the all the characters that the player has unlocked.
	/// </summary>
	public IEnumerable<CharacterModel> Characters
	{
		get
		{
			List<CharacterModel> list = ModelDb.AllCharacters.ToList();
			if (!IsEpochRevealed<Silent1Epoch>())
			{
				list.Remove(ModelDb.Character<Silent>());
			}
			if (!IsEpochRevealed<Regent1Epoch>())
			{
				list.Remove(ModelDb.Character<Regent>());
			}
			if (!IsEpochRevealed<Necrobinder1Epoch>())
			{
				list.Remove(ModelDb.Character<Necrobinder>());
			}
			if (!IsEpochRevealed<Defect1Epoch>())
			{
				list.Remove(ModelDb.Character<Defect>());
			}
			return list;
		}
	}

	/// <summary>
	/// Get all the ancients in the game that don't belong to a specific act and that the player has unlocked.
	/// </summary>
	public IEnumerable<AncientEventModel> SharedAncients
	{
		get
		{
			List<AncientEventModel> list = ModelDb.AllSharedAncients.ToList();
			if (!IsEpochRevealed<DarvEpoch>())
			{
				list.Remove(ModelDb.AncientEvent<Darv>());
			}
			return list;
		}
	}

	/// <summary>
	/// Get all the relics in the game that the player has unlocked.
	/// </summary>
	public IEnumerable<RelicModel> Relics => ModelDb.AllRelicPools.Select((RelicPoolModel p) => p.GetUnlockedRelics(this)).SelectMany((IEnumerable<RelicModel> r) => r);

	/// <summary>
	/// Get all the relics in the game that the player has unlocked.
	/// </summary>
	public IEnumerable<PotionModel> Potions => ModelDb.AllPotionPools.Select((PotionPoolModel p) => p.GetUnlockedPotions(this)).SelectMany((IEnumerable<PotionModel> r) => r);

	/// <summary>
	/// Get all the card pools in the game that belong to specific characters and that the player has unlocked.
	/// </summary>
	public IEnumerable<CardPoolModel> CharacterCardPools => Characters.Select((CharacterModel c) => c.CardPool);

	/// <summary>
	/// Get all the cards in the game that the player has unlocked.
	/// Be careful using this, it includes cards that you shouldn't be able to randomly roll for rewards.
	/// </summary>
	public IEnumerable<CardModel> Cards => CardPools.SelectMany((CardPoolModel p) => p.AllCards).Concat(Characters.SelectMany((CharacterModel c) => c.StartingDeck).Distinct()).Distinct();

	/// <summary>
	/// Get all the card pools in the game that the player has unlocked.
	/// </summary>
	public IEnumerable<CardPoolModel> CardPools => CharacterCardPools.Concat(ModelDb.AllSharedCardPools).Distinct();

	/// <summary>
	/// Returns true if the player has seen this encounter (won or lost).
	/// If this is the multiplayer unlock state, returns true if any player has seen the encounter.
	/// </summary>
	public bool HasSeenEncounter(EncounterModel encounter)
	{
		return _encountersSeen.Contains(encounter.Id);
	}

	/// <summary>
	/// Injection constructor.
	/// </summary>
	public UnlockState(IEnumerable<string> unlockedEpochIds, IEnumerable<ModelId> encountersSeen, int numberOfRuns)
	{
		_unlockedEpochIds = unlockedEpochIds.ToHashSet();
		_encountersSeen = encountersSeen.ToHashSet();
		NumberOfRuns = numberOfRuns;
	}

	/// <summary>
	/// Produces an unlock state from a progress state. Only use this to derive an unlock state for the local player.
	/// </summary>
	/// <param name="progress">The progress state to derive the unlock state from.</param>
	public UnlockState(ProgressState progress)
	{
		_unlockedEpochIds = (from e in progress.Epochs
			where e.State == EpochState.Revealed
			select e.Id).ToHashSet();
		_encountersSeen = progress.EncounterStats.Keys.Where((ModelId id) => ModelDb.GetByIdOrNull<AbstractModel>(id) is EncounterModel).ToHashSet();
		NumberOfRuns = progress.NumberOfRuns;
	}

	/// <summary>
	/// Produces an unlock state that is the union of all passed unlock states.
	/// In essence: In a multiplayer game, if one person has something unlocked, it will be unlocked for everyone in the
	/// multiplayer session.
	/// </summary>
	/// <param name="unlockStatesEnumerable">Set of states to merge together.</param>
	public UnlockState(IEnumerable<UnlockState> unlockStatesEnumerable)
	{
		UnlockState[] source = unlockStatesEnumerable.ToArray();
		_unlockedEpochIds = source.Select((UnlockState s) => s._unlockedEpochIds).SelectMany((HashSet<string> m) => m).Distinct()
			.ToHashSet();
		_encountersSeen = source.Select((UnlockState s) => s._encountersSeen).SelectMany((HashSet<ModelId> b) => b).Distinct()
			.ToHashSet();
		NumberOfRuns = source.Max((UnlockState s) => s.NumberOfRuns);
	}

	/// <summary>
	/// Checks if this player has revealed an epoch on the timeline.
	/// </summary>
	/// <typeparam name="T">The type of epoch.</typeparam>
	public bool IsEpochRevealed<T>() where T : EpochModel
	{
		return _unlockedEpochIds.Contains(EpochModel.GetId<T>());
	}

	/// <returns>The amount of epochs the player has unlocked.</returns>
	public int EpochUnlockCount()
	{
		return _unlockedEpochIds.Count;
	}

	public SerializableUnlockState ToSerializable()
	{
		return new SerializableUnlockState
		{
			UnlockedEpochs = _unlockedEpochIds.ToList(),
			EncountersSeen = _encountersSeen.ToList(),
			NumberOfRuns = NumberOfRuns
		};
	}

	public static UnlockState FromSerializable(SerializableUnlockState unlockState)
	{
		if (unlockState == null)
		{
			return all;
		}
		List<ModelId> encountersSeen = (from e in unlockState.EncountersSeen.Select(SaveUtil.EncounterOrDeprecated)
			select e.Id).ToHashSet().ToList();
		return new UnlockState(unlockState.UnlockedEpochs, encountersSeen, unlockState.NumberOfRuns);
	}
}
