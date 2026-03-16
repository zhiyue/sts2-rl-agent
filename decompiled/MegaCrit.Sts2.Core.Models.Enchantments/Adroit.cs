using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Adroit : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShowAmount => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(0m, ValueProp.Move));

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		await CreatureCmd.GainBlock(base.Card.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	public override void RecalculateValues()
	{
		base.DynamicVars.Block.BaseValue = base.Amount;
	}
}
