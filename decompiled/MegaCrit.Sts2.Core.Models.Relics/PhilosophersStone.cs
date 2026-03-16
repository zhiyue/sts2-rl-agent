using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PhilosophersStone : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<StrengthPower>(1m),
		new EnergyVar(1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.ForEnergy(this)
	});

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		return amount + (decimal)base.DynamicVars.Energy.IntValue;
	}

	public override Task AfterCreatureAddedToCombat(Creature creature)
	{
		if (creature.Side == base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		Flash();
		return PowerCmd.Apply<StrengthPower>(creature, base.DynamicVars["StrengthPower"].BaseValue, null, null);
	}

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			IEnumerable<Creature> targets = from c in base.Owner.Creature.CombatState.GetOpponentsOf(base.Owner.Creature)
				where c.IsAlive
				select c;
			Flash();
			await PowerCmd.Apply<StrengthPower>(targets, base.DynamicVars["StrengthPower"].BaseValue, null, null);
		}
	}
}
