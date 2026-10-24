using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class PacketWriterExtensions
{
	public static void WriteModel<T>(this PacketWriter writer, T model) where T : AbstractModel
	{
		if (model.IsMutable)
		{
			throw new ArgumentException("Cannot serialize mutable models");
		}
		writer.WriteModelEntry(model.Id);
	}

	public static void WriteModelEntry(this PacketWriter writer, ModelId id)
	{
		if (string.IsNullOrEmpty(id.Category) || string.IsNullOrEmpty(id.Entry))
		{
			throw new InvalidOperationException("Tried to serialize an empty ModelId!");
		}
		if (!ModelIdSerializationCache.TryGetNetIdForEntry(id.Entry, out var netId))
		{
			Log.Warn($"Unknown ModelId entry '{id}' during serialization, writing NONE");
			netId = ModelIdSerializationCache.GetNetIdForEntry(ModelId.none.Entry);
		}
		writer.WriteInt(netId, ModelIdSerializationCache.EntryIdBitSize);
	}

	public static void WriteEpoch<T>(this PacketWriter writer) where T : EpochModel
	{
		writer.WriteInt(ModelIdSerializationCache.GetNetIdForEpochId(EpochModel.GetId<T>()), ModelIdSerializationCache.EpochIdBitSize);
	}

	public static void WriteEpoch(this PacketWriter writer, EpochModel epochModel)
	{
		writer.WriteInt(ModelIdSerializationCache.GetNetIdForEpochId(epochModel.Id), ModelIdSerializationCache.EpochIdBitSize);
	}

	public static void WriteEpochId(this PacketWriter writer, string epochId)
	{
		writer.WriteInt(ModelIdSerializationCache.GetNetIdForEpochId(epochId), ModelIdSerializationCache.EpochIdBitSize);
	}

	public static void WriteFullModelId(this PacketWriter writer, ModelId id)
	{
		int netId;
		bool flag = ModelIdSerializationCache.TryGetNetIdForCategory(id.Category, out netId);
		int netId2;
		bool flag2 = ModelIdSerializationCache.TryGetNetIdForEntry(id.Entry, out netId2);
		if (!flag || !flag2)
		{
			Log.Warn($"Unknown ModelId '{id}' during serialization, writing NONE");
			netId = ModelIdSerializationCache.GetNetIdForCategory(ModelId.none.Category);
			netId2 = ModelIdSerializationCache.GetNetIdForEntry(ModelId.none.Entry);
		}
		writer.WriteInt(netId, ModelIdSerializationCache.CategoryIdBitSize);
		writer.WriteInt(netId2, ModelIdSerializationCache.EntryIdBitSize);
	}

	public static void WriteFullModelIdList(this PacketWriter writer, IReadOnlyCollection<ModelId> models)
	{
		writer.WriteInt(models.Count);
		foreach (ModelId model in models)
		{
			writer.WriteFullModelId(model);
		}
	}

	public static void WriteModelList<T>(this PacketWriter writer, IReadOnlyCollection<T> models) where T : AbstractModel
	{
		writer.WriteInt(models.Count);
		foreach (T model in models)
		{
			writer.WriteModel(model);
		}
	}

	public static void WriteModelEntriesInList(this PacketWriter writer, IReadOnlyCollection<ModelId> modelIds)
	{
		writer.WriteInt(modelIds.Count);
		foreach (ModelId modelId in modelIds)
		{
			writer.WriteModelEntry(modelId);
		}
	}
}
