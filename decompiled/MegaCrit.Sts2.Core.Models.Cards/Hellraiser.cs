using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Hellraiser : CardModel
{
	protected override IEnumerable<string> ExtraRunAssetPaths => NHellraiserVfx.AssetPaths;

	public Hellraiser()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<HellraiserPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
	}

	public override async Task OnEnqueuePlayVfx(Creature? target)
	{
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHellraiserVfx.Create(base.Owner.Creature));
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
