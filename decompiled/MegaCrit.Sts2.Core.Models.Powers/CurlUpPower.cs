using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CurlUpPower : PowerModel
{
	private class Data
	{
		public CardModel? playedCard;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!props.IsPoweredAttack())
		{
			return Task.CompletedTask;
		}
		if (cardSource == null)
		{
			return Task.CompletedTask;
		}
		if (GetInternalData<Data>().playedCard != null && cardSource != GetInternalData<Data>().playedCard)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().playedCard = cardSource;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card == GetInternalData<Data>().playedCard)
		{
			GetInternalData<Data>().playedCard = null;
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_curl");
			await CreatureCmd.TriggerAnim(base.Owner, "Curl", 0.25f);
			await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
			if (base.Owner.Monster is LouseProgenitor louseProgenitor)
			{
				louseProgenitor.Curled = true;
			}
			await PowerCmd.Remove(this);
		}
	}
}
