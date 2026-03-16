using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSphereCardReward : CrystalSphereItem
{
	private readonly Player _owner;

	private readonly CrystalSphereMinigame _grid;

	private readonly CardRarity _rarity;

	private string BannerMaterialPath => _rarity switch
	{
		CardRarity.Uncommon => "res://materials/cards/banners/card_banner_uncommon_mat.tres", 
		CardRarity.Rare => "res://materials/cards/banners/card_banner_rare_mat.tres", 
		CardRarity.Curse => "res://materials/cards/banners/card_banner_curse_mat.tres", 
		CardRarity.Status => "res://materials/cards/banners/card_banner_status_mat.tres", 
		CardRarity.Event => "res://materials/cards/banners/card_banner_event_mat.tres", 
		CardRarity.Quest => "res://materials/cards/banners/card_banner_quest_mat.tres", 
		CardRarity.Ancient => "res://materials/cards/banners/card_banner_ancient_mat.tres", 
		_ => "res://materials/cards/banners/card_banner_common_mat.tres", 
	};

	public Material BannerMaterial => PreloadManager.Cache.GetMaterial(BannerMaterialPath);

	public Material FrameMaterial => _owner.Character.CardPool.FrameMaterial;

	public override Vector2I Size => new Vector2I(2, 2);

	protected override string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/crystal_sphere_" + _rarity.ToString().ToLowerInvariant() + "_card_reward.png");

	public override bool IsGood => true;

	public CrystalSphereCardReward(CrystalSphereMinigame grid, CardRarity rarity, Player owner)
	{
		_grid = grid;
		_rarity = rarity;
		_owner = owner;
	}

	public override async Task RevealItem(Player owner)
	{
		await base.RevealItem(owner);
		CardCreationOptions options = new CardCreationOptions(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(owner.Character.CardPool), CardCreationSource.Other, CardRarityOddsType.Uniform, (CardModel c) => c.Rarity == _rarity).WithRngOverride(_grid.Rng);
		_grid.AddReward(new CardReward(options, 3, owner).SetRng(_grid.Rng));
	}
}
