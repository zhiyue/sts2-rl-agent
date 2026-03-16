using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

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
