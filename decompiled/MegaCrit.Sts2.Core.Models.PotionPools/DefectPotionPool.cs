using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class DefectPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "defect";

	public override Color LabOutlineColor => StsColors.blue;

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Defect4Epoch.Potions;
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Defect4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
