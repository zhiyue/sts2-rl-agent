using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Quadcast : CardModel
{
	public override OrbEvokeType OrbEvokeType => OrbEvokeType.Front;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new RepeatVar(4));

	public Quadcast()
		: base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner.PlayerCombatState.OrbQueue.Orbs.Count <= 0)
		{
			return;
		}
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
		{
			await OrbCmd.EvokeNext(choiceContext, base.Owner, i == base.DynamicVars.Repeat.IntValue - 1);
			if (i != base.DynamicVars.Repeat.IntValue - 1)
			{
				await Cmd.CustomScaledWait(0.15f, 0.25f);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
