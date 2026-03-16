using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class SharedRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[118]
		{
			ModelDb.Relic<Akabeko>(),
			ModelDb.Relic<AmethystAubergine>(),
			ModelDb.Relic<Anchor>(),
			ModelDb.Relic<ArtOfWar>(),
			ModelDb.Relic<BagOfMarbles>(),
			ModelDb.Relic<BagOfPreparation>(),
			ModelDb.Relic<BeatingRemnant>(),
			ModelDb.Relic<Bellows>(),
			ModelDb.Relic<BeltBuckle>(),
			ModelDb.Relic<BloodVial>(),
			ModelDb.Relic<BookOfFiveRings>(),
			ModelDb.Relic<BowlerHat>(),
			ModelDb.Relic<Bread>(),
			ModelDb.Relic<BronzeScales>(),
			ModelDb.Relic<BurningSticks>(),
			ModelDb.Relic<Candelabra>(),
			ModelDb.Relic<CaptainsWheel>(),
			ModelDb.Relic<Cauldron>(),
			ModelDb.Relic<CentennialPuzzle>(),
			ModelDb.Relic<Chandelier>(),
			ModelDb.Relic<ChemicalX>(),
			ModelDb.Relic<CloakClasp>(),
			ModelDb.Relic<DingyRug>(),
			ModelDb.Relic<DollysMirror>(),
			ModelDb.Relic<DragonFruit>(),
			ModelDb.Relic<EternalFeather>(),
			ModelDb.Relic<FestivePopper>(),
			ModelDb.Relic<FresnelLens>(),
			ModelDb.Relic<FrozenEgg>(),
			ModelDb.Relic<GamblingChip>(),
			ModelDb.Relic<GamePiece>(),
			ModelDb.Relic<GhostSeed>(),
			ModelDb.Relic<Girya>(),
			ModelDb.Relic<GnarledHammer>(),
			ModelDb.Relic<Gorget>(),
			ModelDb.Relic<GremlinHorn>(),
			ModelDb.Relic<HappyFlower>(),
			ModelDb.Relic<HornCleat>(),
			ModelDb.Relic<IceCream>(),
			ModelDb.Relic<IntimidatingHelmet>(),
			ModelDb.Relic<JossPaper>(),
			ModelDb.Relic<JuzuBracelet>(),
			ModelDb.Relic<Kifuda>(),
			ModelDb.Relic<Kunai>(),
			ModelDb.Relic<Kusarigama>(),
			ModelDb.Relic<Lantern>(),
			ModelDb.Relic<LastingCandy>(),
			ModelDb.Relic<LavaLamp>(),
			ModelDb.Relic<LeesWaffle>(),
			ModelDb.Relic<LetterOpener>(),
			ModelDb.Relic<LizardTail>(),
			ModelDb.Relic<LoomingFruit>(),
			ModelDb.Relic<LuckyFysh>(),
			ModelDb.Relic<Mango>(),
			ModelDb.Relic<MealTicket>(),
			ModelDb.Relic<MeatOnTheBone>(),
			ModelDb.Relic<MembershipCard>(),
			ModelDb.Relic<MercuryHourglass>(),
			ModelDb.Relic<MiniatureCannon>(),
			ModelDb.Relic<MiniatureTent>(),
			ModelDb.Relic<MoltenEgg>(),
			ModelDb.Relic<MummifiedHand>(),
			ModelDb.Relic<MysticLighter>(),
			ModelDb.Relic<Nunchaku>(),
			ModelDb.Relic<OddlySmoothStone>(),
			ModelDb.Relic<OldCoin>(),
			ModelDb.Relic<Orichalcum>(),
			ModelDb.Relic<OrnamentalFan>(),
			ModelDb.Relic<Orrery>(),
			ModelDb.Relic<Pantograph>(),
			ModelDb.Relic<ParryingShield>(),
			ModelDb.Relic<Pear>(),
			ModelDb.Relic<PenNib>(),
			ModelDb.Relic<Pendulum>(),
			ModelDb.Relic<Permafrost>(),
			ModelDb.Relic<PetrifiedToad>(),
			ModelDb.Relic<Planisphere>(),
			ModelDb.Relic<Pocketwatch>(),
			ModelDb.Relic<PotionBelt>(),
			ModelDb.Relic<PrayerWheel>(),
			ModelDb.Relic<PunchDagger>(),
			ModelDb.Relic<RainbowRing>(),
			ModelDb.Relic<RazorTooth>(),
			ModelDb.Relic<RedMask>(),
			ModelDb.Relic<RegalPillow>(),
			ModelDb.Relic<ReptileTrinket>(),
			ModelDb.Relic<RingingTriangle>(),
			ModelDb.Relic<RippleBasin>(),
			ModelDb.Relic<RoyalStamp>(),
			ModelDb.Relic<ScreamingFlagon>(),
			ModelDb.Relic<Shovel>(),
			ModelDb.Relic<Shuriken>(),
			ModelDb.Relic<SlingOfCourage>(),
			ModelDb.Relic<SparklingRouge>(),
			ModelDb.Relic<StoneCalendar>(),
			ModelDb.Relic<StoneCracker>(),
			ModelDb.Relic<Strawberry>(),
			ModelDb.Relic<StrikeDummy>(),
			ModelDb.Relic<SturdyClamp>(),
			ModelDb.Relic<TheAbacus>(),
			ModelDb.Relic<TheCourier>(),
			ModelDb.Relic<TinyMailbox>(),
			ModelDb.Relic<Toolbox>(),
			ModelDb.Relic<ToxicEgg>(),
			ModelDb.Relic<TungstenRod>(),
			ModelDb.Relic<TuningFork>(),
			ModelDb.Relic<UnceasingTop>(),
			ModelDb.Relic<UnsettlingLamp>(),
			ModelDb.Relic<Vajra>(),
			ModelDb.Relic<Vambrace>(),
			ModelDb.Relic<VenerableTeaSet>(),
			ModelDb.Relic<VeryHotCocoa>(),
			ModelDb.Relic<VexingPuzzlebox>(),
			ModelDb.Relic<WarPaint>(),
			ModelDb.Relic<Whetstone>(),
			ModelDb.Relic<WhiteBeastStatue>(),
			ModelDb.Relic<WhiteStar>(),
			ModelDb.Relic<WingCharm>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Relic1Epoch>())
		{
			list.RemoveAll((RelicModel r) => Relic1Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Relic2Epoch>())
		{
			list.RemoveAll((RelicModel r) => Relic2Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Relic3Epoch>())
		{
			list.RemoveAll((RelicModel r) => Relic3Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Relic4Epoch>())
		{
			list.RemoveAll((RelicModel r) => Relic4Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Relic5Epoch>())
		{
			list.RemoveAll((RelicModel r) => Relic5Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		return list;
	}
}
