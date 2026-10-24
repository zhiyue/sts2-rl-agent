using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models;

public abstract class EnchantmentModel : AbstractModel
{
	public const string locTable = "enchantments";

	private string? _iconPath;

	private CardModel? _card;

	private int _amount;

	private DynamicVarSet? _dynamicVars;

	private EnchantmentStatus _status;

	private EnchantmentModel _canonicalInstance;

	public LocString Title => new LocString("enchantments", base.Id.Entry + ".title");

	private LocString Description => new LocString("enchantments", base.Id.Entry + ".description");

	private LocString ExtraCardText => new LocString("enchantments", base.Id.Entry + ".extraCardText");

	public virtual bool HasExtraCardText => false;

	public LocString DynamicDescription
	{
		get
		{
			LocString description = Description;
			description.Add("Amount", Amount);
			DynamicVarSet dynamicVarSet = DynamicVars.Clone(this);
			dynamicVarSet.ClearPreview();
			_card?.UpdateDynamicVarPreview(CardPreviewMode.None, null, dynamicVarSet);
			description.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
			dynamicVarSet.AddTo(description);
			return description;
		}
	}

	public LocString? DynamicExtraCardText
	{
		get
		{
			if (!HasExtraCardText || Status == EnchantmentStatus.Disabled)
			{
				return null;
			}
			LocString extraCardText = ExtraCardText;
			extraCardText.Add("Amount", Amount);
			if (base.IsCanonical)
			{
				extraCardText.Add("TargetType", "None");
			}
			else
			{
				extraCardText.Add("TargetType", Card.TargetType.ToString());
			}
			DynamicVars.AddTo(extraCardText);
			return extraCardText;
		}
	}

	public static string MissingIconPath => ImageHelper.GetImagePath("enchantments/missing_enchantment.png");

	public string IntendedIconPath => ImageHelper.GetImagePath("enchantments/" + base.Id.Entry.ToLowerInvariant() + ".png");

	private string BetaIconPath => ImageHelper.GetImagePath("enchantments/beta/" + base.Id.Entry.ToLowerInvariant() + ".png");

	public string IconPath
	{
		get
		{
			if (_iconPath == null)
			{
				if (ResourceLoader.Exists(IntendedIconPath))
				{
					_iconPath = IntendedIconPath;
				}
				else if (ResourceLoader.Exists(BetaIconPath))
				{
					_iconPath = BetaIconPath;
				}
				else
				{
					_iconPath = MissingIconPath;
				}
			}
			return _iconPath;
		}
	}

	public CompressedTexture2D Icon => PreloadManager.Cache.GetCompressedTexture2D(IconPath);

	public virtual bool ShowAmount => false;

	public virtual int DisplayAmount => Amount;

	public override bool PreviewOutsideOfCombat => true;

	public override bool ShouldReceiveCombatHooks => Card?.ShouldReceiveCombatHooks ?? false;

	/// <summary>
	/// Set this to true to sort the card to the bottom of the draw pile at the beginning of combat.
	/// </summary>
	public virtual bool ShouldStartAtBottomOfDrawPile => false;

	/// <summary>
	/// Get the card that this is enchanting.
	/// This is almost never null, so we leave it as non-nullable to make it easier to use. If you really need to check
	/// for null, use <see cref="P:MegaCrit.Sts2.Core.Models.EnchantmentModel.HasCard" />.
	/// </summary>
	public CardModel Card
	{
		get
		{
			AssertMutable();
			return _card;
		}
		set
		{
			AssertMutable();
			value.AssertMutable();
			if (_card != null)
			{
				throw new InvalidOperationException("Enchantments cannot be moved from one card to another.");
			}
			_card = value;
		}
	}

