"""Character definitions for all five playable STS2 characters.

Each CharacterConfig captures the starting stats, starting relic,
starting deck composition, card-pool membership, and any character-
specific mechanic (orb slots, Stars, Osty companion).

Data sourced from the decompiled C# character and card-pool models.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Sequence

from sts2_env.cards.base import CardInstance
from sts2_env.core.enums import CardId
from sts2_env.relics.base import RelicId


# ---------------------------------------------------------------------------
# Dataclass
# ---------------------------------------------------------------------------

@dataclass(frozen=True)
class CharacterConfig:
    """Immutable description of a playable character."""

    character_id: str
    starting_hp: int
    starting_gold: int
    max_energy: int
    starting_relic: RelicId
    starting_deck: tuple[tuple[CardId, int], ...]
    """(CardId, count) pairs that make up the starting deck."""
    card_pool: tuple[CardId, ...]
    """Every CardId in this character's class card pool."""
    # Character-specific mechanic flags
    base_orb_slots: int = 0
    uses_stars: bool = False
    has_osty: bool = False

    @property
    def starting_deck_size(self) -> int:
        return sum(count for _, count in self.starting_deck)


# ---------------------------------------------------------------------------
# Card pools  (full, unlocked)
# ---------------------------------------------------------------------------

_IRONCLAD_CARD_POOL: tuple[CardId, ...] = (
    # -- Basic --
    CardId.STRIKE_IRONCLAD,
    CardId.DEFEND_IRONCLAD,
    CardId.BASH,
    # -- Common --
    CardId.ANGER,
    CardId.ARMAMENTS,
    CardId.BLOOD_WALL,
    CardId.BLOODLETTING,
    CardId.BODY_SLAM,
    CardId.BREAKTHROUGH,
    CardId.CINDER,
    CardId.HAVOC,
    CardId.HEADBUTT,
    CardId.IRON_WAVE,
    CardId.MOLTEN_FIST,
    CardId.PERFECTED_STRIKE,
    CardId.POMMEL_STRIKE,
    CardId.SETUP_STRIKE_CARD,
    CardId.SHRUG_IT_OFF,
    CardId.SWORD_BOOMERANG,
    CardId.THUNDERCLAP,
    CardId.TREMBLE,
    CardId.TRUE_GRIT,
    CardId.TWIN_STRIKE,
    # -- Uncommon --
    CardId.ASHEN_STRIKE,
    CardId.BATTLE_TRANCE,
    CardId.BLUDGEON,
    CardId.BULLY,
    CardId.BURNING_PACT,
    CardId.DEMONIC_SHIELD,
    CardId.DISMANTLE,
    CardId.DOMINATE,
    CardId.DRUM_OF_BATTLE_CARD,
    CardId.EVIL_EYE,
    CardId.EXPECT_A_FIGHT,
    CardId.FEEL_NO_PAIN_CARD,
    CardId.FIGHT_ME,
    CardId.FLAME_BARRIER_CARD,
    CardId.FORGOTTEN_RITUAL,
    CardId.GRAPPLE,
    CardId.HEMOKINESIS,
    CardId.HOWL_FROM_BEYOND,
    CardId.INFERNAL_BLADE,
    CardId.INFERNO_CARD,
    CardId.INFLAME,
    CardId.JUGGLING_CARD,
    CardId.PILLAGE,
    CardId.RAGE_CARD,
    CardId.RAMPAGE,
    CardId.RUPTURE_CARD,
    CardId.SECOND_WIND,
    CardId.SPITE,
    CardId.STAMPEDE_CARD,
    CardId.STOMP,
    CardId.STONE_ARMOR,
    CardId.TAUNT,
    CardId.UNRELENTING,
    CardId.UPPERCUT,
    CardId.VICIOUS_CARD,
    CardId.WHIRLWIND,
    # -- Rare --
    CardId.AGGRESSION_CARD,
    CardId.BARRICADE_CARD,
    CardId.BRAND,
    CardId.CASCADE,
    CardId.COLOSSUS_CARD,
    CardId.CONFLAGRATION,
    CardId.CRIMSON_MANTLE,
    CardId.CRUELTY_CARD,
    CardId.DARK_EMBRACE_CARD,
    CardId.DEMON_FORM_CARD,
    CardId.FEED,
    CardId.FIEND_FIRE,
    CardId.HELLRAISER_CARD,
    CardId.IMPERVIOUS,
    CardId.JUGGERNAUT_CARD,
    CardId.MANGLE,
    CardId.OFFERING,
    CardId.ONE_TWO_PUNCH_CARD,
    CardId.PACTS_END,
    CardId.PRIMAL_FORCE,
    CardId.PYRE,
    CardId.STOKE,
    CardId.TANK_CARD,
    CardId.TEAR_ASUNDER,
    CardId.THRASH,
    CardId.UNMOVABLE,
    # -- Ancient --
    CardId.BREAK,
    CardId.CORRUPTION_CARD,
)

