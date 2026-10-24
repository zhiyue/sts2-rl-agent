using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LastingCandy : RelicModel
{
	private bool _isActivating;

	private int _combatsSeen;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return CombatsSeen % 2;
			}
			return 2;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty]
	public int CombatsSeen
	{
		get
		{
			return _combatsSeen;
		}
		set
		{
			AssertMutable();
			_combatsSeen = value;
		}
	}

	private bool IsInTriggeringCombat
	{
		get
		{
			if (CombatsSeen > 0)
			{
				return CombatsSeen % 2 == 0;
			}
			return false;
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.Players.Any(delegate(Player p)
		{
			if (p != null && p.Character is Ironclad)
			{
				UnlockState unlockState = p.UnlockState;
				if (unlockState != null)
				{
					return unlockState.NumberOfRuns == 0;
				}
			}
			return false;
		}))
		{
			return false;
		}
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
	{
		if (base.Owner != player)
		{
			return false;
		}
		if (creationOptions.Source != CardCreationSource.Encounter)
		{
			return false;
		}
		if (!IsInTriggeringCombat)
		{
			return false;
		}
		IEnumerable<CardModel> enumerable = from c in creationOptions.GetPossibleCards(player)
			where c.Type == CardType.Power && options.TrueForAll((CardCreationResult o) => o.originalCard.Id != c.Id)
			select c;
		if (!enumerable.Any())
		{
			enumerable = from c in creationOptions.GetPossibleCards(player)
				where c.Type == CardType.Power
				select c;
		}
		if (!enumerable.Any())
		{
			return false;
		}
		CardCreationOptions options2 = new CardCreationOptions(enumerable, CardCreationSource.Other, creationOptions.RarityOdds).WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
		CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options2).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCreationResult cardCreationResult = new CardCreationResult(cardModel);
			cardCreationResult.ModifyCard(cardModel, this);
			options.Add(cardCreationResult);
		}
		return cardModel != null;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		CombatsSeen++;
		if (IsInTriggeringCombat)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
		}
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
