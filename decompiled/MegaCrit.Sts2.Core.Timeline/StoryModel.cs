using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Timeline;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties))]
public abstract class StoryModel
{
	private static readonly Dictionary<string, Type> _storyTypeDictionary;

	protected abstract string Id { get; }

	public abstract EpochModel[] Epochs { get; }

	static StoryModel()
	{
		_storyTypeDictionary = new Dictionary<string, Type>();
		for (int i = 0; i < StoryModelSubtypes.Count; i++)
		{
			Type type = StoryModelSubtypes.Get(i);
			StoryModel storyModel = (StoryModel)Activator.CreateInstance(type);
			_storyTypeDictionary[storyModel.Id] = type;
		}
	}

	public static EpochModel? PrevChapter(EpochModel model)
	{
		if (model.StoryId == null)
		{
			return null;
		}
		StoryModel storyModel = Get(StringHelper.Slugify(model.StoryId));
		for (int i = 0; i < storyModel.Epochs.Length; i++)
		{
			if (!(storyModel.Epochs[i].Id == model.Id))
			{
				continue;
			}
			for (i--; i >= 0; i--)
			{
				EpochModel epochModel = storyModel.Epochs[i];
				if (SaveManager.Instance.IsEpochRevealed(epochModel.Id))
				{
					return epochModel;
				}
			}
			return null;
		}
		Log.Error("Epoch: " + model.Id + " was not found in " + storyModel.Id);
		return null;
	}

	public static EpochModel? NextChapter(EpochModel model)
	{
		if (model.StoryId == null)
		{
			return null;
		}
		StoryModel storyModel = Get(StringHelper.Slugify(model.StoryId));
		for (int i = 0; i < storyModel.Epochs.Length; i++)
		{
			if (!(storyModel.Epochs[i].Id == model.Id))
			{
				continue;
			}
			for (i++; i < storyModel.Epochs.Length; i++)
			{
				EpochModel epochModel = storyModel.Epochs[i];
				if (SaveManager.Instance.IsEpochRevealed(epochModel.Id))
				{
					return epochModel;
				}
			}
			return null;
		}
		Log.Error("Epoch: " + model.Id + " was not found in " + storyModel.Id);
		return null;
	}

	public static StoryModel Get(string id)
	{
		if (_storyTypeDictionary.TryGetValue(id, out Type value))
		{
			return (StoryModel)Activator.CreateInstance(value);
		}
		throw new ArgumentException("Story with id '" + id + "' does not exist.");
	}
}
