using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class MonologuePower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the cards we've seen played and the power amount at the time they were played.
		/// This lets Monologue avoid triggering on cards that started play before it was applied, and avoid gaining
		/// extra block on multiple plays of Monologue.
		/// </summary>
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

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

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

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var value))
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, value, base.Owner, null, silent: true);
			base.DynamicVars["StrengthApplied"].BaseValue += (decimal)base.DynamicVars.Strength.IntValue;
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			await PowerCmd.Remove(this);
			await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -base.DynamicVars["StrengthApplied"].BaseValue, base.Owner, null, silent: true);
		}
	}
}
