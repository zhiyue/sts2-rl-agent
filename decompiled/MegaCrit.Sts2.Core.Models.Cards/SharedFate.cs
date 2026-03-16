using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SharedFate : CardModel
{
	private const string _enemyStrengthLossKey = "EnemyStrengthLoss";

	private const string _playerStrengthLossKey = "PlayerStrengthLoss";

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("EnemyStrengthLoss", 2m),
		new DynamicVar("PlayerStrengthLoss", 2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public SharedFate()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, -base.DynamicVars["PlayerStrengthLoss"].BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<StrengthPower>(cardPlay.Target, -base.DynamicVars["EnemyStrengthLoss"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["EnemyStrengthLoss"].UpgradeValueBy(1m);
	}
}
