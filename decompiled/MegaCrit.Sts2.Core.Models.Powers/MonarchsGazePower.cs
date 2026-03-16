using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class MonarchsGazePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult _, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (dealer == base.Owner && props.IsPoweredAttack())
		{
			await PowerCmd.Apply<MonarchsGazeStrengthDownPower>(target, base.Amount, base.Owner, null);
		}
	}
}
