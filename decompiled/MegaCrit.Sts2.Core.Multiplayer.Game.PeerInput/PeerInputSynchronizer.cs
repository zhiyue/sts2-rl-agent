using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

/// <summary>
/// Synchronizes our local mouse state with peers and remote peers' with ours.
/// </summary>
public class PeerInputSynchronizer : IDisposable
{
	private class PeerInputState
	{
		/// <summary> The player to whom this state belongs. </summary>
		public ulong playerId;

		/// <summary> The mouse position of the player. </summary>
		public Vector2 netMousePosition;

		/// <summary> True if the player's mouse is held down, false otherwise. </summary>
		public bool isMouseDown;

		/// <summary> Which screen this player is looking at. </summary>
		public NetScreenType netScreenType;

		/// <summary> The combat model the player is hovering over. Should only be non-null in combat. </summary>
		public HoveredModelData hoveredModelData;

		/// <summary> True if the player is in combat and is currently in targeting mode (drawing the targeting arrow),
		/// false otherwise. </summary>
		public bool isTargeting;

		/// <summary>
		/// True if the player is currently using a controller.
		/// </summary>
		public bool isUsingController;

		/// <summary>
		/// Tracks the position of the current focused control. Only valid if isUsingController is true.
		/// </summary>
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

	/// <summary>
	/// If this is unset, Time.GetTicksMsec is used to check how much time has passed.
	/// This is used to mock time passing for tests.
	/// </summary>
	public Func<ulong>? mockGetTicksMsec;

	/// <summary>
	/// If this is unset, Task.Delay is used to buffer messages.
	/// This is used to mock time passing for tests.
	/// </summary>
	public Func<int, Task>? mockDelay;

	/// <summary>
	/// If this is unset, Task.Yield is used to wait a moment before messages are sent.
	/// This is used to mock time passing for tests.
	/// </summary>
	public Func<Task>? mockWaitSmall;

	public INetGameService NetService => _netService;

	/// <summary>
	/// Event fired when we first add a PeerInputState for a player.
	/// This occurs when a player emits a <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync.PeerInputMessage" />, or when we attempt to get the input state for a player.
	/// </summary>
	public event Action<ulong>? StateAdded;

	/// <summary>
	/// Event fired when a player disconnects, and their input state is removed.
	/// </summary>
	public event Action<ulong>? StateRemoved;

	/// <summary>
	/// Event fired when any part of a player's input state is changed.
	/// </summary>
	public event Action<ulong>? StateChanged;

	/// <summary>
	/// Event fired only when a player emits a PeerInputMessage with a new screen type.
	/// The second argument is the old screen type.
	/// </summary>
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

	/// <summary>
	/// Sync the local mouse position to other clients.
	/// This should be called any time the mouse position changes - the message will be buffered and sent at regular intervals.
	/// </summary>
	/// <param name="mouseScreenPos">The screen position of the mouse.</param>
	/// <param name="rootControl">The root control, used to normalize mouse coordinates across different resolutions and aspect ratios.</param>
	public void SyncLocalMousePos(Vector2 mouseScreenPos, Control? rootControl)
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

	/// <summary>
	/// Sync the position of the currently focused control node with other clients.
	/// </summary>
	/// <param name="focusPosition">the screen position of the currently focused control node</param>
	/// <param name="rootControl">The root control, used to normalize mouse coordinates across different resolutions and aspect ratios.</param>
	public void SyncLocalControllerFocus(Vector2 focusPosition, Control? rootControl)
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

	/// <summary>
	/// Sync whether or not the local player is using a controller.
	/// </summary>
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

	/// <summary>
	/// Sync the local mouse down state with other clients (so they show the correct cursor).
	/// </summary>
	/// <param name="mouseDown">True if the mouse is down, false otherwise.</param>
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

	/// <summary>
	/// Sync the topmost overlay/capstone on the local player's screen to other peers.
	/// </summary>
	/// <param name="netScreenType">The topmost overlay/capstone on the local player's screen.</param>
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

	/// <summary>
	/// ONLY the HoveredModelTracker should call this! You likely want to call one of the HoveredModelTracker methods instead.
	/// Sync the model that the player is currently hovering.
	/// </summary>
	/// <param name="model">The model that the local player is hovering.</param>
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

	/// <summary>
	/// Sync whether or not the player is currently in targeting mode.
	/// </summary>
	/// <param name="isTargeting">True if we're currently displaying the targeting arrow locally, false otherwise.</param>
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

