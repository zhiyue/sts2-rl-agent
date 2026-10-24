using System.Collections.Generic;
using System.Linq;
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
		if (target != base.Owner || result.UnblockedDamage <= 0 || target.CurrentHp > base.Amount)
		{
			return;
		}
		Flash();
		List<TemporaryStrengthPower> list = base.Owner.GetPowerInstances<TemporaryStrengthPower>().ToList();
		foreach (TemporaryStrengthPower item in list)
		{
			await PowerCmd.Remove(item);
		}
		await PowerCmd.Remove<StrengthPower>(base.Owner);
		MonsterModel monster = base.Owner.Monster;
		if (monster is CeremonialBeast ceremonialBeast)
		{
			await ceremonialBeast.SetStunned();
			await CreatureCmd.Stun(base.Owner, ceremonialBeast.StunnedMove, ceremonialBeast.BeastCryState.StateId);
		}
		else
		{
			await CreatureCmd.Stun(base.Owner);
		}
		await PowerCmd.Remove(this);
	}
}
