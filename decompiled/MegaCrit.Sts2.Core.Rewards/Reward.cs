using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rewards;

public abstract class Reward
{
	protected Rng? _rngOverride;

	public Player Player { get; }

	protected abstract RewardType RewardType { get; }

	public abstract int RewardsSetIndex { get; }

	public abstract LocString Description { get; }

	public abstract bool IsPopulated { get; }

	protected virtual string? IconPath => null;

	public virtual Vector2 IconPosition => Vector2.Zero;

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public virtual IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			List<IHoverTip> list = ExtraHoverTips.ToList();
			if (ParentRewardSet != null)
			{
				list.Add(LinkedRewardSet.HoverTip);
			}
			return list;
		}
	}

	public LinkedRewardSet? ParentRewardSet { get; set; }

	protected Reward(Player player)
	{
		Player = player;
	}

	public abstract Task Populate();

	protected abstract Task<bool> OnSelect();

	public virtual Control? CreateIcon()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		TextureRect textureRect = new TextureRect();
		textureRect.Texture = PreloadManager.Cache.GetCompressedTexture2D(IconPath);
		textureRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		return textureRect;
	}

	public virtual void OnSkipped()
	{
	}

	public async Task<bool> OnSelectWrapper()
	{
		bool success = await OnSelect();
		if (success)
		{
			await Hook.AfterRewardTaken(Player.RunState, Player, this);
		}
		if (ParentRewardSet != null)
		{
			ParentRewardSet.RemoveReward(this);
			await ParentRewardSet.OnSelect();
		}
		return success;
	}

	public abstract void MarkContentAsSeen();

	public virtual SerializableReward ToSerializable()
	{
		return new SerializableReward
		{
			RewardType = RewardType
		};
	}

	public Reward SetRng(Rng rng)
	{
		_rngOverride = rng;
		return this;
	}

	public static Reward FromSerializable(SerializableReward save, Player player)
	{
		switch (save.RewardType)
		{
		case RewardType.RemoveCard:
			return new CardRemovalReward(player);
		case RewardType.SpecialCard:
		{
			CardModel cardModel = CardModel.FromSerializable(save.SpecialCard);
			player.RunState.AddCard(cardModel, player);
			SpecialCardReward specialCardReward = new SpecialCardReward(cardModel, player);
			if (save.CustomDescriptionEncounterSourceId != ModelId.none)
			{
				specialCardReward.SetCustomDescriptionEncounterSource(save.CustomDescriptionEncounterSourceId);
			}
			return specialCardReward;
		}
		case RewardType.Gold:
			return new GoldReward(save.GoldAmount, player, save.WasGoldStolenBack);
		case RewardType.Potion:
			return new PotionReward(player);
		case RewardType.Relic:
			return new RelicReward(player);
		case RewardType.Card:
		{
			CardCreationOptions options = new CardCreationOptions(save.CardPoolIds.Select(ModelDb.GetById<CardPoolModel>), save.Source, save.RarityOdds);
			return new CardReward(options, save.OptionCount, player);
		}
		default:
			throw new NotImplementedException("Serializing these types of rewards hasn't been implemented yet");
		}
	}
}
