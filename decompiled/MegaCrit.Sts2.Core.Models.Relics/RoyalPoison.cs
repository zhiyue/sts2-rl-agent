using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RoyalPoison : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(4m, ValueProp.Unblockable | ValueProp.Unpowered));

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner && player.Creature.CombatState.RoundNumber <= 1)
		{
			Flash();
			await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.Damage, null, null);
		}
	}
}
