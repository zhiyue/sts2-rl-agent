using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HighFive : CardModel
{
	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	protected override bool IsPlayable => !base.Owner.IsOstyMissing;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new OstyDamageVar(11m, ValueProp.Move),
		new PowerVar<VulnerablePower>(2m)
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<VulnerablePower>());

	public HighFive()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue).FromOsty(base.Owner.Osty, this).TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
				.Execute(choiceContext);
			await PowerCmd.Apply<VulnerablePower>(base.CombatState.HittableEnemies, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(2m);
		base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
	}
}
