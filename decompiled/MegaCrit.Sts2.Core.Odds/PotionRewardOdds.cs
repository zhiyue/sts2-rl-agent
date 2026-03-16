using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Odds;

public class PotionRewardOdds : AbstractOdds
{
	public const float targetOdds = 0.5f;

	public const float eliteBonus = 0.25f;

	private const float _basePotionRewardOdds = 0.4f;

	public PotionRewardOdds(Rng rng)
		: base(0.4f, rng)
	{
	}

	public PotionRewardOdds(float initialValue, Rng rng)
		: base(initialValue, rng)
	{
	}

	public bool Roll(Player player, AscensionManager ascensionManager, RoomType roomType)
	{
		float currentValue = base.CurrentValue;
		bool flag = Hook.ShouldForcePotionReward(player.RunState, player, roomType);
		float num = _rng.NextFloat();
		if (num < currentValue || flag)
		{
			base.CurrentValue -= 0.1f;
		}
		else
		{
			base.CurrentValue += 0.1f;
		}
		float num2 = ((roomType != RoomType.Elite) ? 0f : 0.25f);
		float num3 = num2;
		if (!flag)
		{
			return num < currentValue + num3 * 0.5f;
		}
		return true;
	}
}
