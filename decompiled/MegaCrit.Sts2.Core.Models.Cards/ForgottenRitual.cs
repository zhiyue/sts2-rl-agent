using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class ForgottenRitual : CardModel
{
	protected override bool ShouldGlowGoldInternal => WasCardExhaustedThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(3));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		base.EnergyHoverTip,
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	});

	protected override IEnumerable<string> ExtraRunAssetPaths => NGroundFireVfx.AssetPaths;

	private bool WasCardExhaustedThisTurn => CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Any((CardExhaustedEntry e) => e.HappenedThisTurn(base.CombatState) && e.Card.Owner == base.Owner);

	public ForgottenRitual()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(base.Owner.Creature, VfxColor.Purple));
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		if (WasCardExhaustedThisTurn)
		{
			await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