_SILENT_CARD_POOL: tuple[CardId, ...] = (
    # -- Basic --
    CardId.STRIKE_SILENT,
    CardId.DEFEND_SILENT,
    CardId.NEUTRALIZE,
    CardId.SURVIVOR,
    # -- Common --
    CardId.ACROBATICS,
    CardId.ANTICIPATE,
    CardId.BACKFLIP,
    CardId.BLADE_DANCE,
    CardId.CLOAK_AND_DAGGER,
    CardId.DAGGER_SPRAY,
    CardId.DAGGER_THROW,
    CardId.DEADLY_POISON,
    CardId.DEFLECT,
    CardId.DODGE_AND_ROLL,
    CardId.FLICK_FLACK,
    CardId.LEADING_STRIKE,
    CardId.PIERCING_WAIL,
    CardId.POISONED_STAB,
    CardId.PREPARED,
    CardId.RICOCHET,
    CardId.SLICE,
    CardId.SNAKEBITE,
    CardId.SUCKER_PUNCH,
    CardId.UNTOUCHABLE,
    # -- Uncommon --
    CardId.ACCURACY_CARD,
    CardId.BACKSTAB,
    CardId.BLUR_CARD,
    CardId.BOUNCING_FLASK,
    CardId.BUBBLE_BUBBLE,
    CardId.CALCULATED_GAMBLE,
    CardId.DASH,
    CardId.ESCAPE_PLAN,
    CardId.EXPERTISE,
    CardId.EXPOSE,
    CardId.FINISHER,
    CardId.FLANKING,
    CardId.FLECHETTES,
    CardId.FOLLOW_THROUGH,
    CardId.FOOTWORK,
    CardId.HAND_TRICK,
    CardId.HAZE,
    CardId.HIDDEN_DAGGERS,
    CardId.INFINITE_BLADES_CARD,
    CardId.LEG_SWEEP,
    CardId.MEMENTO_MORI,
    CardId.MIRAGE,
    CardId.NOXIOUS_FUMES_CARD,
    CardId.OUTBREAK_CARD,
    CardId.PHANTOM_BLADES_CARD,
    CardId.PINPOINT,
    CardId.POUNCE,
    CardId.PRECISE_CUT,
    CardId.PREDATOR,
    CardId.REFLEX,
    CardId.SKEWER,
    CardId.SPEEDSTER_CARD,
    CardId.STRANGLE,
    CardId.TACTICIAN,
    CardId.UP_MY_SLEEVE,
    CardId.WELL_LAID_PLANS,
    # -- Rare --
    CardId.ABRASIVE,
    CardId.ACCELERANT,
    CardId.ADRENALINE,
    CardId.AFTERIMAGE_CARD,
    CardId.ASSASSINATE,
    CardId.BLADE_OF_INK,
    CardId.BULLET_TIME,
    CardId.BURST,
    CardId.CORROSIVE_WAVE,
    CardId.ECHOING_SLASH,
    CardId.ENVENOM_CARD,
    CardId.FAN_OF_KNIVES_CARD,
    CardId.GRAND_FINALE,
    CardId.KNIFE_TRAP,
    CardId.MALAISE,
    CardId.MASTER_PLANNER,
    CardId.MURDER,
    CardId.NIGHTMARE,
    CardId.SERPENT_FORM_CARD,
    CardId.SHADOW_STEP,
    CardId.SHADOWMELD,
    CardId.SNEAKY_CARD,
    CardId.STORM_OF_STEEL,
    CardId.THE_HUNT,
    CardId.TOOLS_OF_THE_TRADE,
    CardId.TRACKING,
    # -- Ancient --
    CardId.SUPPRESS,
    CardId.WRAITH_FORM,
)

