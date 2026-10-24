using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SilkenTress : RelicModel
{
	private bool _isUsed;

	public override bool IsUsedUp => IsUsed;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Glam>();

	[SavedProperty]
	private bool IsUsed
	{
		get
		{
			return _isUsed;
		}
		set
		{
			AssertMutable();
			_isUsed = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override async Task AfterObtained()
	{
		await PlayerCmd.LoseGold(base.Owner.Gold, base.Owner);
	}

	public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (!options.Flags.HasFlag(CardCreationFlags.IsCardReward))
		{
			return false;
		}
		if (IsUsed)
		{
			return false;
		}
		Glam glam = ModelDb.Enchantment<Glam>();
		foreach (CardCreationResult cardReward in cardRewards)
		{
			CardModel card = cardReward.Card;
			if (glam.CanEnchant(card))
			{
				CardModel card2 = base.Owner.RunState.CloneCard(card);
				CardCmd.Enchant<Glam>(card2, 1m);
				cardReward.ModifyCard(card2, this);
			}
		}
		return true;
	}

	public override Task AfterModifyingCardRewardOptions()
	{
		if (IsUsed)
		{
			return Task.CompletedTask;
		}
		IsUsed = true;
		return Task.CompletedTask;
	}
}
