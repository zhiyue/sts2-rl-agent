using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class HotfixPower : TemporaryFocusPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Hotfix>();
}
