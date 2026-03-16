using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class NecrobinderPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Necrobinder4Epoch.Potions;
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Necrobinder4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
