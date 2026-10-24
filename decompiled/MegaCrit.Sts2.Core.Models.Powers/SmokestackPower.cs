using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SmokestackPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (card.Type == CardType.Status && creator != null && creator.Creature == base.Owner)
		{
			Flash();
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}
}
