using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Cinder : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(18m, ValueProp.Move));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public Cinder()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(base.Owner.Character), Ironclad.GetHeavyAttackDelayIfApplicable(base.Owner.Character))
			.Targeting(cardPlay.Target)
			.WithHitVfxNode((Creature t) => NFireBurstVfx.Create(t, 0.75f))
			.Execute(choiceContext);
		CardPile pile = PileType.Hand.GetPile(base.Owner);
		CardModel cardModel = base.Owner.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
		if (cardModel != null)
		{
			await CardCmd.Exhaust(choiceContext, cardModel);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(6m);
	}
}
