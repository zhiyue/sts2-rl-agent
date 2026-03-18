"""Second batch of focused parity tests for high-risk relics."""

import sts2_env.powers  # noqa: F401

from sts2_env.cards.defect import create_defect_starter_deck
from sts2_env.cards.ironclad import create_ironclad_starter_deck
from sts2_env.cards.ironclad_basic import make_defend_ironclad, make_strike_ironclad
from sts2_env.core.combat import CombatState
from sts2_env.core.enums import CombatSide, OrbType
from sts2_env.core.hooks import fire_before_turn_end
from sts2_env.core.rng import Rng
from sts2_env.monsters.act1_weak import create_shrinker_beetle, create_twig_slime_s


def _make_ironclad_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 42,
    enemies: int = 1,
) -> CombatState:
    combat = CombatState(
        player_hp=80,
        player_max_hp=80,
        deck=create_ironclad_starter_deck(),
        rng_seed=seed,
        character_id="Ironclad",
        relics=relics or [],
    )
    for i in range(enemies):
        if i == 0:
            creature, ai = create_shrinker_beetle(Rng(seed + i))
        else:
            creature, ai = create_twig_slime_s(Rng(seed + i))
        combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


def _make_defect_combat(
    relics: list[str] | None = None,
    *,
    seed: int = 52,
) -> CombatState:
    combat = CombatState(
        player_hp=75,
        player_max_hp=75,
        deck=create_defect_starter_deck(),
        rng_seed=seed,
        character_id="Defect",
        relics=relics or [],
    )
    creature, ai = create_shrinker_beetle(Rng(seed))
    combat.add_enemy(creature, ai)
    combat.start_combat()
    return combat


class TestRelicParityExtra2:
    def test_bag_of_preparation_adds_only_round_one_hand_draw(self):
        """Matches BagOfPreparation.cs: +2 draw only on the first turn."""
        combat = _make_ironclad_combat(["BagOfPreparation"], seed=101)

        assert len(combat.hand) == 7

        combat.end_player_turn()

        assert combat.round_number == 2
        assert len(combat.hand) == 5

    def test_orichalcum_grants_block_only_if_player_has_none_at_turn_end(self):
        """Matches Orichalcum.cs: before turn end, gain block only when current block is zero."""
        combat = _make_ironclad_combat(["Orichalcum"], seed=102)
        combat.player.block = 0

        fire_before_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.block == 6

        combat.player.block = 5
        fire_before_turn_end(CombatSide.PLAYER, combat)
        assert combat.player.block == 5

    def test_mercury_hourglass_hits_all_enemies_each_player_turn_start(self):
        """Matches MercuryHourglass.cs: deal 3 to all enemies every player turn start."""
        combat = _make_ironclad_combat(["MercuryHourglass"], seed=103, enemies=2)
        enemy_a, enemy_b = combat.enemies
        round_one_a = enemy_a.current_hp
        round_one_b = enemy_b.current_hp

        assert round_one_a == enemy_a.max_hp - 3
        assert round_one_b == enemy_b.max_hp - 3

        combat.end_player_turn()

        assert enemy_a.current_hp == round_one_a - 3
        assert enemy_b.current_hp == round_one_b - 3

    def test_gremlin_horn_grants_energy_and_draw_when_enemy_dies(self):
        """Matches GremlinHorn.cs: on enemy death, gain 1 energy and draw 1."""
        combat = _make_ironclad_combat(["GremlinHorn"], seed=104)
        enemy = combat.enemies[0]
        enemy.current_hp = 6
        enemy.max_hp = 6
        drawn = make_defend_ironclad()
        combat.hand = [make_strike_ironclad()]
        combat.draw_pile = [drawn]
        combat.energy = 1

        assert combat.play_card(0, 0)
        assert enemy.is_dead
        assert combat.energy == 1
        assert drawn in combat.hand

    def test_cracked_core_channels_lightning_at_first_player_turn_start(self):
        """Matches CrackedCore.cs: first player turn start channels one Lightning orb."""
        combat = _make_defect_combat(["CrackedCore"], seed=105)

        assert combat.orb_queue is not None
        assert len(combat.orb_queue.orbs) == 1
        assert combat.orb_queue.orbs[0].orb_type == OrbType.LIGHTNING
