using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models;

public abstract class CardModel : AbstractModel
{
	private enum DescriptionPreviewType
	{
		None,
		Upgrade
	}

	private LocString? _titleLocString;

	private CardPoolModel? _pool;

	private Player? _owner;

	private CardEnergyCost? _energyCost;

	private int _baseReplayCount;

	private bool _starCostSet;

	private int _baseStarCost;

	/// <summary>
	/// Was this card's star cost just recently upgraded?
	/// This is mainly used to show upgrade preview values in green.
	/// This should be cleared after the upgrade is complete.
	/// </summary>
	private bool _wasStarCostJustUpgraded;

	private List<TemporaryCardCost> _temporaryStarCosts = new List<TemporaryCardCost>();

	private int _lastStarsSpent;

	private HashSet<CardKeyword>? _keywords;

	private HashSet<CardTag>? _tags;

	private DynamicVarSet? _dynamicVars;

	private bool _exhaustOnNextPlay;

	private bool _hasSingleTurnRetain;

	private bool _hasSingleTurnSly;

	/// <summary>
	/// The card that this is a clone of. Null when this card is not a clone (which is most of the time).
	/// Clones are exactly the same as any other type of card, we just keep track of the original to make some effects
	/// easier to implement (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Terraforming" />).
	/// </summary>
	private CardModel? _cloneOf;

	/// <summary>
	/// Whether or not this card is considered a duplicate. See <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.DupeOf" /> for more info on dupes.
	/// </summary>
	private bool _isDupe;

	private int _currentUpgradeLevel;

	private CardUpgradePreviewType _upgradePreviewType;

	private bool _isEnchantmentPreview;

	private int? _floorAddedToDeck;

	private Creature? _currentTarget;

	private int _currentPlayIndex;

	private CardModel? _deckVersion;

	private CardModel? _canonicalInstance;

	public LocString TitleLocString => _titleLocString ?? (_titleLocString = new LocString("cards", base.Id.Entry + ".title"));

	public virtual string Title
	{
		get
		{
			LocString titleLocString = TitleLocString;
			if (!IsUpgraded)
			{
				return titleLocString.GetFormattedText();
			}
			if (MaxUpgradeLevel > 1)
			{
				return $"{titleLocString.GetFormattedText()}+{CurrentUpgradeLevel}";
			}
			return titleLocString.GetFormattedText() + "+";
		}
	}

	public LocString Description => new LocString("cards", base.Id.Entry + ".description");

	protected LocString SelectionScreenPrompt
	{
		get
		{
			LocString locString = new LocString("cards", base.Id.Entry + ".selectionScreenPrompt");
			if (!locString.Exists())
			{
				throw new InvalidOperationException($"No selection screen prompt for {base.Id}.");
			}
			DynamicVars.AddTo(locString);
			return locString;
		}
	}

