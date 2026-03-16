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

	private readonly Dictionary<RelicRarity, List<RelicModel>> _deques = new Dictionary<RelicRarity, List<RelicModel>>();

	private readonly List<RelicModel> _mpFallbackDequeue = new List<RelicModel>();

	private readonly bool _refreshAllowed;

	private List<RelicModel>? _originalRelics;

	public bool IsPopulated => _deques.Count > 0;

	public bool HasAvailableRelics(IRunState runState)
	{
		foreach (RelicRarity rarity in _rarities)
		{
			List<RelicModel> availableDeque = GetAvailableDeque(rarity);
			if (availableDeque != null && availableDeque.Any((RelicModel r) => r.IsAllowed(runState)))
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
		List<RelicModel> availableDeque = GetAvailableDeque(rarity);
		if (availableDeque == null || availableDeque.Count == 0)
		{
			return null;
		}
		RelicModel relicModel = availableDeque[0];
		while (!relicModel.IsAllowed(runState))
		{
			availableDeque.RemoveAt(0);
			if (availableDeque.Count == 0)
			{
				if (!_refreshAllowed)
				{
					return null;
				}
				RefreshRarity(rarity);
			}
			relicModel = availableDeque[0];
		}
		availableDeque.RemoveAt(0);
		return relicModel;
	}

	public RelicModel? PullFromBack(RelicRarity rarity, IEnumerable<RelicModel> blacklist, IRunState runState)
	{
		foreach (RelicModel item in blacklist)
		{
			item.AssertCanonical();
		}
		List<RelicModel> availableDeque = GetAvailableDeque(rarity);
		if (availableDeque == null || availableDeque.Count == 0)
		{
			return null;
		}
		int num = availableDeque.Count - 1;
		while (blacklist.Contains(availableDeque[num]) || !availableDeque[num].IsAllowed(runState))
		{
			num--;
			if (num < 0)
			{
				return null;
			}
		}
		RelicModel result = availableDeque[num];
		availableDeque.RemoveAt(num);
		return result;
	}

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

	private List<RelicModel>? GetAvailableDeque(RelicRarity rarity)
	{
		List<RelicModel> list = GetDeque(rarity);
		if (list.Count == 0 && _refreshAllowed)
		{
			RefreshRarity(rarity);
		}
		while (list != null && list.Count == 0)
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
		if (list == null && _mpFallbackDequeue.Count > 0)
		{
			list = _mpFallbackDequeue;
		}
		return list;
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
}
