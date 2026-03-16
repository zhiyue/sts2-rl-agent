using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Corrupted : EnchantmentModel
{
	private const decimal _damageAmount = 2m;

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return 1.5m;
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		await CreatureCmd.Damage(choiceContext, base.Card.Owner.Creature, 2m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, base.Card);
	}
}
