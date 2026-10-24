using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Combat;

public interface ICombatState
{
	/// <summary>
	/// The state of the run that this combat exists in.
	/// Will be <see cref="P:MegaCrit.Sts2.Core.Combat.ICombatState.RunState" /> in gameplay and <see cref="T:MegaCrit.Sts2.Core.Runs.NullRunState" /> in some test/debug scenarios.
	/// </summary>
	IRunState RunState { get; }

	/// <summary>
	/// Get all creatures on the Allies side.
	/// </summary>
	IReadOnlyList<Creature> Allies { get; }

	/// <summary>
	/// Get all creatures on the Enemies side.
	/// </summary>
	IReadOnlyList<Creature> Enemies { get; }

	/// <summary>
	/// Get all creatures in the combat on all sides.
	/// </summary>
	IReadOnlyList<Creature> Creatures { get; }

	/// <summary>
	/// Get all the player creatures in the combat.
	/// </summary>
	IReadOnlyList<Creature> PlayerCreatures { get; }

	/// <summary>
	/// Get all players in the combat.
	/// </summary>
	IReadOnlyList<Player> Players { get; }

	/// <summary>
	/// List of custom modifiers applied to this combat, usually via daily or custom runs.
	/// </summary>
	IReadOnlyList<ModifierModel> Modifiers { get; }

	/// <summary>
	/// The model used to scale various things (block, power application) in multiplayer.
	/// </summary>
	MultiplayerScalingModel? MultiplayerScalingModel { get; }

	/// <summary>
	/// BE CAREFUL! You usually want <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.TurnNumber" /> instead of this.
	/// The round of combat that we're on in this combat.
	/// A "round" encompasses both the player's turn (or turns if a player takes an extra turn) and the enemy's turn.
	/// This starts at 1, so it should never be 0.
	/// </summary>
	int RoundNumber { get; set; }

	/// <summary>
	/// The side that is active in this combat.
	/// A round starts with the player side being active, then switches to the enemy side after all players have ended
	/// their turn.
	/// When the enemy turn ends, the current side changes back to the player side, and the round number is incremented.
	/// </summary>
	CombatSide CurrentSide { get; set; }

	EncounterModel? Encounter { get; }

	/// <summary>
	/// A list of creatures that escaped (were removed without dying) during the encounter. Used when rewards are given.
	/// </summary>
	IReadOnlyList<Creature> EscapedCreatures { get; }

	/// <summary>
	/// Get all the creatures on the currently-active side.
	/// </summary>
	IReadOnlyList<Creature> CreaturesOnCurrentSide { get; }

	/// <summary>
	/// Get all hittable enemies.
	/// See <see cref="P:MegaCrit.Sts2.Core.Entities.Creatures.Creature.IsHittable" /> for a definition.
	///
	/// NOTE: We shouldn't add too many methods like this, this one is just extremely common because it's used for AOE
	/// and random attack targeting.
	/// </summary>
	IReadOnlyList<Creature> HittableEnemies { get; }

	/// <summary>
	/// Fired whenever the arrangement of creatures in the combat changes. Specifically, when:
	/// * A creature is added.
	/// * A creature is removed.
	/// * A creature's index changes.
	/// </summary>
	event Action<ICombatState>? CreaturesChanged;

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard``1(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	T CreateCard<T>(Player owner) where T : CardModel;

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	CardModel CreateCard(CardModel canonicalCard, Player owner);

	/// <summary>
	/// WARNING: If you're specifically intending to create a clone in combat for an effect like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.DualWield" />,
	/// you should use <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.CreateClone" /> instead.
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CloneCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	CardModel CloneCard(CardModel mutableCard);

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.AddCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	void AddCard(CardModel card, Player owner);

	void RemoveCard(CardModel card);

	/// <summary>
	/// Does this state contain the specified card?
	/// </summary>
	bool ContainsCard(CardModel card);

	/// <summary>
	/// Add a player's creature on the Allies side.
	/// </summary>
	void AddPlayer(Player player);

	/// <summary>
	/// Creates a new creature from a monster in this combat state.
	/// After you create the creature, you should call AddCreature to add it to combat. This can be done immediately
	/// or after some time.
	/// </summary>
	Creature CreateCreature(MonsterModel monster, CombatSide side, string? slot);

	/// <summary>
	/// Call this to remove a creature that escaped rather than dying.
	/// </summary>
	void CreatureEscaped(Creature creature);

	/// <summary>
	/// Removes the creature from combat.
	/// </summary>
	/// <param name="creature">The creature that will be removed.</param>
	/// <param name="unattach">If true, then the creature cannot be re-added to the combat state.</param>
	void RemoveCreature(Creature creature, bool unattach = true);

	bool ContainsCreature(Creature creature);

	bool ContainsMonster<T>() where T : MonsterModel;

	/// <summary>
	/// Get the creature with the specified combat ID. Null if not found.
	/// </summary>
	Creature? GetCreature(uint? combatId);

	/// <summary>
	/// Get the creature with the specified combat ID.
	/// If the creature doesn't exist, keep checking for a while before timing out and returning null.
	/// </summary>
	/// <param name="combatId">Combat ID of the creature to get.</param>
	/// <param name="timeoutSec">How long to wait for the creature to appear.</param>
	/// <returns>Specified Creature, or null if it doesn't exist and we've waited long enough.</returns>
	Task<Creature?> GetCreatureAsync(uint? combatId, double timeoutSec);

	/// <summary>
	/// Get all the creatures on the specified side.
	/// </summary>
	IReadOnlyList<Creature> GetCreaturesOnSide(CombatSide side);

	/// <summary>
	/// Get all opponents of a creature.
	/// </summary>
	IReadOnlyList<Creature> GetOpponentsOf(Creature creature);

	/// <summary>
	/// Get all teammates of a creature, including the creature itself.
	/// </summary>
	IReadOnlyList<Creature> GetTeammatesOf(Creature creature);

	Player? GetPlayer(ulong playerId);

	/// <summary>
	/// Get all the models that should have combat hooks called on them.
	/// </summary>
	IEnumerable<AbstractModel> IterateHookListeners();

	void SortEnemiesBySlotName();

	void SetEnemyIndex(Creature creature, int index);

	/// <summary>
	/// Adds a creature to the combat.
	/// This should almost always be called right after CreateCreature. Until this is called, creatures cannot be targeted
	/// and their powers will not be triggered. It is separate so that creatures can be removed from combat and re-added
	/// to it later. Currently, this is only used in the Doormaker combat.
	/// </summary>
	/// <param name="creature">The creature to add to the combat.</param>
	void AddCreature(Creature creature);

	/// <summary>
	/// Returns true if this represents a live combat.
	/// If this is false, then you should only trust that whatever object you retrieved this combat state from (like a
	/// Creature) exists, and not any other object, like other Creatures or the NCombatRoom.
	/// </summary>
	bool IsLiveCombat();
}
