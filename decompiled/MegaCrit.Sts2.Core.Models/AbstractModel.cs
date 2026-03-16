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

	public int CategorySortingId { get; private set; }

	public int EntrySortingId { get; private set; }

	public virtual bool PreviewOutsideOfCombat => false;

	public abstract bool ShouldReceiveCombatHooks { get; }

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

	public void AssertMutable()
	{
		if (!IsMutable)
		{
			throw new CanonicalModelException(GetType());
		}
	}

	public void AssertCanonical()
	{
		if (IsMutable)
		{
			throw new MutableModelException(GetType());
		}
	}

	public AbstractModel ClonePreservingMutability()
	{
		if (!IsMutable)
		{
			return this;
		}
		return MutableClone();
	}

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

	protected virtual void AfterCloned()
	{
		this.ExecutionFinished = null;
	}

	public void InvokeExecutionFinished()
	{
		this.ExecutionFinished?.Invoke(this);
	}

	public virtual Task AfterActEntered()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterAddToDeckPrevented(CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeAttack(AttackCommand command)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterAttack(AttackCommand command)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterBlockCleared(Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterBlockBroken(Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardEnteredCombat(CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeCardPlayed(CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCardRetained(CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeCombatStart()
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeCombatStartLate()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCombatEnd(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCombatVictoryEarly(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCombatVictory(CombatRoom room)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCreatureAddedToCombat(Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeDeath(Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterEnergyReset(Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterEnergyResetLate(Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterEnergySpent(CardModel card, int amount)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeCardRemoved(CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterGoldGained(Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterMapGenerated(ActMap map, int actIndex)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingCardPlayCount(CardModel card)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingCardRewardOptions()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingHandDraw()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPreventingDraw()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingHpLostBeforeOsty()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingHpLostAfterOsty()
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingPowerAmountReceived(PowerModel power)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingPowerAmountGiven(PowerModel power)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterModifyingRewards()
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeRewardsOffered(Player player, IReadOnlyList<Reward> rewards)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterOstyRevived(Creature osty)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforePotionUsed(PotionModel potion, Creature? target)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPotionUsed(PotionModel potion, Creature? target)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPotionDiscarded(PotionModel potion)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPotionProcured(PotionModel potion)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPreventingDeath(Creature creature)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRestSiteSmith(Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRewardTaken(Player player, Reward reward)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeRoomEntered(AbstractRoom room)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterRoomEntered(AbstractRoom room)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterStarsSpent(int amount, Player spender)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterStarsGained(int amount, Player gainer)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterForge(decimal amount, Player forger, AbstractModel? source)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterTakingExtraTurn(Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterTargetingBlockedVfx(Creature blocker)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, Player player)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public virtual Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
	{
		return Task.CompletedTask;
	}

	public virtual int ModifyAttackHitCount(AttackCommand attack, int hitCount)
	{
		return hitCount;
	}

	public virtual decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		return 0m;
	}

	public virtual decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		return 1m;
	}

	public virtual int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		return playCount;
	}

	public virtual (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		return (pileType, position);
	}

	public virtual int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
	{
		return triggerCount;
	}

	public virtual CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		return options;
	}

	public virtual CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
	{
		return options;
	}

	public virtual decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
	{
		return odds;
	}

	public virtual decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return 0m;
	}

	public virtual decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return decimal.MaxValue;
	}

	public virtual decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return 1m;
	}

	public virtual ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		return map;
	}

	public virtual ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		return map;
	}

	public virtual decimal ModifyHandDraw(Player player, decimal count)
	{
		return count;
	}

	public virtual decimal ModifyHandDrawLate(Player player, decimal count)
	{
		return count;
	}

	public virtual decimal ModifyHealAmount(Creature creature, decimal amount)
	{
		return amount;
	}

	public virtual decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	public virtual decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	public virtual decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	public virtual decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return amount;
	}

	public virtual decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		return amount;
	}

	public virtual IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
	{
		return options;
	}

	public virtual CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
	{
		return rarity;
	}

	public virtual void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
	}

	public virtual decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
	{
		return cost;
	}

	public virtual decimal ModifyOrbValue(Player player, decimal value)
	{
		return value;
	}

	public virtual decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		return amount;
	}

	public virtual decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
	{
		return amount;
	}

	public virtual void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
	{
	}

	public virtual decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel? source)
	{
		return amount;
	}

	public virtual Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
	{
		return target;
	}

	public virtual EventModel ModifyNextEvent(EventModel currentEvent)
	{
		return currentEvent;
	}

	public virtual IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
	{
		return roomTypes;
	}

	public virtual float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
	{
		return oddsIncrease;
	}

	public virtual int ModifyXValue(CardModel card, int originalValue)
	{
		return originalValue;
	}

	public virtual bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		return false;
	}

	public virtual bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		return false;
	}

	public virtual bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
	{
		return false;
	}

	public virtual bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
	{
		return false;
	}

	public virtual bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
	{
		return false;
	}

	public virtual bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		return false;
	}

	public virtual bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		return false;
	}

	public virtual bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		return false;
	}

	public virtual bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		return false;
	}

	public virtual bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
	{
		return false;
	}

	public virtual bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		return false;
	}

	public virtual bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		return false;
	}

	public virtual IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
	{
		return currentExtraText;
	}

	public virtual bool ShouldAddToDeck(CardModel card)
	{
		return true;
	}

	public virtual bool ShouldAfflict(CardModel card, AfflictionModel affliction)
	{
		return true;
	}

	public virtual bool ShouldAllowAncient(Player player, AncientEventModel ancient)
	{
		return true;
	}

	public virtual bool ShouldAllowHitting(Creature creature)
	{
		return true;
	}

	public virtual bool ShouldAllowTargeting(Creature target)
	{
		return true;
	}

	public virtual bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
	{
		return false;
	}

	public virtual bool ShouldClearBlock(Creature creature)
	{
		return true;
	}

	public virtual bool ShouldDie(Creature creature)
	{
		return true;
	}

	public virtual bool ShouldDieLate(Creature creature)
	{
		return true;
	}

	public virtual bool ShouldDisableRemainingRestSiteOptions(Player player)
	{
		return true;
	}

	public virtual bool ShouldDraw(Player player, bool fromHandDraw)
	{
		return true;
	}

	public virtual bool ShouldEtherealTrigger(CardModel card)
	{
		return true;
	}

	public virtual bool ShouldFlush(Player player)
	{
		return true;
	}

	public virtual bool ShouldGainGold(decimal amount, Player player)
	{
		return true;
	}

	public virtual bool ShouldGainStars(decimal amount, Player player)
	{
		return true;
	}

	public virtual bool ShouldGenerateTreasure(Player player)
	{
		return true;
	}

	public virtual bool ShouldPayExcessEnergyCostWithStars(Player player)
	{
		return false;
	}

	public virtual bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		return true;
	}

	public virtual bool ShouldPlayerResetEnergy(Player player)
	{
		return true;
	}

	public virtual bool ShouldProceedToNextMapPoint()
	{
		return true;
	}

	public virtual bool ShouldProcurePotion(PotionModel potion, Player player)
	{
		return true;
	}

	public virtual bool ShouldPowerBeRemovedOnDeath(PowerModel power)
	{
		return true;
	}

	public virtual bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
	{
		return false;
	}

	public virtual bool ShouldAllowMerchantCardRemoval(Player player)
	{
		return true;
	}

	public virtual bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		return true;
	}

	public virtual bool ShouldStopCombatFromEnding()
	{
		return false;
	}

	public virtual bool ShouldTakeExtraTurn(Player player)
	{
		return false;
	}

	public virtual bool ShouldForcePotionReward(Player player, RoomType roomType)
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
