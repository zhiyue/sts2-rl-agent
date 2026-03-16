using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DemonTongue : RelicModel
{
	private bool _triggeredThisTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (base.Owner.Creature.CombatState != null && base.Owner.Creature.CombatState.CurrentSide == base.Owner.Creature.Side && target == base.Owner.Creature && result.UnblockedDamage > 0 && !_triggeredThisTurn)
		{
			_triggeredThisTurn = true;
			Flash();
			await CreatureCmd.Heal(base.Owner.Creature, result.UnblockedDamage);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		_triggeredThisTurn = false;
		return Task.CompletedTask;
	}
}
