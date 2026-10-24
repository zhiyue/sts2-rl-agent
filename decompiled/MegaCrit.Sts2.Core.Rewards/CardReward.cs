using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rewards;

public class CardReward : Reward
{
	private readonly PlayerChoiceSynchronizer _synchronizer;

	private readonly List<CardCreationResult> _cards = new List<CardCreationResult>();

	/// <summary>
	/// This is true if cards were manually set through the constructor, and Options has an empty card pool. The intent
	/// when this is set to true is that _cards is a static list that should not be re-populated nor changed.
	/// </summary>
	private bool _cardsWereManuallySet;

	private bool _hasBeenRerolled;

	private NCardRewardSelectionScreen? _currentlyShownScreen;

	private static string RareRewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_rare.png");

	private static string UncommonRewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_uncommon.png");

	private static string RewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_card.png");

	protected override RewardType RewardType => RewardType.Card;

	public override int RewardsSetIndex => 5;

	protected override string IconPath
	{
		get
		{
			CardCreationOptions options = Options;
			if ((object)options != null && options.Source == CardCreationSource.Encounter && options.RarityOdds == CardRarityOddsType.BossEncounter)
			{
				return RareRewardIcon;
			}
			if (Options.TryGetSingleRarityInPool().HasValue)
			{
				return _cards[0].Card.Rarity switch
				{
					CardRarity.Rare => RareRewardIcon, 
					CardRarity.Uncommon => UncommonRewardIcon, 
					_ => RewardIcon, 
				};
			}
			return RewardIcon;
		}
	}

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { RareRewardIcon, UncommonRewardIcon, RewardIcon });

	public override LocString Description => new LocString("gameplay_ui", "COMBAT_REWARD_ADD_CARD");

	public IEnumerable<CardModel> Cards => _cards.Select((CardCreationResult e) => e.Card);

	private int OptionCount { get; }

	private CardCreationOptions Options { get; }

	private CardCreationOptions RerollOptions { get; }

	public bool CanReroll { get; set; }

	public bool CanSkip { get; init; } = true;

	public override bool IsPopulated => _cards.Count > 0;

	public event Action? AfterGenerated;

	/// <summary>
	/// Create a card reward from a specified source, using the current character's card pool and the source's
	/// rarity-rolling logic.
	/// This is the typical way one would generate a card reward for something like the end of combat.
	/// </summary>
	/// <param name="options">Options used to generate the card rewards.</param>
	/// <param name="cardCount">How many choices to offer (usually 3).</param>
	/// <param name="player">The player that this reward is for.</param>
	/// <param name="synchronizer">PlayerChoiceSynchronizer, for injection in tests.</param>
	public CardReward(CardCreationOptions options, int cardCount, Player player, PlayerChoiceSynchronizer? synchronizer = null)
		: base(player)
	{
		OptionCount = cardCount;
		Options = options.WithFlags(CardCreationFlags.IsCardReward);
		RerollOptions = options.WithFlags(CardCreationFlags.IsCardReward);
		_synchronizer = synchronizer ?? RunManager.Instance.PlayerChoiceSynchronizer;
		player.RelicObtained += OnRelicObtained;
	}

	/// <summary>
	/// Create a card reward with specific cards in it. This is for exceptionally rare cases where we want to offer a
	/// very specific set of cards that ignores modification, like during the tutorial run.
	/// Note that <see cref="M:MegaCrit.Sts2.Core.Rewards.CardReward.Populate" /> will do nothing if called after this method.
	/// </summary>
	/// <param name="cardsToOffer">List of cards to offer as rewards.</param>
	/// <param name="source">The source of the reward.</param>
	/// <param name="player">The player that this reward is for.</param>
	/// <param name="rerollOptions">If the options are rerolled with <see cref="T:MegaCrit.Sts2.Core.Models.Relics.Driftwood" />, then these options will
	/// be used to generate a new set of cards.</param>
	/// <param name="synchronizer">PlayerChoiceSynchronizer, for injection in tests.</param>
	public CardReward(IEnumerable<CardModel> cardsToOffer, CardCreationSource source, Player player, CardCreationOptions rerollOptions, PlayerChoiceSynchronizer? synchronizer = null)
		: base(player)
	{
		Options = new CardCreationOptions(Array.Empty<CardModel>(), source, CardRarityOddsType.Uniform).WithFlags(CardCreationFlags.NoCardPoolModifications | CardCreationFlags.NoCardModelModifications | CardCreationFlags.IsCardReward);
		RerollOptions = rerollOptions;
		_cardsWereManuallySet = true;
		_cards = cardsToOffer.Select((CardModel c) => new CardCreationResult(c)).ToList();
		OptionCount = _cards.Count;
		_synchronizer = synchronizer ?? RunManager.Instance.PlayerChoiceSynchronizer;
	}

	/// <summary>
	/// Called when the CardReward rolls options for the first time, and may be called again afterward to generate a new
	/// set of the same type of rewards.
	/// </summary>
	public override void Populate()
	{
		CardCreationOptions cardCreationOptions = (_hasBeenRerolled ? RerollOptions : Options);
		if (_cardsWereManuallySet && !_hasBeenRerolled)
		{
			if (Hook.TryModifyCardRewardOptions(base.Player.RunState, base.Player, _cards, cardCreationOptions, out List<AbstractModel> modifiers))
			{
				TaskHelper.RunSafely(Hook.AfterModifyingCardRewardOptions(base.Player.RunState, modifiers));
			}
		}
		else if (_cards.Count <= 0)
		{
			IEnumerable<CardCreationResult> collection = CardFactory.CreateForReward(base.Player, OptionCount, cardCreationOptions);
			_cards.Clear();
			_cards.AddRange(collection);
			IReadOnlyList<CardRewardAlternative> extraOptions = CardRewardAlternative.Generate(this);
			this.AfterGenerated?.Invoke();
			_currentlyShownScreen?.RefreshOptions(_cards, extraOptions);
		}
	}

	private void OnRelicObtained(RelicModel relic)
	{
		if (_cards == null)
		{
			throw new InvalidOperationException("cards must be set first before you can update them");
		}
		if (relic.TryModifyCardRewardOptions(base.Player, _cards, Options))
		{
			relic.AfterModifyingRewards();
		}
		if (relic.TryModifyCardRewardOptionsLate(base.Player, _cards, Options))
		{
			relic.AfterModifyingRewards();
		}
	}

	protected override async Task<bool> OnSelect()
	{
		Log.Info($"Player {base.Player.NetId} selected card reward");
		bool rewardComplete = false;
		bool endSelection = false;
		List<CardModel> chosenCardIds = new List<CardModel>();
		IReadOnlyList<CardRewardAlternative> cardRewardOption = CardRewardAlternative.Generate(this);
		if (LocalContext.IsMe(base.Player))
		{
			_currentlyShownScreen = NCardRewardSelectionScreen.ShowScreen(_cards, cardRewardOption);
		}
		while (!endSelection)
		{
			uint choiceId = _synchronizer.ReserveChoiceId(base.Player);
			int? num;
			CardModel obtainedCard;
			if (LocalContext.IsMe(base.Player))
			{
				if (_currentlyShownScreen != null)
				{
					num = await _currentlyShownScreen.OptionSelected();
				}
				else
				{
					CardRewardSelection? selection = CardSelectCmd.Selector?.GetSelectedCardReward(_cards, cardRewardOption);
					if (!selection.HasValue)
					{
						throw new InvalidOperationException("Card selector unset during test!");
					}
					if (selection.Value.alternative != null)
					{
						obtainedCard = null;
						num = _cards.Count + cardRewardOption.FirstIndex((CardRewardAlternative r) => r == selection.Value.alternative);
					}
					else
					{
						obtainedCard = selection.Value.card;
						num = ((obtainedCard != null) ? new int?(_cards.FirstIndex((CardCreationResult c) => c.Card == selection.Value.card)) : ((int?)null));
					}
				}
				PlayerChoiceResult result = PlayerChoiceResult.FromIndex(num);
				_synchronizer.SyncLocalChoice(base.Player, choiceId, result);
			}
			else
			{
				num = (await _synchronizer.WaitForRemoteChoice(base.Player, choiceId)).AsIndexOrNull();
			}
			NCardHolder cardHolder;
			CardRewardAlternative cardRewardAlternative;
			if (num.HasValue)
			{
				if (num < _cards.Count)
				{
					obtainedCard = _cards[num.Value].Card;
					rewardComplete = true;
					endSelection = !Hook.ShouldAllowSelectingMoreCardRewards(base.Player.RunState, base.Player, this);
					cardHolder = _currentlyShownScreen?.GetCardHolder(obtainedCard);
					cardRewardAlternative = null;
				}
				else
				{
					if (!(num < _cards.Count + cardRewardOption.Count))
					{
						Log.Error($"Received bad player choice index {num} for a card reward with {_cards.Count} cards and {cardRewardOption.Count} alternatives!");
						continue;
					}
					cardRewardAlternative = cardRewardOption[num.Value - _cards.Count];
					rewardComplete = cardRewardAlternative.AfterSelected == PostAlternateCardRewardAction.EndSelectionAndCompleteReward;
					PostAlternateCardRewardAction afterSelected = cardRewardAlternative.AfterSelected;
					bool flag = (uint)(afterSelected - 1) <= 1u;
					endSelection = flag;
					cardHolder = null;
					obtainedCard = null;
				}
			}
			else
			{
				rewardComplete = false;
				endSelection = true;
				cardHolder = null;
				obtainedCard = null;
				cardRewardAlternative = null;
			}
			if (!(obtainedCard != null || cardRewardAlternative != null || rewardComplete))
			{
				continue;
			}
			if (obtainedCard != null)
			{
				CardPileAddResult cardPileAddResult = await CardPileCmd.Add(obtainedCard, PileType.Deck);
				if (cardPileAddResult.success)
				{
					obtainedCard = cardPileAddResult.cardAdded;
					chosenCardIds.Add(obtainedCard);
					_cards.RemoveAll((CardCreationResult c) => c.Card == obtainedCard);
					if (cardHolder != null)
					{
						NCard cardNode = cardHolder.CardNode;
						NRun.Instance.GlobalUi.ReparentCard(cardNode);
						cardHolder.QueueFreeSafely();
						NRun.Instance.GlobalUi.TopBar.TrailContainer.AddChildSafely(NCardFlyVfx.Create(cardNode, PileType.Deck, isAddingToPile: true, obtainedCard.Owner.Character.TrailPath));
					}
					Log.Info($"Player {base.Player.NetId} obtained {obtainedCard.Id} from card reward");
				}
			}
			else if (cardRewardAlternative != null)
			{
				await cardRewardAlternative.OnSelect();
			}
		}
		base.Player.RelicObtained -= OnRelicObtained;
		foreach (CardModel item in chosenCardIds)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(base.Player.NetId).CardChoices.Add(new CardChoiceHistoryEntry(item, wasPicked: true));
		}
		if (rewardComplete)
		{
			foreach (CardCreationResult card in _cards)
			{
				base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(base.Player.NetId).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
			}
		}
		if (_currentlyShownScreen != null)
		{
			NOverlayStack.Instance?.Remove(_currentlyShownScreen);
			_currentlyShownScreen = null;
		}
		return rewardComplete;
	}

	public override void OnSkipped()
	{
		foreach (CardCreationResult card in _cards)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(base.Player.NetId).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
		}
		base.Player.RelicObtained -= OnRelicObtained;
	}

	public void Reroll()
	{
		CanReroll = false;
		foreach (CardCreationResult card in _cards)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(base.Player.NetId).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
		}
		_hasBeenRerolled = true;
		_cards.Clear();
		Populate();
	}

	public override SerializableReward ToSerializable()
	{
		if (Options.CardPools.Count <= 0)
		{
			string text = ((Options.CustomCardPool == null) ? "NULL" : string.Join(",", Options.CustomCardPool));
			throw new NotImplementedException("Tried to serialize a CardReward without any card pools! This is not currently supported. Custom card pool is: " + text);
		}
		if (Options.CardPoolFilter != null)
		{
			throw new NotImplementedException("Tried to serialize a CardReward with a card pool filter! This is not currently supported.");
		}
		CardCreationFlags cardCreationFlags = Options.Flags & ~CardCreationFlags.IsCardReward;
		if (cardCreationFlags != 0)
		{
			throw new NotImplementedException("Tried to serialize a CardReward with card creation flags! " + $"This is not currently supported. Flags: {cardCreationFlags}");
		}
		return new SerializableReward
		{
			RewardType = RewardType.Card,
			Source = Options.Source,
			RarityOdds = Options.RarityOdds,
			CardPoolIds = Options.CardPools.Select((CardPoolModel p) => p.Id).ToList(),
			OptionCount = OptionCount
		};
	}

	public override void MarkContentAsSeen()
	{
	}
}
