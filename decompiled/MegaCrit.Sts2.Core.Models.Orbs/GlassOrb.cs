using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Orbs;

public class GlassOrb : OrbModel
{
	private decimal _passiveVal = 4m;

	protected override string ChannelSfx => "event:/sfx/characters/defect/defect_glass_channel";

	public override Color DarkenedColor => new Color("008585");

	public override decimal PassiveVal => ModifyOrbValue(_passiveVal);

	public override decimal EvokeVal => PassiveVal * 2m;

	public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
	{
		await Passive(choiceContext, null);
	}

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		List<Creature> targets = base.CombatState.HittableEnemies.Where((Creature e) => e.IsHittable).ToList();
		decimal passiveVal = PassiveVal;
		if (!(passiveVal <= 0m))
		{
			Trigger();
			PlayPassiveSfx();
			_passiveVal = Math.Max(0m, _passiveVal - 1m);
			await CreatureCmd.Damage(choiceContext, targets, passiveVal, ValueProp.Unpowered, base.Owner.Creature);
		}
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		List<Creature> enemies = base.CombatState.HittableEnemies.Where((Creature e) => e.IsHittable).ToList();
		if (EvokeVal <= 0m)
		{
			return Array.Empty<Creature>();
		}
		await CreatureCmd.Damage(playerChoiceContext, enemies, EvokeVal, ValueProp.Unpowered, base.Owner.Creature);
		return enemies;
	}
}
