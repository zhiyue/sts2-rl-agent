using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Some helper methods for modding.
/// </summary>
public static class ModHelper
{
	private class ModPoolContent
	{
		public bool isFrozen;

		public List<Type>? modelsToAdd;
	}

	private class ModRunHookSubscriber
	{
		public required string id;

		public required RunHookSubscriptionDelegate del;
	}

	private class ModCombatHookSubscriber
	{
		public required string id;

		public required CombatHookSubscriptionDelegate del;
	}

	private static readonly Dictionary<Type, ModPoolContent> _moddedContentForPools = new Dictionary<Type, ModPoolContent>();

	private static readonly List<ModRunHookSubscriber> _runHookSubscribers = new List<ModRunHookSubscriber>();

	private static readonly List<ModCombatHookSubscriber> _combatHookSubscribers = new List<ModCombatHookSubscriber>();

	/// <summary>
	/// Called by mods to add their models to a pool.
	/// Throws if the pool has already concatenated models to that pool (i.e. the mod was too late in calling this method).
	/// </summary>
	/// <typeparam name="TPoolType">The pool type to add to.</typeparam>
	/// <typeparam name="TModelType">The model to add to the pool.</typeparam>
	public static void AddModelToPool<TPoolType, TModelType>() where TPoolType : AbstractModel, IPoolModel where TModelType : AbstractModel
	{
		AddModelToPool(typeof(TPoolType), typeof(TModelType));
	}

	public static void AddModelToPool(Type poolType, Type modelType)
	{
		if (!_moddedContentForPools.TryGetValue(poolType, out ModPoolContent value))
		{
			value = new ModPoolContent
			{
				modelsToAdd = new List<Type>()
			};
			_moddedContentForPools.Add(poolType, value);
		}
		if (value.isFrozen)
		{
			throw new InvalidOperationException($"Tried to add model {modelType} to pool {poolType}, but it's too late! You must add content before the game is initialized.");
		}
		value.modelsToAdd.Add(modelType);
	}

	/// <summary>
	/// Called by a pool when it is ready to add modded content to its pool of content.
	/// </summary>
	/// <param name="poolModel">The pool that is consuming the content.</param>
	/// <param name="pool">The current pool of content.</param>
	/// <typeparam name="TModelType">The model type of the content that should have been added by mods.</typeparam>
	/// <returns>A new pool with all modded content concatenated to pool.</returns>
	public static IEnumerable<TModelType> ConcatModelsFromMods<TModelType>(IPoolModel poolModel, IEnumerable<TModelType> pool) where TModelType : AbstractModel
	{
		Type type = poolModel.GetType();
		if (!_moddedContentForPools.TryGetValue(type, out ModPoolContent value))
		{
			value = new ModPoolContent();
			_moddedContentForPools.Add(type, value);
		}
		value.isFrozen = true;
		if (value.modelsToAdd == null)
		{
			return pool;
		}
		IEnumerable<TModelType> second = value.modelsToAdd.Select((Type t) => ModelDb.GetById<TModelType>(ModelDb.GetId(t)));
		return pool.Concat(second);
	}

	/// <summary>
	/// Called by mods when they wish to provide custom model types to a RunState when IterateHookListeners is called.
	/// </summary>
	/// <param name="id">An ID to identify the subscription. Usually the name of the mod.</param>
	/// <param name="del">The delegate provided by the mod, which returns the models to iterate over.</param>
	public static void SubscribeForRunStateHooks(string id, RunHookSubscriptionDelegate del)
	{
		if (_runHookSubscribers.Any((ModRunHookSubscriber s) => s.id == id))
		{
			Log.Error("Tried to subscribe for RunState hooks with id " + id + ", but it's already been used! Ignoring subscription");
			return;
		}
		_runHookSubscribers.Add(new ModRunHookSubscriber
		{
			id = id,
			del = del
		});
		_runHookSubscribers.Sort((ModRunHookSubscriber x, ModRunHookSubscriber y) => string.CompareOrdinal(x.id, y.id));
	}

	/// <summary>
	/// Called by mods when they wish to provide custom model types to a CombatState when IterateHookListeners is called.
	/// </summary>
	/// <param name="id">An ID to identify the subscription. Usually the name of the mod.</param>
	/// <param name="del">The delegate provided by the mod, which returns the models to iterate over.</param>
	public static void SubscribeForCombatStateHooks(string id, CombatHookSubscriptionDelegate del)
	{
		if (_combatHookSubscribers.Any((ModCombatHookSubscriber s) => s.id == id))
		{
			Log.Error("Tried to subscribe for CombatState hooks with id " + id + ", but it's already been used! Ignoring subscription");
			return;
		}
		_combatHookSubscribers.Add(new ModCombatHookSubscriber
		{
			id = id,
			del = del
		});
		_combatHookSubscribers.Sort((ModCombatHookSubscriber x, ModCombatHookSubscriber y) => string.CompareOrdinal(x.id, y.id));
	}

	/// <summary>
	/// Called by RunState when it is iterating hook listeners, so that custom mod AbstractModel types have hooks called
	/// on them.
	/// </summary>
	/// <param name="runState">The RunState which is iterating hook listeners.</param>
	/// <returns>All the model types from all subscribing mods who wish to receive hooks.</returns>
	public static IEnumerable<AbstractModel> IterateAllRunStateSubscribers(RunState runState)
	{
		foreach (ModRunHookSubscriber runHookSubscriber in _runHookSubscribers)
		{
			IEnumerable<AbstractModel> enumerable = runHookSubscriber.del(runState);
			if (enumerable == null)
			{
				continue;
			}
			foreach (AbstractModel item in enumerable)
			{
				if (item != null)
				{
					yield return item;
				}
			}
		}
	}

	/// <summary>
	/// Called by CombatState when it is iterating hook listeners, so that custom mod AbstractModel types have hooks
	/// called on them.
	/// </summary>
	/// <param name="combatState">The CombatState which is iterating hook listeners.</param>
	/// <returns>All the model types from all subscribing mods who wish to receive hooks.</returns>
	public static IEnumerable<AbstractModel> IterateAllCombatStateSubscribers(CombatState combatState)
	{
		foreach (ModCombatHookSubscriber combatHookSubscriber in _combatHookSubscribers)
		{
			IEnumerable<AbstractModel> enumerable = combatHookSubscriber.del(combatState);
			if (enumerable == null)
			{
				continue;
			}
			foreach (AbstractModel item in enumerable)
			{
				if (item != null)
				{
					yield return item;
				}
			}
		}
	}
}
