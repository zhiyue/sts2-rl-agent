using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Eidolon : CardModel
{
	private const int _intangibleThreshold = 9;

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			PlayerCombatState? playerCombatState = base.Owner.PlayerCombatState;
			if (playerCombatState == null)
			{
				return false;
			}
			return playerCombatState.Hand.Cards.Count > 9;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromPower<IntangiblePower>()
	});

	public Eidolon()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		List<CardModel> list = base.Owner.PlayerCombatState.Hand.Cards.ToList();
		int exhaustedCount = 0;
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
			exhaustedCount++;
		}
		if (exhaustedCount >= 9)
		{
			await PowerCmd.Apply<IntangiblePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
