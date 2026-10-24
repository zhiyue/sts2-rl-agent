using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockGainBlockOnAttackPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	/// <summary>
	/// This gives the attacker 1 block after every attack.
	/// A multi-hit attack should still only gain 1 block.
	/// </summary>
	public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		if (command.Attacker == base.Owner && command.DamageProps.HasFlag(ValueProp.Move))
		{
			await CreatureCmd.GainBlock(base.Owner, 1m, ValueProp.Unpowered, null);
		}
	}
}
