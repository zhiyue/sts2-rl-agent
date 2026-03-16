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

public class UnlockState
{
	public static readonly UnlockState none = new UnlockState(Array.Empty<string>(), Array.Empty<ModelId>(), 0);

	public static readonly UnlockState all = new UnlockState(EpochModel.AllEpochIds, ModelDb.AllEncounters.Select((EncounterModel e) => e.Id), 9999);

	private readonly HashSet<string> _unlockedEpochIds;

	private readonly HashSet<ModelId> _encountersSeen;

	public int NumberOfRuns { get; }

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

	public IEnumerable<RelicModel> Relics => ModelDb.AllRelicPools.Select((RelicPoolModel p) => p.GetUnlockedRelics(this)).SelectMany((IEnumerable<RelicModel> r) => r);

	public IEnumerable<PotionModel> Potions => ModelDb.AllPotionPools.Select((PotionPoolModel p) => p.GetUnlockedPotions(this)).SelectMany((IEnumerable<PotionModel> r) => r);

	public IEnumerable<CardPoolModel> CharacterCardPools => Characters.Select((CharacterModel c) => c.CardPool);

	public IEnumerable<CardModel> Cards => CardPools.SelectMany((CardPoolModel p) => p.AllCards).Concat(Characters.SelectMany((CharacterModel c) => c.StartingDeck).Distinct()).Distinct();

	public IEnumerable<CardPoolModel> CardPools => CharacterCardPools.Concat(ModelDb.AllSharedCardPools).Distinct();

	public bool HasSeenEncounter(EncounterModel encounter)
	{
		return _encountersSeen.Contains(encounter.Id);
	}

	public UnlockState(IEnumerable<string> unlockedEpochIds, IEnumerable<ModelId> encountersSeen, int numberOfRuns)
	{
		_unlockedEpochIds = unlockedEpochIds.ToHashSet();
		_encountersSeen = encountersSeen.ToHashSet();
		NumberOfRuns = numberOfRuns;
	}

	public UnlockState(ProgressState progress)
	{
		_unlockedEpochIds = (from e in progress.Epochs
			where e.State == EpochState.Revealed
			select e.Id).ToHashSet();
		_encountersSeen = progress.EncounterStats.Keys.Where((ModelId id) => ModelDb.GetByIdOrNull<AbstractModel>(id) is EncounterModel).ToHashSet();
		NumberOfRuns = progress.NumberOfRuns;
	}

	public UnlockState(IEnumerable<UnlockState> unlockStatesEnumerable)
	{
		UnlockState[] source = unlockStatesEnumerable.ToArray();
		_unlockedEpochIds = source.Select((UnlockState s) => s._unlockedEpochIds).SelectMany((HashSet<string> m) => m).Distinct()
			.ToHashSet();
		_encountersSeen = source.Select((UnlockState s) => s._encountersSeen).SelectMany((HashSet<ModelId> b) => b).Distinct()
			.ToHashSet();
		NumberOfRuns = source.Max((UnlockState s) => s.NumberOfRuns);
	}

	public bool IsEpochRevealed<T>() where T : EpochModel
	{
		return _unlockedEpochIds.Contains(EpochModel.GetId<T>());
	}

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
