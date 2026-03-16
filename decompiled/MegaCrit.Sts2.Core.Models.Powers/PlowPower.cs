using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PlowPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Stun),
		HoverTipFactory.FromPower<StrengthPower>()
	});

	public override bool ShouldScaleInMultiplayer => true;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage > 0 && target.CurrentHp <= base.Amount)
		{
			Flash();
			CeremonialBeast monster = (CeremonialBeast)base.Owner.Monster;
			await PowerCmd.Remove<StrengthPower>(base.Owner);
			await monster.SetStunned();
			await CreatureCmd.Stun(base.Owner, monster.StunnedMove, monster.BeastCryState.StateId);
			await PowerCmd.Remove(this);
		}
	}
}
