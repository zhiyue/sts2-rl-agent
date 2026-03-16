using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class MonologuePower : PowerModel
{
	private class Data
	{
		public readonly Dictionary<CardModel, int> amountsForPlayedCards = new Dictionary<CardModel, int>();
	}

	public const string strengthAppliedKey = "StrengthApplied";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType
	{
		get
		{
			if (base.DynamicVars["StrengthApplied"].IntValue != 0)
			{
				return PowerStackType.Counter;
			}
			return PowerStackType.None;
		}
	}

	public override int DisplayAmount => base.DynamicVars["StrengthApplied"].IntValue;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<StrengthPower>(1m),
		new DynamicVar("StrengthApplied", 0m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature != base.Owner)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().amountsForPlayedCards.Add(cardPlay.Card, base.DynamicVars.Strength.IntValue);
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var value))
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner, value, base.Owner, null, silent: true);
			base.DynamicVars["StrengthApplied"].BaseValue += (decimal)base.DynamicVars.Strength.IntValue;
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Remove(this);
			await PowerCmd.Apply<StrengthPower>(base.Owner, -base.DynamicVars["StrengthApplied"].BaseValue, base.Owner, null, silent: true);
		}
	}
}
