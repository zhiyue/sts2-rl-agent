using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public interface IBootstrapSettings
{
	CharacterModel Character { get; }

	RoomType RoomType { get; }

	EncounterModel Encounter { get; }

	EventModel Event { get; }

	ActModel Act { get; }

	int Ascension { get; }

	bool SaveRunHistory { get; }

	string? Seed { get; }

	bool DoPreloading { get; }

	bool BootstrapInMultiplayer { get; }

	List<ModifierModel> Modifiers { get; }

	string? Language => null;

	MapPointType MapPointType
	{
		get
		{
			RoomType roomType = RoomType;
			switch (roomType)
			{
			case RoomType.Monster:
				return MapPointType.Monster;
			case RoomType.Elite:
				return MapPointType.Elite;
			case RoomType.Boss:
				return MapPointType.Boss;
			case RoomType.Treasure:
				return MapPointType.Treasure;
			case RoomType.Shop:
				return MapPointType.Shop;
			case RoomType.Event:
				return MapPointType.Unknown;
			case RoomType.RestSite:
				return MapPointType.RestSite;
			case RoomType.Map:
				return MapPointType.Unknown;
			case RoomType.Unassigned:
				throw new ArgumentOutOfRangeException();
			default:
			{
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(roomType);
				MapPointType result = default(MapPointType);
				return result;
			}
			}
		}
	}

	Task Setup(Player localPlayer);
}
