namespace MegaCrit.Sts2.Core.Entities.Characters;

/// <summary>
/// Represents a character's gender for grammatical purposes.
/// These do not necessarily align with the character's gender identity.
/// For example, in French, a nonbinary character may want to use any of these options, but there is no grammatical
/// gender specifically for "nonbinary" in French.
/// </summary>
public enum CharacterGender
{
	Neutral,
	Feminine,
	Masculine
}
