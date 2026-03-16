using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Potions;

public class PotionProcureResult
{
	public bool success;

	public PotionModel potion;

	public PotionProcureFailureReason failureReason;
}
