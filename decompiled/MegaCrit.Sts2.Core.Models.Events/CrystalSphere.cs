using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class CrystalSphere : EventModel
{
	private const string _uncoverFutureCostKey = "UncoverFutureCost";

	private const string _uncoverFutureProphesizeKey = "UncoverFutureProphesizeCount";

	private const string _paymentPlanKey = "PaymentPlanCount";

	private const int _uncoverFutureCost = 50;

	private const int _uncoverFutureRandomMin = 1;

	private const int _uncoverFutureRandomMax = 50;

	private const int _uncoverFutureProphesizeCount = 3;

	private const int _paymentPlanCount = 6;

	public override bool IsDeterministic => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new DynamicVar("UncoverFutureCost", 50m),
		new DynamicVar("UncoverFutureProphesizeCount", 3m),
		new DynamicVar("PaymentPlanCount", 6m),
		new StringVar("CurseTitle", ModelDb.Card<Debt>().Title)
	});

	public override void CalculateVars()
	{
		base.DynamicVars["UncoverFutureCost"].BaseValue += (decimal)base.Rng.NextInt(1, 50);
	}

	public override bool IsAllowed(RunState runState)
	{
		if (runState.Players.All((Player p) => p.Gold >= 100))
		{
			return runState.CurrentActIndex > 0;
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, UncoverFuture, "CRYSTAL_SPHERE.pages.INITIAL.options.UNCOVER_FUTURE"),
			new EventOption(this, PaymentPlan, "CRYSTAL_SPHERE.pages.INITIAL.options.PAYMENT_PLAN", HoverTipFactory.FromCardWithCardHoverTips<Debt>())
		});
	}

	private async Task UncoverFuture()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["UncoverFutureCost"].BaseValue, base.Owner, GoldLossType.Spent);
		CrystalSphereMinigame crystalSphereMinigame = new CrystalSphereMinigame(base.Owner, base.Rng, 3);
		await crystalSphereMinigame.PlayMinigame();
		SetEventFinished(L10NLookup("CRYSTAL_SPHERE.pages.FINISH.description"));
	}

	private async Task PaymentPlan()
	{
		await CardPileCmd.AddCurseToDeck<Debt>(base.Owner);
		CrystalSphereMinigame crystalSphereMinigame = new CrystalSphereMinigame(base.Owner, base.Rng, 6);
		await crystalSphereMinigame.PlayMinigame();
		SetEventFinished(L10NLookup("CRYSTAL_SPHERE.pages.FINISH.description"));
	}
}
