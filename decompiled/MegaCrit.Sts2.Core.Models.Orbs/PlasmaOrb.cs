using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Orbs;

public class PlasmaOrb : OrbModel
{
	protected override string ChannelSfx => "event:/sfx/characters/defect/defect_plasma_channel";

	public override Color DarkenedColor => new Color("008585");

	public override decimal PassiveVal => 1m;

	public override decimal EvokeVal => 2m;

	public override async Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
	{
		await Passive(choiceContext, null);
	}

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target != null)
		{
			throw new InvalidOperationException("Plasma orbs cannot target creatures.");
		}
		Trigger();
		await PlayerCmd.GainEnergy(PassiveVal, base.Owner);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
	{
		PlayEvokeSfx();
		await PlayerCmd.GainEnergy(EvokeVal, base.Owner);
		return new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(base.Owner.Creature);
	}
}
