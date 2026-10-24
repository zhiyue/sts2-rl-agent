using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSphereRelic : CrystalSphereItem
{
	public override Vector2I Size => new Vector2I(4, 4);

	public override bool IsGood => true;

	protected override string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_relic.png");

	public override Reward? ToReward(Player owner, Rng rng)
	{
		return new RelicReward(owner).SetRng(rng);
	}

	public override SerializableCrystalSphereItem ToSerializable()
	{
		return new SerializableCrystalSphereItem
		{
			type = CrystalSphereItemType.Relic
		};
	}
}
