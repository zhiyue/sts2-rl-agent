using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DampenPower : PowerModel
{
	private class Data
	{
		public readonly HashSet<Creature> casters = new HashSet<Creature>();

		public readonly Dictionary<CardModel, int> downgradedCardsToOldUpgradeLevels = new Dictionary<CardModel, int>();
	}

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.None;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		IEnumerable<CardModel> enumerable = base.Owner.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.IsUpgraded);
		foreach (CardModel item in enumerable)
		{
			GetInternalData<Data>().downgradedCardsToOldUpgradeLevels.Add(item, item.CurrentUpgradeLevel);
			CardCmd.Downgrade(item);
			if (base.Owner.HasPower<HexPower>())
			{
				CardCmd.ApplyKeyword(item, CardKeyword.Ethereal);
			}
		}
		Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented)
		{
			return;
		}
		Data internalData = GetInternalData<Data>();
		if (internalData.casters.Contains(creature))
		{
			internalData.casters.Remove(creature);
			if (internalData.casters.Count == 0)
			{
				await PowerCmd.Remove(this);
			}
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		foreach (KeyValuePair<CardModel, int> downgradedCardsToOldUpgradeLevel in GetInternalData<Data>().downgradedCardsToOldUpgradeLevels)
		{
			downgradedCardsToOldUpgradeLevel.Deconstruct(out var key, out var value);
			CardModel card = key;
			int num = value;
			for (int i = 0; i < num; i++)
			{
				CardCmd.Upgrade(card);
			}
		}
		return Task.CompletedTask;
	}

	public void AddCaster(Creature creature)
	{
		GetInternalData<Data>().casters.Add(creature);
	}
}
