using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class SilentPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "silent";

	public override Color LabOutlineColor => StsColors.green;

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Silent4Epoch.Potions;
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Silent4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
