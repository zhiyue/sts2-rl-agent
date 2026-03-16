using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Modding;

public static class ModHelper
{
	private class ModPoolContent
	{
		public bool isFrozen;

		public List<Type>? modelsToAdd;
	}

	private static readonly Dictionary<Type, ModPoolContent> _moddedContentForPools = new Dictionary<Type, ModPoolContent>();

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
}
