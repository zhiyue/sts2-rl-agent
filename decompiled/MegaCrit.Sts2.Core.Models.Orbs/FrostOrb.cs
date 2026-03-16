using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Orbs;

public class FrostOrb : OrbModel
{
	protected override string ChannelSfx => "event:/sfx/characters/defect/defect_frost_channel";

	public override Color DarkenedColor => new Color("7860a7");

	public override decimal PassiveVal => ModifyOrbValue(2m);

	public override decimal EvokeVal => ModifyOrbValue(5m);

	public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
	{
		await Passive(choiceContext, null);
	}

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target != null)
		{
			throw new InvalidOperationException("Frost orbs cannot target creatures.");
		}
		Trigger();
		PlayPassiveSfx();
		await CreatureCmd.GainBlock(base.Owner.Creature, PassiveVal, ValueProp.Unpowered, null);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		PlayEvokeSfx();
		await CreatureCmd.GainBlock(base.Owner.Creature, EvokeVal, ValueProp.Unpowered, null);
		return new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(base.Owner.Creature);
	}
}
