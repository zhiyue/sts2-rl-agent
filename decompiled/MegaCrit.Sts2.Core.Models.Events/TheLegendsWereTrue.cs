using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TheLegendsWereTrue : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(8m, ValueProp.Unblockable | ValueProp.Unpowered));

	public override bool IsAllowed(RunState runState)
	{
		if (runState.CurrentActIndex == 0 && runState.Players.All((Player p) => p.Deck.Cards.Count > 0))
		{
			return runState.Players.All((Player p) => p.Creature.CurrentHp >= 10);
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, NabTheMap, "THE_LEGENDS_WERE_TRUE.pages.INITIAL.options.NAB_THE_MAP", HoverTipFactory.FromCardWithCardHoverTips<SpoilsMap>()),
			new EventOption(this, SlowlyFindAnExit, "THE_LEGENDS_WERE_TRUE.pages.INITIAL.options.SLOWLY_FIND_AN_EXIT").ThatDoesDamage(base.DynamicVars.Damage.BaseValue)
		});
	}

	private async Task NabTheMap()
	{
		CardModel card = base.Owner.RunState.CreateCard<SpoilsMap>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
		await Cmd.CustomScaledWait(0.5f, 1.2f);
		SetEventFinished(L10NLookup("THE_LEGENDS_WERE_TRUE.pages.NAB_THE_MAP.description"));
	}

	private async Task SlowlyFindAnExit()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Damage, null, null);
		IEnumerable<PotionModel> items = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
		PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
		if (potionModel != null)
		{
			await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
			{
				new PotionReward(potionModel.ToMutable(), base.Owner)
			});
		}
		SetEventFinished(L10NLookup("THE_LEGENDS_WERE_TRUE.pages.SLOWLY_FIND_AN_EXIT.description"));
	}
}
