using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class FeedingFrenzyPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<FeedingFrenzy>();
}
