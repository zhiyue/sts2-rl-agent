using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class BattlewornDummyEventEncounter : EncounterModel
{
	public enum DummySetting
	{
		None,
		Setting1,
		Setting2,
		Setting3
	}

	private const string _settingKey = "Setting";

	private const string _ranOutOfTimeKey = "RanOutOfTime";

	private DummySetting _setting;

	private bool _ranOutOfTime;

	public override RoomType RoomType => RoomType.Monster;

	public override bool ShouldGiveRewards => false;

	public DummySetting Setting
	{
		get
		{
			return _setting;
		}
		set
		{
			AssertMutable();
			_setting = value;
		}
	}

	public bool RanOutOfTime
	{
		get
		{
			return _ranOutOfTime;
		}
		set
		{
			AssertMutable();
			_ranOutOfTime = value;
		}
	}

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<BattleFriendV1>(),
		ModelDb.Monster<BattleFriendV2>(),
		ModelDb.Monster<BattleFriendV3>()
	});

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>(((Setting switch
		{
			DummySetting.Setting1 => ModelDb.Monster<BattleFriendV1>(), 
			DummySetting.Setting2 => ModelDb.Monster<BattleFriendV2>(), 
			DummySetting.Setting3 => ModelDb.Monster<BattleFriendV3>(), 
			_ => throw new InvalidOperationException("Setting must be set!"), 
		}).ToMutable(), null));
	}

	public override Dictionary<string, string> SaveCustomState()
	{
		return new Dictionary<string, string>
		{
			["Setting"] = Setting.ToString(),
			["RanOutOfTime"] = RanOutOfTime.ToString()
		};
	}

	public override void LoadCustomState(Dictionary<string, string> state)
	{
		Setting = Enum.Parse<DummySetting>(state["Setting"]);
		RanOutOfTime = bool.Parse(state["RanOutOfTime"]);
	}
}
