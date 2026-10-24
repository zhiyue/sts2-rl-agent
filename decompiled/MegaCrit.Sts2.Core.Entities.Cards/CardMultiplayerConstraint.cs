namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Manages card restrictions based on how many players there are in a run.
/// </summary>
public enum CardMultiplayerConstraint
{
	None,
	MultiplayerOnly,
	SingleplayerOnly
}
