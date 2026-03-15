# STS2 Relics Reference (Comprehensive)

Total relics: 290 files (288 unique relics + Circlet fallback + DeprecatedRelic + VakuuCardSelector helper)

## Pool Assignments

- **Shared (118):** Akabeko, AmethystAubergine, Anchor, ArtOfWar, BagOfMarbles, BagOfPreparation, BeatingRemnant, Bellows, BeltBuckle, BloodVial, BookOfFiveRings, BowlerHat, Bread, BronzeScales, BurningSticks, Candelabra, CaptainsWheel, Cauldron, CentennialPuzzle, Chandelier, ChemicalX, CloakClasp, DingyRug, DollysMirror, DragonFruit, EternalFeather, FestivePopper, FresnelLens, FrozenEgg, GamblingChip, GamePiece, GhostSeed, Girya, GnarledHammer, Gorget, GremlinHorn, HappyFlower, HornCleat, IceCream, IntimidatingHelmet, JossPaper, JuzuBracelet, Kifuda, Kunai, Kusarigama, Lantern, LastingCandy, LavaLamp, LeesWaffle, LetterOpener, LizardTail, LoomingFruit, LuckyFysh, Mango, MealTicket, MeatOnTheBone, MembershipCard, MercuryHourglass, MiniatureCannon, MiniatureTent, MoltenEgg, MummifiedHand, MysticLighter, Nunchaku, OddlySmoothStone, OldCoin, Orichalcum, OrnamentalFan, Orrery, Pantograph, ParryingShield, Pear, PenNib, Pendulum, Permafrost, PetrifiedToad, Planisphere, Pocketwatch, PotionBelt, PrayerWheel, PunchDagger, RainbowRing, RazorTooth, RedMask, RegalPillow, ReptileTrinket, RingingTriangle, RippleBasin, RoyalStamp, ScreamingFlagon, Shovel, Shuriken, SlingOfCourage, SparklingRouge, StoneCalendar, StoneCracker, Strawberry, StrikeDummy, SturdyClamp, TheAbacus, TheCourier, TinyMailbox, Toolbox, ToxicEgg, TungstenRod, TuningFork, UnceasingTop, UnsettlingLamp, Vajra, Vambrace, VenerableTeaSet, VeryHotCocoa, VexingPuzzlebox, WarPaint, Whetstone, WhiteBeastStatue, WhiteStar, WingCharm
- **Ironclad (8):** Brimstone, BurningBlood, CharonsAshes, DemonTongue, PaperPhrog, RedSkull, RuinedHelmet, SelfFormingClay
- **Silent (8):** HelicalDart, NinjaScroll, PaperKrane, RingOfTheSnake, SneckoSkull, Tingsha, ToughBandages, TwistedFunnel
- **Defect (8):** CrackedCore, DataDisk, EmotionChip, GoldPlatedCables, PowerCell, Metronome, RunicCapacitor, SymbioticVirus
- **Necrobinder (8):** BigHat, BoneFlute, BookRepairKnife, Bookmark, BoundPhylactery, FuneraryMask, IvoryTile, UndyingSigil
- **Regent (8):** DivineRight, FencingManual, GalacticDust, LunarPastry, MiniRegent, OrangeDough, Regalite, VitruvianMinion
- **Event (132):** AlchemicalCoffer, ArcaneScroll, ArchaicTooth, Astrolabe, BeautifulBracelet, BigMushroom, BiiigHug, BingBong, BlackBlood, BlackStar, BlessedAntler, BloodSoakedRose, BoneTea, BoomingConch, BrilliantScarf, Byrdpip, CallingBell, ChoicesParadox, ChosenCheese, Claws, Crossbow, CursedPearl, DarkstonePeriapt, DaughterOfTheWind, DelicateFrond, DiamondDiadem, DistinguishedCape, DivineDestiny, DreamCatcher, Driftwood, DustyTome, Ectoplasm, ElectricShrymp, EmberTea, EmptyCage, FakeAnchor, FakeBloodVial, FakeHappyFlower, FakeLeesWaffle, FakeMango, FakeMerchantsRug, FakeOrichalcum, FakeSneckoEye, FakeStrikeDummy, FakeVenerableTeaSet, Fiddle, ForgottenSoul, FragrantMushroom, FurCoat, GlassEye, Glitter, GoldenCompass, GoldenPearl, HandDrill, HistoryCourse, InfusedCore, IronClub, JeweledMask, JewelryBox, LargeCapsule, LastingCandy(also shared), LavaRock, LeadPaperweight, LeafyPoultice, LordsParasol, LostCoffer, LostWisp, MassiveScroll, MawBank, MeatCleaver, MrStruggles, MusicBox, NeowsTorment, NewLeaf, NutritiousOyster, NutritiousSoup, PaelsBlood, PaelsClaw, PaelsEye, PaelsFlesh, PaelsGrowth, PaelsHorn, PaelsLegion, PaelsTears, PaelsTooth, PaelsWing, PandorasBox, PhilosophersStone, PhylacteryUnbound, PollinousCore, Pomander, PrecariousShears, PreciseScissors, PreservedFog, PrismaticGem, PumpkinCandle, RadiantPearl, RazorTooth(also shared), RingOfTheDrake, RoyalPoison, RunicPyramid, Sai, SandCastle, ScrollBoxes, SeaGlass, SealOfGold, SereTalon, SignetRing, SilverCrucible, SmallCapsule, SneckoEye, Sozu, SparklingRouge(also shared), SpikedGauntlets, StoneHumidifier, Storybook, SwordOfJade, SwordOfStone, TanxsWhistle, TeaOfDiscourtesy, TheBoot, ThrowingAxe, ToastyMittens, TouchOfOrobas, ToyBox, TriBoomerang, VelvetChoker, WarHammer, WhisperingEarring, WongoCustomerAppreciationBadge, WongosMysteryTicket, YummyCookie
- **Fallback:** Circlet
- **Deprecated:** DeprecatedRelic

---

## 1. Starter (10)

### BurningBlood
- ID: BURNING_BLOOD
- Rarity: Starter
- Pool: ironclad
- Vars: {Heal: 6}
- Hooks: [AfterCombatVictory]
- Logic:
  - AfterCombatVictory: if owner not dead, heal owner for 6
- Internal State: none

### BlackBlood
- ID: BLACK_BLOOD
- Rarity: Starter
- Pool: event (upgraded BurningBlood)
- Vars: {Heal: 12}
- Hooks: [AfterCombatVictory]
- Logic:
  - AfterCombatVictory: if owner not dead, heal owner for 12
- Internal State: none

### RingOfTheSnake
- ID: RING_OF_THE_SNAKE
- Rarity: Starter
- Pool: silent
- Vars: {Cards: 2}
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: if round 1, draw +2 cards
- Internal State: none

### RingOfTheDrake
- ID: RING_OF_THE_DRAKE
- Rarity: Starter
- Pool: event (upgraded RingOfTheSnake)
- Vars: {Cards: 2, Turns: 3}
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: if round <= 3, draw +2 cards
- Internal State: none

### CrackedCore
- ID: CRACKED_CORE
- Rarity: Starter
- Pool: defect
- Vars: {Lightning: 1}
- Hooks: [BeforeSideTurnStart]
- Logic:
  - BeforeSideTurnStart: if player side and round 1, channel 1 Lightning orb
- Internal State: none

### InfusedCore
- ID: INFUSED_CORE
- Rarity: Starter
- Pool: event (upgraded CrackedCore)
- Vars: {Lightning: 3}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, channel 3 Lightning orbs
- Internal State: none

### BoundPhylactery
- ID: BOUND_PHYLACTERY
- Rarity: Starter
- Pool: necrobinder
- Vars: {Summon: 1}
- Hooks: [BeforeCombatStart, AfterEnergyResetLate]
- Logic:
  - BeforeCombatStart: summon 1 Osty pet
  - AfterEnergyResetLate: if not round 1, summon 1 Osty pet
- Internal State: none
- SpawnsPets: true

### PhylacteryUnbound
- ID: PHYLACTERY_UNBOUND
- Rarity: Starter
- Pool: event (upgraded BoundPhylactery)
- Vars: {StartOfCombat: 5, StartOfTurn: 2}
- Hooks: [BeforeCombatStart, AfterSideTurnStart]
- Logic:
  - BeforeCombatStart: summon 5 Osty
  - AfterSideTurnStart: if player side, summon 2 Osty
- Internal State: none
- SpawnsPets: true

### DivineRight
- ID: DIVINE_RIGHT
- Rarity: Starter
- Pool: regent
- Vars: {Stars: 3}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, gain 3 stars
- Internal State: none

### DivineDestiny
- ID: DIVINE_DESTINY
- Rarity: Starter
- Pool: event (upgraded DivineRight)
- Vars: {Stars: 6}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, gain 6 stars
- Internal State: none

---

## 2. Common

### AmethystAubergine
- ID: AMETHYST_AUBERGINE
- Rarity: Common
- Pool: shared
- Vars: {Gold: 10}
- Hooks: [IsAllowed, TryModifyRewards, AfterModifyingRewards]
- Logic:
  - TryModifyRewards: if combat room (not final boss), add gold reward of 10
- Internal State: none

### Anchor
- ID: ANCHOR
- Rarity: Common
- Pool: shared
- Vars: {Block: 10}
- Hooks: [BeforeCombatStart]
- Logic:
  - BeforeCombatStart: gain 10 block (unpowered)
- Internal State: none

### BagOfPreparation
- ID: BAG_OF_PREPARATION
- Rarity: Common
- Pool: shared
- Vars: {Cards: 2}
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: if round 1, draw +2 cards
- Internal State: none

### BloodVial
- ID: BLOOD_VIAL
- Rarity: Common
- Pool: shared
- Vars: {Heal: 2}
- Hooks: [AfterPlayerTurnStartLate]
- Logic:
  - AfterPlayerTurnStartLate: if round 1, heal 2
- Internal State: none

### BoneFlute
- ID: BONE_FLUTE
- Rarity: Common
- Pool: necrobinder
- Vars: {Block: 2}
- Hooks: [AfterAttack]
- Logic:
  - AfterAttack: if attacker is Osty owned by player, gain 2 block (unpowered)
- Internal State: none

### BronzeScales
- ID: BRONZE_SCALES
- Rarity: Common
- Pool: shared
- Vars: {ThornsPower: 3}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, apply 3 Thorns to owner
- Internal State: none

### CentennialPuzzle
- ID: CENTENNIAL_PUZZLE
- Rarity: Common
- Pool: shared
- Vars: {Cards: 3}
- Hooks: [AfterDamageReceived, AfterCombatEnd]
- Logic:
  - AfterDamageReceived: if owner takes unblocked damage and not used this combat, draw 3 cards
- Internal State: _usedThisCombat (bool, reset on combat end)

