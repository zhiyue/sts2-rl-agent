using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Doubt : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<WeakPower>(1m));

	public override bool HasTurnEndInHandEffect => true;

	public Doubt()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		bool alreadyHasWeak = base.Owner.Creature.HasPower<WeakPower>();
		PowerModel powerModel = await PowerCmd.Apply<WeakPower>(base.Owner.Creature, base.DynamicVars.Weak.BaseValue, null, this);
		if (powerModel != null && !alreadyHasWeak)
		{
			powerModel.SkipNextDurationTick = true;
		}
	}
}
