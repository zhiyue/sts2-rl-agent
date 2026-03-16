using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public class ClientConnectionFailedException : Exception
{
	public NetErrorInfo info;

	public ClientConnectionFailedException(string message, NetErrorInfo info)
		: base(message)
	{
		this.info = info;
	}
}
