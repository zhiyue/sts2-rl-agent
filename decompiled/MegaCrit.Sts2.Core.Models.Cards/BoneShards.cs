using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BoneShards : CardModel
{
	protected override bool ShouldGlowRedInternal => base.Owner.IsOstyMissing;

	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new OstyDamageVar(9m, ValueProp.Move),
		new BlockVar(9m, ValueProp.Move)
	});

	public BoneShards()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!Osty.CheckMissingWithAnim(base.Owner))
		{
			await DamageCmd.Attack(base.DynamicVars.OstyDamage.BaseValue).FromOsty(base.Owner.Osty, this).TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
				.Execute(choiceContext);
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
			if (base.Owner.IsOstyAlive)
			{
				await CreatureCmd.Kill(base.Owner.Osty);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.OstyDamage.UpgradeValueBy(3m);
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
