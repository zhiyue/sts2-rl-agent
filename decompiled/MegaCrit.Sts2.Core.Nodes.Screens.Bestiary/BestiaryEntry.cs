using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

public class BestiaryEntry
{
	public MonsterModel? monsterModel;

	public required EncounterModel encounterModel;

	public required RoomType roomType;

	public static BestiaryEntry FromMonster(MonsterModel monster, EncounterModel encounter, RoomType type)
	{
		return new BestiaryEntry
		{
			monsterModel = monster,
			encounterModel = encounter,
			roomType = type
		};
	}

	public static BestiaryEntry FromEncounter(EncounterModel encounter, RoomType type)
	{
		return new BestiaryEntry
		{
			encounterModel = encounter,
			roomType = type
		};
	}

	public string GetEncounterTitle()
	{
		return encounterModel.Title.GetFormattedText();
	}

	public string GetEntryTitle()
	{
		if (monsterModel != null)
		{
			return monsterModel.Title.GetFormattedText();
		}
		return GetEncounterTitle();
	}

	public bool CanReuseLayout(NBestiaryLayout? layout)
	{
		if (encounterModel is KaiserCrabBoss)
		{
			return layout is NBestiaryLayoutKaiserCrab;
		}
		if (encounterModel is DecimillipedeElite)
		{
			return layout is NBestiaryLayoutDecimillipede;
		}
		return layout is NBestiaryLayoutDefault;
	}

	public NBestiaryLayout? CreateLayoutNode(NBestiary bestiary)
	{
		if (encounterModel is KaiserCrabBoss)
		{
			return NBestiaryLayoutKaiserCrab.Create();
		}
		if (encounterModel is DecimillipedeElite)
		{
			return NBestiaryLayoutDecimillipede.Create(bestiary);
		}
		return NBestiaryLayoutDefault.Create();
	}

	public bool IsDiscovered(HashSet<ModelId> discoveredMonsterIds, HashSet<ModelId> discoveredEncounterIds)
	{
		if (monsterModel == null)
		{
			return discoveredEncounterIds.Contains(encounterModel.Id);
		}
		return discoveredMonsterIds.Contains(monsterModel.Id);
	}
}
