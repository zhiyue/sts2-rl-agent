using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Random;

public class PlayerRngSet
{
	private readonly Dictionary<PlayerRngType, Rng> _rngs = new Dictionary<PlayerRngType, Rng>();

	public Rng Rewards => GetRng(PlayerRngType.Rewards);

	public Rng Shops => GetRng(PlayerRngType.Shops);

	public Rng Transformations => GetRng(PlayerRngType.Transformations);

	public uint Seed { get; }

	public PlayerRngSet(uint seed)
	{
		Seed = seed;
		PlayerRngType[] values = Enum.GetValues<PlayerRngType>();
		foreach (PlayerRngType playerRngType in values)
		{
			_rngs[playerRngType] = CreateRng(playerRngType);
		}
	}

	private Rng CreateRng(PlayerRngType rngType)
	{
		string name = StringHelper.SnakeCase(rngType.ToString());
		return new Rng(Seed, name);
	}

	public SerializablePlayerRngSet ToSerializable()
	{
		SerializablePlayerRngSet serializablePlayerRngSet = new SerializablePlayerRngSet
		{
			Seed = Seed
		};
		foreach (var (key, rng2) in _rngs)
		{
			serializablePlayerRngSet.Counters[key] = rng2.Counter;
		}
		return serializablePlayerRngSet;
	}

	public static PlayerRngSet FromSerializable(SerializablePlayerRngSet save)
	{
		PlayerRngSet playerRngSet = new PlayerRngSet(save.Seed);
		foreach (KeyValuePair<PlayerRngType, int> counter in save.Counters)
		{
			counter.Deconstruct(out var key, out var value);
			PlayerRngType playerRngType = key;
			int targetCount = value;
			Rng rng = playerRngSet.CreateRng(playerRngType);
			rng.FastForwardCounter(targetCount);
			playerRngSet._rngs[playerRngType] = rng;
		}
		return playerRngSet;
	}

	public void LoadFromSerializable(SerializablePlayerRngSet save)
	{
		if (Seed != save.Seed)
		{
			throw new NotImplementedException("RngSet seed should not change during the run!");
		}
		foreach (KeyValuePair<PlayerRngType, int> counter in save.Counters)
		{
			counter.Deconstruct(out var key, out var value);
			PlayerRngType playerRngType = key;
			int num = value;
			Rng rng = _rngs[playerRngType];
			if (num < rng.Counter)
			{
				rng = CreateRng(playerRngType);
				rng.FastForwardCounter(num);
				_rngs[playerRngType] = rng;
			}
			else
			{
				_rngs[playerRngType].FastForwardCounter(num);
			}
		}
	}

	private Rng GetRng(PlayerRngType rngType)
	{
		return _rngs[rngType];
	}
}
