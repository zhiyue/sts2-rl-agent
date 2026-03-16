using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Uproar : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(5m, ValueProp.Move));

	public Uproar()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(2)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		CardModel cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
			.FirstOrDefault();
		if (cardModel == null)
		{
			cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
				.FirstOrDefault();
		}
		if (cardModel != null)
		{
			await CardCmd.AutoPlay(choiceContext, cardModel, null);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
