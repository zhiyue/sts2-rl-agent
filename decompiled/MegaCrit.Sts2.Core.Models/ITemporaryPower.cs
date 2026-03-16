namespace MegaCrit.Sts2.Core.Models;

public interface ITemporaryPower
{
	AbstractModel OriginModel { get; }

	PowerModel InternallyAppliedPower { get; }

	void IgnoreNextInstance();
}
