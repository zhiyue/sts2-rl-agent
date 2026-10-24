using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

/// <summary>
/// For use when we don't care if player choice blocks the task.
/// In almost all combat-related scenarios, we use HookPlayerChoiceContext to unblock other player queues when a player
/// choice is made. However, there are several scenarios in which we can't do so:
///  - Relic AfterObtained callbacks. The only way these can occur during combat is via console.
///  - Enemy turn. When it's the enemy turn, we want to block the rest of the enemy turn from executing until after the
///    player has finished making their choice. This should only happen in very rare circumstances, like when Centennial
///    Puzzle triggers, drawing a Seeking Strike which is then autoplayed via Hellraiser.
///
/// In all other circumstances, you should prefer either HookPlayerChoiceContext or ThrowingPlayerChoiceContext,
/// depending on the situation.
/// </summary>
public class BlockingPlayerChoiceContext : PlayerChoiceContext
{
	public override Task SignalPlayerChoiceBegun(PlayerChoiceOptions options)
	{
		return Task.CompletedTask;
	}

	public override Task SignalPlayerChoiceEnded()
	{
		return Task.CompletedTask;
	}
}