	public virtual string PortraitPath => ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}.tres");

	public virtual string BetaPortraitPath => ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{Pool.Title.ToLowerInvariant()}/beta/{base.Id.Entry.ToLowerInvariant()}.tres");

	public static string MissingPortraitPath => ImageHelper.GetImagePath("atlases/card_atlas.sprites/beta.tres");

	private string PortraitPngPath => ImageHelper.GetImagePath($"packed/card_portraits/{Pool.Title.ToLowerInvariant()}/{base.Id.Entry.ToLowerInvariant()}.png");

	private string BetaPortraitPngPath => ImageHelper.GetImagePath($"packed/card_portraits/{Pool.Title.ToLowerInvariant()}/beta/{base.Id.Entry.ToLowerInvariant()}.png");

	public bool HasPortrait => ResourceLoader.Exists(PortraitPngPath);

	public bool HasBetaPortrait => ResourceLoader.Exists(BetaPortraitPngPath);

	public Texture2D Portrait => ResourceLoader.Load<Texture2D>(PortraitPath, null, ResourceLoader.CacheMode.Reuse);

	private string FramePath
	{
		get
		{
			CardType cardType;
			switch (Type)
			{
			case CardType.None:
			case CardType.Status:
			case CardType.Curse:
				cardType = CardType.Skill;
				break;
			case CardType.Attack:
			case CardType.Skill:
			case CardType.Power:
			case CardType.Quest:
				cardType = Type;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (Rarity != CardRarity.Ancient)
			{
				return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/card_frame_" + cardType.ToString().ToLowerInvariant() + "_s.tres");
			}
			return ImageHelper.GetImagePath("atlases/card_atlas.sprites/beta.tres");
		}
	}

	public Texture2D Frame => ResourceLoader.Load<Texture2D>(FramePath, null, ResourceLoader.CacheMode.Reuse);

	private string PortraitBorderPath
	{
		get
		{
			CardType cardType;
			switch (Type)
			{
			case CardType.None:
			case CardType.Status:
			case CardType.Curse:
			case CardType.Quest:
				cardType = CardType.Skill;
				break;
			case CardType.Attack:
			case CardType.Skill:
			case CardType.Power:
				cardType = Type;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/card_portrait_border_" + cardType.ToString().ToLowerInvariant() + "_s.tres");
		}
	}

	private static string AncientBorderPath => ImageHelper.GetImagePath("atlases/compressed_atlas.sprites/ancient_card_border.png.tres");

	private string AncientTextBgPath
	{
		get
		{
			if (Rarity != CardRarity.Ancient)
			{
				throw new InvalidOperationException("This card is not an ancient card.");
			}
			CardType cardType;
			switch (Type)
			{
			case CardType.None:
			case CardType.Status:
			case CardType.Curse:
				cardType = CardType.Skill;
				break;
			case CardType.Attack:
			case CardType.Skill:
			case CardType.Power:
			case CardType.Quest:
				cardType = Type;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			CardType cardType2 = cardType;
			return ImageHelper.GetImagePath("atlases/compressed_atlas.sprites/ancient_text_bg_" + cardType2.ToString().ToLowerInvariant() + ".png.tres");
		}
	}

	public Texture2D AncientTextBg => ResourceLoader.Load<Texture2D>(AncientTextBgPath, null, ResourceLoader.CacheMode.Reuse);

	public Texture2D AncientBorder => ResourceLoader.Load<Texture2D>(AncientBorderPath, null, ResourceLoader.CacheMode.Reuse);

	public Texture2D PortraitBorder => ResourceLoader.Load<Texture2D>(PortraitBorderPath, null, ResourceLoader.CacheMode.Reuse);

	private string EnergyIconPath => VisualCardPool.EnergyIconPath;

	public Texture2D EnergyIcon => ResourceLoader.Load<Texture2D>(EnergyIconPath, null, ResourceLoader.CacheMode.Reuse);

	protected IHoverTip EnergyHoverTip => HoverTipFactory.ForEnergy(this);

	private string BannerTexturePath
	{
		get
		{
			if (Rarity != CardRarity.Ancient)
			{
				return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/card_banner.tres");
			}
			return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/card_banner_ancient_s.tres");
		}
	}

	public Texture2D BannerTexture => ResourceLoader.Load<Texture2D>(BannerTexturePath, null, ResourceLoader.CacheMode.Reuse);

	private string BannerMaterialPath => Rarity switch
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

	public Material FrameMaterial => VisualCardPool.FrameMaterial;

	public virtual CardType Type { get; }

	public virtual CardRarity Rarity { get; }

	/// <summary>
	/// Manages card restrictions based on how many players there are in a run.
	/// </summary>
	public virtual CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.None;

	public virtual CardPoolModel Pool
	{
		get
		{
			if (_pool != null)
			{
				return _pool;
			}
			_pool = ModelDb.AllCardPools.FirstOrDefault((CardPoolModel pool) => pool.AllCardIds.Contains(base.Id));
			if (_pool != null)
			{
				return _pool;
			}
			if (ModelDb.CardPool<MockCardPool>().AllCardIds.Contains(base.Id))
			{
				_pool = ModelDb.CardPool<MockCardPool>();
				return _pool;
			}
			throw new InvalidProgramException($"Card {this} is not in any card pool!");
		}
	}

	/// <summary>
	/// Visually what pool we want the card to be from. Normally this is the same as the regular pool.
	/// It is not the case when we want it to be part of one pool, but look like its from another pool
	/// ie Trash Heap cards are event cards, but we want the colors to reflect their original character cards from sts1
	/// </summary>
	public virtual CardPoolModel VisualCardPool => Pool;

	/// <summary>
	/// Get the Player that this card belongs to.
	/// Will technically be null on a canonical card model and in certain edge-case timing moments (end of combat before
	/// transitioning to the next room), but we should rarely be checking that, so we leave this as non-nullable for
	/// convenience in 99% of cases.
	/// If you really need to check for null, override the warning with a comment.
	/// </summary>
	public Player Owner
	{
		get
		{
			AssertMutable();
			return _owner;
		}
		set
		{
			AssertMutable();
			if (_owner != null && value != null)
			{
				throw new InvalidOperationException("Card " + base.Id.Entry + " already has an owner.");
			}
			_owner = value;
		}
	}

	/// <summary>
	/// The pile which this card is in.
	/// Can be null when used by Card HoverTips, Card Rewards, Card Library, things like that.
	/// </summary>
	public CardPile? Pile => _owner?.Piles.FirstOrDefault((CardPile p) => p.Cards.Contains(this));

	/// <summary>
	/// This card's "official" starting energy cost.
	/// This is what would appear on the card if it was printed out on paper.
	///
	/// Note: If you want to check a card's canonical energy cost, use <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.Canonical" /> instead.
	/// </summary>
	protected virtual int CanonicalEnergyCost { get; }

	/// <summary>
	/// Whether this card has an energy cost of X.
	/// X-cost-cards automatically spend all of the player's remaining energy when played, and their effect is
	/// multiplied by the amount spent.
	///
	/// Note: This exists on CardModel for the purposes of overriding in subclasses. If you want to check if a card has
	/// an energy cost of X, use <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.CostsX" /> instead.
	/// </summary>
	protected virtual bool HasEnergyCostX => false;

	/// <summary>
	/// All energy-cost-related information and logic about this card.
	/// </summary>
	public CardEnergyCost EnergyCost
	{
		get
		{
			if (_energyCost == null)
			{
				_energyCost = new CardEnergyCost(this, CanonicalEnergyCost, HasEnergyCostX);
			}
			return _energyCost;
		}
	}

	/// <summary>
	/// The number of extra times this card's logic should be executed when it's played, excluding any effects from
	/// other models.
	/// Defaults to 0, but various effects can permanently set this to a higher value.
	/// </summary>
	public int BaseReplayCount
	{
		get
		{
			return _baseReplayCount;
		}
		set
		{
			AssertMutable();
			_baseReplayCount = value;
			this.ReplayCountChanged?.Invoke();
		}
	}

	public virtual int CanonicalStarCost => -1;

	public int BaseStarCost
	{
		get
		{
			if (!base.IsMutable)
			{
				return CanonicalStarCost;
			}
			if (!_starCostSet)
			{
				_baseStarCost = CanonicalStarCost;
				_starCostSet = true;
			}
			return _baseStarCost;
		}
		private set
		{
			AssertMutable();
			if (!HasStarCostX)
			{
				_baseStarCost = value;
				_starCostSet = true;
			}
			this.StarCostChanged?.Invoke();
		}
	}

	public bool WasStarCostJustUpgraded => _wasStarCostJustUpgraded;

	public TemporaryCardCost? TemporaryStarCost => _temporaryStarCosts.LastOrDefault();

	/// <summary>
	/// Get this card's current star cost.
	///
	/// This works just like <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.GetWithModifiers(MegaCrit.Sts2.Core.Entities.Cards.CostModifiers)" /> passing <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.Local" /> but
	/// for stars, and with one exception.
	/// If the card had no star cost (it was negative) and it is temporarily set to zero, then we still treat the card
	/// as if it has no star cost, so that it still doesn't show up on the card.
	/// </summary>
	public virtual int CurrentStarCost
	{
		get
		{
			int? num = _temporaryStarCosts.LastOrDefault()?.Cost;
			if (num.HasValue)
			{
				if (num == 0 && BaseStarCost < 0)
				{
					return BaseStarCost;
				}
				return num.Value;
			}
			return BaseStarCost;
		}
	}

	public virtual bool HasStarCostX => false;

	/// <summary>
	/// The amount of stars most recently spent to play this card.
	/// Used when duplicating X-cost cards, to make sure the duplicates are played with the same value.
	///
	/// WARNING: Only use this for calculations related to stars spent. If you're using this to calculate a cost-X-stars
	/// card's effect, use <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.ResolveStarXValue" /> instead, as it will take X-value modifications (like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.ChemicalX" />) into account.
	/// </summary>
	public int LastStarsSpent
	{
		get
		{
			return _lastStarsSpent;
		}
		set
		{
			AssertMutable();
			_lastStarsSpent = value;
		}
	}

	/// <summary>
	/// Defines who what creature we should be targeting/highlighting. Some examples:
	/// - AnyEnemy means that this is a single target attack.
	/// - AllEnemies: this is an AOE attack so highlight all valid enemies
	/// - Self: This is a power applied to the player, highlight the players character
	/// </summary>
	public virtual TargetType TargetType { get; }

	public virtual IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	/// <summary>
	/// The keywords applied directly to this card: its canonical keywords, plus any added by <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.AddKeyword(MegaCrit.Sts2.Core.Entities.Cards.CardKeyword)" />,
	/// minus any removed by <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.RemoveKeyword(MegaCrit.Sts2.Core.Entities.Cards.CardKeyword)" />. This is the persistent, instance-owned keyword state; it is
	/// what gets cloned, and is unaffected by other models in the combat state.
	/// See <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.KeywordSources.Local" /> for more details.
	/// </summary>
	private HashSet<CardKeyword> LocalKeywords
	{
		get
		{
			if (_keywords != null)
			{
				return _keywords;
			}
			_keywords = new HashSet<CardKeyword>();
			_keywords.UnionWith(CanonicalKeywords);
			return _keywords;
		}
	}

	/// <summary>
	/// This card's current keywords, including both local keywords (<see cref="F:MegaCrit.Sts2.Core.Entities.Cards.KeywordSources.Local" />) and any
	/// global keywords (<see cref="F:MegaCrit.Sts2.Core.Entities.Cards.KeywordSources.Global" />) granted by other models in the combat state (e.g.
	/// Ethereal from <see cref="T:MegaCrit.Sts2.Core.Models.Powers.HexPower" />). See <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.GetKeywordsWithSources(MegaCrit.Sts2.Core.Entities.Cards.KeywordSources)" />.
	/// </summary>
	public IReadOnlySet<CardKeyword> Keywords => GetKeywordsWithSources(KeywordSources.All);

	/// <summary>
	/// This card's tags. See <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CardTag" /> for details on how to use these.
	///
	/// NOTE: The current implementation assumes a card's tags will never change. If we ever need to dynamically add or
	/// remove tags after creation, change this to work more like Keywords.
	/// </summary>
	public virtual IEnumerable<CardTag> Tags => _tags ?? (_tags = CanonicalTags);

	protected virtual HashSet<CardTag> CanonicalTags => new HashSet<CardTag>();

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

	/// <summary>
	/// This is used for cards like Havoc and Cinder to mark cards that should be sent to the exhaust pile instead
	/// of the discard pile on their next play.
	///
	/// TODO: This might be kind of a hacky solution. We should look into other ways to pass this info along.
	/// </summary>
	public bool ExhaustOnNextPlay
	{
		get
		{
			return _exhaustOnNextPlay;
		}
		set
		{
			AssertMutable();
			_exhaustOnNextPlay = value;
		}
	}

	private bool HasSingleTurnRetain
	{
		get
		{
			return _hasSingleTurnRetain;
		}
		set
		{
			AssertMutable();
			_hasSingleTurnRetain = value;
		}
	}

	/// <summary>
	/// Should this card be retained this turn?
	/// True if the card has the Retain keyword, or if some effect like Well-Laid Plans has made it retain for a single
	/// turn and it's still that turn.
	/// </summary>
	public bool ShouldRetainThisTurn
	{
		get
		{
			if (!Keywords.Contains(CardKeyword.Retain))
			{
				return HasSingleTurnRetain;
			}
			return true;
		}
	}

	private bool HasSingleTurnSly
	{
		get
		{
			return _hasSingleTurnSly;
		}
		set
		{
			AssertMutable();
			_hasSingleTurnSly = value;
		}
	}

	/// <summary>
	/// Is this card Sly this turn?
	/// True if the card has the Sly keyword, or if some effect like Hand Trick has made it Sly for a single turn.
	/// </summary>
	public bool IsSlyThisTurn
	{
		get
		{
			if (!Keywords.Contains(CardKeyword.Sly))
			{
				return HasSingleTurnSly;
			}
			return true;
		}
	}

	public EnchantmentModel? Enchantment { get; private set; }

	public AfflictionModel? Affliction { get; private set; }

	/// <summary>
	/// Whether or not playing this card can heal the player or their pets, either directly (like Feed) or by giving
	/// you something else that can heal you (like Alchemize giving you a Regen Potion).
	///
	/// Used primarily to filter cards out of random in-combat generation effects, to avoid making annoying
	/// "optimal play" behavior.
	/// </summary>
	public virtual bool CanBeGeneratedInCombat => true;

	/// <summary>
	/// Used to filter cards out of modifier pools. Ancient curses and Ascender's Bane should not be generated by Cursed
	/// Run.
	/// </summary>
	public virtual bool CanBeGeneratedByModifiers => true;

	/// <summary>
	/// Manages if and how a card will Evoke an Orb.
	/// Applies to cards like Dualcast, which say "Evoke your next Orb".
	/// Does NOT apply to cards that may evoke an Orb as a side-effect (like Zap's channeling causing an Orb to evoke).
	///
	/// Used primarily to visually update the numbers displayed on your Orbs to reflect their Evoke values while
	/// dragging a card.
	/// </summary>
	public virtual OrbEvokeType OrbEvokeType => OrbEvokeType.None;

	/// <summary>
	/// Whether or not playing this card gains you block immediately.
	/// True for cards like Defend and Survivor.
	/// False for cards like Shadowmeld (which doesn't gain you block until you play an attack after).
	///
	/// Used for things like filtering the Nimble enchantment's targets.
	/// Also automatically sets the block HoverTip.
	///
	/// Note: Don't convert this into a CardTag. It's used by the system for automatic Osty targeting behavior.
	/// </summary>
	public virtual bool GainsBlock => false;

	/// <summary>
	/// Is this card one of the basic Strikes or Defends that you start the game with?
	///
	/// Used for things like Pandora's Box that only operate on these specific starter cards.
	/// </summary>
	public virtual bool IsBasicStrikeOrDefend
	{
		get
		{
			if (Rarity != CardRarity.Basic)
			{
				return false;
			}
			if (Tags.Contains(CardTag.Strike))
			{
				return true;
			}
			if (Tags.Contains(CardTag.Defend))
			{
				return true;
			}
			return false;
		}
	}

	public CardModel? CloneOf => _cloneOf;

	/// <summary>
	/// Whether or not this card is considered a clone. See <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.CloneOf" /> for more info on clones.
	/// </summary>
	public bool IsClone => CloneOf != null;

	/// <summary>
	/// The card that this is a duplicate of. Null when this card is not a duplicate (which is most of the time).
	///
	/// Dupes behave slightly differently from original cards in some situations. For example:
	/// * After playing a dupe, it is sent to the Limbo pile, rather than Discard/Exhaust.
	/// * Dupes cannot be further duplicated by effects like Duplication Potion.
	/// * If an X-cost card is duped, the dupe retains the X-value from the original.
	/// </summary>
	public CardModel? DupeOf
	{
		get
		{
			if (!IsDupe)
			{
				return null;
			}
			return CloneOf;
		}
	}

	public bool IsDupe
	{
		get
		{
			return _isDupe;
		}
		private set
		{
			AssertMutable();
			_isDupe = value;
		}
	}

	public bool IsRemovable => !Keywords.Contains(CardKeyword.Eternal);

	public bool IsTransformable
	{
		get
		{
			if (!IsRemovable)
			{
				CardPile pile = Pile;
				return pile == null || pile.Type != PileType.Deck;
			}
			return true;
		}
	}

	public bool IsInCombat
	{
		get
		{
			if (base.IsMutable)
			{
				return Pile?.IsCombatPile ?? false;
			}
			return false;
		}
	}

	public int CurrentUpgradeLevel
	{
		get
		{
			return _currentUpgradeLevel;
		}
		private set
		{
			AssertMutable();
			if (value > MaxUpgradeLevel)
			{
				throw new InvalidOperationException($"{base.Id} cannot be upgraded past its MaxUpgradeLevel.");
			}
			_currentUpgradeLevel = value;
		}
	}

	public virtual int MaxUpgradeLevel => 1;

	public bool IsUpgraded => CurrentUpgradeLevel > 0;

	public bool IsUpgradable
	{
		get
		{
			if (CurrentUpgradeLevel >= MaxUpgradeLevel)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Is this card currently appearing as a previewed upgrade in <see cref="T:MegaCrit.Sts2.Core.Nodes.Cards.NUpgradePreview" />?
	/// And if so, what type of upgrade preview is it?
	/// This is to facilitate having upgrade previews reflect power values from the player in combat (i.e. Armaments).
	/// </summary>
	public CardUpgradePreviewType UpgradePreviewType
	{
		get
		{
			return _upgradePreviewType;
		}
		set
		{
			AssertMutable();
			if (!value.IsPreview() && _upgradePreviewType.IsPreview())
			{
				throw new InvalidOperationException("A card cannot go to from being upgrade preview. Consider making a new card model instead.");
			}
			_upgradePreviewType = value;
		}
	}

	/// <summary>
	/// Override this property to add extra conditions to check before allowing play.
	/// For example, Grand Finale is only playable if your draw pile is empty, so it would override this.
	/// </summary>
	protected virtual bool IsPlayable => true;

	/// <summary>
	/// Set via constructor parameter to block a card from appearing in the Card Library screen.
	/// </summary>
	public bool ShouldShowInCardLibrary { get; }

	public bool ShouldGlowGold
	{
		get
		{
			if (!ShouldGlowGoldInternal)
			{
				return Enchantment?.ShouldGlowGold ?? false;
			}
			return true;
		}
	}

	public bool ShouldGlowRed
	{
		get
		{
			if (!ShouldGlowRedInternal)
			{
				return Enchantment?.ShouldGlowRed ?? false;
			}
			return true;
		}
	}

	/// <summary>
	/// Override this property to add conditions to check to determine whether to show a gold glow on this card.
	/// For example, Evil Eye adds 6 extra block if you've exhausted a card this turn, so it would override this.
	/// </summary>
	protected virtual bool ShouldGlowGoldInternal => false;

	/// <summary>
	/// Override this property to add conditions to check to determine whether to show a red glow on this card.
	/// For example, Normality should glow red when it blocks card plays.
	/// </summary>
	protected virtual bool ShouldGlowRedInternal => false;

	/// <summary>
	/// Is this card currently appearing as a previewed enchanted card in <see cref="T:MegaCrit.Sts2.Core.Nodes.Cards.NEnchantPreview" />?
	/// </summary>
	public bool IsEnchantmentPreview
	{
		get
		{
			return _isEnchantmentPreview;
		}
		set
		{
			AssertMutable();
			_isEnchantmentPreview = value;
		}
	}

	public virtual bool HasBuiltInOverlay => false;

	public string OverlayPath => SceneHelper.GetScenePath("cards/overlays/" + base.Id.Entry.ToLowerInvariant());

	public int? FloorAddedToDeck
	{
		get
		{
			return _floorAddedToDeck;
		}
		set
		{
			AssertMutable();
			_floorAddedToDeck = value;
		}
	}

	public Creature? CurrentTarget
	{
		get
		{
			return _currentTarget;
		}
		private set
		{
			AssertMutable();
			_currentTarget = value;
		}
	}

	/// <summary>
	/// Index of the play currently in progress while this card is being played (0 = first play, 1 = first Replay,
	/// etc.). Lets damage previews reflect mid-Replay state before the current play's History entry has been logged.
	/// </summary>
	public int CurrentPlayIndex
	{
		get
		{
			return _currentPlayIndex;
		}
		private set
		{
			AssertMutable();
			_currentPlayIndex = value;
		}
	}

	public CardModel? DeckVersion
	{
		get
		{
			return _deckVersion;
		}
		set
		{
			AssertMutable();
			_deckVersion = value;
		}
	}

	/// <summary>
	/// Set to true when this card is removed from the combat/run state (happens pretty rarely, like when a card is
	/// transformed or removed from the player's deck).
	/// Set to false when the card is added _back_ to the combat/run state (even more rarely, like when
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.ThievingHopper" /> steals a card and then you add it back to your deck later).
	/// </summary>
	public bool HasBeenRemovedFromState { get; set; }

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public IEnumerable<IHoverTip> HoverTips
	{
		get
		{
			List<IHoverTip> list = ExtraHoverTips.ToList();
			if (Enchantment != null)
			{
				list.AddRange(Enchantment.HoverTips);
			}
			if (Affliction != null)
			{
				list.AddRange(Affliction.HoverTips);
			}
			int enchantedReplayCount = GetEnchantedReplayCount();
			if (enchantedReplayCount > 0)
			{
				list.Add(HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", enchantedReplayCount)));
			}
			if (OrbEvokeType != OrbEvokeType.None)
			{
				list.Add(HoverTipFactory.Static(StaticHoverTip.Evoke));
			}
			if (GainsBlock)
			{
				list.Add(HoverTipFactory.Static(StaticHoverTip.Block));
			}
			foreach (CardKeyword keyword in Keywords)
			{
				list.Add(HoverTipFactory.FromKeyword(keyword));
				if (keyword == CardKeyword.Ethereal)
				{
					list.Add(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));
				}
			}
			return list.Distinct();
		}
	}

	public CardModel CanonicalInstance
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

	/// <summary>
	/// The state of the run that this card exists in.
	/// Null for cards that exist outside of a run (in the Compendium, in preview HoverTips, etc.)
	/// </summary>
	public IRunState? RunState => _owner?.RunState;

	/// <summary>
	/// The state of the combat that this card exists in.
	/// Null for cards that exist outside of a combat (deck cards, cards offered in rewards or at events, etc.)
	/// </summary>
	public ICombatState? CombatState
	{
		get
		{
			CardPile pile = Pile;
			if ((pile != null && pile.IsCombatPile) || UpgradePreviewType == CardUpgradePreviewType.Combat)
			{
				return _owner?.Creature.CombatState;
			}
			return null;
		}
	}

	/// <summary>
	/// The lowest-level scope that this card exists in.
	/// Combat takes precedence over run, since all cards in a run have a <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.RunState" />, but cards that have been added
	/// to combat also have a CombatState.
	/// Note: For operations that need a scope during combat, we check the owner's CombatState as a fallback because
	/// CombatState is null for cards in non-combat piles (like the deck).
	/// </summary>
	public ICardScope? CardScope => ((ICardScope)CombatState) ?? ((ICardScope)(_owner?.Creature.CombatState)) ?? RunState;

	/// <summary>
	/// Override this property to run this card's <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnTurnEndInHand(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext)" /> when the player ends the turn with this
	/// card in their hand.
	/// </summary>
	public virtual bool HasTurnEndInHandEffect => false;

	public override bool ShouldReceiveCombatHooks => Pile?.IsCombatPile ?? false;

	public virtual IEnumerable<string> AllPortraitPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(PortraitPath);

	/// <summary>
	/// Returns all card-related asset paths for a run.
	/// </summary>
	public IEnumerable<string> RunAssetPaths => ExtraRunAssetPaths;

	/// <summary>
	/// Cards can define VFX that are displayed in combat here.
	/// These are not preloaded when only the card would be displayed, e.g. in the compendium.
	/// </summary>
	protected virtual IEnumerable<string> ExtraRunAssetPaths => Array.Empty<string>();

	public event Action? AfflictionChanged;

	public event Action? EnchantmentChanged;

	public event Action? EnergyCostChanged;

	public event Action? KeywordsChanged;

	public event Action? ReplayCountChanged;

	public event Action? Played;

	public event Action? Drawn;

	public event Action? StarCostChanged;

	public event Action? Upgraded;

	public event Action? Forged;

	/// <summary>
	/// These values are constructor parameters rather than abstract properties to avoid virtual dispatch.
	/// Most cards have constant values for these properties, so storing them in fields eliminates the
	/// overhead of virtual method calls on every access. Cards with dynamic behavior (e.g., MadScience)
	/// can still override the virtual properties.
	/// </summary>
	protected CardModel(int canonicalEnergyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true)
	{
		CanonicalEnergyCost = canonicalEnergyCost;
		Type = type;
		Rarity = rarity;
		TargetType = targetType;
		ShouldShowInCardLibrary = shouldShowInCardLibrary;
	}

	/// <summary>
	/// WARNING: Only use this in tests.
	/// Set this card's energy cost.
	/// </summary>
	protected void MockSetEnergyCost(CardEnergyCost cost)
	{
		_energyCost = cost;
	}

	/// <summary>
	/// Internal method for <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost" /> to invoke the EnergyCostChanged event.
	/// </summary>
	public void InvokeEnergyCostChanged()
	{
		this.EnergyCostChanged?.Invoke();
	}

	/// <summary>
	/// Resolve this card's X energy value.
	/// Takes modifications to X values (like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.ChemicalX" />) into account.
	/// </summary>
	public int ResolveEnergyXValue()
	{
		if (!EnergyCost.CostsX)
		{
			throw new InvalidOperationException("This card does not have an X-cost.");
		}
		return Hook.ModifyXValue(CombatState, this, EnergyCost.CapturedXValue);
	}

	/// <summary>
	/// The number of extra times this card's logic should be executed when it's played, including any effects from
	/// this card's enchantment if it has one.
	/// </summary>
	public int GetEnchantedReplayCount()
	{
		return Enchantment?.EnchantPlayCount(BaseReplayCount) ?? BaseReplayCount;
	}

	/// <summary>
	/// Resolve this card's X star value.
	/// Takes modifications to X values (like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.ChemicalX" />) into account.
	/// </summary>
	public int ResolveStarXValue()
	{
		if (!HasStarCostX)
		{
			throw new InvalidOperationException("This card does not have an X-cost.");
		}
		return Hook.ModifyXValue(CombatState, this, LastStarsSpent);
	}

	/// <summary>
	/// Get this card's keywords, including the specified source types.
	/// This mirrors <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.GetWithModifiers(MegaCrit.Sts2.Core.Entities.Cards.CostModifiers)" />: local keywords live on the card, while global
	/// keywords are computed on demand from other models in the combat state and are never stored.
	/// </summary>
	public IReadOnlySet<CardKeyword> GetKeywordsWithSources(KeywordSources sources)
	{
		bool flag = sources.HasFlag(KeywordSources.Local);
		if (!sources.HasFlag(KeywordSources.Global) || base.IsCanonical || CombatState == null)
		{
			if (!flag)
			{
				return ImmutableHashSet<CardKeyword>.Empty;
			}
			return LocalKeywords;
		}
		HashSet<CardKeyword> hashSet2;
		if (flag)
		{
			HashSet<CardKeyword> hashSet = new HashSet<CardKeyword>();
			foreach (CardKeyword localKeyword in LocalKeywords)
			{
				hashSet.Add(localKeyword);
			}
			hashSet2 = hashSet;
		}
		else
		{
			hashSet2 = new HashSet<CardKeyword>();
		}
		HashSet<CardKeyword> hashSet3 = hashSet2;
		Hook.ModifyKeywordsInCombat(CombatState, this, hashSet3);
		return hashSet3;
	}

	public Control CreateOverlay()
	{
		return PreloadManager.Cache.GetScene(OverlayPath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
	}

	public CardModel ToMutable()
	{
		AssertCanonical();
		return (CardModel)MutableClone();
	}

	protected override void DeepCloneFields()
	{
		HashSet<CardKeyword> hashSet = new HashSet<CardKeyword>();
		foreach (CardKeyword keywordsWithSource in GetKeywordsWithSources(KeywordSources.Local))
		{
			hashSet.Add(keywordsWithSource);
		}
		_keywords = hashSet;
		_dynamicVars = DynamicVars.Clone(this);
		_energyCost = _energyCost?.Clone(this);
		_temporaryStarCosts = _temporaryStarCosts.ToList();
		if (Enchantment != null)
		{
			EnchantmentModel enchantmentModel = (EnchantmentModel)Enchantment.ClonePreservingMutability();
			Enchantment = null;
			EnchantInternal(enchantmentModel, enchantmentModel.Amount);
		}
		if (Affliction != null)
		{
			AfflictionModel afflictionModel = (AfflictionModel)Affliction.ClonePreservingMutability();
			Affliction = null;
			AfflictInternal(afflictionModel, afflictionModel.Amount);
		}
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		if (_canonicalInstance == null)
		{
			_canonicalInstance = ModelDb.GetById<CardModel>(base.Id);
		}
		CurrentTarget = null;
		CurrentPlayIndex = 0;
		DeckVersion = null;
		HasBeenRemovedFromState = false;
		this.AfflictionChanged = null;
		this.Drawn = null;
		this.EnchantmentChanged = null;
		this.EnergyCostChanged = null;
		this.Forged = null;
		this.KeywordsChanged = null;
		this.Played = null;
		this.ReplayCountChanged = null;
		this.StarCostChanged = null;
		this.Upgraded = null;
	}

	/// <summary>
	/// Extra logic that should be run after the card is created (NOT during deserialization).
	/// At this point, the card will have an owner, and will be in a <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.CombatState" /> or <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.RunState" />.
	/// </summary>
	public virtual void AfterCreated()
	{
	}

	/// <summary>
	/// Extra logic that should be run after deserializing.
	/// This should rarely be overridden, just for very unusual cards like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.MadScience" />.
	/// </summary>
	protected virtual void AfterDeserialized()
	{
	}

	protected void NeverEverCallThisOutsideOfTests_ClearOwner()
	{
		if (TestMode.IsOff)
		{
			throw new InvalidOperationException("You monster!");
		}
		_owner = null;
	}

	public void SetToFreeThisTurn()
	{
		EnergyCost.SetThisTurnOrUntilPlayed(0);
		SetStarCostThisTurn(0);
	}

	public void SetToFreeThisCombat()
	{
		EnergyCost.SetThisCombat(0);
		SetStarCostThisCombat(0);
	}

	public void SetStarCostUntilPlayed(int cost)
	{
		AddTemporaryStarCost(TemporaryCardCost.UntilPlayed(cost));
	}

	public void SetStarCostThisTurn(int cost)
	{
		AddTemporaryStarCost(TemporaryCardCost.ThisTurn(cost));
	}

	public void SetStarCostThisCombat(int cost)
	{
		AddTemporaryStarCost(TemporaryCardCost.ThisCombat(cost));
	}

	public int GetStarCostThisCombat()
	{
		return _temporaryStarCosts.FirstOrDefault((TemporaryCardCost cost) => cost != null && !cost.ClearsWhenTurnEnds && !cost.ClearsWhenCardIsPlayed)?.Cost ?? BaseStarCost;
	}

	private void AddTemporaryStarCost(TemporaryCardCost cost)
	{
		AssertMutable();
		_temporaryStarCosts.Add(cost);
		this.StarCostChanged?.Invoke();
	}

	/// <summary>
	/// Upgrade the star cost of this card by the specified amount.
	/// This is meant to be called in OnUpgrade.
	/// </summary>
	/// <param name="addend">Amount to add to the current cost (usually negative).</param>
	protected void UpgradeStarCostBy(int addend)
	{
		if (HasStarCostX)
		{
			throw new InvalidOperationException("UpgradeStarCostBy called on " + base.Id.Entry + " which has star cost X.");
		}
		if (addend == 0)
		{
			return;
		}
		int baseStarCost = BaseStarCost;
		BaseStarCost += addend;
		_wasStarCostJustUpgraded = true;
		if (BaseStarCost < baseStarCost)
		{
			_temporaryStarCosts.RemoveAll((TemporaryCardCost c) => c.Cost > BaseStarCost);
		}
	}

	public void AddKeyword(CardKeyword keyword)
	{
		AssertMutable();
		LocalKeywords.Add(keyword);
		this.KeywordsChanged?.Invoke();
	}

	public void RemoveKeyword(CardKeyword keyword)
	{
		AssertMutable();
		LocalKeywords.Remove(keyword);
		this.KeywordsChanged?.Invoke();
	}

	/// <summary>
	/// Set this card to be retained this turn.
	/// Will be cleared at the end of the turn.
	/// </summary>
	public void GiveSingleTurnRetain()
	{
		HasSingleTurnRetain = true;
	}

	/// <summary>
	/// Set this card to be Sly this turn.
	/// Will be cleared at the end of the turn.
	/// </summary>
	public void GiveSingleTurnSly()
	{
		HasSingleTurnSly = true;
	}

	public string GetDescriptionForPile(PileType pileType, Creature? target = null)
	{
		return GetDescriptionForPile(pileType, DescriptionPreviewType.None, target);
	}

	public string GetDescriptionForUpgradePreview()
	{
		return GetDescriptionForPile(PileType.None, DescriptionPreviewType.Upgrade);
	}

	private string GetDescriptionForPile(PileType pileType, DescriptionPreviewType previewType, Creature? target = null)
	{
		LocString description = Description;
		DynamicVars.AddTo(description);
		AddExtraArgsToDescription(description);
		UpgradeDisplay upgradeDisplay = ((previewType == DescriptionPreviewType.Upgrade) ? UpgradeDisplay.UpgradePreview : (IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal));
		description.Add(new IfUpgradedVar(upgradeDisplay));
		bool flag = ((pileType == PileType.Hand || pileType == PileType.Play) ? true : false);
		bool variable = flag;
		description.Add("OnTable", variable);
		bool variable2 = CombatManager.Instance.IsInProgress && (Pile?.IsCombatPile ?? pileType.IsCombatPile());
		description.Add("InCombat", variable2);
		description.Add("IsTargeting", target != null);
		description.Add("TargetType", TargetType.ToString());
		description.Add("GainsBlock", GainsBlock);
		description.Add("IsOstyAlive", base.IsMutable && (Owner?.IsOstyAlive ?? false));
		string prefix = EnergyIconHelper.GetPrefix(this);
		description.Add("energyPrefix", prefix);
		description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
		foreach (KeyValuePair<string, object> variable3 in description.Variables)
		{
			if (variable3.Value is EnergyVar energyVar)
			{
				energyVar.ColorPrefix = prefix;
			}
		}
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = description.GetFormattedText();
		List<string> list2 = list;
		LocString locString = Enchantment?.DynamicExtraCardText;
		if (locString != null)
		{
			list2.Add("[purple]" + locString.GetFormattedText() + "[/purple]");
		}
		LocString locString2 = Affliction?.DynamicExtraCardText;
		if (locString2 != null)
		{
			list2.Add("[purple]" + locString2.GetFormattedText() + "[/purple]");
		}
		IReadOnlySet<CardKeyword> keywords = Keywords;
		CardKeyword[] beforeDescription = CardKeywordOrder.beforeDescription;
		foreach (CardKeyword cardKeyword in beforeDescription)
		{
			if (cardKeyword switch
			{
				CardKeyword.Sly => IsSlyThisTurn, 
				CardKeyword.Retain => ShouldRetainThisTurn, 
				_ => keywords.Contains(cardKeyword), 
			})
			{
				list2.Insert(0, cardKeyword.GetCardText());
			}
		}
		int enchantedReplayCount = GetEnchantedReplayCount();
		if (enchantedReplayCount > 0)
		{
			LocString locString3 = new LocString("static_hover_tips", "REPLAY.extraText");
			locString3.Add("Times", enchantedReplayCount);
			list2.Add(locString3.GetFormattedText() ?? "");
		}
		foreach (CardKeyword item in CardKeywordOrder.afterDescription.Intersect(keywords))
		{
			list2.Add(item.GetCardText());
		}
		return string.Join('\n', list2.Where((string l) => !string.IsNullOrEmpty(l)));
	}

	/// <summary>
	/// Updates the dynamic variables of this card based on hooks (i.e damage, block, and powers).
	/// This is so powers and relic modifications to these values can be reflected in card descriptions.
	/// </summary>
	/// <param name="previewMode">
	/// The type of preview to show in the card's visuals. See <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CardPreviewMode" /> for details.
	/// </param>
	/// <param name="target">Creature who this card is targeting, if it exists.</param>
	/// <param name="dynamicVarSet">The dynamic variables for this card.</param>
	public void UpdateDynamicVarPreview(CardPreviewMode previewMode, Creature? target, DynamicVarSet dynamicVarSet)
	{
		if (RunState == null && CombatState == null)
		{
			return;
		}
		bool flag = CombatState != null;
		bool flag2 = flag;
		if (flag2)
		{
			bool flag3;
			switch (Pile?.Type)
			{
			case PileType.Hand:
			case PileType.Play:
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = flag3 || UpgradePreviewType == CardUpgradePreviewType.Combat;
		}
		bool runGlobalHooks = flag2;
		IEnumerable<DynamicVar> enumerable = dynamicVarSet.Values.ToList();
		foreach (DynamicVar item in enumerable)
		{
			item.UpdateCardPreview(this, previewMode, target, runGlobalHooks);
		}
	}

	/// <summary>
	/// Add an Enchantment to this card.
	/// </summary>
	/// <remarks>
	/// You should generally use CardCommands.Enchant instead of this method. If you do use this method, you may need to
	/// call <see cref="M:MegaCrit.Sts2.Core.Models.EnchantmentModel.ModifyCard" /> after.
	/// </remarks>
	/// <param name="enchantment">Enchantment to add.</param>
	/// <param name="amount">Amount to set on the added enchantment.</param>
	public void EnchantInternal(EnchantmentModel enchantment, decimal amount)
	{
		AssertMutable();
		enchantment.AssertMutable();
		Enchantment = enchantment;
		Enchantment.ApplyInternal(this, amount);
		this.EnchantmentChanged?.Invoke();
	}

	/// <summary>
	/// Add an Affliction to this card.
	/// </summary>
	/// <remarks>You should generally use CardCmd.Afflict instead of this method.</remarks>
	/// <param name="affliction">Affliction to add.</param>
	/// <param name="amount">Amount to set on the added affliction</param>
	/// <returns>Whether or not adding the affliction was successful.</returns>
	public void AfflictInternal(AfflictionModel affliction, decimal amount)
	{
		AssertMutable();
		affliction.AssertMutable();
		if (Affliction != null)
		{
			throw new InvalidOperationException($"Attempted to afflict card {this} that was already afflicted! This is not allowed");
		}
		Affliction = affliction;
		Affliction.Card = this;
		Affliction.Amount = (int)amount;
		this.AfflictionChanged?.Invoke();
	}

	public void ClearEnchantmentInternal()
	{
		if (Enchantment != null)
		{
			AssertMutable();
			Enchantment.ClearInternal();
			Enchantment = null;
			this.EnchantmentChanged?.Invoke();
		}
	}

	public void ClearAfflictionInternal()
	{
		AssertMutable();
		if (Affliction != null)
		{
			Affliction.ClearInternal();
			Affliction = null;
			Owner.PlayerCombatState.RecalculateCardValues();
			this.AfflictionChanged?.Invoke();
		}
	}

	protected virtual void AddExtraArgsToDescription(LocString description)
	{
	}

	/// <summary>
	/// Get this card's current star cost, including all modifiers.
	/// Usually, this will just be the same as CurrentStarCost, but there are 2 exceptions:
	/// 1. X-cost cards will return the amount of stars that will be spent to play them instead of a 0 placeholder value.
	/// 2. Effects that modify star costs will be reflected here.
	/// </summary>
	/// <returns>Current energy cost including modifiers.</returns>
	public int GetStarCostWithModifiers()
	{
		if (HasStarCostX)
		{
			return Owner.PlayerCombatState?.Stars ?? 0;
		}
		CardPile pile = Pile;
		if (pile != null && pile.IsCombatPile && CombatState != null)
		{
			return (int)Hook.ModifyStarCost(CombatState, this, CurrentStarCost);
		}
		return CurrentStarCost;
	}

	/// <summary>
	/// Does this card have an energy or star (or both) cost?
	/// Used by effects that make cards totally free (no energy OR star cost), to avoid selecting cards that are already
	/// free.
	/// </summary>
	/// <param name="includeGlobalModifiers">
	/// Whether to include global modifiers in the cost.
	/// See <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.Global" /> for details.
	/// </param>
	public bool CostsEnergyOrStars(bool includeGlobalModifiers)
	{
		if (includeGlobalModifiers)
		{
			if (!EnergyCost.CostsX && EnergyCost.GetWithModifiers(CostModifiers.All) > 0)
			{
				return true;
			}
			if (!HasStarCostX && GetStarCostWithModifiers() > 0)
			{
				return true;
			}
		}
		else if (EnergyCost.GetWithModifiers(CostModifiers.Local) > 0 || CurrentStarCost > 0)
		{
			return true;
		}
		return false;
	}

	public void RemoveFromCurrentPile(bool silent = false)
	{
		AssertMutable();
		Pile?.RemoveInternal(this, silent);
	}

	public void RemoveFromState()
	{
		RemoveFromCurrentPile();
		HasBeenRemovedFromState = true;
	}

	public void EndOfTurnCleanup()
	{
		ExhaustOnNextPlay = false;
		HasSingleTurnRetain = false;
		HasSingleTurnSly = false;
		if (EnergyCost.EndOfTurnCleanup())
		{
			this.EnergyCostChanged?.Invoke();
		}
		if (_temporaryStarCosts.RemoveAll((TemporaryCardCost c) => c.ClearsWhenTurnEnds) > 0)
		{
			this.StarCostChanged?.Invoke();
		}
	}

	public virtual void AfterTransformedFrom()
	{
	}

	public virtual void AfterTransformedTo()
	{
	}

	public void AfterForged()
	{
		this.Forged?.Invoke();
	}

	/// <summary>
	/// Override this method for when this card is played.
	/// </summary>
	/// <param name="choiceContext"></param>
	/// <param name="cardPlay">The CardPlay that is being executed.</param>
	protected virtual Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Override this method to add VFX that should run the moment the mouse is released when playing this card.
	/// This is different from OnPlay, which is meant for game logic (dealing damage, gaining block, etc.), and won't
	/// run until the card play action is dequeued from the action queue.
	/// WARNING: Don't put any game logic in here! It might do things you don't expect.
	/// </summary>
	/// <param name="target">Creature that this card is targeting. Should be null for un-targeted cards.</param>
	public virtual Task OnEnqueuePlayVfx(Creature? target)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Override this method with logic for when this card is upgraded.
	/// To upgrade a card's dynamic vars, use <see cref="M:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar.UpgradeValueBy(System.Decimal)" />.
	/// To upgrade a card's energy cost, use <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.UpgradeBy(System.Int32)" />.
	/// </summary>
	protected virtual void OnUpgrade()
	{
	}

	/// <summary>
	/// Override this method for when the player ends the turn with this card in their hand.
	/// NOTES:
	/// * You must also override <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.HasTurnEndInHandEffect" /> to return true.
	/// * While this method is being run, this card will be in the Play pile.
	/// * After this method is run, this card will be added to the Discard pile.
	/// </summary>
	/// <param name="choiceContext">The choice context to use in the event of a player choice.</param>
	protected virtual Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		return Task.CompletedTask;
	}

	public async Task OnTurnEndInHandWrapper(PlayerChoiceContext choiceContext)
	{
		await CardPileCmd.Add(this, PileType.Play);
		if (LocalContext.IsMe(Owner))
		{
			await Cmd.CustomScaledWait(0.3f, 0.6f);
		}
		await OnTurnEndInHand(choiceContext);
		if (Keywords.Contains(CardKeyword.Ethereal))
		{
			await CardCmd.Exhaust(choiceContext, this, causedByEthereal: true);
		}
		else
		{
			await CardPileCmd.Add(this, PileType.Discard.GetPile(Owner));
		}
	}

	/// <summary>
	/// Can this card be played with the specified creature as the target?
	/// </summary>
	/// <param name="target">Creature for this card to target. Null when attempting to play with no target (like Defend).</param>
	/// <returns>Whether the card can be played with the specified target.</returns>
	public bool CanPlayTargeting(Creature? target)
	{
		if (!IsValidTarget(target))
		{
			return false;
		}
		return CanPlay();
	}

	/// <summary>
	/// Can this card be played?
	/// </summary>
	public bool CanPlay()
	{
		UnplayableReason reason;
		AbstractModel preventer;
		return CanPlay(out reason, out preventer);
	}

	/// <summary>
	/// Can this card be played?
	/// </summary>
	/// <param name="reason">
	/// Out param containing the reason that this card cannot be played (None if it can be played).
	/// </param>
	/// <param name="preventer">
	/// First model that made this card unable to be played (null if there is no preventer).
	/// </param>
	/// <returns>Whether this card can be played.</returns>
	public bool CanPlay(out UnplayableReason reason, out AbstractModel? preventer)
	{
		reason = UnplayableReason.None;
		ICombatState combatState = CombatState ?? _owner?.Creature.CombatState;
		if (combatState == null || Owner.PlayerCombatState == null)
		{
			preventer = null;
			return false;
		}
		if (Keywords.Contains(CardKeyword.Unplayable))
		{
			reason |= UnplayableReason.HasUnplayableKeyword;
		}
		if (!Owner.PlayerCombatState.HasEnoughResourcesFor(this, out var reason2))
		{
			reason |= reason2;
		}
		if (TargetType == TargetType.AnyAlly && combatState.PlayerCreatures.Count((Creature c) => c.IsAlive) <= 1)
		{
			reason |= UnplayableReason.NoLivingAllies;
		}
		if (!Hook.ShouldPlay(combatState, this, out preventer, AutoPlayType.None))
		{
			reason |= UnplayableReason.BlockedByHook;
		}
		if (!IsPlayable)
		{
			reason |= UnplayableReason.BlockedByCardLogic;
		}
		return reason == UnplayableReason.None;
	}

	/// <summary>
	/// Returns true if target is valid for this card.
	/// NOTE: This operates differently than potions! Do not try to unify this with PotionModel.IsValidTarget unless you
	/// change UI targeting; namely, PotionModel's TargetType.Self passes a target, whereas this one doesn't.
	/// </summary>
	public bool IsValidTarget(Creature? target)
	{
		if (target == null)
		{
			if (TargetType != TargetType.AnyEnemy)
			{
				return TargetType != TargetType.AnyAlly;
			}
			return false;
		}
		if (!target.IsAlive)
		{
			return false;
		}
		if (TargetType == TargetType.AnyEnemy)
		{
			return target.Side != Owner.Creature.Side;
		}
		if (TargetType == TargetType.AnyAlly)
		{
			return target.Side == Owner.Creature.Side;
		}
		return false;
	}

	public bool TryManualPlay(Creature? target)
	{
		if (CanPlayTargeting(target))
		{
			EnqueueManualPlay(target);
			return true;
		}
		return false;
	}

	private void EnqueueManualPlay(Creature? target)
	{
		TaskHelper.RunSafely(OnEnqueuePlayVfx(target));
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new PlayCardAction(this, target));
	}

	/// <summary>
	/// Spend the resources required to play this card.
	/// </summary>
	/// <returns>The energy and stars spent.</returns>
	public async Task<(int, int)> SpendResources()
	{
		int energy = Owner.PlayerCombatState.Energy;
		int energyToSpend = EnergyCost.GetAmountToSpend();
		int starsToSpend = Math.Max(0, GetStarCostWithModifiers());
		if (energyToSpend > energy && Hook.ShouldPayExcessEnergyCostWithStars(CombatState, Owner))
		{
			starsToSpend += (energyToSpend - energy) * 2;
			energyToSpend = energy;
		}
		await SpendEnergy(energyToSpend);
		await SpendStars(starsToSpend);
		return (energyToSpend, starsToSpend);
	}

	private async Task SpendEnergy(int amount)
	{
		if (EnergyCost.CostsX)
		{
			EnergyCost.CapturedXValue = amount;
		}
		if (amount > 0)
		{
			CombatManager.Instance.History.EnergySpent(CombatState, amount, Owner);
			Owner.PlayerCombatState.LoseEnergy(Math.Max(0, amount));
		}
		await Hook.AfterEnergySpent(CombatState, this, amount);
	}

	private async Task SpendStars(int amount)
	{
		LastStarsSpent = amount;
		if (amount > 0)
		{
			Owner.PlayerCombatState.LoseStars(amount);
			await Hook.AfterStarsSpent(Owner.Creature.CombatState, amount, Owner);
		}
	}

	/// <summary>
	/// Run all the logic for playing this card.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="target">The creature that this card is targeting. Null for un-targeted cards.</param>
	/// <param name="isAutoPlay">
	/// Whether this card is being auto-played.
	/// False when the player plays the card manually from their hand.
	/// True when played automatically by an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.MayhemPower" />.
	/// </param>
	/// <param name="resources">Info about the resources used when playing this card.</param>
	/// <param name="skipCardPileVisuals">Skip card pile visuals (tween to/from pile, smoke puff VFX, etc).</param>
	public async Task OnPlayWrapper(PlayerChoiceContext choiceContext, Creature? target, bool isAutoPlay, ResourceInfo resources, bool skipCardPileVisuals = false)
	{
		choiceContext.PushModel(this);
		await CombatManager.Instance.WaitForUnpause();
		CurrentTarget = target;
		CurrentPlayIndex = 0;
		if (!isAutoPlay)
		{
			await CardPileCmd.AddDuringManualCardPlay(this);
		}
		else
		{
			await CardPileCmd.Add(this, PileType.Play, CardPilePosition.Bottom, null, skipCardPileVisuals);
			if (!skipCardPileVisuals)
			{
				await Cmd.CustomScaledWait(0.25f, 0.35f);
			}
		}
		ICombatState combatState = CombatState;
		if (combatState == null)
		{
			return;
		}
		var (resultPileType, resultPilePosition) = Hook.ModifyCardPlayResultPileTypeAndPosition(combatState, this, isAutoPlay, resources, GetResultPileTypeForCardPlay(), CardPilePosition.Bottom, out IEnumerable<AbstractModel> modifiers);
		foreach (AbstractModel item in modifiers)
		{
			await item.AfterModifyingCardPlayResultPileOrPosition(this, resultPileType, resultPilePosition);
		}
		int playCount = await GeneratePlayCount(combatState, target);
		if (Owner.Creature.IsDead)
		{
			return;
		}
		ulong playStartTime = Time.GetTicksMsec();
		CombatManager.Instance.BeginCardOrPotionEffect(Owner);
		try
		{
			for (int i = 0; i < playCount; i++)
			{
				CurrentPlayIndex = i;
				if (Type == CardType.Power)
				{
					await PlayPowerCardFlyVfx();
				}
				else if (i > 0)
				{
					NCard nCard = NCard.FindOnTable(this);
					if (nCard != null)
					{
						await nCard.AnimMultiCardPlay();
					}
				}
				CardPlay cardPlay = new CardPlay
				{
					Card = this,
					Target = target,
					ResultPile = resultPileType,
					Resources = resources,
					IsAutoPlay = isAutoPlay,
					PlayIndex = i,
					PlayCount = playCount
				};
				await Hook.BeforeCardPlayed(combatState, cardPlay);
				CombatManager.Instance.History.CardPlayStarted(combatState, cardPlay);
				await OnPlay(choiceContext, cardPlay);
				if (Owner.Creature.IsDead)
				{
					return;
				}
				InvokeExecutionFinished();
				if (Enchantment != null)
				{
					await Enchantment.OnPlay(choiceContext, cardPlay);
					if (Owner.Creature.IsDead)
					{
						return;
					}
					Enchantment.InvokeExecutionFinished();
				}
				if (Affliction != null)
				{
					AfflictionModel affliction = Affliction;
					await affliction.OnPlay(choiceContext, target);
					if (Owner.Creature.IsDead)
					{
						return;
					}
					affliction.InvokeExecutionFinished();
				}
				CombatManager.Instance.History.CardPlayFinished(combatState, cardPlay);
				if (CombatManager.Instance.IsInProgress)
				{
					await Hook.AfterCardPlayed(combatState, choiceContext, cardPlay);
					if (Owner.Creature.IsDead)
					{
						return;
					}
				}
			}
		}
		finally
		{
			CombatManager.Instance.EndCardOrPotionEffect(Owner);
		}
		if (!skipCardPileVisuals)
		{
			float num = (float)(Time.GetTicksMsec() - playStartTime) / 1000f;
			await Cmd.CustomScaledWait(0.15f - num, 0.3f - num);
		}
		CardPile? pile = Pile;
		if (pile != null && pile.Type == PileType.Play)
		{
			switch (resultPileType)
			{
			case PileType.None:
				await CardPileCmd.RemoveFromCombat(this, skipCardPileVisuals);
				break;
			case PileType.Exhaust:
				await CardCmd.Exhaust(choiceContext, this, causedByEthereal: false, skipCardPileVisuals);
				break;
			default:
				await CardPileCmd.Add(this, resultPileType, resultPilePosition, null, skipCardPileVisuals);
				break;
			}
		}
		await CombatManager.Instance.CheckForEmptyHand(choiceContext, Owner);
		if (EnergyCost.AfterCardPlayedCleanup())
		{
			this.EnergyCostChanged?.Invoke();
		}
		if (_temporaryStarCosts.RemoveAll((TemporaryCardCost c) => c.ClearsWhenCardIsPlayed) > 0)
		{
			this.StarCostChanged?.Invoke();
		}
		CurrentTarget = null;
		CurrentPlayIndex = 0;
		this.Played?.Invoke();
		choiceContext.PopModel(this);
	}

	/// <summary>
	/// Generate the number of times this card's OnPlay effects should be executed when it's played.
	/// Runs hooks to modify the play count, including state-modifying hooks like
	/// <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterModifyingCardPlayCount(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Models.CardModel,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Models.AbstractModel})" />. This means you can only run this method in situations where it's
	/// okay to modify combat state (i.e. not as part of previews).
	/// </summary>
	/// <param name="combatState">The combat state that this card is being played in.</param>
	/// <param name="target">The creature that this card is targeting. Null for un-targeted cards.</param>
	protected async Task<int> GeneratePlayCount(ICombatState combatState, Creature? target)
	{
		int playCount = GetEnchantedReplayCount() + 1;
		playCount = Hook.ModifyCardPlayCount(combatState, this, playCount, target, out List<AbstractModel> modifyingModels);
		await Hook.AfterModifyingCardPlayCount(combatState, this, modifyingModels);
		return playCount;
	}

	/// <summary>
	/// Plays the VFX which makes the card swirl and fly into the player.
	/// Should only be done for power cards.
	/// </summary>
	private async Task PlayPowerCardFlyVfx()
	{
		NCard node = NCard.FindOnTable(this);
		bool flag = false;
		if (node != null)
		{
			foreach (NCardFlyPowerVfx item in NCombatRoom.Instance.CombatVfxContainer.GetChildren().OfType<NCardFlyPowerVfx>())
			{
				if (item.CardNode == node)
				{
					flag = true;
					break;
				}
			}
		}
		if (node == null || flag)
		{
			node = NCard.Create(this);
			if (node != null)
			{
				Tween tween = node.CreateTween();
				tween.Parallel().TweenProperty(node, "scale", Vector2.One * 1f, 0.10000000149011612).From(Vector2.Zero)
					.SetEase(Tween.EaseType.Out)
					.SetTrans(Tween.TransitionType.Cubic);
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(node);
				node.GlobalPosition = PileType.Play.GetTargetPosition(node);
				node.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
			}
			await Cmd.CustomScaledWait(0.1f, 0.8f);
		}
		if (node != null)
		{
			NCardFlyPowerVfx nCardFlyPowerVfx = NCardFlyPowerVfx.Create(node);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nCardFlyPowerVfx);
			TaskHelper.RunSafely(nCardFlyPowerVfx.PlayAnim());
			float duration = nCardFlyPowerVfx.GetDuration();
			await Cmd.CustomScaledWait(duration * 0.2f, duration);
		}
	}

	/// <summary>
	/// Get the pile that this card should be moved to after being played.
	/// </summary>
	protected virtual PileType GetResultPileTypeForCardPlay()
	{
		if (IsDupe || Type == CardType.Power)
		{
			return PileType.None;
		}
		if (ExhaustOnNextPlay || Keywords.Contains(CardKeyword.Exhaust))
		{
			ExhaustOnNextPlay = false;
			return PileType.Exhaust;
		}
		return PileType.Discard;
	}

	/// <summary>
	/// Send the card to the correct pile after it was attempted to be played while unplayable.
	/// This is the same as MoveCardToResultPileAfterPlay with one important exception: Power cards do not get sent
	/// to Limbo, and instead get sent to the discard.
	/// </summary>
	public async Task MoveToResultPileWithoutPlaying(PlayerChoiceContext choiceContext)
	{
		CardPile? pile = Pile;
		if (pile != null && pile.Type == PileType.Play)
		{
			if (IsDupe)
			{
				await CardPileCmd.RemoveFromCombat(this);
			}
			else if (ExhaustOnNextPlay || Keywords.Contains(CardKeyword.Exhaust))
			{
				await CardCmd.Exhaust(choiceContext, this);
			}
			else
			{
				await CardPileCmd.Add(this, PileType.Discard);
			}
		}
	}

	/// <summary>
	/// WARNING: If you're thinking of calling this from inside a model, you probably want
	/// <see cref="M:MegaCrit.Sts2.Core.Commands.CardCmd.Upgrade(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle)" /> instead.
	///
	/// Upgrade this card. This does not run any hooks.
	/// </summary>
	public void UpgradeInternal()
	{
		AssertMutable();
		CurrentUpgradeLevel++;
		OnUpgrade();
		DynamicVars.RecalculateForUpgradeOrEnchant();
		this.Upgraded?.Invoke();
	}

	/// <summary>
	/// Finalize an upgrade after calling UpgradeInternal. This clears out state that is used for displaying an upgrade
	/// preview.
	/// </summary>
	public void FinalizeUpgradeInternal()
	{
		DynamicVars.FinalizeUpgrade();
		EnergyCost.FinalizeUpgrade();
		_wasStarCostJustUpgraded = false;
	}

	public void DowngradeInternal()
	{
		AssertMutable();
		CurrentUpgradeLevel = 0;
		CardModel cardModel = ModelDb.GetById<CardModel>(base.Id).ToMutable();
		_dynamicVars = cardModel.DynamicVars.Clone(this);
		EnergyCost.ResetForDowngrade();
		_baseStarCost = cardModel.CanonicalStarCost;
		_keywords = cardModel.GetKeywordsWithSources(KeywordSources.Local).ToHashSet();
		AfterDowngraded();
		Enchantment?.ModifyCard();
		Affliction?.AfterApplied();
		this.Upgraded?.Invoke();
	}

	/// <summary>
	/// Gives cards a chance to add extra "cleanup" logic for downgrades.
	/// No-op by default.
	/// </summary>
	protected virtual void AfterDowngraded()
	{
	}

	public void InvokeDrawn()
	{
		this.Drawn?.Invoke();
	}

	/// <summary>
	/// Create a clone of this card.
	/// Clones must only be created in combat; for run-level clones, see <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CloneCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// See <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.CloneOf" /> for more info on clones, and the difference between a clone and a dupe.
	/// </summary>
	public CardModel CreateClone()
	{
		if (Pile != null && !Pile.Type.IsCombatPile())
		{
			throw new InvalidOperationException("Cannot create a clone of a card that is not in a combat pile.");
		}
		AssertMutable();
		CardModel cardModel = CardScope.CloneCard(this);
		cardModel._cloneOf = this;
		cardModel.ExhaustOnNextPlay = false;
		return cardModel;
	}

	/// <summary>
	/// Create a dupe of this card.
	/// See <see cref="P:MegaCrit.Sts2.Core.Models.CardModel.DupeOf" /> for more info on dupes, and the difference between a clone and a dupe.
	/// </summary>
	public CardModel CreateDupe()
	{
		if (IsDupe)
		{
			return DupeOf.CreateDupe();
		}
		AssertMutable();
		CardModel cardModel = CreateClone();
		cardModel.IsDupe = true;
		cardModel.RemoveKeyword(CardKeyword.Exhaust);
		return cardModel;
	}

	public SerializableCard ToSerializable()
	{
		AssertMutable();
		return new SerializableCard
		{
			Id = base.Id,
			CurrentUpgradeLevel = CurrentUpgradeLevel,
			Props = SavedProperties.From(this),
			Enchantment = Enchantment?.ToSerializable(),
			FloorAddedToDeck = FloorAddedToDeck
		};
	}

	/// <summary>
	/// Create a CardModel from a SerializableCard.
	/// Be careful calling this! Make sure all callers eventually use <see cref="T:MegaCrit.Sts2.Core.Runs.ICardScope" /> to add the card to the
	/// correct scope.
	/// </summary>
	public static CardModel FromSerializable(SerializableCard save)
	{
		CardModel cardModel = SaveUtil.CardOrDeprecated(save.Id).ToMutable();
		save.Props?.Fill(cardModel);
		if (save.FloorAddedToDeck.HasValue)
		{
			cardModel.FloorAddedToDeck = save.FloorAddedToDeck;
		}
		cardModel.AfterDeserialized();
		if (!(cardModel is DeprecatedCard))
		{
			if (save.Enchantment != null)
			{
				cardModel.EnchantInternal(EnchantmentModel.FromSerializable(save.Enchantment), save.Enchantment.Amount);
				cardModel.Enchantment.ModifyCard();
				cardModel.FinalizeUpgradeInternal();
			}
			for (int i = 0; i < save.CurrentUpgradeLevel; i++)
			{
				cardModel.UpgradeInternal();
				cardModel.FinalizeUpgradeInternal();
			}
		}
		return cardModel;
	}

	public override int CompareTo(AbstractModel? other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = base.CompareTo(other);
		if (num != 0)
		{
			return num;
		}
		CardModel cardModel = (CardModel)other;
		int num2 = CurrentUpgradeLevel.CompareTo(cardModel.CurrentUpgradeLevel);
		if (num2 != 0)
		{
			return num2;
		}
		return 0;
	}
}
