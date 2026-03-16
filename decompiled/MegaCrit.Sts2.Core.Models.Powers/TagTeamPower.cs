using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TagTeamPower : PowerModel
{
	private const string _applierTag = "Applier";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	public override int DisplayAmount => 1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("Applier"));

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		((StringVar)base.DynamicVars["Applier"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, base.Applier.Player.NetId);
		return Task.CompletedTask;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Type != CardType.Attack)
		{
			return playCount;
		}
		if (card.Owner.Creature == base.Applier)
		{
			return playCount;
		}
		if (target != base.Owner)
		{
			return playCount;
		}
		return playCount + base.Amount;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		await PowerCmd.Remove(this);
	}
}
