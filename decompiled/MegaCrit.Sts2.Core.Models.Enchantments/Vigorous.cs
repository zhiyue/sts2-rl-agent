using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Vigorous : EnchantmentModel
{
	public override bool ShowAmount => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		if (base.Status != EnchantmentStatus.Normal)
		{
			return 0m;
		}
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return base.Amount;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card != base.Card)
		{
			return Task.CompletedTask;
		}
		base.Status = EnchantmentStatus.Disabled;
		return Task.CompletedTask;
	}
}
