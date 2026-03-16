using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class ThisOrThat : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new HpLossVar(6m),
		new GoldVar(0),
		new StringVar("Curse", ModelDb.Card<Clumsy>().Title)
	});

	public override void CalculateVars()
	{
		base.DynamicVars.Gold.BaseValue = base.Rng.NextInt(41, 69);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Plain, "THIS_OR_THAT.pages.INITIAL.options.PLAIN").ThatDoesDamage(base.DynamicVars.HpLoss.IntValue),
			new EventOption(this, Ornate, "THIS_OR_THAT.pages.INITIAL.options.ORNATE", HoverTipFactory.FromCardWithCardHoverTips<Clumsy>())
		});
	}

	private async Task Plain()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		await PlayerCmd.GainGold(base.DynamicVars.Gold.IntValue, base.Owner);
		SetEventFinished(L10NLookup("THIS_OR_THAT.pages.PLAIN.description"));
	}

	private async Task Ornate()
	{
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		await CardPileCmd.AddCurseToDeck<Clumsy>(base.Owner);
		SetEventFinished(L10NLookup("THIS_OR_THAT.pages.ORNATE.description"));
	}
}
