using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Void : CardModel
{
	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Unplayable,
		CardKeyword.Ethereal
	});

	public Void()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card == this)
		{
			await Cmd.Wait(0.25f);
			await PlayerCmd.LoseEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		}
	}
}
