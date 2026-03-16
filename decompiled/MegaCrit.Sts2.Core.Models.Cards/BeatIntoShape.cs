using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BeatIntoShape : CardModel
{
	private const string _calculatedForgeKey = "CalculatedForge";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new DamageVar(5m, ValueProp.Move),
		new CalculationBaseVar(5m),
		new CalculationExtraVar(5m),
		new CalculatedVar("CalculatedForge").WithMultiplier((CardModel card, Creature? target) => CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Count((DamageReceivedEntry e) => e.Receiver == target && e.Dealer == card.Owner.Creature && e.Result.Props.IsPoweredAttack() && e.HappenedThisTurn(card.CombatState)))
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();

	public BeatIntoShape()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
		decimal amount = ((CalculatedVar)base.DynamicVars["CalculatedForge"]).Calculate(cardPlay.Target);
		amount -= (decimal)attackCommand.Results.Count() * base.DynamicVars.CalculationExtra.BaseValue;
		await ForgeCmd.Forge(amount, base.Owner, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars.CalculationBase.UpgradeValueBy(2m);
		base.DynamicVars.CalculationExtra.UpgradeValueBy(2m);
	}
}
