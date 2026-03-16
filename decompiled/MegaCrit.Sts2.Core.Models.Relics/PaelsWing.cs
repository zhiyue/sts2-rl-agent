using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsWing : RelicModel
{
	public const string sacrificeAlternativeKey = "SACRIFICE";

	private const string _sacrificesKey = "Sacrifices";

	private bool _isActivating;

	private int _rewardsSacrificed;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return RewardsSacrificed % base.DynamicVars["Sacrifices"].IntValue;
			}
			return base.DynamicVars["Sacrifices"].IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Sacrifices", 2m));

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
	public int RewardsSacrificed
	{
		get
		{
			return _rewardsSacrificed;
		}
		set
		{
			AssertMutable();
			_rewardsSacrificed = value;
			InvokeDisplayAmountChanged();
		}
	}

	public override bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
	{
		if (base.Owner != player)
		{
			return false;
		}
		alternatives.Add(new CardRewardAlternative("SACRIFICE", OnSacrificeSynchronized, PostAlternateCardRewardAction.DismissScreenAndRemoveReward));
		return true;
	}

	private async Task OnSacrificeSynchronized()
	{
		RunManager.Instance.RewardSynchronizer.SyncLocalPaelsWingSacrifice(this);
		await OnSacrifice();
	}

	public async Task OnSacrifice()
	{
		RewardsSacrificed++;
		Flash();
		if (RewardsSacrificed % base.DynamicVars["Sacrifices"].IntValue == 0)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
			await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable(), base.Owner);
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
