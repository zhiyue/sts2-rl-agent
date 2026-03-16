using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class NecrobinderCardPool : CardPoolModel
{
	public override string Title => "necrobinder";

	public override string EnergyColorName => "necrobinder";

	public override string CardFrameMaterialPath => "card_frame_pink";

	public override Color DeckEntryCardColor => new Color("CD4EED");

	public override Color EnergyOutlineColor => new Color("803367");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[88]
		{
			ModelDb.Card<Afterlife>(),
			ModelDb.Card<BansheesCry>(),
			ModelDb.Card<BlightStrike>(),
			ModelDb.Card<Bodyguard>(),
			ModelDb.Card<BoneShards>(),
			ModelDb.Card<BorrowedTime>(),
			ModelDb.Card<Bury>(),
			ModelDb.Card<Calcify>(),
			ModelDb.Card<CallOfTheVoid>(),
			ModelDb.Card<CaptureSpirit>(),
			ModelDb.Card<Cleanse>(),
			ModelDb.Card<Countdown>(),
			ModelDb.Card<DanseMacabre>(),
			ModelDb.Card<DeathMarch>(),
			ModelDb.Card<Deathbringer>(),
			ModelDb.Card<DeathsDoor>(),
			ModelDb.Card<Debilitate>(),
			ModelDb.Card<DefendNecrobinder>(),
			ModelDb.Card<Defile>(),
			ModelDb.Card<Defy>(),
			ModelDb.Card<Delay>(),
			ModelDb.Card<Demesne>(),
			ModelDb.Card<DevourLife>(),
			ModelDb.Card<Dirge>(),
			ModelDb.Card<DrainPower>(),
			ModelDb.Card<Dredge>(),
			ModelDb.Card<Eidolon>(),
			ModelDb.Card<EndOfDays>(),
			ModelDb.Card<EnfeeblingTouch>(),
			ModelDb.Card<Eradicate>(),
			ModelDb.Card<Fear>(),
			ModelDb.Card<Fetch>(),
			ModelDb.Card<Flatten>(),
			ModelDb.Card<ForbiddenGrimoire>(),
			ModelDb.Card<Friendship>(),
			ModelDb.Card<GlimpseBeyond>(),
			ModelDb.Card<GraveWarden>(),
			ModelDb.Card<Graveblast>(),
			ModelDb.Card<Hang>(),
			ModelDb.Card<Haunt>(),
			ModelDb.Card<HighFive>(),
			ModelDb.Card<Invoke>(),
			ModelDb.Card<LegionOfBone>(),
			ModelDb.Card<Lethality>(),
			ModelDb.Card<Melancholy>(),
			ModelDb.Card<Misery>(),
			ModelDb.Card<NecroMastery>(),
			ModelDb.Card<NegativePulse>(),
			ModelDb.Card<Neurosurge>(),
			ModelDb.Card<NoEscape>(),
			ModelDb.Card<Oblivion>(),
			ModelDb.Card<Pagestorm>(),
			ModelDb.Card<Parse>(),
			ModelDb.Card<Poke>(),
			ModelDb.Card<Protector>(),
			ModelDb.Card<PullAggro>(),
			ModelDb.Card<PullFromBelow>(),
			ModelDb.Card<Putrefy>(),
			ModelDb.Card<Rattle>(),
			ModelDb.Card<Reanimate>(),
			ModelDb.Card<Reap>(),
			ModelDb.Card<ReaperForm>(),
			ModelDb.Card<Reave>(),
			ModelDb.Card<RightHandHand>(),
			ModelDb.Card<Sacrifice>(),
			ModelDb.Card<Scourge>(),
			ModelDb.Card<SculptingStrike>(),
			ModelDb.Card<Seance>(),
			ModelDb.Card<SentryMode>(),
			ModelDb.Card<Severance>(),
			ModelDb.Card<SharedFate>(),
			ModelDb.Card<Shroud>(),
			ModelDb.Card<SicEm>(),
			ModelDb.Card<SleightOfFlesh>(),
			ModelDb.Card<Snap>(),
			ModelDb.Card<SoulStorm>(),
			ModelDb.Card<Sow>(),
			ModelDb.Card<SpiritOfAsh>(),
			ModelDb.Card<Spur>(),
			ModelDb.Card<Squeeze>(),
			ModelDb.Card<StrikeNecrobinder>(),
			ModelDb.Card<TheScythe>(),
			ModelDb.Card<TimesUp>(),
			ModelDb.Card<Transfigure>(),
			ModelDb.Card<Undeath>(),
			ModelDb.Card<Unleash>(),
			ModelDb.Card<Veilpiercer>(),
			ModelDb.Card<Wisp>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Necrobinder2Epoch>())
		{
			list.RemoveAll((CardModel c) => Necrobinder2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Necrobinder5Epoch>())
		{
			list.RemoveAll((CardModel c) => Necrobinder5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Necrobinder7Epoch>())
		{
			list.RemoveAll((CardModel c) => Necrobinder7Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
