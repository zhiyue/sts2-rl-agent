using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Fiddle : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override decimal ModifyHandDrawLate(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		return count + (decimal)base.DynamicVars.Cards.IntValue;
	}

	public override bool ShouldDraw(Player player, bool fromHandDraw)
	{
		if (fromHandDraw)
		{
			return true;
		}
		if (player != base.Owner)
		{
			return true;
		}
		if (player.Creature.Side != player.Creature.CombatState.CurrentSide)
		{
			return true;
		}
		return false;
	}

	public override Task AfterPreventingDraw()
	{
		Flash();
		return Task.CompletedTask;
	}
}
