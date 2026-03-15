"""STS2 character definitions.

Provides CharacterConfig dataclasses for all five playable characters,
starter-deck factories, and card-pool listings.
"""

from sts2_env.characters.all import (  # noqa: F401
    CharacterConfig,
    IRONCLAD,
    SILENT,
    DEFECT,
    REGENT,
    NECROBINDER,
    ALL_CHARACTERS,
    get_character,
    create_starting_deck,
)
