using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class AnticipatePower : TemporaryDexterityPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Anticipate>();
}
