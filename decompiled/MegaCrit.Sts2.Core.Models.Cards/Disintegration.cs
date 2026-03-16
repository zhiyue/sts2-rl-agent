using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Disintegration : CardModel, KnowledgeDemon.IChoosable
{
	public override int MaxUpgradeLevel => 0;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<DisintegrationPower>(6m));

	public Disintegration()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<DisintegrationPower>(base.Owner.Creature, base.DynamicVars["DisintegrationPower"].BaseValue, base.Owner.Creature, this);
	}
}
