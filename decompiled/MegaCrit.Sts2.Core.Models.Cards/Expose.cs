using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Expose : CardModel
{
	private const string _powerKey = "Power";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Power", 2m));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<ArtifactPower>(),
		HoverTipFactory.Static(StaticHoverTip.Block)
	});

	public Expose()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, "vfx/vfx_flying_slash");
		int amount = base.DynamicVars["Power"].IntValue;
		await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);
		if (cardPlay.Target.HasPower<ArtifactPower>())
		{
			await PowerCmd.Remove<ArtifactPower>(cardPlay.Target);
		}
		await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, amount, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Power"].UpgradeValueBy(1m);
	}
}
