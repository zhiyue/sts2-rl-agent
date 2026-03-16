using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PaleBlueDotPower : PowerModel
{
	public const string cardPlayThresholdKey = "CardPlay";

	public const int cardPlayThresholdValue = 5;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CardPlay", 5m));

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner.Player)
		{
			return count;
		}
		int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry c) => c.RoundNumber == base.CombatState.RoundNumber - 1 && c.CardPlay.Card.Owner == base.Owner.Player);
		if (num < base.DynamicVars["CardPlay"].IntValue)
		{
			return count;
		}
		return count + (decimal)base.Amount;
	}

	public override Task AfterModifyingHandDraw()
	{
		Flash();
		return Task.CompletedTask;
	}
}
