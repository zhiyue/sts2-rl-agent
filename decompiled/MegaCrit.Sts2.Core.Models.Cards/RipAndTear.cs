using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class RipAndTear : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(7m, ValueProp.Move));

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<DefectCardPool>();

	public RipAndTear()
		: base(1, CardType.Attack, CardRarity.Event, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this)
			.TargetingRandomOpponents(base.CombatState)
			.WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
