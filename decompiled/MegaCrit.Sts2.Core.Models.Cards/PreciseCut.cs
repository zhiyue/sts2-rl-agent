using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class PreciseCut : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(13m),
		new ExtraDamageVar(2m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier(delegate(CardModel card, Creature? _)
		{
			int num = PileType.Hand.GetPile(card.Owner).Cards.Count;
			CardPile? pile = card.Pile;
			if (pile != null && pile.Type == PileType.Hand)
			{
				num--;
			}
			return -num;
		})
	});

	public PreciseCut()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", null, "dagger_throw.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationBase.UpgradeValueBy(3m);
	}
}