	/// <summary>
	/// Does this enchantment have a card?
	/// </summary>
	public bool HasCard => _card != null;

	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			AssertMutable();
			_amount = value;
		}
	}

	[JsonPropertyName("props")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SavedProperties? Props { get; set; }

	public virtual bool IsStackable => false;

	public DynamicVarSet DynamicVars
	{
		get
		{
			if (_dynamicVars != null)
			{
				return _dynamicVars;
			}
			_dynamicVars = new DynamicVarSet(CanonicalVars);
			_dynamicVars.InitializeWithOwner(this);
			return _dynamicVars;
		}
	}

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public EnchantmentStatus Status
	{
		get
		{
			return _status;
		}
		set
		{
			AssertMutable();
			if (_status != value)
			{
				_status = value;
				this.StatusChanged?.Invoke();
			}
		}
	}

	/// <summary>
	/// Override this property to add conditions to check to determine whether to show a gold glow on this card.
	/// </summary>
	public virtual bool ShouldGlowGold => false;

	/// <summary>
	/// Override this property to add conditions to check to determine whether to show a red glow on this card.
	/// For example, Corrupted causes the card to glow red if it will kill you.
	/// </summary>
	public virtual bool ShouldGlowRed => false;

	public EnchantmentModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		private set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

	public HoverTip HoverTip => new HoverTip(Title, DynamicDescription, Icon);

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			int num = 1;
			List<IHoverTip> list = new List<IHoverTip>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<IHoverTip> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = HoverTip;
			List<IHoverTip> list2 = list;
			list2.AddRange(ExtraHoverTips);
			return list2;
		}
	}

	public event Action? StatusChanged;

	public virtual bool CanEnchantCardType(CardType cardType)
	{
		return true;
	}

	/// <summary>
	/// Checks whether the specified card can be enchanted with this enchantment. For example, Sharp can only
	/// enchant attacks, so this will return true if an Attack is passed, but false if a Skill is passed.
	///
	/// Note: Do not override this method to REMOVE restrictions, just to ADD them. When you override it, make sure to
	/// call `base.CanEnchant`, and then add your own restrictions afterwards. You can also override some other methods
	/// to add specific types of restrictions (check EnchantmentModel.cs for details).
	/// </summary>
	/// <param name="card">Card to check validity of.</param>
	/// <returns>Whether or not the specified card is valid to enchant with this.</returns>
	public virtual bool CanEnchant(CardModel card)
	{
		CardType type = card.Type;
		if ((uint)(type - 4) <= 2u)
		{
			return false;
		}
		if (!CanEnchantCardType(card.Type))
		{
			return false;
		}
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck && card.Keywords.Contains(CardKeyword.Unplayable))
		{
			return false;
		}
		if (card.Enchantment != null && (!IsStackable || card.Enchantment.GetType() != GetType()))
		{
			return false;
		}
		return true;
	}

	public virtual Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		return Task.CompletedTask;
	}

	public EnchantmentModel ToMutable()
	{
		AssertCanonical();
		EnchantmentModel enchantmentModel = (EnchantmentModel)MutableClone();
		enchantmentModel.CanonicalInstance = this;
		return enchantmentModel;
	}

	protected override void DeepCloneFields()
	{
		_card = null;
		this.StatusChanged = null;
		_dynamicVars = DynamicVars.Clone(this);
	}

	/// <summary>
	/// WARNING: If you're thinking of calling this from inside a model, you probably want <see cref="M:MegaCrit.Sts2.Core.Commands.CardCmd.Enchant(MegaCrit.Sts2.Core.Models.EnchantmentModel,MegaCrit.Sts2.Core.Models.CardModel,System.Decimal)" />
	/// instead.
	///
	/// Apply this enchantment to the specified card. This does not run any hooks.
	/// </summary>
	/// <param name="card">Card to enchant.</param>
	/// <param name="amount">Amount for the enchantment.</param>
	public void ApplyInternal(CardModel card, decimal amount)
	{
		if (Card != null)
		{
			throw new InvalidOperationException("Can't apply an enchantment to a card when it's already been applied to a different card.");
		}
		AssertMutable();
		card.AssertMutable();
		Amount = (int)amount;
		Card = card;
	}

	public void ClearInternal()
	{
		AssertMutable();
		_card = null;
	}

	/// <summary>
	/// Run this enchantment's modification logic on the enchanted card. You usually want to call
	/// <see cref="M:MegaCrit.Sts2.Core.Models.EnchantmentModel.ApplyInternal(MegaCrit.Sts2.Core.Models.CardModel,System.Decimal)" /> first.
	///
	/// This is called after a card is first enchanted, and after it is deserialized.
	///
	/// It is also called when an enchantment's modification logic needs to be refreshed. For example, after a card is
	/// downgraded, its DamageVars will be reset to their original values. If the card is enchanted with Sharp, we need
	/// to re-execute its modification logic so the card's damage will still be increased.
	///
	/// It is NOT called when a card is cloned, because the enchantments effects will already be reflected in the card's
	/// values.
	/// </summary>
	public void ModifyCard()
	{
		if (Card == null)
		{
			throw new InvalidOperationException("Card must be set at this point.");
		}
		OnEnchant();
		RecalculateValues();
		Card.DynamicVars.RecalculateForUpgradeOrEnchant();
	}

	public virtual void RecalculateValues()
	{
	}

	public SerializableEnchantment ToSerializable()
	{
		AssertMutable();
		return new SerializableEnchantment
		{
			Id = base.Id,
			Props = SavedProperties.From(this),
			Amount = Amount
		};
	}

	public static EnchantmentModel FromSerializable(SerializableEnchantment save)
	{
		EnchantmentModel enchantmentModel = SaveUtil.EnchantmentOrDeprecated(save.Id).ToMutable();
		save.Props?.Fill(enchantmentModel);
		enchantmentModel.Amount = save.Amount;
		return enchantmentModel;
	}

	/// <summary>
	/// Modify the card that this is enchanting.
	/// Use this for enchantments that do things similar to upgrades, like change the values of a card's DynamicVars
	/// (Sharp), change a card's keywords (Royally Approved), change a card's energy cost (Tezcatara's Ember), etc.
	/// To modify a card's energy cost, use <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.UpgradeBy(System.Int32)" />.
	/// </summary>
	protected virtual void OnEnchant()
	{
	}

	/// <summary>
	/// Add to the amount of block that this enchantment's card gains.
	/// This hook runs BEFORE all other block modification hooks.
	/// Enchantments MUST use this hook instead of <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyBlockAdditive(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	/// </summary>
	/// <param name="originalBlock">The original amount of block that would be gained.</param>
	/// <returns>The amount to add to the block gain.</returns>
	public virtual decimal EnchantBlockAdditive(decimal originalBlock)
	{
		return 0m;
	}

	/// <summary>
	/// Modify the amount of block that this enchantment's card gains.
	/// This hook runs BEFORE all other block modification hooks.
	/// Enchantments MUST use this hook instead of <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyBlockMultiplicative(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" />.
	/// </summary>
	/// <param name="originalBlock">The original amount of block that would be gained.</param>
	/// <returns>The amount to multiply the block gain by.</returns>
	public virtual decimal EnchantBlockMultiplicative(decimal originalBlock)
	{
		return 1m;
	}

	/// <summary>
	/// Add to the amount of damage that this enchantment's card does.
	/// This hook runs BEFORE all other damage modification hooks.
	/// Enchantments MUST use this hook instead of <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageAdditive(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	/// <param name="originalDamage">The amount of damage that would be dealt.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <returns>Amount of damage to be added.</returns>
	public virtual decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		return 0m;
	}

	/// <summary>
	/// Multiply the amount of damage that this enchantment's card does.
	/// This hook runs BEFORE all other damage modification hooks.
	/// Enchantments MUST use this hook instead of <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyDamageMultiplicative(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	/// <param name="originalDamage">The amount of damage that would be dealt.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <returns>Amount that the damage should be multiplied by.</returns>
	public virtual decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
	{
		return 1m;
	}

	/// <summary>
	/// Modify the number of times this enchantment's card is played.
	/// This hook runs BEFORE all other card play count modification hooks.
	/// Enchantments MUST use this hook instead of <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardPlayCount(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Int32)" />.
	/// </summary>
	/// <param name="originalPlayCount">The original number of times this card would be played.</param>
	/// <returns>The new number of times this card should be played.</returns>
	public virtual int EnchantPlayCount(int originalPlayCount)
	{
		return originalPlayCount;
	}
}
