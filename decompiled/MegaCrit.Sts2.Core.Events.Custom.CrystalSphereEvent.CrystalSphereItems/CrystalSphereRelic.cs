using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSphereRelic : CrystalSphereItem
{
	private CrystalSphereMinigame _grid;

	public override Vector2I Size => new Vector2I(4, 4);

	public override bool IsGood => true;

	protected override string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_relic.png");

	public CrystalSphereRelic(CrystalSphereMinigame grid)
	{
		_grid = grid;
	}

	public override async Task RevealItem(Player owner)
	{
		await base.RevealItem(owner);
		_grid.AddReward(new RelicReward(owner).SetRng(_grid.Rng));
	}
}
