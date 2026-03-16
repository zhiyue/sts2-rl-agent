using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class FoulPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Event;

	public override PotionUsage Usage => PotionUsage.AnyTime;

	public override TargetType TargetType
	{
		get
		{
			if (!CombatManager.Instance.IsInProgress)
			{
				return TargetType.TargetedNoCreature;
			}
			return TargetType.AllEnemies;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(12m, ValueProp.Unpowered),
		new GoldVar(100)
	});

	public override bool PassesCustomUsabilityCheck
	{
		get
		{
			if (CombatManager.Instance.IsInProgress)
			{
				return true;
			}
			if (base.Owner.RunState.CurrentRoom is MerchantRoom)
			{
				return true;
			}
			if (base.Owner.RunState.CurrentRoom is EventRoom eventRoom && eventRoom.CanonicalEvent is FakeMerchant)
			{
				return true;
			}
			return false;
		}
	}

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			Creature creature = base.Owner.Creature;
			DamageVar damage = base.DynamicVars.Damage;
			await CreatureCmd.Damage(choiceContext, base.Owner.Creature.CombatState.Creatures, damage.BaseValue, damage.Props, creature, null);
		}
		else if (base.Owner.RunState.CurrentRoom is MerchantRoom)
		{
			NMerchantRoom nMerchantRoom = NRun.Instance?.MerchantRoom;
			if (nMerchantRoom != null)
			{
				ShowPotionVfx(nMerchantRoom.MerchantButton);
				nMerchantRoom.FoulPotionThrown(this);
			}
			await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		}
		else
		{
			if (!(base.Owner.RunState.CurrentRoom is EventRoom eventRoom) || !(eventRoom.CanonicalEvent is FakeMerchant))
			{
				return;
			}
			EventModel localMutableEvent = eventRoom.LocalMutableEvent;
			if (localMutableEvent == null || !(localMutableEvent.Node is NFakeMerchant nFakeMerchant))
			{
				return;
			}
			ShowPotionVfx(nFakeMerchant.MerchantButton);
			List<Task> list = new List<Task>();
			foreach (Player player in base.Owner.RunState.Players)
			{
				FakeMerchant fakeMerchant = (FakeMerchant)RunManager.Instance.EventSynchronizer.GetEventForPlayer(player);
				list.Add(fakeMerchant.FoulPotionThrown(this));
			}
			await Task.WhenAll(list);
		}
	}

	private void ShowPotionVfx(NMerchantButton? merchantButton)
	{
		if (!TestMode.IsOn && merchantButton != null)
		{
			string scenePath = SceneHelper.GetScenePath("vfx/vfx_slime_impact");
			Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			merchantButton.GetParent().AddChildSafely(node2D);
			node2D.GlobalPosition = merchantButton.GlobalPosition;
		}
	}
}
