namespace MegaCrit.Sts2.Core.Events;

public enum EventLayoutType
{
	/// <summary>
	/// The standard event layout type.
	/// At the time I wrote this (10/1/2024), this layout aligns text and buttons to the right side of the screen.
	/// It shows full-screen static art in the background, with the focal point on the left side of the screen.
	/// </summary>
	Default,
	/// <summary>
	/// An event layout type that looks like a combat, with the players on one side and monsters on the other.
	/// No other elements of combat (cards, health bars, etc.) appear in these events, they just look like combat.
	/// </summary>
	Combat,
	/// <summary>
	/// The event layout type used for the Ancients at the beginning of acts.
	/// At the time I wrote this (10/1/2024), this layout aligns text and buttons to the bottom-middle of the screen.
	/// It shows a Spine animation in the background, with the focal point on the tip-middle of the screen.
	/// </summary>
	Ancient,
	/// <summary>
	/// A totally custom scene.
	/// We use this when we want to show events that don't follow any standard layout.
	/// </summary>
	Custom
}
