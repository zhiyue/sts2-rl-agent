using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class DyingStarPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<DyingStar>();

	protected override bool IsPositive => false;
}
