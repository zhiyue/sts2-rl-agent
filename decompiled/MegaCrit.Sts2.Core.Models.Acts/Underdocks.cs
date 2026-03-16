using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.Acts;

public sealed class Underdocks : ActModel
{
	public override IEnumerable<EncounterModel> BossDiscoveryOrder => new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[3]
	{
		ModelDb.Encounter<WaterfallGiantBoss>(),
		ModelDb.Encounter<SoulFyshBoss>(),
		ModelDb.Encounter<LagavulinMatriarchBoss>()
	});

	public override IEnumerable<AncientEventModel> AllAncients => new global::_003C_003Ez__ReadOnlySingleElementList<AncientEventModel>(ModelDb.AncientEvent<Neow>());

	public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";

	public override IEnumerable<EventModel> AllEvents => new global::_003C_003Ez__ReadOnlyArray<EventModel>(new EventModel[10]
	{
		ModelDb.Event<AbyssalBaths>(),
		ModelDb.Event<DrowningBeacon>(),
		ModelDb.Event<EndlessConveyor>(),
		ModelDb.Event<PunchOff>(),
		ModelDb.Event<SpiralingWhirlpool>(),
		ModelDb.Event<SunkenStatue>(),
		ModelDb.Event<SunkenTreasury>(),
		ModelDb.Event<DoorsOfLightAndDark>(),
		ModelDb.Event<TrashHeap>(),
		ModelDb.Event<WaterloggedScriptorium>()
	});

	protected override int NumberOfWeakEncounters => 3;

	protected override int BaseNumberOfRooms => 15;

	public override string[] BgMusicOptions => new string[1] { "event:/music/act1_b1_v1" };

	public override string[] MusicBankPaths => new string[1] { "res://banks/desktop/act1_b1.bank" };

	public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";

	public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_1_skel_data.tres";

	public override string ChestSpineSkinNameNormal => "act1";

	public override string ChestSpineSkinNameStroke => "act1_stroke";

	public override Color MapTraveledColor => new Color("180F24");

	public override Color MapUntraveledColor => new Color("534A62");

	public override Color MapBgColor => new Color("9F95A5");

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[20]
		{
			ModelDb.Encounter<CorpseSlugsNormal>(),
			ModelDb.Encounter<CorpseSlugsWeak>(),
			ModelDb.Encounter<CultistsNormal>(),
			ModelDb.Encounter<LivingFogNormal>(),
			ModelDb.Encounter<FossilStalkerNormal>(),
			ModelDb.Encounter<GremlinMercNormal>(),
			ModelDb.Encounter<HauntedShipNormal>(),
			ModelDb.Encounter<LagavulinMatriarchBoss>(),
			ModelDb.Encounter<SkulkingColonyElite>(),
			ModelDb.Encounter<PhantasmalGardenersElite>(),
			ModelDb.Encounter<PunchConstructNormal>(),
			ModelDb.Encounter<SeapunkWeak>(),
			ModelDb.Encounter<SewerClamNormal>(),
			ModelDb.Encounter<SludgeSpinnerWeak>(),
			ModelDb.Encounter<SoulFyshBoss>(),
			ModelDb.Encounter<TerrorEelElite>(),
			ModelDb.Encounter<ToadpolesNormal>(),
			ModelDb.Encounter<ToadpolesWeak>(),
			ModelDb.Encounter<TwoTailedRatsNormal>(),
			ModelDb.Encounter<WaterfallGiantBoss>()
		});
	}

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
	{
		List<AncientEventModel> list = AllAncients.ToList();
		if (!unlockState.IsEpochRevealed<NeowEpoch>())
		{
			list.Remove(ModelDb.AncientEvent<Neow>());
		}
		return list;
	}

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		int num = mapRng.NextGaussianInt(7, 1, 6, 7);
		if (AscensionHelper.HasAscension(AscensionLevel.Gloom))
		{
			num--;
		}
		return new MapPointTypeCounts(mapRng)
		{
			NumOfRests = num
		};
	}
}
