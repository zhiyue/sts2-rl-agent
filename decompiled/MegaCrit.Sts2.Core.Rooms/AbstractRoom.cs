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

	public abstract ModelId? ModelId { get; }

	public virtual bool IsPreFinished => false;

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

	public abstract Task Enter(IRunState? runState, bool isRestoringRoomStackBase);

	public abstract Task Exit(IRunState? runState);

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
