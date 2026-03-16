using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Sloth : CardModel, KnowledgeDemon.IChoosable
{
	public override int MaxUpgradeLevel => 0;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<SlothPower>(3m));

	public Sloth()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<SlothPower>(base.Owner.Creature, base.DynamicVars["SlothPower"].IntValue, base.Owner.Creature, this);
	}
}
