using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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

public sealed class DaggerSpray : CardModel
{
	private const string _daggerSpraySfx = "event:/sfx/characters/silent/silent_dagger_spray";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(4m, ValueProp.Move));

	public DaggerSpray()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(base.Owner.Creature, new Color("#b1ccca"), goingRight: true))
			.BeforeDamage(delegate
			{
				IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
				foreach (Creature item in hittableEnemies)
				{
					NDaggerSprayImpactVfx child = NDaggerSprayImpactVfx.Create(item, new Color("#b1ccca"), goingRight: true);
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
				}
				return Task.CompletedTask;
			})
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
