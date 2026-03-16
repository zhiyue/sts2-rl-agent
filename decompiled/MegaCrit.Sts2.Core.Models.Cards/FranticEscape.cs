using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FranticEscape : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			SandpitPower sandpitPower = GetSandpitEnemy()?.GetPower<SandpitPower>();
			if (sandpitPower == null)
			{
				return new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<SandpitPower>());
			}
			return sandpitPower.HoverTips;
		}
	}

	public override int MaxUpgradeLevel => 0;

	public override bool CanBeGeneratedInCombat => false;

	public FranticEscape()
		: base(1, CardType.Status, CardRarity.Status, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature sandpitEnemy = GetSandpitEnemy();
		if (sandpitEnemy != null)
		{
			SandpitPower sandpitPower = sandpitEnemy.Powers.OfType<SandpitPower>().FirstOrDefault((SandpitPower s) => s.Target == base.Owner.Creature);
			if (sandpitPower != null)
			{
				await PowerCmd.ModifyAmount(sandpitPower, 1m, sandpitEnemy, this);
			}
		}
		base.EnergyCost.AddThisCombat(1);
	}

	private Creature? GetSandpitEnemy()
	{
		return base.CombatState?.Enemies.FirstOrDefault((Creature c) => c.HasPower<SandpitPower>());
	}
}
