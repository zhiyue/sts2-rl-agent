using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Models;

public class DeterministicModelComparer : IComparer<AbstractModel>
{
	public static DeterministicModelComparer Instance { get; } = new DeterministicModelComparer();

	private DeterministicModelComparer()
	{
	}

	public int Compare(AbstractModel? model1, AbstractModel? model2)
	{
		if (model1 == null && model2 == null)
		{
			return 0;
		}
		if (model1 == model2)
		{
			return 0;
		}
		if (model1 == null)
		{
			return -1;
		}
		if (model2 == null)
		{
			return 1;
		}
		if (model1.CategorySortingId != model2.CategorySortingId)
		{
			return model1.CategorySortingId.CompareTo(model2.CategorySortingId);
		}
		if (model1.EntrySortingId != model2.EntrySortingId)
		{
			return model1.EntrySortingId.CompareTo(model2.EntrySortingId);
		}
		Creature owner = GetOwner(model1);
		Creature owner2 = GetOwner(model2);
		if (owner == null && owner2 == null)
		{
			return 0;
		}
		if (owner == null)
		{
			return -1;
		}
		if (owner2 == null)
		{
			return 1;
		}
		if (owner != owner2)
		{
			if (owner.IsPlayer && owner2.IsPlayer)
			{
				return owner.Player.NetId.CompareTo(owner2.Player.NetId);
			}
			if (owner.CombatId.HasValue && owner2.CombatId.HasValue)
			{
				return owner.CombatId.Value.CompareTo(owner2.CombatId.Value);
			}
			if (owner.CombatId.HasValue)
			{
				return 1;
			}
			return -1;
		}
		return CompareModelsWithSameOwnerAndId(model1, model2);
	}

	private static Creature? GetOwner(AbstractModel model)
	{
		if (!(model is CardModel cardModel))
		{
			if (!(model is RelicModel relicModel))
			{
				if (!(model is PotionModel potionModel))
				{
					if (!(model is PowerModel powerModel))
					{
						if (!(model is AfflictionModel afflictionModel))
						{
							if (model is EnchantmentModel enchantmentModel)
							{
								return enchantmentModel.Card.Owner.Creature;
							}
							return null;
						}
						return afflictionModel.Card.Owner.Creature;
					}
					return powerModel.Owner;
				}
				return potionModel.Owner.Creature;
			}
			return relicModel.Owner.Creature;
		}
		return cardModel.Owner.Creature;
	}

	private static int CompareModelsWithSameOwnerAndId(AbstractModel model1, AbstractModel model2)
	{
		if (model1 is CardModel cardModel && model2 is CardModel cardModel2)
		{
			return CompareCardModelsWithSameOwnerAndId(cardModel, cardModel2);
		}
		if (model1 is AfflictionModel afflictionModel && model2 is AfflictionModel afflictionModel2)
		{
			return CompareCardModelsWithSameOwnerAndId(afflictionModel.Card, afflictionModel2.Card);
		}
		if (model1 is EnchantmentModel enchantmentModel && model2 is EnchantmentModel enchantmentModel2)
		{
			return CompareCardModelsWithSameOwnerAndId(enchantmentModel.Card, enchantmentModel2.Card);
		}
		if (model1 is RelicModel relicModel && model2 is RelicModel relicModel2)
		{
			return relicModel.Owner.Relics.IndexOf(relicModel).CompareTo(relicModel2.Owner.Relics.IndexOf(relicModel2));
		}
		if (model1 is PotionModel potionModel && model2 is PotionModel potionModel2)
		{
			return potionModel.Owner.PotionSlots.IndexOf<PotionModel>(potionModel).CompareTo(potionModel2.Owner.PotionSlots.IndexOf<PotionModel>(potionModel2));
		}
		if (model1 is PowerModel powerModel && model2 is PowerModel powerModel2)
		{
			return powerModel.Owner.Powers.IndexOf(powerModel).CompareTo(powerModel2.Owner.Powers.IndexOf(powerModel2));
		}
		throw new InvalidOperationException($"Tried to compare equivalent models {model1} and {model2} but we can't map them to any know model types!");
	}

	private static int CompareCardModelsWithSameOwnerAndId(CardModel cardModel1, CardModel cardModel2)
	{
		if (cardModel1.Pile == null && cardModel2.Pile == null)
		{
			return 0;
		}
		if (cardModel1.Pile == null)
		{
			return 1;
		}
		if (cardModel2.Pile == null)
		{
			return -1;
		}
		if (cardModel1.Pile.Type != cardModel2.Pile.Type)
		{
			return cardModel1.Pile.Type.CompareTo(cardModel2.Pile.Type);
		}
		return cardModel1.Pile.Cards.IndexOf(cardModel1).CompareTo(cardModel2.Pile.Cards.IndexOf(cardModel2));
	}
}
