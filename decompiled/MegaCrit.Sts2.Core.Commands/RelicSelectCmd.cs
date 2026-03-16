using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Commands;

public static class RelicSelectCmd
{
	private static bool ShouldSelectLocalRelic(Player player)
	{
		if (LocalContext.IsMe(player))
		{
			return RunManager.Instance.NetService.Type != NetGameType.Replay;
		}
		return false;
	}

	public static async Task<RelicModel?> FromChooseARelicScreen(Player player, IReadOnlyList<RelicModel> relics)
	{
		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
		RelicModel relicModel;
		if (ShouldSelectLocalRelic(player))
		{
			NChooseARelicSelection nChooseARelicSelection = NChooseARelicSelection.ShowScreen(relics);
			if (LocalContext.IsMe(player))
			{
				foreach (RelicModel relic in relics)
				{
					SaveManager.Instance.MarkRelicAsSeen(relic);
				}
			}
			relicModel = (await nChooseARelicSelection.RelicsSelected()).FirstOrDefault();
			int index = relics.IndexOf(relicModel);
			PlayerChoiceResult result = PlayerChoiceResult.FromIndex(index);
			RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, result);
		}
		else
		{
			int num = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndex();
			relicModel = ((num < 0) ? null : relics[num]);
		}
		return relicModel;
	}
}
