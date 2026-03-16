using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DivineRight : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StarsVar(3));

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
		}
	}
}
