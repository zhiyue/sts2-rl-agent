using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogueSet
{
	public required AncientDialogue? FirstVisitEverDialogue { get; init; }

	public required Dictionary<string, IReadOnlyList<AncientDialogue>> CharacterDialogues { get; init; }

	public required IReadOnlyList<AncientDialogue> AgnosticDialogues { get; init; } = Array.Empty<AncientDialogue>();

	public IEnumerable<AncientDialogue> GetAllDialogues()
	{
		if (FirstVisitEverDialogue != null)
		{
			yield return FirstVisitEverDialogue;
		}
		foreach (IReadOnlyList<AncientDialogue> value in CharacterDialogues.Values)
		{
			foreach (AncientDialogue item in value)
			{
				yield return item;
			}
		}
		foreach (AncientDialogue agnosticDialogue in AgnosticDialogues)
		{
			yield return agnosticDialogue;
		}
	}

	public IEnumerable<AncientDialogue> GetValidDialogues(ModelId characterId, int charVisits, int totalVisits, bool allowAnyCharacterDialogues)
	{
		if (totalVisits == 0 && FirstVisitEverDialogue != null)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<AncientDialogue>(FirstVisitEverDialogue);
		}
		IReadOnlyList<AncientDialogue> readOnlyList = null;
		if (CharacterDialogues.TryGetValue(characterId.Entry, out IReadOnlyList<AncientDialogue> value))
		{
			readOnlyList = value;
			List<AncientDialogue> list = readOnlyList.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list.Count > 0)
			{
				return list;
			}
		}
		if (allowAnyCharacterDialogues)
		{
			List<AncientDialogue> list2 = AgnosticDialogues.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list2.Count > 0)
			{
				return list2;
			}
		}
		List<AncientDialogue> list3 = new List<AncientDialogue>();
		if (readOnlyList != null)
		{
			AddRepeatingDialogues(readOnlyList, list3, charVisits);
		}
		if (allowAnyCharacterDialogues)
		{
			AddRepeatingDialogues(AgnosticDialogues, list3, charVisits);
		}
		return list3;
	}

	public void PopulateLocKeys(string ancientEntry)
	{
		FirstVisitEverDialogue?.PopulateLines(ancientEntry, "firstVisitEver", 0);
		foreach (KeyValuePair<string, IReadOnlyList<AncientDialogue>> characterDialogue in CharacterDialogues)
		{
			characterDialogue.Deconstruct(out var key, out var value);
			string charEntry = key;
			IReadOnlyList<AncientDialogue> readOnlyList = value;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				readOnlyList[i].PopulateLines(ancientEntry, charEntry, i);
			}
		}
		for (int j = 0; j < AgnosticDialogues.Count; j++)
		{
			AgnosticDialogues[j].PopulateLines(ancientEntry, "ANY", j);
		}
		foreach (AncientDialogue allDialogue in GetAllDialogues())
		{
			for (int k = 0; k < allDialogue.Lines.Count - 1; k++)
			{
				AncientDialogueLine ancientDialogueLine = allDialogue.Lines[k];
				string locEntryKey = ancientDialogueLine.LineText.LocEntryKey;
				string text = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
				string locEntryKey2 = text + ".next";
				ancientDialogueLine.NextButtonText = new LocString("ancients", locEntryKey2);
			}
		}
	}

	private static void AddRepeatingDialogues(IEnumerable<AncientDialogue> source, List<AncientDialogue> destination, int charVisits)
	{
		foreach (AncientDialogue item in source)
		{
			if (item.IsRepeating && (!item.VisitIndex.HasValue || !(charVisits < item.VisitIndex)))
			{
				destination.Add(item);
			}
		}
	}
}
