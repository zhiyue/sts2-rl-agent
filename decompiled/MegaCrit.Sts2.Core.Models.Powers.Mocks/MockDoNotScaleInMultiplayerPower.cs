using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public class MockDoNotScaleInMultiplayerPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool AllowNegative => true;

	public override bool ShouldScaleInMultiplayer => false;
}
