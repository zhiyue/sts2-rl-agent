using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class ShacklingPotionPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Potion<ShacklingPotion>();

	protected override bool IsPositive => false;
}
