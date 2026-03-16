using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FurCoat : RelicModel
{
	private const string _combatsKey = "Combats";

	private int _furCoatActIndex = -1;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	[SavedProperty]
	public int FurCoatActIndex
	{
		get
		{
			return _furCoatActIndex;
		}
		set
		{
			AssertMutable();
			_furCoatActIndex = value;
		}
	}

	[SavedProperty]
	private int[] FurCoatCoordCols { get; set; } = Array.Empty<int>();

	[SavedProperty]
	private int[] FurCoatCoordRows { get; set; } = Array.Empty<int>();

	[SavedProperty]
	private bool FurCoatCoordsSet { get; set; }

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Combats", 7m));

	public override Task AfterObtained()
	{
		FurCoatActIndex = base.Owner.RunState.CurrentActIndex;
		AddMarkedRooms(base.Owner.RunState.Map);
		return Task.CompletedTask;
	}

	public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		return AddMarkedRooms(map);
	}

	private ActMap AddMarkedRooms(ActMap map)
	{
		if (base.Owner.RunState.CurrentActIndex != FurCoatActIndex)
		{
			return map;
		}
		List<MapCoord> markedCoords = GetMarkedCoords();
		bool flag = markedCoords == null;
		if (!flag)
		{
			flag = !markedCoords.TrueForAll((MapCoord c) => map.HasPoint(c) && (map.GetPoint(c).PointType == MapPointType.Monster || map.GetPoint(c).PointType == MapPointType.Elite));
		}
		if (flag)
		{
			Rng rng = new Rng((uint)((int)base.Owner.RunState.Rng.Seed + (int)base.Owner.NetId + StringHelper.GetDeterministicHashCode("FurCoat")));
			List<MapPoint> list = map.GetAllMapPoints().Where(delegate(MapPoint p)
			{
				MapPointType pointType = p.PointType;
				return (uint)(pointType - 5) <= 1u && !p.Quests.Any((AbstractModel q) => q is FurCoat);
			}).ToList();
			list.UnstableShuffle(rng);
			int intValue = base.DynamicVars["Combats"].IntValue;
			List<MapPoint> list2 = list.Take(intValue).ToList();
			FurCoatCoordCols = new int[list2.Count];
			FurCoatCoordRows = new int[list2.Count];
			for (int num = 0; num < list2.Count; num++)
			{
				FurCoatCoordCols[num] = list2[num].coord.col;
				FurCoatCoordRows[num] = list2[num].coord.row;
			}
			FurCoatCoordsSet = true;
			foreach (MapPoint item in list2)
			{
				item.AddQuest(this);
			}
		}
		else
		{
			foreach (MapCoord item2 in markedCoords)
			{
				MapPoint point = map.GetPoint(item2);
				if (point == null)
				{
					throw new InvalidOperationException($"Loaded a fur coat map with coordinate {item2}, but the generated map does not contain that coordinate!");
				}
				point.AddQuest(this);
			}
		}
		return map;
	}

	public override async Task BeforeCombatStart()
	{
		List<MapCoord> markedCoords = GetMarkedCoords();
		if (markedCoords == null || !markedCoords.Contains(base.Owner.RunState.CurrentMapPoint.coord))
		{
			return;
		}
		Flash();
		IReadOnlyList<Creature> hittableEnemies = base.Owner.Creature.CombatState.HittableEnemies;
		VfxCmd.PlayOnCreatureCenters(hittableEnemies, "vfx/vfx_bite");
		foreach (Creature item in hittableEnemies)
		{
			await CreatureCmd.SetCurrentHp(item, 1m);
		}
	}

	public List<MapCoord>? GetMarkedCoords()
	{
		if (!FurCoatCoordsSet)
		{
			return null;
		}
		List<MapCoord> list = new List<MapCoord>();
		for (int i = 0; i < FurCoatCoordCols.Length; i++)
		{
			list.Add(new MapCoord
			{
				col = FurCoatCoordCols[i],
				row = FurCoatCoordRows[i]
			});
		}
		return list;
	}
}
