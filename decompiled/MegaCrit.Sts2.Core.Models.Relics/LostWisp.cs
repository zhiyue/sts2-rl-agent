using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LostWisp : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(8m, ValueProp.Unpowered));

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && CombatManager.Instance.IsInProgress && cardPlay.Card.Type == CardType.Power)
		{
			Flash();
			await CreatureCmd.Damage(context, base.Owner.Creature.CombatState.HittableEnemies, base.DynamicVars.Damage.BaseValue, base.DynamicVars.Damage.Props, base.Owner.Creature, null);
		}
	}
}
