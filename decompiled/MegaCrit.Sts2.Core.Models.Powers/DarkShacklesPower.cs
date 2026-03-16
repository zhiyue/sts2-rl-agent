using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class DarkShacklesPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<DarkShackles>();

	protected override bool IsPositive => false;
}
