using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class ScrollHelper
{
	/// <summary>
	/// How much we travel per scrollWheel tick.
	/// Probably better to get the expected scroll distance from the OS.
	/// </summary>
	private const float _scrollAmount = 40f;

	/// <summary>
	/// How much we travel per touchpad gesture tick.
	/// </summary>
	private const float _panScrollSpeed = 50f;

	/// <summary>
	/// How fast we lerp to the target position when dragging.
	/// </summary>
	public const float dragLerpSpeed = 15f;

	/// <summary>
	/// How close we need to be to the target position before we snap to it.
	/// </summary>
	public const float snapThreshold = 0.5f;

	/// <summary>
	/// How fast we bounce back after scrolling past the edge of the screen.
	/// </summary>
	public const float bounceBackStrength = 12f;

	/// <summary>
	/// Get the amount of scroll drag that should be added for a given scroll event.
	/// </summary>
	public static float GetDragForScrollEvent(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton { ButtonIndex: var buttonIndex })
		{
			switch (buttonIndex)
			{
			case MouseButton.WheelUp:
				return 40f;
			case MouseButton.WheelDown:
				return -40f;
			}
		}
		else if (inputEvent is InputEventPanGesture inputEventPanGesture)
		{
			return (0f - inputEventPanGesture.Delta.Y) * 50f;
		}
		return 0f;
	}
}
