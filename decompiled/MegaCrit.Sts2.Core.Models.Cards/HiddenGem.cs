using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HiddenGem : CardModel
{
	private const string _replayKey = "Replay";

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new IntVar("Replay", 2m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.ReplayStatic));

	public HiddenGem()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		List<CardModel> list = PileType.Draw.GetPile(base.Owner).Cards.ToList();
		if (list.Count == 0)
		{
			return;
		}
		List<CardModel> list2 = list.Where(delegate(CardModel c)
		{
			bool flag = !c.Keywords.Contains(CardKeyword.Unplayable);
			bool flag2 = flag;
			if (flag2)
			{
				CardType type = c.Type;
				bool flag3 = (uint)(type - 5) <= 1u;
				flag2 = !flag3;
			}
			return flag2 && c.GetEnchantedReplayCount() < 1;
		}).ToList();
		List<CardModel> list3 = list2.Where(delegate(CardModel c)
		{
			CardType type = c.Type;
			return (uint)(type - 1) <= 2u;
		}).ToList();
		IEnumerable<CardModel> items = ((list3.Count == 0) ? list2 : list3);
		CardModel cardModel = base.Owner.RunState.Rng.CombatCardSelection.NextItem(items);
		if (cardModel != null)
		{
			cardModel.BaseReplayCount += base.DynamicVars["Replay"].IntValue;
			CardCmd.Preview(cardModel);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Replay"].UpgradeValueBy(1m);
	}
}
