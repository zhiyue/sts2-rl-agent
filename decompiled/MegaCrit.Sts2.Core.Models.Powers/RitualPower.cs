using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class RitualPower : PowerModel
{
	private bool _wasJustAppliedByEnemy;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<StrengthPower>() };

	private bool WasJustAppliedByEnemy
	{
		get
		{
			return _wasJustAppliedByEnemy;
		}
		set
		{
			AssertMutable();
			_wasJustAppliedByEnemy = value;
		}
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (base.Owner.IsEnemy)
		{
			WasJustAppliedByEnemy = true;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			if (WasJustAppliedByEnemy)
			{
				WasJustAppliedByEnemy = false;
				return;
			}
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null);
		}
	}
}
