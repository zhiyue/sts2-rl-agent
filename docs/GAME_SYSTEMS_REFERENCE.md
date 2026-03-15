# STS2 Non-Combat Game Systems Reference

Decompiled from `MegaCrit.Sts2.Core` (Slay the Spire 2 Early Access).

---

## 1. Map Generation

**Source**: `MegaCrit.Sts2.Core.Map/StandardActMap.cs`

### 1.1 MapPointType Enum

```
Unassigned = 0
Unknown    = 1   // "?" rooms -- resolved at runtime via odds
Shop       = 2
Treasure   = 3
RestSite   = 4
Monster    = 5
Elite      = 6
Boss       = 7
Ancient    = 8   // Neow / other ancients
```

### 1.2 Grid Dimensions

| Constant     | Value |
|-------------|-------|
| `_mapWidth`  | 7 columns |
| `_mapLength` | `actModel.GetNumberOfRooms(isMultiplayer) + 1` rows |
| `_iterations`| 7 (number of random paths generated) |

### 1.3 Path Generation Algorithm

```
GenerateMap():
    for i in 0..6:                         // 7 iterations
        startCol = rng.NextInt(0, 7)       // random column in [0,6]
        startPoint = GetOrCreate(startCol, row=1)
        if i == 1:
            // ensure 2nd path starts at a DIFFERENT point
            while startPoint in startMapPoints:
                startCol = rng.NextInt(0, 7)
                startPoint = GetOrCreate(startCol, row=1)
        startMapPoints.add(startPoint)
        PathGenerate(startPoint)           // walk from row 1 to top

    // connect top row to boss
    for each point in row[mapLength-1]:
        point.addChild(BossMapPoint)

    // connect starting point to bottom row
    for each point in row[1]:
        StartingMapPoint.addChild(point)
```

**PathGenerate(current)**:
```
while current.row < mapLength - 1:
    coord = GenerateNextCoord(current)
    next = GetOrCreate(coord)
    current.addChild(next)
    current = next
```

**GenerateNextCoord(current)**:
```
col = current.col
minCol = max(0, col - 1)
maxCol = min(col + 1, 6)
directions = shuffle([-1, 0, 1])       // stable shuffle with RNG
for dir in directions:
    targetCol = (dir == -1) ? minCol : (dir == 0) ? col : maxCol
    targetRow = current.row + 1
    if not HasInvalidCrossover(current, targetCol):
        return (targetCol, targetRow)
```

**HasInvalidCrossover**: Prevents crossing edges. If point at `(targetX, current.row)` already has a child that goes in the opposite diagonal direction, the crossover is invalid.

### 1.4 Room Type Pool Sizes (MapPointTypeCounts)

```
NumOfElites  = round(5 * (Ascension >= SwarmingElites ? 1.6 : 1.0))
             // = 5 normally, 8 at A1+
NumOfShops   = 3
NumOfUnknowns = rng.NextGaussianInt(mean=12, stddev=1, min=10, max=14)
NumOfRests   = rng.NextGaussianInt(mean=5, stddev=1, min=3, max=6)
```

### 1.5 Room Type Assignment

**Fixed assignments** (before random pool):
| Row | Type | Condition |
|-----|------|-----------|
| Last row (`rowCount-1`) | `RestSite` | Always |
| Row `rowCount-7` | `Treasure` | Default |
| Row `rowCount-7` | `Elite` | If `ShouldReplaceTreasureWithElites` |
| Row 1 (first room row) | `Monster` | Always |
| Boss node | `Boss` | Always |
| Starting node | `Ancient` | Always |

**Random assignment** -- the pool (RestSite x NumOfRests, Shop x NumOfShops, Elite x NumOfElites, Unknown x NumOfUnknowns) is queued, then assigned to random unassigned points. Any leftover unassigned points become `Monster`.

### 1.6 Placement Constraints

A point type is **valid** only if all five rules pass:

| Rule | Restricted Types | Condition |
|------|-----------------|-----------|
| **Lower map** (row < 5) | RestSite, Elite | Cannot appear in rows 0-4 |
| **Upper map** (row >= mapLength-3) | RestSite | Cannot appear in last 3 rows |
| **Parent adjacency** | Elite, RestSite, Treasure, Shop | Cannot be same type as any parent or child |
| **Child adjacency** | Elite, RestSite, Treasure, Shop | Cannot be same type as any child |
| **Sibling uniqueness** | RestSite, Monster, Unknown, Elite, Shop | Cannot be same type as any sibling (children of same parent, excluding self) |

**Note**: `MapPointTypeCounts.PointTypesThatIgnoreRules` can bypass all rules for specific types.

### 1.7 Post-Processing

