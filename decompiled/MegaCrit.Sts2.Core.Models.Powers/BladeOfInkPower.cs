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

public sealed class BladeOfInkPower : PowerModel
{
	private const string _strengthAppliedKey = "StrengthApplied";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("StrengthApplied", 0m));

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Attack)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null, silent: true);
			base.DynamicVars["StrengthApplied"].BaseValue += (decimal)base.Amount;
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Apply<StrengthPower>(base.Owner, -base.DynamicVars["StrengthApplied"].BaseValue, base.Owner, null, silent: true);
			await PowerCmd.Remove(this);
		}
	}
}
