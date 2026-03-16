using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Debt : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(10));

	public override bool HasTurnEndInHandEffect => true;

	public Debt()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		int num = Mathf.Min(base.DynamicVars.Gold.IntValue, base.Owner.Gold);
		await PlayerCmd.LoseGold(num, base.Owner);
	}
}
