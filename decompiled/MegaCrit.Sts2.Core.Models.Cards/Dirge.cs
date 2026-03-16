using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Dirge : CardModel
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new SummonVar(3m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon),
		HoverTipFactory.FromCard<Soul>(base.IsUpgraded)
	});

	public Dirge()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int xValue = ResolveEnergyXValue();
		for (int i = 0; i < xValue; i++)
		{
			await OstyCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
		}
		List<Soul> list = Soul.Create(base.Owner, xValue, base.CombatState).ToList();
		if (base.IsUpgraded)
		{
			foreach (Soul item in list)
			{
				CardCmd.Upgrade(item);
			}
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(1m);
	}
}
