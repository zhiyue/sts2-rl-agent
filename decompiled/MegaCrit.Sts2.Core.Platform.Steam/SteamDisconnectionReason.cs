namespace MegaCrit.Sts2.Core.Platform.Steam;

public enum SteamDisconnectionReason
{
	None = 0,
	AppGeneric = 1000,
	AppInternalError = 1017,
	AppException = 2000,
	LocalMin = 3000,
	RunningInOfflineMode = 3001,
	ManyRelayConnectivity = 3002,
	HostedServerPrimaryRelay = 3003,
	NetworkConfig = 3004,
	LocalRights = 3005,
	LocalMax = 3999,
	RemoteTimeout = 4001,
	BadCrypt = 4002,
	BadCert = 4003,
	NotLoggedIn = 4004,
	NotRunningApp = 4005,
	BadProtocolVersion = 4006,
	MiscGeneric = 5001,
	InternalError = 5002,
	MiscTimeout = 5003,
	RelayConnectivity = 5004,
	SteamConnectivity = 5005,
	NoRelaySessions = 5006
}
