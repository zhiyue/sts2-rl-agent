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

# Gym action space
ACTION_END_TURN = 0
ACTION_SPACE_SIZE = 1 + MAX_HAND_SIZE + MAX_HAND_SIZE * MAX_ENEMIES  # 1 + 10 + 50 = 61