### DataDisk
- ID: DATA_DISK
- Rarity: Common
- Pool: defect
- Vars: {FocusPower: 1}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, apply 1 Focus to owner
- Internal State: none

### FencingManual
- ID: FENCING_MANUAL
- Rarity: Common
- Pool: regent
- Vars: {Forge: 10}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, forge 10
- Internal State: none

### FestivePopper
- ID: FESTIVE_POPPER
- Rarity: Common
- Pool: shared
- Vars: {Damage: 9}
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: if round 1, deal 9 unpowered damage to all enemies
- Internal State: none

### Gorget
- ID: GORGET
- Rarity: Common
- Pool: shared
- Vars: {PlatingPower: 4}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, apply 4 Plating to owner
- Internal State: none

### HappyFlower
- ID: HAPPY_FLOWER
- Rarity: Common
- Pool: shared
- Vars: {Energy: 1, Turns: 3}
- Hooks: [AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - AfterSideTurnStart: increment turn counter; every 3 turns gain 1 energy
- Internal State: _turnsSeen [SavedProperty], _isActivating
- Uses [SavedProperty]: yes (TurnsSeen)

### JuzuBracelet
- ID: JUZU_BRACELET
- Rarity: Common
- Pool: shared
- Vars: none
- Hooks: [IsAllowed, ModifyUnknownMapPointRoomTypes]
- Logic:
  - ModifyUnknownMapPointRoomTypes: removes Monster from unknown room types
- Internal State: none

### Lantern
- ID: LANTERN
- Rarity: Common
- Pool: shared
- Vars: {Energy: 1}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, gain 1 energy
- Internal State: none

### MealTicket
- ID: MEAL_TICKET
- Rarity: Common
- Pool: shared
- Vars: {Heal: 15}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if merchant room and owner not dead, heal 15
- Internal State: none

### OddlySmoothStone
- ID: ODDLY_SMOOTH_STONE
- Rarity: Common
- Pool: shared
- Vars: {DexterityPower: 1}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, apply 1 Dexterity to owner
- Internal State: none

### Pendulum
- ID: PENDULUM
- Rarity: Common
- Pool: shared
- Vars: none
- Hooks: [AfterShuffle]
- Logic:
  - AfterShuffle: draw 1 card
- Internal State: none

### Permafrost
- ID: PERMAFROST
- Rarity: Common
- Pool: shared
- Vars: {Block: 6}
- Hooks: [AfterRoomEntered, AfterCardPlayed]
- Logic:
  - AfterCardPlayed: if owner plays a Power card and not activated this combat, gain 6 block (unpowered)
- Internal State: _activatedThisCombat (bool)

### PotionBelt
- ID: POTION_BELT
- Rarity: Common
- Pool: shared
- Vars: {PotionSlots: 2}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 2 max potion slots
- Internal State: none

### RedSkull
- ID: RED_SKULL
- Rarity: Common
- Pool: ironclad
- Vars: {HpThreshold: 50, StrengthPower: 3}
- Hooks: [AfterRoomEntered, AfterCombatEnd, AfterCurrentHpChanged]
- Logic:
  - When HP <= 50% max HP: apply 3 Strength; when HP > 50%: remove 3 Strength
- Internal State: _strengthApplied (bool)

### RegalPillow
- ID: REGAL_PILLOW
- Rarity: Common
- Pool: shared
- Vars: {Heal: 15}
- Hooks: [ModifyRestSiteHealAmount, AfterRestSiteHeal, ModifyExtraRestSiteHealText, AfterRoomEntered]
- Logic:
  - ModifyRestSiteHealAmount: add 15 to rest site heal amount
- Internal State: none

### SneckoSkull
- ID: SNECKO_SKULL
- Rarity: Common
- Pool: silent
- Vars: {PoisonPower: 1}
- Hooks: [ModifyPowerAmountGiven, AfterModifyingPowerAmountGiven]
- Logic:
  - ModifyPowerAmountGiven: if applying Poison and owner is giver, add +1 to amount
- Internal State: none

### Strawberry
- ID: STRAWBERRY
- Rarity: Common
- Pool: shared
- Vars: {MaxHp: 7}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 7 max HP
- Internal State: none

### StrikeDummy
- ID: STRIKE_DUMMY
- Rarity: Common
- Pool: shared
- Vars: {ExtraDamage: 3}
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: if powered attack from Strike-tagged card by owner, add +3 damage
- Internal State: none

### TinyMailbox
- ID: TINY_MAILBOX
- Rarity: Common
- Pool: shared
- Vars: none
- Hooks: [TryModifyRestSiteHealRewards, ModifyExtraRestSiteHealText]
- Logic:
  - TryModifyRestSiteHealRewards: add a potion reward at rest site
- Internal State: none

### Vajra
- ID: VAJRA
- Rarity: Common
- Pool: shared
- Vars: {StrengthPower: 1}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, apply 1 Strength to owner
- Internal State: none

### VenerableTeaSet
- ID: VENERABLE_TEA_SET
- Rarity: Common
- Pool: shared
- Vars: {Energy: 2}
- Hooks: [AfterRoomEntered, AfterEnergyReset]
- Logic:
  - AfterRoomEntered: if rest site, set flag to gain energy next combat
  - AfterEnergyReset: if flag set, gain 2 energy and reset flag
- Internal State: _gainEnergyInNextCombat [SavedProperty]
- Uses [SavedProperty]: yes

### WarPaint
- ID: WAR_PAINT
- Rarity: Common
- Pool: shared
- Vars: {Cards: 2}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: upgrade 2 random Skill cards in deck
- Internal State: none

### Whetstone
- ID: WHETSTONE
- Rarity: Common
- Pool: shared
- Vars: {Cards: 2}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: upgrade 2 random Attack cards in deck
- Internal State: none

### BookOfFiveRings
- ID: BOOK_OF_FIVE_RINGS
- Rarity: Common
- Pool: shared
- Vars: {Cards: 5, Heal: 15}
- Hooks: [IsAllowed, AfterCardChangedPiles]
- Logic:
  - AfterCardChangedPiles: when card added to deck, increment counter; every 5 cards added, heal 15
- Internal State: _cardsAdded [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

---

## 3. Uncommon

### Akabeko
- ID: AKABEKO
- Rarity: Uncommon
- Pool: shared
- Vars: {VigorPower: 8}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, apply 8 Vigor to owner
- Internal State: none

### BagOfMarbles
- ID: BAG_OF_MARBLES
- Rarity: Uncommon
- Pool: shared
- Vars: {VulnerablePower: 1}
- Hooks: [BeforeSideTurnStart]
- Logic:
  - BeforeSideTurnStart: if player side and round 1, apply 1 Vulnerable to all hittable enemies
- Internal State: none

### Bellows
- ID: BELLOWS
- Rarity: Uncommon
- Pool: shared
- Vars: none
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: if round 1, upgrade all cards in hand
- Internal State: none

### BookRepairKnife
- ID: BOOK_REPAIR_KNIFE
- Rarity: Uncommon
- Pool: necrobinder
- Vars: {Heal: 3}
- Hooks: [AfterDiedToDoom]
- Logic:
  - AfterDiedToDoom: heal 3 * number of enemies that died to Doom
- Internal State: none

### BowlerHat
- ID: BOWLER_HAT
- Rarity: Uncommon
- Pool: shared
- Vars: none (uses constant 0.2 multiplier)
- Hooks: [ShouldGainGold, AfterGoldGained]
- Logic:
  - ShouldGainGold/AfterGoldGained: gain 20% bonus gold on each gold gain
- Internal State: _pendingBonusGold, _isApplyingBonus

### Candelabra
- ID: CANDELABRA
- Rarity: Uncommon
- Pool: shared
- Vars: {Energy: 2}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 2, gain 2 energy
- Internal State: none

### EternalFeather
- ID: ETERNAL_FEATHER
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 5, Heal: 3}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if rest site, heal 3 * (deck size / 5)
- Internal State: none

### FuneraryMask
- ID: FUNERARY_MASK
- Rarity: Uncommon
- Pool: necrobinder
- Vars: {Cards: 3}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, create 3 Soul cards and shuffle into draw pile
- Internal State: none

### GalacticDust
- ID: GALACTIC_DUST
- Rarity: Uncommon
- Pool: regent
- Vars: {Stars: 10, Block: 10}
- Hooks: [AfterStarsSpent]
- Logic:
  - AfterStarsSpent: track stars spent; every 10 stars, gain 10 block (unpowered)
- Internal State: _starsSpent [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### GoldPlatedCables
- ID: GOLD_PLATED_CABLES
- Rarity: Uncommon
- Pool: defect
- Vars: none
- Hooks: [ModifyOrbPassiveTriggerCounts, AfterModifyingOrbPassiveTriggerCount]
- Logic:
  - ModifyOrbPassiveTriggerCounts: first orb in queue gets +1 passive trigger
- Internal State: none

### GremlinHorn
- ID: GREMLIN_HORN
- Rarity: Uncommon
- Pool: shared
- Vars: {Energy: 1, Cards: 1}
- Hooks: [AfterDeath]
- Logic:
  - AfterDeath: when enemy dies, gain 1 energy and draw 1 card
- Internal State: none

### HornCleat
- ID: HORN_CLEAT
- Rarity: Uncommon
- Pool: shared
- Vars: {Block: 14}
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: if round 2 and owner, gain 14 block (unpowered)
- Internal State: none

### JossPaper
- ID: JOSS_PAPER
- Rarity: Uncommon
- Pool: shared
- Vars: {ExhaustAmount: 5, Cards: 1}
- Hooks: [AfterCardExhausted, AfterTurnEnd]
- Logic:
  - AfterCardExhausted: count exhausted cards (ethereal counted separately at end of turn); every 5, draw 1
- Internal State: _cardsExhausted [SavedProperty], _etherealCount, _isActivating
- Uses [SavedProperty]: yes

### Kusarigama
- ID: KUSARIGAMA
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 3, Damage: 6}
- Hooks: [BeforeCombatStart, AfterTurnEnd, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: count attacks this turn; every 3 attacks, deal 6 unpowered damage to random enemy
- Internal State: _attacksPlayedThisTurn, _isActivating

### LetterOpener
- ID: LETTER_OPENER
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 3, Damage: 5}
- Hooks: [AfterSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: count skills this turn; every 3, deal 5 unpowered damage to all enemies
- Internal State: _skillsPlayedThisTurn, _isActivating

### LuckyFysh
- ID: LUCKY_FYSH
- Rarity: Uncommon
- Pool: shared
- Vars: {Gold: 15}
- Hooks: [AfterCardChangedPiles]
- Logic:
  - AfterCardChangedPiles: when card added to deck, gain 15 gold
- Internal State: none

### MercuryHourglass
- ID: MERCURY_HOURGLASS
- Rarity: Uncommon
- Pool: shared
- Vars: {Damage: 3}
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: deal 3 unpowered damage to all enemies
- Internal State: none

### MiniatureCannon
- ID: MINIATURE_CANNON
- Rarity: Uncommon
- Pool: shared
- Vars: {ExtraDamage: 3}
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: if powered attack from an upgraded card by owner, add +3 damage
- Internal State: none

### Nunchaku
- ID: NUNCHAKU
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 10, Energy: 1}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: count attacks played; every 10, gain 1 energy
- Internal State: _attacksPlayed [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### Orichalcum
- ID: ORICHALCUM
- Rarity: Uncommon
- Pool: shared
- Vars: {Block: 6}
- Hooks: [BeforeTurnEndVeryEarly, BeforeTurnEnd, BeforeSideTurnStart]
- Logic:
  - BeforeTurnEnd: if owner has 0 block at end of turn, gain 6 block (unpowered)
- Internal State: _shouldTrigger (bool)

### OrnamentalFan
- ID: ORNAMENTAL_FAN
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 3, Block: 4}
- Hooks: [BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: count attacks this turn; every 3, gain 4 block (unpowered)
- Internal State: _attacksPlayedThisTurn, _isActivating

### Pantograph
- ID: PANTOGRAPH
- Rarity: Uncommon
- Pool: shared
- Vars: {Heal: 25}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if boss room, heal 25
- Internal State: none

### PaperPhrog
- ID: PAPER_PHROG
- Rarity: Uncommon
- Pool: ironclad
- Vars: none
- Hooks: [ModifyVulnerableMultiplier]
- Logic:
  - ModifyVulnerableMultiplier: enemies take +25% more damage from Vulnerable (additive)
- Internal State: none

### ParryingShield
- ID: PARRYING_SHIELD
- Rarity: Uncommon
- Pool: shared
- Vars: {Block: 10, Damage: 6}
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: if owner has >= 10 block, deal 6 unpowered damage to random enemy
- Internal State: none

### Pear
- ID: PEAR
- Rarity: Uncommon
- Pool: shared
- Vars: {MaxHp: 10}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 10 max HP
- Internal State: none

### PenNib
- ID: PEN_NIB
- Rarity: Uncommon
- Pool: shared
- Vars: none (threshold constant: 10)
- Hooks: [BeforeCardPlayed, AfterCardPlayed, ModifyDamageMultiplicative]
- Logic:
  - Every 10th attack played: double damage of that attack
- Internal State: _attacksPlayed [SavedProperty], _attackToDouble, _isActivating
- Uses [SavedProperty]: yes

### PetrifiedToad
- ID: PETRIFIED_TOAD
- Rarity: Uncommon
- Pool: shared
- Vars: none
- Hooks: [BeforeCombatStartLate]
- Logic:
  - BeforeCombatStartLate: procure a PotionShapedRock potion
- Internal State: none

### Planisphere
- ID: PLANISPHERE
- Rarity: Uncommon
- Pool: shared
- Vars: {Heal: 4}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if unknown map point room, heal 4
- Internal State: none

### RedMask
- ID: RED_MASK
- Rarity: Uncommon
- Pool: shared
- Vars: {WeakPower: 1}
- Hooks: [BeforeSideTurnStart]
- Logic:
  - BeforeSideTurnStart: if player side and round 1, apply 1 Weak to all enemies
- Internal State: none

### Regalite
- ID: REGALITE
- Rarity: Uncommon
- Pool: regent
- Vars: {Block: 2}
- Hooks: [AfterCardEnteredCombat]
- Logic:
  - AfterCardEnteredCombat: if card is colorless and owned by player, gain 2 block (unpowered)
- Internal State: none

### ReptileTrinket
- ID: REPTILE_TRINKET
- Rarity: Uncommon
- Pool: shared
- Vars: {StrengthPower: 3}
- Hooks: [AfterPotionUsed]
- Logic:
  - AfterPotionUsed: if in combat, apply 3 temporary Strength (via ReptileTrinketPower)
- Internal State: none

### RippleBasin
- ID: RIPPLE_BASIN
- Rarity: Uncommon
- Pool: shared
- Vars: {Block: 4}
- Hooks: [BeforeTurnEnd, AfterCardPlayed, BeforeSideTurnStart, AfterCombatEnd]
- Logic:
  - BeforeTurnEnd: if no attacks played this turn, gain 4 block (unpowered)
- Internal State: none (checks combat history)

### SelfFormingClay
- ID: SELF_FORMING_CLAY
- Rarity: Uncommon
- Pool: ironclad
- Vars: {BlockNextTurn: 3}
- Hooks: [AfterDamageReceived]
- Logic:
  - AfterDamageReceived: if owner takes unblocked damage, apply SelfFormingClayPower (gives 3 block next turn)
- Internal State: none

### SparklingRouge
- ID: SPARKLING_ROUGE
- Rarity: Uncommon
- Pool: shared
- Vars: {StrengthPower: 1, DexterityPower: 1}
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: if round 3 and owner, gain 1 Strength and 1 Dexterity
- Internal State: none

### StoneCracker
- ID: STONE_CRACKER
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 3}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if boss room, upgrade 3 random upgradable cards in draw pile
- Internal State: none

### TuningFork
- ID: TUNING_FORK
- Rarity: Uncommon
- Pool: shared
- Vars: {Cards: 10, Block: 7}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: count skills played; every 10, gain 7 block (unpowered)
- Internal State: _skillsPlayed [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### Tingsha
- ID: TINGSHA
- Rarity: Uncommon
- Pool: silent
- Vars: {Damage: 3}
- Hooks: [AfterCardDiscarded]
- Logic:
  - AfterCardDiscarded: if player's turn, deal 3 unpowered damage to random enemy
- Internal State: none

### Vambrace
- ID: VAMBRACE
- Rarity: Uncommon
- Pool: shared
- Vars: none
- Hooks: [BeforeCombatStart, ModifyBlockMultiplicative, AfterModifyingBlockAmount, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - First card that grants block in combat: double that block (once per combat)
- Internal State: _triggeringCard, _blockGainedThisCombat

---

## 4. Rare

### ArtOfWar
- ID: ART_OF_WAR
- Rarity: Rare
- Pool: shared
- Vars: {Energy: 1}
- Hooks: [AfterCardPlayed, AfterTurnEnd, AfterEnergyReset, AfterCombatEnd]
- Logic:
  - AfterEnergyReset: if no attacks played last turn (after round 1), gain 1 energy
- Internal State: _anyAttacksPlayedLastTurn, _anyAttacksPlayedThisTurn

### BeatingRemnant
- ID: BEATING_REMNANT
- Rarity: Rare
- Pool: shared
- Vars: {MaxHpLoss: 20}
- Hooks: [ModifyHpLostAfterOsty, AfterDamageReceived, BeforeSideTurnStart]
- Logic:
  - ModifyHpLostAfterOsty: cap damage taken per turn to 20
- Internal State: _damageReceivedThisTurn (decimal, resets each player turn)

### BigHat
- ID: BIG_HAT
- Rarity: Rare
- Pool: necrobinder
- Vars: {Cards: 2}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, generate 2 random Ethereal cards into hand
- Internal State: none

### Bookmark
- ID: BOOKMARK
- Rarity: Rare
- Pool: necrobinder
- Vars: none
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: pick a random Retained card with energy cost > 0, reduce its cost by 1 until played
- Internal State: none

### CaptainsWheel
- ID: CAPTAINS_WHEEL
- Rarity: Rare
- Pool: shared
- Vars: {Block: 18}
- Hooks: [AfterBlockCleared]
- Logic:
  - AfterBlockCleared: if round 3 and owner, gain 18 block (unpowered)
- Internal State: none

### Chandelier
- ID: CHANDELIER
- Rarity: Rare
- Pool: shared
- Vars: {Energy: 3}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 3, gain 3 energy
- Internal State: none

### CharonsAshes
- ID: CHARONS_ASHES
- Rarity: Rare
- Pool: ironclad
- Vars: {Damage: 3}
- Hooks: [AfterCardExhausted]
- Logic:
  - AfterCardExhausted: deal 3 unpowered damage to all enemies
- Internal State: none

### DemonTongue
- ID: DEMON_TONGUE
- Rarity: Rare
- Pool: ironclad
- Vars: none
- Hooks: [AfterDamageReceived, BeforeSideTurnStart]
- Logic:
  - AfterDamageReceived: first time owner takes unblocked damage on their turn, heal that amount
- Internal State: _triggeredThisTurn (bool, resets each turn)

### EmotionChip
- ID: EMOTION_CHIP
- Rarity: Rare
- Pool: defect
- Vars: none
- Hooks: [AfterDamageReceived, AfterPlayerTurnStart, AfterCombatEnd]
- Logic:
  - AfterPlayerTurnStart: if lost HP previous turn, trigger all orb passives
- Internal State: none (checks combat history)

### FrozenEgg
- ID: FROZEN_EGG
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck]
- Logic:
  - Auto-upgrade Power cards when obtained/offered
- Internal State: none

### GamblingChip
- ID: GAMBLING_CHIP
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: if round 1, discard any cards from hand and draw that many
- Internal State: none

### GamePiece
- ID: GAME_PIECE
- Rarity: Rare
- Pool: shared
- Vars: {Cards: 1}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Power card played, draw 1 card
- Internal State: none

### Girya
- ID: GIRYA
- Rarity: Rare
- Pool: shared
- Vars: none (max 3 lifts)
- Hooks: [AfterRoomEntered, TryModifyRestSiteOptions]
- Logic:
  - AfterRoomEntered: if combat room, apply Strength = TimesLifted
  - TryModifyRestSiteOptions: add Lift option (max 3 times)
- Internal State: _timesLifted [SavedProperty]
- Uses [SavedProperty]: yes

### HelicalDart
- ID: HELICAL_DART
- Rarity: Rare
- Pool: silent
- Vars: {DexterityPower: 1}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Shiv played, apply HelicalDartPower (1 Dexterity via power)
- Internal State: none

### IceCream
- ID: ICE_CREAM
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [ShouldPlayerResetEnergy]
- Logic:
  - ShouldPlayerResetEnergy: after round 1, prevent energy reset (conserve energy between turns)
- Internal State: none

### IntimidatingHelmet
- ID: INTIMIDATING_HELMET
- Rarity: Rare
- Pool: shared
- Vars: {Block: 4, Energy: 2}
- Hooks: [BeforeCardPlayed]
- Logic:
  - BeforeCardPlayed: if card costs >= 2 energy, gain 4 block (unpowered)
- Internal State: none

### IvoryTile
- ID: IVORY_TILE
- Rarity: Rare
- Pool: necrobinder
- Vars: {Energy: 1, EnergyThreshold: 3}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: if card spent >= 3 energy, gain 1 energy
- Internal State: none

### Kunai
- ID: KUNAI
- Rarity: Rare
- Pool: shared
- Vars: {Cards: 3, DexterityPower: 1}
- Hooks: [BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: count attacks this turn; every 3, gain 1 Dexterity
- Internal State: _attacksPlayedThisTurn, _isActivating

### LastingCandy
- ID: LASTING_CANDY
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyCardRewardOptions, AfterCombatEnd]
- Logic:
  - Every 2nd combat: add an extra Power card to card reward options
- Internal State: _combatsSeen [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### LizardTail
- ID: LIZARD_TAIL
- Rarity: Rare
- Pool: shared
- Vars: {Heal: 50}
- Hooks: [ShouldDieLate, AfterPreventingDeath]
- Logic:
  - ShouldDieLate: prevent owner death once
  - AfterPreventingDeath: heal 50% of max HP
- Internal State: _wasUsed [SavedProperty]
- Uses [SavedProperty]: yes

### LunarPastry
- ID: LUNAR_PASTRY
- Rarity: Rare
- Pool: regent
- Vars: {Stars: 1}
- Hooks: [AfterTurnEnd]
- Logic:
  - AfterTurnEnd: if player side, gain 1 star
- Internal State: none

### Mango
- ID: MANGO
- Rarity: Rare
- Pool: shared
- Vars: {MaxHp: 14}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 14 max HP
- Internal State: none

### MeatOnTheBone
- ID: MEAT_ON_THE_BONE
- Rarity: Rare
- Pool: shared
- Vars: {HpThreshold: 50, Heal: 12}
- Hooks: [BeforeCombatStart, AfterCurrentHpChanged, AfterCombatVictoryEarly]
- Logic:
  - AfterCombatVictoryEarly: if HP <= 50% max, heal 12
- Internal State: none (checks HP dynamically)

### Metronome
- ID: METRONOME
- Rarity: Rare
- Pool: defect
- Vars: {Damage: 30, OrbCount: 7}
- Hooks: [AfterRoomEntered, AfterOrbChanneled, AfterCombatEnd]
- Logic:
  - AfterOrbChanneled: count orbs channeled; at 7, deal 30 unpowered damage to all enemies
- Internal State: _orbsChanneled, _isActivating

### MiniRegent
- ID: MINI_REGENT
- Rarity: Rare
- Pool: regent
- Vars: {StrengthPower: 1}
- Hooks: [AfterStarsSpent, BeforeSideTurnStart, AfterCombatEnd]
- Logic:
  - AfterStarsSpent: first time stars spent each turn, gain 1 Strength
- Internal State: _usedThisTurn (bool)

### MoltenEgg
- ID: MOLTEN_EGG
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck]
- Logic:
  - Auto-upgrade Attack cards when obtained/offered
- Internal State: none

### MummifiedHand
- ID: MUMMIFIED_HAND
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Power played, reduce cost of random card in hand to 0 this turn
- Internal State: none

### OldCoin
- ID: OLD_COIN
- Rarity: Rare
- Pool: shared
- Vars: {Gold: 300}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 300 gold
- Internal State: none

### OrangeDough
- ID: ORANGE_DOUGH
- Rarity: Rare
- Pool: regent
- Vars: {Cards: 2}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, generate 2 random colorless cards into hand
- Internal State: none

### Pocketwatch
- ID: POCKETWATCH
- Rarity: Rare
- Pool: shared
- Vars: {CardThreshold: 3, Cards: 3}
- Hooks: [AfterCardPlayed, ModifyHandDraw, BeforeSideTurnStart, AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - ModifyHandDraw: if <= 3 cards played last turn, draw +3 cards this turn
- Internal State: _cardsPlayedThisTurn, _cardsPlayedLastTurn

### PowerCell
- ID: POWER_CELL
- Rarity: Rare
- Pool: defect
- Vars: {Cards: 2}
- Hooks: [BeforeSideTurnStart]
- Logic:
  - BeforeSideTurnStart: if player side and round 1, move 2 random 0-cost cards from draw pile to hand
- Internal State: none

### PrayerWheel
- ID: PRAYER_WHEEL
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyRewards]
- Logic:
  - TryModifyRewards: add extra card reward after monster combats
- Internal State: none

### RainbowRing
- ID: RAINBOW_RING
- Rarity: Rare
- Pool: shared
- Vars: {StrengthPower: 1, DexterityPower: 1}
- Hooks: [BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: if played Attack + Skill + Power this turn (once per turn), gain 1 Strength + 1 Dexterity
- Internal State: _attacksPlayedThisTurn, _skillsPlayedThisTurn, _powersPlayedThisTurn, _activationCountThisTurn

### RazorTooth
- ID: RAZOR_TOOTH
- Rarity: Rare
- Pool: shared/event
- Vars: none
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Attack or Skill played that is upgradable, upgrade it in combat
- Internal State: none

### Shovel
- ID: SHOVEL
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyRestSiteOptions]
- Logic:
  - TryModifyRestSiteOptions: add Dig rest site option
- Internal State: none

### Shuriken
- ID: SHURIKEN
- Rarity: Rare
- Pool: shared
- Vars: {Cards: 3, StrengthPower: 1}
- Hooks: [BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - AfterCardPlayed: count attacks this turn; every 3, gain 1 Strength
- Internal State: _attacksPlayedThisTurn, _isActivating

### StoneCalendar
- ID: STONE_CALENDAR
- Rarity: Rare
- Pool: shared
- Vars: {Damage: 52, DamageTurn: 7}
- Hooks: [AfterSideTurnStart, BeforeTurnEnd, AfterCombatEnd, AfterRoomEntered]
- Logic:
  - BeforeTurnEnd: on turn 7, deal 52 unpowered damage to all enemies
- Internal State: _isActivating

### SturdyClamp
- ID: STURDY_CLAMP
- Rarity: Rare
- Pool: shared
- Vars: {Block: 10}
- Hooks: [ShouldClearBlock, AfterPreventingBlockClear]
- Logic:
  - ShouldClearBlock: prevent block clear; retain up to 10 block between turns
- Internal State: none

### TheCourier
- ID: THE_COURIER
- Rarity: Rare
- Pool: shared
- Vars: {Discount: 20}
- Hooks: [ModifyMerchantPrice, ShouldRefillMerchantEntry]
- Logic:
  - ModifyMerchantPrice: 20% discount at merchants
  - ShouldRefillMerchantEntry: merchant entries refill
- Internal State: none

### ToughBandages
- ID: TOUGH_BANDAGES
- Rarity: Rare
- Pool: silent
- Vars: {Block: 3}
- Hooks: [AfterCardDiscarded]
- Logic:
  - AfterCardDiscarded: gain 3 block (unpowered) when card discarded on player turn
- Internal State: none

### ToxicEgg
- ID: TOXIC_EGG
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck]
- Logic:
  - Auto-upgrade Skill cards when obtained/offered
- Internal State: none

### TungstenRod
- ID: TUNGSTEN_ROD
- Rarity: Rare
- Pool: shared
- Vars: {HpLossReduction: 1}
- Hooks: [ModifyHpLostAfterOsty, AfterModifyingHpLostAfterOsty]
- Logic:
  - ModifyHpLostAfterOsty: reduce all HP loss by 1 (min 0)
- Internal State: none

### UnceasingTop
- ID: UNCEASING_TOP
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [AfterHandEmptied]
- Logic:
  - AfterHandEmptied: if in play phase, draw 1 card
- Internal State: none

### UnsettlingLamp
- ID: UNSETTLING_LAMP
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [BeforeCombatStart, BeforePowerAmountChanged, ModifyPowerAmountGiven, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - First card that applies debuffs to enemies: double all debuff amounts from that card (once per combat)
- Internal State: _triggeringCard, _doubledPowers (list), _isFinishedTriggering

### RuinedHelmet
- ID: RUINED_HELMET
- Rarity: Rare
- Pool: ironclad
- Vars: none
- Hooks: [TryModifyPowerAmountReceived, AfterModifyingPowerAmountReceived, AfterCombatEnd]
- Logic:
  - TryModifyPowerAmountReceived: first positive Strength gain in combat is doubled
- Internal State: _usedThisCombat (bool)

### VexingPuzzlebox
- ID: VEXING_PUZZLEBOX
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: if round 1, generate random card from character pool with 0 cost this combat into hand
- Internal State: none

### WhiteBeastStatue
- ID: WHITE_BEAST_STATUE
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [ShouldForcePotionReward]
- Logic:
  - ShouldForcePotionReward: force potion reward after all combat rooms
- Internal State: none

### WhiteStar
- ID: WHITE_STAR
- Rarity: Rare
- Pool: shared
- Vars: none
- Hooks: [TryModifyRewards]
- Logic:
  - TryModifyRewards: add boss-rarity card reward after elite combats
- Internal State: none

### PaperKrane
- ID: PAPER_KRANE
- Rarity: Rare
- Pool: silent
- Vars: none
- Hooks: [ModifyWeakMultiplier]
- Logic:
  - ModifyWeakMultiplier: when owner is Weak, reduce the Weak damage multiplier by 15%
- Internal State: none

---

## 5. Shop

### BeltBuckle
- ID: BELT_BUCKLE
- Rarity: Shop
- Pool: shared
- Vars: {DexterityPower: 2}
- Hooks: [AfterObtained, BeforeCombatStart, AfterCombatEnd, AfterPotionProcured, AfterPotionDiscarded, AfterPotionUsed, AfterCombatVictory]
- Logic:
  - When owner has no potions: gain 2 Dexterity; when potion procured: lose 2 Dexterity
- Internal State: _dexterityApplied (bool)

### Bread
- ID: BREAD
- Rarity: Shop
- Pool: shared
- Vars: {GainEnergy: 1, LoseEnergy: 2}
- Hooks: [ModifyMaxEnergy, AfterSideTurnStart]
- Logic:
  - ModifyMaxEnergy: +1 max energy (except round 1)
  - AfterSideTurnStart: on round 1, lose 2 energy
- Internal State: none

### Brimstone
- ID: BRIMSTONE
- Rarity: Shop
- Pool: ironclad
- Vars: {SelfStrength: 2, EnemyStrength: 1}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: gain 2 Strength; all enemies gain 1 Strength
- Internal State: none

### BurningSticks
- ID: BURNING_STICKS
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [AfterRoomEntered, AfterCardExhausted, AfterCombatEnd]
- Logic:
  - AfterCardExhausted: first Skill exhausted per combat: create clone of it in hand
- Internal State: _wasUsedThisCombat (bool)

### Cauldron
- ID: CAULDRON
- Rarity: Shop
- Pool: shared
- Vars: {Potions: 5}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 5 random potions as rewards
- Internal State: none

### ChemicalX
- ID: CHEMICAL_X
- Rarity: Shop
- Pool: shared
- Vars: {Increase: 2}
- Hooks: [BeforeCardPlayed, ModifyXValue]
- Logic:
  - ModifyXValue: X-cost cards get +2 to X value
- Internal State: none

### DingyRug
- ID: DINGY_RUG
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [ModifyCardRewardCreationOptions]
- Logic:
  - ModifyCardRewardCreationOptions: add colorless cards to card reward pool
- Internal State: none

### DollysMirror
- ID: DOLLYS_MIRROR
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: choose a card from deck (not Quest), duplicate it
- Internal State: none

### DragonFruit
- ID: DRAGON_FRUIT
- Rarity: Shop
- Pool: shared
- Vars: {MaxHp: 1}
- Hooks: [AfterGoldGained]
- Logic:
  - AfterGoldGained: gain 1 max HP every time gold is gained
- Internal State: none

### GhostSeed
- ID: GHOST_SEED
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [AfterCardEnteredCombat, AfterRoomEntered]
- Logic:
  - Makes basic Strike/Defend cards Ethereal in combat
- Internal State: none

### GnarledHammer
- ID: GNARLED_HAMMER
- Rarity: Shop
- Pool: shared
- Vars: {Cards: 3, SharpAmount: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select up to 3 cards from deck and enchant them with Sharp(3)
- Internal State: none

### Kifuda
- ID: KIFUDA
- Rarity: Shop
- Pool: shared
- Vars: {Cards: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select up to 3 cards from deck and enchant them with Adroit(3)
- Internal State: none

### LavaLamp
- ID: LAVA_LAMP
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [AfterRoomEntered, AfterDamageReceived, TryModifyCardRewardOptionsLate]
- Logic:
  - TryModifyCardRewardOptionsLate: if no damage taken this combat, upgrade all card rewards
- Internal State: _tookDamageThisCombat [SavedProperty]
- Uses [SavedProperty]: yes

### LeesWaffle
- ID: LEES_WAFFLE
- Rarity: Shop
- Pool: shared
- Vars: {MaxHp: 7}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 7 max HP and heal to full
- Internal State: none

### MembershipCard
- ID: MEMBERSHIP_CARD
- Rarity: Shop
- Pool: shared
- Vars: {Discount: 50}
- Hooks: [ModifyMerchantPrice]
- Logic:
  - ModifyMerchantPrice: 50% discount at merchants
- Internal State: none

### MiniatureTent
- ID: MINIATURE_TENT
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [ShouldDisableRemainingRestSiteOptions]
- Logic:
  - ShouldDisableRemainingRestSiteOptions: allow owner to use multiple rest site options
- Internal State: none

### MysticLighter
- ID: MYSTIC_LIGHTER
- Rarity: Shop
- Pool: shared
- Vars: {Damage: 9}
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: if enchanted card attack by owner, add +9 damage
- Internal State: none

### NinjaScroll
- ID: NINJA_SCROLL
- Rarity: Shop
- Pool: silent
- Vars: {Shivs: 3}
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: if round 1, generate 3 Shivs in hand
- Internal State: none

### Orrery
- ID: ORRERY
- Rarity: Shop
- Pool: shared
- Vars: {Cards: 5}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 5 card rewards (3 choices each)
- Internal State: none

### PunchDagger
- ID: PUNCH_DAGGER
- Rarity: Shop
- Pool: shared
- Vars: {Momentum: 5}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 1 card from deck and enchant with Momentum(5)
- Internal State: none

### RingingTriangle
- ID: RINGING_TRIANGLE
- Rarity: Shop
- Pool: shared
- Vars: none
- Hooks: [ShouldFlush]
- Logic:
  - ShouldFlush: prevent hand flush on turn 1 (retain all cards)
- Internal State: none

### RoyalStamp
- ID: ROYAL_STAMP
- Rarity: Shop
- Pool: shared
- Vars: {Cards: 1, Enchantment: "RoyallyApproved"}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 1 card from deck and enchant with RoyallyApproved
- Internal State: none

### RunicCapacitor
- ID: RUNIC_CAPACITOR
- Rarity: Shop
- Pool: defect
- Vars: {Repeat: 3}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, add 3 orb slots
- Internal State: none

### ScreamingFlagon
- ID: SCREAMING_FLAGON
- Rarity: Shop
- Pool: shared
- Vars: {Damage: 20}
- Hooks: [BeforeTurnEnd]
- Logic:
  - BeforeTurnEnd: if hand is empty at end of turn, deal 20 unpowered damage to all enemies
- Internal State: none

### SlingOfCourage
- ID: SLING_OF_COURAGE
- Rarity: Shop
- Pool: shared
- Vars: {StrengthPower: 2}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if elite room, gain 2 Strength
- Internal State: none

### TheAbacus
- ID: THE_ABACUS
- Rarity: Shop
- Pool: shared
- Vars: {Block: 6}
- Hooks: [AfterShuffle]
- Logic:
  - AfterShuffle: gain 6 block (unpowered) when draw pile shuffled
- Internal State: none

### Toolbox
- ID: TOOLBOX
- Rarity: Shop
- Pool: shared
- Vars: {Cards: 3}
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: if round 1, offer 3 random colorless cards, chosen one added to hand
- Internal State: none

### UndyingSigil
- ID: UNDYING_SIGIL
- Rarity: Shop
- Pool: necrobinder
- Vars: {DamageDecrease: 0.5}
- Hooks: [ModifyDamageMultiplicative]
- Logic:
  - ModifyDamageMultiplicative: if enemy attacker HP <= its Doom amount, halve damage to owner
- Internal State: none

### VitruvianMinion
- ID: VITRUVIAN_MINION
- Rarity: Shop
- Pool: regent
- Vars: none
- Hooks: [ModifyDamageMultiplicative, ModifyBlockMultiplicative]
- Logic:
  - Minion-tagged cards deal 2x damage and grant 2x block
- Internal State: none

### WingCharm
- ID: WING_CHARM
- Rarity: Shop
- Pool: shared
- Vars: {SwiftAmount: 1}
- Hooks: [TryModifyCardRewardOptionsLate]
- Logic:
  - TryModifyCardRewardOptionsLate: one random card reward gets Swift(1) enchantment
- Internal State: none

---

## 6. Event

### AlchemicalCoffer
- ID: ALCHEMICAL_COFFER
- Rarity: Ancient
- Pool: event
- Vars: {PotionSlots: 4}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 4 potion slots and fill them with random potions
- Internal State: none

### ArcaneScroll
- ID: ARCANE_SCROLL
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add 1 random Rare card to deck
- Internal State: none

### ArchaicTooth
- ID: ARCHAIC_TOOTH
- Rarity: Ancient
- Pool: event
- Vars: {StarterCard, AncientCard}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: transform starter card (Bash->Break, Neutralize->Suppress, etc.) into ancient version
- Internal State: _serializableStarterCard [SavedProperty], _serializableAncientCard [SavedProperty]
- Uses [SavedProperty]: yes

### Astrolabe
- ID: ASTROLABE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 3 cards from deck to transform, each gets upgraded
- Internal State: none

### BeautifulBracelet
- ID: BEAUTIFUL_BRACELET
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 3, Swift: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 3 cards from deck, enchant each with Swift(3)
- Internal State: none

### BigMushroom
- ID: BIG_MUSHROOM
- Rarity: Event
- Pool: event
- Vars: {MaxHp: 20, Cards: 2}
- Hooks: [AfterObtained, AfterRoomEntered, ModifyHandDraw]
- Logic:
  - AfterObtained: gain 20 max HP, grow character model
  - ModifyHandDraw: draw -2 cards on round 1
- Internal State: none

### BiiigHug
- ID: BIIIG_HUG
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 4}
- Hooks: [AfterObtained, AfterShuffle]
- Logic:
  - AfterObtained: remove 4 cards from deck
  - AfterShuffle: add Soot card to draw pile (random position)
- Internal State: none

### BingBong
- ID: BING_BONG
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [AfterCardChangedPiles]
- Logic:
  - AfterCardChangedPiles: when card added to deck (from no source), duplicate it to bottom of deck
- Internal State: _cardsToSkip (HashSet, prevents infinite loop)

### BlackStar
- ID: BLACK_STAR
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [TryModifyRewards]
- Logic:
  - TryModifyRewards: add extra relic reward after elite combats
- Internal State: none

### BlessedAntler
- ID: BLESSED_ANTLER
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1, Cards: 3}
- Hooks: [ModifyMaxEnergy, BeforeHandDraw]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - BeforeHandDraw: on round 1, shuffle 3 Dazed cards into draw pile
- Internal State: none

### BloodSoakedRose
- ID: BLOOD_SOAKED_ROSE
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [AfterObtained, ModifyMaxEnergy]
- Logic:
  - AfterObtained: add Enthralled curse to deck
  - ModifyMaxEnergy: +1 max energy
- Internal State: none

### BoneTea
- ID: BONE_TEA
- Rarity: Event
- Pool: event
- Vars: {Combats: 1}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: for 1 combat, upgrade all cards in hand on round 1
- Internal State: _combatsLeft [SavedProperty]
- Uses [SavedProperty]: yes

### BoomingConch
- ID: BOOMING_CONCH
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 2}
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: if round 1 and elite room, draw +2 cards
- Internal State: none

### BrilliantScarf
- ID: BRILLIANT_SCARF
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 5}
- Hooks: [TryModifyEnergyCostInCombat, TryModifyStarCost, BeforeSideTurnStart, AfterCardPlayed, AfterCombatEnd]
- Logic:
  - 5th card played each turn costs 0 energy/stars
- Internal State: _cardsPlayedThisTurn (int)

### Byrdpip
- ID: BYRDPIP
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [AfterObtained, BeforeCombatStart]
- Logic:
  - AfterObtained: assign random skin, transform ByrdonisEgg cards to ByrdSwoop, summon Byrdpip pet
- Internal State: _skin [SavedProperty]
- Uses [SavedProperty]: yes
- AddsPet/SpawnsPets: true

### CallingBell
- ID: CALLING_BELL
- Rarity: Ancient
- Pool: event
- Vars: {Relics: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add CurseOfTheBell to deck; offer 3 relic rewards (1 common, 1 uncommon, 1 rare)
- Internal State: none

### ChoicesParadox
- ID: CHOICES_PARADOX
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 5}
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: round 1, generate 5 random cards with Retain, player picks 1 for hand
- Internal State: none

### ChosenCheese
- ID: CHOSEN_CHEESE
- Rarity: Event
- Pool: event
- Vars: {MaxHp: 1}
- Hooks: [AfterCombatEnd]
- Logic:
  - AfterCombatEnd: gain 1 max HP after every combat
- Internal State: none

### Claws
- ID: CLAWS
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 6}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select up to 6 cards from deck to transform into Maul (preserves upgrades/enchantments)
- Internal State: none

