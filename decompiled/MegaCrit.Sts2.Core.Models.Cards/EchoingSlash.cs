using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class EchoingSlash : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(10m, ValueProp.Move));

	public EchoingSlash()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await using AttackContext attackContext = await AttackCommand.CreateContextAsync(base.CombatState, this);
		int attackCount = 1;
		while (attackCount > 0)
		{
			attackCount--;
			IEnumerable<DamageResult> enumerable = await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature, this);
			attackContext.AddHit(enumerable);
			attackCount += enumerable.Count((DamageResult r) => r.WasTargetKilled);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
