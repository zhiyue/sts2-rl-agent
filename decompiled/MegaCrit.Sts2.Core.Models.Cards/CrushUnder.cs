using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class CrushUnder : CardModel
{
	private const string _strengthLossKey = "StrengthLoss";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(7m, ValueProp.Move),
		new DynamicVar("StrengthLoss", 1m)
	});

	public CrushUnder()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.AttackAnimDelay);
		IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
		foreach (Creature item in enemies)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSpikeSplashVfx.Create(item));
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(choiceContext);
		await PowerCmd.Apply<CrushUnderPower>(enemies, base.DynamicVars["StrengthLoss"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		base.DynamicVars["StrengthLoss"].UpgradeValueBy(1m);
	}
}
