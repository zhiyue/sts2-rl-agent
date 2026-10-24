using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Soul : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public Soul()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
	{
	}

	/// <summary>
	/// Create new Soul cards, owned by the specified player, and add them to their hand.
	/// </summary>
	/// <param name="owner">Player who will own the Soul cards.</param>
	/// <param name="amount">Number of Soul cards to create.</param>
	/// <param name="combatState">CombatState to create the Soul cards in.</param>
	/// <returns>The newly-created Soul cards.</returns>
	public static async Task<IEnumerable<Soul>> CreateInHand(Player owner, int amount, ICombatState combatState)
	{
		IEnumerable<Soul> souls = Create(owner, amount, combatState);
		await CardPileCmd.AddGeneratedCardsToCombat(souls, PileType.Hand, owner);
		return souls;
	}

	/// <summary>
	/// Create new Soul cards, owned by the specified player.
	/// </summary>
	/// <param name="owner">Player who will own the Soul cards.</param>
	/// <param name="amount">Number of Soul cards to create.</param>
	/// <param name="combatState">CombatState to create the Soul cards in.</param>
	/// <returns>The newly-created Soul cards.</returns>
	public static IEnumerable<Soul> Create(Player owner, int amount, ICombatState combatState)
	{
		List<Soul> list = new List<Soul>();
		for (int i = 0; i < amount; i++)
		{
			list.Add(combatState.CreateCard<Soul>(owner));
		}
		return list;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
