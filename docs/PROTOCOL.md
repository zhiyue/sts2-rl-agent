# Bridge Communication Protocol

TCP communication protocol between the C# bridge mod (game side) and the Python agent (RL side).

---

## Overview

- **Transport:** TCP socket on `127.0.0.1:9002`
- **Framing:** Newline-delimited JSON (one JSON object per `\n`-terminated line)
- **Direction:**
  - Game -> Agent: state messages describing the current game situation
  - Agent -> Game: action messages with the agent's decision
- **Flow:** Request-response. The game sends a state, then blocks waiting for exactly one action response before continuing.

---

## Game -> Agent Messages

### combat_action

Sent when the game is in the combat play phase and awaiting a card play or end-turn decision.

```json
{
  "type": "combat_action",
  "player": {
    "hp": 70,
    "max_hp": 80,
    "block": 5,
    "energy": 3,
    "max_energy": 3,
    "powers": [
      {"id": "STRENGTH", "amount": 2},
      {"id": "VULNERABLE", "amount": 1}
    ]
  },
  "hand": [
    {
      "id": "STRIKE_IRONCLAD",
      "cost": 1,
      "type": "Attack",
      "target": "AnyEnemy",
      "playable": true
    },
    {
      "id": "DEFEND_IRONCLAD",
      "cost": 1,
      "type": "Skill",
      "target": "Self",
      "playable": true
    },
    {
      "id": "BASH",
      "cost": 2,
      "type": "Attack",
      "target": "AnyEnemy",
      "playable": true,
      "upgraded": true
    }
  ],
  "enemies": [
    {
      "id": "JAW_WORM",
      "hp": 35,
      "max_hp": 44,
      "block": 0,
      "is_alive": true,
      "intent": "SingleAttack",
      "intent_damage": 12,
      "intent_hits": 1,
      "intent_move_id": "Chomp",
      "powers": []
    },
    {
      "id": "JAW_WORM",
      "hp": 20,
      "max_hp": 40,
      "block": 6,
      "is_alive": true,
      "intent": "Defend",
      "powers": [
        {"id": "STRENGTH", "amount": 3}
      ]
    }
  ],
  "draw_pile_count": 5,
  "discard_pile_count": 2,
  "exhaust_pile_count": 0,
  "round": 2,
  "floor": 5,
  "act": 1
}
```

**Field details:**

