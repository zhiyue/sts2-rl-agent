using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
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

namespace MegaCrit.Sts2.Core.Rewards;

public class CardReward : Reward
{
	private readonly List<CardCreationResult> _cards = new List<CardCreationResult>();

	private bool _cardsWereManuallySet;

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

	public bool CanReroll { get; set; }

	public bool CanSkip { get; init; } = true;

	public override bool IsPopulated => _cards.Count > 0;

	public event Action? AfterGenerated;

	public CardReward(CardCreationOptions options, int cardCount, Player player)
		: base(player)
	{
		OptionCount = cardCount;
		Options = options;
		player.RelicObtained += OnRelicObtained;
	}

	public CardReward(IEnumerable<CardModel> cardsToOffer, CardCreationSource source, Player player)
		: base(player)
	{
		Options = new CardCreationOptions(Array.Empty<CardModel>(), source, CardRarityOddsType.Uniform).WithFlags(CardCreationFlags.NoCardPoolModifications | CardCreationFlags.NoCardModelModifications);
		_cardsWereManuallySet = true;
		_cards = cardsToOffer.Select((CardModel c) => new CardCreationResult(c)).ToList();
		OptionCount = _cards.Count;
	}

	public override Task Populate()
	{
		if (_cardsWereManuallySet)
		{
			if (Hook.TryModifyCardRewardOptions(base.Player.RunState, base.Player, _cards, Options, out List<AbstractModel> modifiers))
			{
				TaskHelper.RunSafely(Hook.AfterModifyingCardRewardOptions(base.Player.RunState, modifiers));
			}
			return Task.CompletedTask;
		}
		if (_cards.Count > 0)
		{
			return Task.CompletedTask;
		}
		IEnumerable<CardCreationResult> collection = CardFactory.CreateForReward(base.Player, OptionCount, Options);
		_cards.Clear();
		_cards.AddRange(collection);
		IReadOnlyList<CardRewardAlternative> extraOptions = CardRewardAlternative.Generate(this);
		this.AfterGenerated?.Invoke();
		_currentlyShownScreen?.RefreshOptions(_cards, extraOptions);
		return Task.CompletedTask;
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
		Log.Info("Card reward selected");
		bool removeReward = false;
		List<CardModel> chosenCardIds = new List<CardModel>();
		IReadOnlyList<CardRewardAlternative> cardRewardOption = CardRewardAlternative.Generate(this);
		_currentlyShownScreen = NCardRewardSelectionScreen.ShowScreen(_cards, cardRewardOption);
		while (true)
		{
			CardModel result = null;
			NCardHolder cardHolder = null;
			if (_currentlyShownScreen != null)
			{
				Tuple<IEnumerable<NCardHolder>, bool> tuple = await _currentlyShownScreen.CardsSelected();
				removeReward = tuple.Item2;
				cardHolder = tuple.Item1.FirstOrDefault();
				if (cardHolder != null)
				{
					result = cardHolder.CardNode.Model;
				}
			}
			else
			{
				result = CardSelectCmd.Selector?.GetSelectedCardReward(_cards, cardRewardOption);
			}
			if (result != null || removeReward)
			{
				if (result != null)
				{
					CardPileAddResult cardPileAddResult = await CardPileCmd.Add(result, PileType.Deck);
					if (cardPileAddResult.success)
					{
						result = cardPileAddResult.cardAdded;
						chosenCardIds.Add(result);
						_cards.RemoveAll((CardCreationResult c) => c.Card == result);
						if (cardHolder != null)
						{
							NCard cardNode = cardHolder.CardNode;
							NRun.Instance.GlobalUi.ReparentCard(cardNode);
							cardHolder.QueueFreeSafely();
							Vector2 targetPosition = PileType.Deck.GetTargetPosition(cardNode);
							NRun.Instance.GlobalUi.TopBar.TrailContainer.AddChildSafely(NCardFlyVfx.Create(cardNode, targetPosition, isAddingToPile: true, result.Owner.Character.TrailPath));
						}
						Log.Info($"Obtained {result.Id} from card reward");
						RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(result);
					}
				}
				base.Player.RelicObtained -= OnRelicObtained;
			}
			if (result == null || !Hook.ShouldAllowSelectingMoreCardRewards(base.Player.RunState, base.Player, this))
			{
				break;
			}
		}
		foreach (CardModel item in chosenCardIds)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).CardChoices.Add(new CardChoiceHistoryEntry(item, wasPicked: true));
		}
		if (removeReward)
		{
			foreach (CardCreationResult card in _cards)
			{
				base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
				RunManager.Instance.RewardSynchronizer.SyncLocalSkippedCard(card.Card);
			}
		}
		if (_currentlyShownScreen != null)
		{
			NOverlayStack.Instance?.Remove(_currentlyShownScreen);
			_currentlyShownScreen = null;
		}
		return removeReward;
	}

	public override void OnSkipped()
	{
		foreach (CardCreationResult card in _cards)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
			RunManager.Instance.RewardSynchronizer.SyncLocalSkippedCard(card.Card);
		}
		base.Player.RelicObtained -= OnRelicObtained;
	}

	public async Task Reroll()
	{
		CanReroll = false;
		foreach (CardCreationResult card in _cards)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).CardChoices.Add(new CardChoiceHistoryEntry(card.Card, wasPicked: false));
			RunManager.Instance.RewardSynchronizer.SyncLocalSkippedCard(card.Card);
		}
		_cards.Clear();
		await Populate();
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
		if (Options.Flags != 0)
		{
			throw new NotImplementedException("Tried to serialize a CardReward with card creation flags! " + $"This is not currently supported. Flags: {Options.Flags}");
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
