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
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BoomingConch : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(2),
		new EnergyVar(1)
	});

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		if (base.Owner.PlayerCombatState.TurnNumber > 1)
		{
			return count;
		}
		AbstractRoom? currentRoom = player.RunState.CurrentRoom;
		if (currentRoom == null || currentRoom.RoomType != RoomType.Elite)
		{
			return count;
		}
		return count + (decimal)base.DynamicVars.Cards.IntValue;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner.Creature) && base.Owner.PlayerCombatState.TurnNumber <= 1)
		{
			AbstractRoom? currentRoom = combatState.RunState.CurrentRoom;
			if (currentRoom != null && currentRoom.RoomType == RoomType.Elite)
			{
				Flash();
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			}
		}
	}
}
