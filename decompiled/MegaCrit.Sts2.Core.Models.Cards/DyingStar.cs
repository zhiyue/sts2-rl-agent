using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DyingStar : CardModel
{
	private const string _strengthLossKey = "StrengthLoss";

	public override int CanonicalStarCost => 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(9m, ValueProp.Move),
		new DynamicVar("StrengthLoss", 9m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Ethereal);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public DyingStar()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
		IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_starry_impact")
			.SpawningHitVfxOnEachCreature()
			.Execute(choiceContext);
		foreach (Creature enemy in enemies)
		{
			await PowerCmd.Apply<DyingStarPower>(enemy, base.DynamicVars["StrengthLoss"].BaseValue, base.Owner.Creature, this);
			VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["StrengthLoss"].UpgradeValueBy(2m);
	}
}
