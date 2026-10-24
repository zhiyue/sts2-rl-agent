using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class PacketReaderExtensions
{
	public static T ReadModel<T>(this PacketReader reader) where T : AbstractModel
	{
		if (typeof(T) == typeof(AbstractModel))
		{
			throw new ArgumentException("T must not be AbstractModel!");
		}
		return ModelDb.GetById<T>(reader.ReadModelIdAssumingType<T>());
	}

	public static ModelId ReadModelIdAssumingType<T>(this PacketReader reader) where T : AbstractModel
	{
		if (typeof(T) == typeof(AbstractModel))
		{
			throw new ArgumentException("T must not be AbstractModel!");
		}
		string entryForNetId = ModelIdSerializationCache.GetEntryForNetId(reader.ReadInt(ModelIdSerializationCache.EntryIdBitSize));
		if (entryForNetId == ModelId.none.Entry)
		{
			return ModelId.none;
		}
		return new ModelId(ModelId.SlugifyCategory(ModelDb.GetCategoryType(typeof(T)).Name), entryForNetId);
	}

	public static ModelId ReadFullModelId(this PacketReader reader)
	{
		string categoryForNetId = ModelIdSerializationCache.GetCategoryForNetId(reader.ReadInt(ModelIdSerializationCache.CategoryIdBitSize));
		string entryForNetId = ModelIdSerializationCache.GetEntryForNetId(reader.ReadInt(ModelIdSerializationCache.EntryIdBitSize));
		if (entryForNetId == ModelId.none.Entry || categoryForNetId == ModelId.none.Category)
		{
			return ModelId.none;
		}
		return new ModelId(categoryForNetId, entryForNetId);
	}

	public static EpochModel ReadEpoch(this PacketReader reader)
	{
		return EpochModel.Get(reader.ReadEpochId());
	}

	public static string ReadEpochId(this PacketReader reader)
	{
		return ModelIdSerializationCache.GetEpochIdForNetId(reader.ReadInt(ModelIdSerializationCache.EpochIdBitSize));
	}

	public static List<ModelId> ReadFullModelIdList(this PacketReader reader)
	{
		List<ModelId> list = new List<ModelId>();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.ReadFullModelId());
		}
		return list;
	}

	public static List<T> ReadModelList<T>(this PacketReader reader) where T : AbstractModel
	{
		List<T> list = new List<T>();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.ReadModel<T>());
		}
		return list;
	}

	public static List<ModelId> ReadModelIdListAssumingType<T>(this PacketReader reader) where T : AbstractModel
	{
		List<ModelId> list = new List<ModelId>();
		int num = reader.ReadInt();
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.ReadModelIdAssumingType<T>());
		}
		return list;
	}
}
