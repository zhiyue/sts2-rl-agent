using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MiniRegent : RelicModel
{
	private bool _usedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	private bool UsedThisTurn
	{
		get
		{
			return _usedThisTurn;
		}
		set
		{
			AssertMutable();
			_usedThisTurn = value;
		}
	}

	public override async Task AfterStarsSpent(int amount, Player spender)
	{
		if (spender == base.Owner && !UsedThisTurn)
		{
			UsedThisTurn = true;
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, null);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		UsedThisTurn = false;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		UsedThisTurn = false;
		return Task.CompletedTask;
	}
}
