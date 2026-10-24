using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.Acts;

public sealed class DeprecatedAct : ActModel
{
	public override string ChestOpenSfx => "";

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => Array.Empty<EncounterModel>();

	public override IEnumerable<AncientEventModel> AllAncients => Array.Empty<AncientEventModel>();

	public override IEnumerable<EventModel> AllEvents => Array.Empty<EventModel>();

	protected override int NumberOfWeakEncounters => 0;

	protected override int BaseNumberOfRooms => 0;

	public override string[] BgMusicOptions => Array.Empty<string>();

	public override string[] MusicBankPaths => Array.Empty<string>();

	public override string AmbientSfx => "";

	public override string ChestSpineResourcePath => "";

	public override string ChestSpineSkinNameNormal => "";

	public override string ChestSpineSkinNameStroke => "";

	public override int Index => -1;

	public override bool IsDefault => false;

	public override Color MapTraveledColor => new Color("27221C");

	public override Color MapUntraveledColor => new Color("6E7750");

	public override Color MapBgColor => new Color("9B9562");

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return Array.Empty<EncounterModel>();
	}

	public override bool IsUnlocked(UnlockState unlockState)
	{
		return true;
	}

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
	{
		return Array.Empty<AncientEventModel>();
	}

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		return new MapPointTypeCounts(0, 0);
	}
}
