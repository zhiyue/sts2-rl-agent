using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class FlexPotionPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Potion<FlexPotion>();
}
