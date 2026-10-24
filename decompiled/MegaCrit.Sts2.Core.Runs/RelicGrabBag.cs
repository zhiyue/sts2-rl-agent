using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

public class RelicGrabBag
{
	private static readonly HashSet<RelicRarity> _rarities = new HashSet<RelicRarity>
	{
		RelicRarity.Common,
		RelicRarity.Uncommon,
		RelicRarity.Rare,
		RelicRarity.Shop
	};

	/// <summary>
	/// "Deque" is short for Double-Ended Queue.
	/// https://en.wikipedia.org/wiki/Double-ended_queue
	/// </summary>
	private readonly Dictionary<RelicRarity, List<RelicModel>> _deques = new Dictionary<RelicRarity, List<RelicModel>>();

	/// <summary>
	/// This is used in multiplayer for relics that are seen at a shared relic picking treasure chest.
	/// Relics that are chosen by a player are removed from their player's grab bag, and the other relics seen at the
	/// chest are moved to this dequeue.
	/// It is not used at all for the shared relic grab bag.
	/// </summary>
	private readonly List<RelicModel> _mpFallbackDequeue = new List<RelicModel>();

	private readonly bool _refreshAllowed;

	private List<RelicModel>? _originalRelics;

	public bool IsPopulated => _deques.Count > 0;

	/// <summary>
	/// Check if any relics are available to pull. Used by events to determine if they should appear.
	/// </summary>
	public bool HasAvailableRelics(IRunState runState)
	{
		foreach (RelicRarity rarity in _rarities)
		{
			List<RelicModel> availableDeque = GetAvailableDeque(rarity, runState, (RelicModel _) => true);
			if (availableDeque != null)
			{
				return true;
			}
		}
		return _mpFallbackDequeue.Any((RelicModel r) => r.IsAllowed(runState));
	}

	public RelicGrabBag()
	{
	}

	public RelicGrabBag(bool refreshAllowed)
	{
		_refreshAllowed = refreshAllowed;
	}

	public void Populate(Player player, Rng rng)
	{
		if (IsPopulated)
		{
			throw new InvalidOperationException("Grab bag was already populated.");
		}
		List<RelicModel> list = ModelDb.RelicPool<SharedRelicPool>().GetUnlockedRelics(player.UnlockState).ToList();
		list.AddRange(player.Character.RelicPool.GetUnlockedRelics(player.UnlockState));
		list.RemoveAll((RelicModel r) => !_rarities.Contains(r.Rarity));
		_originalRelics = list;
		foreach (RelicModel item in list)
		{
			if (!_deques.TryGetValue(item.Rarity, out List<RelicModel> value))
			{
				value = new List<RelicModel>();
				_deques[item.Rarity] = value;
			}
			value.Add(item);
		}
		foreach (List<RelicModel> value2 in _deques.Values)
		{
			value2.UnstableShuffle(rng);
		}
	}

	public void Populate(IEnumerable<RelicModel> relics, Rng rng)
	{
		if (IsPopulated)
		{
			throw new InvalidOperationException("Grab bag was already populated.");
		}
		_originalRelics = relics.ToList();
		foreach (RelicModel originalRelic in _originalRelics)
		{
			if (!_deques.TryGetValue(originalRelic.Rarity, out List<RelicModel> value))
			{
				value = new List<RelicModel>();
				_deques[originalRelic.Rarity] = value;
			}
			value.Add(originalRelic);
		}
		foreach (List<RelicModel> value2 in _deques.Values)
		{
			value2.UnstableShuffle(rng);
		}
	}

	public RelicModel? PullFromFront(RelicRarity rarity, IRunState runState)
	{
		return PullFromFront(rarity, (RelicModel _) => true, runState);
	}

	/// <summary>
	/// Get the front relic from the bucket of the specified rarity and remove it from the bucket.
	/// Used by most relic reward sources (elite combat rewards etc).
	/// </summary>
	/// <param name="rarity">Rarity of the relic we want.</param>
	/// <param name="runState">The current run state, used to check if relics are allowed.</param>
	/// <param name="filter">Filter for what type of relics we are allowed to return.</param>
	/// <returns>RelicModel</returns>
	public RelicModel? PullFromFront(RelicRarity rarity, Func<RelicModel, bool> filter, IRunState runState)
	{
		List<RelicModel> availableDeque = GetAvailableDeque(rarity, runState, filter);
		if (availableDeque == null || availableDeque.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < availableDeque.Count; i++)
		{
			RelicModel relicModel = availableDeque[i];
			if (filter(relicModel))
			{
				availableDeque.RemoveAt(i);
				return relicModel;
			}
		}
		return null;
	}

	/// <summary>
	/// Get the back relic from the bucket of the specified rarity and remove it from the bucket.
	/// Used in shops.
	/// </summary>
	/// <param name="rarity">Rarity of the relic we want.</param>
	/// <param name="filter">Filter for what type of relics we are allowed to return.</param>
	/// <param name="runState">The current run state, used to check if relics are allowed.</param>
	/// <returns>RelicModel</returns>
	public RelicModel? PullFromBack(RelicRarity rarity, Func<RelicModel, bool> filter, IRunState runState)
	{
		List<RelicModel> availableDeque = GetAvailableDeque(rarity, runState, filter);
		if (availableDeque == null || availableDeque.Count == 0)
		{
			return null;
		}
		for (int num = availableDeque.Count - 1; num >= 0; num--)
		{
			RelicModel relicModel = availableDeque[num];
			if (filter(relicModel))
			{
				availableDeque.RemoveAt(num);
				return relicModel;
			}
		}
		return null;
	}

