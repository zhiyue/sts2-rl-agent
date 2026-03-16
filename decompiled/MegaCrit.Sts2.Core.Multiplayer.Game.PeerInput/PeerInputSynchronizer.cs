using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public class PeerInputSynchronizer : IDisposable
{
	private class PeerInputState
	{
		public ulong playerId;

		public Vector2 netMousePosition;

		public bool isMouseDown;

		public NetScreenType netScreenType;

		public HoveredModelData hoveredModelData;

		public bool isTargeting;

		public bool isUsingController;

		public Vector2 controllerFocusPosition;
	}

	public const int minUpdateMsec = 50;

	private readonly INetGameService _netService;

	private readonly List<PeerInputState> _inputStates = new List<PeerInputState>();

	private ulong _lastSyncMsec;

	private Task? _syncMessageTask;

	private PeerInputMessage? _syncMessageToSend;

	private INetCursorPositionTranslator? _cursorTranslator;

	private MegaCrit.Sts2.Core.Logging.Logger _logger = new MegaCrit.Sts2.Core.Logging.Logger("PeerInputSynchronizer", LogType.VisualSync);

	public INetGameService NetService => _netService;

	public event Action<ulong>? StateAdded;

	public event Action<ulong>? StateRemoved;

	public event Action<ulong>? StateChanged;

	public event Action<ulong, NetScreenType>? ScreenChanged;

	public PeerInputSynchronizer(INetGameService netService)
	{
		_netService = netService;
		_netService.RegisterMessageHandler<PeerInputMessage>(HandlePeerInputMessage);
		GetOrCreateStateForPlayer(_netService.NetId);
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<PeerInputMessage>(HandlePeerInputMessage);
	}

	public void SyncLocalMousePos(Vector2 mouseScreenPos, Control rootControl)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		Vector2 vector = _cursorTranslator?.GetNetPositionFromScreenPosition(mouseScreenPos) ?? NetCursorHelper.GetNormalizedPosition(mouseScreenPos, rootControl);
		_syncMessageToSend.netMousePos = vector;
		orCreateStateForPlayer.netMousePosition = vector;
		this.StateChanged?.Invoke(_netService.NetId);
		TrySendSyncMessage();
	}

	public void SyncLocalControllerFocus(Vector2 focusPosition, Control rootControl)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		Vector2 vector = _cursorTranslator?.GetNetPositionFromScreenPosition(focusPosition) ?? NetCursorHelper.GetNormalizedPosition(focusPosition, rootControl);
		_syncMessageToSend.controllerFocusPosition = vector;
		orCreateStateForPlayer.controllerFocusPosition = vector;
		this.StateChanged?.Invoke(_netService.NetId);
		TrySendSyncMessage();
	}

	public void SyncLocalIsUsingController(bool isUsingController)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		_syncMessageToSend.isUsingController = isUsingController;
		orCreateStateForPlayer.isUsingController = isUsingController;
		this.StateChanged?.Invoke(_netService.NetId);
		TrySendSyncMessage();
	}

	public void SyncLocalMouseDown(bool mouseDown)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		orCreateStateForPlayer.isMouseDown = mouseDown;
		orCreateStateForPlayer.isUsingController = false;
		this.StateChanged?.Invoke(_netService.NetId);
		TrySendSyncMessage();
	}

	public void SyncLocalScreen(NetScreenType netScreenType)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		if (orCreateStateForPlayer.netScreenType != netScreenType)
		{
			_logger.Debug($"Local screen changed: {orCreateStateForPlayer.netScreenType}->{netScreenType}");
			orCreateStateForPlayer.netScreenType = netScreenType;
			TrySendSyncMessage();
			this.StateChanged?.Invoke(_netService.NetId);
		}
	}

	public void SyncLocalHoveredModel(AbstractModel? model)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		HoveredModelData hoveredModelData = HoveredModelData.FromModel(model);
		if (!hoveredModelData.Equals(orCreateStateForPlayer.hoveredModelData))
		{
			orCreateStateForPlayer.hoveredModelData = hoveredModelData;
			TrySendSyncMessage();
			this.StateChanged?.Invoke(_netService.NetId);
		}
	}

	public void SyncLocalIsTargeting(bool isTargeting)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
		if (_syncMessageToSend == null)
		{
			_syncMessageToSend = new PeerInputMessage();
		}
		if (orCreateStateForPlayer.isTargeting != isTargeting)
		{
			orCreateStateForPlayer.isTargeting = isTargeting;
			TrySendSyncMessage();
			this.StateChanged?.Invoke(_netService.NetId);
		}
	}

	private void TrySendSyncMessage()
	{
		if (_syncMessageTask == null)
		{
			int num = (int)(_lastSyncMsec + 50 - Time.GetTicksMsec());
			if (num <= 0)
			{
				_syncMessageTask = TaskHelper.RunSafely(SendSyncMessageAfterSmallDelay());
			}
			else
			{
				_syncMessageTask = TaskHelper.RunSafely(QueueSyncMessage(num));
			}
		}
	}

	private async Task QueueSyncMessage(int delayMsec)
	{
		await Task.Delay(delayMsec);
		SendSyncMessage();
	}

	private async Task SendSyncMessageAfterSmallDelay()
	{
		await Task.Yield();
		SendSyncMessage();
	}

	private void SendSyncMessage()
	{
		if (_netService.IsConnected)
		{
			PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
			_syncMessageToSend.mouseDown = orCreateStateForPlayer.isMouseDown;
			_syncMessageToSend.screenType = orCreateStateForPlayer.netScreenType;
			_syncMessageToSend.isTargeting = orCreateStateForPlayer.isTargeting;
			_syncMessageToSend.hoveredModelData = orCreateStateForPlayer.hoveredModelData;
			_syncMessageToSend.isUsingController = orCreateStateForPlayer.isUsingController;
			_syncMessageToSend.controllerFocusPosition = orCreateStateForPlayer.controllerFocusPosition;
			_netService.SendMessage(_syncMessageToSend);
			_lastSyncMsec = Time.GetTicksMsec();
			_syncMessageToSend = null;
			_syncMessageTask = null;
		}
	}

	private PeerInputState? GetStateForPlayer(ulong playerId)
	{
		int num = _inputStates.FindIndex((PeerInputState s) => s.playerId == playerId);
		if (num >= 0)
		{
			return _inputStates[num];
		}
		return null;
	}

	private PeerInputState ForceGetStateForPlayer(ulong playerId)
	{
		return GetStateForPlayer(playerId) ?? throw new InvalidOperationException($"Tried to get PeerInputState for non-existent player {playerId}!");
	}

	private PeerInputState GetOrCreateStateForPlayer(ulong playerId)
	{
		PeerInputState peerInputState = GetStateForPlayer(playerId);
		if (peerInputState == null)
		{
			peerInputState = new PeerInputState();
			peerInputState.playerId = playerId;
			_inputStates.Add(peerInputState);
			this.StateAdded?.Invoke(playerId);
		}
		return peerInputState;
	}

	public void StartOverridingCursorPositioning(INetCursorPositionTranslator positionTranslator)
	{
		_cursorTranslator = positionTranslator;
	}

	public void StopOverridingCursorPositioning()
	{
		_cursorTranslator = null;
	}

	private void HandlePeerInputMessage(PeerInputMessage message, ulong senderId)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(senderId);
		if (orCreateStateForPlayer.isMouseDown != message.mouseDown)
		{
			_logger.Debug($"Mouse down state for {senderId} changed: {orCreateStateForPlayer.isMouseDown}->{message.mouseDown}");
		}
		if (orCreateStateForPlayer.netScreenType != message.screenType)
		{
			_logger.Debug($"Remote screen for {senderId} changed: {orCreateStateForPlayer.netScreenType}->{message.screenType}");
		}
		if (orCreateStateForPlayer.isTargeting != message.isTargeting)
		{
			_logger.Debug($"Targeting state for {senderId} changed: {orCreateStateForPlayer.isTargeting}->{message.isTargeting}");
		}
		if (!orCreateStateForPlayer.hoveredModelData.Equals(message.hoveredModelData))
		{
			_logger.Debug($"Hovered model for {senderId} changed: {orCreateStateForPlayer.hoveredModelData}->{message.hoveredModelData}");
		}
		if (!orCreateStateForPlayer.controllerFocusPosition.Equals(message.controllerFocusPosition))
		{
			_logger.Debug($"Controller focus position for {senderId} changed: {orCreateStateForPlayer.controllerFocusPosition}->{message.controllerFocusPosition}");
		}
		if (!orCreateStateForPlayer.isUsingController.Equals(message.isUsingController))
		{
			_logger.Debug($"Using controller state state for {senderId} changed: {orCreateStateForPlayer.isUsingController}->{message.isUsingController}");
		}
		NetScreenType netScreenType = orCreateStateForPlayer.netScreenType;
		orCreateStateForPlayer.netMousePosition = message.netMousePos ?? orCreateStateForPlayer.netMousePosition;
		orCreateStateForPlayer.isMouseDown = message.mouseDown;
		orCreateStateForPlayer.netScreenType = message.screenType;
		orCreateStateForPlayer.isTargeting = message.isTargeting;
		orCreateStateForPlayer.hoveredModelData = message.hoveredModelData;
		orCreateStateForPlayer.isUsingController = message.isUsingController;
		orCreateStateForPlayer.controllerFocusPosition = message.controllerFocusPosition ?? orCreateStateForPlayer.controllerFocusPosition;
		this.StateChanged?.Invoke(senderId);
		if (netScreenType != orCreateStateForPlayer.netScreenType)
		{
			this.ScreenChanged?.Invoke(senderId, netScreenType);
		}
	}

	public Vector2 GetControlSpaceFocusPosition(ulong playerId, Control rootControl)
	{
		PeerInputState peerInputState = ForceGetStateForPlayer(playerId);
		Vector2 vector = (peerInputState.isUsingController ? peerInputState.controllerFocusPosition : peerInputState.netMousePosition);
		if (_cursorTranslator == null)
		{
			return NetCursorHelper.GetControlSpacePosition(vector, rootControl);
		}
		Vector2 screenPositionFromNetPosition = _cursorTranslator.GetScreenPositionFromNetPosition(vector);
		return rootControl.GetGlobalTransformWithCanvas() * screenPositionFromNetPosition;
	}

	public bool GetMouseDown(ulong playerId)
	{
		return ForceGetStateForPlayer(playerId).isMouseDown;
	}

	public NetScreenType GetScreenType(ulong playerId)
	{
		return ForceGetStateForPlayer(playerId).netScreenType;
	}

	public HoveredModelData GetHoveredModelData(ulong playerId)
	{
		return ForceGetStateForPlayer(playerId).hoveredModelData;
	}

	public bool GetIsTargeting(ulong playerId)
	{
		return ForceGetStateForPlayer(playerId).isTargeting;
	}

	public void OnPlayerDisconnected(ulong playerId)
	{
		_logger.Debug($"Disconnected player {playerId}, removing PeerInputState");
		_inputStates.RemoveAll((PeerInputState p) => p.playerId == playerId);
		this.StateRemoved?.Invoke(playerId);
	}
}
