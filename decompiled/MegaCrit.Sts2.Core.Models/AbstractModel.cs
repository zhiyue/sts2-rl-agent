using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.SourceGeneration;

namespace MegaCrit.Sts2.Core.Models;

[GenerateSubtypes(DynamicallyAccessedMemberTypes = (DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties))]
public abstract class AbstractModel : IComparable<AbstractModel>
{
	public ModelId Id { get; }

	public bool IsMutable { get; private set; }

	public bool IsCanonical => !IsMutable;

	/// <summary>
	/// The category ID of the model, as reported by ModelIdSerializationCache.
	/// Used in the deterministic sort in DeterministicModelComparer for speed purposes; it's much faster to compare
	/// integers than strings.
	/// </summary>
	public int CategorySortingId { get; private set; }

	/// <summary>
	/// The entry ID of the model, as reported by ModelIdSerializationCache.
	/// Used in the deterministic sort in DeterministicModelComparer for speed purposes; it's much faster to compare
	/// integers than strings.
	/// </summary>
	public int EntrySortingId { get; private set; }

	public virtual bool PreviewOutsideOfCombat => false;

	/// <summary>
	/// Whether or not this model should have combat hooks called on it.
	/// For example, AfterCardPlayed is only relevant in combat, and should be called on models that want to "listen" to
	/// the combat (powers, relics, cards in a combat pile, enchantments on those cards, etc.), but not on models that
	/// are disconnected from combat (cards in your deck, enchantments in your deck, etc.).
	/// Conversely, AfterRoomEntered is relevant outside of combat, and should be called on all models.
	/// Similarly, AfterDamageReceived is relevant outside of combat (since damage can be received in events and other
	/// non-combat rooms), so it should be called on all models.
	/// </summary>
	public abstract bool ShouldReceiveCombatHooks { get; }

	/// <summary>
	/// Fires when a hook on this model has finished executing.
	/// This event is a little unreliable, so you should only use it for UI things that can't be done any other way.
	/// </summary>
	public event Action<AbstractModel>? ExecutionFinished;

	protected AbstractModel()
	{
		Type type = GetType();
		if (ModelDb.Contains(type))
		{
			throw new DuplicateModelException(type);
		}
		Id = ModelDb.GetId(type);
	}

	public void InitId(ModelId id)
	{
		AssertCanonical();
		CategorySortingId = ModelIdSerializationCache.GetNetIdForCategory(Id.Category);
		EntrySortingId = ModelIdSerializationCache.GetNetIdForEntry(Id.Entry);
	}

	public virtual int CompareTo(AbstractModel? other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		return Id.CompareTo(other.Id);
	}

	/// <summary>
	/// Ensures that this model instance is mutable. Throws an exception if it's canonical.
	/// Use this in places where you want to ensure that you have a "real" model that you can use in combat and modify.
	///
	/// WARNING: If you're getting an exception from here, don't just convert the model to mutable at the top level
	/// of the stack trace. Instead, find the root of where the model is coming from and convert it there, so the
	/// correct instance is used in the correct places.
	///
	/// For example, canonical CardModels throw an exception if you try to add them to a pile, but you shouldn't just
	/// convert the CardModel to mutable in CardPile.AddCard(). Instead, find where that card was created and
	/// convert it there.
	/// </summary>
	/// <exception cref="T:MegaCrit.Sts2.Core.Models.Exceptions.CanonicalModelException"></exception>
	public void AssertMutable()
	{
		if (!IsMutable)
		{
			throw new CanonicalModelException(GetType());
		}
	}

	/// <summary>
	/// Ensures that this model instance is canonical. Throws an exception if it's mutable.
	/// Use this in places where you want a reference to the "concept" of a model. For example, CardPools only hold
	/// canonical CardModels.
	/// <exception cref="T:MegaCrit.Sts2.Core.Models.Exceptions.MutableModelException"></exception>
	/// </summary>
	public void AssertCanonical()
	{
		if (IsMutable)
		{
			throw new MutableModelException(GetType());
		}
	}

	/// <summary>
	/// Get a "clone" of this model, preserving its mutability status.
	/// This is useful in more backend-ish areas of the codebase, where a model can either be mutable or canonical,
	/// and we don't want to create a new mutable clone if we currently have a reference to the canonical instance.
	/// </summary>
	/// <returns>
	/// If this instance of the model is canonical, this method just returns itself.
	/// If this instance of the model is mutable, it returns another mutable clone.
	/// </returns>
	public AbstractModel ClonePreservingMutability()
	{
		if (!IsMutable)
		{
			return this;
		}
		return MutableClone();
	}

	/// <summary>
	/// WARNING: You almost always want to use `ToMutable()` or `ClonePreservingMutability()` instead, since we usually
	/// don't want to make mutable clones of already-mutable models.
	///
	/// Get a mutable "clone" of this model.
	/// This is useful in very generalized spots where we could either have a mutable or canonical model, and we want a
	/// mutable clone regardless.
	/// </summary>
	/// <returns>A mutable clone of this model.</returns>
	public AbstractModel MutableClone()
	{
		AbstractModel abstractModel = (AbstractModel)MemberwiseClone();
		abstractModel.IsMutable = true;
		abstractModel.DeepCloneFields();
		abstractModel.AfterCloned();
		return abstractModel;
	}

	protected virtual void DeepCloneFields()
	{
		AssertMutable();
	}

	/// <summary>
	/// Called after cloning to clean up shallow-copied references.
	/// IMPORTANT: MemberwiseClone() creates shallow copies of all fields, including event delegates.
	/// Subclasses that declare events MUST override this method and set all events to null,
	/// otherwise clones will fire events to the original's subscribers (correctness bug) and may
	/// prevent garbage collection through retained delegate references (memory leak).
	/// </summary>
	protected virtual void AfterCloned()
	{
		this.ExecutionFinished = null;
	}

	/// <summary>
	/// See <see cref="E:MegaCrit.Sts2.Core.Models.AbstractModel.ExecutionFinished" />.
	/// </summary>
	public void InvokeExecutionFinished()
	{
		this.ExecutionFinished?.Invoke(this);
	}

