using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rooms;

public class MerchantRoom : AbstractRoom
{
	private IRunState? _runState;

	private static MerchantDialogueSet? _dialogue;

	public override RoomType RoomType => RoomType.Shop;

	public List<MerchantInventory> Inventories { get; private set; } = new List<MerchantInventory>();

	public override ModelId? ModelId => null;

	public static MerchantDialogueSet Dialogue
	{
		get
		{
			if (_dialogue != null)
			{
				return _dialogue;
			}
			LocTable table = LocManager.Instance.GetTable("merchant_room");
			IReadOnlyList<LocString> locStringsWithPrefix = table.GetLocStringsWithPrefix("MERCHANT.talk.");
			_dialogue = MerchantDialogueSet.CreateFromLocStrings(locStringsWithPrefix);
			return _dialogue;
		}
	}

	public MerchantInventory GetLocalInventory()
	{
		int playerSlotIndex = _runState.GetPlayerSlotIndex(LocalContext.GetMe(_runState));
		return Inventories[playerSlotIndex];
	}

	public override async Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("MerchantRoom does not support room stack reconstruction.");
		}
		_runState = runState;
		foreach (Player item2 in runState?.Players ?? Array.Empty<Player>())
		{
			MerchantInventory item = MerchantInventory.CreateForNormalMerchant(item2);
			Inventories.Add(item);
		}
		await PreloadManager.LoadRoomMerchantAssets();
		NRun.Instance?.SetCurrentRoom(NMerchantRoom.Create(this, runState?.Players ?? Array.Empty<Player>()));
		if (runState != null)
		{
			await Hook.AfterRoomEntered(runState, this);
		}
	}

	public override Task Exit(IRunState? runState)
	{
		if (TestMode.IsOn)
		{
			return Task.CompletedTask;
		}
		for (int i = 0; i < Inventories.Count; i++)
		{
			MerchantInventory merchantInventory = Inventories[i];
			Player player = _runState.Players[i];
			PlayerMapPointHistoryEntry playerMapPointHistoryEntry = runState?.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
			if (playerMapPointHistoryEntry == null)
			{
				continue;
			}
			foreach (MerchantCardEntry characterCardEntry in merchantInventory.CharacterCardEntries)
			{
				if (characterCardEntry.IsStocked)
				{
					playerMapPointHistoryEntry.CardChoices.Add(new CardChoiceHistoryEntry(characterCardEntry.CreationResult.Card, wasPicked: false));
				}
			}
			foreach (MerchantCardEntry colorlessCardEntry in merchantInventory.ColorlessCardEntries)
			{
				if (colorlessCardEntry.IsStocked)
				{
					playerMapPointHistoryEntry.CardChoices.Add(new CardChoiceHistoryEntry(colorlessCardEntry.CreationResult.Card, wasPicked: false));
				}
			}
			foreach (MerchantRelicEntry relicEntry in merchantInventory.RelicEntries)
			{
				if (relicEntry.IsStocked)
				{
					playerMapPointHistoryEntry.RelicChoices.Add(new ModelChoiceHistoryEntry(relicEntry.Model.Id, wasPicked: false));
				}
			}
			foreach (MerchantPotionEntry potionEntry in merchantInventory.PotionEntries)
			{
				if (potionEntry.IsStocked)
				{
					playerMapPointHistoryEntry.PotionChoices.Add(new ModelChoiceHistoryEntry(potionEntry.Model.Id, wasPicked: false));
				}
			}
		}
		return Task.CompletedTask;
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		throw new NotImplementedException();
	}
}
