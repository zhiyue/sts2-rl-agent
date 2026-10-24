using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Odds;

/// <summary>
/// Keeps track of the odds for different room types to be rolled when the player visits an unknown map point type.
///
/// Note: If a room type has negative odds, that means it should NEVER be rolled, and its odds shouldn't increase as
/// other types are rolled. This is relevant for effects like the Deadly Enemies modifier.
/// </summary>
public class UnknownMapPointOdds : AbstractOdds
{
	public const float baseMonsterOdds = 0.1f;

	public const float baseEliteOdds = -1f;

	public const float baseTreasureOdds = 0.02f;

	public const float baseShopOdds = 0.03f;

	private readonly Dictionary<RoomType, float> _baseOdds = new Dictionary<RoomType, float>
	{
		[RoomType.Monster] = 0.1f,
		[RoomType.Elite] = -1f,
		[RoomType.Treasure] = 0.02f,
		[RoomType.Shop] = 0.03f
	};

	/// <summary>
	/// Contains the odds of all non-Event rooms being rolled.
	/// If we roll higher than the sum of all these odds, the result will be considered an Event room.
	/// </summary>
	private readonly Dictionary<RoomType, float> _nonEventOdds = new Dictionary<RoomType, float>
	{
		[RoomType.Monster] = 0.1f,
		[RoomType.Elite] = -1f,
		[RoomType.Treasure] = 0.02f,
		[RoomType.Shop] = 0.03f
	};

	public float MonsterOdds
	{
		get
		{
			return _nonEventOdds[RoomType.Monster];
		}
		set
		{
			_nonEventOdds[RoomType.Monster] = value;
		}
	}

	public float EliteOdds
	{
		get
		{
			return _nonEventOdds[RoomType.Elite];
		}
		set
		{
			_nonEventOdds[RoomType.Elite] = value;
		}
	}

	public float TreasureOdds
	{
		get
		{
			return _nonEventOdds[RoomType.Treasure];
		}
		set
		{
			_nonEventOdds[RoomType.Treasure] = value;
		}
	}

	public float ShopOdds
	{
		get
		{
			return _nonEventOdds[RoomType.Shop];
		}
		set
		{
			_nonEventOdds[RoomType.Shop] = value;
		}
	}

	public float EventOdds => Math.Max(0f, 1f - _nonEventOdds.Values.Where((float v) => v > 0f).Sum());

	/// <summary>
	/// For creating at the start of a run.
	/// </summary>
	/// <param name="rng">RNG to use for rolls.</param>
	public UnknownMapPointOdds(Rng rng)
		: base(0f, rng)
	{
	}

	/// <summary>
	/// Sets the base odds for a room type.
	/// Note that this does not set the _current_ odds. It only sets what the odds get reset to when that room is rolled.
	/// </summary>
	public void SetBaseOdds(RoomType roomType, float baseOdds)
	{
		_baseOdds[roomType] = baseOdds;
	}

	/// <summary>
	/// Roll for the next room type and update future odds based on what's rolled.
	/// </summary>
	/// <param name="blacklist">Room types that we shouldn't be able to roll.</param>
	/// <param name="runState">The state of the run that this room is being entered in.</param>
	/// <returns>A RoomType.</returns>
	public RoomType Roll(IEnumerable<RoomType> blacklist, IRunState runState)
	{
		if (runState.UnlockState.NumberOfRuns == 0)
		{
			int num = runState.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> l) => l).Count((MapPointHistoryEntry p) => p.MapPointType == MapPointType.Unknown);
			if (num < 2)
			{
				return RoomType.Event;
			}
			if (num == 2)
			{
				return RoomType.Monster;
			}
		}
		IReadOnlySet<RoomType> roomTypes = _nonEventOdds.Keys.Append(RoomType.Event).Except(blacklist).ToHashSet();
		roomTypes = Hook.ModifyUnknownMapPointRoomTypes(runState, roomTypes);
		RoomType roomType = (roomTypes.Contains(RoomType.Event) ? RoomType.Event : roomTypes.Order().First());
		float num2 = _rng.NextFloat();
		float num3 = 0f;
		RoomType key;
		float value;
		foreach (KeyValuePair<RoomType, float> nonEventOdd in _nonEventOdds)
		{
			nonEventOdd.Deconstruct(out key, out value);
			RoomType roomType2 = key;
			float num4 = value;
			if (roomTypes.Contains(roomType2) && !(num4 < 0f))
			{
				num3 += num4;
				if (num2 <= num3)
				{
					roomType = roomType2;
					break;
				}
			}
		}
		foreach (KeyValuePair<RoomType, float> baseOdd in _baseOdds)
		{
			baseOdd.Deconstruct(out key, out value);
			RoomType roomType3 = key;
			float num5 = value;
			if (roomType == roomType3)
			{
				_nonEventOdds[roomType3] = num5;
			}
			else if (roomTypes.Contains(roomType3))
			{
				float num6 = Hook.ModifyOddsIncreaseForUnrolledRoomType(runState, roomType3, num5);
				Dictionary<RoomType, float> nonEventOdds = _nonEventOdds;
				key = roomType3;
				nonEventOdds[key] += num6;
			}
		}
		return roomType;
	}

	/// <summary>
	/// Reset the odds for all room types to their base level.
	/// Called between acts.
	/// </summary>
	public void ResetToBase()
	{
		foreach (var (key, value) in _baseOdds)
		{
			_nonEventOdds[key] = value;
		}
	}
}
