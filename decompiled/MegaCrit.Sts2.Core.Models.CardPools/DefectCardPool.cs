using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class DefectCardPool : CardPoolModel
{
	public override string Title => "defect";

	public override string EnergyColorName => "defect";

	public override string CardFrameMaterialPath => "card_frame_blue";

	public override Color DeckEntryCardColor => new Color("3EB3ED");

	public override Color EnergyOutlineColor => new Color("1D5673");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[88]
		{
			ModelDb.Card<AdaptiveStrike>(),
			ModelDb.Card<AllForOne>(),
			ModelDb.Card<BallLightning>(),
			ModelDb.Card<Barrage>(),
			ModelDb.Card<BeamCell>(),
			ModelDb.Card<BiasedCognition>(),
			ModelDb.Card<BoostAway>(),
			ModelDb.Card<BootSequence>(),
			ModelDb.Card<Buffer>(),
			ModelDb.Card<BulkUp>(),
			ModelDb.Card<Capacitor>(),
			ModelDb.Card<Chaos>(),
			ModelDb.Card<ChargeBattery>(),
			ModelDb.Card<Chill>(),
			ModelDb.Card<Claw>(),
			ModelDb.Card<ColdSnap>(),
			ModelDb.Card<Compact>(),
			ModelDb.Card<CompileDriver>(),
			ModelDb.Card<ConsumingShadow>(),
			ModelDb.Card<Coolant>(),
			ModelDb.Card<Coolheaded>(),
			ModelDb.Card<CreativeAi>(),
			ModelDb.Card<Darkness>(),
			ModelDb.Card<DefendDefect>(),
			ModelDb.Card<Defragment>(),
			ModelDb.Card<DoubleEnergy>(),
			ModelDb.Card<Dualcast>(),
			ModelDb.Card<EchoForm>(),
			ModelDb.Card<EnergySurge>(),
			ModelDb.Card<Feral>(),
			ModelDb.Card<FightThrough>(),
			ModelDb.Card<FlakCannon>(),
			ModelDb.Card<FocusedStrike>(),
			ModelDb.Card<Ftl>(),
			ModelDb.Card<Fusion>(),
			ModelDb.Card<GeneticAlgorithm>(),
			ModelDb.Card<Glacier>(),
			ModelDb.Card<Glasswork>(),
			ModelDb.Card<GoForTheEyes>(),
			ModelDb.Card<GunkUp>(),
			ModelDb.Card<Hailstorm>(),
			ModelDb.Card<HelixDrill>(),
			ModelDb.Card<Hologram>(),
			ModelDb.Card<Hotfix>(),
			ModelDb.Card<Hyperbeam>(),
			ModelDb.Card<IceLance>(),
			ModelDb.Card<Ignition>(),
			ModelDb.Card<Iteration>(),
			ModelDb.Card<Leap>(),
			ModelDb.Card<LightningRod>(),
			ModelDb.Card<Loop>(),
			ModelDb.Card<MachineLearning>(),
			ModelDb.Card<MeteorStrike>(),
			ModelDb.Card<Modded>(),
			ModelDb.Card<MomentumStrike>(),
			ModelDb.Card<MultiCast>(),
			ModelDb.Card<Null>(),
			ModelDb.Card<Overclock>(),
			ModelDb.Card<Quadcast>(),
			ModelDb.Card<Rainbow>(),
			ModelDb.Card<Reboot>(),
			ModelDb.Card<Refract>(),
			ModelDb.Card<RocketPunch>(),
			ModelDb.Card<Scavenge>(),
			ModelDb.Card<Scrape>(),
			ModelDb.Card<ShadowShield>(),
			ModelDb.Card<Shatter>(),
			ModelDb.Card<SignalBoost>(),
			ModelDb.Card<Skim>(),
			ModelDb.Card<Smokestack>(),
			ModelDb.Card<Spinner>(),
			ModelDb.Card<Storm>(),
			ModelDb.Card<StrikeDefect>(),
			ModelDb.Card<Subroutine>(),
			ModelDb.Card<Sunder>(),
			ModelDb.Card<Supercritical>(),
			ModelDb.Card<SweepingBeam>(),
			ModelDb.Card<Synchronize>(),
			ModelDb.Card<Synthesis>(),
			ModelDb.Card<Tempest>(),
			ModelDb.Card<TeslaCoil>(),
			ModelDb.Card<Thunder>(),
			ModelDb.Card<TrashToTreasure>(),
			ModelDb.Card<Turbo>(),
			ModelDb.Card<Uproar>(),
			ModelDb.Card<Voltaic>(),
			ModelDb.Card<WhiteNoise>(),
			ModelDb.Card<Zap>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Defect2Epoch>())
		{
			list.RemoveAll((CardModel c) => Defect2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Defect5Epoch>())
		{
			list.RemoveAll((CardModel c) => Defect5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Defect7Epoch>())
		{
			list.RemoveAll((CardModel c) => Defect7Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
