"""Game constants verified against decompiled C# source."""

MAX_HAND_SIZE = 10
BLOCK_CAP = 999
BASE_DRAW = 5
BASE_ENERGY = 3

# Power multipliers (from VulnerablePower/WeakPower/FrailPower decompiled values)
VULNERABLE_MULTIPLIER = 1.5
WEAK_MULTIPLIER = 0.75
FRAIL_MULTIPLIER = 0.75

# Ironclad character
IRONCLAD_STARTING_HP = 80
IRONCLAD_STARTING_GOLD = 99

# Max enemies in a single encounter
MAX_ENEMIES = 5

# Fixed combat potion belt width used by the RL action space.
# The current simulator can reach 9 slots via the base belt (3) plus the
# largest known slot-increasing relic combination in this repo (+2, +4).
MAX_POTION_SLOTS = 9

# Gym action space
# Layout:
#   0                           -> end turn / confirm choice
#   1..(1 + CARD_ACTION_SIZE)   -> card plays
#   POTION_ACTION_START..       -> potion uses
ACTION_END_TURN = 0
CARD_ACTION_SIZE = MAX_HAND_SIZE + MAX_HAND_SIZE * MAX_ENEMIES
POTION_TARGET_OPTIONS = 1 + MAX_ENEMIES
POTION_ACTION_SIZE = MAX_POTION_SLOTS * POTION_TARGET_OPTIONS
POTION_ACTION_START = 1 + CARD_ACTION_SIZE
ACTION_SPACE_SIZE = 1 + CARD_ACTION_SIZE + POTION_ACTION_SIZE
