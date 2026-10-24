namespace MegaCrit.Sts2.Core.Entities.Ancients;

public enum AncientDialogueSpeaker
{
	/// <summary>
	/// No speaker. This should never be set, it just gives us a neutral default.
	/// </summary>
	None,
	/// <summary>
	/// The Ancient (Neow, Darv, etc.) is speaking.
	/// </summary>
	Ancient,
	/// <summary>
	/// The player character (Ironclad, Silent, etc.) is speaking.
	/// </summary>
	Character
}
