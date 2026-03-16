using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public static class ActionTypes
{
	private static readonly NetTypeCache<INetAction> _cache;

	static ActionTypes()
	{
		List<Type> list = new List<Type>();
		list.AddRange(INetActionSubtypes.All);
		list.AddRange(ReflectionHelper.GetSubtypesInMods<INetAction>());
		_cache = new NetTypeCache<INetAction>(list);
	}

	public static int TypeToId<T>() where T : INetAction
	{
		return _cache.TypeToId<T>();
	}

	private static int TypeToId(Type type)
	{
		return _cache.TypeToId(type);
	}

	public static int ToId(this INetAction message)
	{
		return _cache.TypeToId(message.GetType());
	}

	public static bool TryGetActionType(int id, out Type? type)
	{
		return _cache.TryGetTypeFromId(id, out type);
	}
}
