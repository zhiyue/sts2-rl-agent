using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Orichalcum : RelicModel
{
	private bool _shouldTrigger;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(6m, ValueProp.Unpowered));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	private bool ShouldTrigger
	{
		get
		{
			return _shouldTrigger;
		}
		set
		{
			AssertMutable();
			_shouldTrigger = value;
		}
	}

	public override Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.Creature.Block > 0)
		{
			return Task.CompletedTask;
		}
		ShouldTrigger = true;
		return Task.CompletedTask;
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (ShouldTrigger)
		{
			ShouldTrigger = false;
			Flash();
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		ShouldTrigger = false;
		return Task.CompletedTask;
	}
}