### Crossbow
- ID: CROSSBOW
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: generate 1 random Attack card with 0 cost into hand each turn
- Internal State: none

### CursedPearl
- ID: CURSED_PEARL
- Rarity: Ancient
- Pool: event
- Vars: {Gold: 333}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add Greed curse to deck, gain 333 gold
- Internal State: none

### DarkstonePeriapt
- ID: DARKSTONE_PERIAPT
- Rarity: Event
- Pool: event
- Vars: {MaxHp: 6}
- Hooks: [AfterCardChangedPiles]
- Logic:
  - AfterCardChangedPiles: when curse added to deck, gain 6 max HP
- Internal State: none

### DaughterOfTheWind
- ID: DAUGHTER_OF_THE_WIND
- Rarity: Event
- Pool: event
- Vars: {Block: 1}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Attack played, gain 1 block (unpowered)
- Internal State: none

### DelicateFrond
- ID: DELICATE_FROND
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [BeforeCombatStart]
- Logic:
  - BeforeCombatStart: fill all empty potion slots with random potions
- Internal State: none

### DiamondDiadem
- ID: DIAMOND_DIADEM
- Rarity: Ancient
- Pool: event
- Vars: {CardThreshold: 2}
- Hooks: [AfterCardPlayed, BeforeTurnEnd, AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - BeforeTurnEnd: if played <= 2 cards this turn, gain DiamondDiademPower (stacking buff)
- Internal State: _cardsPlayedThisTurn

### DistinguishedCape
- ID: DISTINGUISHED_CAPE
- Rarity: Ancient
- Pool: event
- Vars: {HpLoss: 9, Cards: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: lose 9 max HP, add 3 Apparition cards to deck
- Internal State: none

### DreamCatcher
- ID: DREAM_CATCHER
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [TryModifyRestSiteHealRewards, ModifyExtraRestSiteHealText]
- Logic:
  - TryModifyRestSiteHealRewards: add card reward at rest site
- Internal State: none

### Driftwood
- ID: DRIFTWOOD
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [TryModifyRewardsLate]
- Logic:
  - TryModifyRewardsLate: allow rerolling card rewards
- Internal State: none

### DustyTome
- ID: DUSTY_TOME
- Rarity: Ancient
- Pool: event
- Vars: {AncientCard: string}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add 1 random Ancient-rarity card (upgraded) to deck
- Internal State: _ancientCard [SavedProperty]
- Uses [SavedProperty]: yes

### Ectoplasm
- ID: ECTOPLASM
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [ShouldGainGold, ModifyMaxEnergy]
- Logic:
  - ShouldGainGold: prevent all gold gain
  - ModifyMaxEnergy: +1 max energy
- Internal State: none

### ElectricShrymp
- ID: ELECTRIC_SHRYMP
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 1 card from deck, enchant with Imbued
- Internal State: none

### EmberTea
- ID: EMBER_TEA
- Rarity: Event
- Pool: event
- Vars: {Combats: 5, StrengthPower: 2}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: for 5 combats, gain 2 Strength at start
- Internal State: _combatsLeft [SavedProperty]
- Uses [SavedProperty]: yes

### EmptyCage
- ID: EMPTY_CAGE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 2}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: remove 2 cards from deck
- Internal State: none

### FakeAnchor
- ID: FAKE_ANCHOR
- Rarity: Event
- Pool: event
- Vars: {Block: 4}
- Hooks: [BeforeCombatStart]
- Logic:
  - BeforeCombatStart: gain 4 block (unpowered)
- Internal State: none
- MerchantCost: 50

### FakeBloodVial
- ID: FAKE_BLOOD_VIAL
- Rarity: Event
- Pool: event
- Vars: {Heal: 1}
- Hooks: [AfterPlayerTurnStartLate]
- Logic:
  - AfterPlayerTurnStartLate: if round 1, heal 1
- Internal State: none
- MerchantCost: 50

### FakeHappyFlower
- ID: FAKE_HAPPY_FLOWER
- Rarity: Event
- Pool: event
- Vars: {Energy: 1, Turns: 5}
- Hooks: [AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - AfterSideTurnStart: every 5 turns, gain 1 energy
- Internal State: _turnsSeen [SavedProperty], _isActivating
- Uses [SavedProperty]: yes
- MerchantCost: 50

### FakeLeesWaffle
- ID: FAKE_LEES_WAFFLE
- Rarity: Event
- Pool: event
- Vars: {Heal: 10}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: heal 10% of max HP
- Internal State: none
- MerchantCost: 50

### FakeMango
- ID: FAKE_MANGO
- Rarity: Event
- Pool: event
- Vars: {MaxHp: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 3 max HP
- Internal State: none
- MerchantCost: 50

### FakeMerchantsRug
- ID: FAKE_MERCHANTS_RUG
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: none
- Logic: (no effect - purely cosmetic/placeholder)
- Internal State: none

### FakeOrichalcum
- ID: FAKE_ORICHALCUM
- Rarity: Event
- Pool: event
- Vars: {Block: 3}
- Hooks: [BeforeTurnEndVeryEarly, BeforeTurnEnd, BeforeSideTurnStart]
- Logic:
  - BeforeTurnEnd: if 0 block at end of turn, gain 3 block (unpowered)
- Internal State: _shouldTrigger (bool)
- MerchantCost: 50

### FakeSneckoEye
- ID: FAKE_SNECKO_EYE
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [AfterObtained, BeforeCombatStart]
- Logic:
  - Applies Confused power (randomizes card costs)
- Internal State: _testEnergyCostOverride
- MerchantCost: 50

### FakeStrikeDummy
- ID: FAKE_STRIKE_DUMMY
- Rarity: Event
- Pool: event
- Vars: {ExtraDamage: 1}
- Hooks: [ModifyDamageAdditive]
- Logic:
  - ModifyDamageAdditive: Strike-tagged cards by owner deal +1 damage
- Internal State: none
- MerchantCost: 50

### FakeVenerableTeaSet
- ID: FAKE_VENERABLE_TEA_SET
- Rarity: Event
- Pool: event
- Vars: {Energy: 1}
- Hooks: [AfterRoomEntered, AfterEnergyReset]
- Logic:
  - After rest site, gain 1 energy next combat
- Internal State: _gainEnergyInNextCombat [SavedProperty]
- Uses [SavedProperty]: yes
- MerchantCost: 50

### Fiddle
- ID: FIDDLE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 2}
- Hooks: [ModifyHandDrawLate, ShouldDraw, AfterPreventingDraw]
- Logic:
  - ModifyHandDrawLate: +2 hand draw
  - ShouldDraw: block non-hand-draw draws on owner's turn (draw only during hand draw phase)
- Internal State: none

### ForgottenSoul
- ID: FORGOTTEN_SOUL
- Rarity: Event
- Pool: event
- Vars: {Damage: 1}
- Hooks: [AfterCardExhausted]
- Logic:
  - AfterCardExhausted: deal 1 unpowered damage to random enemy
- Internal State: none

### FragrantMushroom
- ID: FRAGRANT_MUSHROOM
- Rarity: Event
- Pool: event
- Vars: {HpLoss: 15, Cards: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: take 15 unblockable damage, upgrade 3 random cards in deck
- Internal State: none

### FresnelLens
- ID: FRESNEL_LENS
- Rarity: Event
- Pool: event
- Vars: {NimbleAmount: 2}
- Hooks: [TryModifyCardRewardOptionsLate, ModifyMerchantCardCreationResults, TryModifyCardBeingAddedToDeck]
- Logic:
  - All valid card rewards/additions get Nimble(2) enchantment
- Internal State: none

### FurCoat
- ID: FUR_COAT
- Rarity: Ancient
- Pool: event
- Vars: {Combats: 7}
- Hooks: [AfterObtained, ModifyGeneratedMapLate, BeforeCombatStart]
- Logic:
  - Marks 7 combat rooms on map; in those rooms, set all enemies to 1 HP at combat start
- Internal State: FurCoatActIndex [SavedProperty], FurCoatCoordCols [SavedProperty], FurCoatCoordRows [SavedProperty], FurCoatCoordsSet [SavedProperty]
- Uses [SavedProperty]: yes

### GlassEye
- ID: GLASS_EYE
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 5 card rewards (2 common, 2 uncommon, 1 rare)
- Internal State: none

### Glitter
- ID: GLITTER
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [TryModifyCardRewardOptionsLate]
- Logic:
  - All card rewards get Glam enchantment
- Internal State: none

### GoldenCompass
- ID: GOLDEN_COMPASS
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained, ModifyGeneratedMap, ModifyUnknownMapPointRoomTypes]
- Logic:
  - Replaces current act map with GoldenPathActMap (all events)
- Internal State: _goldenPathAct [SavedProperty]
- Uses [SavedProperty]: yes

### GoldenPearl
- ID: GOLDEN_PEARL
- Rarity: Ancient
- Pool: event
- Vars: {Gold: 150}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 150 gold
- Internal State: none

### HandDrill
- ID: HAND_DRILL
- Rarity: Event
- Pool: event
- Vars: {VulnerablePower: 2}
- Hooks: [AfterDamageGiven]
- Logic:
  - AfterDamageGiven: if owner breaks enemy's block, apply 2 Vulnerable
- Internal State: none

### HistoryCourse
- ID: HISTORY_COURSE
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [AfterPlayerTurnStartEarly]
- Logic:
  - AfterPlayerTurnStartEarly: after round 1, auto-play the last Attack/Skill played previous turn
- Internal State: none (checks combat history)

### IronClub
- ID: IRON_CLUB
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 4}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: every 4 cards played (persistent across combats), draw 1 card
- Internal State: _cardsPlayed [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### JeweledMask
- ID: JEWELED_MASK
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: round 1, pull random Power card from draw pile to hand, make it free this turn
- Internal State: none

### JewelryBox
- ID: JEWELRY_BOX
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add Apotheosis card to deck
- Internal State: none

### LargeCapsule
- ID: LARGE_CAPSULE
- Rarity: Ancient
- Pool: event
- Vars: {Relics: 2}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: obtain 2 random relics + add 1 basic Strike and 1 basic Defend to deck
- Internal State: none

### LavaRock
- ID: LAVA_ROCK
- Rarity: Ancient
- Pool: event
- Vars: {Relics: 2}
- Hooks: [TryModifyRewards]
- Logic:
  - TryModifyRewards: add 2 relic rewards after Act 1 boss (once only)
- Internal State: _hasTriggered [SavedProperty]
- Uses [SavedProperty]: yes

### LeadPaperweight
- ID: LEAD_PAPERWEIGHT
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 2 random Colorless cards, choose 1 to add to deck
- Internal State: none

### LeafyPoultice
- ID: LEAFY_POULTICE
- Rarity: Ancient
- Pool: event
- Vars: {MaxHp: 10}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: lose 10 max HP, transform 1 Strike and 1 Defend from deck
- Internal State: none

### LoomingFruit
- ID: LOOMING_FRUIT
- Rarity: Ancient
- Pool: event
- Vars: {MaxHp: 31}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 31 max HP
- Internal State: none

### LordsParasol
- ID: LORDS_PARASOL
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if merchant room, auto-purchase everything for free
- Internal State: none

### LostCoffer
- ID: LOST_COFFER
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 1 card reward + 1 potion reward
- Internal State: none

### LostWisp
- ID: LOST_WISP
- Rarity: Event
- Pool: event
- Vars: {Damage: 8}
- Hooks: [AfterCardPlayed]
- Logic:
  - AfterCardPlayed: when Power played, deal 8 unpowered damage to all enemies
- Internal State: none

### MassiveScroll
- ID: MASSIVE_SCROLL
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 3 multiplayer-only cards from all pools, choose 1 to add to deck
- Internal State: none

### MawBank
- ID: MAW_BANK
- Rarity: Event
- Pool: event
- Vars: {Gold: 12}
- Hooks: [AfterRoomEntered, AfterItemPurchased]
- Logic:
  - AfterRoomEntered: gain 12 gold entering each room (until something is purchased)
  - AfterItemPurchased: disable relic permanently after first purchase
- Internal State: _hasItemBeenBought [SavedProperty]
- Uses [SavedProperty]: yes

### MeatCleaver
- ID: MEAT_CLEAVER
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [TryModifyRestSiteOptions]
- Logic:
  - TryModifyRestSiteOptions: add Cook rest site option
- Internal State: none

### MrStruggles
- ID: MR_STRUGGLES
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: deal damage = current round number (unpowered) to all enemies each turn
- Internal State: none

### MusicBox
- ID: MUSIC_BOX
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [BeforeCardPlayed, AfterCardPlayed, BeforeSideTurnStart, AfterCombatEnd]
- Logic:
  - First Attack played each turn: create Ethereal clone of it in hand
- Internal State: _wasUsedThisTurn, _cardBeingPlayed

### NeowsTorment
- ID: NEOWS_TORMENT
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add NeowsFury card to deck
- Internal State: none

### NewLeaf
- ID: NEW_LEAF
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 1 card to transform randomly
- Internal State: none

### NutritiousOyster
- ID: NUTRITIOUS_OYSTER
- Rarity: Ancient
- Pool: event
- Vars: {MaxHp: 11}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 11 max HP
- Internal State: none

### NutritiousSoup
- ID: NUTRITIOUS_SOUP
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: enchant all basic Strike cards with TezcatarasEmber
- Internal State: none

### PaelsBlood
- ID: PAELS_BLOOD
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [ModifyHandDraw]
- Logic:
  - ModifyHandDraw: draw +1 card every turn
- Internal State: none

### PaelsClaw
- ID: PAELS_CLAW
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 3, EnchantmentName: Goopy}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: enchant all valid cards in deck with Goopy
- Internal State: none

### PaelsEye
- ID: PAELS_EYE
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [BeforeCardPlayed, AfterSideTurnStart, ShouldTakeExtraTurn, BeforeTurnEndEarly, AfterTakingExtraTurn, AfterCombatEnd]
- Logic:
  - If no cards played on a turn: exhaust all hand, take an extra turn (once per combat)
- Internal State: _usedThisCombat, _anyCardsPlayedThisTurn

### PaelsFlesh
- ID: PAELS_FLESH
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [BeforeCombatStart, BeforeSideTurnStart, AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - AfterSideTurnStart: from round 3 onward, gain 1 energy each turn
- Internal State: none

### PaelsGrowth
- ID: PAELS_GROWTH
- Rarity: Ancient
- Pool: event
- Vars: {EnchantmentName: Clone}
- Hooks: [AfterObtained, TryModifyRestSiteOptions]
- Logic:
  - AfterObtained: enchant 1 card with Clone(4)
  - TryModifyRestSiteOptions: add Clone rest site option
- Internal State: none

### PaelsHorn
- ID: PAELS_HORN
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add 2 Relax cards to deck
- Internal State: none

### PaelsLegion
- ID: PAELS_LEGION
- Rarity: Ancient
- Pool: event
- Vars: {Turns: 2}
- Hooks: [AfterObtained, BeforeCombatStart, ModifyBlockMultiplicative, AfterModifyingBlockAmount, AfterCardPlayed, AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - Summons PaelsLegion pet; doubles block from first block-granting card (2 turn cooldown)
- Internal State: _skin [SavedProperty], _cooldown, _triggeredBlockLastTurn, _affectedCardPlay
- Uses [SavedProperty]: yes
- AddsPet/SpawnsPets: true

### PaelsTears
- ID: PAELS_TEARS
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 2}
- Hooks: [BeforeTurnEnd, AfterSideTurnStart, AfterCombatEnd]
- Logic:
  - If leftover energy at end of turn, gain 2 energy next turn
- Internal State: _hadLeftoverEnergy (bool)

### PaelsTooth
- ID: PAELS_TOOTH
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 5, CardTitles: string}
- Hooks: [AfterObtained, AfterCombatEnd]
- Logic:
  - AfterObtained: remove up to 5 upgradable cards from deck, store them
  - AfterCombatEnd: return 1 random stored card (upgraded) to deck
- Internal State: _serializableCards [SavedProperty]
- Uses [SavedProperty]: yes

### PaelsWing
- ID: PAELS_WING
- Rarity: Ancient
- Pool: event
- Vars: {Sacrifices: 2}
- Hooks: [TryModifyCardRewardAlternatives]
- Logic:
  - Adds "Sacrifice" option to card rewards; every 2 sacrifices, obtain a random relic
- Internal State: _rewardsSacrificed [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### PandorasBox
- ID: PANDORAS_BOX
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: transform all basic Strikes and Defends in deck to random cards
- Internal State: none

### PhilosophersStone
- ID: PHILOSOPHERS_STONE
- Rarity: Ancient
- Pool: event
- Vars: {StrengthPower: 1, Energy: 1}
- Hooks: [ModifyMaxEnergy, AfterCreatureAddedToCombat, AfterRoomEntered]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - AfterRoomEntered/AfterCreatureAddedToCombat: all enemies gain 1 Strength
- Internal State: none

### PollinousCore
- ID: POLLINOUS_CORE
- Rarity: Event
- Pool: event
- Vars: {Cards: 2, Turns: 4}
- Hooks: [BeforeSideTurnStart, AfterCombatEnd, ModifyHandDraw, AfterModifyingHandDraw]
- Logic:
  - Every 4 turns: draw +2 extra cards
- Internal State: _turnsSeen [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### Pomander
- ID: POMANDER
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: upgrade 1 card from deck
- Internal State: none

### PrecariousShears
- ID: PRECARIOUS_SHEARS
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 2, Damage: 13}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: remove 2 cards from deck, take 13 unpowered damage
- Internal State: none

### PreciseScissors
- ID: PRECISE_SCISSORS
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: remove 1 card from deck
- Internal State: none

### PreservedFog
- ID: PRESERVED_FOG
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 5}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: remove 5 cards from deck, add Folly curse to deck
- Internal State: none

### PrismaticGem
- ID: PRISMATIC_GEM
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [ModifyMaxEnergy, ModifyCardRewardCreationOptions]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - ModifyCardRewardCreationOptions: add all character card pools to card rewards
- Internal State: none

### PumpkinCandle
- ID: PUMPKIN_CANDLE
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [AfterObtained, ModifyMaxEnergy, AfterRoomEntered]
- Logic:
  - ModifyMaxEnergy: +1 max energy (only in the act it was obtained)
- Internal State: _activeAct [SavedProperty]
- Uses [SavedProperty]: yes

### RadiantPearl
- ID: RADIANT_PEARL
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 1}
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: round 1, add 1 Luminesce card to hand
- Internal State: none

