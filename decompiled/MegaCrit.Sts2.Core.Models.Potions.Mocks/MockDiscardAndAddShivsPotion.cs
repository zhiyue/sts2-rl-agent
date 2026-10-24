using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Potions.Mocks;

public sealed class MockDiscardAndAddShivsPotion : PotionModel
{
	private const string _shivKey = "Shivs";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(2),
		new DynamicVar("Shivs", 2m)
	});

	public override PotionRarity Rarity => PotionRarity.None;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await CardCmd.Discard(choiceContext, await CardSelectCmd.FromHandForDiscard(prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, base.DynamicVars.Cards.IntValue), context: choiceContext, player: base.Owner, filter: null, source: this));
		await Shiv.CreateInHand(base.Owner, base.DynamicVars["Shivs"].IntValue, base.Owner.Creature.CombatState);
	}
}
