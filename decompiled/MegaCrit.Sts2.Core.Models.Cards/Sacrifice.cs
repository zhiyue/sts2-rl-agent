using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Sacrifice : CardModel
{
	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Retain);

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner.IsOstyAlive ? (card.Owner.Osty.MaxHp * 2) : 0)
	});

	public Sacrifice()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			decimal blockGain = base.DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await CreatureCmd.Kill(base.Owner.Osty);
			await CreatureCmd.GainBlock(base.Owner.Creature, blockGain, base.DynamicVars.CalculatedBlock.Props, cardPlay);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
