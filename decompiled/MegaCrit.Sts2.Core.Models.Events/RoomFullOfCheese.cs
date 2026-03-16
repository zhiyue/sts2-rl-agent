using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class RoomFullOfCheese : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(14m, ValueProp.Unblockable | ValueProp.Unpowered));

	public override bool IsAllowed(RunState runState)
	{
		return runState.CurrentActIndex < 2;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Gorge, "ROOM_FULL_OF_CHEESE.pages.INITIAL.options.GORGE"),
			new EventOption(this, Search, "ROOM_FULL_OF_CHEESE.pages.INITIAL.options.SEARCH", HoverTipFactory.FromRelic<ChosenCheese>()).ThatDoesDamage(base.DynamicVars.Damage.BaseValue)
		});
	}

	private async Task Gorge()
	{
		Player owner = base.Owner;
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(owner.Character.CardPool), (CardModel c) => c.Rarity == CardRarity.Common).WithFlags(CardCreationFlags.NoRarityModification);
		List<CardCreationResult> cards = CardFactory.CreateForReward(owner, 8, options).ToList();
		foreach (CardModel item in await CardSelectCmd.FromSimpleGridForRewards(prefs: new CardSelectorPrefs(L10NLookup("ROOM_FULL_OF_CHEESE.pages.GORGE.selectionScreenPrompt"), 2), context: new BlockingPlayerChoiceContext(), cards: cards, player: owner))
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
		}
		SetEventFinished(L10NLookup("ROOM_FULL_OF_CHEESE.pages.GORGE.description"));
	}

	private async Task Search()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Damage, null, null);
		await RelicCmd.Obtain<ChosenCheese>(base.Owner);
		SetEventFinished(L10NLookup("ROOM_FULL_OF_CHEESE.pages.SEARCH.description"));
	}
}
