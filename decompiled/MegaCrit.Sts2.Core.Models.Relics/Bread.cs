using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Bread : RelicModel
{
	private const string _gainEnergyKey = "GainEnergy";

	private const string _loseEnergyKey = "LoseEnergy";

	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new EnergyVar("GainEnergy", 1),
		new EnergyVar("LoseEnergy", 2)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		PlayerCombatState? playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState != null && playerCombatState.TurnNumber == 1)
		{
			return amount;
		}
		return amount + base.DynamicVars["GainEnergy"].BaseValue;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner.Creature) && base.Owner.PlayerCombatState.TurnNumber == 1)
		{
			await PlayerCmd.LoseEnergy(base.DynamicVars["LoseEnergy"].BaseValue, base.Owner);
		}
	}
}
