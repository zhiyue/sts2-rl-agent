using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BouncingFlask : CardModel
{
	private readonly Color _vfxTint = new Color("83eb85");

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<PoisonPower>(3m),
		new RepeatVar(3)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	public BouncingFlask()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		Vector2 lastPos = Vector2.Zero;
		for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
		{
			Creature enemy = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
			if (enemy == null)
			{
				continue;
			}
			if (TestMode.IsOff)
			{
				if (i == 0)
				{
					lastPos = NCombatRoom.Instance.GetCreatureNode(base.Owner.Creature).VfxSpawnPosition;
				}
				NCreature targetNode = NCombatRoom.Instance.GetCreatureNode(enemy);
				if (targetNode != null)
				{
					NItemThrowVfx child = NItemThrowVfx.Create(lastPos, targetNode.GetBottomOfHitbox(), ModelDb.Potion<PoisonPotion>().Image);
					NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
					lastPos = targetNode.VfxSpawnPosition;
					await Cmd.Wait(0.5f);
					NSplashVfx child2 = NSplashVfx.Create(targetNode.VfxSpawnPosition, _vfxTint);
					NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child2);
					NLiquidOverlayVfx child3 = NLiquidOverlayVfx.Create(enemy, _vfxTint);
					NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child3);
					NGaseousImpactVfx child4 = NGaseousImpactVfx.Create(targetNode.VfxSpawnPosition, _vfxTint);
					NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child4);
				}
			}
			await PowerCmd.Apply<PoisonPower>(enemy, base.DynamicVars.Poison.BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Repeat.UpgradeValueBy(1m);
	}
}
