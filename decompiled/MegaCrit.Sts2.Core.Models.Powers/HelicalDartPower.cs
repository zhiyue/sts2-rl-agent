using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class HelicalDartPower : TemporaryDexterityPower
{
	public override AbstractModel OriginModel => ModelDb.Relic<HelicalDart>();
}
