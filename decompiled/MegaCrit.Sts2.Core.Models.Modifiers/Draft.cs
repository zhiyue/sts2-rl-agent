using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Draft : ModifierModel
{
	public override bool ClearsPlayerDeck => true;

	public override Func<Task> GenerateNeowOption(EventModel eventModel)
	{
		return () => OfferRewards(eventModel.Owner);
	}

	private static async Task OfferRewards(Player player)
	{
		CardCreationOptions creationOptions = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(player.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.RegularEncounter).WithFlags(CardCreationFlags.NoUpgradeRoll);
		for (int i = 0; i < 10; i++)
		{
			CardReward reward = new CardReward(creationOptions, 3, player)
			{
				CanSkip = false
			};
			await reward.Populate();
			if (LocalContext.IsMe(player))
			{
				await reward.OnSelectWrapper();
			}
		}
		foreach (Player player2 in player.RunState.Players)
		{
			player2.RelicGrabBag.Remove<PandorasBox>();
		}
		player.RunState.SharedRelicGrabBag.Remove<PandorasBox>();
	}
}
