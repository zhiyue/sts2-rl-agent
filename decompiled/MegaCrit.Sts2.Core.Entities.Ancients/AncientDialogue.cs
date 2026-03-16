using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogue
{
	private const string _locTable = "ancients";

	public IReadOnlyList<AncientDialogueLine> Lines { get; }

	public bool IsRepeating { get; set; }

	public int? VisitIndex { get; init; }

	public ArchitectAttackers StartAttackers { get; init; }

	public ArchitectAttackers EndAttackers { get; init; }

	public AncientDialogue(params string[] sfxPaths)
	{
		if (sfxPaths.Length == 0)
		{
			throw new ArgumentException("Requires at least 1 SFX path", "sfxPaths");
		}
		Lines = sfxPaths.Select((string sfx) => new AncientDialogueLine(sfx)).ToList();
	}

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
