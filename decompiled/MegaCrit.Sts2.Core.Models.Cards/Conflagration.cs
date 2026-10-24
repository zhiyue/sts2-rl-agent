using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Conflagration : CardModel
{
	protected override IEnumerable<string> ExtraRunAssetPaths => NGroundFireVfx.AssetPaths;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(2m, ValueProp.Move),
		new RepeatVar(4)
	});

	public Conflagration()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
		foreach (Creature item in hittableEnemies)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(item));
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(base.DynamicVars.Repeat.IntValue).FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Repeat.UpgradeValueBy(1m);
	}
}
