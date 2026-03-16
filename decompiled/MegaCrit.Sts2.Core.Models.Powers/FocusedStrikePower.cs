using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class FocusedStrikePower : TemporaryFocusPower
{
	public override AbstractModel OriginModel => ModelDb.Card<FocusedStrike>();
}
