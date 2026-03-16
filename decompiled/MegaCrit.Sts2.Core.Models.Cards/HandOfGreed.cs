using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HandOfGreed : CardModel
{
	public const int goldAmount = 20;

	private const string _goldKey = "Gold";

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(20m, ValueProp.Move),
		new DynamicVar("Gold", 20m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Fatal));

	public HandOfGreed()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
		base.DynamicVars["Gold"].UpgradeValueBy(5m);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
		Vector2? monsterPos = null;
		if (TestMode.IsOff)
		{
			monsterPos = NCombatRoom.Instance.GetCreatureNode(cardPlay.Target)?.VfxSpawnPosition;
		}
		AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
		if (shouldTriggerFatal && attackCommand.Results.Any((DamageResult r) => r.WasTargetKilled))
		{
			if (monsterPos.HasValue)
			{
				VfxCmd.PlayVfx(monsterPos.Value, "vfx/vfx_coin_explosion_regular");
			}
			await PlayerCmd.GainGold(base.DynamicVars["Gold"].IntValue, base.Owner);
		}
	}
}
