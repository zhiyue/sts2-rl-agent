using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FakeOrichalcum : RelicModel
{
	private bool _shouldTrigger;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override int MerchantCost => 50;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(3m, ValueProp.Unpowered));

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

	/// <summary>
	/// This uses the _very early_ hook because it needs to check the player's block before <see cref="T:MegaCrit.Sts2.Core.Models.Powers.PlatingPower" />
	/// triggers (otherwise, <see cref="T:MegaCrit.Sts2.Core.Models.Powers.PlatingPower" /> will prevent this relic's block gain), and
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Powers.PlatingPower" /> in turn needs to run early so it can give you block before you take damage from
	/// another end-of-turn effect.
	/// </summary>
	public override Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner.Creature))
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

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (ShouldTrigger)
		{
			ShouldTrigger = false;
			Flash();
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		ShouldTrigger = false;
		return Task.CompletedTask;
	}
}
