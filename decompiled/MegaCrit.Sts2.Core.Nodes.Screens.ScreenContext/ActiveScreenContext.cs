using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

/// <summary>
/// Manages the screen contexts that is currently active (the top-most screen that the player is interacting with).
/// This is used to span between all of our different screen systems (overlays, capstones, submenus, even rooms).
/// </summary>
public class ActiveScreenContext
{
	private static ActiveScreenContext? _instance;

	public static ActiveScreenContext Instance => _instance ?? (_instance = new ActiveScreenContext());

	/// <summary>
	/// Event used to let listeners know that the Current Active Screen may have changed
	/// Useful for screens that needs to hide/show certain UX depending on if it is the Current Screen or not.
	/// </summary>
	public event Action? Updated;

	public void Update()
	{
		this.Updated?.Invoke();
	}

	/// <summary>
	/// Looks through all of our screen systems in order of priority to determine what the active screen context currently
	/// is.
	/// </summary>
	/// <returns>The screen context that is currently active.</returns>
	public IScreenContext? GetCurrentScreen()
	{
		if (NGame.Instance?.FeedbackScreen != null && NGame.Instance.FeedbackScreen.Visible)
		{
			return NGame.Instance.FeedbackScreen;
		}
		if (NModalContainer.Instance?.OpenModal != null)
		{
			return NModalContainer.Instance.OpenModal;
		}
		if (NGame.Instance?.InspectCardScreen != null && NGame.Instance.InspectCardScreen.Visible)
		{
			return NGame.Instance.InspectCardScreen;
		}
		if (NGame.Instance?.InspectRelicScreen != null && NGame.Instance.InspectRelicScreen.Visible)
		{
			return NGame.Instance.InspectRelicScreen;
		}
		if (NGame.Instance?.LogoAnimation != null)
		{
			return NGame.Instance.LogoAnimation;
		}
		if (NGame.Instance?.MainMenu != null)
		{
			NMainMenu mainMenu = NGame.Instance.MainMenu;
			if (mainMenu.PatchNotesScreen.IsOpen)
			{
				return mainMenu.PatchNotesScreen;
			}
			NMainMenuSubmenuStack submenuStack = mainMenu.SubmenuStack;
			if (submenuStack.SubmenusOpen)
			{
				if (submenuStack.Peek() is NTimelineScreen { CurrentUnlockScreen: not null } nTimelineScreen)
				{
					return nTimelineScreen.CurrentUnlockScreen;
				}
				return submenuStack.Peek();
			}
			return NGame.Instance.MainMenu;
		}
		if (NRun.Instance != null)
		{
			NRun instance = NRun.Instance;
			if (NCapstoneContainer.Instance.CurrentCapstoneScreen != null)
			{
				return NCapstoneContainer.Instance.CurrentCapstoneScreen;
			}
			if (NMapScreen.Instance.IsOpen)
			{
				return NMapScreen.Instance;
			}
			if (NOverlayStack.Instance.ScreenCount > 0)
			{
				return NOverlayStack.Instance.Peek();
			}
			if (instance.EventRoom != null)
			{
				if (instance.EventRoom.CustomEventNode != null)
				{
					return instance.EventRoom.CustomEventNode.CurrentScreenContext;
				}
				if (instance.EventRoom.Layout is NCombatEventLayout nCombatEventLayout)
				{
					if (!nCombatEventLayout.HasCombatStarted)
					{
						return instance.EventRoom;
					}
					return instance.EventRoom.EmbeddedCombatRoom;
				}
				return instance.EventRoom;
			}
			if (instance.CombatRoom != null)
			{
				return instance.CombatRoom;
			}
			if (instance.TreasureRoom != null)
			{
				return instance.TreasureRoom;
			}
			if (instance.RestSiteRoom != null)
			{
				return instance.RestSiteRoom;
			}
			if (instance.MapRoom != null)
			{
				return instance.MapRoom;
			}
			if (instance.MerchantRoom != null)
			{
				NMerchantRoom merchantRoom = instance.MerchantRoom;
				if (merchantRoom.Inventory.IsOpen)
				{
					return merchantRoom.Inventory;
				}
				return merchantRoom;
			}
		}
		return null;
	}

	public bool IsCurrent(IScreenContext screen)
	{
		return screen == GetCurrentScreen();
	}

	public void FocusOnDefaultControl()
	{
		Control control = GetCurrentScreen()?.DefaultFocusedControl;
		if (control != null)
		{
			control.TryGrabFocus();
		}
		else
		{
			NGame.Instance.GetViewport()?.GuiReleaseFocus();
		}
	}
}
