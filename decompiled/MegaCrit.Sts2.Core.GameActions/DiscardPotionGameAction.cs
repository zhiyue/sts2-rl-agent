using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.GameActions;

public class DiscardPotionGameAction : GameAction
{
	private readonly Player _player;

	private readonly uint _potionSlotIndex;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public DiscardPotionGameAction(Player player, uint potionSlotIndex)
	{
		_player = player;
		_potionSlotIndex = potionSlotIndex;
	}

	protected override async Task ExecuteAction()
	{
		if (_potionSlotIndex >= _player.PotionSlots.Count)
		{
			throw new IndexOutOfRangeException($"Tried to discard potion at slot index {_potionSlotIndex}, but player {_player.NetId} only has {_player.PotionSlots.Count} potion slots!");
		}
		PotionModel potionModel = _player.PotionSlots[(int)_potionSlotIndex];
		if (potionModel == null)
		{
			throw new InvalidOperationException($"Tried to discard potion at slot index {_potionSlotIndex}, but player {_player.NetId} has no potion in that slot!");
		}
		Log.Info($"Player {potionModel.Owner.NetId} discarding potion {potionModel.Id.Entry}");
		await PotionCmd.Discard(potionModel);
	}

	public override INetAction ToNetAction()
	{
		return new NetDiscardPotionGameAction
		{
			potionSlotIndex = _potionSlotIndex
		};
	}

	public override string ToString()
	{
		return $"{"NetDiscardPotionGameAction"} for player {_player.NetId} potion slot: {_potionSlotIndex}";
	}
}
