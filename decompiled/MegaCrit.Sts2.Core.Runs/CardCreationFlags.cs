using System;

namespace MegaCrit.Sts2.Core.Runs;

[Flags]
public enum CardCreationFlags
{
	NoRarityModification = 1,
	NoUpgradeRoll = 2,
	NoHookUpgrades = 4,
	NoModifyHooks = 8,
	NoCardPoolModifications = 0x10,
	NoCardModelModifications = 0x20,
	ForceRarityOddsChange = 0x40,
	NoUpgrades = 6,
	NoModifications = -1
}