	/// <summary>
	/// Runs after a player enters a new act.
	/// </summary>
	public virtual Task AfterActEntered()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a model prevents a card from being added to the player's deck.
	/// </summary>
	/// <param name="card">Card that would've been added.</param>
	public virtual Task AfterAddToDeckPrevented(CardModel card)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a creature attacks.
	/// For a multi-attack, this will run once before any hits are done (as opposed to BeforeDamageGiven, which will run
	/// before each hit).
	/// </summary>
	/// <param name="command">The attack command that will be executed.</param>
	public virtual Task BeforeAttack(AttackCommand command)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature attacks.
	/// For a multi-attack, this will run once after all hits are done (as opposed to AfterDamageGiven, which will run
	/// after each hit).
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="command">The attack command that was executed.</param>
	public virtual Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPostPlay" /> phase of a player's turn has been entered.
	/// Effects that auto-play cards at the end of a player's turn should use this hook.
	/// See <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPostPlay" /> for examples.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPostPlay" /> phase was entered.</param>
	public virtual Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterAutoPrePlayPhaseEntered instead of this.
	/// Runs after the <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase of a player's turn has been entered.
	/// Effects that auto-play cards at the start of a player's turn should use this hook.
	/// See <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> for examples.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase was entered.</param>
	public virtual Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase of a player's turn has been entered.
	/// Effects that auto-play cards at the start of a player's turn should use this hook.
	/// See <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> for examples.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase was entered.</param>
	public virtual Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterAutoPrePlayPhaseEntered instead of this.
	/// Runs after the <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase of a player's turn has been entered.
	/// Effects that auto-play cards at the start of a player's turn should use this hook.
	/// See <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> for examples.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> phase was entered.</param>
	public virtual Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature's block has been cleared at the beginning of their round.
	/// Combat-only hook.
	/// </summary>
	/// <param name="creature">Creature whose block was reset.</param>
	public virtual Task AfterBlockCleared(Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a creature gains block.
	/// Combat-only hook.
	/// </summary>
	/// <param name="creature">Creature that will gain block.</param>
	/// <param name="amount">Amount of block they will gain.</param>
	/// <param name="props">ValueProps for the block they will gain.</param>
	/// <param name="cardSource">Optional card that will add the block.</param>
	public virtual Task BeforeBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature gains block.
	/// Combat-only hook.
	/// </summary>
	/// <param name="creature">Creature that gained block.</param>
	/// <param name="amount">Amount of block they gained.</param>
	/// <param name="props">ValueProps for the block they gained.</param>
	/// <param name="cardSource">Optional card that added the block.</param>
	public virtual Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature's block is completely removed.
	/// Prefer this over checking result.WasBlockBroken in AfterDamageReceived, as block can be broken by non-damage,
	/// like Expose.
	/// Combat-only hook.
	/// </summary>
	/// <param name="creature">Creature that gained block.</param>
	public virtual Task AfterBlockBroken(Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card moves from one pile to another.
	/// NOTE: The new pile can be determined by just checking card.Pile.
	/// </summary>
	/// <param name="card">Card that changed piles.</param>
	/// <param name="oldPileType">Type of the card's previous pile</param>
	/// <param name="clonedBy">The model that cloned this card, if applicable. Used to prevent copy effects from recursing.</param>
	public virtual Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterCardChangedPiles instead of this.
	/// Runs after a card moves from one pile to another.
	/// Works just like AfterCardChangedPiles, but runs after it.
	/// NOTE: The new pile can be determined by just checking card.Pile.
	/// </summary>
	/// <param name="card">Card that changed piles.</param>
	/// <param name="oldPileType">Type of the card's previous pile</param>
	/// <param name="clonedBy">The model that cloned this card, if applicable. Used to prevent copy effects from recursing.</param>
	public virtual Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card is discarded.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card that was discarded.</param>
	public virtual Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterCardDrawn instead of this.
	/// Runs after a card is drawn.
	/// Combat-only hook.
	/// Works just like AfterCardDrawn, but runs before it.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card that was drawn.</param>
	/// <param name="fromHandDraw">If this draw happened as part of the initial card draws at the start of your turn.</param>
	public virtual Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card is drawn.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card that was drawn.</param>
	/// <param name="fromHandDraw">If this draw happened as part of the initial card draws at the start of your turn.</param>
	public virtual Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card is put into a combat pile (see <see cref="M:MegaCrit.Sts2.Core.Entities.Cards.PileTypeExtensions.IsCombatPile(MegaCrit.Sts2.Core.Entities.Cards.PileType)" /> details on combat piles).
	/// Combat-only hook.
	/// </summary>
	/// <param name="card">Card that was put into a combat pile.</param>
	public virtual Task AfterCardEnteredCombat(CardModel card)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player generated a card for combat pile. This is different from AfterCardEnteredCombat
	/// because the player explicitly generated it themselves, excluding status cards that are applied by enemies.
	/// Combat-only hook.
	/// </summary>
	/// <param name="card">Card that was put into a combat pile.</param>
	/// <param name="creator">The player that created the card
	/// True if this card is being added by an effect from the player (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.InfiniteBlades" />).
	/// False if an enemy is adding it (like a monster giving you a curse).
	/// </param>
	public virtual Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card is exhausted.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="card">Card that was exhausted.</param>
	/// <param name="causedByEthereal">Was this Exhaust caused by Ethereal?</param>
	public virtual Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a card is automatically played by another card.
	/// Combat-only hook.
	/// </summary>
	/// <param name="card">Card that will be played.</param>
	/// <param name="target">Creature that will be targeted.</param>
	/// <param name="type">The method of autoplay.</param>
	public virtual Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a card is played.
	/// Combat-only hook.
	/// </summary>
	public virtual Task BeforeCardPlayed(CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a card is played.
	/// Combat-only hook.
	/// </summary>
	public virtual Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterCardPlayed instead of this.
	/// Runs after a card is played.
	/// Works just like AfterCardPlayed, but runs after it.
	/// Combat-only hook.
	/// </summary>
	public virtual Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before combat starts.
	/// Note: This may sound like a combat-only hook, but combat start is relevant to non-combat models (like a deck
	/// card that transforms when you start your third combat).
	/// </summary>
	public virtual Task BeforeCombatStart()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use BeforeCombatStart instead of this.
	/// Runs before combat starts.
	/// Note: This may sound like a combat-only hook, but combat start is relevant to non-combat models (like a deck
	/// card that transforms when you start your third combat).
	/// Works just like BeforeCombatStart, but runs after it.
	/// </summary>
	public virtual Task BeforeCombatStartLate()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after combat ends.
	/// Note: This may sound like a combat-only hook, but combat end is relevant to non-combat models (like Guilty).
	/// </summary>
	/// <param name="room">The room where the combat ended.</param>
	public virtual Task AfterCombatEnd(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterCombatVictory instead of this.
	/// Runs after combat ends in a player victory.
	/// Note: This may sound like a combat-only hook, but combat victory is relevant to non-combat models (like a deck
	/// card that transforms after 3 combat victories).
	/// Works just like AfterCombatVictory, but runs before it.
	/// </summary>
	/// <param name="room">The room where the player won the combat.</param>
	public virtual Task AfterCombatVictoryEarly(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after combat ends in a player victory.
	/// Note: This may sound like a combat-only hook, but combat victory is relevant to non-combat models (like a deck
	/// card that transforms after 3 combat victories).
	/// </summary>
	/// <param name="room">The room where the player won the combat.</param>
	public virtual Task AfterCombatVictory(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a new creature is added to combat.
	/// </summary>
	/// <param name="creature">Creature that was added to combat.</param>
	public virtual Task AfterCreatureAddedToCombat(Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature's HP changes for any reason (damage, heal, HP loss, etc).
	/// </summary>
	/// <param name="creature">Creature whose HP changed.</param>
	/// <param name="delta">
	///     Amount that the creature's HP changed by.
	///     A negative amount represents damage, while a positive amount represents healing.
	/// </param>
	public virtual Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature might dealt damage.
	/// Note: Even if the actual damage amount is 0 (due to block or something else), this will still be called.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="dealer">Creature who dealt the damage.</param>
	/// <param name="result">Results from the damage they dealt.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="target">Creature who received the damage.</param>
	/// <param name="cardSource">Optional card that dealt the damage.</param>
	public virtual Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a creature might receive damage.
	/// Note: Even if the actual damage amount will be 0 (due to Weak or something else), this will still be called.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="target">Creature who will receive the damage.</param>
	/// <param name="amount">Amount of damage they will receive.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who will be dealt the damage.</param>
	/// <param name="cardSource">Optional card that will deal the damage.</param>
	public virtual Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature might receive damage.
	/// Note: Even if the actual damage amount is 0 (due to block or something else), this will still be called.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="target">Creature who received the damage.</param>
	/// <param name="result">Results from the damage they received.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who dealt the damage.</param>
	/// <param name="cardSource">Optional card that dealt the damage.</param>
	public virtual Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterDamageReceived instead of this.
	/// Runs after a creature might receive damage.
	/// Works just like AfterDamageReceived, but runs after it.
	/// Note: Even if the actual damage amount is 0 (due to block or something else), this will still be called.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="target">Creature who received the damage.</param>
	/// <param name="result">Results from the damage they received.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who dealt the damage.</param>
	/// <param name="cardSource">Optional card that dealt the damage.</param>
	public virtual Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a creature dies.
	/// Note: This will run even if some effect (like Fairy in a Bottle) would prevent the creature's death.
	/// </summary>
	/// <param name="creature">Creature who is about to die.</param>
	public virtual Task BeforeDeath(Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a creature dies.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="creature">Creature who died.</param>
	/// <param name="wasRemovalPrevented">
	/// Whether the creature's removal from combat was prevented. Usually false, but true for things like reviving
	/// powers.
	/// </param>
	/// <param name="deathAnimLength">
	/// Number of seconds the creature's death animation will last. Good for delaying visuals until the animation
	/// is over.
	/// </param>
	public virtual Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after creatures died to Doom.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="creatures">The creatures that died to Doom.</param>
	public virtual Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the player's energy is reset at the beginning of their turn.
	/// Combat-only hook.
	/// </summary>
	/// <param name="player">Player whose energy is reset.</param>
	public virtual Task AfterEnergyReset(Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterEnergyReset instead of this.
	/// Runs after the player's energy is reset at the beginning of their turn.
	/// Combat-only hook.
	/// Works just like AfterEnergyReset, but runs after it.
	/// </summary>
	/// <param name="player">Player whose energy is reset.</param>
	public virtual Task AfterEnergyResetLate(Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the player's energy is spent.
	/// Combat-only hook.
	/// </summary>
	/// <param name="card">Card that spent the energy was spent on.</param>
	/// <param name="amount">Amount of energy that was spent.</param>
	public virtual Task AfterEnergySpent(CardModel card, int amount)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a card is removed from your deck.
	/// </summary>
	/// <param name="card">Card that is being removed.</param>
	public virtual Task BeforeCardRemoved(CardModel card)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a player's hand is flushed at the end of their turn.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose hand will be flushed.</param>
	public virtual Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use BeforeFlush instead of this.
	/// Runs before a player's hand is flushed at the end of their turn.
	/// Combat-only hook.
	/// Works just like BeforeFlush, but runs after it.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose hand will be flushed.</param>
	public virtual Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player's hand is flushed at the end of their turn.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose hand was flushed.</param>
	/// <param name="flushedCards">Cards that were discarded during the flush.</param>
	/// <param name="retainedCards">Cards that were retained during the flush.</param>
	/// <returns></returns>
	public virtual Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the player gains gold.
	/// </summary>
	/// <param name="player">Player who gained the gold.</param>
	public virtual Task AfterGoldGained(Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before the player draws their hand at the beginning of their turn.
	/// Combat-only hook.
	/// </summary>
	/// <param name="player">The player who is about to draw cards.</param>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="combatState">The CombatState that the hand draw will occur in.</param>
	public virtual Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use BeforeHandDraw instead of this.
	/// Runs before the player draws their hand at the beginning of their turn.
	/// Combat-only hook.
	/// Works just like BeforeHandDraw, but runs after it.
	/// </summary>
	/// <param name="player">The player who is about to draw cards.</param>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="combatState">The CombatState that the hand draw will occur in.</param>
	public virtual Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the player's hand becomes empty.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose hand was emptied.</param>
	public virtual Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player purchase an item from the shop.
	/// </summary>
	/// <param name="player">Player who purchased the item.</param>
	/// <param name="itemPurchased">Item that was purchased.</param>
	/// <param name="goldSpent">Amount of gold that was spent to purchase the item.</param>
	public virtual Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a map is generated for an act.
	/// </summary>
	/// <param name="map">The generated map.</param>
	/// <param name="actIndex">The act index for which the map was generated.</param>
	public virtual Task AfterMapGenerated(ActMap map, int actIndex)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of block was modified.
	/// Combat-only hook.
	/// </summary>
	/// <param name="modifiedAmount">The amount of block after all modifications have been applied.</param>
	/// <param name="cardSource">Card that added the block.</param>
	/// <param name="cardPlay">
	/// CardPlay that added block.
	/// Null if the block is not associated with a CardPlay.
	/// This can be null even if <paramref name="cardSource" /> is not null, because this is also called to preview block
	/// values on cards that are in your hand and haven't been played yet.
	/// </param>
	public virtual Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the play count of a card was modified.
	/// </summary>
	/// <param name="card">Card whose play count was modified.</param>
	public virtual Task AfterModifyingCardPlayCount(CardModel card)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the result pile of a played card was modified.
	/// </summary>
	/// <param name="card">Card whose result pile was modified.</param>
	/// <param name="pileType">Final pile type that the card will be put in.</param>
	/// <param name="position">Final position in the pile that the card will be put in.</param>
	public virtual Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the trigger count of an orb was modified.
	/// </summary>
	/// <param name="orb">Orb whose trigger count was modified.</param>
	public virtual Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the play count of a card was modified.
	/// </summary>
	public virtual Task AfterModifyingCardRewardOptions()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of damage was modified.
	/// </summary>
	public virtual Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of energy to gain was modified.
	/// </summary>
	public virtual Task AfterModifyingEnergyGain()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of gold to gain was modified.
	/// </summary>
	/// <param name="player">Player who is about to gain gold.</param>
	/// <param name="amount">The modified amount of gold they will gain.</param>
	public virtual Task AfterModifyingGoldGained(Player player, decimal amount)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the number of cards drawn at the beginning of the player's turn was modified.
	/// Combat-only hook.
	/// </summary>
	public virtual Task AfterModifyingHandDraw()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after card draw was prevented.
	/// Combat-only hook.
	/// </summary>
	public virtual Task AfterPreventingDraw()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of HP a creature should lose (before Osty redirection) was modified.
	/// </summary>
	public virtual Task AfterModifyingHpLostBeforeOsty()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the amount of HP a creature should lose (after Osty redirection) was modified.
	/// </summary>
	public virtual Task AfterModifyingHpLostAfterOsty()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a power has its amount modified during application.
	/// Combat-only hook.
	/// </summary>
	/// <param name="power">Power whose amount was modified.</param>
	public virtual Task AfterModifyingPowerAmountReceived(PowerModel power)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a power has its amount modified during application.
	/// Combat-only hook.
	/// </summary>
	/// <param name="power">Power whose amount was modified.</param>
	public virtual Task AfterModifyingPowerAmountGiven(PowerModel power)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a list of rewards has its contents modified.
	/// </summary>
	public virtual Task AfterModifyingRewards()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after player channeled an orb
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player who channeled the orb.</param>
	/// <param name="orb">Orb that was channeled.</param>
	public virtual Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after player evokes a orb
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="orb">Orb that was evoked.</param>
	/// <param name="targets">who the orb affected</param>
	public virtual Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after Osty is revived.
	/// </summary>
	/// <param name="osty">The osty that was revived</param>
	public virtual Task AfterOstyRevived(Creature osty)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a potion is used.
	/// Note: This only invokes for relics when out of combat.
	/// </summary>
	/// <param name="potion">Potion that will be used.</param>
	/// <param name="target">Creature that will be targeted.</param>
	public virtual Task BeforePotionUsed(PotionModel potion, Creature? target)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a potion is used.
	/// Note: This only invokes for relics when out of combat.
	/// </summary>
	/// <param name="potion">Potion that was used.</param>
	/// <param name="target">Creature that was targeted.</param>
	public virtual Task AfterPotionUsed(PotionModel potion, Creature? target)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a potion is discarded.
	/// Note: This only invokes for relics when out of combat.
	/// </summary>
	/// <param name="potion">Potion that was used.</param>
	public virtual Task AfterPotionDiscarded(PotionModel potion)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a potion is procured.
	/// Note: This only invokes for relics when out of combat.
	/// </summary>
	/// <param name="potion">Potion that was used.</param>
	public virtual Task AfterPotionProcured(PotionModel potion)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a power's amount is changed, whether it's a new power being added or an existing power's amount being
	/// changed.
	/// Combat-only hook.
	/// </summary>
	/// <param name="power">Power that will be applied.</param>
	/// <param name="amount">Amount of the power that will be added.</param>
	/// <param name="target">The creature that the power will be added to</param>
	/// <param name="applier">
	/// (Optional) The creature that will change the power amount. Null if the change is not caused by a creature (usually
	/// in tests).
	/// </param>
	/// <param name="cardSource">Optional card that is changing the power amount.</param>
	public virtual Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a power's amount is changed, whether it's a new power being added or an existing power's amount being
	/// changed.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="power">Power that was applied.</param>
	/// <param name="amount">Amount of the power that was added</param>
	/// <param name="applier">
	/// (Optional) The creature that changed the power amount. Null if the change was not caused by a creature (usually
	/// in tests).
	/// </param>
	/// <param name="cardSource">
	/// (Optional) The card that changed the power amount. Null if the change was not caused by a card.
	/// </param>
	public virtual Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a power prevents the creature from clearing block.
	/// Combat-only hook.
	/// </summary>
	/// <param name="preventer">Model that prevented the block clear.</param>
	/// <param name="creature">Creature whose block clear was prevented.</param>
	public virtual Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after this model prevents a creature's death.
	/// </summary>
	/// <param name="creature">Creature whose death was prevented.</param>
	public virtual Task AfterPreventingDeath(Creature creature)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// After player rests at a rest site.
	/// </summary>
	/// <param name="player">Player that rested.</param>
	/// <param name="isMimicked">
	/// Is a mimicked rest site heal?
	/// Used in spots that mimicking a rest site heal, but are not actual rest site heals.
	/// For example: <see cref="T:MegaCrit.Sts2.Core.Models.Events.DenseVegetation" /> mimicks a rest site heal when you select certain options.
	/// This should count as a rest site heal for <see cref="T:MegaCrit.Sts2.Core.Models.Relics.RegalPillow" />, but not for <see cref="T:MegaCrit.Sts2.Core.Models.Modifiers.NightTerrors" />.
	/// </param>
	public virtual Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// After player smiths at a rest site.
	/// </summary>
	/// <param name="player">Player that smithed.</param>
	public virtual Task AfterRestSiteSmith(Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player successfully takes a reward from the reward screen.
	/// </summary>
	/// <param name="player">Player who took the reward.</param>
	/// <param name="reward">The reward taken.</param>
	public virtual Task AfterRewardTaken(Player player, Reward reward)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before a player enters a room.
	/// </summary>
	/// <param name="room">Room that will be entered.</param>
	public virtual Task BeforeRoomEntered(AbstractRoom room)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player enters a room.
	/// For "start of combat" effects that should execute before the start of the first turn (like applying powers), you
	/// should use this hook with a "room is CombatRoom" check.
	/// For "start of combat" effects that should execute after the start of the first turn (like dealing damage or
	/// gaining block), you should use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" /> with a RoundNumber check instead.
	/// </summary>
	/// <param name="room">Room that was entered.</param>
	public virtual Task AfterRoomEntered(AbstractRoom room)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player shuffles their draw pile.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="shuffler">Player who shuffled.</param>
	public virtual Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player spends stars on something.
	/// Combat-only hook.
	/// </summary>
	/// <param name="amount">Amount of stars that were spent.</param>
	/// <param name="spender">Player that spent the stars.</param>
	public virtual Task AfterStarsSpent(int amount, Player spender)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player gains stars.
	/// Combat-only hook.
	/// </summary>
	/// <param name="amount">Amount of stars that were gained.</param>
	/// <param name="gainer">Player that gained the stars.</param>
	public virtual Task AfterStarsGained(int amount, Player gainer)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player triggers a Forge effect, increasing the damage of Sovereign Blade.
	/// Combat-only hook.
	/// </summary>
	/// <param name="amount">Amount of forge that was triggered.</param>
	/// <param name="forger">Player that triggered the forge.</param>
	/// <param name="source">The abstract model that triggered the forging</param>
	public virtual Task AfterForge(decimal amount, Player forger, AbstractModel? source)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after a player summons.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="summoner">Player that summoned.</param>
	/// <param name="amount">Amount that was summoned.</param>
	public virtual Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the player takes an extra turn.
	/// Combat-only hook.
	/// <param name="player">Player that took the extra turn.</param>
	/// </summary>
	public virtual Task AfterTakingExtraTurn(Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// WARNING: ONLY DO VFX/SFX THINGS IN THIS HOOK. It is not meant for things that affect game state.
	/// Runs after attempting to target this creature with a card is blocked.
	/// </summary>
	/// <param name="blocker">Creature who blocked the targeting.</param>
	public virtual Task AfterTargetingBlockedVfx(Creature blocker)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before the start of a side's turn, before any effects like energy reset or card draw.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext"></param>
	/// <param name="side">Side whose turn is about to start.</param>
	/// <param name="participants">
	/// Creatures participating in this turn.
	/// If a player is taking an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	/// <param name="combatState">State of the combat that the turn is about to start in in.</param>
	public virtual Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the start of a side's turn.
	/// For "start of combat" effects that should execute after the start of the first turn (like dealing damage or
	/// gaining block), you should use this hook with a RoundNumber check.
	/// For "start of combat" effects that should execute before the start of the first turn (like applying powers), you
	/// should use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRoomEntered(MegaCrit.Sts2.Core.Rooms.AbstractRoom)" /> with a "room is CombatRoom" check instead.
	/// <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPlayerTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" /> should be used for any hooks that trigger a player choice.
	/// Combat-only hook.
	/// </summary>
	/// <param name="side">Side whose turn started.</param>
	/// <param name="participants">
	/// Creatures participating in this turn.
	/// If a player is taking an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	/// <param name="combatState">State of the combat that the turn is about to start in in.</param>
	public virtual Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterSideTurnStart instead of this.
	/// Runs after the start of a side's turn.
	/// For "start of combat" effects that should execute after the start of the first turn (like dealing damage or
	/// gaining block), you should use this hook with a RoundNumber check.
	/// For "start of combat" effects that should execute before the start of the first turn (like applying powers), you
	/// should use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterRoomEntered(MegaCrit.Sts2.Core.Rooms.AbstractRoom)" /> with a "room is CombatRoom" check instead.
	/// <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterPlayerTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Players.Player)" /> should be used for any hooks that trigger a player choice.
	/// Combat-only hook.
	/// </summary>
	/// <param name="side">Side whose turn started.</param>
	/// <param name="participants">
	/// Creatures participating in this turn.
	/// If a player is taking an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	/// <param name="combatState">State of the combat that the turn is about to start in in.</param>
	public virtual Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterPlayerTurnStart instead of this.
	/// Runs after the start of a player's turn.
	/// Most of the time, this and <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" /> are interchangeable. However, if you need to make a
	/// player choice call, do it here. Otherwise, problems will arise when draw effects that trigger player choice
	/// (e.g. Mayhem, Stratagem) interact with the hook.
	/// Works just like AfterPlayerTurnStart, but runs before it.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose turn started.</param>
	public virtual Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the start of a player's turn.
	/// Most of the time, this and <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" /> are interchangeable. However, if you need to make a
	/// player choice call, do it here. Otherwise, problems will arise when draw effects that trigger player choice
	/// (e.g. Mayhem, Stratagem) interact with the hook.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose turn started.</param>
	public virtual Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterPlayerTurnStart instead of this.
	/// Runs after the start of a player's turn.
	/// Most of the time, this and <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart(MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature},MegaCrit.Sts2.Core.Combat.ICombatState)" /> are interchangeable. However, if you need to make a
	/// player choice call, do it here. Otherwise, problems will arise when draw effects that trigger player choice
	/// (e.g. Mayhem, Stratagem) interact with the hook.
	/// Works just like AfterPlayerTurnStart, but runs after it.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="player">Player whose turn started.</param>
	public virtual Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use BeforeSideTurnEnd or BeforeSideTurnEndEarly instead of this.
	/// Runs before the end of a side's turn.
	/// Works just like BeforeSideTurnEnd and BeforeSideTurnEndEarly, but runs before either of them.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="side">Side whose turn ended.</param>
	/// <param name="participants">
	/// Creatures who participated in this turn.
	/// If a player took an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	public virtual Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use BeforeSideTurnEnd instead of this.
	/// Runs before the end of a side's turn.
	/// Works just like BeforeSideTurnEnd, but runs before it.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="side">Side whose turn ended.</param>
	/// <param name="participants">
	/// Creatures who participated in this turn.
	/// If a player took an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	public virtual Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs before the end of a side's turn.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="side">Side whose turn ended.</param>
	/// <param name="participants">
	/// Creatures who participated in this turn.
	/// If a player took an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	public virtual Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs after the end of a side's turn.
	/// Note: Enemy-damaging effects (Bedlam Beacon, The Bomb, etc) should NOT go in here. Put them in BeforeSideTurnEnd
	/// instead. Self-damaging effects (Acid Dust, Magic Bomb, etc) can go in here.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="side">Side whose turn ended.</param>
	/// <param name="participants">
	/// Creatures who participated in this turn.
	/// If a player took an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	public virtual Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// CAREFUL! You should usually use AfterSideTurnEnd instead of this.
	/// Runs after the end of a side's turn.
	/// Note: Enemy-damaging effects (Bedlam Beacon, The Bomb, etc) should NOT go in here. Put them in BeforeSideTurnEnd
	/// instead. Self-damaging effects (Acid Dust, Magic Bomb, etc) can go in here.
	/// Combat-only hook.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="side">Side whose turn ended.</param>
	/// <param name="participants">
	/// Creatures who participated in this turn.
	/// If a player took an extra turn in multiplayer, other players may be missing from this list, and the models
	/// they own should usually exit early in this case.
	/// </param>
	public virtual Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Modify the number of times the specified attack will hit.
	/// </summary>
	/// <param name="attack">Attack whose hit count we're modifying.</param>
	/// <param name="hitCount">The current number of times this attack should hit.</param>
	/// <returns>The new number of times this attack should hit.</returns>
	public virtual int ModifyAttackHitCount(AttackCommand attack, int hitCount)
	{
		return hitCount;
	}

	/// <summary>
	/// Add to the amount of block that will be gained.
	/// Use this for effects that add amounts to existing block, like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DexterityPower" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Fasten" />.
	/// </summary>
	/// <param name="target">Creature who will gain the block.</param>
	/// <param name="block">Initial amount of block.</param>
	/// <param name="props">ValueProp for the block.</param>
	/// <param name="cardSource">
	/// Card that will be adding the block.
	/// Null if the block is coming from something other than a card (like a Relic).
	/// </param>
	/// <param name="cardPlay">
	/// CardPlay that will be adding the block.
	/// Null if the block is not associated with a CardPlay.
	/// This can be null even if <paramref name="cardSource" /> is not null, because this is also called to preview block
	/// values on cards that are in your hand and haven't been played yet.
	/// </param>
	/// <returns>Amount of block to be added.</returns>
	public virtual decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		return 0m;
	}

	/// <summary>
	/// Multiply the amount of damage that will be gained.
	/// Use this for effects that multiply existing block by some amount, like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Unmovable" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Powers.FrailPower" />.
	/// </summary>
	/// <param name="target">Creature who will receive the block.</param>
	/// <param name="block">Initial amount of block.</param>
	/// <param name="props">ValueProp for the block.</param>
	/// <param name="cardSource">
	/// Card that will be adding the block.
	/// Null if the block is coming from something other than a card (like a Relic).
	/// </param>
	/// <param name="cardPlay">
	/// CardPlay that will be adding the block.
	/// Null if the block is not associated with a CardPlay.
	/// This can be null even if <paramref name="cardSource" /> is not null, because this is also called to preview block
	/// values on cards that are in your hand and haven't been played yet.
	/// </param>
	/// <returns>Amount that the block should be multiplied by.</returns>
	public virtual decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		return 1m;
	}

	/// <summary>
	/// Modify the number of times the currently-being-played card will be played.
	/// Good for "Your next card is played twice" effects.
	/// </summary>
	/// <param name="card">The card that is being played.</param>
	/// <param name="playCount">The number of times it should be played.</param>
	/// <param name="target">Creature this card is targeting</param>
	/// <returns>The new number of times this card should be played.</returns>
	public virtual int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		return playCount;
	}

	/// <summary>
	/// Modify the pile that the card will be put in after it is played, and the position it will have in that pile.
	/// </summary>
	/// <param name="card">The card that is being played.</param>
	/// <param name="isAutoPlay">
	/// Whether this card is being auto-played.
	/// False when the player plays the card manually from their hand.
	/// True when played automatically by an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.MayhemPower" />.
	/// </param>
	/// <param name="resources">Info about the resources used when playing this card.</param>
	/// <param name="pileType">The type of pile that the card will currently be put in.</param>
	/// <param name="position">The position that the card will currently be put into in pileType.</param>
	/// <returns>The new type of pile that this card should be put in, and the position it should have.</returns>
	public virtual (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		return (pileType, position);
	}

	/// <summary>
	/// Modify the number of times an orb's passive will fire.
	/// Doesn't trigger if the passive is fired manually (ie loop).
	/// </summary>
	/// <param name="orb">Orb whose passive is being triggered.</param>
	/// <param name="triggerCount">The number of times the orb's passive should be triggered.</param>
	/// <returns></returns>
	public virtual int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
	{
		return triggerCount;
	}

	/// <summary>
	/// Modify the cards reward options before a card reward is generated from them.
	/// You may remove cards, add cards, or add duplicate cards (to increase the chances of that card showing up) to
	/// the card pool.
	/// </summary>
	/// <param name="player">The player who is being offered rewards.</param>
	/// <param name="options">What the card rewards are being generated for.</param>
	public virtual CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		return options;
	}

	/// <summary>
	/// CAREFUL! You should usually use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyCardRewardCreationOptions(MegaCrit.Sts2.Core.Entities.Players.Player,MegaCrit.Sts2.Core.Runs.CardCreationOptions)" /> instead of this.
	/// Modify the cards reward options before a card reward is generated from them.
	/// You may remove cards, add cards, or add duplicate cards (to increase the chances of that card showing up) to
	/// the card pool.
	/// </summary>
	/// <param name="player">The player who is being offered rewards.</param>
	/// <param name="options">What the card rewards are being generated for.</param>
	public virtual CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
	{
		return options;
	}

	/// <summary>
	/// Modify the odds of a card reward being offered upgraded.
	/// </summary>
	/// <param name="player">Player who is being offered rewards.</param>
	/// <param name="card">Card reward.</param>
	/// <param name="odds">Initial odds of it being upgraded.</param>
	/// <returns>New odds of it being upgraded.</returns>
	public virtual decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
	{
		return odds;
	}

	/// <summary>
	/// Add to the amount of damage that will be dealt.
	/// Use this for effects that add amounts to existing damage, like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.StrengthPower" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Powers.VigorPower" />.
	/// </summary>
	/// <param name="target">Creature who will receive the damage.</param>
	/// <param name="amount">Current amount of damage that will be dealt.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who will deal the damage.</param>
	/// <param name="cardSource">Card that will be dealing the damage.</param>
	/// <returns>Amount of damage to be added.</returns>
	public virtual decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return 0m;
	}

	/// <summary>
	/// Set the maximum amount of damage that will be dealt in a single hit.
	/// </summary>
	/// <param name="target">Creature who will receive the damage.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who will deal the damage.</param>
	/// <param name="cardSource">Card that will be dealing the damage.</param>
	/// <returns></returns>
	public virtual decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return decimal.MaxValue;
	}

	/// <summary>
	/// Multiply the amount of damage that will be dealt.
	/// Use this for effects that multiply existing damage by some amount, like <see cref="T:MegaCrit.Sts2.Core.Models.Powers.VulnerablePower" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Powers.WeakPower" />.
	/// </summary>
	/// <param name="target">Creature who will receive the damage.</param>
	/// <param name="amount">Current amount of damage that will be dealt.</param>
	/// <param name="props">ValueProp for damage.</param>
	/// <param name="dealer">Creature who will deal the damage.</param>
	/// <param name="cardSource">Card that will be dealing the damage.</param>
	/// <returns>Amount that the damage should be multiplied by.</returns>
	public virtual decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return 1m;
	}

	/// <summary>
	/// When the player is about to gain energy, modify the amount that they should gain.
	/// </summary>
	/// <param name="player">Player who will gain energy.</param>
	/// <param name="amount">Original amount of energy they would gain.</param>
	/// <returns>New amount of energy to gain.</returns>
	public virtual decimal ModifyEnergyGain(Player player, decimal amount)
	{
		return amount;
	}

	/// <summary>
	/// When the player is about to gain gold, modify the amount that they should gain.
	/// </summary>
	/// <param name="player">Player who will gain gold.</param>
	/// <param name="amount">Original amount of gold they would gain.</param>
	/// <returns>New amount of gold to gain.</returns>
	public virtual decimal ModifyGoldGained(Player player, decimal amount)
	{
		return amount;
	}

	/// <summary>
	/// Modify the dungeon map before it is set up.
	/// </summary>
	/// <param name="runState">Run state.</param>
	/// <param name="map">The generated map.</param>
	/// <param name="actIndex">The act index for which the map was generated.</param>
	public virtual ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		return map;
	}

	/// <summary>
	/// Annotate the dungeon map after all topology-replacing hooks have run.
	/// Must not replace the map (i.e., return a different ActMap instance) because this hook also
	/// runs on saved maps during multiplayer reconnect, where the topology must be preserved.
	/// Use this for quest markers, metadata, etc. To replace the map, use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyGeneratedMap(MegaCrit.Sts2.Core.Runs.IRunState,MegaCrit.Sts2.Core.Map.ActMap,System.Int32)" />.
	/// </summary>
	/// <param name="runState">Run state.</param>
	/// <param name="map">The generated map.</param>
	/// <param name="actIndex">The act index for which the map was generated.</param>
	public virtual ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		return map;
	}

	/// <summary>
	/// Modify the amount of cards the player should draw at the start of their turn.
	/// </summary>
	/// <param name="player">Player who will draw the cards.</param>
	/// <param name="count">Original number of cards that would be drawn.</param>
	/// <returns>New amount of cards to be drawn.</returns>
	public virtual decimal ModifyHandDraw(Player player, decimal count)
	{
		return count;
	}

	/// <summary>
	/// CAREFUL! You should usually use ModifyHandDraw instead of this.
	/// Modify the amount of cards the player should draw at the start of their turn.
	/// </summary>
	/// <param name="player">Player who will draw the cards.</param>
	/// <param name="count">Original number of cards that would be drawn.</param>
	/// <returns>New amount of cards to be drawn.</returns>
	public virtual decimal ModifyHandDrawLate(Player player, decimal count)
	{
		return count;
	}

	/// <summary>
	/// Modify the amount of HP that will be lost by the target.
	/// This runs when damage is being dealt, after block is applied.
	/// This runs BEFORE damage is redirected from Necrobinder to Osty. If you want to modify damage AFTER this
	/// redirection, use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostAfterOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// Note: Only <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DieForYouPower" /> causes damage redirection. If we ever get a second source of damage
	/// redirection, we should rename this method to be more general.
	/// </summary>
	/// <param name="target">Creature who will lose HP.</param>
	/// <param name="amount">Original amount of HP to be lost.</param>
	/// <param name="props">ValueProp for amount.</param>
	/// <param name="dealer">Creature who will be causing the HP loss.</param>
	/// <param name="cardSource">Card that will be causing the HP loss.</param>
	/// <returns>New amount of HP to be lost.</returns>
	public virtual decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	/// <summary>
	/// CAREFUL! You should usually use ModifyHpLostBeforeOsty instead of this.
	/// Modify the amount of HP that will be lost by the target.
	/// This runs when damage is being dealt, after block is applied.
	/// This runs BEFORE damage is redirected from Necrobinder to Osty. If you want to modify damage AFTER this
	/// redirection, use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostAfterOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// Note: Only <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DieForYouPower" /> causes damage redirection. If we ever get a second source of damage
	/// redirection, we should rename this method to be more general.
	/// </summary>
	/// <param name="target">Creature who will lose HP.</param>
	/// <param name="amount">Original amount of HP to be lost.</param>
	/// <param name="props">ValueProp for amount.</param>
	/// <param name="dealer">Creature who will be causing the HP loss.</param>
	/// <param name="cardSource">Card that will be causing the HP loss.</param>
	/// <returns>New amount of HP to be lost.</returns>
	public virtual decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	/// <summary>
	/// Modify the amount of HP that will be lost by the target.
	/// This runs when damage is being dealt, after block is applied.
	/// This runs AFTER damage is redirected from Necrobinder to Osty. If you want to modify damage BEFORE this
	/// redirection, use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostBeforeOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// Note: Only <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DieForYouPower" /> causes damage redirection. If we ever get a second source of damage
	/// redirection, we should rename this method to be more general.
	/// </summary>
	/// <param name="target">Creature who will lose HP.</param>
	/// <param name="amount">Original amount of HP to be lost.</param>
	/// <param name="props">ValueProp for amount.</param>
	/// <param name="dealer">Creature who will be causing the HP loss.</param>
	/// <param name="cardSource">Card that will be causing the HP loss.</param>
	/// <returns>New amount of HP to be lost.</returns>
	public virtual decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	/// <summary>
	/// CAREFUL! You should usually use ModifyHpLostAfterOsty instead of this.
	/// Modify the amount of HP that will be lost by the target.
	/// This runs when damage is being dealt, after block is applied.
	/// This runs AFTER damage is redirected from Necrobinder to Osty. If you want to modify damage BEFORE this
	/// redirection, use <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostBeforeOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// Note: Only <see cref="T:MegaCrit.Sts2.Core.Models.Powers.DieForYouPower" /> causes damage redirection. If we ever get a second source of damage
	/// redirection, we should rename this method to be more general.
	/// </summary>
	/// <param name="target">Creature who will lose HP.</param>
	/// <param name="amount">Original amount of HP to be lost.</param>
	/// <param name="props">ValueProp for amount.</param>
	/// <param name="dealer">Creature who will be causing the HP loss.</param>
	/// <param name="cardSource">Card that will be causing the HP loss.</param>
	/// <returns>New amount of HP to be lost.</returns>
	public virtual decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	/// <summary>
	/// Modify the max energy a player has.
	/// </summary>
	/// <param name="player">Player who we are modifying.</param>
	/// <param name="amount">Current amount of max energy they have.</param>
	public virtual decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		return amount;
	}

	/// <summary>
	/// Modify the cards available in the merchant's card pool.
	/// You may remove cards, add cards, or add duplicate cards (to increase the chances of that card showing up).
	/// </summary>
	/// <param name="player">The player who is at the merchant.</param>
	/// <param name="options">The original options in the card pool.</param>
	/// <returns>The modified card pool.</returns>
	public virtual IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
	{
		return options;
	}

	/// <summary>
	/// Modify the rarity of a merchant card entry
	/// </summary>
	/// <param name="player">The player who is at the merchant.</param>
	/// <param name="rarity">Original rarity of the merchant card entry.</param>
	/// <returns>New rarity.</returns>
	public virtual CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
	{
		return rarity;
	}

	/// <summary>
	/// Modify the cards created to sell at the merchant.
	/// </summary>
	/// <param name="player">Player who is at the merchant</param>
	/// <param name="cards">
	///     CardCreationResults created to sell at the merchant.
	///     Note that they could already have been modified by another source.
	/// </param>
	public virtual void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
	}

	/// <summary>
	/// Modify the cost of items at the merchant.
	/// </summary>
	/// <param name="player">The player who is at the merchant.</param>
	/// <param name="entry">the merchant entry we are modifying the price for</param>
	/// <param name="cost">Original cost of the merchant item.</param>
	/// <returns>New cost of the merchant item.</returns>
	public virtual decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
	{
		return cost;
	}

	/// <summary>
	/// Modify orb value for this creature.
	/// </summary>
	/// <param name="orb">Orb we are modifying.</param>
	/// <param name="value">Original value that the orb would use for its action.</param>
	/// <returns>New value that the orb should use for its action.</returns>
	public virtual decimal ModifyOrbValue(OrbModel orb, decimal value)
	{
		return value;
	}

	/// <summary>
	/// Add to the amount of a power that will be applied by a creature.
	/// Use this for effects that add a flat amount to the power, like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.SneckoSkull" />.
	/// This runs before <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyPowerAmountGivenMultiplicative(MegaCrit.Sts2.Core.Models.PowerModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	/// <param name="power">The power that will be applied.</param>
	/// <param name="giver">Creature who will be applying the power.</param>
	/// <param name="amount">Current amount of the power to be applied.</param>
	/// <param name="target">Optional creature who the power will be applied to.</param>
	/// <param name="cardSource">Optional card that applied the power.</param>
	/// <returns>Amount to add to the power.</returns>
	public virtual decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		return 0m;
	}

	/// <summary>
	/// Multiply the amount of a power that will be applied by a creature.
	/// Use this for effects that scale the power, like <see cref="T:MegaCrit.Sts2.Core.Models.Relics.UnsettlingLamp" />.
	/// This runs after <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyPowerAmountGivenAdditive(MegaCrit.Sts2.Core.Models.PowerModel,MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	/// <param name="power">The power that will be applied.</param>
	/// <param name="giver">Creature who will be applying the power.</param>
	/// <param name="amount">Current amount of the power to be applied (after additive modifiers).</param>
	/// <param name="target">Optional creature who the power will be applied to.</param>
	/// <param name="cardSource">Optional card that applied the power.</param>
	/// <returns>Amount that the power should be multiplied by.</returns>
	public virtual decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		return 1m;
	}

