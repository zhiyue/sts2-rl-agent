using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class WasteAway : CardModel, KnowledgeDemon.IChoosable
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<WasteAwayPower>(1m));

	public WasteAway()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<WasteAwayPower>(base.Owner.Creature, base.DynamicVars["WasteAwayPower"].IntValue, base.Owner.Creature, this);
	}
}
