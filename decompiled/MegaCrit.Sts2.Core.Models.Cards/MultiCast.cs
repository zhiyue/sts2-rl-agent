using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class MultiCast : CardModel
{
	protected override bool HasEnergyCostX => true;

	public override OrbEvokeType OrbEvokeType => OrbEvokeType.All;

	public MultiCast()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int evokeCount = ResolveEnergyXValue();
		if (base.IsUpgraded)
		{
			evokeCount++;
		}
		for (int i = 0; i < evokeCount; i++)
		{
			await OrbCmd.EvokeNext(choiceContext, base.Owner, i == evokeCount - 1);
			await Cmd.Wait(0.25f);
		}
	}
}
