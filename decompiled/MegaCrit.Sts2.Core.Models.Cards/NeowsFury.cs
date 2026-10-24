using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class NeowsFury : CardModel
{
	public override bool CanBeGeneratedInCombat => false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(10m, ValueProp.Move),
		new CardsVar(2)
	});

	public NeowsFury()
		: base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		int num = Math.Min(base.DynamicVars.Cards.IntValue, CardPile.MaxCardsInHand - PileType.Hand.GetPile(base.Owner).Cards.Count);
		if (num > 0)
		{
			await CardPileCmd.Add(await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 0, num)), PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
