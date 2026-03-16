using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PenNib : RelicModel
{
	private const int _attacksThreshold = 10;

	private bool _isActivating;

	private int _attacksPlayed;

	private CardModel? _attackToDouble;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return AttacksPlayed % 10;
			}
			return 10;
		}
	}

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			UpdateDisplay();
		}
	}

	[SavedProperty]
	public int AttacksPlayed
	{
		get
		{
			return _attacksPlayed;
		}
		private set
		{
			AssertMutable();
			_attacksPlayed = value % 10;
			UpdateDisplay();
		}
	}

	private CardModel? AttackToDouble
	{
		get
		{
			return _attackToDouble;
		}
		set
		{
			AssertMutable();
			_attackToDouble = value;
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			base.Status = RelicStatus.Normal;
		}
		else
		{
			base.Status = ((AttacksPlayed == 9) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public void NotifyAttackPlayed()
	{
		AttacksPlayed++;
		if (AttacksPlayed == 0)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (cardSource == null)
		{
			return 1m;
		}
		if (dealer != base.Owner.Creature && dealer != base.Owner.Osty)
		{
			return 1m;
		}
		if (AttackToDouble == null)
		{
			CardPile? pile = cardSource.Pile;
			if ((pile == null || pile.Type != PileType.Play) && AttacksPlayed == 9)
			{
				return 2m;
			}
			return 1m;
		}
		if (cardSource == AttackToDouble)
		{
			return 2m;
		}
		return 1m;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Type != CardType.Attack)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		NotifyAttackPlayed();
		if (AttacksPlayed == 0)
		{
			AttackToDouble = cardPlay.Card;
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (AttackToDouble == null)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card != AttackToDouble)
		{
			return Task.CompletedTask;
		}
		AttackToDouble = null;
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
