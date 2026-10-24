using System;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Various flags which can be passed to the CardCreationOptions to control card generation behavior.
/// This controls both built-in behavior (rolling for upgrades in rewards) as well as hook behavior (Molten Egg, Ruby
/// Earrings).
/// </summary>
[Flags]
public enum CardCreationFlags
{
	/// <summary>
	/// Indicates that rarity should not be modified.
	/// This should be used when an event option specifies the rarity. For example, <see cref="T:MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese" />
	/// specifies that its rewards are uncommon, so no relics should modify the rarities.
	/// </summary>
	NoRarityModification = 1,
	/// <summary>
	/// Indicates that the built-in reward upgrade should not be applied.
	/// When offering combat rewards, there is a chance that cards are offered upgraded. This should not apply to most
	/// event rewards, but it does for some other rewards like those offered from <see cref="T:MegaCrit.Sts2.Core.Models.Relics.Orrery" />.
	/// </summary>
	NoUpgradeRoll = 2,
	/// <summary>
	/// Indicates that all modify hooks that upgrade cards should not apply their effect.
	/// Applies to things like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.MoltenEgg" />.
	/// </summary>
	NoHookUpgrades = 4,
	/// <summary>
	/// Indicates that no modify hooks should be run.
	/// This is used when a modify hook itself generates a card, to prevent infinite looping. For example,
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.LastingCandy" /> uses this when generating new rewards so that it itself doesn't apply to the reward
	/// it generates.
	/// </summary>
	NoModifyHooks = 8,
	/// <summary>
	/// Indicates that the card pool passed to the reward generation should not be modified.
	/// Relics like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PrismaticGem" /> can modify the reward pool passed into the reward generation. However,
	/// relics like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.LastingCandy" /> generate a card from a pool of only powers, which should not be modified
	/// by hooks.
	/// </summary>
	NoCardPoolModifications = 0x10,
	/// <summary>
	/// Indicates that the cards passed to the reward generation should not be modified.
	/// FTUE can manually set the cards that are offered to the player. <see cref="T:MegaCrit.Sts2.Core.Models.Relics.SilverCrucible" /> should be allowed
	/// to upgraded these cards (the player can get it if they're playing their first run in multiplayer), but Prismatic
	/// Shard should not be allowed to change the cards.
	/// </summary>
	NoCardModelModifications = 0x20,
	/// <summary>
	/// Indicates that the odds of rolling rare cards should be modified and that we should use the modified odds, even
	/// if this is not an encounter roll.
	/// </summary>
	ForceRarityOddsChange = 0x40,
	/// <summary>
	/// Indicates that the cards created other specifically for a card reward. It is explicitly added in  <see cref="T:MegaCrit.Sts2.Core.Rewards.CardReward" />.
	/// Important for relics like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PrismaticGem" /> or <see cref="T:MegaCrit.Sts2.Core.Models.Relics.DingyRug" /> that only affect card rewards.
	/// </summary>
	IsCardReward = 0x80,
	/// <summary>
	/// Indicates that no upgrades should be applied to the generated card at all.
	/// </summary>
	NoUpgrades = 6,
	/// <summary>
	/// Specifies that the pool and cards generated may not be modified.
	/// </summary>
	NoModifications = -1
}
