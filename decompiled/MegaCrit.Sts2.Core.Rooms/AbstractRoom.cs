using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public abstract class AbstractRoom
{
	public abstract RoomType RoomType { get; }

	/// <summary>
	/// The model ID associated with this room. Non-null for combat rooms and event rooms.
	/// </summary>
	public abstract ModelId? ModelId { get; }

	/// <summary>
	/// Whether or not this room should load in as already finished.
	/// Used by the save/load system when the player saves after finishing a room, but before choosing a new map
	/// location to visit.
	/// </summary>
	public virtual bool IsPreFinished => false;

	/// <summary>
	/// The ID of the room within the current map coordinate.
	/// This id is only unique within each map coordinate. You usually shouldn't have to use this directly; look at
	/// <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.RunLocation" /> instead.
	/// This is not stable across save/load.
	/// </summary>
	public int? Id { get; private set; }

	/// <summary>
	/// Is this the "Victory" room where the player meets the Architect and the run ends?
	/// </summary>
	public bool IsVictoryRoom
	{
		get
		{
			if (this is EventRoom eventRoom)
			{
				return eventRoom.CanonicalEvent is TheArchitect;
			}
			return false;
		}
	}

	/// <summary>
	/// Called when this room is being entered.
	/// </summary>
	/// <param name="runState">The state of the run that this room is being entered in.</param>
	/// <param name="isRestoringRoomStackBase">
	/// If true, skip hooks and room visit tracking. Used when reconstructing the base of the room stack on load
	/// (e.g., pushing a parent EventRoom underneath a pre-finished CombatRoom).
	/// </param>
	public Task Enter(IRunState? runState, bool isRestoringRoomStackBase)
	{
		Id = runState?.GetAndIncrementNextRoomId();
		return EnterInternal(runState, isRestoringRoomStackBase);
	}

	/// <summary>
	/// Override this in implementors.
	/// </summary>
	/// <param name="runState">The state of the run that this room is being entered in.</param>
	/// <param name="isRestoringRoomStackBase">
	/// If true, skip hooks and room visit tracking. Used when reconstructing the base of the room stack on load
	/// (e.g., pushing a parent EventRoom underneath a pre-finished CombatRoom).
	/// </param>
	public abstract Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase);

	/// <summary>
	/// Called when this room is being exited.
	/// </summary>
	/// <param name="runState">The state of the run that this room is being exited in.</param>
	public abstract Task Exit(IRunState? runState);

	/// <summary>
	/// Called when another room is popped off the room stack, causing this to become the current room again.
	/// </summary>
	/// <param name="exitedRoom">The room that was exited before this was called.</param>
	/// <param name="runState">The state of the run that this room is being resumed in.</param>
	public abstract Task Resume(AbstractRoom exitedRoom, IRunState? runState);

	public virtual SerializableRoom ToSerializable()
	{
		return new SerializableRoom
		{
			RoomType = RoomType
		};
	}

	public static AbstractRoom? FromSerializable(SerializableRoom? serializableRoom, IRunState? runState)
	{
		if (serializableRoom == null)
		{
			return null;
		}
		switch (serializableRoom.RoomType)
		{
		case RoomType.Monster:
		case RoomType.Elite:
		case RoomType.Boss:
			return CombatRoom.FromSerializable(serializableRoom, runState);
		case RoomType.Event:
			return new EventRoom(serializableRoom);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
