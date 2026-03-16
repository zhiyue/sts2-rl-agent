using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Helpers;

public static class EnergyIconHelper
{
	public static string GetPrefix(AbstractModel model)
	{
		return GetPool(model).EnergyColorName;
	}

	public static string GetPath(AbstractModel model)
	{
		return GetPath(GetPrefix(model));
	}

	public static string GetPath(string prefix)
	{
		return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/energy_" + prefix.ToLowerInvariant() + ".tres");
	}

	private static IPoolModel GetPool(AbstractModel model)
	{
		if (model is IPoolModel result)
		{
			return result;
		}
		Player player = null;
		IPoolModel poolModel = null;
		if (!(model is CardModel cardModel))
		{
			if (!(model is EnchantmentModel enchantmentModel))
			{
				if (!(model is PotionModel potionModel))
				{
					if (!(model is RelicModel relicModel))
					{
						if (model is PowerModel powerModel)
						{
							if (powerModel != null && powerModel.IsMutable)
							{
								Creature owner = powerModel.Owner;
								if (owner != null && owner.IsPlayer)
								{
									player = powerModel.Owner.Player;
									goto IL_0108;
								}
							}
							poolModel = ModelDb.CardPool<ColorlessCardPool>();
						}
					}
					else
					{
						if (relicModel.IsMutable)
						{
							player = relicModel.Owner;
						}
						poolModel = relicModel.Pool;
					}
				}
				else
				{
					if (potionModel.IsMutable)
					{
						player = potionModel.Owner;
					}
					poolModel = potionModel.Pool;
				}
			}
			else if (enchantmentModel.HasCard)
			{
				player = enchantmentModel.Card.Owner;
				poolModel = enchantmentModel.Card.Pool;
			}
			else
			{
				poolModel = ModelDb.CardPool<ColorlessCardPool>();
			}
		}
		else
		{
			if (cardModel.IsMutable)
			{
				player = cardModel.Owner;
			}
			poolModel = cardModel.Pool;
		}
		goto IL_0108;
		IL_0108:
		if (player != null)
		{
			return player.Character.CardPool;
		}
		if (poolModel != null)
		{
			return poolModel;
		}
		Log.Error($"Model {model.Id} is not in any pool! It was probably deprecated without being removed.");
		return ModelDb.CardPool<IroncladCardPool>();
	}
}
