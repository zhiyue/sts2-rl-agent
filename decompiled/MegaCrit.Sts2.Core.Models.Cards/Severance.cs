using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Severance : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(13m, ValueProp.Move));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Soul>());

	public Severance()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		List<Soul> souls = Soul.Create(base.Owner, 3, base.CombatState).ToList();
		CardPileAddResult drawResult = await CardPileCmd.AddGeneratedCardToCombat(souls[0], PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
		CardPileAddResult discardResult = await CardPileCmd.AddGeneratedCardToCombat(souls[1], PileType.Discard, addedByPlayer: true);
		await CardPileCmd.AddGeneratedCardToCombat(souls[2], PileType.Hand, addedByPlayer: true);
		CardCmd.PreviewCardPileAdd(new global::_003C_003Ez__ReadOnlyArray<CardPileAddResult>(new CardPileAddResult[2] { drawResult, discardResult }));
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