	/// <summary>
	/// Sends a sync message if enough time has passed since the last one, or buffers it to be sent otherwise.
	/// </summary>
	private void TrySendSyncMessage()
	{
		if (_syncMessageTask == null)
		{
			int num = (int)(_lastSyncMsec + 50 - GetTicksMsec());
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
		await (mockDelay?.Invoke(delayMsec) ?? Task.Delay(delayMsec));
		SendSyncMessage();
	}

	private async Task SendSyncMessageAfterSmallDelay()
	{
		if (mockWaitSmall == null)
		{
			await Task.Yield();
		}
		else
		{
			await mockWaitSmall();
		}
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
			_lastSyncMsec = GetTicksMsec();
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

	private PeerInputState GetOrCreateStateForPlayer(ulong playerId)
	{
		PeerInputState peerInputState = GetStateForPlayer(playerId);
		if (peerInputState == null)
		{
			peerInputState = new PeerInputState
			{
				playerId = playerId
			};
			_inputStates.Add(peerInputState);
			this.StateAdded?.Invoke(playerId);
		}
		return peerInputState;
	}

	/// <summary>
	/// Override the normalization calculation for the mouse cursor position.
	/// On screens like the Map screen, synchronizing cursor position based on screen position is not correct. This
	/// method allows you to override the translation from net position to screen position and vice versa.
	/// </summary>
	/// <param name="positionTranslator">The object to use when translating cursor positions.</param>
	public void StartOverridingCursorPositioning(INetCursorPositionTranslator positionTranslator)
	{
		_cursorTranslator = positionTranslator;
	}

	/// <summary>
	/// Stop overriding the normalization calculation for the mouse cursor position.
	/// Should be called after StartOverridingCursorPositioning when the screen is exited.
	/// </summary>
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

	/// <summary>
	/// Returns the control-space position of a given player within the given control.
	/// If the player is using a mouse, then this would be the mouse's position
	/// If the player is using the controller, then this would be the position of the control node that that player is
	/// currently focused on (this is currently only set up for the treasure room).
	/// Different resolutions and aspect ratios are all mapped to the same 1920x1080 reference resolution in control-
	/// space. Essentially, the position returned by this method can be used set the position property on a Control with
	/// a cursor image as rootControl's child and have it look approximately the same across all peers.
	/// </summary>
	/// <param name="playerId">Player for which the mouse cursor position should be obtained.</param>
	/// <param name="rootControl">The root control to position the mouse cursor relative to. Should only be null in tests.</param>
	/// <returns></returns>
	public Vector2 GetControlSpaceFocusPosition(ulong playerId, Control? rootControl)
	{
		PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(playerId);
		Vector2 vector = (orCreateStateForPlayer.isUsingController ? orCreateStateForPlayer.controllerFocusPosition : orCreateStateForPlayer.netMousePosition);
		if (_cursorTranslator == null)
		{
			return NetCursorHelper.GetControlSpacePosition(vector, rootControl);
		}
		Vector2 screenPositionFromNetPosition = _cursorTranslator.GetScreenPositionFromNetPosition(vector);
		if (rootControl == null)
		{
			if (TestMode.IsOn)
			{
				return screenPositionFromNetPosition;
			}
			throw new InvalidOperationException("Root node should only be null in tests!");
		}
		return rootControl.GetGlobalTransformWithCanvas() * screenPositionFromNetPosition;
	}

	/// <returns>True if the given player is holding the mouse down, false otherwise.</returns>
	public bool GetMouseDown(ulong playerId)
	{
		return GetOrCreateStateForPlayer(playerId).isMouseDown;
	}

	/// <returns>The top-most overlay or capstone that the given player is looking at.</returns>
	public NetScreenType GetScreenType(ulong playerId)
	{
		return GetOrCreateStateForPlayer(playerId).netScreenType;
	}

	/// <summary> WARNING: You probably want HoveredModelTracker methods instead! </summary>
	/// <returns>Info about what model the player is hovering.</returns>
	public HoveredModelData GetHoveredModelData(ulong playerId)
	{
		return GetOrCreateStateForPlayer(playerId).hoveredModelData;
	}

	/// <returns>True if the player is currently in combat targeting mode, false otherwise.</returns>
	public bool GetIsTargeting(ulong playerId)
	{
		return GetOrCreateStateForPlayer(playerId).isTargeting;
	}

	/// <summary>
	/// Removes the player state from the synchronizer if it exists and notifies listeners.
	/// Note that, if the player is still connected to the session and sends a PeerInputMessage, the state will be
	/// re-created, so this method should be called as late as possible in the disconnection flow.
	/// </summary>
	/// <param name="playerId">The player that left the game.</param>
	public void OnPlayerDisconnected(ulong playerId)
	{
		_logger.Debug($"Disconnected player {playerId}, removing PeerInputState");
		_inputStates.RemoveAll((PeerInputState p) => p.playerId == playerId);
		this.StateRemoved?.Invoke(playerId);
	}

	private ulong GetTicksMsec()
	{
		return mockGetTicksMsec?.Invoke() ?? Time.GetTicksMsec();
	}
}
