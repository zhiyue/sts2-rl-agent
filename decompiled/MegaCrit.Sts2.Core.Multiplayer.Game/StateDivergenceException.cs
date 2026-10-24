using System;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class StateDivergenceException : Exception
{
	public StateDivergenceException(string message)
		: base(message)
	{
	}
}
