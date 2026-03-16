using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public class SweepingGaze : CardModel
{
	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Ethereal,
		CardKeyword.Exhaust
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new OstyDamageVar(10m, ValueProp.Move));

	public SweepingGaze()
		: base(0, CardType.Attack, CardRarity.Token, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue).FromOsty(base.Owner.Osty, this).TargetingRandomOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(5m);
	}
}
