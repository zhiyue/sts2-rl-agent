using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSpherePotion : CrystalSphereItem
{
	private readonly PotionRarity _rarity;

	protected override string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_" + _rarity.ToString().ToLowerInvariant() + "_potion.png");

	public override bool IsGood => true;

	public override Vector2I Size
	{
		get
		{
			if (_rarity != PotionRarity.Rare)
			{
				return new Vector2I(1, 3);
			}
			return new Vector2I(2, 2);
		}
	}

	public CrystalSpherePotion(PotionRarity rarity)
	{
		_rarity = rarity;
	}

	public override Reward? ToReward(Player owner, Rng rng)
	{
		IEnumerable<PotionModel> items = from p in PotionFactory.GetPotionOptions(owner, Array.Empty<PotionModel>())
			where p.Rarity == _rarity
			select p;
		PotionModel potion = rng.NextItem(items).ToMutable();
		return new PotionReward(potion, owner).SetRng(rng);
	}

	public override SerializableCrystalSphereItem ToSerializable()
	{
		return new SerializableCrystalSphereItem
		{
			type = CrystalSphereItemType.Potion,
			potionRarity = _rarity
		};
	}
}
