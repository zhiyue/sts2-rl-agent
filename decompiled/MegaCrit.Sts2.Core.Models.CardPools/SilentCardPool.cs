using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class SilentCardPool : CardPoolModel
{
	public override string Title => "silent";

	public override string EnergyColorName => "silent";

	public override string CardFrameMaterialPath => "card_frame_green";

	public override Color DeckEntryCardColor => new Color("5EBD00");

	public override Color EnergyOutlineColor => new Color("1A6625");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[88]
		{
			ModelDb.Card<Abrasive>(),
			ModelDb.Card<Accelerant>(),
			ModelDb.Card<Accuracy>(),
			ModelDb.Card<Acrobatics>(),
			ModelDb.Card<Adrenaline>(),
			ModelDb.Card<Afterimage>(),
			ModelDb.Card<Anticipate>(),
			ModelDb.Card<Assassinate>(),
			ModelDb.Card<Backflip>(),
			ModelDb.Card<Backstab>(),
			ModelDb.Card<BladeOfInk>(),
			ModelDb.Card<BladeDance>(),
			ModelDb.Card<Blur>(),
			ModelDb.Card<BouncingFlask>(),
			ModelDb.Card<BubbleBubble>(),
			ModelDb.Card<BulletTime>(),
			ModelDb.Card<Burst>(),
			ModelDb.Card<CalculatedGamble>(),
			ModelDb.Card<CloakAndDagger>(),
			ModelDb.Card<CorrosiveWave>(),
			ModelDb.Card<DaggerSpray>(),
			ModelDb.Card<DaggerThrow>(),
			ModelDb.Card<Dash>(),
			ModelDb.Card<DeadlyPoison>(),
			ModelDb.Card<DefendSilent>(),
			ModelDb.Card<Deflect>(),
			ModelDb.Card<DodgeAndRoll>(),
			ModelDb.Card<EchoingSlash>(),
			ModelDb.Card<Envenom>(),
			ModelDb.Card<EscapePlan>(),
			ModelDb.Card<Expertise>(),
			ModelDb.Card<Expose>(),
			ModelDb.Card<FanOfKnives>(),
			ModelDb.Card<Finisher>(),
			ModelDb.Card<Flanking>(),
			ModelDb.Card<Flechettes>(),
			ModelDb.Card<FlickFlack>(),
			ModelDb.Card<FollowThrough>(),
			ModelDb.Card<Footwork>(),
			ModelDb.Card<GrandFinale>(),
			ModelDb.Card<HandTrick>(),
			ModelDb.Card<Haze>(),
			ModelDb.Card<HiddenDaggers>(),
			ModelDb.Card<InfiniteBlades>(),
			ModelDb.Card<KnifeTrap>(),
			ModelDb.Card<LeadingStrike>(),
			ModelDb.Card<LegSweep>(),
			ModelDb.Card<Malaise>(),
			ModelDb.Card<MasterPlanner>(),
			ModelDb.Card<MementoMori>(),
			ModelDb.Card<Mirage>(),
			ModelDb.Card<Murder>(),
			ModelDb.Card<Neutralize>(),
			ModelDb.Card<Nightmare>(),
			ModelDb.Card<NoxiousFumes>(),
			ModelDb.Card<Outbreak>(),
			ModelDb.Card<PhantomBlades>(),
			ModelDb.Card<PiercingWail>(),
			ModelDb.Card<Pinpoint>(),
			ModelDb.Card<PoisonedStab>(),
			ModelDb.Card<Pounce>(),
			ModelDb.Card<PreciseCut>(),
			ModelDb.Card<Predator>(),
			ModelDb.Card<Prepared>(),
			ModelDb.Card<Reflex>(),
			ModelDb.Card<Ricochet>(),
			ModelDb.Card<SerpentForm>(),
			ModelDb.Card<ShadowStep>(),
			ModelDb.Card<Shadowmeld>(),
			ModelDb.Card<Skewer>(),
			ModelDb.Card<Slice>(),
			ModelDb.Card<Snakebite>(),
			ModelDb.Card<Sneaky>(),
			ModelDb.Card<Speedster>(),
			ModelDb.Card<StormOfSteel>(),
			ModelDb.Card<Strangle>(),
			ModelDb.Card<StrikeSilent>(),
			ModelDb.Card<SuckerPunch>(),
			ModelDb.Card<Suppress>(),
			ModelDb.Card<Survivor>(),
			ModelDb.Card<Tactician>(),
			ModelDb.Card<TheHunt>(),
			ModelDb.Card<ToolsOfTheTrade>(),
			ModelDb.Card<Tracking>(),
			ModelDb.Card<Untouchable>(),
			ModelDb.Card<UpMySleeve>(),
			ModelDb.Card<WellLaidPlans>(),
			ModelDb.Card<WraithForm>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Silent2Epoch>())
		{
			list.RemoveAll((CardModel c) => Silent2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Silent5Epoch>())
		{
			list.RemoveAll((CardModel c) => Silent5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Silent7Epoch>())
		{
			list.RemoveAll((CardModel c) => Silent7Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
