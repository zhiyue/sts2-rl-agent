using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class MessageTypes
{
	private static NetTypeCache<INetMessage>? _cache;

	public static void Initialize()
	{
		List<Type> list = new List<Type>();
		list.AddRange(INetMessageSubtypes.All);
		list.AddRange(ReflectionHelper.GetSubtypesInMods<INetMessage>());
		_cache = new NetTypeCache<INetMessage>(list);
	}

	public static int TypeToId<T>() where T : INetMessage
	{
		return (_cache ?? throw new InvalidOperationException()).TypeToId<T>();
	}

	private static int TypeToId(Type type)
	{
		return (_cache ?? throw new InvalidOperationException()).TypeToId(type);
	}

	public static int ToId(this INetMessage message)
	{
		return (_cache ?? throw new InvalidOperationException()).TypeToId(message.GetType());
	}

	public static bool TryGetMessageType(int id, out Type? type)
	{
		return (_cache ?? throw new InvalidOperationException()).TryGetTypeFromId(id, out type);
	}
}
