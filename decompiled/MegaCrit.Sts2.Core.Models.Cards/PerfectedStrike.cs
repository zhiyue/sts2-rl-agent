using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class PerfectedStrike : CardModel
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(6m),
		new ExtraDamageVar(2m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(CardTag.Strike)))
	});

	public PerfectedStrike()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		AttackCommand attackCommand = DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx(null, null, "heavy_attack.mp3")
			.WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t))
			.WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t));
		if (base.DynamicVars.CalculatedDamage.Calculate(cardPlay.Target) > 12m)
		{
			attackCommand.WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(base.Owner.Character), Ironclad.GetHeavyAttackDelayIfApplicable(base.Owner.Character));
		}
		await attackCommand.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
	}
}
