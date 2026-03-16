using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Poke : CardModel
{
	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new OstyDamageVar(6m, ValueProp.Move));

	public Poke()
		: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue).FromOsty(base.Owner.Osty, this).Targeting(cardPlay.Target)
				.WithAttackerAnim("attack_poke", 0.3f)
				.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(3m);
	}
}
