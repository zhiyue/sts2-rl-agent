using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class RegentPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "regent";

	public override Color LabOutlineColor => StsColors.orange;

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Regent4Epoch.Potions;
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Regent4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
