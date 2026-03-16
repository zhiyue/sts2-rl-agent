using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Jackpot : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(25m, ValueProp.Move),
		new CardsVar(3)
	});

	public Jackpot()
		: base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		IEnumerable<CardModel> forCombat = CardFactory.GetForCombat(base.Owner, base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint).Where(delegate(CardModel c)
		{
			CardEnergyCost energyCost = c.EnergyCost;
			return energyCost != null && energyCost.Canonical == 0 && !energyCost.CostsX;
		}), base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardGeneration);
		foreach (CardModel item in forCombat)
		{
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(item);
			}
			await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