1. **PruneDuplicateSegments** -- Finds segments where multiple paths share the same sequence of room types between the same start/end coordinates, then prunes redundant paths (up to 50 iterations).
2. **CenterGrid** -- If left two columns are empty but right two are not (or vice versa), shift all points by 1 column.
3. **SpreadAdjacentMapPoints** -- Iteratively moves nodes to maximize minimum gap between nodes in the same row.
4. **StraightenPaths** -- If a node with 1 parent + 1 child forms a zigzag (both parent and child are on the same side), shift the node to reduce the bend.

---

## 2. Run Flow

**Source**: `MegaCrit.Sts2.Core.Runs/RunState.cs`, `RunManager.cs`

### 2.1 RunState Persistent Fields

| Field | Type | Description |
|-------|------|-------------|
| `Players` | `List<Player>` | All players in the run |
| `Acts` | `IReadOnlyList<ActModel>` | Act definitions (mutable clones) |
| `CurrentActIndex` | `int` | Current act (0-based); setting clears visited coords & resets ActFloor |
| `Map` | `ActMap` | Current act's generated map |
| `VisitedMapCoords` | `List<MapCoord>` | Coords visited in current act |
| `ActFloor` | `int` | Floor within current act |
| `TotalFloor` | `int` | Sum of all MapPointHistory entry counts |
| `VisitedEventIds` | `HashSet<ModelId>` | Events already seen (prevents repeat) |
| `AscensionLevel` | `int` | Ascension level for the run |
| `Rng` | `RunRngSet` | All RNG streams (seeded) |
| `Odds` | `RunOddsSet` | Contains UnknownMapPointOdds |
| `SharedRelicGrabBag` | `RelicGrabBag` | Shared relic pool |
| `Modifiers` | `IReadOnlyList<ModifierModel>` | Custom/daily run modifiers |
| `ExtraFields` | `ExtraRunFields` | Misc extra state |
| `MultiplayerScalingModel` | `MultiplayerScalingModel` | MP scaling config |

### 2.2 Run Initialization Flow

```
1. SetUpNewSinglePlayer(state, shouldSave)
2. InitializeShared() -- sets up sync, action queues, ascension, timers
3. InitializeNewRun():
   a. Populate SharedRelicGrabBag from SharedRelicPool
   b. Populate each player's RelicGrabBag
   c. Set StartedWithNeow flag (based on NeowEpoch unlock)
   d. Run modifier.OnRunCreated()
   e. ApplyAscensionEffects() for each player
4. GenerateRooms():
   a. Shuffle ancients among acts (act 0 excluded)
   b. For each act: act.GenerateRooms(rng, unlockState, isMultiplayer)
   c. Apply discovery order modifications (for tutorial)
   d. If final act + A10: pick a second boss encounter
```

### 2.3 Entering a Map Point

```
EnterMapCoord(coord):
    RunState.AddVisitedMapCoord(coord)
    actFloor = coord.row + 1
    pointType = Map.GetPoint(coord).PointType
    // Build room type blacklist
    blacklist = {}
    if previousMapPoint had Shop OR all next points are Shops:
        blacklist.add(Shop)
    roomType = RollRoomTypeFor(pointType, blacklist)
    room = CreateRoom(roomType, pointType)
    Enter room
```

**MapPointType to RoomType mapping**:
| MapPointType | RoomType |
|-------------|----------|
| Unknown | `Odds.UnknownMapPoint.Roll(blacklist, state)` |
| Shop | Shop |
| Treasure | Treasure |
| RestSite | RestSite |
| Monster | Monster |
| Elite | Elite |
| Boss | Boss |
| Ancient | Event |

### 2.4 Act Transition

```
EnterNextAct():
    if currentActIndex >= acts.Count - 1:
        if currentRoom.IsVictoryRoom:
            WinRun()
        else:
            Enter TheArchitect event (final event before win)
    else:
        EnterAct(currentActIndex + 1)

EnterAct(actIndex):
    state.CurrentActIndex = actIndex
    state.ClearVisitedMapCoords()
    Odds.UnknownMapPoint.ResetToBase()
    GenerateMap()
    if act 0 AND StartedWithNeow:
        Enter Neow (ancient) map coord
    else:
        Enter MapRoom (show map screen)
```

---

## 3. Card Rewards

**Source**: `MegaCrit.Sts2.Core.Factories/CardFactory.cs`, `MegaCrit.Sts2.Core.Odds/CardRarityOdds.cs`

### 3.1 Card Rarity Odds (Base)

| Context | Common | Uncommon | Rare |
|---------|--------|----------|------|
| **Regular Encounter** | 0.60 (A7+: 0.615) | 0.37 | 0.03 (A7+: 0.0149) |
| **Elite Encounter** | 0.50 (A7+: 0.549) | 0.40 | 0.10 (A7+: 0.05) |
| **Boss Encounter** | 0.00 | 0.00 | 1.00 |
| **Shop** | 0.54 (A7+: 0.585) | 0.37 | 0.09 (A7+: 0.045) |
| **Uniform** | 0.33 | 0.33 | 0.33 |

