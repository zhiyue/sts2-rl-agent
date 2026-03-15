"""Intent types for monster moves."""

from __future__ import annotations

from dataclasses import dataclass

from sts2_env.core.enums import IntentType


@dataclass
class Intent:
    """A single intent shown to the player."""

    intent_type: IntentType
    damage: int = 0
    hits: int = 1

    @property
    def total_damage(self) -> int:
        return self.damage * self.hits

    @property
    def is_attack(self) -> bool:
        return self.intent_type in (IntentType.ATTACK, IntentType.MULTI_ATTACK)


def attack_intent(damage: int) -> Intent:
    return Intent(IntentType.ATTACK, damage=damage, hits=1)


def multi_attack_intent(damage: int, hits: int) -> Intent:
    return Intent(IntentType.MULTI_ATTACK, damage=damage, hits=hits)


def defend_intent() -> Intent:
    return Intent(IntentType.DEFEND)


def buff_intent() -> Intent:
    return Intent(IntentType.BUFF)


def debuff_intent() -> Intent:
    return Intent(IntentType.DEBUFF)


def strong_debuff_intent() -> Intent:
    return Intent(IntentType.DEBUFF_STRONG)


def sleep_intent() -> Intent:
    return Intent(IntentType.SLEEP)


def status_intent() -> Intent:
    return Intent(IntentType.STATUS_CARD)
