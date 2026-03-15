"""Act 2 specific events.

Events that only appear in Act 2 (act_index == 1), or require act > 0.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from sts2_env.run.events import EventModel, EventOption, EventResult, register_event

if TYPE_CHECKING:
    from sts2_env.run.run_state import RunState


# ── CrystalSphere ─────────────────────────────────────────────────────

class CrystalSphere(EventModel):
    """Uncover Future: Pay 50-100g for 3 Prophesize picks.
    Payment Plan: Gain Debt curse for 6 Prophesize picks.
    """

    event_id = "CrystalSphere"

    def __init__(self) -> None:
        self._cost = 50

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 100 and run_state.current_act_index > 0

    def calculate_vars(self, run_state: RunState) -> None:
        extra = run_state.rng.up_front.next_int(1, 50)
        self._cost = 50 + extra

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("pay", f"Uncover Future ({self._cost}g)",
                         "3 Prophesize picks"),
            EventOption("debt", "Payment Plan",
                         "6 picks, gain Debt curse"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "pay":
            run_state.player.lose_gold(self._cost)
            return EventResult(finished=True,
                               description=f"Paid {self._cost}g for 3 Prophesize picks.")
        if option_id == "debt":
            return EventResult(finished=True,
                               description="Gained Debt curse for 6 Prophesize picks.")
        return EventResult(finished=True)


register_event(CrystalSphere())


# ── DollRoom ──────────────────────────────────────────────────────────

class DollRoom(EventModel):
    """Multi-page: Choose how to pick a doll (relic).

    Random: Get a random doll relic.
    Take Some Time: Take 5 damage, choose from 2 doll relics.
    Examine: Take 15 damage, choose from all 3 doll relics.
    Dolls: Daughter of the Wind, Mr Struggles, Bing Bong.
    """

    event_id = "DollRoom"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index == 1

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("random", "Random", "Get a random doll relic"),
            EventOption("take_time", "Take Some Time",
                         "Take 5 damage, choose from 2 dolls"),
            EventOption("examine", "Examine",
                         "Take 15 damage, choose from 3 dolls"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "random":
            return EventResult(finished=True,
                               description="Got a random doll relic.")
        if option_id == "take_time":
            run_state.player.lose_hp(5)
            return EventResult(
                finished=False,
                description="Took 5 damage, examining dolls.",
                next_options=[
                    EventOption("doll_1", "Daughter of the Wind",
                                 "Gain Daughter of the Wind relic"),
                    EventOption("doll_2", "Mr Struggles",
                                 "Gain Mr Struggles relic"),
                ],
            )
        if option_id == "examine":
            run_state.player.lose_hp(15)
            return EventResult(
                finished=False,
                description="Took 15 damage, all dolls revealed.",
                next_options=[
                    EventOption("doll_1", "Daughter of the Wind",
                                 "Gain Daughter of the Wind relic"),
                    EventOption("doll_2", "Mr Struggles",
                                 "Gain Mr Struggles relic"),
                    EventOption("doll_3", "Bing Bong",
                                 "Gain Bing Bong relic"),
                ],
            )
        # doll chosen
        return EventResult(finished=True,
                           description="Obtained a doll relic.")


register_event(DollRoom())


# ── EndlessConveyor ───────────────────────────────────────────────────

class EndlessConveyor(EventModel):
    """Conveyor belt sushi bar. Pay 35g per grab for random benefits.

    Dishes: Caviar (+4 Max HP), Clam Roll (Heal 10), Spicy Snappy (Upgrade 1),
    Jelly Liver (Transform 1), Fried Eel (Colorless card), Golden Fysh (+75g),
    Seapunk Salad (Feeding Frenzy card), Suspicious Condiment (Potion).
    Observe Chef: Upgrade 1 card (free).
    """

    event_id = "EndlessConveyor"

    def __init__(self) -> None:
        self._grabs = 0

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 105

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._grabs = 0
        return [
            EventOption("grab", "Grab Something (35g)",
                         "Random dish from the conveyor"),
            EventOption("observe", "Observe Chef",
                         "Upgrade 1 random card (free)"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "observe":
            return EventResult(finished=True,
                               description="Observed chef, upgraded a random card.")
        if option_id == "leave":
            return EventResult(finished=True, description="Left the conveyor.")

        # grab
        if run_state.player.gold >= 35:
            run_state.player.lose_gold(35)
            self._grabs += 1

            can_grab = run_state.player.gold >= 35
            next_opts = []
            if can_grab:
                next_opts.append(EventOption("grab", "Grab Another (35g)",
                                              "Random dish"))
            next_opts.append(EventOption("leave", "Leave", "Done eating"))

            return EventResult(
                finished=False,
                description=f"Grabbed dish #{self._grabs} from the conveyor.",
                next_options=next_opts,
            )
        return EventResult(finished=True, description="Cannot afford more food.")


register_event(EndlessConveyor())


# ── FakeMerchant ──────────────────────────────────────────────────────

class FakeMerchant(EventModel):
    """Custom merchant event: buy fake relics for 50g each.

    Can throw Foul Potion to start a fight for real rewards.
    """

    event_id = "FakeMerchant"

    def is_allowed(self, run_state: RunState) -> bool:
        if run_state.current_act_index < 1:
            return False
        has_foul_potion = any(
            p.potion_id == "FoulPotion"
            for p in run_state.player.held_potions()
        )
        return run_state.player.gold >= 100 or has_foul_potion

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        options = [
            EventOption("buy", "Buy a Relic (50g)", "Purchase a fake relic"),
        ]
        has_foul = any(
            p.potion_id == "FoulPotion"
            for p in run_state.player.held_potions()
        )
        if has_foul:
            options.append(EventOption("throw_foul", "Throw Foul Potion",
                                        "Fight for real rewards"))
        options.append(EventOption("leave", "Leave", "Leave the merchant"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "buy":
            run_state.player.lose_gold(50)
            return EventResult(finished=True,
                               description="Bought a fake relic for 50g.")
        if option_id == "throw_foul":
            return EventResult(finished=True,
                               description="Threw Foul Potion, fought for real rewards.")
        return EventResult(finished=True, description="Left the fake merchant.")


register_event(FakeMerchant())


# ── FieldOfManSizedHoles ──────────────────────────────────────────────

class FieldOfManSizedHoles(EventModel):
    """Resist: Remove 2 cards, gain Normality curse.
    Enter Your Hole: Enchant a card with Perfect Fit.
    """

    event_id = "FieldOfManSizedHoles"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("resist", "Resist",
                         "Remove 2 cards, gain Normality curse"),
            EventOption("enter", "Enter Your Hole",
                         "Enchant a card with Perfect Fit"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "resist":
            return EventResult(finished=True,
                               description="Removed 2 cards, gained Normality curse.")
        return EventResult(finished=True,
                           description="Enchanted a card with Perfect Fit.")


register_event(FieldOfManSizedHoles())


# ── JungleMazeAdventure ──────────────────────────────────────────────

class JungleMazeAdventure(EventModel):
    """Solo Quest: Take 18 damage, gain ~150 gold.
    Join Forces: Gain ~50 gold (safe).
    """

    event_id = "JungleMazeAdventure"

    def __init__(self) -> None:
        self._solo_gold = 150
        self._join_gold = 50

    def calculate_vars(self, run_state: RunState) -> None:
        self._solo_gold = 150 + run_state.rng.up_front.next_int(-15, 16)
        self._join_gold = 50 + run_state.rng.up_front.next_int(-15, 16)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        return [
            EventOption("solo", "Solo Quest",
                         f"Take 18 damage, gain {self._solo_gold} gold"),
            EventOption("join", "Join Forces",
                         f"Gain {self._join_gold} gold"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "solo":
            run_state.player.lose_hp(18)
            run_state.player.gain_gold(self._solo_gold)
            return EventResult(finished=True,
                               description=f"Took 18 damage, gained {self._solo_gold} gold.")
        run_state.player.gain_gold(self._join_gold)
        return EventResult(finished=True,
                           description=f"Gained {self._join_gold} gold.")


register_event(JungleMazeAdventure())


# ── LuminousChoir ─────────────────────────────────────────────────────

class LuminousChoir(EventModel):
    """Reach Into the Flesh: Remove 2 cards, gain Spore Mind curse.
    Offer Tribute: Pay ~100-149g, gain a relic.
    """

    event_id = "LuminousChoir"

    def __init__(self) -> None:
        self._cost = 149

    def calculate_vars(self, run_state: RunState) -> None:
        self._cost = 149 - run_state.rng.up_front.next_int(0, 50)

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self.calculate_vars(run_state)
        options = [
            EventOption("reach", "Reach Into the Flesh",
                         "Remove 2 cards, gain Spore Mind curse"),
        ]
        if run_state.player.gold >= self._cost:
            options.append(EventOption("tribute", f"Offer Tribute ({self._cost}g)",
                                        "Gain a relic"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "reach":
            return EventResult(finished=True,
                               description="Removed 2 cards, gained Spore Mind curse.")
        run_state.player.lose_gold(self._cost)
        return EventResult(finished=True,
                           description=f"Paid {self._cost}g, gained a relic.")


register_event(LuminousChoir())


# ── MorphicGrove ──────────────────────────────────────────────────────

class MorphicGrove(EventModel):
    """Group: Lose 100g, transform 2 cards. Loner: Gain 5 Max HP."""

    event_id = "MorphicGrove"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 100

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("group", "Group",
                         "Lose 100g, transform 2 cards"),
            EventOption("loner", "Loner", "Gain 5 Max HP"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "group":
            run_state.player.lose_gold(100)
            return EventResult(finished=True,
                               description="Lost 100g, transformed 2 cards.")
        run_state.player.gain_max_hp(5)
        return EventResult(finished=True, description="Gained 5 Max HP.")


register_event(MorphicGrove())


# ── PotionCourier ────────────────────────────────────────────────────

class PotionCourier(EventModel):
    """Grab Potions: Gain 3 Foul Potions. Ransack: Gain 1 uncommon potion."""

    event_id = "PotionCourier"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index > 0

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("grab", "Grab Potions", "Gain 3 Foul Potions"),
            EventOption("ransack", "Ransack", "Gain 1 uncommon potion"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "grab":
            return EventResult(finished=True, description="Gained 3 Foul Potions.")
        return EventResult(finished=True, description="Gained an uncommon potion.")


register_event(PotionCourier())


# ── RanwidTheElder ────────────────────────────────────────────────────

class RanwidTheElder(EventModel):
    """Give Potion: Lose a potion, gain a relic.
    Give Gold: Lose 100g, gain a relic.
    Give Relic: Lose a relic, gain 2 relics.
    """

    event_id = "RanwidTheElder"

    def is_allowed(self, run_state: RunState) -> bool:
        return (
            run_state.current_act_index > 0
            and run_state.player.gold >= 100
            and len(run_state.player.held_potions()) > 0
            and len(run_state.relics) > 0
        )

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        options: list[EventOption] = []
        if run_state.player.held_potions():
            options.append(EventOption("potion", "Give a Potion",
                                        "Lose a potion, gain a relic"))
        options.append(EventOption("gold", "Give 100 Gold",
                                    "Lose 100g, gain a relic"))
        if run_state.relics:
            options.append(EventOption("relic", "Give a Relic",
                                        "Lose a relic, gain 2 relics"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "potion":
            return EventResult(finished=True,
                               description="Gave a potion, gained a relic.")
        if option_id == "gold":
            run_state.player.lose_gold(100)
            return EventResult(finished=True,
                               description="Paid 100g, gained a relic.")
        return EventResult(finished=True,
                           description="Gave a relic, gained 2 relics.")


register_event(RanwidTheElder())


# ── RelicTrader ───────────────────────────────────────────────────────

class RelicTrader(EventModel):
    """Trade an owned relic for a new random relic (up to 3 trade options)."""

    event_id = "RelicTrader"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index > 0 and len(run_state.relics) >= 5

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        num_trades = min(3, len(run_state.relics))
        options = []
        for i in range(num_trades):
            options.append(EventOption(f"trade_{i}", f"Trade Relic {i+1}",
                                        "Swap an owned relic for a new one"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True,
                           description="Traded a relic for a new one.")


register_event(RelicTrader())


# ── SlipperyBridge ────────────────────────────────────────────────────

class SlipperyBridge(EventModel):
    """Multi-page: Overcome (lose a random card) or Hold On (take escalating damage).

    Hold On damage starts at 3 and increases by 1 each time.
    Each Hold On rerolls which card would be lost.
    """

    event_id = "SlipperyBridge"

    def __init__(self) -> None:
        self._hold_ons = 0

    def is_allowed(self, run_state: RunState) -> bool:
        has_removable = any(
            c.rarity.name not in ("STATUS", "CURSE")
            for c in run_state.player.deck
        )
        return run_state.total_floor > 6 and has_removable

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        self._hold_ons = 0
        dmg = 3
        return [
            EventOption("overcome", "Overcome", "Lose a random card"),
            EventOption("hold_on", "Hold On", f"Take {dmg} damage, reroll card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "overcome":
            return EventResult(finished=True,
                               description="Lost a random card to cross the bridge.")

        # hold_on
        dmg = 3 + self._hold_ons
        run_state.player.lose_hp(dmg)
        self._hold_ons += 1
        next_dmg = 3 + self._hold_ons

        return EventResult(
            finished=False,
            description=f"Took {dmg} damage, holding on.",
            next_options=[
                EventOption("overcome", "Overcome", "Lose a random card"),
                EventOption("hold_on", "Hold On",
                             f"Take {next_dmg} damage, reroll card"),
            ],
        )


register_event(SlipperyBridge())


# ── SpiralingWhirlpool ────────────────────────────────────────────────

class SpiralingWhirlpool(EventModel):
    """Observe: Enchant a card with Spiral. Drink: Heal 33% of Max HP."""

    event_id = "SpiralingWhirlpool"

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        heal = int(run_state.player.max_hp * 0.33)
        return [
            EventOption("observe", "Observe the Spiral",
                         "Enchant a card with Spiral"),
            EventOption("drink", "Drink", f"Heal {heal} HP"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "observe":
            return EventResult(finished=True,
                               description="Enchanted a card with Spiral.")
        heal = int(run_state.player.max_hp * 0.33)
        run_state.player.heal(heal)
        return EventResult(finished=True, description=f"Healed {heal} HP.")


register_event(SpiralingWhirlpool())


# ── StoneOfAllTime ────────────────────────────────────────────────────

class StoneOfAllTime(EventModel):
    """Lift: Discard a potion, gain 10 Max HP.
    Push: Take 6 damage, enchant a card with Vigorous +8.
    """

    event_id = "StoneOfAllTime"

    def is_allowed(self, run_state: RunState) -> bool:
        return (
            run_state.current_act_index == 1
            and len(run_state.player.held_potions()) >= 1
        )

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("lift", "Lift",
                         "Discard a potion, gain 10 Max HP"),
            EventOption("push", "Push",
                         "Take 6 damage, enchant a card with Vigorous +8"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "lift":
            run_state.player.gain_max_hp(10)
            return EventResult(finished=True,
                               description="Discarded a potion, gained 10 Max HP.")
        run_state.player.lose_hp(6)
        return EventResult(finished=True,
                           description="Took 6 damage, enchanted a card with Vigorous +8.")


register_event(StoneOfAllTime())


# ── Symbiote ──────────────────────────────────────────────────────────

class Symbiote(EventModel):
    """Approach: Enchant a card with Corrupted.
    Kill with Fire: Transform 1 card.
    """

    event_id = "Symbiote"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index > 0

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("approach", "Approach",
                         "Enchant a card with Corrupted"),
            EventOption("kill_fire", "Kill with Fire", "Transform 1 card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "approach":
            return EventResult(finished=True,
                               description="Enchanted a card with Corrupted.")
        return EventResult(finished=True, description="Transformed 1 card.")


register_event(Symbiote())


# ── TheFutureOfPotions ────────────────────────────────────────────────

class TheFutureOfPotions(EventModel):
    """Trade a potion for upgraded card rewards matching rarity."""

    event_id = "TheFutureOfPotions"

    def is_allowed(self, run_state: RunState) -> bool:
        return len(run_state.player.held_potions()) >= 2

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        potions = run_state.player.held_potions()
        options = []
        for i, p in enumerate(potions[:3]):
            options.append(
                EventOption(f"trade_{i}", f"Trade {p.potion_id}",
                             "Discard potion for upgraded card reward")
            )
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        return EventResult(finished=True,
                           description="Traded a potion for upgraded card rewards.")


register_event(TheFutureOfPotions())


# ── WaterloggedScriptorium ────────────────────────────────────────────

class WaterloggedScriptorium(EventModel):
    """Bloody Ink: Gain 6 Max HP.
    Tentacle Quill: Pay 65g, enchant 1 card with Steady.
    Prickly Sponge: Pay 155g, enchant 2 cards with Steady.
    """

    event_id = "WaterloggedScriptorium"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 65

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        gold = run_state.player.gold
        options = [
            EventOption("bloody_ink", "Bloody Ink", "Gain 6 Max HP"),
        ]
        if gold >= 65:
            options.append(EventOption("tentacle_quill", "Tentacle Quill (65g)",
                                        "Enchant 1 card with Steady"))
        if gold >= 155:
            options.append(EventOption("prickly_sponge", "Prickly Sponge (155g)",
                                        "Enchant 2 cards with Steady"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bloody_ink":
            run_state.player.gain_max_hp(6)
            return EventResult(finished=True, description="Gained 6 Max HP.")
        if option_id == "tentacle_quill":
            run_state.player.lose_gold(65)
            return EventResult(finished=True,
                               description="Paid 65g, enchanted 1 card with Steady.")
        run_state.player.lose_gold(155)
        return EventResult(finished=True,
                           description="Paid 155g, enchanted 2 cards with Steady.")


register_event(WaterloggedScriptorium())


# ── WelcomeToWongos ───────────────────────────────────────────────────

class WelcomeToWongos(EventModel):
    """Bargain Bin: 100g for common relic. Featured Item: 200g for rare relic.
    Mystery Box: 300g for mystery ticket. Leave: Downgrade a random card.
    """

    event_id = "WelcomeToWongos"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.current_act_index == 1 and run_state.player.gold >= 100

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        gold = run_state.player.gold
        options: list[EventOption] = []
        if gold >= 100:
            options.append(EventOption("bargain_bin", "Bargain Bin (100g)",
                                        "Common relic"))
        if gold >= 200:
            options.append(EventOption("featured", "Featured Item (200g)",
                                        "Rare relic"))
        if gold >= 300:
            options.append(EventOption("mystery", "Mystery Box (300g)",
                                        "Mystery ticket relic"))
        options.append(EventOption("leave", "Leave",
                                    "Downgrade a random upgraded card"))
        return options

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "bargain_bin":
            run_state.player.lose_gold(100)
            return EventResult(finished=True,
                               description="Bought common relic for 100g.")
        if option_id == "featured":
            run_state.player.lose_gold(200)
            return EventResult(finished=True,
                               description="Bought rare relic for 200g.")
        if option_id == "mystery":
            run_state.player.lose_gold(300)
            return EventResult(finished=True,
                               description="Bought mystery box for 300g.")
        return EventResult(finished=True,
                           description="Left Wongo's, downgraded a card.")


register_event(WelcomeToWongos())


# ── WhisperingHollow ──────────────────────────────────────────────────

class WhisperingHollow(EventModel):
    """Gold: Pay 50g, gain 2 potions. Hug: Take 9 damage, transform 1 card."""

    event_id = "WhisperingHollow"

    def is_allowed(self, run_state: RunState) -> bool:
        return run_state.player.gold >= 50

    def generate_initial_options(self, run_state: RunState) -> list[EventOption]:
        return [
            EventOption("gold", "Pay Gold (50g)", "Gain 2 potions"),
            EventOption("hug", "Hug", "Take 9 damage, transform 1 card"),
        ]

    def choose(self, run_state: RunState, option_id: str) -> EventResult:
        if option_id == "gold":
            run_state.player.lose_gold(50)
            return EventResult(finished=True,
                               description="Paid 50g, gained 2 potions.")
        run_state.player.lose_hp(9)
        return EventResult(finished=True,
                           description="Took 9 damage, transformed 1 card.")


register_event(WhisperingHollow())
