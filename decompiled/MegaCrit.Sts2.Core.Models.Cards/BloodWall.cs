using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BloodWall : CardModel
{
	private const string _bloodWallVfxPath = "vfx/vfx_blood_wall";

	private const string _bloodWallSfx = "event:/sfx/characters/ironclad/ironclad_bloodwall";

	protected override IEnumerable<string> ExtraRunAssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(SceneHelper.GetScenePath("vfx/vfx_blood_wall"));

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HpLossVar(2m),
		new BlockVar(16m, ValueProp.Move)
	});

	public BloodWall()
		: base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		VfxCmd.PlayOnCreatureCenter(base.Owner.Creature, "vfx/vfx_bloody_impact");
		await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_bloodwall");
		VfxCmd.PlayOnCreature(base.Owner.Creature, "vfx/vfx_blood_wall");
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(4m);
	}
}
