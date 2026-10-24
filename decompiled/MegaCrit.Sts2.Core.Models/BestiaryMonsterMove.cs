using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Models;

/// <summary>
/// Encapsulates the data needed to play an animation and accompanied sfx for a
/// "move" for a given creature in the Bestiary.
/// </summary>
public struct BestiaryMonsterMove
{
	private static readonly LocString _attackMoveName = new LocString("bestiary", "ACTION_NAME.attack");

	private static readonly LocString _castMoveName = new LocString("bestiary", "ACTION_NAME.cast");

	private static readonly LocString _dieMoveName = new LocString("bestiary", "ACTION_NAME.die");

	private static readonly LocString _hurtMoveName = new LocString("bestiary", "ACTION_NAME.hurt");

	private static readonly LocString _reviveMoveName = new LocString("bestiary", "ACTION_NAME.revive");

	private static readonly LocString _stunMoveName = new LocString("bestiary", "ACTION_NAME.stun");

	public string displayName;

	public string? animId;

	public string? sfx;

	public string? stateId;

	public Func<IReadOnlyList<Creature>, Task>? nonStateMove;

	public Func<Task>? action;

	public bool stopSfxLoops;

	public static BestiaryMonsterMove FromAnim(string animId, string? sfx)
	{
		BestiaryMonsterMove result = new BestiaryMonsterMove
		{
			animId = animId
		};
		string text = (animId.StartsWith("attack") ? _attackMoveName.GetRawText() : (animId switch
		{
			"cast" => _castMoveName.GetRawText(), 
			"die" => _dieMoveName.GetRawText(), 
			"hurt" => _hurtMoveName.GetRawText(), 
			"revive" => _reviveMoveName.GetRawText(), 
			"stun" => _stunMoveName.GetRawText(), 
			_ => animId, 
		}));
		result.displayName = text;
		result.sfx = sfx;
		return result;
	}

	public BestiaryMonsterMove StopOtherSfx()
	{
		stopSfxLoops = true;
		return this;
	}

	public static BestiaryMonsterMove FromAnim(LocString moveName, string animId, string? sfx)
	{
		return new BestiaryMonsterMove
		{
			animId = animId,
			displayName = moveName.GetRawText(),
			sfx = sfx
		};
	}

	public static BestiaryMonsterMove FromState(LocString moveName, string stateId)
	{
		return new BestiaryMonsterMove
		{
			displayName = moveName.GetRawText(),
			stateId = stateId
		};
	}

	public static BestiaryMonsterMove FromState(string stateId)
	{
		return new BestiaryMonsterMove
		{
			displayName = stateId,
			stateId = stateId
		};
	}

	public static BestiaryMonsterMove FromNonStateMove(LocString moveName, Func<IReadOnlyList<Creature>, Task> nonStateMove)
	{
		return new BestiaryMonsterMove
		{
			displayName = moveName.GetRawText(),
			nonStateMove = nonStateMove
		};
	}

	public static BestiaryMonsterMove FromAction(LocString moveName, Func<Task> action)
	{
		return new BestiaryMonsterMove
		{
			displayName = moveName.GetRawText(),
			action = action
		};
	}

	public static BestiaryMonsterMove FromStun(Func<Task> action)
	{
		return FromAction(new LocString("monsters", "GENERIC.moves.STUNNED.title"), action);
	}
}
