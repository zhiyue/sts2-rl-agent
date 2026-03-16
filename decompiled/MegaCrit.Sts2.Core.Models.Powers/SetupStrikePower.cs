using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class SetupStrikePower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<SetupStrike>();
}
