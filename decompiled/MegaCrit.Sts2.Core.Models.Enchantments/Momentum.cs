using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Momentum : EnchantmentModel
{
	private int _extraDamage;

	public override bool HasExtraCardText => true;

	public override bool ShowAmount => true;

	private int ExtraDamage
	{
		get
		{
			return _extraDamage;
		}
		set
		{
			AssertMutable();
			_extraDamage = value;
		}
	}

	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		ExtraDamage += base.Amount;
		return Task.CompletedTask;
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return ExtraDamage;
	}
}
