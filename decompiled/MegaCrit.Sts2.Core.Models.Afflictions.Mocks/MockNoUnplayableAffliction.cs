namespace MegaCrit.Sts2.Core.Models.Afflictions.Mocks;

public sealed class MockNoUnplayableAffliction : AfflictionModel
{
	public override bool CanAfflictUnplayableCards => false;
}
