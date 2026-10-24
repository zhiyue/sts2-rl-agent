using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Runs;

public class RunRngSet
{
	private static readonly RunRngSet _mockInstance = new RunRngSet(string.Empty);

	private readonly Dictionary<RunRngType, Rng> _rngs = new Dictionary<RunRngType, Rng>();

	/// <summary>
	/// We generate a string for the seed that gets hashed to a uint for the actual thing passed to the RNGs.
	/// This is the original string that was input, for display purposes.
	/// </summary>
	public string StringSeed { get; }

	/// <summary>
	/// The seed that was hashed from the InputSeed.
	/// </summary>
	public uint Seed { get; }

	/// <summary>
	/// Determines everything that's generated upfront when a run first starts. This includes:
	/// * Which monsters you'll fight.
	/// * Which events you'll run into.
	/// * Which relics you'll be offered.
	/// </summary>
	public Rng UpFront => GetRng(RunRngType.UpFront);

	/// <summary>
	/// Determines how your draw pile gets shuffled, both at the start of combat and when you
	/// run out of cards in it.
	/// </summary>
	public Rng Shuffle => GetRng(RunRngType.Shuffle);

	/// <summary>
	/// Determines what types of room is rolled when visiting an unknown map point.
	/// </summary>
	public Rng UnknownMapPoint => GetRng(RunRngType.UnknownMapPoint);

	/// <summary>
	/// Determines what cards are generated during combat by things like Attack Potion.
	/// Distinct from CardRewards because we don't want Attack Potion usage to impact card rewards.
	/// </summary>
	public Rng CombatCardGeneration => GetRng(RunRngType.CombatCardGeneration);

	/// <summary>
	/// Determines what potions are generated during combat by things like Alchemize.
	/// Distinct from PotionRewards because these can't generate healing potions.
	/// </summary>
	public Rng CombatPotionGeneration => GetRng(RunRngType.CombatPotionGeneration);

	/// <summary>
	/// Determines what cards are randomly chosen during combat by things like True Grit.
	/// </summary>
	public Rng CombatCardSelection => GetRng(RunRngType.CombatCardSelection);

	/// <summary>
	/// Determines random in-combat energy costs for things like Confusion and Snecko Eye.
	/// </summary>
	public Rng CombatEnergyCosts => GetRng(RunRngType.CombatEnergyCosts);

	/// <summary>
	/// Determines the results of random targeting during combat (Bouncing Flask, Sword Boomerang, etc.).
	/// </summary>
	public Rng CombatTargets => GetRng(RunRngType.CombatTargets);

	/// <summary>
	/// Determines what moves each monster makes whenever there's randomness involved.
	/// </summary>
	public Rng MonsterAi => GetRng(RunRngType.MonsterAi);

	/// <summary>
	/// Determines some niche one-off RNG stuff that we don't care about interacting, like the <see cref="T:MegaCrit.Sts2.Core.Models.Modifiers.CursedRun" />
	/// modifier.
	/// </summary>
	public Rng Niche => GetRng(RunRngType.Niche);

	/// <summary>
	/// Determines what orbs are randomly chosen during combat by things like Chaos.
	/// </summary>
	public Rng CombatOrbGeneration => GetRng(RunRngType.CombatOrbs);

	/// <summary>
	/// Determines who gets treasure when multiple players pick the same relic at the treasure room.
	/// </summary>
	public Rng TreasureRoomRelics => GetRng(RunRngType.TreasureRoomRelics);

	public static RunRngSet GetMockInstance()
	{
		if (TestMode.IsOff)
		{
			throw new InvalidOperationException("Cannot get RunRng when not in a run outside of tests!");
		}
		return _mockInstance;
	}

	public RunRngSet(string seed)
	{
		StringSeed = seed;
		Seed = (uint)StringHelper.GetDeterministicHashCode(seed);
		RunRngType[] values = Enum.GetValues<RunRngType>();
		foreach (RunRngType runRngType in values)
		{
			_rngs[runRngType] = CreateRng(runRngType);
		}
	}

	private Rng CreateRng(RunRngType rngType)
	{
		string name = StringHelper.SnakeCase(rngType.ToString());
		return new Rng(Seed, name);
	}

	public SerializableRunRngSet ToSerializable()
	{
		SerializableRunRngSet serializableRunRngSet = new SerializableRunRngSet
		{
			Seed = StringSeed
		};
		foreach (var (key, rng2) in _rngs)
		{
			serializableRunRngSet.Counters[key] = rng2.Counter;
		}
		return serializableRunRngSet;
	}

	public static RunRngSet FromSave(SerializableRunRngSet save)
	{
		RunRngSet runRngSet = new RunRngSet(save.Seed);
		foreach (KeyValuePair<RunRngType, int> counter in save.Counters)
		{
			counter.Deconstruct(out var key, out var value);
			RunRngType runRngType = key;
			int targetCount = value;
			Rng rng = runRngSet.CreateRng(runRngType);
			rng.FastForwardCounter(targetCount);
			runRngSet._rngs[runRngType] = rng;
		}
		return runRngSet;
	}

	public void LoadFromSerializable(SerializableRunRngSet save)
	{
		if (StringSeed != save.Seed)
		{
			throw new NotImplementedException("RngSet seed should not change during the run!");
		}
		foreach (KeyValuePair<RunRngType, int> counter in save.Counters)
		{
			counter.Deconstruct(out var key, out var value);
			RunRngType runRngType = key;
			int num = value;
			Rng rng = _rngs[runRngType];
			if (num < rng.Counter)
			{
				rng = CreateRng(runRngType);
				rng.FastForwardCounter(num);
				_rngs[runRngType] = rng;
			}
			else
			{
				_rngs[runRngType].FastForwardCounter(num);
			}
		}
	}

	/// <summary>
	/// ONLY USE THIS FOR TESTING!
	/// Mock out the specified RNG type to use the specified seed.
	/// </summary>
	public void MockRng(RunRngType rngType, uint seed)
	{
		_rngs[rngType] = new Rng(seed);
	}

	private Rng GetRng(RunRngType rngType)
	{
		return _rngs[rngType];
	}
}
