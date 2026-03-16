using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Bolas : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(3m, ValueProp.Move));

	protected override IEnumerable<string> ExtraRunAssetPaths => NBolasVfx.AssetPaths;

	public Bolas()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NBolasVfx.Create(base.Owner.Creature, cardPlay.Target));
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) => e.RoundNumber == base.CombatState.RoundNumber - 1 && e.CardPlay.Card == this))
		{
			CardPile? pile = base.Pile;
			if (pile == null || pile.Type != PileType.Hand)
			{
				await CardPileCmd.Add(this, PileType.Hand);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
	}
}
