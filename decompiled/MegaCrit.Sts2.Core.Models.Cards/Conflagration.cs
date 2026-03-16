using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CalculationBaseVar(8m),
		new ExtraDamageVar(2m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => CombatManager.Instance.History.CardPlaysFinished.Count(delegate(CardPlayFinishedEntry e)
		{
			if (!e.HappenedThisTurn(card.CombatState))
			{
				return false;
			}
			if (e.CardPlay.Card.Type != CardType.Attack)
			{
				return false;
			}
			return (e.CardPlay.Card.Owner == card.Owner) ? true : false;
		}))
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
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationBase.UpgradeValueBy(1m);
		base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
	}
}
