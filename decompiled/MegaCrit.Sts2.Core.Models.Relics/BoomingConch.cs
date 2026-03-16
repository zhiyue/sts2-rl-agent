using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BoomingConch : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		if (player.Creature.CombatState.RoundNumber > 1)
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
}
