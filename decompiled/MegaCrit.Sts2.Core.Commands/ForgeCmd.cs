using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class ForgeCmd
{
	private const string _forgeInitialSfx = "event:/sfx/characters/regent/regent_forge";

	private const string _forgeRefineSfx = "event:/sfx/characters/regent/regent_refine";

	public static async Task<IEnumerable<SovereignBlade>> Forge(decimal amount, Player player, AbstractModel? source)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<SovereignBlade>();
		}
		List<SovereignBlade> blades = GetSovereignBlades(player, includeExhausted: false).ToList();
		if (blades.Count == 0)
		{
			SovereignBlade sovereignBlade = player.Creature.CombatState.CreateCard<SovereignBlade>(player);
			sovereignBlade.CreatedThroughForge = true;
			await CardPileCmd.AddGeneratedCardToCombat(sovereignBlade, PileType.Hand, addedByPlayer: true);
			blades.Add(sovereignBlade);
		}
		IncreaseSovereignBladeDamage(amount, player);
		await Hook.AfterForge(player.Creature.CombatState, amount, player, source);
		return blades;
	}

	private static void IncreaseSovereignBladeDamage(decimal amount, Player player)
	{
		List<SovereignBlade> list = GetSovereignBlades(player, includeExhausted: true).ToList();
		foreach (SovereignBlade item in list)
		{
			item.AddDamage(amount);
			item.AfterForged();
			PlayCombatRoomForgeVfx(player, item);
		}
		PreviewSovereignBlade(list);
	}

	private static IEnumerable<SovereignBlade> GetSovereignBlades(Player player, bool includeExhausted)
	{
		return player.PlayerCombatState.AllCards.Where(delegate(CardModel c)
		{
			if (!c.IsDupe)
			{
				if (!includeExhausted)
				{
					CardPile? pile = c.Pile;
					if (pile == null)
					{
						return true;
					}
					return pile.Type != PileType.Exhaust;
				}
				return true;
			}
			return false;
		}).OfType<SovereignBlade>();
	}

	private static void PreviewSovereignBlade(IReadOnlyCollection<SovereignBlade> blades)
	{
		if (TestMode.IsOn || !LocalContext.IsMine(blades.First()))
		{
			return;
		}
		List<SovereignBlade> list = blades.Where((SovereignBlade c) => c.Pile.Type == PileType.Hand).ToList();
		List<SovereignBlade> list2 = blades.Where((SovereignBlade c) => c.Pile.Type != PileType.Hand).ToList();
		foreach (SovereignBlade item in list)
		{
			NCardSmithVfx child = NCardSmithVfx.Create(NCombatRoom.Instance.Ui.Hand.GetCard(item), playSfx: false);
			NRun.Instance.GlobalUi.AboveTopBarVfxContainer.AddChildSafely(child);
		}
		if (list2.Count != 0)
		{
			NCardSmithVfx child2 = NCardSmithVfx.Create(list2, playSfx: false);
			NRun.Instance.GlobalUi.CardPreviewContainer.AddChildSafely(child2);
		}
	}

	public static void PlayCombatRoomForgeVfx(Player player, CardModel card)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
		if (nCreature == null)
		{
			return;
		}
		NSovereignBladeVfx nSovereignBladeVfx = SovereignBlade.GetVfxNode(player, card);
		bool flag = nSovereignBladeVfx == null;
		if (flag)
		{
			nSovereignBladeVfx = NSovereignBladeVfx.Create(card);
			nCreature.AddChildSafely(nSovereignBladeVfx);
			nSovereignBladeVfx.Position = Vector2.Zero;
			SfxCmd.Play("event:/sfx/characters/regent/regent_forge");
		}
		else
		{
			SfxCmd.Play("event:/sfx/characters/regent/regent_refine");
		}
		nSovereignBladeVfx.Forge(card.DynamicVars.Damage.IntValue, flag);
		if (!flag)
		{
			return;
		}
		List<SovereignBlade> list = GetSovereignBlades(player, includeExhausted: false).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			NSovereignBladeVfx vfxNode = SovereignBlade.GetVfxNode(player, list[i]);
			if (vfxNode != null)
			{
				vfxNode.OrbitProgress = (float)i / (float)list.Count;
			}
		}
	}
}