_DEFECT_CARD_POOL: tuple[CardId, ...] = (
    # -- Basic --
    CardId.STRIKE_DEFECT,
    CardId.DEFEND_DEFECT,
    CardId.ZAP,
    CardId.DUALCAST,
    # -- Common --
    CardId.BALL_LIGHTNING,
    CardId.BARRAGE,
    CardId.BEAM_CELL,
    CardId.BOOST_AWAY,
    CardId.CHARGE_BATTERY,
    CardId.CLAW,
    CardId.COLD_SNAP,
    CardId.COMPILE_DRIVER,
    CardId.COOLHEADED,
    CardId.FOCUSED_STRIKE_CARD,
    CardId.GO_FOR_THE_EYES,
    CardId.GUNK_UP,
    CardId.HOLOGRAM,
    CardId.HOTFIX,
    CardId.LEAP,
    CardId.LIGHTNING_ROD,
    CardId.MOMENTUM_STRIKE,
    CardId.SWEEPING_BEAM,
    CardId.TURBO,
    CardId.UPROAR,
    # -- Uncommon --
    CardId.BOOT_SEQUENCE,
    CardId.BULK_UP,
    CardId.CAPACITOR,
    CardId.CHAOS,
    CardId.CHILL,
    CardId.COMPACT,
    CardId.DARKNESS_CARD,
    CardId.DOUBLE_ENERGY,
    CardId.ENERGY_SURGE,
    CardId.FERAL,
    CardId.FIGHT_THROUGH,
    CardId.FTL,
    CardId.FUSION,
    CardId.GLACIER,
    CardId.GLASSWORK,
    CardId.HAILSTORM,
    CardId.ITERATION_CARD,
    CardId.LOOP_CARD,
    CardId.NULL_CARD,
    CardId.OVERCLOCK,
    CardId.REFRACT,
    CardId.ROCKET_PUNCH,
    CardId.SCAVENGE,
    CardId.SCRAPE,
    CardId.SHADOW_SHIELD,
    CardId.SKIM,
    CardId.SMOKESTACK,
    CardId.STORM_CARD,
    CardId.SUBROUTINE,
    CardId.SUNDER,
    CardId.SYNCHRONIZE,
    CardId.SYNTHESIS,
    CardId.TEMPEST,
    CardId.TESLA_COIL,
    CardId.THUNDER_CARD,
    CardId.WHITE_NOISE,
    # -- Rare --
    CardId.ADAPTIVE_STRIKE,
    CardId.ALL_FOR_ONE,
    CardId.BUFFER_CARD,
    CardId.CONSUMING_SHADOW,
    CardId.COOLANT,
    CardId.CREATIVE_AI_CARD,
    CardId.DEFRAGMENT,
    CardId.ECHO_FORM_CARD,
    CardId.FLAK_CANNON,
    CardId.GENETIC_ALGORITHM,
    CardId.HELIX_DRILL,
    CardId.HYPERBEAM,
    CardId.ICE_LANCE,
    CardId.IGNITION,
    CardId.MACHINE_LEARNING_CARD,
    CardId.METEOR_STRIKE,
    CardId.MODDED,
    CardId.MULTI_CAST,
    CardId.RAINBOW,
    CardId.REBOOT,
    CardId.SHATTER,
    CardId.SIGNAL_BOOST,
    CardId.SPINNER_CARD,
    CardId.SUPERCRITICAL,
    CardId.TRASH_TO_TREASURE,
    CardId.VOLTAIC,
    # -- Ancient --
    CardId.BIASED_COGNITION_CARD,
    CardId.QUADCAST,
)

