using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class CardSelectCmd
{
	private sealed class StackedSelectorScope : IDisposable
	{
		private readonly MegaCrit.Sts2.Core.TestSupport.ICardSelector _selector;

		private bool _disposed;

		public StackedSelectorScope(MegaCrit.Sts2.Core.TestSupport.ICardSelector selector)
		{
			_selector = selector;
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				if (_selectorStack.Count > 0 && _selectorStack.Peek() == _selector)
				{
					_selectorStack.Pop();
				}
			}
		}
	}

	private sealed class SelectorScope : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_selectorStack.Clear();
			}
		}
	}

	private static readonly Stack<MegaCrit.Sts2.Core.TestSupport.ICardSelector> _selectorStack = new Stack<MegaCrit.Sts2.Core.TestSupport.ICardSelector>();

	public static MegaCrit.Sts2.Core.TestSupport.ICardSelector? Selector
	{
		get
		{
			if (_selectorStack.Count <= 0)
			{
				return null;
			}
			return _selectorStack.Peek();
		}
	}

	public static IDisposable UseSelector(MegaCrit.Sts2.Core.TestSupport.ICardSelector selector)
	{
		if (_selectorStack.Count > 0)
		{
			throw new InvalidOperationException("A card selector is already active.");
		}
		_selectorStack.Push(selector);
		return new SelectorScope();
	}

	public static IDisposable PushSelector(MegaCrit.Sts2.Core.TestSupport.ICardSelector selector)
	{
		_selectorStack.Push(selector);
		return new StackedSelectorScope(selector);
	}

	private static bool ShouldSelectLocalCard(Player player)
	{
		if (LocalContext.IsMe(player))
		{
			return RunManager.Instance.NetService.Type != NetGameType.Replay;
		}
		return false;
	}

	public static async Task<CardModel?> FromChooseACardScreen(PlayerChoiceContext context, IReadOnlyList<CardModel> cards, Player player, bool canSkip = false)
	{
		if (cards.Count > 3)
		{
			throw new ArgumentException("Only works with less than 3 cards", "cards");
		}
		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
		await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
		CardModel result;
		if (ShouldSelectLocalCard(player))
		{
			NPlayerHand.Instance?.CancelAllCardPlay();
			if (Selector != null)
			{
				result = (await Selector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
			}
			else
			{
				NChooseACardSelectionScreen nChooseACardSelectionScreen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip);
				if (LocalContext.IsMe(player))
				{
					foreach (CardModel card in cards)
					{
						SaveManager.Instance.MarkCardAsSeen(card);
					}
				}
				result = (await nChooseACardSelectionScreen.CardsSelected()).FirstOrDefault();
			}
			int index = cards.IndexOf(result);
			PlayerChoiceResult result2 = PlayerChoiceResult.FromIndex(index);
			RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, result2);
		}
		else
		{
			int num = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndex();
			result = ((num < 0) ? null : cards[num]);
		}
		await context.SignalPlayerChoiceEnded();
		LogChoice(player, new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(result));
		return result;
	}

	public static async Task<IEnumerable<CardModel>> FromSimpleGridForRewards(PlayerChoiceContext context, List<CardCreationResult> cards, Player player, CardSelectorPrefs prefs)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Array.Empty<CardModel>();
		}
		List<CardModel> result;
		if (!prefs.RequireManualConfirmation && cards.Count <= prefs.MinSelect)
		{
			result = cards.Select((CardCreationResult c) => c.Card).ToList();
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
			if (ShouldSelectLocalCard(player))
			{
				if (Selector != null)
				{
					IEnumerable<CardModel> options = cards.Select((CardCreationResult c) => c.Card);
					result = (await Selector.GetSelectedCards(options, prefs.MinSelect, prefs.MaxSelect)).ToList();
				}
				else
				{
					NSimpleCardSelectScreen nSimpleCardSelectScreen = NSimpleCardSelectScreen.Create(cards, prefs);
					NOverlayStack.Instance.Push(nSimpleCardSelectScreen);
					result = (await nSimpleCardSelectScreen.CardsSelected()).ToList();
				}
				List<int> indexes = result.Select((CardModel c) => cards.FindIndex((CardCreationResult r) => r.Card == c)).ToList();
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(indexes));
			}
			else
			{
				result = (from i in (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndexes()
					select cards[i].Card).ToList();
			}
			await context.SignalPlayerChoiceEnded();
		}
		LogChoice(player, result);
		return result;
	}

	public static async Task<IEnumerable<CardModel>> FromSimpleGrid(PlayerChoiceContext context, IReadOnlyList<CardModel> cards, Player player, CardSelectorPrefs prefs)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Array.Empty<CardModel>();
		}
		List<CardModel> result;
		if (!prefs.RequireManualConfirmation && cards.Count <= prefs.MinSelect)
		{
			result = cards.ToList();
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
			if (ShouldSelectLocalCard(player))
			{
				if (Selector != null)
				{
					result = (await Selector.GetSelectedCards(cards, prefs.MinSelect, prefs.MaxSelect)).ToList();
				}
				else
				{
					NPlayerHand.Instance?.CancelAllCardPlay();
					NSimpleCardSelectScreen nSimpleCardSelectScreen = NSimpleCardSelectScreen.Create(cards, prefs);
					NOverlayStack.Instance.Push(nSimpleCardSelectScreen);
					result = (await nSimpleCardSelectScreen.CardsSelected()).ToList();
				}
				List<int> indexes = result.Select(cards.IndexOf<CardModel>).ToList();
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(indexes));
			}
			else
			{
				result = (from i in (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndexes()
					select cards[i]).ToList();
			}
			await context.SignalPlayerChoiceEnded();
		}
		LogChoice(player, result);
		return result;
	}

	public static async Task<IEnumerable<CardModel>> FromDeckForUpgrade(Player player, CardSelectorPrefs prefs)
	{
		List<CardModel> list = PileType.Deck.GetPile(player).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		IEnumerable<CardModel> enumerable;
		if (list.Count <= prefs.MinSelect && !prefs.RequireManualConfirmation)
		{
			enumerable = list;
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			if (ShouldSelectLocalCard(player))
			{
				if (Selector != null)
				{
					enumerable = await Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
				}
				else
				{
					NDeckUpgradeSelectScreen nDeckUpgradeSelectScreen = NDeckUpgradeSelectScreen.ShowScreen(list, prefs, player.RunState);
					enumerable = await nDeckUpgradeSelectScreen.CardsSelected();
				}
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableDeckCards(enumerable));
			}
			else
			{
				enumerable = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsDeckCards();
			}
		}
		LogChoice(player, enumerable);
		return enumerable;
	}

	public static async Task<IEnumerable<CardModel>> FromDeckForTransformation(Player player, CardSelectorPrefs prefs, Func<CardModel, CardTransformation>? cardToTransformation = null)
	{
		List<CardModel> list = PileType.Deck.GetPile(player).Cards.Where((CardModel c) => c.Type != CardType.Quest && c.IsTransformable).ToList();
		IEnumerable<CardModel> enumerable;
		if (list.Count <= prefs.MinSelect && !prefs.RequireManualConfirmation)
		{
			enumerable = list;
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			if (ShouldSelectLocalCard(player))
			{
				if (Selector != null)
				{
					enumerable = await Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
				}
				else
				{
					if (cardToTransformation == null)
					{
						cardToTransformation = (CardModel c) => new CardTransformation(c);
					}
					NDeckTransformSelectScreen nDeckTransformSelectScreen = NDeckTransformSelectScreen.ShowScreen(list, cardToTransformation, prefs);
					enumerable = await nDeckTransformSelectScreen.CardsSelected();
				}
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableDeckCards(enumerable));
			}
			else
			{
				enumerable = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsDeckCards();
			}
		}
		LogChoice(player, enumerable);
		return enumerable;
	}

	public static async Task<IEnumerable<CardModel>> FromDeckForEnchantment(Player player, EnchantmentModel enchantment, int amount, CardSelectorPrefs prefs)
	{
		return await FromDeckForEnchantment(player, enchantment, amount, null, prefs);
	}

	public static async Task<IEnumerable<CardModel>> FromDeckForEnchantment(Player player, EnchantmentModel enchantment, int amount, Func<CardModel?, bool>? additionalFilter, CardSelectorPrefs prefs)
	{
		IReadOnlyList<CardModel> cards = PileType.Deck.GetPile(player).Cards.Where((CardModel c) => enchantment.CanEnchant(c) && (additionalFilter?.Invoke(c) ?? true)).ToList();
		return await FromDeckForEnchantment(cards, enchantment, amount, prefs);
	}

	public static async Task<IEnumerable<CardModel>> FromDeckForEnchantment(IReadOnlyList<CardModel> cards, EnchantmentModel enchantment, int amount, CardSelectorPrefs prefs)
	{
		if (cards.Any((CardModel c) => c.Pile.Type != PileType.Deck || !enchantment.CanEnchant(c)))
		{
			throw new ArgumentException("All cards must be in the player's deck and enchantable.");
		}
		IEnumerable<CardModel> enumerable;
		if (cards.Count <= prefs.MinSelect)
		{
			enumerable = cards;
		}
		else
		{
			Player player = cards[0].Owner;
			if (player.Creature.IsDead)
			{
				return Array.Empty<CardModel>();
			}
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			if (ShouldSelectLocalCard(player))
			{
				Dictionary<CardModel, int> indexMap = PileType.Deck.GetPile(player).Cards.Select((CardModel card, int index) => new { card, index }).ToDictionary(x => x.card, x => x.index);
				List<CardModel> list = cards.OrderBy((CardModel c) => indexMap[c]).ToList();
				if (Selector != null)
				{
					enumerable = await Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
				}
				else
				{
					NDeckEnchantSelectScreen nDeckEnchantSelectScreen = NDeckEnchantSelectScreen.ShowScreen(list, enchantment, amount, prefs);
					enumerable = await nDeckEnchantSelectScreen.CardsSelected();
				}
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableDeckCards(enumerable));
			}
			else
			{
				enumerable = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsDeckCards();
			}
		}
		if (cards.Count > 0)
		{
			LogChoice(cards[0].Owner, enumerable);
		}
		return enumerable;
	}

	public static Task<IEnumerable<CardModel>> FromDeckForRemoval(Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter = null)
	{
		List<CardModel> deck = PileType.Deck.GetPile(player).Cards.ToList();
		return FromDeckGeneric(player, prefs, (CardModel c) => c.IsRemovable && (filter == null || filter(c)), (CardModel c) => (c.Type != CardType.Curse) ? deck.IndexOf(c) : (-999999999));
	}

	public static async Task<IEnumerable<CardModel>> FromDeckGeneric(Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter = null, Func<CardModel, int>? sortingOrder = null)
	{
		List<CardModel> source = PileType.Deck.GetPile(player).Cards.ToList();
		List<CardModel> list = ((filter == null) ? source.ToList() : source.Where(filter).ToList());
		if (player.Creature.IsDead)
		{
			return Array.Empty<CardModel>();
		}
		if (sortingOrder != null)
		{
			list = list.OrderBy(sortingOrder).ToList();
		}
		IEnumerable<CardModel> enumerable;
		if (!prefs.RequireManualConfirmation && list.Count <= prefs.MinSelect)
		{
			enumerable = list;
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			if (ShouldSelectLocalCard(player))
			{
				if (Selector != null)
				{
					enumerable = await Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
				}
				else
				{
					NDeckCardSelectScreen nDeckCardSelectScreen = NDeckCardSelectScreen.Create(list, prefs);
					NOverlayStack.Instance.Push(nDeckCardSelectScreen);
					enumerable = await nDeckCardSelectScreen.CardsSelected();
				}
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableDeckCards(enumerable));
			}
			else
			{
				enumerable = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsDeckCards();
			}
		}
		LogChoice(player, enumerable);
		return enumerable;
	}

	public static async Task<IEnumerable<CardModel>> FromHand(PlayerChoiceContext context, Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel source)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}
		if (ShouldSelectLocalCard(player))
		{
			NPlayerHand.Instance?.CancelAllCardPlay();
		}
		List<CardModel> cards = PileType.Hand.GetPile(player).Cards.Where(filter ?? ((Func<CardModel, bool>)((CardModel _) => true))).ToList();
		IEnumerable<CardModel> result;
		if (cards.Count == 0)
		{
			result = cards;
		}
		else if (!prefs.RequireManualConfirmation && cards.Count <= prefs.MinSelect)
		{
			result = cards;
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.CancelPlayCardActions);
			if (ShouldSelectLocalCard(player))
			{
				result = ((Selector == null) ? (await NCombatRoom.Instance.Ui.Hand.SelectCards(prefs, filter, source)) : (await Selector.GetSelectedCards(cards, prefs.MinSelect, prefs.MaxSelect)));
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableCombatCards(result));
			}
			else
			{
				result = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsCombatCards();
			}
			await context.SignalPlayerChoiceEnded();
		}
		LogChoice(player, result);
		return result;
	}

	public static async Task<IEnumerable<CardModel>> FromHandForDiscard(PlayerChoiceContext context, Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel source)
	{
		prefs.ShouldGlowGold = delegate(CardModel c)
		{
			if (!c.IsSlyThisTurn)
			{
				return false;
			}
			UnplayableReason reason;
			AbstractModel preventer;
			return c.CanPlay(out reason, out preventer) || reason.HasResourceCostReason();
		};
		return await FromHand(context, player, prefs, filter, source);
	}

	public static async Task<CardModel?> FromHandForUpgrade(PlayerChoiceContext context, Player player, AbstractModel source)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return null;
		}
		if (ShouldSelectLocalCard(player))
		{
			NPlayerHand.Instance?.CancelAllCardPlay();
		}
		List<CardModel> cards = PileType.Hand.GetPile(player).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		CardModel result;
		if (cards.Count <= 1)
		{
			result = cards.FirstOrDefault();
		}
		else
		{
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.CancelPlayCardActions);
			if (ShouldSelectLocalCard(player))
			{
				result = ((Selector == null) ? (await NCombatRoom.Instance.Ui.Hand.SelectCards(new CardSelectorPrefs(new LocString("gameplay_ui", "CHOOSE_CARD_UPGRADE_HEADER"), 1), (CardModel c) => c.IsUpgradable, source, NPlayerHand.Mode.UpgradeSelect)).FirstOrDefault() : (await Selector.GetSelectedCards(cards, 1, 1)).FirstOrDefault());
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableCombatCard(result));
			}
			else
			{
				result = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsCombatCards().First();
			}
			await context.SignalPlayerChoiceEnded();
		}
		LogChoice(player, new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(result));
		return result;
	}

	public static async Task<IEnumerable<CardModel>> FromChooseABundleScreen(Player player, IReadOnlyList<IReadOnlyList<CardModel>> bundles)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Array.Empty<CardModel>();
		}
		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
		IReadOnlyList<CardModel> readOnlyList;
		if (ShouldSelectLocalCard(player))
		{
			NChooseABundleSelectionScreen nChooseABundleSelectionScreen = NChooseABundleSelectionScreen.ShowScreen(bundles);
			readOnlyList = (await nChooseABundleSelectionScreen.CardsSelected()).FirstOrDefault() ?? Array.Empty<CardModel>();
			RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndex(bundles.IndexOf<IReadOnlyList<CardModel>>(readOnlyList)));
		}
		else
		{
			int num = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndex();
			IReadOnlyList<CardModel> readOnlyList2;
			if (num >= 0)
			{
				readOnlyList2 = bundles[num];
			}
			else
			{
				IReadOnlyList<CardModel> readOnlyList3 = Array.Empty<CardModel>();
				readOnlyList2 = readOnlyList3;
			}
			readOnlyList = readOnlyList2;
		}
		LogChoice(player, readOnlyList);
		return readOnlyList;
	}

	private static void LogChoice(Player player, IEnumerable<CardModel?> cards)
	{
		string value = string.Join(",", from c in cards.OfType<CardModel>()
			select c.Id.Entry);
		Log.Info($"Player {player.NetId} chose cards [{value}]");
	}
}
