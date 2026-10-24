using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class EventRoom : AbstractRoom
{
	private bool _isPreFinished;

	public override RoomType RoomType => RoomType.Event;

	public override ModelId ModelId => CanonicalEvent.Id;

	/// <summary>
	/// The canonical version of the event that the player is doing in this room.
	/// Unlike CombatRoom.Encounter, we want this to be canonical, because we create a separate mutable copy for each
	/// player.
	/// </summary>
	public EventModel CanonicalEvent { get; }

	/// <summary>
	/// The mutable version of the event that the local player is doing in this room.
	/// When using this, keep in mind that the
	/// </summary>
	public EventModel LocalMutableEvent => RunManager.Instance.EventSynchronizer.GetLocalEvent();

	public Action<EventModel>? OnStart { private get; init; }

	public override bool IsPreFinished => _isPreFinished;

	public EventRoom(EventModel eventModel)
	{
		eventModel.AssertCanonical();
		CanonicalEvent = eventModel;
	}

	public EventRoom(SerializableRoom serializableRoom)
	{
		CanonicalEvent = SaveUtil.EventOrDeprecated(serializableRoom.EventId);
		if (serializableRoom.IsPreFinished)
		{
			MarkPreFinished();
		}
	}

	public override async Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase)
	{
		await PreloadManager.LoadRoomEventAssets(CanonicalEvent, runState ?? NullRunState.Instance);
		RunManager.Instance.EventSynchronizer.BeginEvent(CanonicalEvent, IsPreFinished, OnStart);
		foreach (EventModel @event in RunManager.Instance.EventSynchronizer.Events)
		{
			@event.StateChanged += OnEventStateChanged;
			if (@event.IsFinished && !IsPreFinished)
			{
				OnEventStateChanged(@event);
			}
		}
		EventModel localEvent = RunManager.Instance.EventSynchronizer.GetLocalEvent();
		if (localEvent.LayoutType == EventLayoutType.Combat)
		{
			localEvent.GenerateInternalCombatState(runState ?? NullRunState.Instance);
		}
		if (!isRestoringRoomStackBase)
		{
			NEventRoom currentRoom = NEventRoom.Create(localEvent, runState, _isPreFinished);
			NRun.Instance?.SetCurrentRoom(currentRoom);
		}
		if (runState != null)
		{
			await Hook.AfterRoomEntered(runState, this);
		}
		await localEvent.AfterEventStarted();
	}

	public override async Task Exit(IRunState? runState)
	{
		await RunManager.Instance.EventSynchronizer.AwaitPendingOptionTasks();
		EventModel localEvent = RunManager.Instance.EventSynchronizer.GetLocalEvent();
		if (localEvent.IsDeterministic)
		{
			RunManager.Instance.ChecksumTracker.GenerateChecksum($"Exiting event room {localEvent.Id}", null);
		}
		if (localEvent.LayoutType == EventLayoutType.Combat)
		{
			localEvent.ResetInternalCombatState();
		}
		foreach (EventModel @event in RunManager.Instance.EventSynchronizer.Events)
		{
			@event.StateChanged -= OnEventStateChanged;
			@event.EnsureCleanup();
		}
	}

	public override Task Resume(AbstractRoom exitedRoom, IRunState? runState)
	{
		RunManager.Instance.EventSynchronizer.ResumeEvents(exitedRoom);
		EventModel localEvent = RunManager.Instance.EventSynchronizer.GetLocalEvent();
		NRun.Instance?.SetCurrentRoom(NEventRoom.Create(localEvent, runState, _isPreFinished));
		return Task.CompletedTask;
	}

	public override SerializableRoom ToSerializable()
	{
		SerializableRoom serializableRoom = base.ToSerializable();
		serializableRoom.EventId = CanonicalEvent.Id;
		serializableRoom.IsPreFinished = IsPreFinished;
		return serializableRoom;
	}

	public void MarkPreFinished()
	{
		_isPreFinished = true;
	}

	private void OnEventStateChanged(EventModel eventModel)
	{
		if (!(eventModel is AncientEventModel))
		{
			return;
		}
		foreach (EventModel @event in RunManager.Instance.EventSynchronizer.Events)
		{
			if (!@event.IsFinished)
			{
				return;
			}
		}
		MarkPreFinished();
		TaskHelper.RunSafely(SaveManager.Instance.SaveRun(this));
	}
}
