using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
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

public sealed class EndOfDays : CardModel
{
	public const int doomAmount = 29;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<DoomPower>(29m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public EndOfDays()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		Vector2? sideCenterFloor = VfxCmd.GetSideCenterFloor(CombatSide.Enemy, base.CombatState);
		if (sideCenterFloor.HasValue)
		{
			NLargeMagicMissileVfx nLargeMagicMissileVfx = NLargeMagicMissileVfx.Create(sideCenterFloor.Value, new Color("8c2447"));
			if (nLargeMagicMissileVfx != null)
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nLargeMagicMissileVfx);
				await Cmd.Wait(nLargeMagicMissileVfx.WaitTime);
			}
		}
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<DoomPower>(hittableEnemy, base.DynamicVars.Doom.BaseValue, base.Owner.Creature, this);
		}
		await DoomPower.DoomKill(DoomPower.GetDoomedCreatures(base.CombatState.HittableEnemies));
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Doom.UpgradeValueBy(8m);
	}
}
