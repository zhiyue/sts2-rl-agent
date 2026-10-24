using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Encounters;

public sealed class GremlinMercNormal : EncounterModel
{
	public const string mercSlot = "merc";

	public const string sneakySlot = "sneaky";

	public const string fatSlot = "fat";

	private bool _goldWasStolen;

	public override RoomType RoomType => RoomType.Monster;

	public override bool HasScene => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new global::_003C_003Ez__ReadOnlyArray<MonsterModel>(new MonsterModel[3]
	{
		ModelDb.Monster<GremlinMerc>(),
		ModelDb.Monster<FatGremlin>(),
		ModelDb.Monster<SneakyGremlin>()
	});

	/// <summary>
	/// Whether GremlinMerc stole at least some gold this combat. Set by <see cref="T:MegaCrit.Sts2.Core.Models.Powers.SurprisePower" />
	/// when the merc dies and its stolen gold is transferred to Fat Gremlin.
	/// </summary>
	private bool GoldWasStolen
	{
		get
		{
			return _goldWasStolen;
		}
		set
		{
			AssertMutable();
			_goldWasStolen = value;
		}
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((ModelDb.Monster<GremlinMerc>().ToMutable(), "merc"));
	}

	public void MarkGoldStolen()
	{
		GoldWasStolen = true;
	}

	public override float CalculateGoldProportion(CombatState combatState)
	{
		if (!combatState.EscapedCreatures.Any((Creature c) => c.Monster is FatGremlin))
		{
			return 1f;
		}
		if (!GoldWasStolen)
		{
			return 0.5f;
		}
		return 0f;
	}
}
