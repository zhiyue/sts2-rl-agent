using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class UpMySleeve : CardModel
{
	private int _timesPlayedThisCombat;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	private int TimesPlayedThisCombat
	{
		get
		{
			return _timesPlayedThisCombat;
		}
		set
		{
			AssertMutable();
			_timesPlayedThisCombat = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Shiv>());

	public UpMySleeve()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
		{
			await Shiv.CreateInHand(base.Owner, base.CombatState);
			await Cmd.Wait(0.1f);
		}
		TimesPlayedThisCombat++;
		base.EnergyCost.AddThisCombat(-1);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