### RoyalPoison
- ID: ROYAL_POISON
- Rarity: Event
- Pool: event
- Vars: {Damage: 4}
- Hooks: [AfterPlayerTurnStart]
- Logic:
  - AfterPlayerTurnStart: round 1, deal 4 unblockable/unpowered damage to self
- Internal State: none

### RunicPyramid
- ID: RUNIC_PYRAMID
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [ShouldFlush]
- Logic:
  - ShouldFlush: prevent hand flush (retain all cards every turn)
- Internal State: none

### Sai
- ID: SAI
- Rarity: Ancient
- Pool: event
- Vars: {Block: 7}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: gain 7 block (unpowered) every turn
- Internal State: none

### SandCastle
- ID: SAND_CASTLE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 6}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: upgrade 6 random upgradable cards in deck
- Internal State: none

### ScrollBoxes
- ID: SCROLL_BOXES
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: lose all gold; choose from 2 bundles of 3 cards (2 common + 1 uncommon each)
- Internal State: none

### SeaGlass
- ID: SEA_GLASS
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 15, Character: string}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 15 cards from assigned character's pool (5 common, 5 uncommon, 5 rare)
- Internal State: _characterId [SavedProperty]
- Uses [SavedProperty]: yes

### SealOfGold
- ID: SEAL_OF_GOLD
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1, Gold: 5}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if owner has >= 5 gold, gain 1 energy and lose 5 gold
- Internal State: none

