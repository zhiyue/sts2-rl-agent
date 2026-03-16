using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public sealed class MerchantCardEntry : MerchantEntry
{
	private readonly MerchantInventory? _inventory;

	private readonly IEnumerable<CardModel> _cardPool;

	private readonly CardType? _cardType;

	private readonly CardRarity? _cardRarity;

	public CardCreationResult? CreationResult { get; private set; }

	public bool IsOnSale { get; private set; }

	public override bool IsStocked => CreationResult != null;

	private static int GetCost(CardModel card)
	{
		int num = card.Rarity switch
		{
			CardRarity.Rare => 150, 
			CardRarity.Uncommon => 75, 
			_ => 50, 
		};
		if (card.Pool is ColorlessCardPool)
		{
			num = Mathf.RoundToInt((float)num * 1.15f);
		}
		return num;
	}

	public MerchantCardEntry(Player player, MerchantInventory? inventory, IEnumerable<CardModel> cardPool, CardType cardType)
		: base(player)
	{
		_inventory = inventory;
		_cardPool = cardPool;
		_cardType = cardType;
	}

	public MerchantCardEntry(Player player, MerchantInventory? inventory, IEnumerable<CardModel> cardPool, CardRarity cardRarity)
		: base(player)
	{
		_inventory = inventory;
		_cardPool = cardPool;
		_cardRarity = cardRarity;
	}

	public void Populate()
	{
		HashSet<CardModel> second = _inventory?.CardEntries.Select((MerchantCardEntry e) => e.CreationResult?.Card.CanonicalInstance).OfType<CardModel>().ToHashSet() ?? new HashSet<CardModel>();
		if (_cardType.HasValue)
		{
			CreationResult = CardFactory.CreateForMerchant(_player, _cardPool.Except(second), _cardType.Value);
		}
		else
		{
			if (!_cardRarity.HasValue)
			{
				throw new InvalidOperationException();
			}
			CreationResult = CardFactory.CreateForMerchant(_player, _cardPool.Except(second), _cardRarity.Value);
		}
		IRunState runState = _player.RunState;
		Player player = _player;
		int num = 1;
		List<CardCreationResult> list = new List<CardCreationResult>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CardCreationResult> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = CreationResult;
		Hook.ModifyMerchantCardCreationResults(runState, player, list);
		CalcCost();
	}

	protected override void UpdateEntry()
	{
		if (CreationResult != null)
		{
			IRunState runState = _player.RunState;
			Player player = _player;
			int num = 1;
			List<CardCreationResult> list = new List<CardCreationResult>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardCreationResult> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = CreationResult;
			Hook.ModifyMerchantCardCreationResults(runState, player, list);
		}
	}

	public void SetOnSale()
	{
		IsOnSale = true;
		CalcCost();
	}

	public override void CalcCost()
	{
		if (CreationResult == null)
		{
			throw new InvalidOperationException("There is no item to purchase.");
		}
		_cost = Mathf.RoundToInt((float)GetCost(CreationResult.Card) * _player.PlayerRng.Shops.NextFloat(0.95f, 1.05f));
		if (IsOnSale)
		{
			_cost /= 2;
		}
	}

	protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
	{
		if (!(await CardPileCmd.Add(CreationResult.Card, PileType.Deck)).success)
		{
			InvokePurchaseFailed(PurchaseStatus.FailureSpace);
			return (false, 0);
		}
		if (!ignoreCost)
		{
			await PlayerCmd.LoseGold(base.Cost, _player, GoldLossType.Spent);
		}
		RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost(base.Cost);
		RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(CreationResult.Card);
		if (CreationResult.Card.Pool is ColorlessCardPool)
		{
			_player.RunState.CurrentMapPointHistoryEntry?.GetEntry(_player.NetId).BoughtColorless.Add(CreationResult.Card.Id);
		}
		return (true, (!ignoreCost) ? base.Cost : 0);
	}

	protected override void ClearAfterPurchase()
	{
		CreationResult = null;
	}

	protected override void RestockAfterPurchase(MerchantInventory? inventory)
	{
		IsOnSale = false;
		Populate();
	}
}
