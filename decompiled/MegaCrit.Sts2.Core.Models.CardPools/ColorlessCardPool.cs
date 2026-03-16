using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class ColorlessCardPool : CardPoolModel
{
	public const string energyColorName = "colorless";

	public override string Title => "colorless";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => new Color("A3A3A3FF");

	public override bool IsColorless => true;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[64]
		{
			ModelDb.Card<Alchemize>(),
			ModelDb.Card<Anointed>(),
			ModelDb.Card<Automation>(),
			ModelDb.Card<BeaconOfHope>(),
			ModelDb.Card<BeatDown>(),
			ModelDb.Card<BelieveInYou>(),
			ModelDb.Card<Bolas>(),
			ModelDb.Card<Calamity>(),
			ModelDb.Card<Catastrophe>(),
			ModelDb.Card<Coordinate>(),
			ModelDb.Card<DarkShackles>(),
			ModelDb.Card<Discovery>(),
			ModelDb.Card<DramaticEntrance>(),
			ModelDb.Card<Entropy>(),
			ModelDb.Card<Equilibrium>(),
			ModelDb.Card<EternalArmor>(),
			ModelDb.Card<Fasten>(),
			ModelDb.Card<Finesse>(),
			ModelDb.Card<Fisticuffs>(),
			ModelDb.Card<FlashOfSteel>(),
			ModelDb.Card<GangUp>(),
			ModelDb.Card<GoldAxe>(),
			ModelDb.Card<HandOfGreed>(),
			ModelDb.Card<HiddenGem>(),
			ModelDb.Card<HuddleUp>(),
			ModelDb.Card<Impatience>(),
			ModelDb.Card<Intercept>(),
			ModelDb.Card<JackOfAllTrades>(),
			ModelDb.Card<Jackpot>(),
			ModelDb.Card<Knockdown>(),
			ModelDb.Card<Lift>(),
			ModelDb.Card<MasterOfStrategy>(),
			ModelDb.Card<Mayhem>(),
			ModelDb.Card<Mimic>(),
			ModelDb.Card<MindBlast>(),
			ModelDb.Card<Nostalgia>(),
			ModelDb.Card<Omnislice>(),
			ModelDb.Card<Panache>(),
			ModelDb.Card<PanicButton>(),
			ModelDb.Card<PrepTime>(),
			ModelDb.Card<Production>(),
			ModelDb.Card<Prolong>(),
			ModelDb.Card<Prowess>(),
			ModelDb.Card<Purity>(),
			ModelDb.Card<Rally>(),
			ModelDb.Card<Rend>(),
			ModelDb.Card<Restlessness>(),
			ModelDb.Card<RollingBoulder>(),
			ModelDb.Card<Salvo>(),
			ModelDb.Card<Scrawl>(),
			ModelDb.Card<SecretTechnique>(),
			ModelDb.Card<SecretWeapon>(),
			ModelDb.Card<SeekerStrike>(),
			ModelDb.Card<Shockwave>(),
			ModelDb.Card<Splash>(),
			ModelDb.Card<Stratagem>(),
			ModelDb.Card<TagTeam>(),
			ModelDb.Card<TheBomb>(),
			ModelDb.Card<TheGambit>(),
			ModelDb.Card<ThinkingAhead>(),
			ModelDb.Card<ThrummingHatchet>(),
			ModelDb.Card<UltimateDefend>(),
			ModelDb.Card<UltimateStrike>(),
			ModelDb.Card<Volley>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Colorless1Epoch>())
		{
			list.RemoveAll((CardModel c) => Colorless1Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Colorless2Epoch>())
		{
			list.RemoveAll((CardModel c) => Colorless2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Colorless3Epoch>())
		{
			list.RemoveAll((CardModel c) => Colorless3Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Colorless4Epoch>())
		{
			list.RemoveAll((CardModel c) => Colorless4Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Colorless5Epoch>())
		{
			list.RemoveAll((CardModel c) => Colorless5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
