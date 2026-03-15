# Slay the Spire 2 - Monsters & Encounters Reference

> Auto-generated from decompiled source. All values shown as `normal (ascension)`.
> Ascension HP uses `AscensionLevel.ToughEnemies`; ascension damage uses `AscensionLevel.DeadlyEnemies`.

---

## Act 1 - Overgrowth

Encounters in this act: FuzzyWurmCrawlerWeak, NibbitsWeak, ShrinkerBeetleWeak, SlimesWeak, CubexConstructNormal, FlyconidNormal, FogmogNormal, InkletsNormal, MawlerNormal, NibbitsNormal, OvergrowthCrawlers, RubyRaidersNormal, SlimesNormal, SlitheringStranglerNormal, SnappingJaxfruitNormal, VineShamblerNormal, BygoneEffigyElite, ByrdonisElite, PhrogParasiteElite, CeremonialBeastBoss, TheKinBoss, VantomBoss.

### Weak Encounters

---

### FuzzyWurmCrawler
- ID: FUZZY_WURM_CRAWLER
- Type: Normal (Weak encounter)
- HP: 55-57 (ascension: 58-59)
- Damage Values: {AcidGoop: 4 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: FIRST_ACID_GOOP
  FIRST_ACID_GOOP -> INHALE -> ACID_GOOP -> FIRST_ACID_GOOP (loop)
  ```
- Moves:
  - FIRST_ACID_GOOP: Deal AcidGoopDamage [SingleAttack]
  - INHALE: Gain 7 Strength [Buff]
  - ACID_GOOP: Deal AcidGoopDamage [SingleAttack]
- Special: Puffs up visually when inhaling

---

### Nibbit
- ID: NIBBIT
- Type: Normal (appears in Weak and Normal encounters)
- HP: 11-13 (ascension: 12-14)
- Damage Values: {Nibble: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: NIBBLE_MOVE
  NIBBLE_MOVE -> NIBBLE_MOVE (repeat forever)
  ```
- Moves:
  - NIBBLE_MOVE: Deal NibbleDamage [SingleAttack]
- Special: Simple single-move monster

---

### ShrinkerBeetle
- ID: SHRINKER_BEETLE
- Type: Normal (Weak encounter)
- HP: 23-26 (ascension: 24-27)
- Damage Values: {Chomp: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SHRINK_MOVE
  SHRINK_MOVE -> CHOMP_MOVE -> CHOMP_MOVE (repeat)
  ```
- Moves:
  - SHRINK_MOVE: Apply 1 Weak + 1 Vulnerable to targets [Debuff]
  - CHOMP_MOVE: Deal ChompDamage [SingleAttack]
- Special: AfterAddedToRoom: applies ShrinkerPower(1) to self

---

### LeafSlimeM (Medium Leaf Slime)
- ID: LEAF_SLIME_M
- Type: Normal (appears in SlimesWeak/SlimesNormal)
- HP: 27-29 (ascension: 29-31)
- Damage Values: {Slam: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLAM_MOVE
  SLAM_MOVE -> SLAM_MOVE (repeat)
  ```
- Moves:
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]
- Special: AfterAddedToRoom: applies SplitPower to self. On death, splits into 2 LeafSlimeS

---

### LeafSlimeS (Small Leaf Slime)
- ID: LEAF_SLIME_S
- Type: Normal (spawned from LeafSlimeM split)
- HP: 6-9 (ascension: 7-10)
- Damage Values: {Slam: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLAM_MOVE
  SLAM_MOVE -> SLAM_MOVE (repeat)
  ```
- Moves:
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]
- Special: Spawned from LeafSlimeM split

---

### TwigSlimeM (Medium Twig Slime)
- ID: TWIG_SLIME_M
- Type: Normal (appears in SlimesWeak/SlimesNormal)
- HP: 27-29 (ascension: 29-31)
- Damage Values: {Slam: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLAM_MOVE
  SLAM_MOVE -> SLAM_MOVE (repeat)
  ```
- Moves:
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]
- Special: AfterAddedToRoom: applies SplitPower to self. On death, splits into 2 TwigSlimeS

---

### TwigSlimeS (Small Twig Slime)
- ID: TWIG_SLIME_S
- Type: Normal (spawned from TwigSlimeM split)
- HP: 6-9 (ascension: 7-10)
- Damage Values: {Slam: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLAM_MOVE
  SLAM_MOVE -> SLAM_MOVE (repeat)
  ```
- Moves:
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]

---

### Normal Encounters

---

### CubexConstruct
- ID: CUBEX_CONSTRUCT
- Type: Normal
- HP: 65 (ascension: 70)
- Damage Values: {Blast: 7 (8), Expel: 5 (6)}
- Block Values: {Submerge: 15, InitialBlock: 13}
- AI State Machine:
  ```
  INITIAL: CHARGE_UP_MOVE
  CHARGE_UP_MOVE -> REPEATER_MOVE -> REPEATER_MOVE_2 -> EXPEL_BLAST -> REPEATER_MOVE (loop)
  SUBMERGE_MOVE -> CHARGE_UP_MOVE
  ```
- Moves:
  - CHARGE_UP_MOVE: Gain 2 Strength [Buff]
  - REPEATER_MOVE: Deal BlastDamage + gain 2 Strength [SingleAttack + Buff]
  - REPEATER_MOVE_2: Deal BlastDamage + gain 2 Strength [SingleAttack + Buff]
  - EXPEL_BLAST: Deal ExpelDamage x2 [MultiAttack]
  - SUBMERGE_MOVE: Gain 15 Block [Defend]
- Special: AfterAddedToRoom: gain 13 Block + 1 Artifact

---

### Flyconid
- ID: FLYCONID
- Type: Normal
- HP: 47-49 (ascension: 51-53)
- Damage Values: {Smash: 11 (12), FrailSpores: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: INITIAL_RANDOM (weighted: FrailSpores 2, Smash 1)
  then -> RAND (weighted: VulnerableSpores 3, FrailSpores 2, Smash 1; all CannotRepeat)
  All moves -> RAND
  ```
- Moves:
  - VULNERABLE_SPORES_MOVE: Apply 2 Vulnerable to targets [Debuff]
  - FRAIL_SPORES_MOVE: Deal SporeDamage + apply 2 Frail [SingleAttack + Debuff]
  - SMASH_MOVE: Deal SmashDamage [SingleAttack]
- Special: First turn cannot use VulnerableSpores

---

### Fogmog
- ID: FOGMOG
- Type: Normal
- HP: 74 (ascension: 78)
- Damage Values: {Swipe: 8 (9), Headbutt: 14 (16)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: ILLUSION_MOVE
  ILLUSION_MOVE -> SWIPE_MOVE -> RAND(SwipeRandom 40%, Headbutt 60%; CannotRepeat)
  SWIPE_RANDOM -> HEADBUTT -> SWIPE_MOVE (loop)
  HEADBUTT -> SWIPE_MOVE
  ```
- Moves:
  - ILLUSION_MOVE: Summon EyeWithTeeth minion [Summon]
  - SWIPE_MOVE: Deal SwipeDamage + gain 1 Strength [SingleAttack + Buff]
  - HEADBUTT_MOVE: Deal HeadbuttDamage [SingleAttack]
- Special: Summons EyeWithTeeth (6HP, Illusion power, adds 3 Dazed to discard each turn)

---

### EyeWithTeeth
- ID: EYE_WITH_TEETH
- Type: Minion (summoned by Fogmog)
- HP: 6 (fixed)
- Damage Values: none
- Block Values: none
- AI State Machine:
  ```
  INITIAL: DISTRACT_MOVE
  DISTRACT_MOVE -> DISTRACT_MOVE (repeat)
  ```
- Moves:
  - DISTRACT_MOVE: Add 3 Dazed to targets' discard pile [Status]
- Special: Has Illusion power. Does not disappear from Doom. Death anim only plays when no primary enemies alive.

---

### Inklet
- ID: INKLET
- Type: Normal
- HP: 30-33 (ascension: 32-35)
- Damage Values: {Splatter: 6 (7), Submerge: 4 (5)}
- Block Values: {Submerge: 8 (9)}
- AI State Machine:
  ```
  INITIAL: CONDITIONAL
    if slot == "first": SPLATTER_MOVE
    if slot == "second": SUBMERGE_MOVE
    else: RAND
  SPLATTER_MOVE -> SUBMERGE_MOVE -> RAND(Splatter CannotRepeat, Submerge CannotRepeat)
  Both -> RAND
  ```
- Moves:
  - SPLATTER_MOVE: Deal SplatterDamage [SingleAttack]
  - SUBMERGE_MOVE: Deal SubDamage + gain Block [SingleAttack + Defend]
- Special: Starting move depends on slot position

---

### Mawler
- ID: MAWLER
- Type: Normal
- HP: 72 (ascension: 76)
- Damage Values: {RipAndTear: 14 (16), Claw: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAND(RipAndTear CannotRepeat, Roar UseOnlyOnce, Claw CannotRepeat)
  All -> RAND
  ```
- Moves:
  - RIP_AND_TEAR_MOVE: Deal RipAndTearDamage [SingleAttack]
  - ROAR_MOVE: Apply 2 Vulnerable to targets [Debuff] (UseOnlyOnce)
  - CLAW_MOVE: Deal ClawDamage x2 [MultiAttack]
- Special: Roar can only be used once per combat

---

### VineShambler
- ID: VINE_SHAMBLER
- Type: Normal
- HP: 40-43 (ascension: 42-45)
- Damage Values: {VineWhip: 7 (8), Tangle: 10 (12)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAND(VineWhip CannotRepeat, Tangle CannotRepeat)
  Both -> RAND
  ```
- Moves:
  - VINE_WHIP_MOVE: Deal VineWhipDamage + apply 1 Weak [SingleAttack + Debuff]
  - TANGLE_MOVE: Deal TangleDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Thorns(3) to self

---

### SlitheringStrangler
- ID: SLITHERING_STRANGLER
- Type: Normal
- HP: 45-49 (ascension: 47-51)
- Damage Values: {Squeeze: 5 (6), Strangle: 12 (14)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SQUEEZE_MOVE
  SQUEEZE_MOVE -> STRANGLE_MOVE -> SQUEEZE_MOVE (alternating)
  ```
- Moves:
  - SQUEEZE_MOVE: Deal SqueezeDamage x2 [MultiAttack]
  - STRANGLE_MOVE: Deal StrangleDamage + apply 1 Weak [SingleAttack + Debuff]
- Special: AfterAddedToRoom: applies Constrict(3) to self

---

### SnappingJaxfruit
- ID: SNAPPING_JAXFRUIT
- Type: Normal
- HP: 53-56 (ascension: 56-59)
- Damage Values: {Snap: 7 (8), SeedSpit: 1}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAND(Snap CannotRepeat, SeedSpit CannotRepeat, Burrow CannotRepeat; weight 1 each)
  All -> RAND
  ```
- Moves:
  - SNAP_MOVE: Deal SnapDamage [SingleAttack]
  - SEED_SPIT_MOVE: Deal 1 damage x SeedSpitRepeats(4/5 asc) + apply 1 Frail [MultiAttack + Debuff]
  - BURROW_MOVE: Gain 2 Strength [Buff]
- Special: AfterAddedToRoom: applies ThornsPower(3) to self

---

### RubyRaiders (encounter with multiple raider types)

### AssassinRubyRaider
- ID: ASSASSIN_RUBY_RAIDER
- Type: Normal (part of RubyRaidersNormal)
- HP: 18-23 (ascension: 19-24)
- Damage Values: {Killshot: 11 (12)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: KILLSHOT_MOVE
  KILLSHOT_MOVE -> KILLSHOT_MOVE (repeat)
  ```
- Moves:
  - KILLSHOT_MOVE: Deal KillshotDamage [SingleAttack]

---

### AxeRubyRaider
- ID: AXE_RUBY_RAIDER
- Type: Normal (part of RubyRaidersNormal)
- HP: 20-22 (ascension: 21-23)
- Damage Values: {Swing: 5 (6), BigSwing: 12 (13)}
- Block Values: {Swing: 5 (6)}
- AI State Machine:
  ```
  INITIAL: SWING_1
  SWING_1 -> SWING_2 -> BIG_SWING -> SWING_1 (cycle)
  ```
- Moves:
  - SWING_1/SWING_2: Deal SwingDamage + gain SwingBlock [SingleAttack + Defend]
  - BIG_SWING: Deal BigSwingDamage [SingleAttack]

---

### BruteRubyRaider
- ID: BRUTE_RUBY_RAIDER
- Type: Normal (part of RubyRaidersNormal)
- HP: 30-33 (ascension: 31-34)
- Damage Values: {Beat: 7 (8)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: BEAT_MOVE
  BEAT_MOVE -> ROAR_MOVE -> BEAT_MOVE (alternating)
  ```
- Moves:
  - BEAT_MOVE: Deal BeatDamage [SingleAttack]
  - ROAR_MOVE: Gain 3 Strength [Buff]

---

### CrossbowRubyRaider
- ID: CROSSBOW_RUBY_RAIDER
- Type: Normal (part of RubyRaidersNormal)
- HP: 18-21 (ascension: 19-22)
- Damage Values: {Fire: 14 (16)}
- Block Values: {Reload: 3}
- AI State Machine:
  ```
  INITIAL: RELOAD_MOVE
  RELOAD_MOVE -> FIRE_MOVE -> RELOAD_MOVE (alternating)
  ```
- Moves:
  - FIRE_MOVE: Deal FireDamage [SingleAttack]
  - RELOAD_MOVE: Gain 3 Block [Defend]

---

### TrackerRubyRaider
- ID: TRACKER_RUBY_RAIDER
- Type: Normal (part of RubyRaidersNormal)
- HP: 21-25 (ascension: 22-26)
- Damage Values: {Hounds: 1 (1) x 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: TRACK_MOVE
  TRACK_MOVE -> HOUNDS_MOVE -> HOUNDS_MOVE (repeat)
  ```
- Moves:
  - TRACK_MOVE: Apply 2 Frail to targets [Debuff]
  - HOUNDS_MOVE: Deal HoundsDamage x HoundsRepeat [MultiAttack]

---

### OvergrowthCrawlers (encounter name for mixed weak monsters from act 1)

---

### Elite Encounters

---

### BygoneEffigy
- ID: BYGONE_EFFIGY
- Type: Elite
- HP: 127 (ascension: 132)
- Damage Values: {Slash: 15 (17)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: INITIAL_SLEEP_MOVE
  INITIAL_SLEEP_MOVE -> WAKE_MOVE -> SLASHES_MOVE -> SLASHES_MOVE (repeat)
  SLEEP_MOVE -> SLASHES_MOVE
  ```
- Moves:
  - INITIAL_SLEEP_MOVE: Does nothing, displays speak line [Sleep]
  - WAKE_MOVE: Gain 10 Strength [Buff]
  - SLEEP_MOVE: Does nothing [Sleep]
  - SLASHES_MOVE: Deal SlashDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Slow power. Sleeps turn 1, wakes turn 2 with +10 Str, then attacks forever

---

### Byrdonis
- ID: BYRDONIS
- Type: Elite
- HP: 91-94 (ascension: 99)
- Damage Values: {Peck: 3 (4) x 3, Swoop: 16 (18)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SWOOP_MOVE
  SWOOP_MOVE -> PECK_MOVE -> SWOOP_MOVE (alternating)
  ```
- Moves:
  - PECK_MOVE: Deal PeckDamage x PeckRepeat [MultiAttack]
  - SWOOP_MOVE: Deal SwoopDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Territorial power

---

### PhrogParasite
- ID: PHROG_PARASITE
- Type: Elite
- HP: 80-83 (ascension: 83-86)
- Damage Values: {Lunge: 12 (14), Bite: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: LUNGE_MOVE
  LUNGE_MOVE -> INFEST_MOVE -> BITE_MOVE -> LUNGE_MOVE (cycle)
  ```
- Moves:
  - LUNGE_MOVE: Deal LungeDamage [SingleAttack]
  - INFEST_MOVE: Add 2 Parasite cards to targets' draw pile [Status]
  - BITE_MOVE: Deal BiteDamage x3 [MultiAttack]
- Special: AfterAddedToRoom: applies CurlUpPower(10/12 asc) to self

---

### Boss Encounters

---

### Vantom
- ID: VANTOM
- Type: Boss
- HP: 206 (ascension: 216)
- Damage Values: {Chomp: 9 (11), GhastlySmash: 22 (25), Wail: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONSUME_MOVE
  CONSUME_MOVE -> RAND(Chomp maxRepeat2, GhastlySmash maxRepeat2, Wail maxRepeat2)
  All -> RAND
  ```
- Moves:
  - CONSUME_MOVE: Summon 2 Parafright minions [Summon + Buff]
  - CHOMP_MOVE: Deal ChompDamage x2 [MultiAttack]
  - GHASTLY_SMASH_MOVE: Deal GhastlySmashDamage [SingleAttack]
  - WAIL_MOVE: Deal WailDamage x3 + apply debuffs [MultiAttack + Debuff]
- Special: AfterAddedToRoom: applies PhaseShift power. Summons Parafrights.

---

### Parafright
- ID: PARAFRIGHT
- Type: Minion (summoned by Vantom)
- HP: 1 (fixed)
- Damage Values: {Haunt: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: HAUNT_MOVE
  HAUNT_MOVE -> HAUNT_MOVE (repeat)
  ```
- Moves:
  - HAUNT_MOVE: Deal HauntDamage + apply 1 Frail [SingleAttack + Debuff]
- Special: Has Illusion power. Minion.

---

### CeremonialBeast
- ID: CEREMONIAL_BEAST
- Type: Boss
- HP: 252 (ascension: 262)
- Damage Values: {Plow: 18 (20), Stomp: 15 (17), Crush: 17 (19)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: STAMP_MOVE
  Phase 1:
    STAMP_MOVE -> PLOW_MOVE -> PLOW_MOVE (repeat until Plow broken)
  Phase 2 (after stun):
    STUN_MOVE -> BEAST_CRY_MOVE -> STOMP_MOVE -> CRUSH_MOVE -> BEAST_CRY_MOVE (cycle)
  ```
- Moves:
  - STAMP_MOVE: Apply PlowPower(150/160 asc) to self [Buff]
  - PLOW_MOVE: Deal PlowDamage + gain 2 Strength [SingleAttack + Buff]
  - STUN_MOVE: Stunned (does nothing, MustPerformOnce) [Stun]
  - BEAST_CRY_MOVE: Apply 1 Ringing to targets [Debuff]
  - STOMP_MOVE: Deal StompDamage [SingleAttack]
  - CRUSH_MOVE: Deal CrushDamage + gain 3(4 asc) Strength [SingleAttack + Buff]
- Special: Phase 1: gains Plow (like block). When Plow breaks, becomes stunned. Phase 2: cycles Beast Cry -> Stomp -> Crush -> Beast Cry

---

### KinPriest
- ID: KIN_PRIEST
- Type: Boss (part of TheKinBoss)
- HP: 119 (ascension: 125)
- Damage Values: {SmiteDamage: 22 (24)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONVERSION_MOVE
  CONVERSION_MOVE -> SMITE_MOVE -> SMITE_MOVE (repeat)
  ```
- Moves:
  - CONVERSION_MOVE: Gain 4 Ritual [Buff]
  - SMITE_MOVE: Deal SmiteDamage [SingleAttack]
- Special: AfterAddedToRoom: applies DarkArtsPower and MarkOfTheKinPower

---

### KinFollower
- ID: KIN_FOLLOWER
- Type: Boss (part of TheKinBoss)
- HP: 65-71 (ascension: 67-73)
- Damage Values: {Bash: 10 (11), Bite: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONDITIONAL
    if slot == "first": BASH_MOVE
    if slot == "second": BASH_MOVE
    if slot == "third": BITE_MOVE
    else: BASH_MOVE
  BASH_MOVE -> BITE_MOVE -> BASH_MOVE (alternating)
  BITE_MOVE -> BASH_MOVE -> BITE_MOVE (alternating)
  ```
- Moves:
  - BASH_MOVE: Deal BashDamage [SingleAttack]
  - BITE_MOVE: Deal BiteDamage x2 + apply 1 Weak [MultiAttack + Debuff]
- Special: AfterAddedToRoom: applies MarkOfTheKinPower

---

## Act 2 - Hive

Encounters: BowlbugsWeak, ExoskeletonsWeak, ThievingHopperWeak, TunnelerWeak, BowlbugsNormal, ChompersNormal, ExoskeletonsNormal, HunterKillerNormal, LouseProgenitorNormal, MytesNormal, OvicopterNormal, SlumberingBeetleNormal, SpinyToadNormal, TheObscuraNormal, TunnelerNormal, DecimillipedeElite, EntomancerElite, InfestedPrismsElite, KaiserCrabBoss, KnowledgeDemonBoss, TheInsatiableBoss.

### Weak Encounters

---

### ThievingHopper
- ID: THIEVING_HOPPER
- Type: Normal (Weak encounter)
- HP: 15-18 (ascension: 16-19)
- Damage Values: {Mug: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: MUG_MOVE
  MUG_MOVE -> MUG_MOVE (repeat)
  ```
- Moves:
  - MUG_MOVE: Deal MugDamage + steal gold [SingleAttack]
- Special: AfterAddedToRoom: applies ThieveryPower(15). Steals gold on each attack.

---

### Tunneler
- ID: TUNNELER
- Type: Normal (Weak and Normal encounters)
- HP: 24-28 (ascension: 26-29)
- Damage Values: {Bite: 7 (8)}
- Block Values: {Burrow: 8 (9)}
- AI State Machine:
  ```
  INITIAL: BURROW_MOVE
  BURROW_MOVE -> BITE_MOVE -> BURROW_MOVE (alternating)
  ```
- Moves:
  - BURROW_MOVE: Gain BurrowBlock [Defend]
  - BITE_MOVE: Deal BiteDamage [SingleAttack]

---

### BowlbugEgg
- ID: BOWLBUG_EGG
- Type: Normal (part of Bowlbugs encounter)
- HP: 21-22 (ascension: 23-24)
- Damage Values: {Bite: 7 (8)}
- Block Values: {Bite: 7 (8)}
- AI State Machine:
  ```
  INITIAL: BITE_MOVE
  BITE_MOVE -> BITE_MOVE (repeat)
  ```
- Moves:
  - BITE_MOVE: Deal BiteDamage + gain ProtectBlock [SingleAttack + Defend]

---

### BowlbugNectar
- ID: BOWLBUG_NECTAR
- Type: Normal (part of Bowlbugs encounter)
- HP: 35-38 (ascension: 36-39)
- Damage Values: {Thrash: 3}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: THRASH_MOVE
  THRASH_MOVE -> BUFF_MOVE -> THRASH2_MOVE -> THRASH2_MOVE (repeat)
  ```
- Moves:
  - THRASH_MOVE: Deal 3 damage [SingleAttack]
  - BUFF_MOVE: Gain 15(16 asc) Strength [Buff]
  - THRASH2_MOVE: Deal 3 damage (with huge strength) [SingleAttack]

---

### BowlbugRock
- ID: BOWLBUG_ROCK
- Type: Normal (part of Bowlbugs encounter)
- HP: 45-48 (ascension: 46-49)
- Damage Values: {Headbutt: 15 (16)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: HEADBUTT_MOVE
  HEADBUTT_MOVE -> CONDITIONAL:
    if IsOffBalance: DIZZY_MOVE -> HEADBUTT_MOVE
    else: HEADBUTT_MOVE (repeat)
  ```
- Moves:
  - HEADBUTT_MOVE: Deal HeadbuttDamage. If IsOffBalance, becomes stunned [SingleAttack]
  - DIZZY_MOVE: Recover from stun [Stun]
- Special: AfterAddedToRoom: applies Imbalanced power. Gets stunned after attacking if off balance.

---

### BowlbugSilk
- ID: BOWLBUG_SILK
- Type: Normal (part of Bowlbugs encounter)
- HP: 40-43 (ascension: 41-44)
- Damage Values: {Thrash: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: TOXIC_SPIT_MOVE
  TOXIC_SPIT_MOVE -> THRASH_MOVE -> TOXIC_SPIT_MOVE (alternating)
  ```
- Moves:
  - THRASH_MOVE: Deal ThrashDamage x2 [MultiAttack]
  - TOXIC_SPIT_MOVE: Apply 1 Weak to targets [Debuff]

---

### Exoskeleton
- ID: EXOSKELETON
- Type: Normal (Weak and Normal encounters)
- HP: 24-28 (ascension: 25-29)
- Damage Values: {Skitter: 1 x 3(4 asc), Mandibles: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONDITIONAL by slot:
    "first": SKITTER_MOVE
    "second": MANDIBLE_MOVE
    "third": ENRAGE_MOVE
    "fourth": RAND
  SKITTER_MOVE -> RAND(Skitter CannotRepeat, Mandible CannotRepeat)
  MANDIBLE_MOVE -> ENRAGE_MOVE -> RAND
  ```
- Moves:
  - SKITTER_MOVE: Deal 1 x SkitterRepeats [MultiAttack]
  - MANDIBLE_MOVE: Deal MandiblesDamage [SingleAttack]
  - ENRAGE_MOVE: Gain 2 Strength [Buff]
- Special: AfterAddedToRoom: applies HardToKill(9). Starting move depends on slot position.

---

### Normal Encounters

---

### Chomper
- ID: CHOMPER
- Type: Normal
- HP: 60-64 (ascension: 63-67)
- Damage Values: {Clamp: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CLAMP_MOVE (or SCREECH_MOVE if ScreamFirst=true)
  CLAMP_MOVE -> SCREECH_MOVE -> CLAMP_MOVE (alternating)
  ```
- Moves:
  - CLAMP_MOVE: Deal ClampDamage x2 [MultiAttack]
  - SCREECH_MOVE: Add 3 Dazed to targets' discard pile [Status]
- Special: AfterAddedToRoom: applies 2 Artifact. ScreamFirst flag varies per encounter setup.

---

### HunterKiller
- ID: HUNTER_KILLER
- Type: Normal
- HP: 60-65 (ascension: 63-68)
- Damage Values: {Hunt: 4 (5), Kill: 15 (17)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: HUNT_MOVE
  HUNT_MOVE -> KILL_MOVE -> HUNT_MOVE (alternating)
  ```
- Moves:
  - HUNT_MOVE: Deal HuntDamage x2 + apply 1 Vulnerable [MultiAttack + Debuff]
  - KILL_MOVE: Deal KillDamage [SingleAttack]
- Special: AfterAddedToRoom: applies CamouflagePower(1)

---

### LouseProgenitor
- ID: LOUSE_PROGENITOR
- Type: Normal
- HP: 52-56 (ascension: 54-58)
- Damage Values: {Scratch: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SPAWN_MOVE
  SPAWN_MOVE -> SCRATCH_MOVE -> SCRATCH_MOVE (repeat)
  ```
- Moves:
  - SPAWN_MOVE: Summon 2 Wrigglers [Summon]
  - SCRATCH_MOVE: Deal ScratchDamage [SingleAttack]
- Special: Summons Wriggler minions

---

### Wriggler
- ID: WRIGGLER
- Type: Minion (summoned by LouseProgenitor)
- HP: 8-10 (ascension: 9-11)
- Damage Values: {Bite: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: BITE_MOVE
  BITE_MOVE -> BITE_MOVE (repeat)
  ```
- Moves:
  - BITE_MOVE: Deal BiteDamage [SingleAttack]
- Special: AfterAddedToRoom: applies MinionPower

---

### Myte
- ID: MYTE
- Type: Normal
- HP: 22-26 (ascension: 24-28)
- Damage Values: {Bite: 6 (7), Infest: 5 (6)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAND(Bite CannotRepeat, Infest CannotRepeat)
  Both -> RAND
  ```
- Moves:
  - BITE_MOVE: Deal BiteDamage [SingleAttack]
  - INFEST_MOVE: Deal InfestDamage + add 1 Parasite to draw pile [SingleAttack + Status]
- Special: AfterAddedToRoom: applies CurlUpPower(6/7 asc)

---

### Ovicopter
- ID: OVICOPTER
- Type: Normal
- HP: 67-72 (ascension: 70-75)
- Damage Values: {Dive: 14 (16)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: LAY_EGGS_MOVE
  LAY_EGGS_MOVE -> DIVE_MOVE -> LAY_EGGS_MOVE (alternating)
  ```
- Moves:
  - LAY_EGGS_MOVE: Summon ToughEgg [Summon]
  - DIVE_MOVE: Deal DiveDamage [SingleAttack]
- Special: Summons ToughEgg minions

---

### ToughEgg
- ID: TOUGH_EGG
- Type: Minion (summoned by Ovicopter)
- HP: 10-12 (ascension: 11-13)
- Damage Values: none
- Block Values: none
- AI State Machine:
  ```
  INITIAL: WAIT_MOVE
  WAIT_MOVE -> WAIT_MOVE (repeat)
  ```
- Moves:
  - WAIT_MOVE: Does nothing [Hidden intent]
- Special: AfterAddedToRoom: applies MinionPower and HatchPower. If not killed, hatches into a stronger enemy.

---

### SlumberingBeetle
- ID: SLUMBERING_BEETLE
- Type: Normal
- HP: 66-70 (ascension: 69-73)
- Damage Values: {Gore: 9 (10)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLEEP_MOVE
  SLEEP_MOVE -> CONDITIONAL:
    if awakened: GORE_MOVE -> GORE_MOVE (repeat)
    else: SLEEP_MOVE (repeat)
  ```
- Moves:
  - SLEEP_MOVE: Does nothing [Sleep]
  - GORE_MOVE: Deal GoreDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Dormant power. Wakes up when attacked. On wake: gains 6(8 asc) Strength.

---

### SpinyToad
- ID: SPINY_TOAD
- Type: Normal
- HP: 116-119 (ascension: 121-124)
- Damage Values: {Lash: 17 (19), Explosion: 23 (25)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: LASH_MOVE
  LASH_MOVE -> SPINES_MOVE -> LASH2_MOVE -> EXPLOSION_MOVE -> LASH_MOVE (cycle)
  ```
- Moves:
  - LASH_MOVE: Deal LashDamage [SingleAttack]
  - SPINES_MOVE: Gain SpinesAmount(8/10 asc) Thorns [Buff]
  - LASH2_MOVE: Deal LashDamage [SingleAttack]
  - EXPLOSION_MOVE: Deal ExplosionDamage + lose all Thorns [SingleAttack]
- Special: Cycles through gaining thorns then exploding to remove them

---

### TheObscura
- ID: THE_OBSCURA
- Type: Normal
- HP: 36-39 (ascension: 38-41)
- Damage Values: {Chomp: 5 (6) x2, Strike: 13 (15)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: DARKNESS_MOVE
  DARKNESS_MOVE -> RAND(Chomp 2, Strike 2; maxRepeat 2) -> RAND (loop)
  ```
- Moves:
  - DARKNESS_MOVE: Apply 2 Frail + 2 Weak to targets [Debuff]
  - CHOMP_MOVE: Deal ChompDamage x2 [MultiAttack]
  - STRIKE_MOVE: Deal StrikeDamage [SingleAttack]
- Special: Starts with Darkness debuff then random attacks

---

### Elite Encounters

---

### DecimillipedeSegment (Front/Middle/Back)
- ID: DECIMILLIPEDE_SEGMENT
- Type: Elite
- HP: 42-48 (ascension: 48-56)
- Damage Values: {Writhe: 5 (6) x2, Constrict: 8 (9), Bulk: 6 (7)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: varies by StarterMoveIdx (0: Writhe, 1: Bulk, 2: Constrict)
  Normal cycle: WRITHE -> CONSTRICT -> BULK -> WRITHE (cycle)
  After death+reattach: DEAD_MOVE -> REATTACH_MOVE -> RAND(Writhe/Bulk/Constrict CannotRepeat)
  ```
- Moves:
  - WRITHE_MOVE: Deal WritheDamage x2 [MultiAttack]
  - BULK_MOVE: Deal BulkDamage + gain 2 Strength [SingleAttack + Buff]
  - CONSTRICT_MOVE: Deal ConstrictDamage + apply 1 Weak [SingleAttack + Debuff]
  - DEAD_MOVE: Does nothing
  - REATTACH_MOVE: Heal via Reattach power [Heal] (MustPerformOnce)
- Special: AfterAddedToRoom: applies Reattach(25) power. 3 segments (Front, Middle, Back) share same logic. Each has unique HP to prevent confusion. Can revive after death.

---

### Entomancer
- ID: ENTOMANCER
- Type: Elite
- HP: 145 (ascension: 155)
- Damage Values: {Spear: 18 (20), Bees: 3 (3) x 7(8 asc)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: BEES_MOVE
  BEES_MOVE -> SPEAR_MOVE -> PHEROMONE_SPIT_MOVE -> BEES_MOVE (cycle)
  ```
- Moves:
  - BEES_MOVE: Deal BeesDamage x BeesRepeat [MultiAttack]
  - SPEAR_MOVE: Deal SpearMoveDamage [SingleAttack]
  - PHEROMONE_SPIT_MOVE: +1 PersonalHive (if <3) or +2 Str (if >=3) [Buff]
- Special: AfterAddedToRoom: applies PersonalHive(1) power. Gains Strength scaling.

---

### InfestedPrism
- ID: INFESTED_PRISM
- Type: Elite
- HP: 40-45 (ascension: 42-47)
- Damage Values: {LaserBeam: 8 (9), InfestedLaser: 10 (12)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONDITIONAL by slot:
    "first": LASER_BEAM
    "second": INFESTED_LASER
    else: RAND
  LASER_BEAM -> INFESTED_LASER -> RAND(LaserBeam CannotRepeat, InfestedLaser CannotRepeat)
  INFESTED_LASER -> LASER_BEAM -> RAND
  ```
- Moves:
  - LASER_BEAM_MOVE: Deal LaserBeamDamage [SingleAttack]
  - INFESTED_LASER_MOVE: Deal InfestedLaserDamage + add 1 Parasite to draw pile [SingleAttack + Status]
- Special: AfterAddedToRoom: applies ShiftingPower(1) - cycles through resistances

---

### Boss Encounters

---

### TheInsatiable
- ID: THE_INSATIABLE
- Type: Boss
- HP: 242 (ascension: 256)
- Damage Values: {Chomp: 17 (19), DevouringMaw: 7 (8) x3, AcidBlast: 23 (27)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CHOMP_MOVE
  Phase cycle: CHOMP -> DEVOURING_MAW -> ACID_BLAST -> CHOMP (repeat cycle)
  At half HP: EVOLVE_MOVE interrupts, then enhanced cycle
  ```
- Moves:
  - CHOMP_MOVE: Deal ChompDamage [SingleAttack]
  - DEVOURING_MAW_MOVE: Deal DevouringMawDamage x3 [MultiAttack]
  - ACID_BLAST_MOVE: Deal AcidBlastDamage [SingleAttack]
  - EVOLVE_MOVE: Gain large Strength + heal [Buff]
- Special: AfterAddedToRoom: applies InsatiableHunger power. Phase transition at 50% HP.

---

### KnowledgeDemon
- ID: KNOWLEDGE_DEMON
- Type: Boss
- HP: 286 (ascension: 300)
- Damage Values: {DarkBlast: 18 (20), MindSpike: 6 (7) x3, Annihilate: 40 (45)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: FORBIDDEN_TOME_MOVE
  FORBIDDEN_TOME -> DARK_BLAST -> MIND_SPIKE -> ANNIHILATE -> FORBIDDEN_TOME (cycle)
  ```
- Moves:
  - FORBIDDEN_TOME_MOVE: Add status cards to player + gain Strength [Buff + Status]
  - DARK_BLAST_MOVE: Deal DarkBlastDamage [SingleAttack]
  - MIND_SPIKE_MOVE: Deal MindSpikeDamage x3 [MultiAttack]
  - ANNIHILATE_MOVE: Deal AnnihilateDamage [SingleAttack]
- Special: Cycles through a fixed pattern. Adds Dazed/Void status cards.

---

### Crusher (Kaiser Crab Left Arm)
- ID: CRUSHER
- Type: Boss (part of KaiserCrabBoss)
- HP: 199 (ascension: 209)
- Damage Values: {Thrash: 12 (14), EnlargingStrike: 4 (4), BugSting: 6 (7) x2, GuardedStrike: 12 (14)}
- Block Values: {GuardedStrike: 18}
- AI State Machine:
  ```
  INITIAL: THRASH_MOVE
  THRASH -> ENLARGING_STRIKE -> BUG_STING -> ADAPT -> GUARDED_STRIKE -> THRASH (cycle)
  ```
- Moves:
  - THRASH_MOVE: Deal ThrashDamage [SingleAttack]
  - ENLARGING_STRIKE_MOVE: Deal EnlargingStrikeDamage [SingleAttack]
  - BUG_STING_MOVE: Deal BugStingDamage x2 + apply 2 Weak + 2 Frail [MultiAttack + Debuff]
  - ADAPT_MOVE: Gain 2 Strength [Buff]
  - GUARDED_STRIKE_MOVE: Deal GuardedStrikeDamage + gain 18 Block [SingleAttack + Defend]
- Special: AfterAddedToRoom: applies BackAttackLeft + CrabRage powers. Part of Kaiser Crab boss (background creature).

---

### Rocket (Kaiser Crab Right Arm)
- ID: ROCKET
- Type: Boss (part of KaiserCrabBoss)
- HP: 189 (ascension: 199)
- Damage Values: {TargetingReticle: 3 (4), PrecisionBeam: 18 (20), Laser: 31 (35)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: TARGETING_RETICLE_MOVE
  TARGETING_RETICLE -> PRECISION_BEAM -> LASER -> TARGETING_RETICLE (cycle)
  ```
- Moves:
  - TARGETING_RETICLE_MOVE: Deal TargetingReticleDamage + apply 2 Vulnerable [SingleAttack + Debuff]
  - PRECISION_BEAM_MOVE: Deal PrecisionBeamDamage [SingleAttack]
  - LASER_MOVE: Deal LaserDamage [SingleAttack]
- Special: AfterAddedToRoom: applies BackAttackRight + CrabRage powers. Part of Kaiser Crab boss.

---

## Act 3 - Glory

Encounters: DevotedSculptorWeak, ScrollsOfBitingWeak, TurretOperatorWeak, AxebotsNormal, ConstructMenagerieNormal, FabricatorNormal, FrogKnightNormal, GlobeHeadNormal, OwlMagistrateNormal, ScrollsOfBitingNormal, SlimedBerserkerNormal, TheLostAndForgottenNormal, KnightsElite, MechaKnightElite, SoulNexusElite, DoormakerBoss, QueenBoss, TestSubjectBoss.

### Weak Encounters

---

### DevotedSculptor
- ID: DEVOTED_SCULPTOR
- Type: Normal (Weak encounter)
- HP: 162 (ascension: 172)
- Damage Values: {Savage: 12 (15)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: FORBIDDEN_INCANTATION_MOVE
  FORBIDDEN_INCANTATION_MOVE -> SAVAGE_MOVE -> SAVAGE_MOVE (repeat)
  ```
- Moves:
  - FORBIDDEN_INCANTATION_MOVE: Gain 9 Ritual [Buff]
  - SAVAGE_MOVE: Deal SavageDamage [SingleAttack]
- Special: High Ritual gain makes this dangerous if not killed quickly

---

### ScrollOfBiting
- ID: SCROLL_OF_BITING
- Type: Normal (Weak and Normal encounters)
- HP: 24-26 (ascension: 26-28)
- Damage Values: {Bite: 7 (8)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: BITE_MOVE
  BITE_MOVE -> BITE_MOVE (repeat)
  ```
- Moves:
  - BITE_MOVE: Deal BiteDamage [SingleAttack]
- Special: AfterAddedToRoom: applies FlyPower(1)

---

### TurretOperator
- ID: TURRET_OPERATOR
- Type: Normal (Weak encounter)
- HP: 28-30 (ascension: 30-32)
- Damage Values: {Shoot: 9 (10)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: DEPLOY_TURRET_MOVE
  DEPLOY_TURRET_MOVE -> SHOOT_MOVE -> SHOOT_MOVE (repeat)
  ```
- Moves:
  - DEPLOY_TURRET_MOVE: Summon turret/apply power [Buff]
  - SHOOT_MOVE: Deal ShootDamage [SingleAttack]
- Special: Deploys turret on first turn

---

### Normal Encounters

---

### Axebot
- ID: AXEBOT
- Type: Normal
- HP: 40-44 (ascension: 42-46)
- Damage Values: {OneTwo: 5 (6) x2, HammerUppercut: 8 (10)}
- Block Values: {BootUp: 10}
- AI State Machine:
  ```
  INITIAL: RAND (or BOOT_UP if spawned with stock override)
  BOOT_UP_MOVE -> RAND
  RAND: weighted branches:
    ONE_TWO_MOVE (weight 2)
    SHARPEN_MOVE (CannotRepeat)
    HAMMER_UPPERCUT_MOVE (weight 2)
  All -> RAND
  ```
- Moves:
  - BOOT_UP_MOVE: Gain 10 Block + 1 Strength [Defend + Buff]
  - ONE_TWO_MOVE: Deal OneTwoDamage x2 [MultiAttack]
  - SHARPEN_MOVE: Gain 4 Strength [Buff]
  - HAMMER_UPPERCUT_MOVE: Deal HammerUppercutDamage + apply 1 Weak + 1 Frail [SingleAttack + Debuff]
- Special: AfterAddedToRoom: applies Stock(2) power. If spawned by Fabricator, starts with BOOT_UP.

---

### Fabricator
- ID: FABRICATOR
- Type: Normal
- HP: 150 (ascension: 155)
- Damage Values: {FabricatingStrike: 18 (21), Disintegrate: 11 (13)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: fabricateBranch CONDITIONAL:
    if CanFabricate (<4 alive allies): RAND(Fabricate, FabricatingStrike; equal weight)
    else: DISINTEGRATE_MOVE
  All -> fabricateBranch
  ```
- Moves:
  - FABRICATE_MOVE: Spawn 1 defense bot + 1 aggro bot [Summon]
  - FABRICATING_STRIKE_MOVE: Deal FabricatingStrikeDamage + spawn 1 aggro bot [SingleAttack + Summon]
  - DISINTEGRATE_MOVE: Deal DisintegrateDamage [SingleAttack]
- Special: Summons from pools: Aggro={Zapbot, Stabbot}, Defense={Guardbot, Noisebot}. Won't repeat last spawned type. Stops fabricating at 4 allies.

---

### Zapbot
- ID: ZAPBOT
- Type: Minion (spawned by Fabricator)
- HP: 23-28 (ascension: 24-29)
- Damage Values: {Zap: 14 (15)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: ZAP
  ZAP -> ZAP (repeat)
  ```
- Moves:
  - ZAP: Deal ZapDamage [SingleAttack]
- Special: AfterAddedToRoom: applies HighVoltage(2) power

---

### Stabbot
- ID: STABBOT
- Type: Minion (spawned by Fabricator)
- HP: 23-28 (ascension: 24-29)
- Damage Values: {Stab: 3 (4) x3}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: STAB_MOVE
  STAB_MOVE -> STAB_MOVE (repeat)
  ```
- Moves:
  - STAB_MOVE: Deal StabDamage x3 [MultiAttack]

---

### Guardbot
- ID: GUARDBOT
- Type: Minion (spawned by Fabricator)
- HP: 21-25 (ascension: 22-26)
- Damage Values: none
- Block Values: {Guard: variable (gives block to Fabricator)}
- AI State Machine:
  ```
  INITIAL: GUARD_MOVE
  GUARD_MOVE -> GUARD_MOVE (repeat)
  ```
- Moves:
  - GUARD_MOVE: Give block to all allied Fabricators [Defend]

---

### Noisebot
- ID: NOISEBOT
- Type: Minion (spawned by Fabricator)
- HP: 23-28 (ascension: 24-29)
- Damage Values: none
- Block Values: none
- AI State Machine:
  ```
  INITIAL: NOISE_MOVE
  NOISE_MOVE -> NOISE_MOVE (repeat)
  ```
- Moves:
  - NOISE_MOVE: Add 2 Dazed (1 to discard, 1 to random draw) [Status]

---

### FrogKnight
- ID: FROG_KNIGHT
- Type: Normal
- HP: 191 (ascension: 199)
- Damage Values: {StrikeDownEvil: 21 (23), TongueLash: 13 (14), BeetleCharge: 35 (40)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: TONGUE_LASH
  TONGUE_LASH -> STRIKE_DOWN_EVIL -> FOR_THE_QUEEN -> CONDITIONAL:
    if HP >= 50% or already charged: TONGUE_LASH
    if HP < 50% and not charged: BEETLE_CHARGE -> TONGUE_LASH
  ```
- Moves:
  - TONGUE_LASH: Deal TongueLashDamage + apply 2 Frail [SingleAttack + Debuff]
  - STRIKE_DOWN_EVIL: Deal StrikeDownEvilDamage [SingleAttack]
  - FOR_THE_QUEEN: Gain 5 Strength [Buff]
  - BEETLE_CHARGE: Deal BeetleChargeDamage (one-time) [SingleAttack]
- Special: AfterAddedToRoom: applies Plating(15/19 asc). Beetle Charge triggers once when below 50% HP.

---

### GlobeHead
- ID: GLOBE_HEAD
- Type: Normal
- HP: 148 (ascension: 158)
- Damage Values: {ThunderStrike: 6 (7) x3, ShockingSlap: 13 (14), GalvanicBurst: 16 (17)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SHOCKING_SLAP
  SHOCKING_SLAP -> THUNDER_STRIKE -> GALVANIC_BURST -> SHOCKING_SLAP (cycle)
  ```
- Moves:
  - SHOCKING_SLAP: Deal ShockingSlapDamage + apply 2 Frail [SingleAttack + Debuff]
  - THUNDER_STRIKE: Deal ThunderStrikeDamage x3 [MultiAttack]
  - GALVANIC_BURST: Deal GalvanicBurstDamage + gain 2 Strength [SingleAttack + Buff]
- Special: AfterAddedToRoom: applies Galvanic(6) power

---

### OwlMagistrate
- ID: OWL_MAGISTRATE
- Type: Normal
- HP: 82 (ascension: 86)
- Damage Values: {Judgement: 10 (12), Sentence: 16 (18)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: JUDGEMENT_MOVE
  JUDGEMENT_MOVE -> SENTENCE_MOVE -> JUDGEMENT_MOVE (alternating)
  ```
- Moves:
  - JUDGEMENT_MOVE: Deal JudgementDamage + apply debuff [SingleAttack + Debuff]
  - SENTENCE_MOVE: Deal SentenceDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Verdict power

---

### SlimedBerserker
- ID: SLIMED_BERSERKER
- Type: Normal
- HP: 60-65 (ascension: 64-69)
- Damage Values: {Slash: 8 (9), Slam: 14 (16)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLASH_MOVE
  SLASH_MOVE -> SLAM_MOVE -> ENRAGE_MOVE -> SLASH_MOVE (cycle)
  ```
- Moves:
  - SLASH_MOVE: Deal SlashDamage [SingleAttack]
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]
  - ENRAGE_MOVE: Gain Strength [Buff]
- Special: AfterAddedToRoom: applies Berserk power

---

### TheLost
- ID: THE_LOST
- Type: Normal (part of TheLostAndForgotten encounter)
- HP: 50-54 (ascension: 53-57)
- Damage Values: {DarkSlash: 9 (10), ShadowStrike: 13 (15)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: DARK_SLASH_MOVE
  DARK_SLASH -> SHADOW_STRIKE -> DARK_SLASH (alternating)
  ```
- Moves:
  - DARK_SLASH_MOVE: Deal DarkSlashDamage [SingleAttack]
  - SHADOW_STRIKE_MOVE: Deal ShadowStrikeDamage [SingleAttack]

---

### TheForgotten
- ID: THE_FORGOTTEN
- Type: Normal (part of TheLostAndForgotten encounter)
- HP: 50-54 (ascension: 53-57)
- Damage Values: {Haunt: 7 (8), WailOfSorrow: 11 (13)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: HAUNT_MOVE
  HAUNT -> WAIL_OF_SORROW -> HAUNT (alternating)
  ```
- Moves:
  - HAUNT_MOVE: Deal HauntDamage + apply debuff [SingleAttack + Debuff]
  - WAIL_OF_SORROW_MOVE: Deal WailOfSorrowDamage [SingleAttack]

---

### Elite Encounters

---

### FlailKnight
- ID: FLAIL_KNIGHT
- Type: Elite (part of KnightsElite)
- HP: 101 (ascension: 108)
- Damage Values: {Flail: 9 (10) x2, Ram: 15 (17)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAM_MOVE
  All -> RAND:
    WAR_CHANT (CannotRepeat)
    FLAIL_MOVE (weight 2)
    RAM_MOVE (weight 2)
  ```
- Moves:
  - WAR_CHANT: Gain 3 Strength [Buff]
  - FLAIL_MOVE: Deal FlailDamage x2 [MultiAttack]
  - RAM_MOVE: Deal RamDamage [SingleAttack]

---

### MagiKnight
- ID: MAGI_KNIGHT
- Type: Elite (part of KnightsElite)
- HP: 101 (ascension: 108)
- Damage Values: {MagicMissile: 5 (6) x3, ArcaneBlast: 15 (17)}
- Block Values: {ArcaneSiege: 12 (14)}
- AI State Machine:
  ```
  INITIAL: MAGIC_MISSILE_MOVE
  All -> RAND:
    MAGIC_MISSILE (CannotRepeat)
    ARCANE_BLAST (weight 2)
    ARCANE_SIEGE (CannotRepeat)
  ```
- Moves:
  - MAGIC_MISSILE_MOVE: Deal MagicMissileDamage x3 [MultiAttack]
  - ARCANE_BLAST_MOVE: Deal ArcaneBlastDamage [SingleAttack]
  - ARCANE_SIEGE_MOVE: Deal ArcaneBlastDamage + gain Block [SingleAttack + Defend]

---

### SpectralKnight
- ID: SPECTRAL_KNIGHT
- Type: Elite (part of KnightsElite)
- HP: 101 (ascension: 108)
- Damage Values: {GhostSlash: 12 (14), PhantomRush: 5 (6) x2}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: GHOST_SLASH_MOVE
  All -> RAND:
    GHOST_SLASH (CannotRepeat)
    PHANTOM_RUSH (weight 2)
    ETHEREAL_SHIFT (CannotRepeat)
  ```
- Moves:
  - GHOST_SLASH_MOVE: Deal GhostSlashDamage [SingleAttack]
  - PHANTOM_RUSH_MOVE: Deal PhantomRushDamage x2 [MultiAttack]
  - ETHEREAL_SHIFT_MOVE: Gain Intangible [Buff]

---

### MechaKnight
- ID: MECHA_KNIGHT
- Type: Elite
- HP: 155 (ascension: 165)
- Damage Values: {SteelSlash: 14 (16), MissileSalvo: 4 (5) x4, OverdriveStrike: 24 (28)}
- Block Values: {ShieldUp: 16 (18)}
- AI State Machine:
  ```
  INITIAL: STEEL_SLASH_MOVE
  STEEL_SLASH -> MISSILE_SALVO -> SHIELD_UP -> OVERDRIVE_STRIKE -> STEEL_SLASH (cycle)
  ```
- Moves:
  - STEEL_SLASH_MOVE: Deal SteelSlashDamage [SingleAttack]
  - MISSILE_SALVO_MOVE: Deal MissileSalvoDamage x4 [MultiAttack]
  - SHIELD_UP_MOVE: Gain Block + Strength [Defend + Buff]
  - OVERDRIVE_STRIKE_MOVE: Deal OverdriveStrikeDamage [SingleAttack]
- Special: Fixed 4-turn cycle

---

### SoulNexus
- ID: SOUL_NEXUS
- Type: Elite
- HP: 155 (ascension: 165)
- Damage Values: {SoulDrain: 10 (12), SpiritBarrage: 4 (5) x3}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SUMMON_MOVE
  SUMMON -> SOUL_DRAIN -> SPIRIT_BARRAGE -> SUMMON (cycle)
  ```
- Moves:
  - SUMMON_MOVE: Summon Osty minion [Summon]
  - SOUL_DRAIN_MOVE: Deal SoulDrainDamage + heal self [SingleAttack + Heal]
  - SPIRIT_BARRAGE_MOVE: Deal SpiritBarrageDamage x3 [MultiAttack]
- Special: Summons Osty minions periodically

---

### Osty
- ID: OSTY
- Type: Minion (summoned by SoulNexus)
- HP: 1 (fixed)
- Damage Values: {Haunt: 4 (5)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: HAUNT_MOVE
  HAUNT_MOVE -> HAUNT_MOVE (repeat)
  ```
- Moves:
  - HAUNT_MOVE: Deal HauntDamage [SingleAttack]
- Special: AfterAddedToRoom: applies Illusion power + MinionPower

---

### Boss Encounters

---

### Door
- ID: DOOR
- Type: Boss (part of DoormakerBoss)
- HP: 155 (ascension: 165)
- Damage Values: {DramaticOpen: 25 (28), Enforce: 20, DoorSlam: 15 (15) x2}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: DRAMATIC_OPEN_MOVE
  DRAMATIC_OPEN -> DOOR_SLAM -> ENFORCE -> DRAMATIC_OPEN (cycle)
  DEAD_MOVE -> DEAD_MOVE (if fully dead) or -> DRAMATIC_OPEN (if revived)
  ```
- Moves:
  - DRAMATIC_OPEN_MOVE: Deal DramaticOpenDamage [SingleAttack]
  - ENFORCE_MOVE: Deal 20 damage + gain 3(4 asc) Strength [SingleAttack + Buff]
  - DOOR_SLAM_MOVE: Deal DoorSlamDamage x2 [MultiAttack]
  - DEAD_MOVE: Does nothing
- Special: AfterAddedToRoom: applies DoorRevivalPower. Creates Doormaker creature. Can be revived by Doormaker.

---

### Doormaker
- ID: DOORMAKER
- Type: Boss (part of DoormakerBoss)
- HP: 489 (ascension: 512)
- Damage Values: {LaserBeam: 31 (34), GetBackIn: 40 (45)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: WHAT_IS_IT_MOVE
  WHAT_IS_IT -> BEAM -> GET_BACK_IN -> GET_BACK_IN (repeat)
  ```
- Moves:
  - WHAT_IS_IT_MOVE: Does nothing, speaks dialogue [Stun]
  - BEAM_MOVE: Deal LaserBeamDamage [SingleAttack]
  - GET_BACK_IN_MOVE: Deal GetBackInDamage + gain 5 Str + revive Door + escape [SingleAttack + Buff + Escape]
- Special: Each time it "gets back in," Door gains +3(4 asc) Str per return count, Door HP increases by 20(25 asc) per return count. Doormaker escapes after reviving Door.

---

### Queen
- ID: QUEEN
- Type: Boss
- HP: 302 (ascension: 322)
- Damage Values: {RoyalStrike: 18 (20), Guillotine: 30 (35), RoyalGuardStrike: 15 (17)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SUMMON_GUARD
  SUMMON_GUARD -> ROYAL_STRIKE -> GUILLOTINE -> SUMMON_GUARD (cycle)
  ```
- Moves:
  - SUMMON_GUARD: Summon RoyalGuard minion [Summon]
  - ROYAL_STRIKE_MOVE: Deal RoyalStrikeDamage + debuff [SingleAttack + Debuff]
  - GUILLOTINE_MOVE: Deal GuillotineDamage [SingleAttack]
- Special: AfterAddedToRoom: applies QueenPower. Summons guards periodically.

---

### TestSubject
- ID: TEST_SUBJECT
- Type: Boss
- HP: 255 (ascension: 270)
- Damage Values: {Discharge: 11 (13) x2, Overload: 28 (32), ChainLightning: 7 (8) x4}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CHARGE_UP_MOVE
  Phase 1: CHARGE_UP -> DISCHARGE -> OVERLOAD -> CHARGE_UP (cycle)
  Phase 2 (at HP threshold): Enhanced cycle with CHAIN_LIGHTNING
  ```
- Moves:
  - CHARGE_UP_MOVE: Gain Strength + buffs [Buff]
  - DISCHARGE_MOVE: Deal DischargeDamage x2 [MultiAttack]
  - OVERLOAD_MOVE: Deal OverloadDamage [SingleAttack]
  - CHAIN_LIGHTNING_MOVE: Deal ChainLightningDamage x4 [MultiAttack]
- Special: Phase transition with enhanced attacks

---

## Act 4 - Underdocks

Encounters: CorpseSlugsWeak, SeapunkWeak, SludgeSpinnerWeak, ToadpolesWeak, CorpseSlugsNormal, CultistsNormal, FossilStalkerNormal, GremlinMercNormal, HauntedShipNormal, LivingFogNormal, PunchConstructNormal, SewerClamNormal, ToadpolesNormal, TwoTailedRatsNormal, PhantasmalGardenersElite, SkulkingColonyElite, TerrorEelElite, LagavulinMatriarchBoss, SoulFyshBoss, WaterfallGiantBoss.

### Weak Encounters

---

### CorpseSlug
- ID: CORPSE_SLUG
- Type: Normal (Weak and Normal encounters)
- HP: 25-27 (ascension: 27-29)
- Damage Values: {WhipSlap: 3 x2, Glomp: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: varies by StarterMoveIdx (0: WhipSlap, 1: Glomp, 2: Goop)
  WHIP_SLAP -> GLOMP -> GOOP -> WHIP_SLAP (cycle)
  ```
- Moves:
  - WHIP_SLAP_MOVE: Deal 3 x2 [MultiAttack]
  - GLOMP_MOVE: Deal GlompDamage [SingleAttack]
  - GOOP_MOVE: Apply 2 Frail to targets [Debuff]
- Special: AfterAddedToRoom: applies Ravenous(4/5 asc) power. Multiple corpse slugs start on different moves.

---

### Seapunk
- ID: SEAPUNK
- Type: Normal (Weak encounter)
- HP: 35-38 (ascension: 37-40)
- Damage Values: {Jab: 6 (7), PoisonSpit: 9 (10)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: JAB_MOVE
  JAB_MOVE -> POISON_SPIT_MOVE -> JAB_MOVE (alternating)
  ```
- Moves:
  - JAB_MOVE: Deal JabDamage [SingleAttack]
  - POISON_SPIT_MOVE: Deal PoisonSpitDamage + apply poison [SingleAttack + Debuff]

---

### SludgeSpinner
- ID: SLUDGE_SPINNER
- Type: Normal (Weak encounter)
- HP: 20-23 (ascension: 22-25)
- Damage Values: {SludgeBall: 6 (7)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: TOXIC_SPRAY_MOVE
  TOXIC_SPRAY -> SLUDGE_BALL -> SLUDGE_BALL (repeat)
  ```
- Moves:
  - TOXIC_SPRAY_MOVE: Apply 2 Weak to targets [Debuff]
  - SLUDGE_BALL_MOVE: Deal SludgeBallDamage [SingleAttack]

---

### Toadpole
- ID: TOADPOLE
- Type: Normal (Weak and Normal encounters)
- HP: 14-16 (ascension: 15-17)
- Damage Values: {Tongue: 5 (6), Chomp: 9 (10)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONDITIONAL by slot:
    "first": TONGUE_MOVE
    "second": CHOMP_MOVE
    else: RAND
  TONGUE -> CHOMP -> RAND(Tongue CannotRepeat, Chomp CannotRepeat)
  ```
- Moves:
  - TONGUE_MOVE: Deal TongueDamage [SingleAttack]
  - CHOMP_MOVE: Deal ChompDamage [SingleAttack]

---

### Normal Encounters

---

### CalcifiedCultist
- ID: CALCIFIED_CULTIST
- Type: Normal (part of CultistsNormal)
- HP: 38-41 (ascension: 39-42)
- Damage Values: {DarkStrike: 9 (11)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: INCANTATION_MOVE
  INCANTATION_MOVE -> DARK_STRIKE_MOVE -> DARK_STRIKE_MOVE (repeat)
  ```
- Moves:
  - INCANTATION_MOVE: Gain 2 Ritual [Buff]
  - DARK_STRIKE_MOVE: Deal DarkStrikeDamage [SingleAttack]

---

### DampCultist
- ID: DAMP_CULTIST
- Type: Normal (part of CultistsNormal)
- HP: 51-53 (ascension: 52-54)
- Damage Values: {DarkStrike: 1 (3)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: INCANTATION_MOVE
  INCANTATION_MOVE -> DARK_STRIKE_MOVE -> DARK_STRIKE_MOVE (repeat)
  ```
- Moves:
  - INCANTATION_MOVE: Gain 5(6 asc) Ritual [Buff]
  - DARK_STRIKE_MOVE: Deal DarkStrikeDamage [SingleAttack]
- Special: Low base damage but very high Ritual gain

---

### FossilStalker
- ID: FOSSIL_STALKER
- Type: Normal
- HP: 51-53 (ascension: 54-56)
- Damage Values: {Tackle: 9 (11), Latch: 12 (14), Lash: 3 (4) x2}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: LATCH_MOVE
  All -> RAND(Latch weight2, Tackle weight2, Lash weight2)
  ```
- Moves:
  - TACKLE_MOVE: Deal TackleDamage + apply 1 Frail [SingleAttack + Debuff]
  - LATCH_MOVE: Deal LatchDamage [SingleAttack]
  - LASH_MOVE: Deal LashDamage x2 [MultiAttack]
- Special: AfterAddedToRoom: applies Suck(3) power

---

### GremlinMerc
- ID: GREMLIN_MERC
- Type: Normal
- HP: 47-49 (ascension: 51-53)
- Damage Values: {Gimme: 7 (8) x2, DoubleSmash: 6 (7) x2, Hehe: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: GIMME_MOVE
  GIMME -> DOUBLE_SMASH -> HEHE -> GIMME (cycle)
  ```
- Moves:
  - GIMME_MOVE: Deal GimmeDamage x2 + steal gold [MultiAttack]
  - DOUBLE_SMASH_MOVE: Deal DoubleSmashDamage x2 + apply 2 Weak + steal gold [MultiAttack + Debuff]
  - HEHE_MOVE: Deal HeheDamage + gain 2 Strength + steal gold [SingleAttack + Buff]
- Special: AfterAddedToRoom: applies Surprise(1) + Thievery(20) per player. Steals gold on every attack.

---

### HauntedShip
- ID: HAUNTED_SHIP
- Type: Normal
- HP: 58-62 (ascension: 61-65)
- Damage Values: {Broadside: 7 (8) x2, RammingSpeed: 18 (20)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: BROADSIDE_MOVE
  BROADSIDE -> RAMMING_SPEED -> BROADSIDE (alternating)
  ```
- Moves:
  - BROADSIDE_MOVE: Deal BroadsideDamage x2 [MultiAttack]
  - RAMMING_SPEED_MOVE: Deal RammingSpeedDamage [SingleAttack]
- Special: AfterAddedToRoom: applies HauntedPower

---

### LivingFog
- ID: LIVING_FOG
- Type: Normal
- HP: 70-74 (ascension: 73-77)
- Damage Values: {Engulf: 10 (12)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SPAWN_MOVE
  SPAWN_MOVE -> ENGULF_MOVE -> SPAWN_MOVE (alternating)
  ```
- Moves:
  - SPAWN_MOVE: Summon GasBomb minion [Summon]
  - ENGULF_MOVE: Deal EngulfDamage + apply debuff [SingleAttack + Debuff]
- Special: Summons GasBomb minions

---

### GasBomb
- ID: GAS_BOMB
- Type: Minion (summoned by LivingFog)
- HP: 10 (ascension: 12)
- Damage Values: {Explode: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: EXPLODE_MOVE
  EXPLODE_MOVE (kills self after)
  ```
- Moves:
  - EXPLODE_MOVE: Deal ExplodeDamage + kill self [DeathBlow]
- Special: AfterAddedToRoom: applies MinionPower. Explodes and dies.

---

### PunchConstruct
- ID: PUNCH_CONSTRUCT
- Type: Normal
- HP: 55-59 (ascension: 58-62)
- Damage Values: {Punch: 9 (10), HeavyPunch: 17 (19)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: PUNCH_MOVE
  PUNCH -> HEAVY_PUNCH -> PUNCH (alternating)
  ```
- Moves:
  - PUNCH_MOVE: Deal PunchDamage [SingleAttack]
  - HEAVY_PUNCH_MOVE: Deal HeavyPunchDamage [SingleAttack]

---

### SewerClam
- ID: SEWER_CLAM
- Type: Normal
- HP: 50-54 (ascension: 53-57)
- Damage Values: {Snap: 8 (9), PearlSpit: 5 (6)}
- Block Values: {Shell: 10 (12)}
- AI State Machine:
  ```
  INITIAL: SHELL_MOVE
  SHELL -> SNAP -> PEARL_SPIT -> SHELL (cycle)
  ```
- Moves:
  - SHELL_MOVE: Gain ShellBlock [Defend]
  - SNAP_MOVE: Deal SnapDamage [SingleAttack]
  - PEARL_SPIT_MOVE: Deal PearlSpitDamage + apply debuff [SingleAttack + Debuff]

---

### TwoTailedRat
- ID: TWO_TAILED_RAT
- Type: Normal
- HP: 25-28 (ascension: 27-30)
- Damage Values: {Gnaw: 6 (7), TailWhip: 4 (5) x2}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CONDITIONAL by slot:
    "first": GNAW_MOVE
    "second": TAIL_WHIP_MOVE
    else: RAND
  GNAW -> TAIL_WHIP -> RAND(Gnaw CannotRepeat, TailWhip CannotRepeat)
  ```
- Moves:
  - GNAW_MOVE: Deal GnawDamage [SingleAttack]
  - TAIL_WHIP_MOVE: Deal TailWhipDamage x2 [MultiAttack]

---

### FatGremlin
- ID: FAT_GREMLIN
- Type: Minion (part of GremlinMerc encounter)
- HP: 13-17 (ascension: 14-18)
- Damage Values: none
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SPAWNED_MOVE
  SPAWNED_MOVE -> FLEE_MOVE -> FLEE_MOVE (repeat)
  ```
- Moves:
  - SPAWNED_MOVE: Wake up [Stun]
  - FLEE_MOVE: Escape from combat [Escape]
- Special: Wakes up then tries to flee. Kill before it escapes for gold reward.

---

### SneakyGremlin
- ID: SNEAKY_GREMLIN
- Type: Minion (part of GremlinMerc encounter)
- HP: 9-12 (ascension: 10-13)
- Damage Values: {Stab: 8 (9)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: STAB_MOVE
  STAB_MOVE -> STAB_MOVE (repeat)
  ```
- Moves:
  - STAB_MOVE: Deal StabDamage [SingleAttack]

---

### Elite Encounters

---

### PhantasmalGardener
- ID: PHANTASMAL_GARDENER
- Type: Elite
- HP: 60-65 (ascension: 63-68)
- Damage Values: {Prune: 10 (12), Uproot: 16 (18)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: RAND(Prune CannotRepeat, Uproot CannotRepeat)
  All -> RAND
  ```
- Moves:
  - PRUNE_MOVE: Deal PruneDamage + apply debuff [SingleAttack + Debuff]
  - UPROOT_MOVE: Deal UprootDamage [SingleAttack]
- Special: AfterAddedToRoom: applies ShadowForm power (Intangible cycling)

---

### SkulkingColony
- ID: SKULKING_COLONY
- Type: Elite
- HP: 140 (ascension: 150)
- Damage Values: {Lash: 6 (7) x3, Devour: 20 (23)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SKULK_MOVE
  SKULK -> LASH -> DEVOUR -> SKULK (cycle)
  ```
- Moves:
  - SKULK_MOVE: Apply debuffs [Debuff]
  - LASH_MOVE: Deal LashDamage x3 [MultiAttack]
  - DEVOUR_MOVE: Deal DevourDamage [SingleAttack]
- Special: AfterAddedToRoom: applies unique colony powers

---

### TerrorEel
- ID: TERROR_EEL
- Type: Elite
- HP: 130 (ascension: 140)
- Damage Values: {Shock: 12 (14), Coil: 8 (9) x2, Surge: 25 (28)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SHOCK_MOVE
  SHOCK -> COIL -> SURGE -> SHOCK (cycle)
  ```
- Moves:
  - SHOCK_MOVE: Deal ShockDamage + apply debuff [SingleAttack + Debuff]
  - COIL_MOVE: Deal CoilDamage x2 [MultiAttack]
  - SURGE_MOVE: Deal SurgeDamage [SingleAttack]
- Special: AfterAddedToRoom: applies TerrorPower

---

### Boss Encounters

---

### WaterfallGiant
- ID: WATERFALL_GIANT
- Type: Boss
- HP: 310 (ascension: 330)
- Damage Values: {Slam: 22 (25), Crush: 14 (16) x2, Torrent: 35 (40)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: ROAR_MOVE
  ROAR -> SLAM -> CRUSH -> TORRENT -> ROAR (cycle)
  ```
- Moves:
  - ROAR_MOVE: Gain Strength + apply debuffs [Buff + Debuff]
  - SLAM_MOVE: Deal SlamDamage [SingleAttack]
  - CRUSH_MOVE: Deal CrushDamage x2 [MultiAttack]
  - TORRENT_MOVE: Deal TorrentDamage [SingleAttack]
- Special: AfterAddedToRoom: applies WaterfallGiantPower. Massive HP boss with cycling pattern.

---

### SoulFysh
- ID: SOUL_FYSH
- Type: Boss
- HP: 270 (ascension: 290)
- Damage Values: {Chomp: 16 (18), SoulDrain: 8 (10) x3, DeepDive: 30 (34)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: CHOMP_MOVE
  CHOMP -> SOUL_DRAIN -> DEEP_DIVE -> CHOMP (cycle)
  ```
- Moves:
  - CHOMP_MOVE: Deal ChompDamage [SingleAttack]
  - SOUL_DRAIN_MOVE: Deal SoulDrainDamage x3 [MultiAttack]
  - DEEP_DIVE_MOVE: Deal DeepDiveDamage [SingleAttack]
- Special: AfterAddedToRoom: applies SoulFyshPower

---

### LagavulinMatriarch
- ID: LAGAVULIN_MATRIARCH
- Type: Boss
- HP: 280 (ascension: 300)
- Damage Values: {Swipe: 18 (20), Slam: 12 (14) x2, Roar: 28 (32)}
- Block Values: none
- AI State Machine:
  ```
  INITIAL: SLEEP_MOVE (sleeps for 2 turns)
  After wake: SWIPE -> SLAM -> ROAR -> SWIPE (cycle)
  ```
- Moves:
  - SLEEP_MOVE: Does nothing [Sleep]
  - SWIPE_MOVE: Deal SwipeDamage [SingleAttack]
  - SLAM_MOVE: Deal SlamDamage x2 [MultiAttack]
  - ROAR_MOVE: Deal RoarDamage + debuffs [SingleAttack + Debuff]
- Special: AfterAddedToRoom: applies MetallicizePower and Dormant power. Sleeps initially, gaining block. Wakes when attacked or after 2 turns.

---

## Event-Only / Special Monsters

### Architect
- ID: ARCHITECT
- Type: Event (TheArchitectEventEncounter)
- HP: 9999
- AI: Does nothing (NOTHING move, HiddenIntent, loops forever)
- Special: Unkillable event NPC

---

### BigDummy
- ID: BIG_DUMMY
- Type: Test
- HP: 9999
- AI: Does nothing
- Special: Test/debug monster

---

### Byrdpip
- ID: BYRDPIP
- Type: Pet/Relic
- HP: 9999 (invisible health bar)
- AI: Does nothing
- Special: Pet companion from Byrdpip relic

---

### BattleFriendV1/V2/V3
- ID: BATTLE_FRIEND_V1 / V2 / V3
- Type: Event (BattlewornDummyEventEncounter)
- HP: 75 / 150 / 300
- AI: Does nothing
- Special: AfterAddedToRoom: applies BattlewornDummyTimeLimitPower(3). Training dummy with time limit.

---

### MysteriousKnight
- ID: MYSTERIOUS_KNIGHT
- Type: Event (MysteriousKnightEventEncounter)
- HP: 80 (ascension: 85)
- Damage Values: {Slash: 12 (14)}
- AI: Simple attack loop
- Special: Event encounter only

---

### LivingShield
- ID: LIVING_SHIELD
- Type: Event/Minion
- HP: 1
- AI: Does nothing
- Special: AfterAddedToRoom: applies Illusion power

---

### PaelsLegion
- ID: PAELS_LEGION
- Type: Special (Ancient event)
- HP: variable
- Damage Values: variable
- Special: Part of Pael ancient event encounter

---

### TorchHeadAmalgam
- ID: TORCH_HEAD_AMALGAM
- Type: Event (Amalgamator)
- HP: variable
- Special: Created from Amalgamator event

---

---

## Encounters Reference

### Act 1 - Overgrowth Encounters

### FuzzyWurmCrawlerWeak
- ID: FUZZY_WURM_CRAWLER_WEAK
- Act: 1
- Room Type: Monster (Weak)
- Monsters: [FuzzyWurmCrawler x1]

### NibbitsWeak
- ID: NIBBITS_WEAK
- Act: 1
- Room Type: Monster (Weak)
- Monsters: [Nibbit x3, slots: "first", "second", "third"]

### ShrinkerBeetleWeak
- ID: SHRINKER_BEETLE_WEAK
- Act: 1
- Room Type: Monster (Weak)
- Monsters: [ShrinkerBeetle x2, slots: "first", "second"]

### SlimesWeak
- ID: SLIMES_WEAK
- Act: 1
- Room Type: Monster (Weak)
- Monsters: [Random mix of LeafSlimeM and TwigSlimeM x2]

### CubexConstructNormal
- ID: CUBEX_CONSTRUCT_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [CubexConstruct x1]

### FlyconidNormal
- ID: FLYCONID_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Flyconid x2, slots: "first", "second"]

### FogmogNormal
- ID: FOGMOG_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Fogmog x1]

### InkletsNormal
- ID: INKLETS_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Inklet x3, slots: "first", "second", "third"]

### MawlerNormal
- ID: MAWLER_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Mawler x1]

### NibbitsNormal
- ID: NIBBITS_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Nibbit x4, slots: "first", "second", "third", "fourth"]

### OvergrowthCrawlers
- ID: OVERGROWTH_CRAWLERS
- Act: 1
- Room Type: Monster
- Monsters: [Mix of act 1 crawling monsters]

### RubyRaidersNormal
- ID: RUBY_RAIDERS_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Random selection of 2 from: AssassinRubyRaider, AxeRubyRaider, BruteRubyRaider, CrossbowRubyRaider, TrackerRubyRaider]

### SlimesNormal
- ID: SLIMES_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [Random mix of LeafSlimeM and TwigSlimeM x3]

### SlitheringStranglerNormal
- ID: SLITHERING_STRANGLER_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [SlitheringStrangler x1]

### SnappingJaxfruitNormal
- ID: SNAPPING_JAXFRUIT_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [SnappingJaxfruit x1]

### VineShamblerNormal
- ID: VINE_SHAMBLER_NORMAL
- Act: 1
- Room Type: Monster
- Monsters: [VineShambler x2]

### BygoneEffigyElite
- ID: BYGONE_EFFIGY_ELITE
- Act: 1
- Room Type: Elite
- Monsters: [BygoneEffigy x1]

### ByrdonisElite
- ID: BYRDONIS_ELITE
- Act: 1
- Room Type: Elite
- Monsters: [Byrdonis x1]

### PhrogParasiteElite
- ID: PHROG_PARASITE_ELITE
- Act: 1
- Room Type: Elite
- Monsters: [PhrogParasite x2, slots: "first", "second"]

### CeremonialBeastBoss
- ID: CEREMONIAL_BEAST_BOSS
- Act: 1
- Room Type: Boss
- Monsters: [CeremonialBeast x1]

### TheKinBoss
- ID: THE_KIN_BOSS
- Act: 1
- Room Type: Boss
- Monsters: [KinPriest x1, KinFollower x2-3]

### VantomBoss
- ID: VANTOM_BOSS
- Act: 1
- Room Type: Boss
- Monsters: [Vantom x1]

---

### Act 2 - Hive Encounters

### BowlbugsWeak
- ID: BOWLBUGS_WEAK
- Act: 2
- Room Type: Monster (Weak)
- Monsters: [Random selection of 2 from: BowlbugEgg, BowlbugNectar, BowlbugRock, BowlbugSilk]

### ExoskeletonsWeak
- ID: EXOSKELETONS_WEAK
- Act: 2
- Room Type: Monster (Weak)
- Monsters: [Exoskeleton x2, slots: "first", "second"]

### ThievingHopperWeak
- ID: THIEVING_HOPPER_WEAK
- Act: 2
- Room Type: Monster (Weak)
- Monsters: [ThievingHopper x2]

### TunnelerWeak
- ID: TUNNELER_WEAK
- Act: 2
- Room Type: Monster (Weak)
- Monsters: [Tunneler x2]

### BowlbugsNormal
- ID: BOWLBUGS_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Random selection of 3 from: BowlbugEgg, BowlbugNectar, BowlbugRock, BowlbugSilk]

### ChompersNormal
- ID: CHOMPERS_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Chomper x2 (one ScreamFirst=true, one ScreamFirst=false)]

### ExoskeletonsNormal
- ID: EXOSKELETONS_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Exoskeleton x4, slots: "first", "second", "third", "fourth"]

### HunterKillerNormal
- ID: HUNTER_KILLER_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [HunterKiller x1]

### LouseProgenitorNormal
- ID: LOUSE_PROGENITOR_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [LouseProgenitor x1]

### MytesNormal
- ID: MYTES_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Myte x3]

### OvicopterNormal
- ID: OVICOPTER_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Ovicopter x1]

### SlumberingBeetleNormal
- ID: SLUMBERING_BEETLE_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [SlumberingBeetle x1]

### SpinyToadNormal
- ID: SPINY_TOAD_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [SpinyToad x1]

### TheObscuraNormal
- ID: THE_OBSCURA_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [TheObscura x2]

### TunnelerNormal
- ID: TUNNELER_NORMAL
- Act: 2
- Room Type: Monster
- Monsters: [Tunneler x3]

### DecimillipedeElite
- ID: DECIMILLIPEDE_ELITE
- Act: 2
- Room Type: Elite
- Monsters: [DecimillipedeSegmentFront, DecimillipedeSegmentMiddle, DecimillipedeSegmentBack]

### EntomancerElite
- ID: ENTOMANCER_ELITE
- Act: 2
- Room Type: Elite
- Monsters: [Entomancer x1]

### InfestedPrismsElite
- ID: INFESTED_PRISMS_ELITE
- Act: 2
- Room Type: Elite
- Monsters: [InfestedPrism x2-3]

### KaiserCrabBoss
- ID: KAISER_CRAB_BOSS
- Act: 2
- Room Type: Boss
- Monsters: [Crusher (left arm), Rocket (right arm)]

### KnowledgeDemonBoss
- ID: KNOWLEDGE_DEMON_BOSS
- Act: 2
- Room Type: Boss
- Monsters: [KnowledgeDemon x1]

### TheInsatiableBoss
- ID: THE_INSATIABLE_BOSS
- Act: 2
- Room Type: Boss
- Monsters: [TheInsatiable x1]

---

### Act 3 - Glory Encounters

### DevotedSculptorWeak
- ID: DEVOTED_SCULPTOR_WEAK
- Act: 3
- Room Type: Monster (Weak)
- Monsters: [DevotedSculptor x1]

### ScrollsOfBitingWeak
- ID: SCROLLS_OF_BITING_WEAK
- Act: 3
- Room Type: Monster (Weak)
- Monsters: [ScrollOfBiting x2]

### TurretOperatorWeak
- ID: TURRET_OPERATOR_WEAK
- Act: 3
- Room Type: Monster (Weak)
- Monsters: [TurretOperator x2]

### AxebotsNormal
- ID: AXEBOTS_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [Axebot x2, slots: "front", "back"]

### ConstructMenagerieNormal
- ID: CONSTRUCT_MENAGERIE_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [Mix of construct-type enemies]

### FabricatorNormal
- ID: FABRICATOR_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [Fabricator x1 (summons bots)]

### FrogKnightNormal
- ID: FROG_KNIGHT_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [FrogKnight x1]

### GlobeHeadNormal
- ID: GLOBE_HEAD_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [GlobeHead x1]

### OwlMagistrateNormal
- ID: OWL_MAGISTRATE_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [OwlMagistrate x1]

### ScrollsOfBitingNormal
- ID: SCROLLS_OF_BITING_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [ScrollOfBiting x3]

### SlimedBerserkerNormal
- ID: SLIMED_BERSERKER_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [SlimedBerserker x1]

### TheLostAndForgottenNormal
- ID: THE_LOST_AND_FORGOTTEN_NORMAL
- Act: 3
- Room Type: Monster
- Monsters: [TheLost x1, TheForgotten x1]

### KnightsElite
- ID: KNIGHTS_ELITE
- Act: 3
- Room Type: Elite
- Monsters: [Random 2 from: FlailKnight, MagiKnight, SpectralKnight]

### MechaKnightElite
- ID: MECHA_KNIGHT_ELITE
- Act: 3
- Room Type: Elite
- Monsters: [MechaKnight x1]

### SoulNexusElite
- ID: SOUL_NEXUS_ELITE
- Act: 3
- Room Type: Elite
- Monsters: [SoulNexus x1]

### DoormakerBoss
- ID: DOORMAKER_BOSS
- Act: 3
- Room Type: Boss
- Monsters: [Door x1 (Doormaker spawns automatically)]

### QueenBoss
- ID: QUEEN_BOSS
- Act: 3
- Room Type: Boss
- Monsters: [Queen x1]

### TestSubjectBoss
- ID: TEST_SUBJECT_BOSS
- Act: 3
- Room Type: Boss
- Monsters: [TestSubject x1]

---

### Act 4 - Underdocks Encounters

### CorpseSlugsWeak
- ID: CORPSE_SLUGS_WEAK
- Act: 4
- Room Type: Monster (Weak)
- Monsters: [CorpseSlug x2]

### SeapunkWeak
- ID: SEAPUNK_WEAK
- Act: 4
- Room Type: Monster (Weak)
- Monsters: [Seapunk x2]

### SludgeSpinnerWeak
- ID: SLUDGE_SPINNER_WEAK
- Act: 4
- Room Type: Monster (Weak)
- Monsters: [SludgeSpinner x2]

### ToadpolesWeak
- ID: TOADPOLES_WEAK
- Act: 4
- Room Type: Monster (Weak)
- Monsters: [Toadpole x2, slots: "first", "second"]

### CorpseSlugsNormal
- ID: CORPSE_SLUGS_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [CorpseSlug x3 (each starts on different move)]

### CultistsNormal
- ID: CULTISTS_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [CalcifiedCultist x1, DampCultist x1]

### FossilStalkerNormal
- ID: FOSSIL_STALKER_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [FossilStalker x1]

### GremlinMercNormal
- ID: GREMLIN_MERC_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [GremlinMerc x1 (+ FatGremlin, SneakyGremlin)]

### HauntedShipNormal
- ID: HAUNTED_SHIP_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [HauntedShip x1]

### LivingFogNormal
- ID: LIVING_FOG_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [LivingFog x1]

### PunchConstructNormal
- ID: PUNCH_CONSTRUCT_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [PunchConstruct x2]

### SewerClamNormal
- ID: SEWER_CLAM_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [SewerClam x2]

### ToadpolesNormal
- ID: TOADPOLES_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [Toadpole x4, slots: "first", "second", "third", "fourth"]

### TwoTailedRatsNormal
- ID: TWO_TAILED_RATS_NORMAL
- Act: 4
- Room Type: Monster
- Monsters: [TwoTailedRat x3, slots: "first", "second", "third"]

### PhantasmalGardenersElite
- ID: PHANTASMAL_GARDENERS_ELITE
- Act: 4
- Room Type: Elite
- Monsters: [PhantasmalGardener x2]

### SkulkingColonyElite
- ID: SKULKING_COLONY_ELITE
- Act: 4
- Room Type: Elite
- Monsters: [SkulkingColony x1]

### TerrorEelElite
- ID: TERROR_EEL_ELITE
- Act: 4
- Room Type: Elite
- Monsters: [TerrorEel x1]

### LagavulinMatriarchBoss
- ID: LAGAVULIN_MATRIARCH_BOSS
- Act: 4
- Room Type: Boss
- Monsters: [LagavulinMatriarch x1]

### SoulFyshBoss
- ID: SOUL_FYSH_BOSS
- Act: 4
- Room Type: Boss
- Monsters: [SoulFysh x1]

### WaterfallGiantBoss
- ID: WATERFALL_GIANT_BOSS
- Act: 4
- Room Type: Boss
- Monsters: [WaterfallGiant x1]

---

## Event Encounters (not part of normal act pools)

### BattlewornDummyEventEncounter
- ID: BATTLEWORN_DUMMY_EVENT_ENCOUNTER
- Room Type: Monster (no rewards)
- Monsters: [BattleFriendV1 or V2 or V3 depending on setting]

### DenseVegetationEventEncounter
- ID: DENSE_VEGETATION_EVENT_ENCOUNTER
- Room Type: Monster
- Monsters: [VineShambler x1]

### FakeMerchantEventEncounter
- ID: FAKE_MERCHANT_EVENT_ENCOUNTER
- Room Type: Monster
- Monsters: [FakeMerchantMonster x1]

### MysteriousKnightEventEncounter
- ID: MYSTERIOUS_KNIGHT_EVENT_ENCOUNTER
- Room Type: Monster
- Monsters: [MysteriousKnight x1]

### PunchOffEventEncounter
- ID: PUNCH_OFF_EVENT_ENCOUNTER
- Room Type: Monster
- Monsters: [PunchConstruct x1]

### TheArchitectEventEncounter
- ID: THE_ARCHITECT_EVENT_ENCOUNTER
- Room Type: Monster (no rewards)
- Monsters: [Architect x1]
