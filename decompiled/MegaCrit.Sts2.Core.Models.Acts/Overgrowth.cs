using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.Acts;

public sealed class Overgrowth : ActModel
{
	public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[3]
	{
		ModelDb.Encounter<VantomBoss>(),
		ModelDb.Encounter<CeremonialBeastBoss>(),
		ModelDb.Encounter<TheKinBoss>()
	});

	public override IEnumerable<AncientEventModel> AllAncients => new global::_003C_003Ez__ReadOnlySingleElementList<AncientEventModel>(ModelDb.AncientEvent<Neow>());

	public override IEnumerable<EventModel> AllEvents => new global::_003C_003Ez__ReadOnlyArray<EventModel>(new EventModel[13]
	{
		ModelDb.Event<AromaOfChaos>(),
		ModelDb.Event<ByrdonisNest>(),
		ModelDb.Event<DenseVegetation>(),
		ModelDb.Event<JungleMazeAdventure>(),
		ModelDb.Event<LuminousChoir>(),
		ModelDb.Event<MorphicGrove>(),
		ModelDb.Event<SapphireSeed>(),
		ModelDb.Event<SunkenStatue>(),
		ModelDb.Event<TabletOfTruth>(),
		ModelDb.Event<UnrestSite>(),
		ModelDb.Event<Wellspring>(),
		ModelDb.Event<WhisperingHollow>(),
		ModelDb.Event<WoodCarvings>()
	});

	protected override int NumberOfWeakEncounters => 3;

	protected override int BaseNumberOfRooms => 15;

	public override string[] BgMusicOptions => new string[2] { "event:/music/act1_a1_v1", "event:/music/act1_a2_v2" };

	public override string[] MusicBankPaths => new string[2] { "res://banks/desktop/act1_a1.bank", "res://banks/desktop/act1_a2.bank" };

	public override string AmbientSfx => "event:/sfx/ambience/act1_ambience";

	public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_1_skel_data.tres";

	public override string ChestSpineSkinNameNormal => "act1";

	public override string ChestSpineSkinNameStroke => "act1_stroke";

	public override Color MapTraveledColor => new Color("28231D");

	public override Color MapUntraveledColor => new Color("877256");

	public override Color MapBgColor => new Color("A78A67");

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[22]
		{
			ModelDb.Encounter<BygoneEffigyElite>(),
			ModelDb.Encounter<ByrdonisElite>(),
			ModelDb.Encounter<CeremonialBeastBoss>(),
			ModelDb.Encounter<CubexConstructNormal>(),
			ModelDb.Encounter<FlyconidNormal>(),
			ModelDb.Encounter<FogmogNormal>(),
			ModelDb.Encounter<FuzzyWurmCrawlerWeak>(),
			ModelDb.Encounter<InkletsNormal>(),
			ModelDb.Encounter<MawlerNormal>(),
			ModelDb.Encounter<NibbitsNormal>(),
			ModelDb.Encounter<NibbitsWeak>(),
			ModelDb.Encounter<OvergrowthCrawlers>(),
			ModelDb.Encounter<PhrogParasiteElite>(),
			ModelDb.Encounter<RubyRaidersNormal>(),
			ModelDb.Encounter<ShrinkerBeetleWeak>(),
			ModelDb.Encounter<SlimesNormal>(),
			ModelDb.Encounter<SlimesWeak>(),
			ModelDb.Encounter<SlitheringStranglerNormal>(),
			ModelDb.Encounter<SnappingJaxfruitNormal>(),
			ModelDb.Encounter<TheKinBoss>(),
			ModelDb.Encounter<VantomBoss>(),
			ModelDb.Encounter<VineShamblerNormal>()
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
		if (unlockState.NumberOfRuns == 0)
		{
			Log.Info("First run ever. Presenting rooms in a set order.");
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, NibbitsWeak>(_rooms.normalEncounters, 0);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, SlimesWeak>(_rooms.normalEncounters, 1);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, ShrinkerBeetleWeak>(_rooms.normalEncounters, 2);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, InkletsNormal>(_rooms.normalEncounters, 3);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, MawlerNormal>(_rooms.normalEncounters, 4);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, RubyRaidersNormal>(_rooms.normalEncounters, 5);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, NibbitsNormal>(_rooms.normalEncounters, 6);
			RoomSet.SwapToOrCreateAtIndex<EventModel, ByrdonisNest>(_rooms.events, 0);
			RoomSet.SwapToOrCreateAtIndex<EventModel, SapphireSeed>(_rooms.events, 1);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, ByrdonisElite>(_rooms.eliteEncounters, 0);
			RoomSet.SwapToOrCreateAtIndex<EncounterModel, PhrogParasiteElite>(_rooms.eliteEncounters, 1);
		}
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
