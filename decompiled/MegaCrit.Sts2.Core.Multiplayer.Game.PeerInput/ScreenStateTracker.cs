using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

/// <summary>
/// Keeps track of the local player's screen state and synchronizes it to remote peers via the PeerInputSynchronizer.
/// </summary>
public class ScreenStateTracker
{
	private NetScreenType _capstoneScreen;

	private NetScreenType _overlayScreen;

	private bool _mapScreenVisible;

	private bool _isInSharedRelicPicking;

	/// <summary>
	/// Callable.From() creates a new delegate each call, so we store it to ensure IsConnected() can match the same
	/// instance used to Connect(). This prevents Godot's "signal already connected" error when the rewards screen
	/// re-emerges as the top overlay after another overlay is pushed on top and later popped.
	/// </summary>
	private readonly Callable _onRewardsScreenCompleted;

	public ScreenStateTracker(NMapScreen mapScreen, NCapstoneContainer capstoneContainer, NOverlayStack overlayStack)
	{
		_onRewardsScreenCompleted = Callable.From(SyncLocalScreen);
		capstoneContainer.Connect(NCapstoneContainer.SignalName.Changed, Callable.From(OnCapstoneScreenChanged));
		overlayStack.Connect(NOverlayStack.SignalName.Changed, Callable.From(OnOverlayStackChanged));
		mapScreen.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnMapScreenVisibilityChanged));
	}

	private void OnCapstoneScreenChanged()
	{
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			_capstoneScreen = NCapstoneContainer.Instance.CurrentCapstoneScreen?.ScreenType ?? NetScreenType.None;
			SyncLocalScreen();
		}
	}

	private void OnOverlayStackChanged()
	{
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			IOverlayScreen overlayScreen = NOverlayStack.Instance.Peek();
			if (overlayScreen is NRewardsScreen nRewardsScreen && !nRewardsScreen.IsConnected(NRewardsScreen.SignalName.Completed, _onRewardsScreenCompleted))
			{
				nRewardsScreen.Connect(NRewardsScreen.SignalName.Completed, _onRewardsScreenCompleted);
			}
			_overlayScreen = overlayScreen?.ScreenType ?? NetScreenType.None;
			SyncLocalScreen();
		}
	}

	private void SyncLocalScreen()
	{
		RunManager.Instance.InputSynchronizer.SyncLocalScreen(GetCurrentScreen());
	}

	/// <summary>
	/// The map is not an overlay or a capstone, it's part of <see cref="T:MegaCrit.Sts2.Core.Nodes.NRun" />.
	/// </summary>
	private void OnMapScreenVisibilityChanged()
	{
		_mapScreenVisible = NMapScreen.Instance.Visible;
		RunManager.Instance.InputSynchronizer.SyncLocalScreen(GetCurrentScreen());
	}

	/// <summary>
	/// The shared relic picking screen at the treasure room is not an overlay or a capstone, it's part of the state of
	/// the room, so it needs to get synchronized manually.
	/// </summary>
	public void SetIsInSharedRelicPickingScreen(bool isInSharedRelicPicking)
	{
		_isInSharedRelicPicking = isInSharedRelicPicking;
		RunManager.Instance.InputSynchronizer.SyncLocalScreen(GetCurrentScreen());
	}

	private NetScreenType GetCurrentScreen()
	{
		if (_capstoneScreen != NetScreenType.None)
		{
			return _capstoneScreen;
		}
		if (_mapScreenVisible)
		{
			return NetScreenType.Map;
		}
		if (_overlayScreen == NetScreenType.Rewards)
		{
			if (NOverlayStack.Instance.Peek() is NRewardsScreen { IsComplete: false })
			{
				return _overlayScreen;
			}
		}
		else if (_overlayScreen != NetScreenType.None)
		{
			return _overlayScreen;
		}
		if (_isInSharedRelicPicking)
		{
			return NetScreenType.SharedRelicPicking;
		}
		return NetScreenType.Room;
	}
}
