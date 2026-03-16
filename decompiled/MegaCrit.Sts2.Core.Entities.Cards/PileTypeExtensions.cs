using System;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class PileTypeExtensions
{
	public static CardPile GetPile(this PileType pileType, Player player)
	{
		ArgumentNullException.ThrowIfNull(player, "player");
		CardPile cardPile = CardPile.Get(pileType, player);
		if (cardPile == null)
		{
			throw new InvalidOperationException($"Tried to get {pileType} pile while out of combat.");
		}
		return cardPile;
	}

	public static bool IsCombatPile(this PileType pileType)
	{
		if ((uint)(pileType - 1) <= 4u)
		{
			return true;
		}
		return false;
	}

	public static Vector2 GetTargetPosition(this PileType pileType, NCard? node)
	{
		if (pileType.IsCombatPile() && !CombatManager.Instance.IsInProgress)
		{
			return Vector2.Zero;
		}
		Vector2 size = NGame.Instance.GetViewportRect().Size;
		return pileType switch
		{
			PileType.None => size, 
			PileType.Draw => NCombatRoom.Instance.Ui.DrawPile.GlobalPosition + NCombatRoom.Instance.Ui.DrawPile.Size * 0.5f, 
			PileType.Hand => new Vector2(size.X * 0.5f - node.Size.X * 0.5f, size.Y - node.Size.Y * 0.5f), 
			PileType.Discard => NCombatRoom.Instance.Ui.DiscardPile.GlobalPosition + NCombatRoom.Instance.Ui.DiscardPile.Size * 0.5f, 
			PileType.Play => NCombatRoom.Instance.Ui.PlayContainer.Size * 0.5f - node.Size * 0.5f + Vector2.Up * 100f, 
			PileType.Deck => NRun.Instance.GlobalUi.TopBar.Deck.GlobalPosition + NRun.Instance.GlobalUi.TopBar.Deck.Size * 0.5f, 
			PileType.Exhaust => NCombatRoom.Instance.Ui.ExhaustPile.GlobalPosition + NCombatRoom.Instance.Ui.ExhaustPile.Size * 0.5f, 
			_ => throw new ArgumentOutOfRangeException("pileType", pileType, "Unknown pile type"), 
		};
	}
}
