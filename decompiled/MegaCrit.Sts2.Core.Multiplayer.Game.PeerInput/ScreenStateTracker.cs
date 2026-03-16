using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public class ScreenStateTracker
{
	private NetScreenType _capstoneScreen;

	private NetScreenType _overlayScreen;

	private bool _mapScreenVisible;

	private bool _isInSharedRelicPicking;

	private NRewardsScreen? _connectedRewardsScreen;

	public ScreenStateTracker(NMapScreen mapScreen, NCapstoneContainer capstoneContainer, NOverlayStack overlayStack)
	{
		capstoneContainer.Connect(NCapstoneContainer.SignalName.Changed, Callable.From(OnCapstoneScreenChanged));
		overlayStack.Connect(NOverlayStack.SignalName.Changed, Callable.From(OnOverlayStackChanged));
		mapScreen.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnMapScreenVisibilityChanged));
	}

	private void OnCapstoneScreenChanged()
	{
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
		{
			_capstoneScreen = NCapstoneContainer.Instance.CurrentCapstoneScreen?.ScreenType ?? NetScreenType.None;
			SyncLocalScreen();
		}
	}

	private void OnOverlayStackChanged()
	{
		if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
		{
			return;
		}
		IOverlayScreen overlayScreen = NOverlayStack.Instance.Peek();
		if (overlayScreen is NRewardsScreen nRewardsScreen)
		{
			if (_connectedRewardsScreen != nRewardsScreen)
			{
				_connectedRewardsScreen = nRewardsScreen;
				nRewardsScreen.Connect(NRewardsScreen.SignalName.Completed, Callable.From(SyncLocalScreen));
			}
		}
		else
		{
			_connectedRewardsScreen = null;
		}
		_overlayScreen = overlayScreen?.ScreenType ?? NetScreenType.None;
		SyncLocalScreen();
	}

	private void SyncLocalScreen()
	{
		RunManager.Instance.InputSynchronizer.SyncLocalScreen(GetCurrentScreen());
	}

	private void OnMapScreenVisibilityChanged()
	{
		_mapScreenVisible = NMapScreen.Instance.Visible;
		RunManager.Instance.InputSynchronizer.SyncLocalScreen(GetCurrentScreen());
	}

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