_REGENT_CARD_POOL: tuple[CardId, ...] = (
    # -- Basic --
    CardId.STRIKE_REGENT,
    CardId.DEFEND_REGENT,
    CardId.FALLING_STAR,
    CardId.VENERATE,
    # -- Common --
    CardId.ASTRAL_PULSE,
    CardId.BEGONE,
    CardId.CELESTIAL_MIGHT,
    CardId.CLOAK_OF_STARS,
    CardId.COLLISION_COURSE,
    CardId.COSMIC_INDIFFERENCE,
    CardId.CRESCENT_SPEAR,
    CardId.CRUSH_UNDER,
    CardId.GATHER_LIGHT,
    CardId.GLITTERSTREAM,
    CardId.GLOW,
    CardId.GUIDING_STAR,
    CardId.HIDDEN_CACHE,
    CardId.KNOW_THY_PLACE,
    CardId.PATTER,
    CardId.PHOTON_CUT,
    CardId.REFINE_BLADE,
    CardId.SOLAR_STRIKE,
    CardId.SPOILS_OF_BATTLE,
    CardId.WROUGHT_IN_WAR,
    # -- Uncommon --
    CardId.ALIGNMENT,
    CardId.BLACK_HOLE,
    CardId.BULWARK,
    CardId.CHARGE,
    CardId.CHILD_OF_THE_STARS,
    CardId.CONQUEROR,
    CardId.CONVERGENCE,
    CardId.DEVASTATE,
    CardId.FURNACE,
    CardId.GAMMA_BLAST,
    CardId.GLIMMER,
    CardId.HEGEMONY,
    CardId.KINGLY_KICK,
    CardId.KINGLY_PUNCH,
    CardId.KNOCKOUT_BLOW,
    CardId.LARGESSE,
    CardId.LUNAR_BLAST,
    CardId.MANIFEST_AUTHORITY,
    CardId.MONOLOGUE_CARD,
    CardId.ORBIT,
    CardId.PALE_BLUE_DOT,
    CardId.PARRY_CARD,
    CardId.PARTICLE_WALL,
    CardId.PILLAR_OF_CREATION,
    CardId.PROPHESIZE,
    CardId.QUASAR,
    CardId.RADIATE,
    CardId.REFLECT_CARD,
    CardId.RESONANCE,
    CardId.ROYAL_GAMBLE,
    CardId.SHINING_STRIKE,
    CardId.SPECTRUM_SHIFT,
    CardId.STARDUST,
    CardId.SUMMON_FORTH,
    CardId.SUPERMASSIVE,
    CardId.TERRAFORMING,
    # -- Rare --
    CardId.ARSENAL,
    CardId.BEAT_INTO_SHAPE,
    CardId.BIG_BANG,
    CardId.BOMBARDMENT,
    CardId.BUNDLE_OF_JOY,
    CardId.COMET,
    CardId.CRASH_LANDING,
    CardId.DECISIONS_DECISIONS,
    CardId.DYING_STAR,
    CardId.FOREGONE_CONCLUSION,
    CardId.GENESIS,
    CardId.GUARDS,
    CardId.HAMMER_TIME,
    CardId.HEAVENLY_DRILL,
    CardId.HEIRLOOM_HAMMER,
    CardId.I_AM_INVINCIBLE,
    CardId.MAKE_IT_SO,
    CardId.MONARCHS_GAZE_CARD,
    CardId.NEUTRON_AEGIS,
    CardId.ROYALTIES_CARD,
    CardId.SEEKING_EDGE,
    CardId.SEVEN_STARS,
    CardId.SWORD_SAGE,
    CardId.THE_SMITH,
    CardId.TYRANNY_CARD,
    CardId.VOID_FORM,
    # -- Ancient --
    CardId.METEOR_SHOWER,
    CardId.THE_SEALED_THRONE,
)

