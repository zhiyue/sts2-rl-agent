using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Fasten : CardModel
{
	private const string _extraBlockKey = "ExtraBlock";

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			CardModel cardModel = null;
			if (base.IsMutable && base.Owner != null)
			{
				cardModel = base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint).First((CardModel c) => c.Tags.Contains(CardTag.Defend));
			}
			if (cardModel == null)
			{
				cardModel = ModelDb.Card<DefendIronclad>();
			}
			return new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
			{
				HoverTipFactory.Static(StaticHoverTip.Block),
				HoverTipFactory.FromCard(cardModel)
			});
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("ExtraBlock", 5m));

	public Fasten()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<FastenPower>(base.Owner.Creature, base.DynamicVars["ExtraBlock"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["ExtraBlock"].UpgradeValueBy(2m);
	}
}
