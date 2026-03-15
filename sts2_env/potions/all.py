"""All 63 potions from STS2.

Each potion is registered with its rarity, usage type, and target type,
matching the decompiled MegaCrit.Sts2.Core.Models.Potions source.
"""

from sts2_env.core.enums import PotionRarity, PotionUsageType, PotionTargetType
from sts2_env.potions.base import PotionModel, register_potion

C = PotionRarity.COMMON
U = PotionRarity.UNCOMMON
R = PotionRarity.RARE
E = PotionRarity.EVENT
T = PotionRarity.TOKEN
N = PotionRarity.NONE

CO = PotionUsageType.COMBAT_ONLY
AT = PotionUsageType.ANY_TIME
AU = PotionUsageType.AUTOMATIC

S = PotionTargetType.SELF
AE = PotionTargetType.ANY_ENEMY
ALE = PotionTargetType.ALL_ENEMIES
AP = PotionTargetType.ANY_PLAYER


def _r(pid: str, rarity: PotionRarity, usage: PotionUsageType, target: PotionTargetType) -> None:
    register_potion(PotionModel(potion_id=pid, rarity=rarity, usage_type=usage, target_type=target))


# ── Common (20) ───────────────────────────────────────────────────────
_r("AttackPotion",        C,  CO, S)
_r("BlockPotion",         C,  CO, AP)
_r("BloodPotion",         C,  AT, AP)
_r("ColorlessPotion",     C,  CO, S)
_r("DexterityPotion",     C,  CO, AP)
_r("EnergyPotion",        C,  CO, AP)
_r("ExplosiveAmpoule",    C,  CO, ALE)
_r("FirePotion",          C,  CO, AE)
_r("FlexPotion",          C,  CO, AP)
_r("FocusPotion",         C,  CO, S)
_r("PoisonPotion",        C,  CO, AE)
_r("PotionOfDoom",        C,  CO, AE)
_r("PowerPotion",         C,  CO, S)
_r("SkillPotion",         C,  CO, S)
_r("SpeedPotion",         C,  CO, AP)
_r("StarPotion",          C,  CO, S)
_r("StrengthPotion",      C,  CO, AP)
_r("SwiftPotion",         C,  CO, AP)
_r("VulnerablePotion",    C,  CO, AE)
_r("WeakPotion",          C,  CO, AE)

# ── Uncommon (22) ─────────────────────────────────────────────────────
_r("Ashwater",            U,  CO, S)
_r("BlessingOfTheForge",  U,  CO, S)
_r("BoneBrew",            U,  CO, S)
_r("Clarity",             U,  CO, AP)
_r("CunningPotion",       U,  CO, S)
_r("CureAll",             U,  CO, AP)
_r("Duplicator",          U,  CO, S)
_r("Fortifier",           U,  CO, AP)
_r("FyshOil",             U,  CO, AP)
_r("GamblersBrew",        U,  CO, S)
_r("HeartOfIron",         U,  CO, AP)
_r("KingsCourage",        U,  CO, AP)
_r("LiquidBronze",        U,  CO, AP)
_r("PotionOfBinding",     U,  CO, ALE)
_r("PotionOfCapacity",    U,  CO, S)
_r("PowderedDemise",      U,  CO, AE)
_r("RadiantTincture",     U,  CO, AP)
_r("RegenPotion",         U,  CO, AP)
_r("StableSerum",         U,  CO, AP)
_r("TouchOfInsanity",     U,  CO, S)

# ── Rare (18) ─────────────────────────────────────────────────────────
_r("BeetleJuice",         R,  CO, AE)
_r("BottledPotential",    R,  CO, AP)
_r("CosmicConcoction",    R,  CO, S)
_r("DistilledChaos",      R,  CO, S)
_r("DropletOfPrecognition", R, CO, S)
_r("EntropicBrew",        R,  AT, S)
_r("EssenceOfDarkness",   R,  CO, S)
_r("FairyInABottle",      R,  AU, S)
_r("FruitJuice",          R,  AT, AP)
_r("GhostInAJar",         R,  CO, AP)
_r("GigantificationPotion", R, CO, AP)
_r("LiquidMemories",      R,  CO, S)
_r("LuckyTonic",          R,  CO, AP)
_r("MazalethsGift",       R,  CO, AP)
_r("OrobicAcid",          R,  CO, S)
_r("PotOfGhouls",         R,  CO, S)
_r("ShacklingPotion",     R,  CO, ALE)
_r("ShipInABottle",       R,  CO, AP)
_r("SneckoOil",           R,  CO, AP)
_r("SoldiersStew",        R,  CO, AP)

# ── Event / Token / Deprecated ────────────────────────────────────────
_r("FoulPotion",          E,  AT, S)
_r("GlowwaterPotion",     E,  CO, S)
_r("PotionShapedRock",    T,  CO, AE)
