using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Commands;

public static class RelicCmd
{
	public static async Task<T> Obtain<T>(Player player) where T : RelicModel
	{
		return (T)(await Obtain(ModelDb.Relic<T>().ToMutable(), player));
	}

	public static async Task<RelicModel> Obtain(RelicModel relic, Player player, int index = -1)
	{
		relic.AssertMutable();
		IRunState runState = player.RunState;
		runState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).RelicChoices.Add(new ModelChoiceHistoryEntry(relic.Id, wasPicked: true));
		player.AddRelicInternal(relic, index);
		if (!relic.IsStackable)
		{
			player.RelicGrabBag.Remove(relic);
			runState.SharedRelicGrabBag.Remove(relic);
		}
		if (LocalContext.IsMe(player))
		{
			NRun.Instance?.GlobalUi.RelicInventory.AnimateRelic(relic);
			NDebugAudioManager.Instance?.Play("relic_get.mp3");
			SaveManager.Instance.MarkRelicAsSeen(relic);
		}
		relic.FloorAddedToDeck = runState.TotalFloor;
		await relic.AfterObtained();
		return relic;
	}

	public static async Task Remove(RelicModel relic)
	{
		relic.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(relic.Owner.NetId).RelicsRemoved.Add(relic.Id);
		relic.Owner.RemoveRelicInternal(relic);
		await relic.AfterRemoved();
	}

	public static async Task<RelicModel> Replace(RelicModel original, RelicModel replace)
	{
		original.AssertMutable();
		replace.AssertMutable();
		Player player = original.Owner;
		int indexOfOriginal = player.Relics.IndexOf(original);
		await Remove(original);
		return await Obtain(replace, player, indexOfOriginal);
	}

	public static async Task Melt(RelicModel relic)
	{
		relic.Owner.MeltRelicInternal(relic);
		await relic.AfterRemoved();
	}
}
