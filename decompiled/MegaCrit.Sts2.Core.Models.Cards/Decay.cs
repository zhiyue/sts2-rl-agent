using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Decay : CardModel
{
	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(2m, ValueProp.Unpowered | ValueProp.Move));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public override bool HasTurnEndInHandEffect => true;

	public Decay()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.Damage, this);
	}
}
