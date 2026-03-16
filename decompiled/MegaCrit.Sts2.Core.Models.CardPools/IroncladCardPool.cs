using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class IroncladCardPool : CardPoolModel
{
	public override string Title => "ironclad";

	public override string EnergyColorName => "ironclad";

	public override string CardFrameMaterialPath => "card_frame_red";

	public override Color DeckEntryCardColor => new Color("D62000");

	public override Color EnergyOutlineColor => new Color("802020");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[87]
		{
			ModelDb.Card<Aggression>(),
			ModelDb.Card<Anger>(),
			ModelDb.Card<Armaments>(),
			ModelDb.Card<AshenStrike>(),
			ModelDb.Card<Barricade>(),
			ModelDb.Card<Bash>(),
			ModelDb.Card<BattleTrance>(),
			ModelDb.Card<BloodWall>(),
			ModelDb.Card<Bloodletting>(),
			ModelDb.Card<Bludgeon>(),
			ModelDb.Card<BodySlam>(),
			ModelDb.Card<Brand>(),
			ModelDb.Card<Break>(),
			ModelDb.Card<Breakthrough>(),
			ModelDb.Card<Bully>(),
			ModelDb.Card<BurningPact>(),
			ModelDb.Card<Cascade>(),
			ModelDb.Card<Cinder>(),
			ModelDb.Card<Colossus>(),
			ModelDb.Card<Conflagration>(),
			ModelDb.Card<Corruption>(),
			ModelDb.Card<CrimsonMantle>(),
			ModelDb.Card<Cruelty>(),
			ModelDb.Card<DarkEmbrace>(),
			ModelDb.Card<DefendIronclad>(),
			ModelDb.Card<DemonForm>(),
			ModelDb.Card<DemonicShield>(),
			ModelDb.Card<Dismantle>(),
			ModelDb.Card<Dominate>(),
			ModelDb.Card<DrumOfBattle>(),
			ModelDb.Card<EvilEye>(),
			ModelDb.Card<ExpectAFight>(),
			ModelDb.Card<Feed>(),
			ModelDb.Card<FeelNoPain>(),
			ModelDb.Card<FiendFire>(),
			ModelDb.Card<FightMe>(),
			ModelDb.Card<FlameBarrier>(),
			ModelDb.Card<ForgottenRitual>(),
			ModelDb.Card<Grapple>(),
			ModelDb.Card<Havoc>(),
			ModelDb.Card<Headbutt>(),
			ModelDb.Card<Hellraiser>(),
			ModelDb.Card<Hemokinesis>(),
			ModelDb.Card<HowlFromBeyond>(),
			ModelDb.Card<Impervious>(),
			ModelDb.Card<InfernalBlade>(),
			ModelDb.Card<Inferno>(),
			ModelDb.Card<Inflame>(),
			ModelDb.Card<IronWave>(),
			ModelDb.Card<Juggernaut>(),
			ModelDb.Card<Juggling>(),
			ModelDb.Card<Mangle>(),
			ModelDb.Card<MoltenFist>(),
			ModelDb.Card<Offering>(),
			ModelDb.Card<OneTwoPunch>(),
			ModelDb.Card<PactsEnd>(),
			ModelDb.Card<PerfectedStrike>(),
			ModelDb.Card<Pillage>(),
			ModelDb.Card<PommelStrike>(),
			ModelDb.Card<PrimalForce>(),
			ModelDb.Card<Pyre>(),
			ModelDb.Card<Rage>(),
			ModelDb.Card<Rampage>(),
			ModelDb.Card<Rupture>(),
			ModelDb.Card<SecondWind>(),
			ModelDb.Card<SetupStrike>(),
			ModelDb.Card<ShrugItOff>(),
			ModelDb.Card<Spite>(),
			ModelDb.Card<Stampede>(),
			ModelDb.Card<Stoke>(),
			ModelDb.Card<Stomp>(),
			ModelDb.Card<StoneArmor>(),
			ModelDb.Card<StrikeIronclad>(),
			ModelDb.Card<SwordBoomerang>(),
			ModelDb.Card<Tank>(),
			ModelDb.Card<Taunt>(),
			ModelDb.Card<TearAsunder>(),
			ModelDb.Card<Thrash>(),
			ModelDb.Card<Thunderclap>(),
			ModelDb.Card<Tremble>(),
			ModelDb.Card<TrueGrit>(),
			ModelDb.Card<TwinStrike>(),
			ModelDb.Card<Unmovable>(),
			ModelDb.Card<Unrelenting>(),
			ModelDb.Card<Uppercut>(),
			ModelDb.Card<Vicious>(),
			ModelDb.Card<Whirlwind>()
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();
		if (!unlockState.IsEpochRevealed<Ironclad2Epoch>())
		{
			list.RemoveAll((CardModel c) => Ironclad2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Ironclad5Epoch>())
		{
			list.RemoveAll((CardModel c) => Ironclad5Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		if (!unlockState.IsEpochRevealed<Ironclad7Epoch>())
		{
			list.RemoveAll((CardModel c) => Ironclad7Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		return list;
	}
}
