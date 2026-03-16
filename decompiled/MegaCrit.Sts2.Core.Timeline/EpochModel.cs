using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Timeline;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public abstract class EpochModel
{
	private static IReadOnlyList<string>? _allEpochIds;

	private static readonly Dictionary<string, Type> _epochTypeDictionary;

	private static readonly Dictionary<Type, string> _typeToIdDictionary;

	private static IEnumerable<string> EpochIds => new global::_003C_003Ez__ReadOnlyArray<string>(new string[57]
	{
		GetId<Act2BEpoch>(),
		GetId<Act3BEpoch>(),
		GetId<Colorless1Epoch>(),
		GetId<Colorless2Epoch>(),
		GetId<Colorless3Epoch>(),
		GetId<Colorless4Epoch>(),
		GetId<Colorless5Epoch>(),
		GetId<CustomAndSeedsEpoch>(),
		GetId<DailyRunEpoch>(),
		GetId<DarvEpoch>(),
		GetId<Defect1Epoch>(),
		GetId<Defect2Epoch>(),
		GetId<Defect3Epoch>(),
		GetId<Defect4Epoch>(),
		GetId<Defect5Epoch>(),
		GetId<Defect6Epoch>(),
		GetId<Defect7Epoch>(),
		GetId<Event1Epoch>(),
		GetId<Event2Epoch>(),
		GetId<Event3Epoch>(),
		GetId<Ironclad2Epoch>(),
		GetId<Ironclad3Epoch>(),
		GetId<Ironclad4Epoch>(),
		GetId<Ironclad5Epoch>(),
		GetId<Ironclad6Epoch>(),
		GetId<Ironclad7Epoch>(),
		GetId<Necrobinder1Epoch>(),
		GetId<Necrobinder2Epoch>(),
		GetId<Necrobinder3Epoch>(),
		GetId<Necrobinder4Epoch>(),
		GetId<Necrobinder5Epoch>(),
		GetId<Necrobinder6Epoch>(),
		GetId<Necrobinder7Epoch>(),
		GetId<NeowEpoch>(),
		GetId<OrobasEpoch>(),
		GetId<Potion1Epoch>(),
		GetId<Potion2Epoch>(),
		GetId<Regent1Epoch>(),
		GetId<Regent2Epoch>(),
		GetId<Regent3Epoch>(),
		GetId<Regent4Epoch>(),
		GetId<Regent5Epoch>(),
		GetId<Regent6Epoch>(),
		GetId<Regent7Epoch>(),
		GetId<Relic1Epoch>(),
		GetId<Relic2Epoch>(),
		GetId<Relic3Epoch>(),
		GetId<Relic4Epoch>(),
		GetId<Relic5Epoch>(),
		GetId<Silent1Epoch>(),
		GetId<Silent2Epoch>(),
		GetId<Silent3Epoch>(),
		GetId<Silent4Epoch>(),
		GetId<Silent5Epoch>(),
		GetId<Silent6Epoch>(),
		GetId<Silent7Epoch>(),
		GetId<UnderdocksEpoch>()
	});

	public static IReadOnlyList<string> AllEpochIds => _allEpochIds ?? (_allEpochIds = EpochIds.ToList());

	public string Year => new LocString("eras", StringHelper.Slugify(Era.ToString()) + ".year").GetFormattedText();

	public string EraName => new LocString("eras", StringHelper.Slugify(Era.ToString()) + ".name").GetFormattedText();

	public abstract string Id { get; }

	public ModelId ModelId => new ModelId("epoch", Id);

	public virtual bool IsArtPlaceholder => true;

	public LocString Title => new LocString("epochs", Id + ".title");

	public string Description => new LocString("epochs", Id + ".description").GetFormattedText();

	public string? StoryTitle
	{
		get
		{
			if (StoryId == null)
			{
				return null;
			}
			return new LocString("epochs", "STORY_" + StoryId.ToUpperInvariant()).GetRawText();
		}
	}

	public virtual string? StoryId => null;

	public LocString UnlockInfo => new LocString("epochs", Id + ".unlockInfo");

	public virtual string UnlockText => new LocString("epochs", Id + ".unlockText").GetFormattedText();

	public abstract EpochEra Era { get; }

	public abstract int EraPosition { get; }

	public Texture2D Portrait => ResourceLoader.Load<Texture2D>(PackedPortraitPath, null, ResourceLoader.CacheMode.Reuse);

	public string PackedPortraitPath => ImageHelper.GetImagePath("atlases/epoch_atlas.sprites/" + Id.ToLowerInvariant() + ".tres");

	public Texture2D BigPortrait => ResourceLoader.Load<Texture2D>(BigPortraitPath, null, ResourceLoader.CacheMode.Reuse);

	public string BigPortraitPath => ImageHelper.GetImagePath("timeline/epoch_portraits/" + Id.ToLowerInvariant() + ".png");

	public int ChapterIndex
	{
		get
		{
			if (StoryId == null)
			{
				return -1;
			}
			EpochModel[] epochs = StoryModel.Get(StringHelper.Slugify(StoryId)).Epochs;
			for (int i = 0; i < epochs.Length; i++)
			{
				if (epochs[i].Id == Id)
				{
					return i + 1;
				}
			}
			return -1;
		}
	}

	static EpochModel()
	{
		_epochTypeDictionary = new Dictionary<string, Type>();
		_typeToIdDictionary = new Dictionary<Type, string>();
		for (int i = 0; i < EpochModelSubtypes.Count; i++)
		{
			Type type = EpochModelSubtypes.Get(i);
			EpochModel epochModel = (EpochModel)Activator.CreateInstance(type);
			_epochTypeDictionary[epochModel.Id] = type;
			_typeToIdDictionary[type] = epochModel.Id;
		}
	}

	public virtual EpochModel[] GetTimelineExpansion()
	{
		return Array.Empty<EpochModel>();
	}

	public virtual void QueueUnlocks()
	{
	}

	public static string GetId<T>() where T : EpochModel
	{
		return _typeToIdDictionary[typeof(T)];
	}

	public static string GetId(Type t)
	{
		return _typeToIdDictionary[t];
	}

	public static bool IsValid(string id)
	{
		return AllEpochIds.Any((string epoch) => epoch.Equals(id));
	}

	public static EpochModel Get(string id)
	{
		if (_epochTypeDictionary.TryGetValue(id, out Type value))
		{
			return (EpochModel)Activator.CreateInstance(value);
		}
		throw new ArgumentException("Epoch with id '" + id + "' does not exist.");
	}

	public static EpochModel Get<T>() where T : EpochModel
	{
		return Get(GetId<T>());
	}

	protected static void QueueTimelineExpansion(EpochModel[] epochs)
	{
		Log.Info("Queueing a Timeline expansion...");
		List<EpochSlotData> list = new List<EpochSlotData>();
		foreach (EpochModel epoch in epochs)
		{
			SerializableEpoch serializableEpoch = SaveManager.Instance.Progress.Epochs.FirstOrDefault((SerializableEpoch e) => e.Id == epoch.Id);
			if (serializableEpoch != null && serializableEpoch.State == EpochState.ObtainedNoSlot)
			{
				Log.Info("We have it already Yay: " + serializableEpoch.Id);
				list.Add(new EpochSlotData(epoch, EpochSlotState.Obtained));
			}
			else
			{
				list.Add(new EpochSlotData(epoch, EpochSlotState.NotObtained));
			}
		}
		NTimelineScreen.Instance.QueueTimelineExpansion(list);
		foreach (EpochModel epochModel in epochs)
		{
			SaveManager.Instance.UnlockSlot(epochModel.Id);
		}
	}

	protected string CreateCardUnlockText(List<CardModel> cards)
	{
		LocString locString = new LocString("timeline", "UNLOCK_TEXT.cards");
		cards = cards.OrderBy((CardModel c) => c.Rarity).ToList();
		for (int num = 0; num < 3; num++)
		{
			locString.Add($"Card{num + 1}", GetColoredCardName(cards[num]));
		}
		return locString.GetFormattedText();
	}

	private string GetColoredCardName(CardModel card)
	{
		if (card.Rarity == CardRarity.Common)
		{
			return card.TitleLocString.GetRawText();
		}
		if (card.Rarity == CardRarity.Uncommon)
		{
			return "[blue]" + card.TitleLocString.GetRawText() + "[/blue]";
		}
		if (card.Rarity == CardRarity.Rare)
		{
			return "[gold]" + card.TitleLocString.GetRawText() + "[/gold]";
		}
		return "ERROR";
	}

	protected string CreateRelicUnlockText(List<RelicModel> relics)
	{
		LocString locString = new LocString("timeline", "UNLOCK_TEXT.relics");
		relics = relics.OrderBy((RelicModel r) => r.Rarity).ToList();
		for (int num = 0; num < 3; num++)
		{
			locString.Add($"Relic{num + 1}", GetColoredRelicName(relics[num]));
		}
		return locString.GetFormattedText();
	}

	private string GetColoredRelicName(RelicModel relic)
	{
		if (relic.Rarity == RelicRarity.Common)
		{
			return relic.Title.GetRawText();
		}
		if (relic.Rarity == RelicRarity.Uncommon)
		{
			return "[blue]" + relic.Title.GetRawText() + "[/blue]";
		}
		if (relic.Rarity == RelicRarity.Rare)
		{
			return "[gold]" + relic.Title.GetRawText() + "[/gold]";
		}
		return "ERROR";
	}

	protected string CreatePotionUnlockText(List<PotionModel> potions)
	{
		LocString locString = new LocString("timeline", "UNLOCK_TEXT.potions");
		potions = potions.OrderBy((PotionModel r) => r.Rarity).ToList();
		for (int num = 0; num < 3; num++)
		{
			locString.Add($"Potion{num + 1}", GetColoredPotionName(potions[num]));
		}
		return locString.GetFormattedText();
	}

	private string GetColoredPotionName(PotionModel potion)
	{
		if (potion.Rarity == PotionRarity.Common)
		{
			return potion.Title.GetRawText();
		}
		if (potion.Rarity == PotionRarity.Uncommon)
		{
			return "[blue]" + potion.Title.GetRawText() + "[/blue]";
		}
		if (potion.Rarity == PotionRarity.Rare)
		{
			return "[gold]" + potion.Title.GetRawText() + "[/gold]";
		}
		return "ERROR";
	}
}
