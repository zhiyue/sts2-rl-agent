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
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FanOfKnives : CardModel
{
	private const string _shivsKey = "Shivs";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar("Shivs", 4));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Shiv>());

	protected override IEnumerable<string> ExtraRunAssetPaths => NFanOfKnivesVfx.AssetPaths;

	public FanOfKnives()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<FanOfKnivesPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		for (int i = 0; i < base.DynamicVars["Shivs"].IntValue; i++)
		{
			await Shiv.CreateInHand(base.Owner, base.CombatState);
			await Cmd.CustomScaledWait(0.1f, 0.2f);
		}
	}

	public override async Task OnEnqueuePlayVfx(Creature? target)
	{
		NCombatRoom.Instance?.BackCombatVfxContainer.AddChildSafely(NFanOfKnivesVfx.Create(base.Owner.Creature));
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Shivs"].UpgradeValueBy(1m);
	}
}
