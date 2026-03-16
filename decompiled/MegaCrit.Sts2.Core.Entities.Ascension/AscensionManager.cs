using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Entities.Ascension;

public class AscensionManager
{
	public const int maxAscensionAllowed = 10;

	private readonly int _level;

	public AscensionManager(int level)
	{
		_level = level;
	}

	public AscensionManager(AscensionLevel level)
	{
		_level = (int)level;
	}

	public bool HasLevel(AscensionLevel level)
	{
		return _level >= (int)level;
	}

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
