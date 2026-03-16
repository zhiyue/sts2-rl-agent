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

	public string StringSeed { get; }

	public uint Seed { get; }

	public Rng UpFront => GetRng(RunRngType.UpFront);

	public Rng Shuffle => GetRng(RunRngType.Shuffle);

	public Rng UnknownMapPoint => GetRng(RunRngType.UnknownMapPoint);

	public Rng CombatCardGeneration => GetRng(RunRngType.CombatCardGeneration);

	public Rng CombatPotionGeneration => GetRng(RunRngType.CombatPotionGeneration);

	public Rng CombatCardSelection => GetRng(RunRngType.CombatCardSelection);

	public Rng CombatEnergyCosts => GetRng(RunRngType.CombatEnergyCosts);

	public Rng CombatTargets => GetRng(RunRngType.CombatTargets);

	public Rng MonsterAi => GetRng(RunRngType.MonsterAi);

	public Rng Niche => GetRng(RunRngType.Niche);

	public Rng CombatOrbGeneration => GetRng(RunRngType.CombatOrbs);

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

	public void MockRng(RunRngType rngType, uint seed)
	{
		_rngs[rngType] = new Rng(seed);
	}

	private Rng GetRng(RunRngType rngType)
	{
		return _rngs[rngType];
	}
}
