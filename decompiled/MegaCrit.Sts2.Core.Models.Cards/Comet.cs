using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Comet : CardModel
{
	public override int CanonicalStarCost => 5;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(33m, ValueProp.Move),
		new PowerVar<VulnerablePower>(3m),
		new PowerVar<WeakPower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	});

	public Comet()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
		if (nCreature != null)
		{
			NSmallMagicMissileVfx nSmallMagicMissileVfx = NSmallMagicMissileVfx.Create(nCreature.GetBottomOfHitbox(), new Color("50b598"));
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nSmallMagicMissileVfx);
			await Cmd.Wait(nSmallMagicMissileVfx.WaitTime);
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithNoAttackerAnim()
			.Execute(choiceContext);
		await PowerCmd.Apply<WeakPower>(cardPlay.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(11m);
	}
}
