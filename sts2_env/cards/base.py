"""Card instance dataclass and factory."""

from __future__ import annotations

from dataclasses import dataclass, field

from sts2_env.core.enums import CardId, CardTag, CardType, TargetType, CardRarity


@dataclass
class CardInstance:
    """A single card instance in combat."""

    card_id: CardId
    cost: int
    card_type: CardType
    target_type: TargetType
    rarity: CardRarity = CardRarity.BASIC
    base_damage: int | None = None
    base_block: int | None = None
    upgraded: bool = False
    keywords: frozenset[str] = frozenset()
    tags: frozenset[str] = frozenset()
    can_be_generated_in_combat: bool = True
    can_be_generated_by_modifiers: bool = True
    enchantments: dict[str, int] = field(default_factory=dict)
    effect_vars: dict[str, int] = field(default_factory=dict)
    instance_id: int = 0
    # X-cost and Star-cost support
    has_energy_cost_x: bool = False
    star_cost: int = 0
    # Persistent per-combat state (e.g. Rampage extra damage, Claw buff)
    combat_vars: dict[str, int] = field(default_factory=dict)
    # Original cost for cost-modification tracking
    original_cost: int | None = None
    single_turn_retain: bool = False
    bound: bool = False
    base_replay_count: int = 0

    def __post_init__(self):
        if self.original_cost is None:
            self.original_cost = self.cost

    @property
    def is_attack(self) -> bool:
        return self.card_type == CardType.ATTACK

    @property
    def is_skill(self) -> bool:
        return self.card_type == CardType.SKILL

    @property
    def is_power(self) -> bool:
        return self.card_type == CardType.POWER

    @property
    def is_status(self) -> bool:
        return self.card_type == CardType.STATUS

    @property
    def is_curse(self) -> bool:
        return self.card_type == CardType.CURSE

    @property
    def exhausts(self) -> bool:
        return "exhaust" in self.keywords

    @property
    def is_unplayable(self) -> bool:
        return "unplayable" in self.keywords

    @property
    def is_ethereal(self) -> bool:
        return "ethereal" in self.keywords

    @property
    def is_innate(self) -> bool:
        return "innate" in self.keywords

    @property
    def is_retain(self) -> bool:
        return "retain" in self.keywords

    @property
    def is_sly(self) -> bool:
        return "sly" in self.keywords or bool(self.combat_vars.get("sly_this_turn"))

    @property
    def has_tag(self) -> bool:
        return len(self.tags) > 0

    @property
    def is_enchanted(self) -> bool:
        return bool(self.enchantments)

    def has_card_tag(self, tag: str) -> bool:
        return tag in self.tags

    def has_enchantment(self, name: str) -> bool:
        return name in self.enchantments

    def add_enchantment(self, name: str, amount: int = 1) -> None:
        self.enchantments[name] = amount

    @property
    def is_shiv(self) -> bool:
        return self.card_id == CardId.SHIV or CardTag.SHIV in self.tags

    def clone(self, new_id: int) -> CardInstance:
        """Create a copy with a new instance_id."""
        return CardInstance(
            card_id=self.card_id,
            cost=self.cost,
            card_type=self.card_type,
            target_type=self.target_type,
            rarity=self.rarity,
            base_damage=self.base_damage,
            base_block=self.base_block,
            upgraded=self.upgraded,
            keywords=self.keywords,
            tags=self.tags,
            can_be_generated_in_combat=self.can_be_generated_in_combat,
            can_be_generated_by_modifiers=self.can_be_generated_by_modifiers,
            enchantments=dict(self.enchantments),
            effect_vars=dict(self.effect_vars),
            instance_id=new_id,
            has_energy_cost_x=self.has_energy_cost_x,
            star_cost=self.star_cost,
            combat_vars=dict(self.combat_vars),
            original_cost=self.original_cost,
            single_turn_retain=self.single_turn_retain,
            bound=self.bound,
            base_replay_count=self.base_replay_count,
        )

    @property
    def should_retain_this_turn(self) -> bool:
        return self.is_retain or self.single_turn_retain

    @property
    def energy_cost(self) -> int:
        return self.cost

    @energy_cost.setter
    def energy_cost(self, value: int) -> None:
        self.cost = value

    def set_temporary_cost_for_turn(self, cost: int) -> None:
        self.combat_vars["_turn_cost_override"] = cost
        self.cost = cost

    def set_combat_cost(self, cost: int) -> None:
        self.cost = cost

    def after_forged(self) -> None:
        """Card lifecycle hook fired after a forge increases this card's damage."""
        return

    def end_of_turn_cleanup(self) -> None:
        self.single_turn_retain = False
        self.bound = False
        self.combat_vars.pop("sly_this_turn", None)
        if "_turn_cost_override" in self.combat_vars:
            self.combat_vars.pop("_turn_cost_override", None)
            self.cost = self.original_cost if self.original_cost is not None else self.cost

    def __repr__(self) -> str:
        name = self.card_id.name
        cost_str = "X" if self.has_energy_cost_x else str(self.cost)
        parts = [f"{name}({cost_str}E"]
        if self.base_damage is not None:
            parts.append(f" {self.base_damage}dmg")
        if self.base_block is not None:
            parts.append(f" {self.base_block}blk")
        if self.upgraded:
            parts.append("+")
        return "".join(parts) + ")"


# Global instance counter for unique IDs
_next_instance_id = 0


def _get_next_id() -> int:
    global _next_instance_id
    _next_instance_id += 1
    return _next_instance_id


def reset_instance_counter() -> None:
    global _next_instance_id
    _next_instance_id = 0
