using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class EmberTea : RelicModel
{
	private const string _combatsKey = "Combats";

	private int _combatsLeft = 5;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool IsUsedUp => CombatsLeft <= 0;

	public override bool ShowCounter => true;

	public override int DisplayAmount => Math.Max(0, CombatsLeft);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Combats", CombatsLeft),
		new PowerVar<StrengthPower>(2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	[SavedProperty]
	public int CombatsLeft
	{
		get
		{
			return _combatsLeft;
		}
		set
		{
			AssertMutable();
			_combatsLeft = value;
			base.DynamicVars["Combats"].BaseValue = _combatsLeft;
			InvokeDisplayAmountChanged();
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (!IsUsedUp && room is CombatRoom)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, null, null);
			CombatsLeft--;
		}
	}
}
