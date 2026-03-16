using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Orbs;

public class DarkOrb : OrbModel
{
	private decimal _evokeVal = 6m;

	protected override string ChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";

	public override Color DarkenedColor => new Color("9001d3");

	public override decimal PassiveVal => ModifyOrbValue(6m);

	public override decimal EvokeVal => _evokeVal;

	public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
	{
		await Passive(choiceContext, null);
	}

	public override Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target != null)
		{
			throw new InvalidOperationException("Dark orbs cannot target creatures.");
		}
		Trigger();
		_evokeVal += PassiveVal;
		NCombatRoom.Instance?.GetCreatureNode(base.Owner.Creature)?.OrbManager?.UpdateVisuals(OrbEvokeType.None);
		return Task.CompletedTask;
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
		if (hittableEnemies.Count == 0)
		{
			return Array.Empty<Creature>();
		}
		PlayEvokeSfx();
		Creature weakestEnemy = hittableEnemies.MinBy((Creature c) => c.CurrentHp);
		await CreatureCmd.Damage(playerChoiceContext, weakestEnemy, EvokeVal, ValueProp.Unpowered, base.Owner.Creature);
		return new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(weakestEnemy);
	}
}