### SereTalon
- ID: SERE_TALON
- Rarity: Ancient
- Pool: event
- Vars: {Curses: 2, Wishes: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add 2 random curses and 3 Wish cards to deck
- Internal State: none

### SignetRing
- ID: SIGNET_RING
- Rarity: Ancient
- Pool: event
- Vars: {Gold: 999}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: gain 999 gold
- Internal State: none

### SilverCrucible
- ID: SILVER_CRUCIBLE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 3}
- Hooks: [TryModifyCardRewardOptionsLate, AfterModifyingCardRewardOptions, AfterRoomEntered, ShouldGenerateTreasure]
- Logic:
  - First 3 card rewards: all cards pre-upgraded
  - Skips first treasure room generation
- Internal State: _timesUsed [SavedProperty], _treasureRoomsEntered [SavedProperty]
- Uses [SavedProperty]: yes

### SmallCapsule
- ID: SMALL_CAPSULE
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: offer 1 random relic reward
- Internal State: none

### SneckoEye
- ID: SNECKO_EYE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 2}
- Hooks: [AfterObtained, BeforeCombatStart, ModifyHandDraw]
- Logic:
  - Applies Confused power + draw +2 cards per turn
- Internal State: _testEnergyCostOverride

### Sozu
- ID: SOZU
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [ShouldProcurePotion, ModifyMaxEnergy]
- Logic:
  - ShouldProcurePotion: prevent all potion procurement
  - ModifyMaxEnergy: +1 max energy
