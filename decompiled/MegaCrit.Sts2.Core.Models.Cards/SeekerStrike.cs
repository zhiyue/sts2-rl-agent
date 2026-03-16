using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SeekerStrike : CardModel
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(6m, ValueProp.Move),
		new CardsVar(3)
	});

	public SeekerStrike()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		IEnumerable<CardModel> source = PileType.Draw.GetPile(base.Owner).Cards.ToList().StableShuffle(base.Owner.RunState.Rng.CombatCardSelection);
		CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, source.Take(base.DynamicVars.Cards.IntValue).ToList(), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			await CardPileCmd.Add(cardModel, PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