_NECROBINDER_CARD_POOL: tuple[CardId, ...] = (
    # -- Basic --
    CardId.STRIKE_NECROBINDER,
    CardId.DEFEND_NECROBINDER,
    CardId.BODYGUARD,
    CardId.UNLEASH,
    # -- Common --
    CardId.AFTERLIFE,
    CardId.BLIGHT_STRIKE,
    CardId.DEFILE,
    CardId.DEFY,
    CardId.DRAIN_POWER,
    CardId.FEAR,
    CardId.FLATTEN,
    CardId.GRAVE_WARDEN,
    CardId.GRAVEBLAST,
    CardId.INVOKE,
    CardId.NEGATIVE_PULSE,
    CardId.POKE,
    CardId.PULL_AGGRO,
    CardId.REAP,
    CardId.REAVE,
    CardId.SCOURGE,
    CardId.SCULPTING_STRIKE,
    CardId.SNAP,
    CardId.SOW,
    CardId.WISP,
    # -- Uncommon --
    CardId.BONE_SHARDS,
    CardId.BORROWED_TIME,
    CardId.BURY,
    CardId.CALCIFY_CARD,
    CardId.CAPTURE_SPIRIT,
    CardId.CLEANSE,
    CardId.COUNTDOWN_CARD,
    CardId.DANSE_MACABRE,
    CardId.DEATH_MARCH,
    CardId.DEATHBRINGER,
    CardId.DEATHS_DOOR,
    CardId.DEBILITATE_CARD,
    CardId.DELAY,
    CardId.DIRGE,
    CardId.DREDGE,
    CardId.ENFEEBLING_TOUCH,
    CardId.FETCH,
    CardId.FRIENDSHIP,
    CardId.HAUNT,
    CardId.HIGH_FIVE,
    CardId.LEGION_OF_BONE,
    CardId.LETHALITY_CARD,
    CardId.MELANCHOLY,
    CardId.NO_ESCAPE,
    CardId.PAGESTORM,
    CardId.PARSE,
    CardId.PULL_FROM_BELOW,
    CardId.PUTREFY,
    CardId.RATTLE,
    CardId.RIGHT_HAND_HAND,
    CardId.SEVERANCE,
    CardId.SHROUD,
    CardId.SIC_EM,
    CardId.SLEIGHT_OF_FLESH,
    CardId.SPUR,
    CardId.VEILPIERCER,
    # -- Rare --
    CardId.BANSHEES_CRY,
    CardId.CALL_OF_THE_VOID,
    CardId.DEMESNE,
    CardId.DEVOUR_LIFE_CARD,
    CardId.EIDOLON,
    CardId.END_OF_DAYS,
    CardId.ERADICATE,
    CardId.GLIMPSE_BEYOND,
    CardId.HANG,
    CardId.MISERY,
    CardId.NECRO_MASTERY_CARD,
    CardId.NEUROSURGE,
    CardId.OBLIVION,
    CardId.REANIMATE,
    CardId.REAPER_FORM,
    CardId.SACRIFICE,
    CardId.SEANCE,
    CardId.SENTRY_MODE,
    CardId.SHARED_FATE,
    CardId.SOUL_STORM,
    CardId.SPIRIT_OF_ASH,
    CardId.SQUEEZE,
    CardId.THE_SCYTHE,
    CardId.TIMES_UP,
    CardId.TRANSFIGURE,
    CardId.UNDEATH,
    # -- Ancient --
    CardId.FORBIDDEN_GRIMOIRE,
    CardId.PROTECTOR,
)


# ---------------------------------------------------------------------------
# Colorless card pool (shared across all characters)
# ---------------------------------------------------------------------------

