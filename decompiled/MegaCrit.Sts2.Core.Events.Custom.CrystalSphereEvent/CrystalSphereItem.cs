using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;

/// <summary>
/// The items that are on the grid for the Crystal Sphere minigame
/// </summary>
public abstract class CrystalSphereItem
{
	public abstract Vector2I Size { get; }

	public Vector2I Position { get; private set; }

	protected virtual string TexturePath => ImageHelper.GetImagePath("events/crystal_sphere/" + GetType().Name.ToSnakeCase() + ".png");

	public Texture2D Texture => PreloadManager.Cache.GetTexture2D(TexturePath);

	public abstract bool IsGood { get; }

	public event Action<CrystalSphereItem>? Revealed;

	/// <summary>
	/// Places an item randomly on the minigame's grid
	/// </summary>
	/// <param name="game"></param>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	public bool PlaceItem(CrystalSphereMinigame game)
	{
		List<Vector2I> list = new List<Vector2I>();
		for (int i = 0; i < game.GridSize.X; i++)
		{
			for (int j = 0; j < game.GridSize.Y; j++)
			{
				if (CanPlaceHere(game.cells, i, j))
				{
					list.Add(new Vector2I(i, j));
				}
			}
		}
		if (!list.Any())
		{
			return false;
		}
		Position = game.Rng.NextItem(list);
		for (int k = 0; k < Size.X; k++)
		{
			for (int l = 0; l < Size.Y; l++)
			{
				int num = Position.X + k;
				int num2 = Position.Y + l;
				game.cells[num, num2].SetItem(this);
			}
		}
		return true;
	}

	/// <summary>
	/// Returns true if this coordinate is a valid place to put this item
	/// </summary>
	/// <param name="grid">Grid we are doing the placement on</param>
	/// <param name="x">Potential X coord we are validating</param>
	/// <param name="y">Potential Y coord we are validating</param>
	/// <returns></returns>
	private bool CanPlaceHere(CrystalSphereCell[,] grid, int x, int y)
	{
		for (int i = 0; i < Size.X; i++)
		{
			for (int j = 0; j < Size.Y; j++)
			{
				int num = x + i;
				int num2 = y + j;
				if (num < 0 || num >= grid.GetLength(0))
				{
					return false;
				}
				if (num2 < 0 || num2 >= grid.GetLength(1))
				{
					return false;
				}
				if (!grid[num, num2].IsHidden)
				{
					return false;
				}
				if (grid[num, num2].Item != null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual Task RevealItem(Player _)
	{
		this.Revealed?.Invoke(this);
		return Task.CompletedTask;
	}

	public virtual Reward? ToReward(Player owner, Rng rng)
	{
		return null;
	}

	public abstract SerializableCrystalSphereItem ToSerializable();

	public static CrystalSphereItem FromSerializable(SerializableCrystalSphereItem serializable, Player owner)
	{
		return serializable.type switch
		{
			CrystalSphereItemType.CardReward => new CrystalSphereCardReward(serializable.cardRarity, owner), 
			CrystalSphereItemType.Potion => new CrystalSpherePotion(serializable.potionRarity), 
			CrystalSphereItemType.Gold => new CrystalSphereGold(serializable.isBigGold), 
			CrystalSphereItemType.Curse => new CrystalSphereCurse(), 
			CrystalSphereItemType.Relic => new CrystalSphereRelic(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