	/// <summary>
	/// Removes the specified relic from the bucket of the appropriate rarity so that it is never rolled.
	/// If it's already not there, then nothing happens.
	/// </summary>
	public void Remove<T>() where T : RelicModel
	{
		Remove(ModelDb.Relic<T>());
	}

	public void Remove(RelicModel relic)
	{
		foreach (KeyValuePair<RelicRarity, List<RelicModel>> deque in _deques)
		{
			List<RelicModel> value = deque.Value;
			value.RemoveAll((RelicModel r) => r.Id == relic.Id);
		}
	}

	public void MoveToFallback(RelicModel toRemove)
	{
		RelicModel relicModel = null;
		foreach (KeyValuePair<RelicRarity, List<RelicModel>> deque in _deques)
		{
			List<RelicModel> value = deque.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].Id == toRemove.Id)
				{
					if (relicModel == null)
					{
						relicModel = value[i];
					}
					value.RemoveAt(i);
					i--;
				}
			}
		}
		if (relicModel != null)
		{
			_mpFallbackDequeue.Add(relicModel);
		}
	}

	private List<RelicModel>? GetAvailableDeque(RelicRarity rarity, IRunState runState, Func<RelicModel, bool> filter)
	{
		RemoveDisallowedRelicsFromDeques(runState);
		List<RelicModel> list = GetDeque(rarity);
		if (list.Count == 0 && _refreshAllowed)
		{
			RefreshRarity(rarity);
			RemoveDisallowedRelicsFromDeques(runState);
		}
		while (list != null && !DequeHasAnyRelics(list, filter))
		{
			rarity = rarity switch
			{
				RelicRarity.Shop => RelicRarity.Common, 
				RelicRarity.Common => RelicRarity.Uncommon, 
				RelicRarity.Uncommon => RelicRarity.Rare, 
				_ => RelicRarity.None, 
			};
			list = ((rarity == RelicRarity.None) ? null : GetDeque(rarity));
		}
		if (list == null && DequeHasAnyRelics(_mpFallbackDequeue, filter))
		{
			list = _mpFallbackDequeue;
		}
		return list;
	}

	private bool DequeHasAnyRelics(List<RelicModel> deque, Func<RelicModel, bool> filter)
	{
		return deque.Any(filter.Invoke);
	}

	private void RemoveDisallowedRelicsFromDeques(IRunState runState)
	{
		foreach (KeyValuePair<RelicRarity, List<RelicModel>> deque in _deques)
		{
			List<RelicModel> value = deque.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (!value[i].IsAllowed(runState))
				{
					value.RemoveAt(i);
					i--;
				}
			}
		}
		for (int j = 0; j < _mpFallbackDequeue.Count; j++)
		{
			if (!_mpFallbackDequeue[j].IsAllowed(runState))
			{
				_mpFallbackDequeue.RemoveAt(j);
				j--;
			}
		}
	}

	public SerializableRelicGrabBag ToSerializable()
	{
		SerializableRelicGrabBag serializableRelicGrabBag = new SerializableRelicGrabBag();
		foreach (KeyValuePair<RelicRarity, List<RelicModel>> deque in _deques)
		{
			deque.Deconstruct(out var key, out var value);
			RelicRarity key2 = key;
			List<RelicModel> source = value;
			List<ModelId> value2 = source.Select((RelicModel r) => r.Id).ToList();
			serializableRelicGrabBag.RelicIdLists[key2] = value2;
		}
		return serializableRelicGrabBag;
	}

	public static RelicGrabBag FromSerializable(SerializableRelicGrabBag save)
	{
		RelicGrabBag relicGrabBag = new RelicGrabBag();
		relicGrabBag.LoadFromSerializable(save);
		return relicGrabBag;
	}

	public void LoadFromSerializable(SerializableRelicGrabBag save)
	{
		foreach (var (key, list2) in save.RelicIdLists)
		{
			if (!_deques.TryGetValue(key, out List<RelicModel> value))
			{
				value = new List<RelicModel>();
				_deques[key] = value;
			}
			value.Clear();
			foreach (ModelId item in list2)
			{
				RelicModel byIdOrNull = ModelDb.GetByIdOrNull<RelicModel>(item);
				if (byIdOrNull != null)
				{
					value.Add(byIdOrNull);
				}
			}
		}
	}

	private List<RelicModel> GetDeque(RelicRarity rarity)
	{
		if (!_deques.TryGetValue(rarity, out List<RelicModel> value))
		{
			return new List<RelicModel>();
		}
		return value;
	}

	private void RefreshRarity(RelicRarity rarity)
	{
		if (_originalRelics == null)
		{
			throw new InvalidOperationException("Tried to refresh relics but original list is null");
		}
		foreach (RelicModel originalRelic in _originalRelics)
		{
			if (originalRelic.Rarity == rarity)
			{
				if (!_deques.TryGetValue(originalRelic.Rarity, out List<RelicModel> value))
				{
					value = new List<RelicModel>();
					_deques[originalRelic.Rarity] = value;
				}
				value.Add(originalRelic);
			}
		}
	}

	public bool Contains(RelicModel relic)
	{
		relic.AssertCanonical();
		foreach (KeyValuePair<RelicRarity, List<RelicModel>> deque in _deques)
		{
			if (deque.Value.Contains(relic))
			{
				return true;
			}
		}
		return false;
	}
}