- Internal State: none

### SpikedGauntlets
- ID: SPIKED_GAUNTLETS
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [ModifyMaxEnergy, TryModifyEnergyCostInCombat]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - TryModifyEnergyCostInCombat: Power cards cost +1 energy
- Internal State: none

### StoneHumidifier
- ID: STONE_HUMIDIFIER
- Rarity: Ancient
- Pool: event
- Vars: {MaxHp: 5}
- Hooks: [AfterRestSiteHeal, ModifyExtraRestSiteHealText]
- Logic:
  - AfterRestSiteHeal: gain 5 max HP after rest site heal
- Internal State: none

### Storybook
- ID: STORYBOOK
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add BrightestFlame card to deck
- Internal State: none

### SwordOfStone
- ID: SWORD_OF_STONE
- Rarity: Event
- Pool: event
- Vars: {Elites: 5}
- Hooks: [AfterCombatVictory]
- Logic:
  - AfterCombatVictory: count elite victories; at 5, transform into SwordOfJade
- Internal State: _elitesDefeated [SavedProperty]
- Uses [SavedProperty]: yes

### SwordOfJade
- ID: SWORD_OF_JADE
- Rarity: Event
- Pool: event (transformed from SwordOfStone)
- Vars: {StrengthPower: 3}
- Hooks: [AfterRoomEntered]
- Logic:
  - AfterRoomEntered: if combat room, gain 3 Strength
