using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Commands;

public static class PotionCmd
{
	public static async Task<PotionProcureResult> TryToProcure<T>(Player player) where T : PotionModel
	{
		return await TryToProcure(ModelDb.Potion<T>().ToMutable(), player);
	}

	public static async Task<PotionProcureResult> TryToProcure(PotionModel potion, Player player, int slotIndex = -1)
	{
		potion.AssertMutable();
		if (!Hook.ShouldProcurePotion(player.RunState, player.Creature.CombatState, potion, player))
		{
			return new PotionProcureResult
			{
				potion = potion,
				success = false,
				failureReason = PotionProcureFailureReason.NotAllowed
			};
		}
		PotionProcureResult result = player.AddPotionInternal(potion, slotIndex);
		if (result.success)
		{
			player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).PotionChoices.Add(new ModelChoiceHistoryEntry(potion.Id, wasPicked: true));
			NDebugAudioManager.Instance?.Play("gain_potion.mp3", 1f, PitchVariance.Small);
			NRun.Instance?.GlobalUi.TopBar.PotionContainer.AnimatePotion(potion);
			await Hook.AfterPotionProcured(player.RunState, player.Creature.CombatState, potion);
			if (LocalContext.IsMe(player))
			{
				SaveManager.Instance.MarkPotionAsSeen(potion);
			}
		}
		return result;
	}

	public static async Task Discard(PotionModel potion)
	{
		potion.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(potion.Owner.NetId).PotionDiscarded.Add(potion.Id);
		potion.Discard();
		await Hook.AfterPotionDiscarded(potion.Owner.RunState, potion.Owner.Creature.CombatState, potion);
	}
}