COLORLESS_CARD_POOL: tuple[CardId, ...] = (
    # -- Uncommon --
    CardId.AUTOMATION,
    CardId.BELIEVE_IN_YOU,
    CardId.CATASTROPHE,
    CardId.COORDINATE_CARD,
    CardId.DARK_SHACKLES,
    CardId.DISCOVERY,
    CardId.DRAMATIC_ENTRANCE,
    CardId.EQUILIBRIUM,
    CardId.FASTEN,
    CardId.FINESSE,
    CardId.FISTICUFFS,
    CardId.FLASH_OF_STEEL,
    CardId.GANG_UP,
    CardId.HUDDLE_UP,
    CardId.IMPATIENCE,
    CardId.INTERCEPT_CARD,
    CardId.JACK_OF_ALL_TRADES,
    CardId.LIFT,
    CardId.MIND_BLAST,
    CardId.OMNISLICE,
    CardId.PANACHE_CARD,
    CardId.PANIC_BUTTON,
    CardId.PREP_TIME,
    CardId.PRODUCTION,
    CardId.PROLONG,
    CardId.PROWESS,
    CardId.PURITY,
    CardId.RESTLESSNESS,
    CardId.SEEKER_STRIKE,
    CardId.SHOCKWAVE,
    CardId.SPLASH,
    CardId.STRATAGEM,
    CardId.TAG_TEAM,
    CardId.THE_BOMB_CARD,
    CardId.THINKING_AHEAD,
    CardId.THRUMMING_HATCHET,
    CardId.ULTIMATE_DEFEND,
    CardId.ULTIMATE_STRIKE,
    CardId.VOLLEY,
    # -- Rare --
    CardId.ALCHEMIZE,
    CardId.ANOINTED,
    CardId.BEACON_OF_HOPE,
    CardId.BEAT_DOWN,
    CardId.BOLAS,
    CardId.CALAMITY_CARD,
    CardId.ENTROPY,
    CardId.ETERNAL_ARMOR,
    CardId.GOLD_AXE,
    CardId.HAND_OF_GREED,
    CardId.HIDDEN_GEM,
    CardId.JACKPOT,
    CardId.KNOCKDOWN,
    CardId.MASTER_OF_STRATEGY,
    CardId.MAYHEM_CARD,
    CardId.MIMIC,
    CardId.NOSTALGIA_CARD,
    CardId.RALLY,
    CardId.REND,
    CardId.ROLLING_BOULDER,
    CardId.SALVO,
    CardId.SCRAWL,
    CardId.SECRET_TECHNIQUE,
    CardId.SECRET_WEAPON,
    CardId.THE_GAMBIT,
)


# ---------------------------------------------------------------------------
# Character configurations
# ---------------------------------------------------------------------------

IRONCLAD = CharacterConfig(
    character_id="Ironclad",
    starting_hp=80,
    starting_gold=99,
    max_energy=3,
    starting_relic=RelicId.BURNING_BLOOD,
    starting_deck=(
        (CardId.STRIKE_IRONCLAD, 5),
        (CardId.DEFEND_IRONCLAD, 4),
        (CardId.BASH, 1),
    ),
    card_pool=_IRONCLAD_CARD_POOL,
)

SILENT = CharacterConfig(
    character_id="Silent",
    starting_hp=70,
    starting_gold=99,
    max_energy=3,
    starting_relic=RelicId.RING_OF_THE_SNAKE,
    starting_deck=(
        (CardId.STRIKE_SILENT, 5),
        (CardId.DEFEND_SILENT, 5),
        (CardId.NEUTRALIZE, 1),
        (CardId.SURVIVOR, 1),
    ),
    card_pool=_SILENT_CARD_POOL,
)

DEFECT = CharacterConfig(
    character_id="Defect",
    starting_hp=75,
    starting_gold=99,
    max_energy=3,
    starting_relic=RelicId.CRACKED_CORE,
    starting_deck=(
        (CardId.STRIKE_DEFECT, 4),
        (CardId.DEFEND_DEFECT, 4),
        (CardId.ZAP, 1),
        (CardId.DUALCAST, 1),
    ),
    card_pool=_DEFECT_CARD_POOL,
    base_orb_slots=3,
)

