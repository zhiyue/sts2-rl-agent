using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class AllForOne : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(10m, ValueProp.Move));

	public AllForOne()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(choiceContext);
		IEnumerable<CardModel> enumerable = PileType.Discard.GetPile(base.Owner).Cards.Where(Filter).ToList();
		foreach (CardModel item in enumerable)
		{
			await CardPileCmd.Add(item, PileType.Hand);
		}
	}

	private bool Filter(CardModel card)
	{
		bool flag = card.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && !card.EnergyCost.CostsX;
		bool flag2 = flag;
		if (flag2)
		{
			CardType type = card.Type;
			bool flag3 = (uint)(type - 1) <= 2u;
			flag2 = flag3;
		}
		return flag2;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
