using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Bombardment : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(18m, ValueProp.Move));

	public Bombardment()
		: base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
		if (nCreature != null)
		{
			NLargeMagicMissileVfx nLargeMagicMissileVfx = NLargeMagicMissileVfx.Create(nCreature.GetBottomOfHitbox(), new Color("50b598"));
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nLargeMagicMissileVfx);
			await Cmd.Wait(nLargeMagicMissileVfx.WaitTime);
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		CardPile? pile = base.Pile;
		if (pile != null && pile.Type == PileType.Exhaust && player == base.Owner)
		{
			await CardCmd.AutoPlay(choiceContext, this, null);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(6m);
	}
}
