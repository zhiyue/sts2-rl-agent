using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SwordOfStone : RelicModel
{
	private const string _elitesKey = "Elites";

	private int _elitesDefeated;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool ShowCounter => true;

	public override int DisplayAmount => ElitesDefeated;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Elites", 5m));

	[SavedProperty]
	public int ElitesDefeated
	{
		get
		{
			return _elitesDefeated;
		}
		set
		{
			AssertMutable();
			_elitesDefeated = value;
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		if (room.RoomType == RoomType.Elite)
		{
			ElitesDefeated++;
			Flash();
			if ((decimal)ElitesDefeated >= base.DynamicVars["Elites"].BaseValue)
			{
				await RelicCmd.Replace(this, ModelDb.Relic<SwordOfJade>().ToMutable());
			}
		}
	}
}
