using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TeaOfDiscourtesy : RelicModel
{
	private const string _combatsKey = "Combats";

	private const string _dazedCountKey = "DazedCount";

	private int _combatsLeft = 1;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool IsUsedUp => CombatsLeft <= 0;

	public override bool ShowCounter => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new HealVar(1m),
		new DynamicVar("Combats", CombatsLeft),
		new DynamicVar("DazedCount", 2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Dazed>());

	[SavedProperty]
	private int CombatsLeft
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

	public override async Task BeforeCombatStart()
	{
		if (CombatsLeft > 0)
		{
			await CardPileCmd.AddToCombatAndPreview<Dazed>(base.Owner.Creature, PileType.Draw, base.DynamicVars["DazedCount"].IntValue, addedByPlayer: true, CardPilePosition.Random);
			CombatsLeft--;
			Flash();
		}
	}
}
