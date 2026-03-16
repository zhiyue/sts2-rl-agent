using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SlipperyBridge : EventModel
{
	private const string _randomCardKey = "RandomCard";

	private const string _hpLossKey = "HpLoss";

	private const string _overcomeLocKey = "SLIPPERY_BRIDGE.pages.INITIAL.options.OVERCOME";

	private const int _initialHpLoss = 3;

	private int _numberOfHoldOns;

	private CardModel? _randomCardToLose;

	private int NumberOfHoldOns
	{
		get
		{
			return _numberOfHoldOns;
		}
		set
		{
			AssertMutable();
			_numberOfHoldOns = value;
		}
	}

	private CardModel? RandomCardToLose
	{
		get
		{
			return _randomCardToLose;
		}
		set
		{
			AssertMutable();
			_randomCardToLose = value;
		}
	}

	private int CurrentHpLoss => 3 + NumberOfHoldOns;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("RandomCard"),
		new DynamicVar("HpLoss", CurrentHpLoss)
	});

	public override bool IsAllowed(RunState runState)
	{
		if (runState.TotalFloor > 6)
		{
			return runState.Players.All((Player p) => p.Deck.Cards.Any((CardModel c) => c.IsRemovable));
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		GetNewRandomCard();
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Overcome, "SLIPPERY_BRIDGE.pages.INITIAL.options.OVERCOME", HoverTipFactory.FromCard(RandomCardToLose)),
			new EventOption(this, HoldOn, "SLIPPERY_BRIDGE.pages.INITIAL.options.HOLD_ON_0").ThatDoesDamage(CurrentHpLoss)
		});
	}

	public override void OnRoomEnter()
	{
		NEventRoom.Instance?.VfxContainer?.AddChildSafely(NRainVfx.Create());
	}

	private void GetNewRandomCard()
	{
		List<CardModel> list = ((RandomCardToLose != null) ? base.Owner.Deck.Cards.Where((CardModel c) => c.GetType() != RandomCardToLose.GetType()).ToList() : base.Owner.Deck.Cards.Where((CardModel c) => c.Rarity != CardRarity.Basic).ToList());
		list.RemoveAll((CardModel c) => !c.IsRemovable);
		if (list.Count == 0)
		{
			list = base.Owner.Deck.Cards.Where((CardModel c) => c.IsRemovable).ToList();
		}
		RandomCardToLose = base.Rng.NextItem(list);
		StringVar stringVar = (StringVar)base.DynamicVars["RandomCard"];
		stringVar.StringValue = RandomCardToLose.Title;
	}

	private async Task Overcome()
	{
		await CardPileCmd.RemoveFromDeck(RandomCardToLose);
		SetEventFinished(L10NLookup("SLIPPERY_BRIDGE.pages.OVERCOME.description"));
	}

	private async Task HoldOn()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, CurrentHpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		NumberOfHoldOns++;
		base.DynamicVars["HpLoss"].BaseValue = CurrentHpLoss;
		GetNewRandomCard();
		string holdOnSuffix = GetHoldOnSuffix(NumberOfHoldOns - 1);
		string holdOnSuffix2 = GetHoldOnSuffix(NumberOfHoldOns);
		string textKey = "SLIPPERY_BRIDGE.pages.HOLD_ON_" + holdOnSuffix + ".options.HOLD_ON_" + holdOnSuffix2;
		SetEventState(L10NLookup("SLIPPERY_BRIDGE.pages.HOLD_ON_" + holdOnSuffix + ".description"), new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Overcome, "SLIPPERY_BRIDGE.pages.INITIAL.options.OVERCOME", HoverTipFactory.FromCard(RandomCardToLose)),
			new EventOption(this, HoldOn, textKey).ThatDoesDamage(CurrentHpLoss)
		}));
	}

	private string GetHoldOnSuffix(int holdOnNumber)
	{
		if (holdOnNumber >= 7)
		{
			return "LOOP";
		}
		return holdOnNumber.ToString();
	}
}
