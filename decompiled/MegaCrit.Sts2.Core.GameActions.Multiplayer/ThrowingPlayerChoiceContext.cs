using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

/// <summary>
/// For use when we are quite certain that no path will ever result in a player choice call deeper in the callstack.
/// This is currently used in a few scenarios:
///  - In events, where we're out-of-combat, and so player choice cannot occur.
///  - In tests where we are certain that calling a cmd will not trigger a player choice.
///  - In Sleight of Flesh Power's trigger, to prevent having to pass PlayerChoiceContext to PowerCmd.Apply. (This could
///    change if we find out that player choice can be triggered in that scenario.)
/// </summary>
public class ThrowingPlayerChoiceContext : PlayerChoiceContext
{
	public override Task SignalPlayerChoiceBegun(PlayerChoiceOptions options)
	{
		throw new NotImplementedException();
	}

	public override Task SignalPlayerChoiceEnded()
	{
		throw new NotImplementedException();
	}
}
