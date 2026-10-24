using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class UnsettlingLamp : RelicModel
{
	private CardModel? _triggeringCard;

	private List<PowerModel>? _doubledPowers;

	private bool _isFinishedTriggering;

	public override RelicRarity Rarity => RelicRarity.Rare;

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

	private List<PowerModel> DoubledPowers
	{
		get
		{
			AssertMutable();
			if (_doubledPowers == null)
			{
				_doubledPowers = new List<PowerModel>();
			}
			return _doubledPowers;
		}
	}

	private bool IsFinishedTriggering
	{
		get
		{
			return _isFinishedTriggering;
		}
		set
		{
			AssertMutable();
			_isFinishedTriggering = value;
		}
	}

	public override Task BeforeCombatStart()
	{
		TriggeringCard = null;
		DoubledPowers.Clear();
		IsFinishedTriggering = false;
		base.Status = RelicStatus.Active;
		return Task.CompletedTask;
	}

	public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
	{
		if (TriggeringCard != null)
		{
			return Task.CompletedTask;
		}
		if (IsFinishedTriggering)
		{
			return Task.CompletedTask;
		}
		if (cardSource == null)
		{
			return Task.CompletedTask;
		}
		if (applier != base.Owner.Creature)
		{
			return Task.CompletedTask;
		}
		if (target.Side == base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (!power.IsVisible)
		{
			return Task.CompletedTask;
		}
		if (power.GetTypeForAmount(amount) != PowerType.Debuff)
		{
			return Task.CompletedTask;
		}
		TriggeringCard = cardSource;
		DoubledPowers.Add(power);
		return Task.CompletedTask;
	}

	public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		if (TriggeringCard == null)
		{
			return 1m;
		}
		if (cardSource != TriggeringCard)
		{
			return 1m;
		}
		if (IsFinishedTriggering)
		{
			return 1m;
		}
		if (HasDoubledTemporaryPowerSource(power))
		{
			return 1m;
		}
		if (power.GetTypeForAmount(amount) != PowerType.Debuff)
		{
			return 1m;
		}
		return 2m;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card != TriggeringCard)
		{
			return Task.CompletedTask;
		}
		if (IsFinishedTriggering)
		{
			return Task.CompletedTask;
		}
		Flash();
		IsFinishedTriggering = true;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		TriggeringCard = null;
		DoubledPowers.Clear();
		IsFinishedTriggering = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	/// <summary>
	/// If we've doubled an <see cref="T:MegaCrit.Sts2.Core.Models.ITemporaryPower" /> and now we're trying to double its internal debuff,
	/// skip it so we don't double-dip.
	/// </summary>
	/// <example>
	/// If we've doubled <see cref="T:MegaCrit.Sts2.Core.Models.Powers.PiercingWailPower" />, we don't also want to double negative <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DexterityPower" />,
	/// because that'll end up quadrupling it.
	/// </example>
	private bool HasDoubledTemporaryPowerSource(PowerModel power)
	{
		return DoubledPowers.OfType<ITemporaryPower>().Any((ITemporaryPower p) => p.InternallyAppliedPower.GetType() == power.GetType());
	}
}
