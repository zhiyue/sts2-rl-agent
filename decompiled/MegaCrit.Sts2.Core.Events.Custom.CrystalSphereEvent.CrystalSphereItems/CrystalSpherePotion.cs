using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSpherePotion : CrystalSphereItem
{
	private CrystalSphereMinigame _grid;

	private PotionModel _potion;

	protected override string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_" + _potion.Rarity.ToString().ToLowerInvariant() + "_potion.png");

	public override bool IsGood => true;

	public override Vector2I Size
	{
		get
		{
			if (_potion.Rarity != PotionRarity.Rare)
			{
				return new Vector2I(1, 3);
			}
			return new Vector2I(2, 2);
		}
	}

	public CrystalSpherePotion(CrystalSphereMinigame grid, PotionModel potion)
	{
		_grid = grid;
		_potion = potion;
	}

	public override async Task RevealItem(Player owner)
	{
		await base.RevealItem(owner);
		_grid.AddReward(new PotionReward(_potion, owner).SetRng(_grid.Rng));
	}
}
