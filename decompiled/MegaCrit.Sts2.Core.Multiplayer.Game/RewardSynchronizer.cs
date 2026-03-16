using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class RewardSynchronizer : IDisposable
{
	private struct BufferedMessage
	{
		public ulong senderId;

		public RewardObtainedMessage? rewardMessage;

		public GoldLostMessage? goldLostMessage;

		public CardRemovedMessage? cardRemovedMessage;

		public PaelsWingSacrificeMessage? paelsWingSacrificeMessage;
	}

	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly INetGameService _gameService;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	private readonly List<BufferedMessage> _bufferedMessages = new List<BufferedMessage>();

	private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

	public RewardSynchronizer(RunLocationTargetedMessageBuffer messageBuffer, INetGameService gameService, IPlayerCollection playerCollection, ulong localPlayerId)
	{
		_gameService = gameService;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		_messageBuffer = messageBuffer;
		_messageBuffer.RegisterMessageHandler<RewardObtainedMessage>(HandleRewardObtainedMessage);
		_messageBuffer.RegisterMessageHandler<GoldLostMessage>(HandleGoldLostMessage);
		_messageBuffer.RegisterMessageHandler<CardRemovedMessage>(HandleCardRemovedMessage);
		messageBuffer.RegisterMessageHandler<PaelsWingSacrificeMessage>(HandlePaelsWingSacrifice);
		CombatManager.Instance.CombatEnded += OnCombatEnded;
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<RewardObtainedMessage>(HandleRewardObtainedMessage);
		_messageBuffer.UnregisterMessageHandler<PaelsWingSacrificeMessage>(HandlePaelsWingSacrifice);
		_messageBuffer.UnregisterMessageHandler<GoldLostMessage>(HandleGoldLostMessage);
		_messageBuffer.UnregisterMessageHandler<CardRemovedMessage>(HandleCardRemovedMessage);
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
	}

	public void SyncLocalObtainedCard(CardModel card)
	{
		SyncLocalCardEvent(card, skipped: false);
	}

	public void SyncLocalSkippedCard(CardModel card)
	{
		SyncLocalCardEvent(card, skipped: true);
	}

	private void SyncLocalCardEvent(CardModel card, bool skipped)
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException($"Tried to sync card event {card} during combat! This is not allowed");
		}
		RewardObtainedMessage message = new RewardObtainedMessage
		{
			rewardType = RewardType.Card,
			cardModel = card,
			wasSkipped = skipped,
			location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	public void SyncLocalObtainedRelic(RelicModel relic)
	{
		SyncLocalRelicEvent(relic, skipped: false);
	}

	public void SyncLocalSkippedRelic(RelicModel relic)
	{
		SyncLocalRelicEvent(relic, skipped: true);
	}

	private void SyncLocalRelicEvent(RelicModel relic, bool skipped)
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException($"Tried to sync relic event {relic} during combat! This is not allowed");
		}
		RewardObtainedMessage message = new RewardObtainedMessage
		{
			rewardType = RewardType.Relic,
			relicModel = relic,
			wasSkipped = skipped,
			location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	public void SyncLocalObtainedPotion(PotionModel potion)
	{
		SyncLocalPotionEvent(potion, skipped: false);
	}

	public void SyncLocalSkippedPotion(PotionModel potion)
	{
		SyncLocalPotionEvent(potion, skipped: true);
	}

	private void SyncLocalPotionEvent(PotionModel potion, bool skipped)
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException($"Tried to sync potion event {potion} during combat! This is not allowed");
		}
		RewardObtainedMessage message = new RewardObtainedMessage
		{
			rewardType = RewardType.Potion,
			potionModel = potion,
			wasSkipped = skipped,
			location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	public void SyncLocalObtainedGold(int goldAmount)
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException($"Tried to sync obtaining {goldAmount} gold during combat! This is not allowed");
		}
		RewardObtainedMessage message = new RewardObtainedMessage
		{
			rewardType = RewardType.Gold,
			goldAmount = goldAmount,
			wasSkipped = false,
			location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	public void SyncLocalGoldLost(int goldLost)
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer && CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException($"Tried to sync losing {goldLost} gold during combat! This is not allowed");
		}
		GoldLostMessage message = new GoldLostMessage
		{
			goldLost = goldLost,
			location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	public async Task<bool> DoLocalCardRemoval()
	{
		CardRemovedMessage message = new CardRemovedMessage
		{
			Location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
		return await DoCardRemoval(LocalPlayer);
	}

	private void HandleRewardObtainedMessage(RewardObtainedMessage message, ulong senderId)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			BufferedMessage item = new BufferedMessage
			{
				senderId = senderId,
				rewardMessage = message
			};
			_bufferedMessages.Add(item);
			return;
		}
		Player player = _playerCollection.GetPlayer(senderId);
		MapPointHistoryEntry historyEntryFor = player.RunState.GetHistoryEntryFor(message.location);
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = null;
		if (historyEntryFor != null)
		{
			playerMapPointHistoryEntry = historyEntryFor.GetEntry(player.NetId);
		}
		switch (message.rewardType)
		{
		case RewardType.Card:
		{
			CardModel cardModel = message.cardModel;
			if (!message.wasSkipped)
			{
				player.RunState.AddCard(cardModel, player);
				TaskHelper.RunSafely(CardPileCmd.Add(cardModel, PileType.Deck));
				Log.Debug($"Player {player.NetId} obtained {cardModel.Id} from card reward");
			}
			else
			{
				Log.Debug($"Player {player.NetId} skipped {cardModel.Id} from card reward");
			}
			playerMapPointHistoryEntry?.CardChoices.Add(new CardChoiceHistoryEntry(message.cardModel, !message.wasSkipped));
			break;
		}
		case RewardType.Gold:
			if (message.wasSkipped)
			{
				throw new NotImplementedException("Cannot handle skip gold reward message!");
			}
			TaskHelper.RunSafely(PlayerCmd.GainGold(message.goldAmount.Value, player));
			Log.Debug($"Player {player.NetId} obtained {message.goldAmount} gold from gold reward");
			break;
		case RewardType.Potion:
			if (!message.wasSkipped)
			{
				TaskHelper.RunSafely(PotionCmd.TryToProcure(message.potionModel.ToMutable(), player));
				Log.Debug($"Player {player.NetId} obtained {message.potionModel?.Id} from potion reward");
			}
			else
			{
				Log.Debug($"Player {player.NetId} skipped {message.potionModel?.Id} from potion reward");
				playerMapPointHistoryEntry?.PotionChoices.Add(new ModelChoiceHistoryEntry(message.potionModel.Id, !message.wasSkipped));
			}
			break;
		case RewardType.Relic:
			if (!message.wasSkipped)
			{
				TaskHelper.RunSafely(RelicCmd.Obtain(message.relicModel, player));
				Log.Debug($"Player {player.NetId} obtained {message.relicModel?.Id} from relic reward");
			}
			else
			{
				Log.Debug($"Player {player.NetId} skipped {message.relicModel?.Id} from relic reward");
				playerMapPointHistoryEntry?.RelicChoices.Add(new ModelChoiceHistoryEntry(message.relicModel.Id, !message.wasSkipped));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("rewardType", message.rewardType, null);
		}
	}

	private void HandleGoldLostMessage(GoldLostMessage message, ulong senderId)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			BufferedMessage item = new BufferedMessage
			{
				senderId = senderId,
				goldLostMessage = message
			};
			_bufferedMessages.Add(item);
			return;
		}
		Player player = _playerCollection.GetPlayer(senderId);
		TaskHelper.RunSafely(PlayerCmd.LoseGold(message.goldLost, player));
		Log.Debug($"Player {player.NetId} lost {message.goldLost} gold");
	}

	private void HandleCardRemovedMessage(CardRemovedMessage message, ulong senderId)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			BufferedMessage item = new BufferedMessage
			{
				senderId = senderId,
				cardRemovedMessage = message
			};
			_bufferedMessages.Add(item);
			return;
		}
		Player player = _playerCollection.GetPlayer(senderId);
		if (player == LocalPlayer)
		{
			throw new InvalidOperationException("CardRemovedMessage should not be sent to the player removing the card!");
		}
		TaskHelper.RunSafely(DoCardRemoval(player));
	}

	public void SyncLocalPaelsWingSacrifice(PaelsWing paelsWing)
	{
		int num = paelsWing.Owner.Relics.IndexOf(paelsWing);
		if (num < 0)
		{
			throw new InvalidOperationException();
		}
		PaelsWingSacrificeMessage message = new PaelsWingSacrificeMessage
		{
			relicIndex = (uint)num,
			Location = _messageBuffer.CurrentLocation
		};
		_gameService.SendMessage(message);
	}

	private void HandlePaelsWingSacrifice(PaelsWingSacrificeMessage message, ulong senderId)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			BufferedMessage item = new BufferedMessage
			{
				senderId = senderId,
				paelsWingSacrificeMessage = message
			};
			_bufferedMessages.Add(item);
			return;
		}
		Player player = _playerCollection.GetPlayer(senderId);
		if (player == LocalPlayer)
		{
			throw new InvalidOperationException("PaelsWingSacrificeMessage should not be sent to the player doing the sacrifice!");
		}
		if (message.relicIndex >= player.Relics.Count)
		{
			Log.Error($"PaelsWingSacrificeMessage has index {message.relicIndex} which is out of bounds! Player {senderId} has {player.Relics.Count} relics");
		}
		else
		{
			RelicModel relicModel = player.Relics[(int)message.relicIndex];
			if (!(relicModel is PaelsWing paelsWing))
			{
				Log.Error($"PaelsWingSacrificeMessage has index {message.relicIndex} which is not Pael's Wing! It is {relicModel}");
			}
			else
			{
				TaskHelper.RunSafely(paelsWing.OnSacrifice());
			}
		}
	}

	private void OnCombatEnded(CombatRoom _)
	{
		foreach (BufferedMessage bufferedMessage in _bufferedMessages)
		{
			if (bufferedMessage.rewardMessage.HasValue)
			{
				HandleRewardObtainedMessage(bufferedMessage.rewardMessage.Value, bufferedMessage.senderId);
			}
			else if (bufferedMessage.goldLostMessage.HasValue)
			{
				HandleGoldLostMessage(bufferedMessage.goldLostMessage.Value, bufferedMessage.senderId);
			}
			else if (bufferedMessage.cardRemovedMessage != null)
			{
				HandleCardRemovedMessage(bufferedMessage.cardRemovedMessage, bufferedMessage.senderId);
			}
			else if (bufferedMessage.paelsWingSacrificeMessage != null)
			{
				HandlePaelsWingSacrifice(bufferedMessage.paelsWingSacrificeMessage, bufferedMessage.senderId);
			}
		}
		_bufferedMessages.Clear();
	}

	private async Task<bool> DoCardRemoval(Player player)
	{
		CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(new LocString("gameplay_ui", "COMBAT_REWARD_CARD_REMOVAL.selectionScreenPrompt"), 1);
		cardSelectorPrefs.Cancelable = true;
		cardSelectorPrefs.RequireManualConfirmation = true;
		CardSelectorPrefs prefs = cardSelectorPrefs;
		CardModel card = (await CardSelectCmd.FromDeckForRemoval(player, prefs)).FirstOrDefault();
		if (card != null)
		{
			await CardPileCmd.RemoveFromDeck(card);
			Log.Debug($"Player {player.NetId} removed {card.Id} from deck");
			return true;
		}
		return false;
	}
}
