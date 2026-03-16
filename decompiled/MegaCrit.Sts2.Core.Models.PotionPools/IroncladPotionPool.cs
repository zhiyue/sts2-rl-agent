using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class IroncladPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "ironclad";

	public override Color LabOutlineColor => StsColors.red;

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Ironclad4Epoch.Potions;
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Ironclad4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
