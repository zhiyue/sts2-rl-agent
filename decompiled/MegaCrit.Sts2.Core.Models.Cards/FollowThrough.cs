using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FollowThrough : CardModel
{
	protected override bool ShouldGlowGoldInternal => WasLastCardPlayedSkill;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(6m, ValueProp.Move),
		new PowerVar<WeakPower>(1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	private bool WasLastCardPlayedSkill
	{
		get
		{
			CardPlayStartedEntry cardPlayStartedEntry = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault((CardPlayStartedEntry e) => e.CardPlay.Card.Owner == base.Owner && e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card != this);
			if (cardPlayStartedEntry == null)
			{
				return false;
			}
			return cardPlayStartedEntry.CardPlay.Card.Type == CardType.Skill;
		}
	}

	public FollowThrough()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (WasLastCardPlayedSkill)
		{
			await PowerCmd.Apply<WeakPower>(base.CombatState.HittableEnemies, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars.Weak.UpgradeValueBy(1m);
	}
}
