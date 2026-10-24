using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models;

public abstract class AfflictionModel : AbstractModel
{
	public const string locTable = "afflictions";

	private CardModel? _card;

	private int _amount;

	private AfflictionModel _canonicalInstance;

	public LocString Title => new LocString("afflictions", base.Id.Entry + ".title");

	private LocString Description => new LocString("afflictions", base.Id.Entry + ".description");

	private LocString ExtraCardText => new LocString("afflictions", base.Id.Entry + ".extraCardText");

	public virtual bool HasExtraCardText => false;

	public LocString DynamicDescription
	{
		get
		{
			LocString description = Description;
			description.Add("Amount", Amount);
			return description;
		}
	}

	public LocString? DynamicExtraCardText
	{
		get
		{
			if (!HasExtraCardText)
			{
				return null;
			}
			LocString extraCardText = ExtraCardText;
			extraCardText.Add("Amount", Amount);
			return extraCardText;
		}
	}

	public string OverlayPath => SceneHelper.GetScenePath("cards/overlays/afflictions/" + base.Id.Entry.ToLowerInvariant());

	public bool HasOverlay => ResourceLoader.Exists(OverlayPath);

	/// <summary>
	/// Get the card that this is afflicting.
	/// This is almost never null, so we leave it as non-nullable to make it easier to use. If you really need to check
	/// for null, use <see cref="P:MegaCrit.Sts2.Core.Models.AfflictionModel.HasCard" />.
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
				throw new InvalidOperationException("Afflictions cannot be moved from one card to another.");
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
			if (_amount != value)
			{
				int amount = _amount;
				_amount = value;
				if (_card != null)
				{
					_card.Owner.PlayerCombatState.RecalculateCardValues();
				}
				this.AmountChanged?.Invoke(amount, _amount);
			}
		}
	}

	/// <summary>
	/// Get the CombatState of the card that this is afflicting.
	/// Will never be null, since Afflictions are combat-only.
	/// </summary>
	public ICombatState CombatState => Card.CombatState;

	/// <summary>
	/// Can this afflict Unplayable cards?
	/// Usually true, because Unplayable cards sometimes have Sly and we might want the affliction's effect to still
	/// trigger. Some exceptions for when it doesn't make sense (like one that makes the card cost more energy).
	/// </summary>
	public virtual bool CanAfflictUnplayableCards => true;

	public virtual bool IsStackable => false;

	public AfflictionModel CanonicalInstance
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

	public override bool ShouldReceiveCombatHooks => true;

	protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

	public HoverTip HoverTip => new HoverTip(this, DynamicDescription);

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

	public event Action<int, int>? AmountChanged;

	public Control CreateOverlay()
	{
		return PreloadManager.Cache.GetScene(OverlayPath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
	}

	public virtual bool CanAfflictCardType(CardType cardType)
	{
		return true;
	}

	/// <summary>
	/// Checks whether the specified card can be afflicted with this affliction. For example, Ringing can only
	/// afflict attacks, so this will return true if an Attack is passed, but false if a Skill is passed.
	///
	/// Note: Do not override this method to REMOVE restrictions, just to ADD them. When you override it, make sure to
	/// call `base.CanAfflict`, and then add your own restrictions afterwards. You can also override some other methods
	/// to add specific types of restrictions (check AfflictionModel.cs for details).
	/// </summary>
	/// <param name="card">Card to check validity of.</param>
	/// <returns>Whether or not the specified card is valid to afflict with this.</returns>
	public virtual bool CanAfflict(CardModel card)
	{
		if (!CanAfflictCardType(card.Type))
		{
			return false;
		}
		if (card.Keywords.Contains(CardKeyword.Unplayable) && !CanAfflictUnplayableCards)
		{
			return false;
		}
		if (card.Affliction != null && (!IsStackable || card.Affliction.GetType() != GetType()))
		{
			return false;
		}
		return true;
	}

	public virtual void AfterApplied()
	{
	}

	public virtual void BeforeRemoved()
	{
	}

	public virtual Task OnPlay(PlayerChoiceContext choiceContext, Creature? target)
	{
		return Task.CompletedTask;
	}

	public AfflictionModel ToMutable()
	{
		AssertCanonical();
		AfflictionModel afflictionModel = (AfflictionModel)MutableClone();
		afflictionModel.CanonicalInstance = this;
		return afflictionModel;
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		this.AmountChanged = null;
		_card = null;
	}

	/// <summary>
	/// Chooses random targets out of a set of cards to afflict.
	/// </summary>
	/// <param name="rngSet">The RNG set to use. Does not use Card.Owner because we may be the canonical version.</param>
	/// <param name="cards">The possible cards to afflict.</param>
	/// <param name="count">Number of cards to afflict.</param>
	/// <returns>The cards to afflict.</returns>
	public IReadOnlyList<CardModel> PickRandomTargets(RunRngSet rngSet, IEnumerable<CardModel> cards, int count)
	{
		List<CardModel> list = cards.Where(CanAfflict).ToList().UnstableShuffle(rngSet.CombatCardGeneration);
		list.RemoveRange(Math.Clamp(list.Count - 1, 0, count), Math.Max(0, list.Count - count));
		return list;
	}

	/// <summary>
	/// Remove this affliction from its card.
	/// Should only be called by <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.ClearAfflictionInternal" /> and in tests.
	/// </summary>
	public void ClearInternal()
	{
		BeforeRemoved();
		_card = null;
	}
}
