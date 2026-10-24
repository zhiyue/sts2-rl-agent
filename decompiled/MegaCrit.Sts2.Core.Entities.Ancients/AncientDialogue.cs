using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogue
{
	private const string _locTable = "ancients";

	public IReadOnlyList<AncientDialogueLine> Lines { get; }

	/// <summary>
	/// Should this dialogue be added to the pool of dialogues that can be repeated on future visits?
	/// Can be set manually or auto-populated from loc keys via the "r" suffix (e.g., "1-0r.ancient").
	/// </summary>
	public bool IsRepeating { get; set; }

	/// <summary>
	/// What visit should this dialogue be shown on?
	/// This can be combined with <see cref="P:MegaCrit.Sts2.Core.Entities.Ancients.AncientDialogue.IsRepeating" /> to show a dialogue on the Nth visit, and then add it to the
	/// pool of repeating dialogues for visits after that.
	/// </summary>
	/// <example>
	/// 0: Show this dialogue on this character's first visit to this ancient.
	/// 1: Show this dialogue on this character's second visit to this ancient.
	/// 4: Show this dialogue on this character's fifth visit to this ancient.
	/// null: Show this dialogue on any visit to this ancient.
	/// </example>
	public int? VisitIndex { get; init; }

	/// <summary>
	/// For Architect dialogues, who should attack at the start?
	/// </summary>
	public ArchitectAttackers StartAttackers { get; init; }

	/// <summary>
	/// For Architect dialogues, who should attack at the end?
	/// </summary>
	public ArchitectAttackers EndAttackers { get; init; }

	/// <summary>
	/// Create a new dialogue.
	/// The number of SFX you pass controls how many lines this dialogue has.
	/// </summary>
	/// <param name="sfxPaths">
	/// The paths to the SFX that should be played when this dialogue is shown.
	/// Each SFX corresponds to a line in the dialogue.
	/// Passing an empty string for a given line indicates that no dialogue should be played for it, but you must pass
	/// some string for every line.
	/// For example, if you have a single-line dialogue with no SFX, call `new AncientDialogue("")`.
	/// If you called `new AncientDialogue()`, you'd create a dialogue with 0 lines.
	/// </param>
	public AncientDialogue(params string[] sfxPaths)
	{
		if (sfxPaths.Length == 0)
		{
			throw new ArgumentException("Requires at least 1 SFX path", "sfxPaths");
		}
		Lines = sfxPaths.Select((string sfx) => new AncientDialogueLine(sfx)).ToList();
	}

	/// <summary>
	/// Populate this dialogue's lines with the correct loc keys.
	/// IsRepeating is derived from the presence of the "r" suffix in the first line's loc key.
	/// Speaker is derived from which key suffix exists in the JSON: ".ancient" or ".char".
	/// </summary>
	/// <param name="ancientEntry">The ID entry for the ancient that this line is for.</param>
	/// <param name="charEntry">
	/// The ID entry for the character that this line is for.
	/// Can also be "ANY" for character-agnostic dialogues, or "firstVisitEver" for the dialogue that should be shown the
	/// first time you ever visit this ancient (regardless of character).
	/// </param>
	/// <param name="dialogueIndex">
	/// The index of this dialogue in the list of dialogues for this ancient and character.
	/// </param>
	public void PopulateLines(string ancientEntry, string charEntry, int dialogueIndex)
	{
		string baseKey = $"{ancientEntry}.talk.{charEntry}.{dialogueIndex}-0";
		bool flag = HasRepeatingSuffix(baseKey);
		if (flag)
		{
			IsRepeating = true;
		}
		string text = (flag ? "r" : "");
		for (int i = 0; i < Lines.Count; i++)
		{
			string text2 = $"{ancientEntry}.talk.{charEntry}.{dialogueIndex}-{i}";
			bool flag2 = HasRepeatingSuffix(text2);
			if (flag && !flag2)
			{
				throw new InvalidOperationException($"Dialogue {ancientEntry}.talk.{charEntry}.{dialogueIndex}: line 0 has 'r' suffix but line {i} does not.");
			}
			if (!flag && flag2)
			{
				throw new InvalidOperationException($"Dialogue {ancientEntry}.talk.{charEntry}.{dialogueIndex}: line 0 has no 'r' suffix but line {i} does.");
			}
			string text3 = text2 + text;
			string text4 = text3 + ".ancient";
			string locEntryKey = text3 + ".char";
			if (LocString.Exists("ancients", text4))
			{
				Lines[i].LineText = new LocString("ancients", text4);
				Lines[i].Speaker = AncientDialogueSpeaker.Ancient;
			}
			else
			{
				Lines[i].LineText = new LocString("ancients", locEntryKey);
				Lines[i].Speaker = AncientDialogueSpeaker.Character;
			}
		}
	}

	private static bool HasRepeatingSuffix(string baseKey)
	{
		if (!LocString.Exists("ancients", baseKey + "r.ancient"))
		{
			return LocString.Exists("ancients", baseKey + "r.char");
		}
		return true;
	}
}
