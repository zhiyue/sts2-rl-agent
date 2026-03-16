using MegaCrit.Sts2.Core.Models.Cards.Mocks;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public class MockTemporaryStrengthLossPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<MockSkillCard>();

	protected override bool IsPositive => false;
}
