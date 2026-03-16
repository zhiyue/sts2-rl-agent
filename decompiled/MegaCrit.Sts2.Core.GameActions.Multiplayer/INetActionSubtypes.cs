using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.DevConsole;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public static class INetActionSubtypes
{
	private static readonly Type _t0 = typeof(NetConsoleCmdGameAction);

	private static readonly Type _t1 = typeof(NetDiscardPotionGameAction);

	private static readonly Type _t2 = typeof(NetEndPlayerTurnAction);

	private static readonly Type _t3 = typeof(NetMoveToMapCoordAction);

	private static readonly Type _t4 = typeof(NetPickRelicAction);

	private static readonly Type _t5 = typeof(NetPlayCardAction);

	private static readonly Type _t6 = typeof(NetReadyToBeginEnemyTurnAction);

	private static readonly Type _t7 = typeof(NetUndoEndPlayerTurnAction);

	private static readonly Type _t8 = typeof(NetUsePotionAction);

	private static readonly Type _t9 = typeof(NetVoteForMapCoordAction);

	private static readonly Type _t10 = typeof(NetVoteToMoveToNextActAction);

	private static readonly Type[] _subtypes = new Type[11]
	{
		_t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8, _t9,
		_t10
	};

	public static int Count => 11;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
