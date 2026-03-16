using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Goopy : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override bool CanEnchant(CardModel card)
	{
		if (base.CanEnchant(card))
		{
			return card.Tags.Contains(CardTag.Defend);
		}
		return false;
	}

	protected override void OnEnchant()
	{
		base.Card.AddKeyword(CardKeyword.Exhaust);
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card != base.Card)
		{
			return Task.CompletedTask;
		}
		base.Amount++;
		if (base.Card.DeckVersion != null)
		{
			base.Card.DeckVersion.Enchantment.Amount++;
		}
		return Task.CompletedTask;
	}

	public override decimal EnchantBlockAdditive(decimal originalBlock, ValueProp props)
	{
		if (!props.IsPoweredCardOrMonsterMoveBlock())
		{
			return 0m;
		}
		return base.Amount - 1;
	}
}
