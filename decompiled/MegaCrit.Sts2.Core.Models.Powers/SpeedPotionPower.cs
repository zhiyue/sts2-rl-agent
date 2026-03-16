using MegaCrit.Sts2.Core.Models.Potions;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class SpeedPotionPower : TemporaryDexterityPower
{
	public override AbstractModel OriginModel => ModelDb.Potion<SpeedPotion>();
}
