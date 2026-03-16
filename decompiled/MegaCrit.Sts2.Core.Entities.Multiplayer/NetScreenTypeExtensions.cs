using System;
using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public static class NetScreenTypeExtensions
{
	public static Texture2D? GetLocationIcon(this NetScreenType screenType)
	{
		string text = screenType switch
		{
			NetScreenType.None => null, 
			NetScreenType.Room => null, 
			NetScreenType.Map => "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_map.tres", 
			NetScreenType.Settings => "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_settings.tres", 
			NetScreenType.PauseMenu => "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_settings.tres", 
			NetScreenType.Compendium => "res://images/atlases/ui_atlas.sprites/compendium.tres", 
			NetScreenType.DeckView => "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_deck.tres", 
			NetScreenType.CardPile => "res://images/packed/combat_ui/discard_pile.png", 
			NetScreenType.SimpleCardsView => "res://images/ui/reward_screen/reward_icon_card.png", 
			NetScreenType.CardSelection => "res://images/ui/reward_screen/reward_icon_card.png", 
			NetScreenType.GameOver => null, 
			NetScreenType.Rewards => "res://images/ui/reward_screen/reward_icon_money.png", 
			NetScreenType.Feedback => "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_settings.tres", 
			NetScreenType.SharedRelicPicking => "res://images/ui/reward_screen/reward_icon_shared_relic.png", 
			NetScreenType.RemotePlayerExpandedState => null, 
			_ => throw new ArgumentOutOfRangeException("screenType", screenType, null), 
		};
		if (text == null)
		{
			return null;
		}
		return PreloadManager.Cache.GetTexture2D(text);
	}
}