| Field | Type | Description |
|-------|------|-------------|
| `type` | string | Always `"combat_action"` |
| `player.hp` | int | Current HP |
| `player.max_hp` | int | Maximum HP |
| `player.block` | int | Current block |
| `player.energy` | int | Current energy this turn |
| `player.max_energy` | int | Max energy per turn |
| `player.powers` | array | Active buffs/debuffs on the player |
| `hand` | array | Cards currently in hand |
| `hand[].id` | string | Card ID (e.g. `"STRIKE_IRONCLAD"`, matches C# `CardModel.Id.Entry`) |
| `hand[].cost` | int | Current energy cost (after modifiers) |
| `hand[].type` | string | `"Attack"`, `"Skill"`, `"Power"`, `"Status"`, `"Curse"` |
| `hand[].target` | string | `"Self"`, `"None"`, `"AnyEnemy"`, `"AllEnemies"`, `"RandomEnemy"` |
| `hand[].playable` | bool | Whether the card can be played right now |
| `hand[].upgraded` | bool | (optional) Whether the card is upgraded |
| `enemies` | array | All enemy creatures (including dead ones) |
| `enemies[].id` | string | Monster ID (e.g. `"JAW_WORM"`) |
| `enemies[].hp` / `max_hp` | int | Current and max HP |
| `enemies[].block` | int | Current block |
| `enemies[].is_alive` | bool | Whether the enemy is alive |
| `enemies[].intent` | string | Intent type (see Intent Types below) |
| `enemies[].intent_damage` | int | (optional) Damage the attack will deal |
| `enemies[].intent_hits` | int | (optional) Number of hits (for multi-attack) |
| `enemies[].intent_move_id` | string | (optional) Internal move ID |
| `enemies[].powers` | array | Active buffs/debuffs on the enemy |
| `draw_pile_count` | int | Cards in draw pile |
| `discard_pile_count` | int | Cards in discard pile |
| `exhaust_pile_count` | int | Cards in exhaust pile |
| `round` | int | Current combat round (1-indexed) |
| `floor` | int | Current floor in the run |
| `act` | int | Current act (1-indexed) |

**Intent types** (as serialized by C#):
- `SingleAttack` -- single attack
- `MultiAttack` -- multi-hit attack
- `Defend` -- gaining block
- `Buff` -- buffing itself
- `Debuff` -- debuffing the player
- `DebuffStrong` -- strong debuff
- `Sleep` -- sleeping
- `Summon` -- summoning
- `Escape` -- fleeing
- `Unknown` -- unknown intent
- `StatusCard` -- adding status cards
- `Stun` -- stunned
- `Heal` -- healing
- `DeathBlow` -- lethal attack

### map_select

Sent when the player needs to choose a map node.

```json
{
  "type": "map_select",
  "nodes": [
    {"index": 0, "type": "Monster", "row": 2, "col": 1},
    {"index": 1, "type": "Elite", "row": 2, "col": 3},
    {"index": 2, "type": "Unknown", "row": 2, "col": 5}
  ],
  "floor": 3,
  "act": 1
}
```

### card_reward

Sent when the player must pick a card reward after combat.

```json
{
  "type": "card_reward",
  "cards": [
    {"id": "INFLAME", "type": "Power", "cost": 1},
    {"id": "CARNAGE", "type": "Attack", "cost": 2},
    {"id": "SHRUG_IT_OFF", "type": "Skill", "cost": 1}
  ],
  "can_skip": true
}
```

### card_select

Sent when the player must select cards (upgrade, transform, etc.).

```json
{
  "type": "card_select",
  "cards": [
    {"id": "STRIKE_IRONCLAD", "type": "Attack", "cost": 1},
    {"id": "DEFEND_IRONCLAD", "type": "Skill", "cost": 1}
  ],
  "min_select": 1,
  "max_select": 1
}
```

### game_over

Sent when the run ends.

```json
{
  "type": "game_over",
  "result": "victory"
}
```

Or:
```json
{
  "type": "game_over",
  "message": "Game over"
}
```

### run_complete

Sent when the AutoSlayer run finishes (win or loss).

```json
{
  "type": "run_complete",
  "result": "victory"
}
```

### pong

Response to a ping from the agent.

```json
{
  "type": "pong"
}
```

---

## Agent -> Game Messages

All agent messages use the `"action"` field (not `"type"`).

### play

Play a card from hand.

```json
{
  "action": "play",
  "card_index": 0,
  "target_index": 0
}
```

| Field | Type | Description |
|-------|------|-------------|
| `action` | string | `"play"` |
| `card_index` | int | 0-indexed position in the hand array |
| `target_index` | int | 0-indexed position in the enemies array. Use -1 or omit for self/all-target cards. |

### end_turn

End the current turn.

```json
{
  "action": "end_turn"
}
```

### choose

Choose an option (map node, card reward, event option, etc.).

```json
{
  "action": "choose",
  "index": 1
}
```

| Field | Type | Description |
|-------|------|-------------|
| `action` | string | `"choose"` |
| `index` | int | 0-indexed choice from the available options |

For card rewards, the `index` maps to the `cards` array. To skip, use an index >= the number of offered cards (e.g., 99).

### skip

Skip the current screen (e.g., skip card reward).

```json
{
  "action": "skip"
}
```

### potion

Use a potion.

```json
{
  "action": "potion",
  "slot": 0,
  "target_index": 1
}
```

| Field | Type | Description |
|-------|------|-------------|
| `action` | string | `"potion"` |
| `slot` | int | Potion slot index |
| `target_index` | int | Target enemy index (-1 for self-target potions) |

### ping

Health check. The game responds with `{"type": "pong"}`.

```json
{
  "action": "ping"
}
```

---

## Full Combat Turn Example

A complete message exchange for one combat turn:

```
=== Turn Start ===

Game -> Agent:
{"type":"combat_action","player":{"hp":72,"max_hp":80,"block":0,"energy":3,"max_energy":3},"hand":[{"id":"STRIKE_IRONCLAD","cost":1,"type":"Attack","target":"AnyEnemy","playable":true},{"id":"STRIKE_IRONCLAD","cost":1,"type":"Attack","target":"AnyEnemy","playable":true},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":true},{"id":"BASH","cost":2,"type":"Attack","target":"AnyEnemy","playable":true},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":true}],"enemies":[{"id":"JAW_WORM","hp":40,"max_hp":44,"block":0,"is_alive":true,"intent":"SingleAttack","intent_damage":11,"intent_hits":1}],"draw_pile_count":5,"discard_pile_count":0,"exhaust_pile_count":0,"round":1,"floor":1,"act":1}

Agent -> Game (play Bash on enemy 0):
{"action":"play","card_index":3,"target_index":0}

=== After Bash plays (2 energy spent, enemy takes damage + vulnerable) ===

Game -> Agent:
{"type":"combat_action","player":{"hp":72,"max_hp":80,"block":0,"energy":1,"max_energy":3},"hand":[{"id":"STRIKE_IRONCLAD","cost":1,"type":"Attack","target":"AnyEnemy","playable":true},{"id":"STRIKE_IRONCLAD","cost":1,"type":"Attack","target":"AnyEnemy","playable":true},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":true},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":true}],"enemies":[{"id":"JAW_WORM","hp":32,"max_hp":44,"block":0,"is_alive":true,"intent":"SingleAttack","intent_damage":11,"intent_hits":1,"powers":[{"id":"VULNERABLE","amount":2}]}],"draw_pile_count":5,"discard_pile_count":1,"exhaust_pile_count":0,"round":1,"floor":1,"act":1}

Agent -> Game (play Strike on enemy 0):
{"action":"play","card_index":0,"target_index":0}

=== After Strike plays (1 energy spent, damage boosted by Vulnerable) ===

Game -> Agent:
{"type":"combat_action","player":{"hp":72,"max_hp":80,"block":0,"energy":0,"max_energy":3},"hand":[{"id":"STRIKE_IRONCLAD","cost":1,"type":"Attack","target":"AnyEnemy","playable":false},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":false},{"id":"DEFEND_IRONCLAD","cost":1,"type":"Skill","target":"Self","playable":false}],"enemies":[{"id":"JAW_WORM","hp":23,"max_hp":44,"block":0,"is_alive":true,"intent":"SingleAttack","intent_damage":11,"intent_hits":1,"powers":[{"id":"VULNERABLE","amount":2}]}],"draw_pile_count":5,"discard_pile_count":2,"exhaust_pile_count":0,"round":1,"floor":1,"act":1}

Agent -> Game (end turn, no energy left):
{"action":"end_turn"}

=== Enemy turn executes, then new player turn starts ===
=== Game sends next combat_action state... ===
```

---

## Connection Lifecycle

1. **Game starts:** Bridge mod initializes, TCP server starts listening on port 9002.
2. **Agent connects:** Python agent connects via TCP. Server accepts one client at a time.
3. **AutoSlay starts:** The mod launches the RL-driven AutoSlayer, which navigates menus and starts a run.
4. **Decision loop:** For each decision point, the game sends state JSON, blocks waiting for a response, then executes the action.
5. **Timeout handling:** If the agent does not respond within 30 seconds, the game falls back to random play for that action.
6. **Disconnection:** If the TCP connection drops, the game falls back to random play. The server continues listening for a new connection.
7. **Run end:** The game sends `run_complete` or `game_over`, then returns to the main menu and can start a new run.

---

## Python Client Usage

```python
from sts2_env.bridge.client import STS2GameClient

with STS2GameClient(host="127.0.0.1", port=9002) as client:
    while True:
        state = client.receive_state()

        if state["type"] == "combat_action":
            # Use trained model or heuristic to decide
            client.play_card(card_index=0, target_index=0)
            # or: client.end_turn()

        elif state["type"] == "map_select":
            client.choose(index=0)

        elif state["type"] == "card_reward":
            client.choose(index=0)  # pick first card
            # or: client.choose(index=99)  # skip

        elif state["type"] == "game_over":
            break
```

---

## Protocol Notes

1. **Thread safety:** The C# TCP server runs on a background thread. All game state reads and action injections are dispatched to the Godot main thread via `CallDeferred` or `async Task`.

2. **Action field naming:** Agent messages use `"action"` (not `"type"`) as the primary field. The game uses `"type"` for state messages.

3. **Index semantics:** All indices (`card_index`, `target_index`, `index`) are 0-based and reference the arrays in the most recent state message.

4. **Playability:** The `playable` field on hand cards is informational. If the agent sends a play action for an unplayable card, the game logs an error and does not execute it.

5. **Energy display:** The `energy` field reflects the player's actual current energy. After playing a card, the next state message will show reduced energy.

6. **Dead enemies:** Dead enemies remain in the `enemies` array with `is_alive: false`. Their index is stable throughout the combat.
