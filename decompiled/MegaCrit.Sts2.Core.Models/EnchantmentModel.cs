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

	public LocString Description => new LocString("enchantments", base.Id.Entry + ".description");

	public LocString ExtraCardText => new LocString("enchantments", base.Id.Entry + ".extraCardText");

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

	public virtual bool ShouldStartAtBottomOfDrawPile => false;

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

	public virtual bool ShouldGlowGold => false;

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

	protected virtual void OnEnchant()
	{
	}

	public virtual decimal EnchantBlockAdditive(decimal originalBlock, ValueProp props)
	{
		return 0m;
	}

	public virtual decimal EnchantBlockMultiplicative(decimal originalBlock, ValueProp props)
	{
		return 1m;
	}

	public virtual decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		return 0m;
	}

	public virtual decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
	{
		return 1m;
	}

	public virtual int EnchantPlayCount(int originalPlayCount)
	{
		return originalPlayCount;
	}
}