### 3.2 Pity Counter Mechanics

The `CardRarityOdds` system maintains a `CurrentValue` (pity offset) that shifts rare odds over time.

```
Initial CurrentValue = -0.05

Roll(type):
    // Boss encounters ignore pity (use offset = 0)
    offset = (type == Boss) ? 0 : CurrentValue
    roll = rng.NextFloat()         // [0, 1)
    rareThreshold = BaseRareOdds(type) + offset
    if roll < rareThreshold:
        result = Rare
        CurrentValue = -0.05       // RESET on rare hit
    else if roll < UncommonOdds(type) + rareThreshold:
        result = Uncommon
        CurrentValue += RarityGrowth  // +0.01 (A7+: +0.005)
    else:
        result = Common
        CurrentValue += RarityGrowth

    CurrentValue = min(CurrentValue, 0.4)  // capped at +0.4
    return result
```

**When pity changes**: Only when `source == Encounter` AND `rollMethod` is Regular/Elite/Boss. Shop rolls use `RollWithBaseOdds` (no pity).

Merchant card rarity uses `RollWithoutChangingFutureOdds` -- consumes RNG but does NOT modify the pity counter.

### 3.3 Upgrade Probability Formula

```
RollForUpgrade(player, card, baseChance, rng):
    roll = rng.NextFloat()
    if card.IsUpgradable:
        odds = baseChance
        if card.Rarity != Rare:
            odds += currentActIndex * UpgradedCardOddScaling
        // UpgradedCardOddScaling = 0.25 (A7+: 0.125)
        odds = Hook.ModifyCardRewardUpgradeOdds(odds)
        if roll <= odds:
            Upgrade(card)
```

| Context | Base Chance | Notes |
|---------|------------|-------|
| Combat reward | 0.0 | Scales with act: +0.25/act (A7+: +0.125/act) |
| Merchant card | -999999999 | Never upgrades (massive negative base) |

So for combat rewards: Act 0 = 0%, Act 1 = 25% (non-rare), Act 2 = 50% (non-rare). Rare cards always have 0% base (no act scaling).

### 3.4 Blacklist Logic

Cards already picked in the same reward set are blacklisted (via the `blacklist` parameter accumulating `card.CanonicalInstance`). If the rolled rarity has no valid cards after blacklisting, `GetNextHighestRarity()` is called to bump rarity up.

---

## 4. Shop (Merchant)

**Source**: `MegaCrit.Sts2.Core.Entities.Merchant/MerchantInventory.cs`, related files

### 4.1 Inventory Composition

| Slot | Count | Details |
|------|-------|---------|
| Character cards | 5 | Types: Attack, Attack, Skill, Skill, Power |
| Colorless cards | 2 | Rarities: Uncommon, Rare |
| Relics | 3 | 2x `RelicFactory.RollRarity()` + 1x `RelicRarity.Shop` |
| Potions | 3 | Random potions via `PotionFactory.CreateRandomPotionsOutOfCombat` |
| Card removal | 1 | Always available |

**Sale**: Exactly 1 of the 5 character cards is randomly selected to be on sale (50% off).

### 4.2 Card Pricing

```
GetCost(card):
    baseCost = switch card.Rarity:
        Rare     -> 150
        Uncommon -> 75
        default  -> 50  (Common/Basic)

    if card.Pool is ColorlessCardPool:
        baseCost = round(baseCost * 1.15)

    finalCost = round(baseCost * rng.NextFloat(0.95, 1.05))
    if onSale:
        finalCost /= 2

    return Hook.ModifyMerchantPrice(finalCost)
```

### 4.3 Relic Pricing

```
cost = round(relic.MerchantCost * rng.NextFloat(0.85, 1.15))
```

The `MerchantCost` property is defined per-relic on the model. Pricing has wider variance than cards (0.85-1.15 vs 0.95-1.05).

**Blacklisted from shop relics**: `TheCourier`, `OldCoin`.

### 4.4 Potion Pricing

```
baseCost = switch potion.Rarity:
    Rare     -> 100
    Uncommon -> 75
    default  -> 50  (Common)

finalCost = round(baseCost * rng.NextFloat(0.95, 1.05))
```

### 4.5 Card Removal Cost

```
cost = 75 + 25 * player.ExtraFields.CardShopRemovalsUsed
```

Each removal increases the cost by 25 gold. First removal = 75, second = 100, third = 125, etc.

### 4.6 Relic Rarity Roll (for shops and combat rewards)

