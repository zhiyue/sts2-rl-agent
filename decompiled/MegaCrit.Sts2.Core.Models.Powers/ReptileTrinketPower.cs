using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class ReptileTrinketPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Relic<ReptileTrinket>();
}
