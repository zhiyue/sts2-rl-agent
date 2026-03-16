using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Multiplayer.Messages;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class INetMessageSubtypes
{
	private static readonly Type _t0 = typeof(ActionEnqueuedMessage);

	private static readonly Type _t1 = typeof(CardRemovedMessage);

	private static readonly Type _t2 = typeof(ChecksumDataMessage);

	private static readonly Type _t3 = typeof(StateDivergenceMessage);

	private static readonly Type _t4 = typeof(ClearMapDrawingsMessage);

	private static readonly Type _t5 = typeof(EndTurnPingMessage);

	private static readonly Type _t6 = typeof(MapDrawingMessage);

	private static readonly Type _t7 = typeof(MapDrawingModeChangedMessage);

	private static readonly Type _t8 = typeof(MapPingMessage);

	private static readonly Type _t9 = typeof(ReactionMessage);

	private static readonly Type _t10 = typeof(RestSiteOptionHoveredMessage);

	private static readonly Type _t11 = typeof(HookActionEnqueuedMessage);

	private static readonly Type _t12 = typeof(MerchantCardRemovalMessage);

	private static readonly Type _t13 = typeof(PaelsWingSacrificeMessage);

	private static readonly Type _t14 = typeof(PlayerChoiceMessage);

	private static readonly Type _t15 = typeof(RequestEnqueueActionMessage);

	private static readonly Type _t16 = typeof(RequestEnqueueHookActionMessage);

	private static readonly Type _t17 = typeof(RequestResumeActionAfterPlayerChoiceMessage);

	private static readonly Type _t18 = typeof(ResumeActionAfterPlayerChoiceMessage);

	private static readonly Type _t19 = typeof(RunAbandonedMessage);

	private static readonly Type _t20 = typeof(GoldLostMessage);

	private static readonly Type _t21 = typeof(OptionIndexChosenMessage);

	private static readonly Type _t22 = typeof(PeerInputMessage);

	private static readonly Type _t23 = typeof(RewardObtainedMessage);

	private static readonly Type _t24 = typeof(SharedEventOptionChosenMessage);

	private static readonly Type _t25 = typeof(VotedForSharedEventOptionMessage);

	private static readonly Type _t26 = typeof(SyncPlayerDataMessage);

	private static readonly Type _t27 = typeof(SyncRngMessage);

	private static readonly Type _t28 = typeof(TreasureChestOpenedMessage);

	private static readonly Type _t29 = typeof(HeartbeatRequestMessage);

	private static readonly Type _t30 = typeof(HeartbeatResponseMessage);

	private static readonly Type _t31 = typeof(ClientLoadJoinRequestMessage);

	private static readonly Type _t32 = typeof(ClientLoadJoinResponseMessage);

	private static readonly Type _t33 = typeof(ClientLobbyJoinRequestMessage);

	private static readonly Type _t34 = typeof(ClientLobbyJoinResponseMessage);

	private static readonly Type _t35 = typeof(ClientRejoinRequestMessage);

	private static readonly Type _t36 = typeof(ClientRejoinResponseMessage);

	private static readonly Type _t37 = typeof(InitialGameInfoMessage);

	private static readonly Type _t38 = typeof(LobbyAscensionChangedMessage);

	private static readonly Type _t39 = typeof(LobbyBeginLoadedRunMessage);

	private static readonly Type _t40 = typeof(LobbyBeginRunMessage);

	private static readonly Type _t41 = typeof(LobbyModifiersChangedMessage);

	private static readonly Type _t42 = typeof(LobbyPlayerChangedCharacterMessage);

	private static readonly Type _t43 = typeof(LobbyPlayerSetReadyMessage);

	private static readonly Type _t44 = typeof(LobbySeedChangedMessage);

	private static readonly Type _t45 = typeof(PlayerJoinedMessage);

	private static readonly Type _t46 = typeof(PlayerLeftMessage);

	private static readonly Type _t47 = typeof(PlayerReconnectedMessage);

	private static readonly Type _t48 = typeof(PlayerRejoinedMessage);

	private static readonly Type[] _subtypes = new Type[49]
	{
		_t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8, _t9,
		_t10, _t11, _t12, _t13, _t14, _t15, _t16, _t17, _t18, _t19,
		_t20, _t21, _t22, _t23, _t24, _t25, _t26, _t27, _t28, _t29,
		_t30, _t31, _t32, _t33, _t34, _t35, _t36, _t37, _t38, _t39,
		_t40, _t41, _t42, _t43, _t44, _t45, _t46, _t47, _t48
	};

	public static int Count => 49;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
