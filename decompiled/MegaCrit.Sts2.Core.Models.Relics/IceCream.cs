using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class IceCream : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShouldPlayerResetEnergy(Player player)
	{
		if (player != base.Owner)
		{
			return true;
		}
		if (base.Owner.PlayerCombatState.TurnNumber == 1)
		{
			return true;
		}
		return false;
	}
}
