using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Shame : CardModel
{
	private const string _frailKey = "Frail";

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<FrailPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Frail", 1m));

	public override bool HasTurnEndInHandEffect => true;

	public Shame()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		bool alreadyHasFrail = base.Owner.Creature.HasPower<FrailPower>();
		PowerModel powerModel = await PowerCmd.Apply<FrailPower>(base.Owner.Creature, base.DynamicVars["Frail"].BaseValue, null, this);
		if (powerModel != null && !alreadyHasFrail)
		{
			powerModel.SkipNextDurationTick = true;
		}
	}
}
