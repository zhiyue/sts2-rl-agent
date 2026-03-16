using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BansheesCry : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(33m, ValueProp.Move),
		new EnergyVar(2)
	});

	public BansheesCry()
		: base(6, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(6m);
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != this)
		{
			return Task.CompletedTask;
		}
		if (base.IsClone)
		{
			return Task.CompletedTask;
		}
		int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.WasEthereal && e.CardPlay.Card.Owner == base.Owner);
		base.EnergyCost.AddThisCombat(-num * base.DynamicVars.Energy.IntValue);
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal))
		{
			return Task.CompletedTask;
		}
		base.EnergyCost.AddThisCombat(-base.DynamicVars.Energy.IntValue);
		return Task.CompletedTask;
	}
}
