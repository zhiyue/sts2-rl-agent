using System;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;

[Flags]
public enum SteamNetworkingSend
{
	Unreliable = 0,
	Reliable = 8,
	NoNagle = 1,
	NoDelay = 4
}
