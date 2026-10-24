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
	/// <summary>
	/// If this is set, uses this rng rather than the default one used by the particular reward (Typically a player RNG)
	/// Used in the Crystal Sphere Items so that we can pass the event rng to be used and not hit state divergences in multiplayer
	/// since we don't sync the player rng sets in that case
	/// </summary>
	protected Rng? _rngOverride;

	/// <summary>
	/// The player that this reward is for.
	/// </summary>
	public Player Player { get; }

	protected abstract RewardType RewardType { get; }

	/// <summary>
	/// The index to use for this reward when ordering the rewards displayed in a <see cref="T:MegaCrit.Sts2.Core.Rewards.RewardsSet" />.
	/// Lower indexes are displayed first.
	/// </summary>
	public abstract int RewardsSetIndex { get; }

	public abstract LocString Description { get; }

	/// <summary>
	/// Whether this reward has been populated.
	/// </summary>
	public abstract bool IsPopulated { get; }

	/// <summary>
	/// Whether the reward has successfully been taken.
	/// </summary>
	public bool SuccessfullySelected { get; private set; }

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

	/// <summary>
	/// Logic to run to populate this reward with options.
	/// For example, calling this on a CardReward would generate 3 cards for the player to choose from.
	/// </summary>
	public abstract void Populate();

	/// <summary>
	/// Logic to run when this reward is selected.
	/// </summary>
	/// <returns>
	/// Whether the reward was received. Usually true, but false in certain cases (like if the player tries to
	/// take a potion when they have no more room for potions).
	/// </returns>
	protected abstract Task<bool> OnSelect();

	/// <summary>
	/// Create an icon node representing this reward.
	/// Null if we're in test mode.
	/// </summary>
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

	/// <summary>
	/// YOU SHOULD MOST LIKELY NOT BE CALLING THIS!
	/// See <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Game.RewardsSetSynchronizer.SelectLocalReward(MegaCrit.Sts2.Core.Rewards.Reward)" />.
	/// This only selects the reward on the local machine.
	/// </summary>
	public async Task<bool> SelectUnsynchronized()
	{
		bool success = await OnSelect();
		if (success)
		{
			await Hook.AfterRewardTaken(Player.RunState, Player, this);
			SuccessfullySelected = true;
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
			if (save.PredeterminedModelId != ModelId.none)
			{
				return new RelicReward(ModelDb.GetById<RelicModel>(save.PredeterminedModelId).ToMutable(), player);
			}
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
