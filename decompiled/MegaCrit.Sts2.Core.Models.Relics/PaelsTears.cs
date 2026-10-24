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
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsTears : RelicModel
{
	private bool _hadLeftoverEnergy;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	private bool HadLeftoverEnergy
	{
		get
		{
			return _hadLeftoverEnergy;
		}
		set
		{
			AssertMutable();
			_hadLeftoverEnergy = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(2));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return Task.CompletedTask;
		}
		HadLeftoverEnergy = base.Owner.PlayerCombatState.Energy > 0;
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner.Creature) && HadLeftoverEnergy)
		{
			Flash();
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		HadLeftoverEnergy = false;
		return Task.CompletedTask;
	}
}
