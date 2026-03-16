using System.Collections.Generic;
using System.Linq;
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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Hyperbeam : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(26m, ValueProp.Move),
		new PowerVar<FocusPower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<FocusPower>());

	public Hyperbeam()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithAttackerAnim("Cast", 0.5f)
			.BeforeDamage(async delegate
			{
				List<Creature> enemies = base.CombatState.Enemies.Where((Creature e) => e.IsAlive).ToList();
				NHyperbeamVfx nHyperbeamVfx = NHyperbeamVfx.Create(base.Owner.Creature, enemies.Last());
				if (nHyperbeamVfx != null)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamVfx);
					await Cmd.Wait(0.5f);
				}
				foreach (Creature item in enemies)
				{
					NHyperbeamImpactVfx nHyperbeamImpactVfx = NHyperbeamImpactVfx.Create(base.Owner.Creature, item);
					if (nHyperbeamImpactVfx != null)
					{
						NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamImpactVfx);
					}
				}
			})
			.Execute(choiceContext);
		await PowerCmd.Apply<FocusPower>(base.Owner.Creature, -base.DynamicVars["FocusPower"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(8m);
	}
}
