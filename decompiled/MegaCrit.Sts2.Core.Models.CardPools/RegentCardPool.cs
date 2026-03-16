using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class RegentCardPool : CardPoolModel
{
	public override string Title => "regent";

	public override string EnergyColorName => "regent";

	public override string CardFrameMaterialPath => "card_frame_orange";

	public override Color DeckEntryCardColor => new Color("E36600");

	public override Color EnergyOutlineColor => new Color("803D0E");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[88]
		{
			ModelDb.Card<Alignment>(),
			ModelDb.Card<Arsenal>(),
			ModelDb.Card<AstralPulse>(),
			ModelDb.Card<BeatIntoShape>(),
			ModelDb.Card<Begone>(),
			ModelDb.Card<BigBang>(),
			ModelDb.Card<BlackHole>(),
			ModelDb.Card<Bombardment>(),
			ModelDb.Card<Bulwark>(),
			ModelDb.Card<BundleOfJoy>(),
			ModelDb.Card<CelestialMight>(),
			ModelDb.Card<Charge>(),
			ModelDb.Card<ChildOfTheStars>(),
			ModelDb.Card<CloakOfStars>(),
			ModelDb.Card<CollisionCourse>(),
			ModelDb.Card<Comet>(),
			ModelDb.Card<Conqueror>(),
			ModelDb.Card<Convergence>(),
			ModelDb.Card<CosmicIndifference>(),
			ModelDb.Card<CrashLanding>(),
			ModelDb.Card<CrescentSpear>(),
			ModelDb.Card<CrushUnder>(),
			ModelDb.Card<DecisionsDecisions>(),
			ModelDb.Card<DefendRegent>(),
			ModelDb.Card<Devastate>(),
			ModelDb.Card<DyingStar>(),
			ModelDb.Card<FallingStar>(),
			ModelDb.Card<ForegoneConclusion>(),
			ModelDb.Card<Furnace>(),
			ModelDb.Card<GammaBlast>(),
			ModelDb.Card<GatherLight>(),
			ModelDb.Card<Genesis>(),
			ModelDb.Card<Glimmer>(),
			ModelDb.Card<Glitterstream>(),
			ModelDb.Card<Glow>(),
			ModelDb.Card<Guards>(),
			ModelDb.Card<GuidingStar>(),
			ModelDb.Card<HammerTime>(),
			ModelDb.Card<HeavenlyDrill>(),
			ModelDb.Card<Hegemony>(),
			ModelDb.Card<HeirloomHammer>(),
			ModelDb.Card<HiddenCache>(),
			ModelDb.Card<IAmInvincible>(),
			ModelDb.Card<KinglyKick>(),
			ModelDb.Card<KinglyPunch>(),
			ModelDb.Card<KnockoutBlow>(),
			ModelDb.Card<KnowThyPlace>(),
			ModelDb.Card<Largesse>(),
			ModelDb.Card<LunarBlast>(),
			ModelDb.Card<MakeItSo>(),
			ModelDb.Card<ManifestAuthority>(),
			ModelDb.Card<MeteorShower>(),
			ModelDb.Card<MonarchsGaze>(),
			ModelDb.Card<Monologue>(),
			ModelDb.Card<NeutronAegis>(),
			ModelDb.Card<Orbit>(),
			ModelDb.Card<PaleBlueDot>(),
			ModelDb.Card<Parry>(),
			ModelDb.Card<ParticleWall>(),
			ModelDb.Card<Patter>(),
			ModelDb.Card<PhotonCut>(),
			ModelDb.Card<PillarOfCreation>(),
			ModelDb.Card<Prophesize>(),
			ModelDb.Card<Quasar>(),
			ModelDb.Card<Radiate>(),
			ModelDb.Card<RefineBlade>(),
			ModelDb.Card<Reflect>(),
			ModelDb.Card<Resonance>(),
			ModelDb.Card<RoyalGamble>(),
			ModelDb.Card<Royalties>(),
			ModelDb.Card<SeekingEdge>(),
			ModelDb.Card<SevenStars>(),
			ModelDb.Card<ShiningStrike>(),
			ModelDb.Card<SolarStrike>(),
			ModelDb.Card<SpectrumShift>(),
			ModelDb.Card<SpoilsOfBattle>(),
			ModelDb.Card<Stardust>(),
			ModelDb.Card<StrikeRegent>(),
			ModelDb.Card<SummonForth>(),
			ModelDb.Card<Supermassive>(),
			ModelDb.Card<SwordSage>(),
			ModelDb.Card<Terraforming>(),
			ModelDb.Card<TheSealedThrone>(),
			ModelDb.Card<TheSmith>(),
			ModelDb.Card<Tyranny>(),
			ModelDb.Card<Venerate>(),
			ModelDb.Card<VoidForm>(),
			ModelDb.Card<WroughtInWar>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Regent2Epoch>())
		{
			list.RemoveAll((CardModel c) => Regent2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Regent5Epoch>())
		{
			list.RemoveAll((CardModel c) => Regent5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Regent7Epoch>())
		{
			list.RemoveAll((CardModel c) => Regent7Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
