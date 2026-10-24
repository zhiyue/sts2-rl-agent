"""Encounter pool access and C# EncounterModel tag metadata.

Tags mirror the ``EncounterModel.Tags`` overrides in
decompiled/MegaCrit.Sts2.Core.Models.Encounters (v0.107.1). They drive the
C# ActModel.AddWithoutRepeatingTags dedup: a drawn encounter must not share a
tag with (nor equal) the previously chosen encounter.
"""

from __future__ import annotations

from typing import Callable

from sts2_env.encounters.room_set import ROOM_TYPE_BOSS, ROOM_TYPE_ELITE, ROOM_TYPE_MONSTER

EncounterSetup = Callable[..., None]

# C# EncounterModel.Tags overrides, keyed by this repo's setup-function name.
ENCOUNTER_TAGS: dict[str, frozenset[str]] = {
    # Act 1 (Overgrowth)
    "setup_slimes_normal": frozenset({"slimes"}),
    "setup_slimes_weak": frozenset({"slimes"}),
    "setup_flyconid_normal": frozenset({"mushroom", "slimes"}),
    "setup_shrinker_beetle_weak": frozenset({"shrinker"}),
    "setup_nibbits_weak": frozenset({"nibbit"}),
    "setup_fuzzy_wurm_crawler_weak": frozenset({"crawler"}),
    "setup_overgrowth_crawlers": frozenset({"crawler", "shrinker"}),
    "setup_snapping_jaxfruit_normal": frozenset({"mushroom"}),
    # Act 2 (Hive)
    "setup_bowlbugs_normal": frozenset({"workers"}),
    "setup_bowlbugs_weak": frozenset({"workers"}),
    "setup_chompers_normal": frozenset({"chomper"}),
    "setup_exoskeletons_normal": frozenset({"exoskeletons"}),
    "setup_exoskeletons_weak": frozenset({"exoskeletons"}),
    "setup_slumbering_beetle_normal": frozenset({"workers"}),
    "setup_thieving_hopper_weak": frozenset({"thieves"}),
    "setup_tunneler_normal": frozenset({"burrower", "chomper"}),
    "setup_tunneler_weak": frozenset({"burrower"}),
    # Act 3 (Glory)
    "setup_scrolls_of_biting_normal": frozenset({"scrolls"}),
    "setup_scrolls_of_biting_weak": frozenset({"scrolls"}),
    "setup_knights_elite": frozenset({"knights"}),
    # Alternate act 1 (Underdocks)
    "setup_corpse_slugs_normal": frozenset({"slugs"}),
    "setup_corpse_slugs_weak": frozenset({"slugs"}),
    "setup_seapunk_normal": frozenset({"seapunk"}),
    "setup_seapunk_weak": frozenset({"seapunk"}),
}


def encounter_tags(setup_fn: EncounterSetup) -> frozenset[str]:
    """C# EncounterModel.Tags for a setup function (empty when untagged)."""
    return ENCOUNTER_TAGS.get(getattr(setup_fn, "__name__", ""), frozenset())


def shares_tags_with(candidate: EncounterSetup, other: EncounterSetup | None) -> bool:
    """C# EncounterModel.SharesTagsWith."""
    if other is None:
        return False
    return bool(encounter_tags(candidate) & encounter_tags(other))


def get_act_encounter_pools(act_index: int) -> dict[str, list[EncounterSetup]]:
    """Return {weak, normal, elite, boss} setup lists for an act.

    For acts 0-2 the pools are derived from each act's ALL_ENCOUNTER_SPECS
    table, which preserves the exact C# ``GenerateAllEncounters`` array order
    and composition (ActModel.cs feeds AllWeak/AllRegular/AllEliteEncounters
    into the GrabBags in that order, so GrabBag scan order matters for RNG
    parity). Notably C# Hive's pool has no TunnelerNormal.
    Act indexes beyond 2 (the simulator's extra act) keep the act4 lists.
    """
    if act_index == 0:
        from sts2_env.encounters.act1 import ALL_ENCOUNTER_SPECS as specs
    elif act_index == 1:
        from sts2_env.encounters.act2 import ALL_ENCOUNTER_SPECS as specs
    elif act_index == 2:
        from sts2_env.encounters.act3 import ALL_ENCOUNTER_SPECS as specs
    else:
        from sts2_env.encounters.act4 import (
            WEAK_ENCOUNTERS, NORMAL_ENCOUNTERS, ELITE_ENCOUNTERS, BOSS_ENCOUNTERS,
        )
        return {
            "weak": list(WEAK_ENCOUNTERS),
            "normal": list(NORMAL_ENCOUNTERS),
            "elite": list(ELITE_ENCOUNTERS),
            "boss": list(BOSS_ENCOUNTERS),
        }
    return {
        "weak": [
            e.setup for e in specs
            if e.room_type == ROOM_TYPE_MONSTER and e.is_weak
        ],
        "normal": [
            e.setup for e in specs
            if e.room_type == ROOM_TYPE_MONSTER and not e.is_weak
        ],
        "elite": [e.setup for e in specs if e.room_type == ROOM_TYPE_ELITE],
        "boss": [e.setup for e in specs if e.room_type == ROOM_TYPE_BOSS],
    }
