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

	public EventModel CanonicalEvent { get; }

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

	public override async Task Enter(IRunState? runState, bool isRestoringRoomStackBase)
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

	public override Task Exit(IRunState? runState)
	{
		if (CanonicalEvent.IsDeterministic)
		{
			RunManager.Instance.ChecksumTracker.GenerateChecksum($"Exiting event room {CanonicalEvent.Id}", null);
		}
		foreach (EventModel @event in RunManager.Instance.EventSynchronizer.Events)
		{
			@event.StateChanged -= OnEventStateChanged;
			@event.EnsureCleanup();
		}
		return Task.CompletedTask;
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