	/// <summary>
	/// Modify the amount to be healed at the rest site.
	/// </summary>
	/// <param name="creature">Creature who will be healing (usually the player, but Osty when playing Necrobinder).</param>
	/// <param name="amount">The original amount that would be healed.</param>
	/// <returns>The new amount to be healed.</returns>
	public virtual decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
	{
		return amount;
	}

	/// <summary>
	/// Modify the order in which cards are shuffled into the player's draw pile.
	/// </summary>
	/// <param name="player">Player whose draw pile is being shuffled.</param>
	/// <param name="cards">
	/// Cards that are being shuffled. Modifications made to this list will be reflected in the draw pile.
	/// </param>
	/// <param name="isInitialShuffle">
	/// True if this is the initial shuffle at the start of combat, false if this is a reshuffle during combat.
	/// </param>
	public virtual void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
	{
	}

	/// <summary>
	/// Modify the amount of a Summon.
	/// </summary>
	/// <param name="summoner">Player who is doing the summoning.</param>
	/// <param name="amount">Amount that is being summoned.</param>
	/// <param name="source">
	/// The model that this Summon came from. For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Bodyguard" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery" />
	/// pass themselves here.
	/// Null if the Summon did not come from any model (generally only relevant in tests).
	/// </param>
	/// <returns>The new amount to be summoned.</returns>
	public virtual decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel? source)
	{
		return amount;
	}

	/// <summary>
	/// Modify the creature that will receive unblocked damage.
	/// </summary>
	/// <param name="target">Creature who will receive the damage.</param>
	/// <param name="amount">Amount of damage to be dealt.</param>
	/// <param name="props">ValueProp for amount.</param>
	/// <param name="dealer">Creature who will be dealing the damage.</param>
	/// <returns>New creature to receive the unblocked damage.</returns>
	public virtual Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
	{
		return target;
	}

	/// <summary>
	/// Modify the current event for an event room
	/// </summary>
	/// <param name="currentEvent">The currently rolled event.</param>
	/// <returns>New event</returns>
	public virtual EventModel ModifyNextEvent(EventModel currentEvent)
	{
		return currentEvent;
	}

	/// <summary>
	/// Modify the room types that can be rolled when the player enters an unknown location.
	/// </summary>
	/// <param name="roomTypes">Room types that can be rolled.</param>
	/// <returns>New set of room types that can be rolled.</returns>
	public virtual IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		return roomTypes;
	}

	/// <summary>
	/// When a room type is not chosen as the resolved room for an unknown map point, this modifies the increased odds
	/// that it will show up the next time.
	/// </summary>
	/// <param name="roomType">The room type whose odds are being increased.</param>
	/// <param name="oddsIncrease">The amount the room would increase odds without modification.</param>
	/// <returns>The modified odds increase.</returns>
	public virtual float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
	{
		return oddsIncrease;
	}

	/// <summary>
	/// Modify the X-value used for an X-cost card's effect.
	/// </summary>
	/// <param name="card">Card whose X-value we're modifying</param>
	/// <param name="originalValue">Original X-value that we're modifying.</param>
	/// <returns>The new X-value to use.</returns>
	public virtual int ModifyXValue(CardModel card, int originalValue)
	{
		return originalValue;
	}

	/// <summary>
	/// Runs before a card is added to deck to give relics an opportunity to modify the card.
	/// Note that this can be run as part of transformation as well.
	/// The new owner can be determined by checking card.Owner.
	/// </summary>
	/// <param name="card">Card that was put into a deck.</param>
	/// <param name="newCard">New card that should be put into the deck instead.</param>
	public virtual bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		return false;
	}

	/// <summary>
	/// NOTE: You probably want to use TryModifyCardBeingAddedToDeck instead!
	/// Runs before a card is added to deck to give relics an opportunity to modify the card.
	/// Note that this can be run as part of transformation as well.
	/// The new owner can be determined by checking card.Owner.
	/// </summary>
	/// <param name="card">Card that was put into a deck.</param>
	/// <param name="newCard">New card that should be put into the deck instead.</param>
	public virtual bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		return false;
	}

	/// <summary>
	/// Modify the alternative options (options other than picking a card offered in a card reward).
	/// Ex: Skip, Heal +2 (Dream Catcher), Sacrifice (Pael Wing).
	/// </summary>
	/// <param name="player">Player to whom we are presenting the options.</param>
	/// <param name="cardReward">The reward that is being displayed.</param>
	/// <param name="alternatives">Card reward alternatives.</param>
	/// <returns>Whether the alternatives were modified.</returns>
	public virtual bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
	{
		return false;
	}

	/// <summary>
	/// Modifies the options for a card reward.
	/// </summary>
	/// <param name="player">The player receiving the rewards.</param>
	/// <param name="cardRewardOptions">Options in the card reward.</param>
	/// <param name="creationOptions">How the original cards were created.</param>
	public virtual bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
	{
		return false;
	}

	/// <summary>
	/// CAREFUL! You should usually use TryModifyCardRewardOptions instead of this.
	/// Modifies the options for a card reward.
	/// </summary>
	/// <param name="player">The player receiving the rewards.</param>
	/// <param name="cardRewardOptions">Options in the card reward.</param>
	/// <param name="creationOptions">How the original cards were created.</param>
	public virtual bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
	{
		return false;
	}

	/// <summary>
	/// Modify the amount of energy that a card will cost during combat.
	/// This will never be called while outside of combat.
	/// </summary>
	/// <param name="card">Card whose energy cost we're modifying.</param>
	/// <param name="originalCost">Original energy cost.</param>
	/// <param name="modifiedCost">Modified energy cost.</param>
	/// <returns>Whether the energy cost was modified</returns>
	public virtual bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		return false;
	}

	/// <summary>
	/// CAREFUL! You should usually use TryModifyEnergyCostInCombat instead of this.
	/// Modify the amount of energy that a card will cost during combat.
	/// This will never be called while outside of combat.
	/// </summary>
	/// <param name="card">Card whose energy cost we're modifying.</param>
	/// <param name="originalCost">Original energy cost.</param>
	/// <param name="modifiedCost">Modified energy cost.</param>
	/// <returns>Whether the energy cost was modified</returns>
	public virtual bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		return false;
	}

	/// <summary>
	/// Add or remove keywords on a card during combat. This is the global counterpart to the local keywords stored on
	/// the card itself (see <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.KeywordSources" />), and works just like <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.TryModifyEnergyCostInCombat(MegaCrit.Sts2.Core.Models.CardModel,System.Decimal,System.Decimal@)" />
	/// does for energy cost: a model contributes keywords for as long as it exists, with no explicit cleanup needed when
	/// it's removed.
	/// This will never be called while outside of combat.
	/// Mutate <paramref name="keywords" /> in place (add or remove).
	/// </summary>
	/// <param name="card">Card whose keywords we're modifying.</param>
	/// <param name="keywords">The card's current keyword set, to be mutated in place.</param>
	/// <returns>Whether the keyword set was modified.</returns>
	public virtual bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
	{
		return false;
	}

	/// <summary>
	/// Modify the amount of star that a card will cost.
	/// </summary>
	/// <param name="card">Card whose star cost we're modifying.</param>
	/// <param name="originalCost">Original star cost.</param>
	/// <param name="modifiedCost">Modified star cost.</param>
	/// <returns>Whether the energy cost was modified</returns>
	public virtual bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		return false;
	}

	/// <summary>
	/// Modify the amount of a power that will be applied to the target.
	/// </summary>
	/// <param name="canonicalPower">Canonical power that will be applied.</param>
	/// <param name="target">Creature who the power will be applied to.</param>
	/// <param name="amount">Original amount of the power to be applied.</param>
	/// <param name="applier">Optional creature who will be applying the power.</param>
	/// <param name="modifiedAmount">New amount of the power to be applied.</param>
	/// <returns>Whether the amount was modified.</returns>
	public virtual bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		return false;
	}

	/// <summary>
	/// Modify the options offered to the player when they enter a rest site.
	/// </summary>
	/// <param name="player">Player who owns the rest site options</param>
	/// <param name="options">Rest site options</param>
	/// <returns>Whether the options were modified.</returns>
	public virtual bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		return false;
	}

	/// <summary>
	/// Modify the rewards received when resting at the rest site. Normally this is empty.
	/// </summary>
	/// <param name="player">The player being offered rewards.</param>
	/// <param name="rewards">The rewards offered to the player.</param>
	/// <param name="isMimicked">
	/// Is a mimicked rest site heal?
	/// Used in spots that mimicking a rest site heal, but are not actual rest site heals.
	/// For example: <see cref="T:MegaCrit.Sts2.Core.Models.Events.DenseVegetation" /> mimicks a rest site heal when you select certain options.
	/// This should count as a rest site heal for <see cref="T:MegaCrit.Sts2.Core.Models.Relics.RegalPillow" />, but not for <see cref="T:MegaCrit.Sts2.Core.Models.Modifiers.NightTerrors" />.
	/// </param>
	/// <returns>Whether the rewards were modified.</returns>
	public virtual bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
	{
		return false;
	}

	/// <summary>
	/// Modify the rewards offered to the player.
	/// Remember that rewards can be generated in several ways:
	/// - As part of the end of an encounter
	/// - By relics like Orrery
	/// - At a rest site by Tiny Mailbox or Dreamcatcher
	/// - By an event like The Future of Potions
	/// </summary>
	/// <param name="player">The player being offered rewards.</param>
	/// <param name="rewards">The rewards offered to the player.</param>
	/// <param name="room">The room which the rewards are generated for. If this is null, then the rewards were generated
	/// as part of something that wasn't room completion (e.g. an event choice or a relic pickup).</param>
	/// <returns>Whether the rewards were modified.</returns>
	public virtual bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		return false;
	}

	/// <summary>
	/// CAREFUL! You should usually use TryModifyRewards instead of this.
	/// Modify the rewards offered to the player after completing a particular room/encounter.
	/// Works just like TryModifyRewards, but runs after it.
	/// </summary>
	/// <param name="player">The player being offered rewards.</param>
	/// <param name="rewards">Rewards</param>
	/// <param name="room">The room which the rewards are generated for. If this is null, then the rewards were generated
	/// as part of something that wasn't room completion (e.g. an event choice or a relic pickup).</param>
	/// <returns>Whether the rewards were modified.</returns>
	public virtual bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		return false;
	}

	/// <summary>
	/// Modify the extra text that will be displayed when the player heals at the rest site.
	/// Most implementors use this to add extra lines of text.
	/// </summary>
	/// <param name="player">The player who is at the rest site.</param>
	/// <param name="currentExtraText">The current extra text that will be displayed.</param>
	/// <returns>The new extra text that will be displayed.</returns>
	public virtual IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
	{
		return currentExtraText;
	}

	/// <summary>
	/// If a card would be added to the player's deck, should it still be?
	/// </summary>
	/// <param name="card">Card to be added.</param>
	/// <returns>Whether to allow it.</returns>
	public virtual bool ShouldAddToDeck(CardModel card)
	{
		return true;
	}

	/// <summary>
	/// Should we allow the specified affliction to be added to this card?
	/// </summary>
	/// <param name="card">Card to be afflicted.</param>
	/// <param name="affliction">Affliction that would be added to the card.</param>
	/// <returns>Whether to allow the affliction.</returns>
	public virtual bool ShouldAfflict(CardModel card, AfflictionModel affliction)
	{
		return true;
	}

	/// <summary>
	/// Should we allow the player to speak to this Ancient?
	/// Used by things like the Wax Choker relic.
	/// </summary>
	/// <param name="player">Player speaking to the Ancient.</param>
	/// <param name="ancient">Ancient the player is trying to speak to.</param>
	/// <returns>Whether to allow it.</returns>
	public virtual bool ShouldAllowAncient(Player player, AncientEventModel ancient)
	{
		return true;
	}

	/// <summary>
	/// If an effect would normally hit a creature, should it still?
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Creatures.Creature.IsHittable" /> for how hitting differs from targeting.
	/// </summary>
	/// <param name="creature">Creature who we want to hit.</param>
	/// <returns></returns>
	public virtual bool ShouldAllowHitting(Creature creature)
	{
		return true;
	}

	/// <summary>
	/// If a player tries to target a creature, should they be allowed to?
	/// </summary>
	/// <param name="target">Creature who we want to target.</param>
	/// <returns>Whether to allow player to target creature.</returns>
	public virtual bool ShouldAllowTargeting(Creature target)
	{
		return true;
	}

	/// <summary>
	/// Called after a reward is selected at the rewards screen to determine whether or not we should close the rewards screen.
	/// </summary>
	/// <param name="player">Player to whom we are presenting the options.</param>
	/// <param name="cardReward">The reward that is being displayed.</param>
	/// <returns>True to allow the player to continue to select rewards, false otherwise.</returns>
	public virtual bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
	{
		return false;
	}

	/// <summary>
	/// If a creature's block is about to be cleared, should it still be cleared?
	/// </summary>
	/// <param name="creature">Creature whose block is about to be cleared.</param>
	/// <returns>Whether the creature's block should still be cleared.</returns>
	public virtual bool ShouldClearBlock(Creature creature)
	{
		return true;
	}

	/// <summary>
	/// If a creature is about to die, should they still?
	/// </summary>
	/// <param name="creature">Creature who is about to die.</param>
	/// <returns>Whether the creature should still die.</returns>
	public virtual bool ShouldDie(Creature creature)
	{
		return true;
	}

	/// <summary>
	/// CAREFUL! You should usually use ShouldDie instead of this.
	/// If a creature is about to die, should they still?
	/// Works just like ShouldDie, but runs after it.
	/// </summary>
	/// <param name="creature">Creature who is about to die.</param>
	/// <returns>Whether the creature should still die.</returns>
	public virtual bool ShouldDieLate(Creature creature)
	{
		return true;
	}

	/// <summary>
	/// After the player selects a Rest Site option, should we still disable the rest of them?
	/// </summary>
	/// <returns>Whether to disable remaining Rest Site options.</returns>
	public virtual bool ShouldDisableRemainingRestSiteOptions(Player player)
	{
		return true;
	}

	/// <summary>
	/// If a player is about to draw cards, should they still?
	/// </summary>
	/// <param name="player">Player is about to draw cards.</param>
	/// <param name="fromHandDraw">draw is part of the initial hand draw at the start of your turn.</param>
	/// <returns>Whether the player should still draw cards.</returns>
	public virtual bool ShouldDraw(Player player, bool fromHandDraw)
	{
		return true;
	}

	/// <summary>
	/// If Ethereal is about to trigger for a card, should it still?
	/// </summary>
	/// <param name="card">The card that is about to be Exhausted due to Ethereal.</param>
	/// <returns>Whether the card should still be Exhausted.</returns>
	public virtual bool ShouldEtherealTrigger(CardModel card)
	{
		return true;
	}

	/// <summary>
	/// If a player is about to flush their hand, should they still?
	/// </summary>
	/// <param name="player">Player who is about to flush their hand.</param>
	/// <returns>Whether the player should still flush their hand.</returns>
	public virtual bool ShouldFlush(Player player)
	{
		return true;
	}

	/// <summary>
	/// If a player is about to gain stars, should they still?
	/// </summary>
	/// <param name="amount">Amount of stars they would gain.</param>
	/// <param name="player">Player who is about to gain stars.</param>
	/// <returns>Whether the player should still gain stars.</returns>
	public virtual bool ShouldGainStars(decimal amount, Player player)
	{
		return true;
	}

	/// <summary>
	/// If a player is about to receive treasure from a treasure room, should they?
	/// </summary>
	/// <param name="player">Player who is about to receive the treasure.</param>
	/// <returns>Whether the player should receive the treasure.</returns>
	public virtual bool ShouldGenerateTreasure(Player player)
	{
		return true;
	}

	/// <summary>
	/// If a player is about to pay an energy cost that they don't have enough energy for, should they be allowed to
	/// spend stars to pay for the excess energy cost?
	/// </summary>
	/// <param name="player">Player who is about to spend energy.</param>
	/// <returns>Whether the player can spend stars to pay for the excess energy cost.</returns>
	public virtual bool ShouldPayExcessEnergyCostWithStars(Player player)
	{
		return false;
	}

	/// <summary>
	/// If a card is about to be played, should it still?
	/// </summary>
	/// <param name="card">Card that is about to be played</param>
	/// <param name="autoPlayType">The type of autoplay this card play is (ie played by echo form). None if its not an auto play.</param>
	/// <returns>Whether the card should still be played.</returns>
	public virtual bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		return true;
	}

	/// <summary>
	/// If the player's energy is about to be reset, should it be reset?
	/// If not, the player's max energy is still added on to the current amount.
	/// </summary>
	/// <returns>Whether the player's energy should be reset.</returns>
	public virtual bool ShouldPlayerResetEnergy(Player player)
	{
		return true;
	}

	/// <summary>
	/// If the player is attempting to proceed to the next point on the map, should they be allowed to?
	/// </summary>
	/// <returns>Whether to allow it.</returns>
	public virtual bool ShouldProceedToNextMapPoint()
	{
		return true;
	}

	/// <summary>
	/// If the player would procure the specified potion, should they still?
	/// </summary>
	/// <param name="potion">Potion we are procuring.</param>
	/// <param name="player">Player trying to procure the potion.</param>
	/// <returns>Whether to allow it.</returns>
	public virtual bool ShouldProcurePotion(PotionModel potion, Player player)
	{
		return true;
	}

	/// <summary>
	/// If the power should be removed after the creature has died. For things like Calcify
	/// which can prevent this.
	/// </summary>
	/// <returns>Whether the power should be removed on death</returns>
	public virtual bool ShouldPowerBeRemovedOnDeath(PowerModel power)
	{
		return true;
	}

	/// <summary>
	/// If the player has just purchased something from the merchant, should the now-empty entry be refilled?
	/// </summary>
	/// <param name="entry">Entry that might be refilled.</param>
	/// <param name="player">Player who is at the merchant.</param>
	/// <returns>Whether the merchant entry should be refilled.</returns>
	public virtual bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
	{
		return false;
	}

	/// <summary>
	/// At the merchant, should the player be allowed to remove cards?
	/// </summary>
	/// <param name="player">Player who is at the merchant.</param>
	/// <returns>True if the player should be allowed to remove cards, false otherwise.</returns>
	public virtual bool ShouldAllowMerchantCardRemoval(Player player)
	{
		return true;
	}

	/// <summary>
	/// If this creature just died, should it be removed from combat?
	/// Usually true, but things like reviving powers should return false so they can revive the creature later.
	/// </summary>
	/// <param name="creature">Creature that died.</param>
	/// <returns>Whether the creature should be removed from combat.</returns>
	public virtual bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		return true;
	}

	/// <summary>
	/// If all enemies are dead, should we stop combat from ending?
	/// This should be set to true when all enemies are dead but new enemies are about to spawn.
	/// </summary>
	/// <returns>Whether combat should be marked as about to end</returns>
	public virtual bool ShouldStopCombatFromEnding()
	{
		return false;
	}

	/// <summary>
	/// If the player should be allowed to take an extra turn.
	/// </summary>
	/// <param name="player">Player ending turn.</param>
	/// <returns>Whether to allow it.</returns>
	public virtual bool ShouldTakeExtraTurn(Player player)
	{
		return false;
	}

	/// <summary>
	/// If true is returned from this hook, a potion will always be generated in the rewards for the room, no matter what.
	/// </summary>
	/// <param name="player">Player who is receiving the reward.</param>
	/// <param name="roomType">Type of room that the reward is being offered in.</param>
	public virtual bool ShouldForcePotionReward(Player player, RoomType roomType)
	{
		return false;
	}

	/// <summary>
	/// When the player is able to choose a map point, should they be allowed to choose any map point in the next row?
	/// </summary>
	public virtual bool ShouldAllowFreeTravel()
	{
		return false;
	}

	public override string ToString()
	{
		return $"{Id} ({RuntimeHelpers.GetHashCode(this)})";
	}

	protected void NeverEverCallThisOutsideOfTests_SetIsMutable(bool isMutable)
	{
		if (TestMode.IsOff)
		{
			throw new InvalidOperationException("You monster!");
		}
		IsMutable = isMutable;
	}
}
