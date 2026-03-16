using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DeathsDoor : CardModel
{
	public override bool GainsBlock => true;

	protected override bool ShouldGlowGoldInternal => WasDoomAppliedThisTurn;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(6m, ValueProp.Move),
		new RepeatVar(2)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	private bool WasDoomAppliedThisTurn => CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>().Any((PowerReceivedEntry e) => e.HappenedThisTurn(base.CombatState) && e.Power is DoomPower && e.Applier == base.Owner.Creature);

	public DeathsDoor()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int blockGains = 1;
		if (WasDoomAppliedThisTurn)
		{
			blockGains += base.DynamicVars.Repeat.IntValue;
		}
		for (int i = 0; i < blockGains; i++)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
	}
}
