using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Inflame : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(2m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	protected override IEnumerable<string> ExtraRunAssetPaths => NGroundFireVfx.AssetPaths;

	public Inflame()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		NPowerUpVfx.CreateNormal(base.Owner.Creature);
		await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);
	}

	public override async Task OnEnqueuePlayVfx(Creature? target)
	{
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(base.Owner.Creature));
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["StrengthPower"].UpgradeValueBy(1m);
	}
}