- Internal State: none

### TanxsWhistle
- ID: TANXS_WHISTLE
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: add Whistle card to deck
- Internal State: none

### TeaOfDiscourtesy
- ID: TEA_OF_DISCOURTESY
- Rarity: Event
- Pool: event
- Vars: {Heal: 1, Combats: 1, DazedCount: 2}
- Hooks: [BeforeCombatStart]
- Logic:
  - BeforeCombatStart: for 1 combat, add 2 Dazed cards to draw pile
- Internal State: _combatsLeft [SavedProperty]
- Uses [SavedProperty]: yes

### TheBoot
- ID: THE_BOOT
- Rarity: Event
- Pool: event
- Vars: {DamageMinimum: 5, DamageThreshold: 4}
- Hooks: [ModifyHpLostBeforeOsty, AfterModifyingHpLostBeforeOsty]
- Logic:
  - ModifyHpLostBeforeOsty: if owner's powered attack deals < 5 damage (but > 0), set to 5
- Internal State: none

### ThrowingAxe
- ID: THROWING_AXE
- Rarity: Ancient
- Pool: event
- Vars: none
- Hooks: [AfterRoomEntered, ModifyCardPlayCount, AfterModifyingCardPlayCount, AfterCombatEnd]
- Logic:
  - First card played each combat: played twice (once per combat)
