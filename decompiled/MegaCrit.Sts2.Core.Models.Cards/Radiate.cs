using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Radiate : CardModel
{
	private const string _calculatedHitsKey = "CalculatedHits";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[5]
	{
		new DamageVar(3m, ValueProp.Move),
		new StarsVar(1),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => (from e in CombatManager.Instance.History.Entries.OfType<StarsModifiedEntry>()
			where e.HappenedThisTurn(card.CombatState) && e.Amount > 0 && e.Actor == card.Owner.Creature
			select e).Sum((StarsModifiedEntry e) => e.Amount))
	});

	public Radiate()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount((int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target)).FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
			.SpawningHitVfxOnEachCreature()
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
	}
}
