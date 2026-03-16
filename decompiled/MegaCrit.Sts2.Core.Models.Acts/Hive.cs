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

public sealed class Hive : ActModel
{
	public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act2";

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[3]
	{
		ModelDb.Encounter<TheInsatiableBoss>(),
		ModelDb.Encounter<KnowledgeDemonBoss>(),
		ModelDb.Encounter<KaiserCrabBoss>()
	});

	public override IEnumerable<AncientEventModel> AllAncients => new global::_003C_003Ez__ReadOnlyArray<AncientEventModel>(new AncientEventModel[3]
	{
		ModelDb.AncientEvent<Orobas>(),
		ModelDb.AncientEvent<Pael>(),
		ModelDb.AncientEvent<Tezcatara>()
	});

	public override IEnumerable<EventModel> AllEvents => new global::_003C_003Ez__ReadOnlyArray<EventModel>(new EventModel[10]
	{
		ModelDb.Event<Amalgamator>(),
		ModelDb.Event<Bugslayer>(),
		ModelDb.Event<ColorfulPhilosophers>(),
		ModelDb.Event<ColossalFlower>(),
		ModelDb.Event<FieldOfManSizedHoles>(),
		ModelDb.Event<InfestedAutomaton>(),
		ModelDb.Event<LostWisp>(),
		ModelDb.Event<SpiritGrafter>(),
		ModelDb.Event<TheLanternKey>(),
		ModelDb.Event<ZenWeaver>()
	});

	protected override int NumberOfWeakEncounters => 2;

	protected override int BaseNumberOfRooms => 14;

	public override string[] BgMusicOptions => new string[2] { "event:/music/act2_a1_v2", "event:/music/act2_a2_v2" };

	public override string[] MusicBankPaths => new string[2] { "res://banks/desktop/act2_a1.bank", "res://banks/desktop/act2_a2.bank" };

	public override string AmbientSfx => "event:/sfx/ambience/act2_ambience";

	public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_2_skel_data.tres";

	public override string ChestSpineSkinNameNormal => "act2";

	public override string ChestSpineSkinNameStroke => "act2_stroke";

	public override Color MapTraveledColor => new Color("27221C");

	public override Color MapUntraveledColor => new Color("6E7750");

	public override Color MapBgColor => new Color("9B9562");

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EncounterModel>(new EncounterModel[21]
		{
			ModelDb.Encounter<BowlbugsNormal>(),
			ModelDb.Encounter<BowlbugsWeak>(),
			ModelDb.Encounter<ChompersNormal>(),
			ModelDb.Encounter<DecimillipedeElite>(),
			ModelDb.Encounter<EntomancerElite>(),
			ModelDb.Encounter<ExoskeletonsNormal>(),
			ModelDb.Encounter<ExoskeletonsWeak>(),
			ModelDb.Encounter<HunterKillerNormal>(),
			ModelDb.Encounter<KaiserCrabBoss>(),
			ModelDb.Encounter<InfestedPrismsElite>(),
			ModelDb.Encounter<KnowledgeDemonBoss>(),
			ModelDb.Encounter<LouseProgenitorNormal>(),
			ModelDb.Encounter<MytesNormal>(),
			ModelDb.Encounter<OvicopterNormal>(),
			ModelDb.Encounter<SlumberingBeetleNormal>(),
			ModelDb.Encounter<SpinyToadNormal>(),
			ModelDb.Encounter<TheInsatiableBoss>(),
			ModelDb.Encounter<TheObscuraNormal>(),
			ModelDb.Encounter<ThievingHopperWeak>(),
			ModelDb.Encounter<TunnelerNormal>(),
			ModelDb.Encounter<TunnelerWeak>()
		});
	}

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
	{
		List<AncientEventModel> list = AllAncients.ToList();
		if (!unlockState.IsEpochRevealed<OrobasEpoch>())
		{
			list.Remove(ModelDb.AncientEvent<Orobas>());
		}
		return list;
	}

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		Rng rng = new Rng(mapRng.Seed, mapRng.Counter);
		MapPointTypeCounts mapPointTypeCounts = new MapPointTypeCounts(rng);
		int num = mapRng.NextGaussianInt(6, 1, 6, 7);
		if (AscensionHelper.HasAscension(AscensionLevel.Gloom))
		{
			num--;
		}
		return new MapPointTypeCounts(mapRng)
		{
			NumOfUnknowns = mapPointTypeCounts.NumOfUnknowns - 1,
			NumOfRests = num
		};
	}
}