- Internal State: _usedThisCombat (bool)

### ToastyMittens
- ID: TOASTY_MITTENS
- Rarity: Ancient
- Pool: event
- Vars: {StrengthPower: 1}
- Hooks: [BeforeHandDraw]
- Logic:
  - BeforeHandDraw: every turn, exhaust top card of draw pile and gain 1 Strength
- Internal State: none

### TouchOfOrobas
- ID: TOUCH_OF_OROBAS
- Rarity: Ancient
- Pool: event
- Vars: {StarterRelic: string, UpgradedRelic: string}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: replaces starter relic with upgraded version (BurningBlood->BlackBlood, etc.)
- Internal State: _starterRelic [SavedProperty], _upgradedRelic [SavedProperty]
- Uses [SavedProperty]: yes

### ToyBox
- ID: TOY_BOX
- Rarity: Ancient
- Pool: event
- Vars: {Relics: 4, Combats: 3}
- Hooks: [AfterObtained, AfterCombatEnd]
- Logic:
  - AfterObtained: offer 4 wax relics
  - AfterCombatEnd: every 3 combats, melt one wax relic
- Internal State: _combatsSeen [SavedProperty], _isActivating
- Uses [SavedProperty]: yes

### TriBoomerang
- ID: TRI_BOOMERANG
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 3}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 3 cards from deck, enchant with Instinct
- Internal State: none

### VelvetChoker
- ID: VELVET_CHOKER
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 6, Energy: 1}
- Hooks: [ModifyMaxEnergy, ShouldPlay, AfterCardPlayed, AfterRoomEntered, AfterCombatEnd, BeforeSideTurnStart]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - ShouldPlay: prevent playing cards after 6th card each turn
- Internal State: _cardsPlayedThisTurn

### VeryHotCocoa
- ID: VERY_HOT_COCOA
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 4}
- Hooks: [AfterSideTurnStart]
- Logic:
  - AfterSideTurnStart: if player side and round 1, gain 4 energy
- Internal State: none

### WarHammer
- ID: WAR_HAMMER
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 4}
- Hooks: [AfterCombatVictory]
- Logic:
  - AfterCombatVictory: if elite room, upgrade 4 random upgradable cards in deck
- Internal State: none

### WhisperingEarring
- ID: WHISPERING_EARRING
- Rarity: Ancient
- Pool: event
- Vars: {Energy: 1}
- Hooks: [ModifyMaxEnergy, BeforePlayPhaseStart]
- Logic:
  - ModifyMaxEnergy: +1 max energy
  - BeforePlayPhaseStart: round 1, auto-play all playable cards in hand (up to 13)
- Internal State: none

### WongoCustomerAppreciationBadge
- ID: WONGO_CUSTOMER_APPRECIATION_BADGE
- Rarity: Event
- Pool: event
- Vars: none
- Hooks: none
- Logic: (no effect)
- Internal State: none

### WongosMysteryTicket
- ID: WONGOS_MYSTERY_TICKET
- Rarity: Event
- Pool: event
- Vars: {Repeat: 3, RemainingCombats: 5}
- Hooks: [AfterCombatEnd, TryModifyRewards, AfterModifyingRewards]
- Logic:
  - AfterCombatEnd: count combats; after 5, add 3 relic rewards to next combat (once)
- Internal State: _combatsFinished [SavedProperty], _gaveRelic [SavedProperty]
- Uses [SavedProperty]: yes

### YummyCookie
- ID: YUMMY_COOKIE
- Rarity: Ancient
- Pool: event
- Vars: {Cards: 4}
- Hooks: [AfterObtained]
- Logic:
  - AfterObtained: select 4 cards from deck to upgrade
- Internal State: none (character-based icon)

---

## 7. Ancient

Note: Many "Ancient" relics are in the Event pool. The "Ancient" here reflects their RelicRarity property. They have been listed under Event (section 6) above since that is their pool assignment. The following relics have Rarity=Ancient but were not covered above:

(All Ancient-rarity relics have been listed above under their pool assignments in section 6.)

---

## 8. Special / Other

### Circlet
- ID: CIRCLET
- Rarity: None
- Pool: fallback
- Vars: none
- Hooks: none
- Logic: stackable placeholder relic when no other relics available
- Internal State: none
- IsStackable: true

### DeprecatedRelic
- ID: DEPRECATED_RELIC
- Rarity: None
- Pool: deprecated
- Vars: none
- Hooks: none
- Logic: stackable placeholder for deprecated relics
- Internal State: none
- IsStackable: true

### VakuuCardSelector
- Not a relic. Helper class implementing ICardSelector for WhisperingEarring's auto-play logic.

---

## Summary Statistics

| Rarity | Count |
|--------|-------|
| Starter | 10 |
| Common | ~32 |
| Uncommon | ~32 |
| Rare | ~40 |
| Shop | ~28 |
| Event | ~50 |
| Ancient | ~96 |
| None | 2 |

## Key Hook Methods Reference

| Hook | Description |
|------|-------------|
| AfterObtained | When relic is picked up |
| BeforeCombatStart / BeforeCombatStartLate | Before combat begins |
| AfterRoomEntered | When entering a room |
| BeforeSideTurnStart / AfterSideTurnStart | Start of a side's turn |
| AfterPlayerTurnStart / AfterPlayerTurnStartEarly / AfterPlayerTurnStartLate | Player turn start |
| BeforeHandDraw | Before drawing opening hand |
| AfterEnergyReset / AfterEnergyResetLate | When energy resets |
| BeforeCardPlayed / AfterCardPlayed | Card play hooks |
| BeforeTurnEnd / BeforeTurnEndVeryEarly / BeforeTurnEndEarly / AfterTurnEnd | Turn end hooks |
| AfterCardExhausted | When card is exhausted |
| AfterCardDiscarded | When card is discarded |
| AfterCardChangedPiles | When card moves between piles |
| AfterCardEnteredCombat | When card enters combat zone |
| AfterDamageReceived / AfterDamageGiven | Damage hooks |
| AfterDeath | When creature dies |
| AfterCombatEnd / AfterCombatVictory / AfterCombatVictoryEarly | Combat end hooks |
| ModifyHandDraw / ModifyHandDrawLate | Modify cards drawn per turn |
| ModifyMaxEnergy | Modify max energy |
| ModifyDamageAdditive / ModifyDamageMultiplicative | Damage modification |
| ModifyBlockMultiplicative | Block modification |
| ModifyHpLostBeforeOsty / ModifyHpLostAfterOsty | HP loss modification |
| ModifyXValue | Modify X-cost card values |
| ModifyCardPlayCount | Modify how many times card is played |
| ModifyPowerAmountGiven / ModifyPowerAmountGiven | Power amount modification |
| ModifyMerchantPrice | Merchant price modification |
| ModifyCardRewardCreationOptions | Card reward pool modification |
| TryModifyRewards / TryModifyRewardsLate | Reward modification |
| TryModifyCardRewardOptions / TryModifyCardRewardOptionsLate | Card reward options modification |
| TryModifyCardBeingAddedToDeck | Modify card before deck addition |
| TryModifyEnergyCostInCombat | Modify card energy cost |
| TryModifyRestSiteOptions | Add rest site options |
| TryModifyRestSiteHealRewards | Add rewards at rest site |
| ShouldGainGold | Prevent gold gain |
| ShouldPlay | Prevent card play |
| ShouldFlush | Prevent hand flush (retain) |
| ShouldDieLate | Prevent death |
| ShouldPlayerResetEnergy | Prevent energy reset |
| ShouldProcurePotion | Prevent potion procurement |
| ShouldDraw | Prevent card draw |
| ShouldClearBlock | Prevent block clear |
| ShouldForcePotionReward | Force potion in rewards |
| ShouldTakeExtraTurn | Grant extra turn |
| AfterShuffle | When draw pile shuffled |
| AfterHandEmptied | When hand is empty |
| AfterBlockCleared | After block clear phase |
| AfterCurrentHpChanged | When HP changes |
| AfterPotionUsed / AfterPotionProcured / AfterPotionDiscarded | Potion hooks |
| AfterStarsSpent | When stars spent |
| AfterOrbChanneled | When orb channeled |
| AfterRestSiteHeal | After rest site heal |
| ModifyRestSiteHealAmount | Modify rest heal amount |
| AfterGoldGained | After gold gained |
| AfterItemPurchased | After merchant purchase |
| ModifyGeneratedMap / ModifyGeneratedMapLate | Map generation modification |
| ModifyUnknownMapPointRoomTypes | Unknown room type modification |
