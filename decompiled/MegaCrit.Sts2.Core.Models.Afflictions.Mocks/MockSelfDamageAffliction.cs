using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Afflictions.Mocks;

public sealed class MockSelfDamageAffliction : AfflictionModel
{
	public override bool IsStackable => true;

	public override async Task OnPlay(PlayerChoiceContext choiceContext, Creature? target)
	{
		await CreatureCmd.Damage(choiceContext, base.Card.Owner.Creature, base.Amount, ValueProp.Unpowered | ValueProp.Move, null, null);
	}
}
