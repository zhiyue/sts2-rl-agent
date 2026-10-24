namespace MegaCrit.Sts2.Core.Entities.Ancients;

/// <summary>
/// Who should attack during The Architect dialogue?
/// </summary>
public enum ArchitectAttackers
{
	/// <summary>
	/// No attacks should occur during the specified portion of the dialogue.
	/// </summary>
	None,
	/// <summary>
	/// The Player attacks.
	/// </summary>
	Player,
	/// <summary>
	/// The Architect attacks.
	/// </summary>
	Architect,
	/// <summary>
	/// The player attacks, then the Architect immediately attacks back.
	/// </summary>
	Both
}
