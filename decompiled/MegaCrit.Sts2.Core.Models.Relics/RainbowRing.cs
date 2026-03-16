using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RainbowRing : RelicModel
{
	private int _attacksPlayedThisTurn;

	private int _skillsPlayedThisTurn;

	private int _powersPlayedThisTurn;

	private int _activationCountThisTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<StrengthPower>(1m),
		new PowerVar<DexterityPower>(1m)
	});

	private int AttacksPlayedThisTurn
	{
		get
		{
			return _attacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_attacksPlayedThisTurn = value;
		}
	}

	private int SkillsPlayedThisTurn
	{
		get
		{
			return _skillsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_skillsPlayedThisTurn = value;
		}
	}

	private int PowersPlayedThisTurn
	{
		get
		{
			return _powersPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_powersPlayedThisTurn = value;
		}
	}

	private int ActivationCountThisTurn
	{
		get
		{
			return _activationCountThisTurn;
		}
		set
		{
			AssertMutable();
			_activationCountThisTurn = value;
			base.Status = ((_activationCountThisTurn > 0) ? RelicStatus.Active : RelicStatus.Normal);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		AttacksPlayedThisTurn = 0;
		SkillsPlayedThisTurn = 0;
		PowersPlayedThisTurn = 0;
		ActivationCountThisTurn = 0;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && CombatManager.Instance.IsInProgress && ActivationCountThisTurn < 1)
		{
			AttacksPlayedThisTurn += ((cardPlay.Card.Type == CardType.Attack) ? 1 : 0);
			SkillsPlayedThisTurn += ((cardPlay.Card.Type == CardType.Skill) ? 1 : 0);
			PowersPlayedThisTurn += ((cardPlay.Card.Type == CardType.Power) ? 1 : 0);
			if (AttacksPlayedThisTurn > 0 && SkillsPlayedThisTurn > 0 && PowersPlayedThisTurn > 0)
			{
				Flash();
				await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, null);
				await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, null);
				ActivationCountThisTurn++;
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		AttacksPlayedThisTurn = 0;
		SkillsPlayedThisTurn = 0;
		PowersPlayedThisTurn = 0;
		ActivationCountThisTurn = 0;
		return Task.CompletedTask;
	}
}
