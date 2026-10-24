namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// The status of which an SerializableEpoch can be in.
/// The states are:
///     Revealed: Complete!
///     Obtained: The player has met the requirements to get this AND the slot is available in the Timeline.
///     ObtainedNoSlot: The player has met the requirements to get this but the slot isn't visible.
///     Unobtained: The player has unlocked the slot but has not yet obtained this Epoch.
/// </summary>
public enum EpochState
{
	None,
	NoSlot,
	NotObtained,
	ObtainedNoSlot,
	Obtained,
	Revealed
}
