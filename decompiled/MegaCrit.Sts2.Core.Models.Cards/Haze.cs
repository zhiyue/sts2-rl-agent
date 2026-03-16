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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Haze : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Sly);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<PoisonPower>(4m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	public Haze()
		: base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		SpawnVfx();
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<PoisonPower>(hittableEnemy, base.DynamicVars.Poison.BaseValue, base.Owner.Creature, this);
		}
	}

	private void SpawnVfx()
	{
		Node node = NCombatRoom.Instance?.CombatVfxContainer;
		if (node == null)
		{
			return;
		}
		NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.8f, 0.3f, 0.66f), new Color(0f, 4f, 0f, 0.33f));
		node.AddChildSafely(child);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			node.AddChildSafely(NSmokePuffVfx.Create(hittableEnemy, NSmokePuffVfx.SmokePuffColor.Green));
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Poison.UpgradeValueBy(2m);
	}
}
