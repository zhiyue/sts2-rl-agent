using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class CrashLanding : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Debris>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(21m, ValueProp.Move));

	public CrashLanding()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(choiceContext);
		int num = 10 - CardPile.GetCards(base.Owner, PileType.Hand).Count();
		List<CardModel> list = new List<CardModel>();
		for (int i = 0; i < num; i++)
		{
			list.Add(base.CombatState.CreateCard<Debris>(base.Owner));
		}
		await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Hand, addedByPlayer: true);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
