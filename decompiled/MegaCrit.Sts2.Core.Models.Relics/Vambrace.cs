using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Vambrace : RelicModel
{
	private CardModel? _triggeringCard;

	private bool _blockGainedThisCombat;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	private CardModel? TriggeringCard
	{
		get
		{
			return _triggeringCard;
		}
		set
		{
			AssertMutable();
			_triggeringCard = value;
		}
	}

	private bool BlockGainedThisCombat
	{
		get
		{
			return _blockGainedThisCombat;
		}
		set
		{
			AssertMutable();
			_blockGainedThisCombat = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override Task BeforeCombatStart()
	{
		TriggeringCard = null;
		BlockGainedThisCombat = false;
		base.Status = RelicStatus.Active;
		return Task.CompletedTask;
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!props.IsCardOrMonsterMove())
		{
			return 1m;
		}
		if (cardSource == null)
		{
			return 1m;
		}
		if (TriggeringCard != null && TriggeringCard != cardSource)
		{
			return 1m;
		}
		if (target != base.Owner.Creature)
		{
			return 1m;
		}
		if (BlockGainedThisCombat)
		{
			return 1m;
		}
		return 2m;
	}

	public override Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (modifiedAmount <= 0m)
		{
			return Task.CompletedTask;
		}
		if (cardSource == null)
		{
			return Task.CompletedTask;
		}
		Flash();
		base.Status = RelicStatus.Normal;
		TriggeringCard = cardSource;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card != TriggeringCard)
		{
			return Task.CompletedTask;
		}
		if (BlockGainedThisCombat)
		{
			return Task.CompletedTask;
		}
		BlockGainedThisCombat = true;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		TriggeringCard = null;
		BlockGainedThisCombat = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
