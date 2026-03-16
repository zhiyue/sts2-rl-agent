using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSphereGold : CrystalSphereItem
{
	private readonly CrystalSphereMinigame _grid;

	private readonly bool _isBig;

	private const int _smallAmount = 10;

	private const int _largeAmount = 30;

	protected override string TexturePath
	{
		get
		{
			if (!_isBig)
			{
				return ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_gold.png");
			}
			return ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_big_gold.png");
		}
	}

	private int Amount
	{
		get
		{
			if (!_isBig)
			{
				return 10;
			}
			return 30;
		}
	}

	public override Vector2I Size
	{
		get
		{
			if (!_isBig)
			{
				return Vector2I.One;
			}
			return new Vector2I(2, 1);
		}
	}

	public override bool IsGood => true;

	public CrystalSphereGold(CrystalSphereMinigame grid, bool isBig)
	{
		_grid = grid;
		_isBig = isBig;
	}

	public override async Task RevealItem(Player owner)
	{
		await base.RevealItem(owner);
		_grid.AddReward(new GoldReward(Amount, owner).SetRng(_grid.Rng));
	}
}
