using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LavaRock : RelicModel
{
	private const string _relicsKey = "Relics";

	private bool _hasTriggered;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool ShowCounter => false;

	[SavedProperty]
	public bool HasTriggered
	{
		get
		{
			return _hasTriggered;
		}
		set
		{
			AssertMutable();
			_hasTriggered = value;
			InvokeDisplayAmountChanged();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Relics", 2m));

	public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (room == null || room.RoomType != RoomType.Boss)
		{
			return false;
		}
		if (base.Owner.RunState.CurrentActIndex != 0)
		{
			return false;
		}
		if (HasTriggered)
		{
			return false;
		}
		Flash();
		for (int i = 0; i < base.DynamicVars["Relics"].IntValue; i++)
		{
			rewards.Add(new RelicReward(player));
		}
		HasTriggered = true;
		base.Status = RelicStatus.Disabled;
		return true;
	}
}
