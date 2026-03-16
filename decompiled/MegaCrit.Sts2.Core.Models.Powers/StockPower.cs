using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class StockPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented && target == base.Owner && base.Amount > 0)
		{
			await Cmd.CustomScaledWait(deathAnimLength, deathAnimLength);
			Axebot axebot = (Axebot)ModelDb.Monster<Axebot>().ToMutable();
			axebot.ShouldPlaySpawnAnimation = true;
			axebot.StockAmount = base.Amount - 1;
			await CreatureCmd.TriggerAnim(await CreatureCmd.Add(axebot, base.CombatState, base.Owner.Side, base.Owner.SlotName), "respawn", 0f);
		}
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return true;
	}
}
