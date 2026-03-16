using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HelixDrill : CardModel
{
	private const string _calculatedHitsKey = "CalculatedHits";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new DamageVar(3m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedHits").WithMultiplier(delegate(CardModel card, Creature? _)
		{
			if (card.Pile == null)
			{
				return 0m;
			}
			int num = (from e in CombatManager.Instance.History.Entries.OfType<EnergySpentEntry>()
				where e.HappenedThisTurn(card.CombatState) && e.Actor.Player == card.Owner
				select e).Sum((EnergySpentEntry c) => c.Amount);
			if (card.Pile.Type == PileType.Play)
			{
				num -= card.EnergyCost.GetWithModifiers(CostModifiers.All);
			}
			return num;
		})
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	public HelixDrill()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount((int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target)).FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
