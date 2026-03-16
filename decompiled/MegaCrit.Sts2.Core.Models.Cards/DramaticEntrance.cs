using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DramaticEntrance : CardModel
{
	public const string vfxPath = "vfx/vfx_dramatic_entrance_fullscreen";

	protected override IEnumerable<string> ExtraRunAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(SceneHelper.GetScenePath("vfx/vfx_dramatic_entrance_fullscreen"));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(11m, ValueProp.Move));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Exhaust,
		CardKeyword.Innate
	});

	public DramaticEntrance()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
		VfxCmd.PlayFullScreenInCombat("vfx/vfx_dramatic_entrance_fullscreen");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}
}
