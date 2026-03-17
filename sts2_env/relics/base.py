"""RelicInstance base class with all hook method stubs.

Every relic should inherit from this and override only the hooks it needs.
The Hook dispatcher iterates all relic listeners and calls these methods.
Follows the same pattern as PowerInstance but for relics.
"""

from __future__ import annotations

from enum import Enum, auto
from typing import TYPE_CHECKING

from sts2_env.core.enums import RelicRarity, ValueProp, CombatSide, CardType, PowerId

if TYPE_CHECKING:
    from sts2_env.cards.base import CardInstance
    from sts2_env.core.creature import Creature
    from sts2_env.core.combat import CombatState
    from sts2_env.run.rest_site import RestSiteOption
    from sts2_env.run.reward_objects import CardReward, Reward
    from sts2_env.run.rewards import CardRewardGenerationOptions
    from sts2_env.run.shop import ShopInventory
    from sts2_env.run.rooms import Room
    from sts2_env.run.run_state import RunState


class RelicId(Enum):
    """Unique identifier for each relic."""
    # Starter
    BURNING_BLOOD = auto()
    BLACK_BLOOD = auto()
    RING_OF_THE_SNAKE = auto()
    RING_OF_THE_DRAKE = auto()
    CRACKED_CORE = auto()
    INFUSED_CORE = auto()
    BOUND_PHYLACTERY = auto()
    PHYLACTERY_UNBOUND = auto()
    DIVINE_RIGHT = auto()
    DIVINE_DESTINY = auto()

    # Common
    AMETHYST_AUBERGINE = auto()
    ANCHOR = auto()
    BAG_OF_PREPARATION = auto()
    BLOOD_VIAL = auto()
    BONE_FLUTE = auto()
    BRONZE_SCALES = auto()
    CENTENNIAL_PUZZLE = auto()
    DATA_DISK = auto()
    FENCING_MANUAL = auto()
    FESTIVE_POPPER = auto()
    GORGET = auto()
    HAPPY_FLOWER = auto()
    JUZU_BRACELET = auto()
    LANTERN = auto()
    MEAL_TICKET = auto()
    ODDLY_SMOOTH_STONE = auto()
    PENDULUM = auto()
    PERMAFROST = auto()
    POTION_BELT = auto()
    RED_SKULL = auto()
    REGAL_PILLOW = auto()
    SNECKO_SKULL = auto()
    STRAWBERRY = auto()
    STRIKE_DUMMY = auto()
    TINY_MAILBOX = auto()
    VAJRA = auto()
    VENERABLE_TEA_SET = auto()
    WAR_PAINT = auto()
    WHETSTONE = auto()
    BOOK_OF_FIVE_RINGS = auto()

    # Uncommon
    AKABEKO = auto()
    BAG_OF_MARBLES = auto()
    BELLOWS = auto()
    BOOK_REPAIR_KNIFE = auto()
    BOWLER_HAT = auto()
    CANDELABRA = auto()
    ETERNAL_FEATHER = auto()
    FUNERARY_MASK = auto()
    GALACTIC_DUST = auto()
    GOLD_PLATED_CABLES = auto()
    GREMLIN_HORN = auto()
    HORN_CLEAT = auto()
    JOSS_PAPER = auto()
    KUSARIGAMA = auto()
    LETTER_OPENER = auto()
    LUCKY_FYSH = auto()
    MERCURY_HOURGLASS = auto()
    MINIATURE_CANNON = auto()
    NUNCHAKU = auto()
    ORICHALCUM = auto()
    ORNAMENTAL_FAN = auto()
    PANTOGRAPH = auto()
    PAPER_PHROG = auto()
    PARRYING_SHIELD = auto()
    PEAR = auto()
    PEN_NIB = auto()
    PETRIFIED_TOAD = auto()
    PLANISPHERE = auto()
    RED_MASK = auto()
    REGALITE = auto()
    REPTILE_TRINKET = auto()
    RIPPLE_BASIN = auto()
    SELF_FORMING_CLAY = auto()
    SPARKLING_ROUGE = auto()
    STONE_CRACKER = auto()
    TUNING_FORK = auto()
    TINGSHA = auto()
    VAMBRACE = auto()

    # Rare
    ART_OF_WAR = auto()
    BEATING_REMNANT = auto()
    BIG_HAT = auto()
    BOOKMARK = auto()
    CAPTAINS_WHEEL = auto()
    CHANDELIER = auto()
    CHARONS_ASHES = auto()
    DEMON_TONGUE = auto()
    EMOTION_CHIP = auto()
    FROZEN_EGG = auto()
    GAMBLING_CHIP = auto()
    GAME_PIECE = auto()
    GIRYA = auto()
    HELICAL_DART = auto()
    ICE_CREAM = auto()
    INTIMIDATING_HELMET = auto()
    IVORY_TILE = auto()
    KUNAI = auto()
    LASTING_CANDY = auto()
    LIZARD_TAIL = auto()
    LUNAR_PASTRY = auto()
    MANGO = auto()
    MEAT_ON_THE_BONE = auto()
    METRONOME = auto()
    MINI_REGENT = auto()
    MOLTEN_EGG = auto()
    MUMMIFIED_HAND = auto()
    OLD_COIN = auto()
    ORANGE_DOUGH = auto()
    POCKETWATCH = auto()
    POWER_CELL = auto()
    PRAYER_WHEEL = auto()
    RAINBOW_RING = auto()
    RAZOR_TOOTH = auto()
    SHOVEL = auto()
    SHURIKEN = auto()
    STONE_CALENDAR = auto()
    STURDY_CLAMP = auto()
    THE_COURIER = auto()
    TOUGH_BANDAGES = auto()
    TOXIC_EGG = auto()
    TUNGSTEN_ROD = auto()
    UNCEASING_TOP = auto()
    UNSETTLING_LAMP = auto()
    RUINED_HELMET = auto()
    VEXING_PUZZLEBOX = auto()
    WHITE_BEAST_STATUE = auto()
    WHITE_STAR = auto()
    PAPER_KRANE = auto()

    # Shop
    BELT_BUCKLE = auto()
    BREAD = auto()
    BRIMSTONE = auto()
    BURNING_STICKS = auto()
    CAULDRON = auto()
    CHEMICAL_X = auto()
    DINGY_RUG = auto()
    DOLLYS_MIRROR = auto()
    DRAGON_FRUIT = auto()
    GHOST_SEED = auto()
    GNARLED_HAMMER = auto()
    KIFUDA = auto()
    LAVA_LAMP = auto()
    LEES_WAFFLE = auto()
    MEMBERSHIP_CARD = auto()
    MINIATURE_TENT = auto()
    MYSTIC_LIGHTER = auto()
    NINJA_SCROLL = auto()
    ORRERY = auto()
    PUNCH_DAGGER = auto()
    RINGING_TRIANGLE = auto()
    ROYAL_STAMP = auto()
    RUNIC_CAPACITOR = auto()
    SCREAMING_FLAGON = auto()
    SLING_OF_COURAGE = auto()
    THE_ABACUS = auto()
    TOOLBOX = auto()
    UNDYING_SIGIL = auto()
    VITRUVIAN_MINION = auto()
    WING_CHARM = auto()

    # Event / Ancient
    ALCHEMICAL_COFFER = auto()
    ARCANE_SCROLL = auto()
    ARCHAIC_TOOTH = auto()
    ASTROLABE = auto()
    BEAUTIFUL_BRACELET = auto()
    BIG_MUSHROOM = auto()
    BIIIG_HUG = auto()
    BING_BONG = auto()
    BLACK_STAR = auto()
    BLESSED_ANTLER = auto()
    BLOOD_SOAKED_ROSE = auto()
    BONE_TEA = auto()
    BOOMING_CONCH = auto()
    BRILLIANT_SCARF = auto()
    BYRDPIP = auto()
    CALLING_BELL = auto()
    CHOICES_PARADOX = auto()
    CHOSEN_CHEESE = auto()
    CLAWS = auto()
    CROSSBOW = auto()
    CURSED_PEARL = auto()
    DARKSTONE_PERIAPT = auto()
    DAUGHTER_OF_THE_WIND = auto()
    DELICATE_FROND = auto()
    DIAMOND_DIADEM = auto()
    DISTINGUISHED_CAPE = auto()
    DREAM_CATCHER = auto()
    DRIFTWOOD = auto()
    DUSTY_TOME = auto()
    ECTOPLASM = auto()
    ELECTRIC_SHRYMP = auto()
    EMBER_TEA = auto()
    EMPTY_CAGE = auto()
    FAKE_ANCHOR = auto()
    FAKE_BLOOD_VIAL = auto()
    FAKE_HAPPY_FLOWER = auto()
    FAKE_LEES_WAFFLE = auto()
    FAKE_MANGO = auto()
    FAKE_MERCHANTS_RUG = auto()
    FAKE_ORICHALCUM = auto()
    FAKE_SNECKO_EYE = auto()
    FAKE_STRIKE_DUMMY = auto()
    FAKE_VENERABLE_TEA_SET = auto()
    FIDDLE = auto()
    FORGOTTEN_SOUL = auto()
    FRAGRANT_MUSHROOM = auto()
    FRESNEL_LENS = auto()
    FUR_COAT = auto()
    GLASS_EYE = auto()
    GLITTER = auto()
    GOLDEN_COMPASS = auto()
    GOLDEN_PEARL = auto()
    HAND_DRILL = auto()
    HISTORY_COURSE = auto()
    IRON_CLUB = auto()
    JEWELED_MASK = auto()
    JEWELRY_BOX = auto()
    LARGE_CAPSULE = auto()
    LAVA_ROCK = auto()
    LEAD_PAPERWEIGHT = auto()
    LEAFY_POULTICE = auto()
    LOOMING_FRUIT = auto()
    LORDS_PARASOL = auto()
    LOST_COFFER = auto()
    LOST_WISP = auto()
    MASSIVE_SCROLL = auto()
    MAW_BANK = auto()
    MEAT_CLEAVER = auto()
    MR_STRUGGLES = auto()
    MUSIC_BOX = auto()
    NEOWS_TORMENT = auto()
    NEW_LEAF = auto()
    NUTRITIOUS_OYSTER = auto()
    NUTRITIOUS_SOUP = auto()
    PAELS_BLOOD = auto()
    PAELS_CLAW = auto()
    PAELS_EYE = auto()
    PAELS_FLESH = auto()
    PAELS_GROWTH = auto()
    PAELS_HORN = auto()
    PAELS_LEGION = auto()
    PAELS_TEARS = auto()
    PAELS_TOOTH = auto()
    PAELS_WING = auto()
    PANDORAS_BOX = auto()
    PHILOSOPHERS_STONE = auto()
    POLLINOUS_CORE = auto()
    POMANDER = auto()
    PRECARIOUS_SHEARS = auto()
    PRECISE_SCISSORS = auto()
    PRESERVED_FOG = auto()
    PRISMATIC_GEM = auto()
    PUMPKIN_CANDLE = auto()
    RADIANT_PEARL = auto()
    ROYAL_POISON = auto()
    RUNIC_PYRAMID = auto()
    SAI = auto()
    SAND_CASTLE = auto()
    SCROLL_BOXES = auto()
    SEA_GLASS = auto()
    SEAL_OF_GOLD = auto()
    SERE_TALON = auto()
    SIGNET_RING = auto()
    SILVER_CRUCIBLE = auto()
    SMALL_CAPSULE = auto()
    SNECKO_EYE = auto()
    SOZU = auto()
    SPIKED_GAUNTLETS = auto()
    STONE_HUMIDIFIER = auto()
    STORYBOOK = auto()
    SWORD_OF_STONE = auto()
    SWORD_OF_JADE = auto()
    TANXS_WHISTLE = auto()
    TEA_OF_DISCOURTESY = auto()
    THE_BOOT = auto()
    THROWING_AXE = auto()
    TOASTY_MITTENS = auto()
    TOUCH_OF_OROBAS = auto()
    TOY_BOX = auto()
    TRI_BOOMERANG = auto()
    VELVET_CHOKER = auto()
    VERY_HOT_COCOA = auto()
    WAR_HAMMER = auto()
    WHISPERING_EARRING = auto()
    WONGO_CUSTOMER_APPRECIATION_BADGE = auto()
    WONGOS_MYSTERY_TICKET = auto()
    YUMMY_COOKIE = auto()

    # Uncommon (additions)
    SYMBIOTIC_VIRUS = auto()
    TWISTED_FUNNEL = auto()

    # Rare (additions)
    CLOAK_CLASP = auto()

    # Special
    CIRCLET = auto()
    DEPRECATED_RELIC = auto()


