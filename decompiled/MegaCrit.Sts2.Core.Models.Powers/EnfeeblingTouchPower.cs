using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class EnfeeblingTouchPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<EnfeeblingTouch>();

	protected override bool IsPositive => false;
}
