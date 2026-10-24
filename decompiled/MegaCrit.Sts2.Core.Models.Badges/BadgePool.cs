using System.Collections.Generic;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Badges;

public static class BadgePool
{
	public static IReadOnlyCollection<Badge> CreateAll(SerializableRun run, ulong playerId, bool won)
	{
		return new global::_003C_003Ez__ReadOnlyArray<Badge>(new Badge[23]
		{
			new CccCombo(run, won, playerId),
			new Curses(run, won, playerId),
			new DamageLeader(run, won, playerId),
			new Debuffer(run, won, playerId),
			new DoubleSnecko(run, won, playerId),
			new EliteKiller(run, won, playerId),
			new Famished(run, won, playerId),
			new Glutton(run, won, playerId),
			new Healer(run, won, playerId),
			new Highlander(run, won, playerId),
			new Honed(run, won, playerId),
			new BigDeck(run, won, playerId),
			new ILikeShiny(run, won, playerId),
			new KaChing(run, won, playerId),
			new MoneyMoney(run, won, playerId),
			new MysteryMachine(run, won, playerId),
			new Perfect(run, won, playerId),
			new Restful(run, won, playerId),
			new Restless(run, won, playerId),
			new Speedy(run, won, playerId),
			new TabletBadge(run, won, playerId),
			new TeamPlayer(run, won, playerId),
			new TinyDeck(run, won, playerId)
		});
	}
}
