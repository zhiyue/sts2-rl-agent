using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class UnlockConsoleCmd : AbstractConsoleCmd
{
	private static readonly Dictionary<string, Action<UnlockConsoleCmd, List<string>?>> _unlockActions = new Dictionary<string, Action<UnlockConsoleCmd, List<string>>>
	{
		["cards"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockCards(s);
		},
		["potions"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockPotions(s);
		},
		["relics"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockRelics(s);
		},
		["monsters"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockMonsters(s);
		},
		["events"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockEvents(s);
		},
		["epochs"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockEpochs(s);
		},
		["ascensions"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockAscensions(s);
		},
		["all"] = delegate(UnlockConsoleCmd c, List<string>? s)
		{
			c.UnlockCards(null);
			c.UnlockPotions(null);
			c.UnlockRelics(null);
			c.UnlockMonsters(null);
			c.UnlockEvents(null);
			c.UnlockEpochs(null);
			c.UnlockAscensions(null);
		}
	};

	private static readonly List<string> _validDiscoveryTypes = _unlockActions.Keys.ToList();

	private static readonly Dictionary<string, Func<IEnumerable<string>>> _validIdSources = new Dictionary<string, Func<IEnumerable<string>>>
	{
		["cards"] = () => ModelDb.AllCards.Select((CardModel c) => c.Id.Entry),
		["potions"] = () => ModelDb.AllPotions.Select((PotionModel c) => c.Id.Entry),
		["relics"] = () => ModelDb.AllRelics.Select((RelicModel c) => c.Id.Entry),
		["monsters"] = () => ModelDb.Monsters.Select((MonsterModel c) => c.Id.Entry),
		["events"] = () => ModelDb.AllEvents.Select((EventModel c) => c.Id.Entry),
		["epochs"] = () => EpochModel.AllEpochIds
	};

	public override string CmdName => "unlock";

	public override string Args => "<type:string>";

	public override string Description => "Marks all cards/potions/relics/monsters/events/epochs/ascensions as discovered, or 'all' to unlock everything.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, "No argument specified.\n" + Args);
		}
		List<string> args2 = ((args.Length > 1) ? args[1..].ToList() : null);
		return ProcessUnlock(args[0], args2);
	}

	private CmdResult ProcessUnlock(string discoveryType, List<string>? args)
	{
		if (!_unlockActions.TryGetValue(discoveryType, out Action<UnlockConsoleCmd, List<string>> value))
		{
			return new CmdResult(success: false, "Argument " + discoveryType + " not recognized as a discovery type.\n" + Args);
		}
		if (args != null)
		{
			for (int i = 0; i < args.Count; i++)
			{
				args[i] = args[i].ToUpperInvariant();
			}
			if (_validIdSources.TryGetValue(discoveryType, out Func<IEnumerable<string>> value2))
			{
				HashSet<string> validIds = value2().ToHashSet();
				List<string> list = args.Where((string id) => !validIds.Contains(id)).ToList();
				if (list.Count > 0)
				{
					return new CmdResult(success: false, "Unknown " + discoveryType + ": " + string.Join(", ", list));
				}
			}
		}
		value(this, args);
		SaveManager.Instance.SaveProgressFile();
		return new CmdResult(success: true, "Unlocked " + discoveryType + " " + ((args != null) ? string.Join(", ", args) : ""));
	}

	private void UnlockCards(List<string>? cards)
	{
		IEnumerable<ModelId> enumerable = cards?.Select((string c) => new ModelId("CARD", c)) ?? ModelDb.AllCards.Select((CardModel c) => c.Id);
		foreach (ModelId item in enumerable)
		{
			SaveManager.Instance.Progress.MarkCardAsSeen(item);
		}
	}

	private void UnlockRelics(List<string>? relics)
	{
		IEnumerable<ModelId> enumerable = relics?.Select((string c) => new ModelId("RELIC", c)) ?? ModelDb.AllRelics.Select((RelicModel c) => c.Id);
		foreach (ModelId item in enumerable)
		{
			SaveManager.Instance.Progress.MarkRelicAsSeen(item);
		}
	}

	private void UnlockPotions(List<string>? potions)
	{
		IEnumerable<ModelId> enumerable = potions?.Select((string c) => new ModelId("POTION", c)) ?? ModelDb.AllPotions.Select((PotionModel c) => c.Id);
		foreach (ModelId item in enumerable)
		{
			SaveManager.Instance.Progress.MarkPotionAsSeen(item);
		}
	}

	private void UnlockMonsters(List<string>? monsters)
	{
		IEnumerable<ModelId> enumerable = monsters?.Select((string c) => new ModelId("MONSTER", c)) ?? ModelDb.Monsters.Select((MonsterModel c) => c.Id);
		foreach (ModelId item in enumerable)
		{
			EnemyStats orCreateEnemyStats = SaveManager.Instance.Progress.GetOrCreateEnemyStats(item);
			if (orCreateEnemyStats.FightStats.Count == 0)
			{
				orCreateEnemyStats.FightStats.Add(new FightStats
				{
					Character = ModelDb.GetId<Ironclad>(),
					Wins = 1
				});
			}
		}
	}

	private void UnlockEvents(List<string>? events)
	{
		IEnumerable<ModelId> enumerable = events?.Select((string c) => new ModelId("EVENT", c)) ?? ModelDb.AllEvents.Select((EventModel c) => c.Id);
		foreach (ModelId item in enumerable)
		{
			SaveManager.Instance.Progress.MarkEventAsSeen(item);
		}
	}

	private void UnlockEpochs(List<string>? epochs)
	{
		IEnumerable<string> enumerable = (epochs ?? EpochModel.AllEpochIds).Except(from e in SaveManager.Instance.Progress.Epochs
			where e.State == EpochState.Revealed
			select e.Id);
		foreach (string item in enumerable)
		{
			SaveManager.Instance.ObtainEpochOverride(item, EpochState.Revealed);
		}
	}

	private void UnlockAscensions(List<string>? ascensions)
	{
		if (ascensions != null)
		{
			throw new NotImplementedException();
		}
		SaveManager.Instance.Progress.MaxMultiplayerAscension = 10;
		foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
		{
			CharacterStats orCreateCharacterStats = SaveManager.Instance.Progress.GetOrCreateCharacterStats(allCharacter.Id);
			orCreateCharacterStats.MaxAscension = 10;
		}
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			string partial = ((args.Length != 0) ? args[0] : "");
			List<string> candidates = ((!string.IsNullOrWhiteSpace(partial)) ? _validDiscoveryTypes.Where((string type) => type.Contains(partial, StringComparison.OrdinalIgnoreCase)).ToList() : _validDiscoveryTypes);
			return new CompletionResult
			{
				Candidates = candidates,
				Type = CompletionType.Argument,
				ArgumentContext = CmdName
			};
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
