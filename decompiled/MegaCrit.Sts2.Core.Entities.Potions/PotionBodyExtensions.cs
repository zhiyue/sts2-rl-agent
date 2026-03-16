using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Entities.Potions;

public static class PotionBodyExtensions
{
	private static readonly Dictionary<(PotionBody, PotionOverlay), string> _potionOverlayMap = new Dictionary<(PotionBody, PotionOverlay), string>
	{
		[(PotionBody.Cube, PotionOverlay.Bubbles)] = "potion_cube_bubbles.png",
		[(PotionBody.Cube, PotionOverlay.Curve)] = "potion_cube_curve.png",
		[(PotionBody.Diamond, PotionOverlay.Bubbles)] = "potion_diamond_bubbles.png",
		[(PotionBody.Fairy, PotionOverlay.Sparkle)] = "potion_fairy_sparkle.png",
		[(PotionBody.FatDiamond, PotionOverlay.Curve)] = "potion_fat_diamond_curve.png",
		[(PotionBody.Heart, PotionOverlay.Curve)] = "potion_heart_curve.png",
		[(PotionBody.Spiky, PotionOverlay.Curve)] = "potion_spiky_curve.png"
	};

	public static string GetBodyPath(this PotionBody body)
	{
		return ImageHelper.GetImagePath("packed/potion/body/" + body switch
		{
			PotionBody.None => throw new ArgumentOutOfRangeException("body", body, null), 
			PotionBody.Anvil => "potion_anvil_body.png", 
			PotionBody.Bolt => "potion_bolt_body.png", 
			PotionBody.Card => "potion_card_body.png", 
			PotionBody.Cube => "potion_cube_body.png", 
			PotionBody.Diamond => "potion_diamond_body.png", 
			PotionBody.Eye => "potion_eye_body.png", 
			PotionBody.Fairy => "potion_fairy_body.png", 
			PotionBody.Fat => "potion_fat_body.png", 
			PotionBody.FatDiamond => "potion_fat_diamond_body.png", 
			PotionBody.Flask => "potion_flask_body.png", 
			PotionBody.Ghost => "potion_ghost_body.png", 
			PotionBody.Heart => "potion_heart_body.png", 
			PotionBody.Moon => "potion_moon_body.png", 
			PotionBody.Shield => "potion_shield_body.png", 
			PotionBody.Snecko => "potion_snecko_body.png", 
			PotionBody.Sphere => "potion_sphere_body.png", 
			PotionBody.Spiky => "potion_spiky_body.png", 
			PotionBody.Thin => "potion_thin_body.png", 
			_ => throw new ArgumentOutOfRangeException("body", body, null), 
		});
	}

	public static string? GetGradientPath(this PotionBody body)
	{
		string text = body switch
		{
			PotionBody.Anvil => "potion_anvil_gradient.png", 
			PotionBody.Card => "potion_card_gradient.png", 
			PotionBody.Sphere => "potion_sphere_gradient.png", 
			_ => null, 
		};
		if (text != null)
		{
			return ImageHelper.GetImagePath("packed/potion/gradient/" + text);
		}
		return null;
	}

	public static string GetJuicePath(this PotionBody body)
	{
		return ImageHelper.GetImagePath("packed/potion/juice/" + body switch
		{
			PotionBody.None => throw new ArgumentOutOfRangeException("body", body, null), 
			PotionBody.Anvil => "potion_anvil_juice.png", 
			PotionBody.Bolt => "potion_bolt_juice.png", 
			PotionBody.Card => "potion_card_juice.png", 
			PotionBody.Cube => "potion_cube_juice.png", 
			PotionBody.Diamond => "potion_diamond_juice.png", 
			PotionBody.Eye => "potion_eye_juice.png", 
			PotionBody.Fairy => "potion_fairy_juice.png", 
			PotionBody.Fat => "potion_fat_juice.png", 
			PotionBody.FatDiamond => "potion_fat_diamond_juice.png", 
			PotionBody.Flask => "potion_flask_juice.png", 
			PotionBody.Ghost => "potion_ghost_juice.png", 
			PotionBody.Heart => "potion_heart_juice.png", 
			PotionBody.Moon => "potion_moon_juice.png", 
			PotionBody.Shield => "potion_shield_juice.png", 
			PotionBody.Snecko => "potion_snecko_juice.png", 
			PotionBody.Sphere => "potion_sphere_juice.png", 
			PotionBody.Spiky => "potion_spiky_juice.png", 
			PotionBody.Thin => "potion_thin_juice.png", 
			_ => throw new ArgumentOutOfRangeException("body", body, null), 
		});
	}

	public static string? GetOverlayPath(this PotionBody body, PotionOverlay overlay)
	{
		if (_potionOverlayMap.TryGetValue((body, overlay), out string value))
		{
			return ImageHelper.GetImagePath("packed/potion/overlay/" + value);
		}
		return null;
	}
}
