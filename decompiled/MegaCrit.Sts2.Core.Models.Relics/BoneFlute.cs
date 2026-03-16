using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BoneFlute : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(2m, ValueProp.Unpowered));

	public override Task AfterAttack(AttackCommand command)
	{
		if (!(command.Attacker?.Monster is Osty))
		{
			return Task.CompletedTask;
		}
		if (command.Attacker.PetOwner?.Creature != base.Owner.Creature)
		{
			return Task.CompletedTask;
		}
		Flash();
		return CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null);
	}
}
