using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class ScrollHelper
{
	private const float _scrollAmount = 40f;

	private const float _panScrollSpeed = 50f;

	public const float dragLerpSpeed = 15f;

	public const float snapThreshold = 0.5f;

	public const float bounceBackStrength = 12f;

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
