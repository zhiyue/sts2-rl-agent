using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class GuidingStar : CardModel
{
	private const string _guidingStarSfx = "event:/sfx/characters/regent/regent_guiding_star";

	public override int CanonicalStarCost => 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(12m, ValueProp.Move),
		new CardsVar(2)
	});

	public GuidingStar()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
		if (nCreature != null)
		{
			SfxCmd.Play("event:/sfx/characters/regent/regent_guiding_star");
			NSmallMagicMissileVfx nSmallMagicMissileVfx = NSmallMagicMissileVfx.Create(nCreature.GetBottomOfHitbox(), new Color("50b598"));
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nSmallMagicMissileVfx);
			await Cmd.Wait(nSmallMagicMissileVfx.WaitTime);
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithNoAttackerAnim().FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
