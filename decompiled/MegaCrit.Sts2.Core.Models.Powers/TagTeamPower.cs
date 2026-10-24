using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TagTeamPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public override int DisplayAmount => 1;

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Type != CardType.Attack)
		{
			return playCount;
		}
		if (card.Owner.Creature == base.Applier)
		{
			return playCount;
		}
		if (card.TargetType == TargetType.AnyEnemy && target != base.Owner)
		{
			return playCount;
		}
		TargetType targetType = card.TargetType;
		if ((uint)(targetType - 2) > 1u)
		{
			return playCount;
		}
		return playCount + base.Amount;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		await PowerCmd.Remove(this);
	}
}
