"""Per-act configuration: room counts, encounter pools, event pools, boss pools.

Based on decompiled MegaCrit.Sts2.Core.Models.Acts source.
"""

from __future__ import annotations

from dataclasses import dataclass, field


@dataclass
class ActConfig:
    """Configuration for a single act."""

    act_index: int
    num_rooms: int  # Number of room rows (used as mapLength input)
    num_weak_encounters: int = 3  # C# NumberOfWeakEncounters (3 for Acts 0/3, 2 for Acts 1/2)
    boss_ids: list[str] = field(default_factory=list)
    elite_ids: list[str] = field(default_factory=list)
    weak_encounter_ids: list[str] = field(default_factory=list)
    strong_encounter_ids: list[str] = field(default_factory=list)
    event_ids: list[str] = field(default_factory=list)
    events_visited: int = 0

    def to_mutable(self) -> "ActConfig":
        return ActConfig(
            act_index=self.act_index,
            num_rooms=self.num_rooms,
            num_weak_encounters=self.num_weak_encounters,
            boss_ids=list(self.boss_ids),
            elite_ids=list(self.elite_ids),
            weak_encounter_ids=list(self.weak_encounter_ids),
            strong_encounter_ids=list(self.strong_encounter_ids),
            event_ids=list(self.event_ids),
            events_visited=self.events_visited,
        )


# ── Act definitions ───────────────────────────────────────────────────
#
# Event pools mirror each C# act's `AllEvents` plus ModelDb.AllSharedEvents
# (v0.107.1). The C# shared pool (18 events) is appended to every act:
#   BrainLeech, CrystalSphere, DollRoom, FakeMerchant, PotionCourier,
#   RanwidTheElder, RelicTrader, RoomFullOfCheese, SelfHelpBook,
#   SlipperyBridge, StoneOfAllTime, Symbiote, TeaMaster, TheFutureOfPotions,
#   TheLegendsWereTrue, ThisOrThat, WarHistorianRepy, WelcomeToWongos
#
# Ancients are NOT part of the event rotation in C#: they are
# AncientEventModels rolled per act (Overgrowth/Underdocks: Neow;
# Hive: Orobas (epoch-gated) / Pael / Tezcatara; Glory: Nonupeipe / Tanx /
# Vakuu; shared ancient: Darv) and only appear at the act's Ancient map node.
# This simulator does not implement the per-act Ancient node yet (acts 2/3
# start directly on the map), so ancients are excluded from these pools.

_SHARED_EVENT_IDS = [
    "BrainLeech", "CrystalSphere", "DollRoom", "FakeMerchant",
    "PotionCourier", "RanwidTheElder", "RelicTrader", "RoomFullOfCheese",
    "SelfHelpBook", "SlipperyBridge", "StoneOfAllTime", "Symbiote",
    "TeaMaster", "TheFutureOfPotions", "TheLegendsWereTrue", "ThisOrThat",
    "WarHistorianRepy", "WelcomeToWongos",
]

ACT_0 = ActConfig(
    act_index=0,
    num_rooms=15,
    boss_ids=["VantomBoss", "CeremonialBeastBoss", "TheKinBoss"],
    elite_ids=["BygoneEffigyElite", "ByrdonisElite", "PhrogParasiteElite"],
    weak_encounter_ids=[
        "ShrinkerBeetleWeak", "FuzzyWurmCrawlerWeak", "NibbitsWeak", "SlimesWeak",
    ],
    strong_encounter_ids=[
        "CubexConstructNormal", "FlyconidNormal", "FogmogNormal",
        "InkletsNormal", "MawlerNormal", "NibbitsNormal", "OvergrowthCrawlers",
        "RubyRaidersNormal", "SlimesNormal", "SlitheringStranglerNormal",
        "SnappingJaxfruitNormal", "VineShamblerNormal",
    ],
    event_ids=[
        # Overgrowth.AllEvents (C# MegaCrit.Sts2.Core.Models.Acts/Overgrowth.cs)
        "AromaOfChaos", "ByrdonisNest", "DenseVegetation",
        "JungleMazeAdventure", "LuminousChoir", "MorphicGrove",
        "SapphireSeed", "SunkenStatue", "TabletOfTruth", "UnrestSite",
        "Wellspring", "WhisperingHollow", "WoodCarvings",
        # ModelDb.AllSharedEvents
        *_SHARED_EVENT_IDS,
    ],
)

ACT_1 = ActConfig(
    act_index=1,
    num_rooms=14,  # C# Hive.BaseNumberOfRooms = 14
    num_weak_encounters=2,  # C# Hive.NumberOfWeakEncounters = 2
    boss_ids=["KaiserCrabBoss", "KnowledgeDemonBoss", "TheInsatiableBoss"],
    elite_ids=["DecimillipedeElite", "EntomancerElite", "InfestedPrismsElite"],
    weak_encounter_ids=[
        "BowlbugsWeak", "ExoskeletonsWeak", "ThievingHopperWeak", "TunnelerWeak",
    ],
    strong_encounter_ids=[
        "BowlbugsNormal", "ChompersNormal", "ExoskeletonsNormal",
        "HunterKillerNormal", "LouseProgenitorNormal", "MytesNormal",
        "OvicopterNormal", "SlumberingBeetleNormal", "SpinyToadNormal",
        "TheObscuraNormal",
    ],
    event_ids=[
        # Hive.AllEvents (C# MegaCrit.Sts2.Core.Models.Acts/Hive.cs)
        "Amalgamator", "Bugslayer", "ColorfulPhilosophers", "ColossalFlower",
        "FieldOfManSizedHoles", "InfestedAutomaton", "LostWisp",
        "SpiritGrafter", "TheLanternKey", "ZenWeaver",
        # ModelDb.AllSharedEvents
        *_SHARED_EVENT_IDS,
    ],
)

ACT_2 = ActConfig(
    act_index=2,
    num_rooms=13,  # C# Glory.BaseNumberOfRooms = 13
    num_weak_encounters=2,  # C# Glory.NumberOfWeakEncounters = 2
    boss_ids=["AeonglassBoss", "QueenBoss", "TestSubjectBoss"],
    elite_ids=["KnightsElite", "MechaKnightElite", "SoulNexusElite"],
    weak_encounter_ids=[
        "DevotedSculptorWeak", "ScrollsOfBitingWeak", "TurretOperatorWeak",
    ],
    strong_encounter_ids=[
        "AxebotsNormal", "ConstructMenagerieNormal", "FabricatorNormal",
        "FrogKnightNormal", "GlobeHeadNormal", "OwlMagistrateNormal",
        "ScrollsOfBitingNormal", "SlimedBerserkerNormal",
        "TheLostAndForgottenNormal",
    ],
    event_ids=[
        # Glory.AllEvents (C# MegaCrit.Sts2.Core.Models.Acts/Glory.cs)
        "BattlewornDummy", "GraveOfTheForgotten", "HungryForMushrooms",
        "Reflections", "RoundTeaParty", "TinkerTime", "Trial",
        # ModelDb.AllSharedEvents
        *_SHARED_EVENT_IDS,
    ],
)

ALL_ACTS = [ACT_0, ACT_1, ACT_2]


def get_act_config(act_index: int) -> ActConfig:
    if 0 <= act_index < len(ALL_ACTS):
        return ALL_ACTS[act_index]
    raise ValueError(f"Invalid act index: {act_index}")
