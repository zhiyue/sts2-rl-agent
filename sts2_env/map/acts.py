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


# ── Act definitions ───────────────────────────────────────────────────

ACT_0 = ActConfig(
    act_index=0,
    num_rooms=15,
    boss_ids=["TheLich"],
    elite_ids=["SentryAndSentry", "GremlinNob", "BookOfStabbing"],
    weak_encounter_ids=[
        "TwoLouses", "ThreeJawWorms", "SmallSlimes",
        "Cultist", "GremlinGang",
    ],
    strong_encounter_ids=[
        "BlueSlaver", "RedSlaver", "FungiBeast",
        "LooterGroup", "ExordiumWildlife", "LotOfSlimes",
    ],
    event_ids=[
        "AbyssalBaths", "Amalgamator", "BattlewornDummy", "BrainLeech",
        "Bugslayer", "ByrdonisNest", "ColorfulPhilosophers",
        "ColossalFlower", "Darv", "DenseVegetation",
        "DoorsOfLightAndDark", "DrowningBeacon",
        "GraveOfTheForgotten", "HungryForMushrooms",
        "InfestedAutomaton", "LostWisp", "Nonupeipe",
        "Orobas", "Pael", "PunchOff", "Reflections",
        "RoomFullOfCheese", "RoundTeaParty", "SapphireSeed",
        "SelfHelpBook", "SpiritGrafter", "SunkenStatue",
        "SunkenTreasury", "TabletOfTruth", "Tanx",
        "TeaMaster", "Tezcatara", "TheLegendsWereTrue",
        "ThisOrThat", "TinkerTime", "TrashHeap", "Trial",
        "UnrestSite", "Vakuu", "Wellspring",
        "WoodCarvings", "ZenWeaver",
    ],
)

ACT_1 = ActConfig(
    act_index=1,
    num_rooms=14,  # C# Hive.BaseNumberOfRooms = 14
    num_weak_encounters=2,  # C# Hive.NumberOfWeakEncounters = 2
    boss_ids=["TheCollector", "Automaton", "Champ"],
    elite_ids=["TaskMaster", "SphericGuardian", "Snecko"],
    weak_encounter_ids=[
        "SnakePlant", "Centurion", "ThreeByrds",
    ],
    strong_encounter_ids=[
        "SlaverGroup", "BookOfStabbing", "MushroomGroup",
    ],
    event_ids=[
        "AbyssalBaths", "Amalgamator", "AromaOfChaos",
        "BattlewornDummy", "Bugslayer", "ByrdonisNest",
        "ColorfulPhilosophers", "ColossalFlower", "CrystalSphere",
        "Darv", "DenseVegetation", "DollRoom",
        "DoorsOfLightAndDark", "DrowningBeacon", "EndlessConveyor",
        "FakeMerchant", "FieldOfManSizedHoles",
        "GraveOfTheForgotten", "HungryForMushrooms",
        "InfestedAutomaton", "JungleMazeAdventure",
        "LostWisp", "LuminousChoir", "MorphicGrove",
        "Nonupeipe", "Orobas", "Pael", "PotionCourier",
        "PunchOff", "RanwidTheElder", "Reflections",
        "RelicTrader", "RoundTeaParty", "SapphireSeed",
        "SelfHelpBook", "SlipperyBridge", "SpiralingWhirlpool",
        "SpiritGrafter", "StoneOfAllTime", "SunkenStatue",
        "SunkenTreasury", "Symbiote", "TabletOfTruth",
        "Tanx", "Tezcatara", "TheFutureOfPotions",
        "TheLanternKey", "ThisOrThat", "TinkerTime",
        "TrashHeap", "Trial", "UnrestSite", "Vakuu",
        "WaterloggedScriptorium", "WelcomeToWongos",
        "Wellspring", "WhisperingHollow", "WoodCarvings",
        "ZenWeaver",
    ],
)

ACT_2 = ActConfig(
    act_index=2,
    num_rooms=13,  # C# Glory.BaseNumberOfRooms = 13
    num_weak_encounters=2,  # C# Glory.NumberOfWeakEncounters = 2
    boss_ids=["AwakenedOne", "TimeEater", "DonuAndDeca"],
    elite_ids=["GiantHead", "Nemesis", "Reptomancer"],
    weak_encounter_ids=[
        "Darkling", "OrbWalker",
    ],
    strong_encounter_ids=[
        "WrithingMass", "Transient", "Maw",
    ],
    event_ids=[
        "AbyssalBaths", "Amalgamator", "AromaOfChaos",
        "BattlewornDummy", "Bugslayer", "ByrdonisNest",
        "ColorfulPhilosophers", "ColossalFlower", "CrystalSphere",
        "Darv", "DenseVegetation", "DoorsOfLightAndDark",
        "DrowningBeacon", "EndlessConveyor", "FakeMerchant",
        "FieldOfManSizedHoles", "GraveOfTheForgotten",
        "HungryForMushrooms", "InfestedAutomaton",
        "JungleMazeAdventure", "LostWisp", "LuminousChoir",
        "MorphicGrove", "Nonupeipe", "Orobas", "Pael",
        "PotionCourier", "PunchOff", "RanwidTheElder",
        "Reflections", "RelicTrader", "RoundTeaParty",
        "SapphireSeed", "SelfHelpBook", "SlipperyBridge",
        "SpiralingWhirlpool", "SpiritGrafter",
        "SunkenStatue", "SunkenTreasury", "Symbiote",
        "TabletOfTruth", "Tanx", "Tezcatara",
        "TheFutureOfPotions", "TheLanternKey", "ThisOrThat",
        "TinkerTime", "TrashHeap", "Trial", "UnrestSite",
        "Vakuu", "WaterloggedScriptorium", "Wellspring",
        "WhisperingHollow", "WoodCarvings", "ZenWeaver",
    ],
)

ALL_ACTS = [ACT_0, ACT_1, ACT_2]


def get_act_config(act_index: int) -> ActConfig:
    if 0 <= act_index < len(ALL_ACTS):
        return ALL_ACTS[act_index]
    raise ValueError(f"Invalid act index: {act_index}")
