using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// This action is enqueued when the player chooses to discard a potion from their belt. It runs both in combat and
/// outside of combat.
/// </summary>
public class DiscardPotionGameAction : GameAction
{
	private readonly Player _player;

	private readonly uint _potionSlotIndex;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType
	{
		get
		{
			if (!WasEnqueuedInCombat)
			{
				return GameActionType.NonCombat;
			}
			return GameActionType.CombatPlayPhaseOnly;
		}
	}

	/// <summary>
	/// True if the player discarded a potion in combat, false otherwise.
	/// The player may discard a potion at any time. However, there's a specific situation in which order of GameAction
	/// enqueues matters when out-of-combat: if the player discards a potion post-combat while another player is still
	/// executing the player turn. If the action is set to GameActionType.Any, it will be executed during combat, before
	/// the enemy turn.
	/// </summary>
	public bool WasEnqueuedInCombat { get; }

	public DiscardPotionGameAction(Player player, uint potionSlotIndex, bool isCombatInProgress)
	{
		_player = player;
		_potionSlotIndex = potionSlotIndex;
		WasEnqueuedInCombat = isCombatInProgress;
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
			Log.Warn($"{"DiscardPotionGameAction"}: potion at slot index {_potionSlotIndex} is null for player {_player.NetId}, canceling");
			Cancel();
		}
		else
		{
			Log.Info($"Player {potionModel.Owner.NetId} discarding potion {potionModel.Id.Entry}");
			await PotionCmd.Discard(potionModel);
		}
	}

	protected override void CancelAction()
	{
		PotionModel potionAtSlotIndex = _player.GetPotionAtSlotIndex((int)_potionSlotIndex);
		if (TestMode.IsOff && NRun.Instance != null && LocalContext.IsMe(_player) && potionAtSlotIndex != null)
		{
			NRun.Instance.GlobalUi.TopBar.PotionContainer.OnPotionUseOrDiscardCanceled(potionAtSlotIndex);
		}
		potionAtSlotIndex?.AfterUsageCanceled();
	}

	public override INetAction ToNetAction()
	{
		return new NetDiscardPotionGameAction
		{
			potionSlotIndex = _potionSlotIndex,
			wasEnqueuedInCombat = WasEnqueuedInCombat
		};
	}

	public override string ToString()
	{
		return $"{"NetDiscardPotionGameAction"} for player {_player.NetId} potion slot: {_potionSlotIndex} in combat: {WasEnqueuedInCombat}";
	}
}