class RelicPool(Enum):
    """Which character pool a relic belongs to."""
    SHARED = auto()
    IRONCLAD = auto()
    SILENT = auto()
    DEFECT = auto()
    NECROBINDER = auto()
    REGENT = auto()
    EVENT = auto()
    FALLBACK = auto()
    DEPRECATED = auto()


class RelicInstance:
    """A single relic instance in the player's relic collection.

    Subclass and override hook methods to implement specific relic behavior.
    All hook methods are no-ops by default.
    """

    relic_id: RelicId
    rarity: RelicRarity = RelicRarity.COMMON
    pool: RelicPool = RelicPool.SHARED

    def __init__(self, relic_id: RelicId):
        self.relic_id = relic_id
        self.enabled: bool = True

    # ─── Damage Modification Hooks ──────────────────────────────────────

    def modify_damage_additive(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp,
        card: object | None = None
    ) -> int:
        return 0

    def modify_damage_multiplicative(
        self, owner: Creature, dealer: Creature | None, target: Creature, props: ValueProp,
        card: object | None = None
    ) -> float:
        return 1.0

    def modify_damage_cap(
        self, owner: Creature, dealer: Creature | None, target: Creature, damage: float, props: ValueProp
    ) -> float:
        return float("inf")

    # ─── Block Modification Hooks ───────────────────────────────────────

    def modify_block_additive(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> int:
        return 0

    def modify_block_multiplicative(
        self, owner: Creature, target: Creature, props: ValueProp,
        card_source: object | None = None, card_play: object | None = None,
    ) -> float:
        return 1.0

    # ─── HP Loss Modification ───────────────────────────────────────────

    def modify_hp_lost(
        self, owner: Creature, target: Creature, amount: float,
        dealer: Creature | None, props: ValueProp
    ) -> float:
        return amount

    # ─── Block Clearing ─────────────────────────────────────────────────

    def should_clear_block(self, owner: Creature, creature: Creature) -> bool | None:
        return None

    # ─── Draw / Energy Modification ─────────────────────────────────────

    def modify_hand_draw(self, owner: Creature, draw: int, combat: CombatState) -> int:
        return draw

    def modify_max_energy(self, owner: Creature, energy: int) -> int:
        return energy

    # ─── Card Play Count ────────────────────────────────────────────────

    def modify_card_play_count(self, owner: Creature, count: int, card: object) -> int:
        return count

    def after_modifying_card_play_count(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    # ─── Turn Lifecycle Hooks ───────────────────────────────────────────

    def before_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def after_side_turn_start(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def before_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    def after_turn_end(self, owner: Creature, side: CombatSide, combat: CombatState) -> None:
        pass

    # ─── Card Hooks ─────────────────────────────────────────────────────

    def before_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_played(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_exhausted(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    def after_card_discarded(self, owner: Creature, card: object, combat: CombatState) -> None:
        pass

    # ─── Damage Event Hooks ─────────────────────────────────────────────

    def before_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    def after_damage_received(
        self, owner: Creature, target: Creature, dealer: Creature | None,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    def after_damage_given(
        self, owner: Creature, dealer: Creature, target: Creature,
        damage: int, props: ValueProp, combat: CombatState
    ) -> None:
        pass

    # ─── Attack Command Hooks ───────────────────────────────────────────

    def before_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        pass

    def after_attack(self, owner: Creature, attack: object, combat: CombatState) -> None:
        pass

    # ─── Block Event Hooks ──────────────────────────────────────────────

    def after_block_gained(self, owner: Creature, creature: Creature, amount: int, combat: CombatState) -> None:
        pass

    def after_block_cleared(self, owner: Creature, creature: Creature, combat: CombatState) -> None:
        pass

    # ─── Combat Lifecycle ───────────────────────────────────────────────

    def before_combat_start(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_creature_added_to_combat(
        self,
        owner: Creature,
        creature: Creature,
        combat: CombatState,
    ) -> None:
        pass

    def after_combat_victory(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_combat_end(self, owner: Creature, combat: CombatState) -> None:
        pass

    def after_forge(
        self,
        owner: Creature,
        amount: int,
        forger: Creature,
        source: object | None,
        combat: CombatState,
    ) -> None:
        pass

    def on_stars_gained(self, owner: Creature, amount: int, combat: CombatState) -> None:
        pass

    def on_stars_spent(self, owner: Creature, amount: int, combat: CombatState) -> None:
        pass

    def modify_summon_amount(
        self,
        owner: Creature,
        summoner: Creature,
        amount: int,
        source: object | None,
        combat: CombatState,
    ) -> int:
        return amount

    def after_summon(
        self,
        owner: Creature,
        summoner: Creature,
        amount: int,
        combat: CombatState,
    ) -> None:
        pass

    def after_osty_revived(self, owner: Creature, osty: Creature, combat: CombatState) -> None:
        pass

    # ─── Death Hooks ────────────────────────────────────────────────────

    def after_death(self, owner: Creature, dead: Creature, combat: CombatState) -> None:
        pass

    def should_die_late(self, owner: Creature, combat: CombatState) -> bool | None:
        """Return False to prevent death. None = no opinion."""
        return None

    # ─── Energy Hooks ───────────────────────────────────────────────────

    def after_energy_reset(self, owner: Creature, combat: CombatState) -> None:
        pass

    def should_reset_energy(self, owner: Creature, combat: CombatState) -> bool | None:
        """Return False to prevent energy reset. None = no opinion."""
        return None

    # ─── Shuffle Hooks ──────────────────────────────────────────────────

    def after_shuffle(self, owner: Creature, combat: CombatState) -> None:
        pass

    # ─── Hand Empty Hooks ───────────────────────────────────────────────

    def after_hand_emptied(self, owner: Creature, combat: CombatState) -> None:
        pass

    # ─── Flush Hooks ────────────────────────────────────────────────────

    def should_flush(self, owner: Creature, combat: CombatState) -> bool | None:
        """Return False to prevent hand flush. None = no opinion."""
        return None

    def should_take_extra_turn(self, owner: Creature, combat: CombatState) -> bool | None:
        """Return True if the owner should take an extra turn."""
        return None

    def after_taking_extra_turn(self, owner: Creature, combat: CombatState) -> None:
        pass

    # ─── Play Prevention ────────────────────────────────────────────────

    def should_play(self, owner: Creature, card: object, combat: CombatState) -> bool | None:
        """Return False to prevent card from being played. None = no opinion."""
        return None

    # ─── Room / Map Hooks (non-combat, for simulator completeness) ─────

    def after_room_entered(self, owner: Creature, room_type: object) -> None:
        pass

    def after_obtained(self, owner: Creature) -> None:
        pass

    # ─── Reward Hooks ───────────────────────────────────────────────────

    def modify_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        room: Room | None,
        run_state: RunState,
    ) -> list[Reward]:
        return rewards

    def modify_card_reward_creation_options(
        self,
        owner: Creature,
        options: CardRewardGenerationOptions,
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> CardRewardGenerationOptions:
        return options

    def modify_card_reward_options_late(
        self,
        owner: Creature,
        cards: list[CardInstance],
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> list[CardInstance]:
        return cards

    def modify_card_being_added_to_deck(
        self,
        owner: Creature,
        card: CardInstance,
    ) -> CardInstance:
        return card

    def allow_card_reward_reroll(
        self,
        owner: Creature,
        reward: CardReward,
        room: Room | None,
        run_state: RunState,
    ) -> bool:
        return False

    def should_generate_treasure(self, owner: Creature) -> bool | None:
        return None

    # ─── Merchant Hooks ─────────────────────────────────────────────────

    def modify_merchant_inventory(
        self,
        owner: Creature,
        inventory: ShopInventory,
        run_state: RunState,
    ) -> ShopInventory:
        return inventory

    def modify_merchant_card_creation_results(
        self,
        owner: Creature,
        card: CardInstance,
        *,
        is_colorless: bool,
        run_state: RunState,
    ) -> CardInstance:
        return card

    def modify_merchant_price(
        self,
        owner: Creature,
        price: int,
        *,
        item_kind: str,
        item: object,
        run_state: RunState,
    ) -> int:
        return price

    def should_refill_merchant_entry(
        self,
        owner: Creature,
        *,
        item_kind: str,
        item: object,
        run_state: RunState,
    ) -> bool | None:
        return None

    def on_item_purchased(self, owner: Creature) -> None:
        pass

    # ─── Rest Site Hooks ────────────────────────────────────────────────

    def modify_rest_site_options(
        self,
        owner: Creature,
        options: list[RestSiteOption],
        run_state: RunState,
    ) -> list[RestSiteOption]:
        return options

    def modify_rest_site_heal_amount(
        self,
        owner: Creature,
        amount: int,
        run_state: RunState,
    ) -> int:
        return amount

    def modify_rest_site_heal_rewards(
        self,
        owner: Creature,
        rewards: list[Reward],
        run_state: RunState,
    ) -> list[Reward]:
        return rewards

    def after_rest_site_heal(
        self,
        owner: Creature,
        healed: int,
        run_state: RunState,
    ) -> None:
        pass

    def should_disable_remaining_rest_site_options(
        self,
        owner: Creature,
        chosen_option: object,
        run_state: RunState,
    ) -> bool | None:
        return None

    # ─── Potion Hooks ───────────────────────────────────────────────────

    def should_procure_potion(self, owner: Creature) -> bool | None:
        """Return False to prevent potion procurement. None = no opinion."""
        return None

    def should_gain_gold(self, owner: Creature, amount: int) -> bool | None:
        """Return False to prevent gold gain. None = no opinion."""
        return None

    def should_force_potion_reward(self, owner: Creature) -> bool | None:
        """Return True to force potion reward. None = no opinion."""
        return None

    # ─── X Value ────────────────────────────────────────────────────────

    def modify_x_value(self, owner: Creature, x_value: int, card: object) -> int:
        return x_value

    # ─── Display ────────────────────────────────────────────────────────

    def __repr__(self) -> str:
        return f"{self.relic_id.name}"
