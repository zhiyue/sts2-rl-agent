using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class ColorfulPhilosophers : EventModel
{
	private static IEnumerable<CardPoolModel> CardPoolColorOrder => new global::_003C_003Ez__ReadOnlyArray<CardPoolModel>(new CardPoolModel[5]
	{
		ModelDb.CardPool<NecrobinderCardPool>(),
		ModelDb.CardPool<IroncladCardPool>(),
		ModelDb.CardPool<RegentCardPool>(),
		ModelDb.CardPool<SilentCardPool>(),
		ModelDb.CardPool<DefectCardPool>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = new List<EventOption>();
		CharacterModel character = base.Owner.Character;
		List<CardPoolModel> list2 = base.Owner.UnlockState.CharacterCardPools.ToList();
		foreach (CardPoolModel cardPool in CardPoolColorOrder)
		{
			if (character.CardPool != cardPool && list2.Contains(cardPool))
			{
				list.Add(new EventOption(this, () => OfferRewards(cardPool), "COLORFUL_PHILOSOPHERS.pages.INITIAL.options." + cardPool.EnergyColorName.ToUpperInvariant()));
			}
		}
		int num = Mathf.Min(3, list.Count);
		while (list.Count > num)
		{
			list.RemoveAt(base.Rng.NextInt(list.Count));
		}
		return list;
	}

	private async Task OfferRewards(CardPoolModel pool)
	{
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(pool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == CardRarity.Common).WithFlags(CardCreationFlags.NoRarityModification);
		CardCreationOptions options2 = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(pool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == CardRarity.Uncommon).WithFlags(CardCreationFlags.NoRarityModification);
		CardCreationOptions options3 = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(pool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == CardRarity.Rare).WithFlags(CardCreationFlags.NoRarityModification);
		await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(3)
		{
			new CardReward(options, base.DynamicVars.Cards.IntValue, base.Owner),
			new CardReward(options2, base.DynamicVars.Cards.IntValue, base.Owner),
			new CardReward(options3, base.DynamicVars.Cards.IntValue, base.Owner)
		});
		SetEventFinished(L10NLookup("COLORFUL_PHILOSOPHERS.pages.DONE.description"));
	}
}
