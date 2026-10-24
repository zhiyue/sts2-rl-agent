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

	/// <summary>
	/// Only return the Potions if the associated Epoch is revealed.
	/// NOTE: This needs to be updated if a character has more than 3 potions. See: Silent4Epoch.cs
	/// </summary>
	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		if (!unlockState.IsEpochRevealed<Silent4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		return GenerateAllPotions();
	}
}