REGENT = CharacterConfig(
    character_id="Regent",
    starting_hp=75,
    starting_gold=99,
    max_energy=3,
    starting_relic=RelicId.DIVINE_RIGHT,
    starting_deck=(
        (CardId.STRIKE_REGENT, 4),
        (CardId.DEFEND_REGENT, 4),
        (CardId.FALLING_STAR, 1),
        (CardId.VENERATE, 1),
    ),
    card_pool=_REGENT_CARD_POOL,
    uses_stars=True,
)

NECROBINDER = CharacterConfig(
    character_id="Necrobinder",
    starting_hp=66,
    starting_gold=99,
    max_energy=3,
    starting_relic=RelicId.BOUND_PHYLACTERY,
    starting_deck=(
        (CardId.STRIKE_NECROBINDER, 4),
        (CardId.DEFEND_NECROBINDER, 4),
        (CardId.BODYGUARD, 1),
        (CardId.UNLEASH, 1),
    ),
    card_pool=_NECROBINDER_CARD_POOL,
    has_osty=True,
)


ALL_CHARACTERS: Sequence[CharacterConfig] = (
    IRONCLAD,
    SILENT,
    DEFECT,
    REGENT,
    NECROBINDER,
)

_BY_ID: dict[str, CharacterConfig] = {
    cfg.character_id.lower(): cfg for cfg in ALL_CHARACTERS
}


# ---------------------------------------------------------------------------
# Public helpers
# ---------------------------------------------------------------------------

def get_character(character_id: str) -> CharacterConfig:
    """Look up a character config by name (case-insensitive).

    >>> get_character("Ironclad").starting_hp
    80
    """
    key = character_id.lower()
    cfg = _BY_ID.get(key)
    if cfg is None:
        valid = ", ".join(sorted(_BY_ID))
        raise ValueError(
            f"Unknown character {character_id!r}. "
            f"Valid ids: {valid}"
        )
    return cfg


def create_starting_deck(character_id: str) -> list[CardInstance]:
    """Create the starting deck for the given character.

    Delegates to the per-character ``create_<char>_starter_deck``
    factory so that each card instance gets correct stats, keywords,
    and a unique ``instance_id``.

    >>> len(create_starting_deck("Ironclad"))
    10
    """
    key = character_id.lower()
    factory = _STARTER_DECK_FACTORIES.get(key)
    if factory is None:
        valid = ", ".join(sorted(_STARTER_DECK_FACTORIES))
        raise ValueError(
            f"Unknown character {character_id!r}. "
            f"Valid ids: {valid}"
        )
    return factory()


# ---------------------------------------------------------------------------
# Starter-deck factory registry
#
# Each character module already defines a create_<char>_starter_deck()
# that builds the exact cards with the right factories.  We import
# lazily so this module can be imported without pulling in every card
# effect function at import time.
# ---------------------------------------------------------------------------

def _make_ironclad_deck() -> list[CardInstance]:
    from sts2_env.cards.ironclad import (
        create_ironclad_starter_deck,
    )
    return create_ironclad_starter_deck()


def _make_silent_deck() -> list[CardInstance]:
    from sts2_env.cards.silent import (
        create_silent_starter_deck,
    )
    return create_silent_starter_deck()


def _make_defect_deck() -> list[CardInstance]:
    from sts2_env.cards.defect import (
        create_defect_starter_deck,
    )
    return create_defect_starter_deck()


def _make_regent_deck() -> list[CardInstance]:
    from sts2_env.cards.regent import (
        create_regent_starter_deck,
    )
    return create_regent_starter_deck()


def _make_necrobinder_deck() -> list[CardInstance]:
    from sts2_env.cards.necrobinder import (
        create_necrobinder_starter_deck,
    )
    return create_necrobinder_starter_deck()


_STARTER_DECK_FACTORIES: dict[str, object] = {
    "ironclad": _make_ironclad_deck,
    "silent": _make_silent_deck,
    "defect": _make_defect_deck,
    "regent": _make_regent_deck,
    "necrobinder": _make_necrobinder_deck,
}
