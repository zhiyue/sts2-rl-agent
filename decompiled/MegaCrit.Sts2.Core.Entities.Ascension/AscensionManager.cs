using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Entities.Ascension;

/// <summary>
/// Manages ascension logic for runs.
/// Responsible for tracking ascension levels and applying ascension effects.
/// </summary>
public class AscensionManager
{
	/// <summary>
	/// The maximum ascension level allowed in the game.
	/// </summary>
	public const int maxAscensionAllowed = 10;

	/// <summary>
	/// The current ascension level for the run.
	/// </summary>
	private readonly int _level;

	/// <summary>
	/// Create a new AscensionManager with the specified ascension level.
	/// </summary>
	/// <param name="level">The ascension level for this run.</param>
	public AscensionManager(int level)
	{
		_level = level;
	}

	/// <summary>
	/// Create a new AscensionManager with the specified ascension level.
	/// </summary>
	/// <param name="level">The ascension level enum value for this run.</param>
	public AscensionManager(AscensionLevel level)
	{
		_level = (int)level;
	}

	/// <summary>
	/// Check if a given ascension level is active in the current run.
	/// </summary>
	/// <param name="level">The ascension level to check.</param>
	/// <returns>True if the specified level is active.</returns>
	public bool HasLevel(AscensionLevel level)
	{
		return _level >= (int)level;
	}

	/// <summary>
	/// Apply ascension effects to a player based on the current ascension level.
	/// </summary>
	/// <param name="player">The player to apply effects to.</param>
	public void ApplyEffectsTo(Player player)
	{
		if (HasLevel(AscensionLevel.TightBelt))
		{
			player.SubtractFromMaxPotionCount(1);
		}
		if (HasLevel(AscensionLevel.AscendersBane))
		{
			AscendersBane ascendersBane = player.RunState.CreateCard<AscendersBane>(player);
			ascendersBane.FloorAddedToDeck = 1;
			player.Deck.AddInternal(ascendersBane, -1, silent: true);
		}
	}
}
