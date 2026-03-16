using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Glitterstream : CardModel
{
	private const string _blockNextTurnKey = "BlockNextTurn";

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(11m, ValueProp.Move),
		new BlockVar("BlockNextTurn", 4m, ValueProp.Move)
	});

	public Glitterstream()
		: base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		BlockVar blockVar = (BlockVar)base.DynamicVars["BlockNextTurn"];
		IEnumerable<AbstractModel> modifiers;
		decimal blockNextTurnAmount = Hook.ModifyBlock(base.CombatState, base.Owner.Creature, blockVar.BaseValue, blockVar.Props, this, cardPlay, out modifiers);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<BlockNextTurnPower>(base.Owner.Creature, blockNextTurnAmount, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
		base.DynamicVars["BlockNextTurn"].UpgradeValueBy(2m);
	}
}
