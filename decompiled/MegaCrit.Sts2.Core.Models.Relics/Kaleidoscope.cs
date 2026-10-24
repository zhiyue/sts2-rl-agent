using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Kaleidoscope : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override bool IsAllowedAtNeow(Player player)
	{
		if (base.IsAllowedAtNeow(player))
		{
			return player.UnlockState.CharacterCardPools.Count() == ModelDb.AllCharacters.Count();
		}
		return false;
	}

	public override async Task AfterObtained()
	{
		List<Reward> list = new List<Reward>();
		CardCreationOptions rerollOptions = CardCreationOptions.ForNonCombatWithDefaultOdds(Array.Empty<CardModel>());
		for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
		{
			List<CardModel> list2 = new List<CardModel>();
			IEnumerable<CardPoolModel> enumerable = base.Owner.UnlockState.CharacterCardPools.Where((CardPoolModel p) => p != base.Owner.Character.CardPool).ToList().StableShuffle(base.Owner.RunState.Rng.Niche)
				.Take(3);
			foreach (CardPoolModel item in enumerable)
			{
				CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(item), CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(CardCreationFlags.NoCardPoolModifications);
				list2.Add(CardFactory.CreateForReward(base.Owner, 1, options).First().Card);
			}
			list.Add(new CardReward(list2, CardCreationSource.Other, base.Owner, rerollOptions));
		}
		await RewardsCmd.OfferCustom(base.Owner, list);
	}
}