```
RelicFactory.RollRarity(rng):
    roll = rng.NextFloat()
    if roll < 0.50: return Common
    if roll < 0.83: return Uncommon
    return Rare
```

| Rarity | Probability |
|--------|------------|
| Common | 50% |
| Uncommon | 33% |
| Rare | 17% |

---

## 5. Rest Site

**Source**: `MegaCrit.Sts2.Core.Entities.RestSite/`

### 5.1 Default Options

Generated via `RestSiteOption.Generate(player)`:
- **Heal** (always)
- **Smith** (always, but disabled if no upgradable cards)
- **Mend** (multiplayer only -- heal another player)

Additional options are injected by relics via `Hook.ModifyRestSiteOptions`.

### 5.2 All Rest Site Options

| Option | ID | Effect | Source/Condition |
|--------|----|--------|-----------------|
| **Heal** | `HEAL` | Heal `floor(maxHp * 0.3)` HP | Default |
| **Smith** | `SMITH` | Upgrade 1 card from deck (cancelable) | Default; disabled if no upgradable cards |
| **Mend** | `MEND` | Heal another player for `floor(targetMaxHp * 0.3)` HP | Multiplayer only |
| **Lift** | `LIFT` | Increment Girya counter (+1 Strength when 3 lifts done). Max 3 lifts. | Relic: Girya |
| **Dig** | `DIG` | Obtain a relic (pulled from front of grab bag) | Relic: Shovel |
| **Cook** | `COOK` | Remove 2 cards, gain +9 Max HP | Relic: (added via hook); requires >= 2 removable cards |
| **Clone** | `CLONE` | Duplicate all cards that have the Clone enchantment | Relic: Pael's Growth |
| **Hatch** | `HATCH` | Obtain the Byrdpip relic | Relic: (Byrdoni's Nest event pet) |

### 5.3 Heal Formula

```
baseHealAmount = maxHp * 0.3
finalHealAmount = Hook.ModifyRestSiteHealAmount(baseHealAmount)
```

The base is always 30% of max HP (decimal, applied as `CreatureCmd.Heal` which rounds).

---

## 6. Events

**Source**: `MegaCrit.Sts2.Core.Models.Events/` (68 event files)

### 6.1 Event List with IsAllowed Conditions

Events without an `IsAllowed` override are always allowed (default = true). Deprecated/disabled events return false.

| Event | IsAllowed Condition |
|-------|-------------------|
| AbyssalBaths | Always |
| Amalgamator | All players have >= 2 Strike-tagged AND >= 2 Defend-tagged cards |
| AromaOfChaos | Always |
| BattlewornDummy | Always |
| BrainLeech | `currentActIndex < 2` |
| Bugslayer | Always |
| ByrdonisNest | No player has an event pet |
| ColorfulPhilosophers | Always |
| ColossalFlower | All players have >= 19 HP |
| CrystalSphere | All players have >= 100 gold AND `currentActIndex > 0` |
| Darv | Always |
| DenseVegetation | Always |
| DollRoom | `currentActIndex == 1` |
| DoorsOfLightAndDark | Always |
| DrowningBeacon | Always |
| EndlessConveyor | All players have >= 105 gold |
| FakeMerchant | `currentActIndex >= 1` AND singleplayer AND (gold >= 100 OR has FoulPotion) |
| FieldOfManSizedHoles | All players have a card that can be enchanted with PerfectFit |
| GraveOfTheForgotten | Always |
| HungryForMushrooms | Always |
| InfestedAutomaton | Always |
| JungleMazeAdventure | Always |
| LostWisp | Always |
| LuminousChoir | All players have enough gold (variable) AND have available relics in grab bag |
| MorphicGrove | All players have enough gold (variable) |
| Nonupeipe | Always |
| Orobas | Always |
| Pael | Always |
| PotionCourier | `currentActIndex > 0` |
| PunchOff | `totalFloor >= 6` |
| RanwidTheElder | `currentActIndex > 0` AND all players have tradable relics AND gold >= 100 |
| Reflections | Always |
| RelicTrader | `currentActIndex > 0` AND all players have >= 5 tradable relics |
| RoomFullOfCheese | `currentActIndex < 2` |
| RoundTeaParty | Always |
| SapphireSeed | Always |
| SelfHelpBook | Always |
| SlipperyBridge | `totalFloor > 6` AND all players have a removable card |
| SpiralingWhirlpool | All players have a card that can be enchanted with Spiral |
| SpiritGrafter | Always |
| StoneOfAllTime | `currentActIndex == 1` AND all players have >= 1 potion |
| SunkenStatue | Always |
| SunkenTreasury | Always |
| Symbiote | `currentActIndex > 0` |
| TabletOfTruth | Always |
| Tanx | Always |
| TeaMaster | `currentActIndex < 2` AND all players have >= 150 gold |
| Tezcatara | Always |
| TheArchitect | (Special -- final act transition event, not in normal pool) |
| TheFutureOfPotions | All players have >= 2 potions |
| TheLanternKey | Always |
| TheLegendsWereTrue | `currentActIndex == 0` AND all players have cards AND >= 10 HP |
| ThisOrThat | Always |
| TinkerTime | Always |
| TrashHeap | All players have > 5 HP |
| Trial | Always |
| UnrestSite | All players have HP <= 70% of max HP |
| Vakuu | Always |
| WarHistorianRepy | **Disabled** (returns false) |
| WaterloggedScriptorium | All players have >= 65 gold |
| WelcomeToWongos | `currentActIndex == 1` AND all players have >= 100 gold |
| Wellspring | Always |
| WhisperingHollow | All players have enough gold (variable) |
| WoodCarvings | All players have a removable Basic-rarity card in deck |
| ZenWeaver | All players have enough gold (variable - "EmotionalAwarenessCost") |

### 6.2 Event Structure Pattern

Each event extends `EventModel` and implements:

```
class MyEvent : EventModel
    // Dynamic variables for display
    CanonicalVars -> [DamageVar, HealVar, GoldVar, etc.]

    // Optional: conditions
    IsAllowed(runState) -> bool

    // Optional: randomize variables
    CalculateVars()

    // Required: define choices
    GenerateInitialOptions() -> List<EventOption>

    // Each option is an async function that:
    //   - Executes effects (damage, heal, gain gold, etc.)
    //   - Either calls SetEventFinished(description) to end
    //   - Or calls SetEventState(description, newOptions) for multi-page
```

### 6.3 Notable Event Mechanics

**AbyssalBaths**: Multi-page. Immerse: +2 Max HP, take 3 damage. Each Linger: +2 Max HP, damage increases by 1. Can repeat up to 9 times. Abstain: Heal 10 HP.

**CrystalSphere**: Costs 50 + random(1-49) gold for 3 Prophesize picks, OR gain Debt curse for 6 picks.

**Neow** (Ancient): Offers 2 random positive relic options + 1 random curse relic option. Special pairing rules prevent contradictory pairs (e.g., CursedPearl excludes GoldenPearl).

**FakeMerchant**: Custom shop layout with 6 random fake relics at 50g each. Throwing FoulPotion starts a combat.

**WelcomeToWongos**: Act 2 only. Bargain Bin (100g -> common relic), Featured Item (200g -> rare relic), Mystery Box (300g -> WongosMysteryTicket relic). Leaving downgrades a random upgraded card. Tracks Wongo Points across runs; every 2000 points grants WongoCustomerAppreciationBadge.

**RelicTrader**: Trade up to 3 owned relics for 3 randomly pulled new relics. Requires act > 0 and >= 5 tradable relics.

---

## 7. Unknown Room Odds

**Source**: `MegaCrit.Sts2.Core.Odds/UnknownMapPointOdds.cs`

### 7.1 Base Probabilities

| Outcome | Base Odds | Notes |
|---------|----------|-------|
| Monster | 0.10 | |
| Elite | -1.00 | Negative = impossible until boosted |
| Treasure | 0.02 | |
| Shop | 0.03 | |
| Event | `1 - sum(positive odds)` | ~0.85 initially |

### 7.2 Rolling Algorithm

```
Roll(blacklist, runState):
    // Tutorial override (first run, first 2 unknowns = Event, 3rd = Monster)
    if numberOfRuns == 0:
        unknownCount = count of Unknown entries in history
        if unknownCount < 2: return Event
        if unknownCount == 2: return Monster

    default = Event (or first non-blacklisted type if Event is blacklisted)
    roll = rng.NextFloat()
    cumulative = 0
    for each (roomType, odds) in nonEventOdds:
        if roomType not blacklisted AND odds > 0:
            cumulative += odds
            if roll <= cumulative:
                result = roomType
                break
    else:
        result = default (Event)

    // Update odds for NEXT roll:
    for each (roomType, baseOdds) in baseOdds:
        if roomType == result:
            nonEventOdds[roomType] = baseOdds   // reset the rolled type
        else if roomType in allowedTypes:
            nonEventOdds[roomType] += baseOdds   // increase un-rolled types
    // (Hook can modify the increase amount)

    return result
```

**Key behavior**: Each time a non-event type is NOT rolled, its odds increase by its base value. When it IS rolled, it resets to base. This creates a "soft pity" for rare outcomes. Odds reset entirely on act change (`ResetToBase()`).

---

## 8. Potion System

**Source**: `MegaCrit.Sts2.Core.Factories/PotionFactory.cs`, `MegaCrit.Sts2.Core.Models.Potions/`, `MegaCrit.Sts2.Core.Odds/PotionRewardOdds.cs`

### 8.1 Potion Drop Odds (Combat Reward)

```
PotionRewardOdds:
    initialValue = 0.40
    targetOdds   = 0.50
    eliteBonus   = 0.25

Roll(player, ascensionManager, roomType):
    currentValue = this.currentValue
    forced = Hook.ShouldForcePotionReward(...)
    roll = rng.NextFloat()
    if roll < currentValue OR forced:
        currentValue -= 0.10    // got potion, reduce future odds
    else:
        currentValue += 0.10    // no potion, increase future odds

    eliteBonus = (roomType == Elite) ? 0.25 : 0
    threshold = currentValue + eliteBonus * 0.50

    return forced OR (roll < threshold)
```

So potion drops oscillate around ~40-50% with +/- 10% swings. Elite fights get +12.5% bonus.

### 8.2 Potion Rarity Distribution

```
roll = rng.NextFloat()
if roll <= 0.10: Rare
elif roll <= 0.35: Uncommon    // 0.10 + 0.25
else: Common                   // remaining 0.65
```

| Rarity | Probability |
|--------|------------|
| Common | 65% |
| Uncommon | 25% |
| Rare | 10% |

### 8.3 Potion Generation Pool

Potions are drawn from `player.Character.PotionPool` (character-specific) combined with `SharedPotionPool` (universal). Event and Token rarity potions are excluded from normal generation.

### 8.4 All Potions Reference

| Potion | Rarity | Usage | Target |
|--------|--------|-------|--------|
| Ashwater | Uncommon | CombatOnly | Self |
| AttackPotion | Common | CombatOnly | Self |
| BeetleJuice | Rare | CombatOnly | AnyEnemy |
| BlessingOfTheForge | Uncommon | CombatOnly | Self |
| BlockPotion | Common | CombatOnly | AnyPlayer |
| BloodPotion | Common | AnyTime | AnyPlayer |
| BoneBrew | Uncommon | CombatOnly | Self |
| BottledPotential | Rare | CombatOnly | AnyPlayer |
| Clarity | Uncommon | CombatOnly | AnyPlayer |
| ColorlessPotion | Common | CombatOnly | Self |
| CosmicConcoction | Rare | CombatOnly | Self |
| CunningPotion | Uncommon | CombatOnly | Self |
| CureAll | Uncommon | CombatOnly | AnyPlayer |
| DexterityPotion | Common | CombatOnly | AnyPlayer |
| DistilledChaos | Rare | CombatOnly | Self |
| DropletOfPrecognition | Rare | CombatOnly | Self |
| Duplicator | Uncommon | CombatOnly | Self |
| EnergyPotion | Common | CombatOnly | AnyPlayer |
| EntropicBrew | Rare | AnyTime | Self |
| EssenceOfDarkness | Rare | CombatOnly | Self |
| ExplosiveAmpoule | Common | CombatOnly | AllEnemies |
| FairyInABottle | Rare | Automatic | Self |
| FirePotion | Common | CombatOnly | AnyEnemy |
| FlexPotion | Common | CombatOnly | AnyPlayer |
| FocusPotion | Common | CombatOnly | Self |
| Fortifier | Uncommon | CombatOnly | AnyPlayer |
| FoulPotion | Event | AnyTime | (special) |
| FruitJuice | Rare | AnyTime | AnyPlayer |
| FyshOil | Uncommon | CombatOnly | AnyPlayer |
| GamblersBrew | Uncommon | CombatOnly | Self |
| GhostInAJar | Rare | CombatOnly | AnyPlayer |
| GigantificationPotion | Rare | CombatOnly | AnyPlayer |
| GlowwaterPotion | Event | CombatOnly | Self |
| HeartOfIron | Uncommon | CombatOnly | AnyPlayer |
| KingsCourage | Uncommon | CombatOnly | AnyPlayer |
| LiquidBronze | Uncommon | CombatOnly | AnyPlayer |
| LiquidMemories | Rare | CombatOnly | Self |
| LuckyTonic | Rare | CombatOnly | AnyPlayer |
| MazalethsGift | Rare | CombatOnly | AnyPlayer |
| OrobicAcid | Rare | CombatOnly | Self |
| PoisonPotion | Common | CombatOnly | AnyEnemy |
| PotOfGhouls | Rare | CombatOnly | Self |
| PotionOfBinding | Uncommon | CombatOnly | AllEnemies |
| PotionOfCapacity | Uncommon | CombatOnly | Self |
| PotionOfDoom | Common | CombatOnly | AnyEnemy |
| PotionShapedRock | Token | CombatOnly | AnyEnemy |
| PowderedDemise | Uncommon | CombatOnly | AnyEnemy |
| PowerPotion | Common | CombatOnly | Self |
| RadiantTincture | Uncommon | CombatOnly | AnyPlayer |
| RegenPotion | Uncommon | CombatOnly | AnyPlayer |
| ShacklingPotion | Rare | CombatOnly | AllEnemies |
| ShipInABottle | Rare | CombatOnly | AnyPlayer |
| SkillPotion | Common | CombatOnly | Self |
| SneckoOil | Rare | CombatOnly | AnyPlayer |
| SoldiersStew | Rare | CombatOnly | AnyPlayer |
| SpeedPotion | Common | CombatOnly | AnyPlayer |
| StableSerum | Uncommon | CombatOnly | AnyPlayer |
| StarPotion | Common | CombatOnly | Self |
| StrengthPotion | Common | CombatOnly | AnyPlayer |
| SwiftPotion | Common | CombatOnly | AnyPlayer |
| TouchOfInsanity | Uncommon | CombatOnly | Self |
| VulnerablePotion | Common | CombatOnly | AnyEnemy |
| WeakPotion | Common | CombatOnly | AnyEnemy |

**Usage Types**:
- `CombatOnly` -- Can only be used during combat
- `AnyTime` -- Can be used on the map or in combat
- `Automatic` -- Triggers automatically (e.g., Fairy In A Bottle on death)

**Not in normal generation pool**: DeprecatedPotion (Rarity=None), FoulPotion (Rarity=Event), GlowwaterPotion (Rarity=Event), PotionShapedRock (Rarity=Token).

---

## 9. Orbs (Defect System)

**Source**: `MegaCrit.Sts2.Core.Models.Orbs/`, `MegaCrit.Sts2.Core.Entities.Orbs/OrbQueue.cs`

### 9.1 Orb Types

| Orb | Passive | Passive Trigger | Evoke | Special |
|-----|---------|-----------------|-------|---------|
| **Lightning** | 3 dmg to random enemy | Before turn end | 8 dmg to random enemy | Focus modifies values |
| **Frost** | 2 Block to self | Before turn end | 5 Block to self | Focus modifies values |
| **Dark** | +6 to evoke value (stacking) | Before turn end | Deal accumulated value to lowest-HP enemy | Starts at 6 evoke; gains PassiveVal each turn |
| **Plasma** | +1 Energy | After turn start | +2 Energy | Values NOT modified by Focus |
| **Glass** | 4 dmg to all enemies (decreases by 1 each trigger) | Before turn end | PassiveVal * 2 to all enemies | Decaying; focus modifies values |

### 9.2 Orb Values (Focus-Modified)

Lightning, Frost, Dark, and Glass use `ModifyOrbValue(base)` which adds Focus. Plasma does NOT use `ModifyOrbValue`.

```
ModifyOrbValue(base):
    return base + Focus
```

### 9.3 OrbQueue Mechanics

| Constant | Value |
|----------|-------|
| `maxCapacity` | 10 |
| Initial capacity | 0 (must be granted by cards/relics) |

```
TryEnqueue(orb):
    if capacity == 0: return false
    if orbs.Count >= capacity: ERROR (caller must evoke first)
    orbs.Add(orb)

// When channeling a new orb with full slots:
//   The FIRST orb in the queue is evoked, then removed, then new orb is added

BeforeTurnEnd(choiceContext):
    for each orb in queue:
        triggerCount = Hook.ModifyOrbPassiveTriggerCount(orb, 1)
        for i in 0..triggerCount-1:
            orb.BeforeTurnEndOrbTrigger()

AfterTurnStart(choiceContext):
    for each orb in queue:
        triggerCount = Hook.ModifyOrbPassiveTriggerCount(orb, 1)
        for i in 0..triggerCount-1:
            orb.AfterTurnStartOrbTrigger()
```

---

## 10. Ascension System

**Source**: `MegaCrit.Sts2.Core.Entities.Ascension/AscensionLevel.cs`, `AscensionManager.cs`, `AscensionHelper.cs`

### 10.1 Max Level

```
maxAscensionAllowed = 10
```

### 10.2 All Ascension Levels

| Level | Enum Name | Effect |
|-------|-----------|--------|
| 0 | `None` | No modifiers |
| 1 | `SwarmingElites` | Elite count on map: `round(5 * 1.6)` = 8 (up from 5) |
| 2 | `WearyTraveler` | (Effect defined in localization/hooks -- reduced rest healing or similar) |
| 3 | `Poverty` | Gold multiplier = 0.75x (`PovertyAscensionGoldMultiplier`) |
| 4 | `TightBelt` | Max potion slots reduced by 1 (`player.SubtractFromMaxPotionCount(1)`) |
| 5 | `AscendersBane` | Adds `AscendersBane` curse card to starting deck |
| 6 | `Gloom` | (Effect defined in localization/hooks) |
| 7 | `Scarcity` | Card rarity odds shifted (see table below) + upgrade scaling halved |
| 8 | `ToughEnemies` | (Effect on enemy stats via hooks) |
| 9 | `DeadlyEnemies` | (Effect on enemy stats via hooks) |
| 10 | `DoubleBoss` | Final act gets a SECOND boss encounter (different from the first) |

### 10.3 Ascension 7 (Scarcity) Exact Changes

| Parameter | Normal | A7+ |
|-----------|--------|-----|
| Regular Common odds | 0.600 | 0.615 |
| Regular Rare odds | 0.030 | 0.0149 |
| Elite Common odds | 0.500 | 0.549 |
| Elite Rare odds | 0.100 | 0.050 |
| Shop Common odds | 0.540 | 0.585 |
| Shop Rare odds | 0.090 | 0.045 |
| Rarity pity growth | +0.010/roll | +0.005/roll |
| Upgrade scaling/act | 0.250 | 0.125 |

### 10.4 Ascension Effects Applied at Run Start

```
ApplyEffectsTo(player):
    if level >= TightBelt (4):
        player.SubtractFromMaxPotionCount(1)
    if level >= AscendersBane (5):
        add AscendersBane curse to deck
```

All other ascension effects are implemented via `AscensionHelper.GetValueIfAscension()` checks scattered throughout the codebase, and via hooks that modify enemy stats, gold rewards, healing, etc.

---

## Appendix A: RNG Streams

The `RunRngSet` contains separate seeded RNG streams to ensure determinism:

- Map generation: `act_{N}_map`
- Up-front decisions: `UpFront`
- Unknown map point: `UnknownMapPoint`
- Combat targets: `CombatTargets`
- Treasure room relics: `TreasureRoomRelics`
- Niche: `Niche`
- Combat potion generation: `CombatPotionGeneration`

Per-player streams (`PlayerRngSet`):
- `Rewards` (card rarity, upgrades, potion drops)
- `Shops` (merchant inventory generation)

## Appendix B: Key File Paths in Decompiled Source

| System | Path |
|--------|------|
| Map generation | `MegaCrit.Sts2.Core.Map/StandardActMap.cs` |
| Map point types | `MegaCrit.Sts2.Core.Map/MapPointType.cs` |
| Map room counts | `MegaCrit.Sts2.Core.Map/MapPointTypeCounts.cs` |
| Run state | `MegaCrit.Sts2.Core.Runs/RunState.cs` |
| Run manager | `MegaCrit.Sts2.Core.Runs/RunManager.cs` |
| Card factory | `MegaCrit.Sts2.Core.Factories/CardFactory.cs` |
| Card rarity odds | `MegaCrit.Sts2.Core.Odds/CardRarityOdds.cs` |
| Unknown room odds | `MegaCrit.Sts2.Core.Odds/UnknownMapPointOdds.cs` |
| Potion drop odds | `MegaCrit.Sts2.Core.Odds/PotionRewardOdds.cs` |
| Potion factory | `MegaCrit.Sts2.Core.Factories/PotionFactory.cs` |
| Relic factory | `MegaCrit.Sts2.Core.Factories/RelicFactory.cs` |
| Merchant inventory | `MegaCrit.Sts2.Core.Entities.Merchant/MerchantInventory.cs` |
| Card pricing | `MegaCrit.Sts2.Core.Entities.Merchant/MerchantCardEntry.cs` |
| Relic pricing | `MegaCrit.Sts2.Core.Entities.Merchant/MerchantRelicEntry.cs` |
| Potion pricing | `MegaCrit.Sts2.Core.Entities.Merchant/MerchantPotionEntry.cs` |
| Card removal | `MegaCrit.Sts2.Core.Entities.Merchant/MerchantCardRemovalEntry.cs` |
| Rest site options | `MegaCrit.Sts2.Core.Entities.RestSite/*.cs` |
| Events (~68 files) | `MegaCrit.Sts2.Core.Models.Events/*.cs` |
| Potions (~65 files) | `MegaCrit.Sts2.Core.Models.Potions/*.cs` |
| Orb models | `MegaCrit.Sts2.Core.Models.Orbs/*.cs` |
| Orb queue | `MegaCrit.Sts2.Core.Entities.Orbs/OrbQueue.cs` |
| Ascension levels | `MegaCrit.Sts2.Core.Entities.Ascension/AscensionLevel.cs` |
| Ascension manager | `MegaCrit.Sts2.Core.Entities.Ascension/AscensionManager.cs` |
| Ascension helper | `MegaCrit.Sts2.Core.Helpers/AscensionHelper.cs` |
