using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Voltaic : CardModel
{
	private const string _calculatedChannelsKey = "CalculatedChannels";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedChannels").WithMultiplier((CardModel card, Creature? _) => CombatManager.Instance.History.Entries.OfType<OrbChanneledEntry>().Count((OrbChanneledEntry e) => e.Actor.Player == card.Owner && e.Orb is LightningOrb))
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Voltaic()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int lightningChanneledCount = (int)((CalculatedVar)base.DynamicVars["CalculatedChannels"]).Calculate(cardPlay.Target);
		for (int i = 0; i < lightningChanneledCount; i++)
		{
			await OrbCmd.Channel<LightningOrb>(choiceContext, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
