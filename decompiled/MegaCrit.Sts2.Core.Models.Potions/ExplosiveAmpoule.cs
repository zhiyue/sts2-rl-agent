using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class ExplosiveAmpoule : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AllEnemies;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(10m, ValueProp.Unpowered));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		Creature player = base.Owner.Creature;
		DamageVar damage = base.DynamicVars.Damage;
		IReadOnlyList<Creature> targets = player.CombatState.HittableEnemies;
		foreach (Creature item in targets)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(item));
		}
		await Cmd.CustomScaledWait(0.2f, 0.3f);
		await CreatureCmd.Damage(choiceContext, targets, damage.BaseValue, damage.Props, player, null);
	}
}
