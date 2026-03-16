using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

public sealed class SharedPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<PotionModel>(new PotionModel[45]
		{
			ModelDb.Potion<AttackPotion>(),
			ModelDb.Potion<BeetleJuice>(),
			ModelDb.Potion<BlessingOfTheForge>(),
			ModelDb.Potion<BlockPotion>(),
			ModelDb.Potion<BottledPotential>(),
			ModelDb.Potion<Clarity>(),
			ModelDb.Potion<ColorlessPotion>(),
			ModelDb.Potion<CureAll>(),
			ModelDb.Potion<DexterityPotion>(),
			ModelDb.Potion<DistilledChaos>(),
			ModelDb.Potion<DropletOfPrecognition>(),
			ModelDb.Potion<Duplicator>(),
			ModelDb.Potion<EnergyPotion>(),
			ModelDb.Potion<EntropicBrew>(),
			ModelDb.Potion<ExplosiveAmpoule>(),
			ModelDb.Potion<FairyInABottle>(),
			ModelDb.Potion<FirePotion>(),
			ModelDb.Potion<FlexPotion>(),
			ModelDb.Potion<Fortifier>(),
			ModelDb.Potion<FruitJuice>(),
			ModelDb.Potion<FyshOil>(),
			ModelDb.Potion<GamblersBrew>(),
			ModelDb.Potion<GigantificationPotion>(),
			ModelDb.Potion<HeartOfIron>(),
			ModelDb.Potion<LiquidBronze>(),
			ModelDb.Potion<LiquidMemories>(),
			ModelDb.Potion<LuckyTonic>(),
			ModelDb.Potion<MazalethsGift>(),
			ModelDb.Potion<OrobicAcid>(),
			ModelDb.Potion<PotionOfBinding>(),
			ModelDb.Potion<PowderedDemise>(),
			ModelDb.Potion<PowerPotion>(),
			ModelDb.Potion<RadiantTincture>(),
			ModelDb.Potion<RegenPotion>(),
			ModelDb.Potion<ShacklingPotion>(),
			ModelDb.Potion<ShipInABottle>(),
			ModelDb.Potion<SkillPotion>(),
			ModelDb.Potion<SneckoOil>(),
			ModelDb.Potion<SpeedPotion>(),
			ModelDb.Potion<StableSerum>(),
			ModelDb.Potion<StrengthPotion>(),
			ModelDb.Potion<SwiftPotion>(),
			ModelDb.Potion<TouchOfInsanity>(),
			ModelDb.Potion<VulnerablePotion>(),
			ModelDb.Potion<WeakPotion>()
		});
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		List<PotionModel> list = base.AllPotions.ToList();
		if (!unlockState.IsEpochRevealed<Potion1Epoch>())
		{
			foreach (PotionModel potion in Potion1Epoch.Potions)
			{
				list.Remove(potion);
			}
		}
		if (!unlockState.IsEpochRevealed<Potion2Epoch>())
		{
			foreach (PotionModel potion2 in Potion2Epoch.Potions)
			{
				list.Remove(potion2);
			}
		}
		return list;
	}
}
