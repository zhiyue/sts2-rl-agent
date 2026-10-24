using System;
using Steamworks;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public class SteamRemoteSaveStoreException : Exception
{
	public EResult Result { get; private set; }

	public SteamRemoteSaveStoreException(string message, EResult result)
		: base(message)
	{
		Result = result;
	}
}
