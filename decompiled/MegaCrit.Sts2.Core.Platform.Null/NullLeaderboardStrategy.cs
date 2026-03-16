using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Platform.Null;

public class NullLeaderboardStrategy : ILeaderboardStrategy
{
	private readonly GodotFileIo _fileIo = new GodotFileIo(UserDataPathProvider.GetAccountScopedBasePath(null));

	private List<NullLeaderboard> _leaderboards;

	public PlatformType Platform => PlatformType.None;

	public NullLeaderboardStrategy()
	{
		_leaderboards = Read();
	}

	public Task<ILeaderboardHandle?> GetLeaderboard(string name)
	{
		foreach (NullLeaderboard leaderboard in _leaderboards)
		{
			if (leaderboard.name == name)
			{
				return Task.FromResult((ILeaderboardHandle)new NullLeaderboardHandle
				{
					leaderboard = leaderboard
				});
			}
		}
		return Task.FromResult<ILeaderboardHandle>(null);
	}

	public async Task<ILeaderboardHandle> GetOrCreateLeaderboard(string name)
	{
		await CheckRefreshLeaderboard(null, PlatformUtil.GetLocalPlayerId(PlatformType.None));
		ILeaderboardHandle leaderboardHandle = await GetLeaderboard(name);
		if (leaderboardHandle != null)
		{
			return leaderboardHandle;
		}
		NullLeaderboard nullLeaderboard = new NullLeaderboard
		{
			name = name,
			entries = new List<NullLeaderboardFileEntry>()
		};
		_leaderboards.Add(nullLeaderboard);
		Write(_leaderboards);
		return new NullLeaderboardHandle
		{
			leaderboard = nullLeaderboard
		};
	}

	public async Task UploadLocalScore(ILeaderboardHandle handleInterface, int score, IReadOnlyList<ulong> userIds)
	{
		NullLeaderboardHandle handle = (NullLeaderboardHandle)handleInterface;
		ulong id = PlatformUtil.GetLocalPlayerId(PlatformType.None);
		await CheckRefreshLeaderboard(handle, id);
		NullLeaderboardFileEntry item = new NullLeaderboardFileEntry
		{
			name = PlatformUtil.GetPlayerName(PlatformType.None, id),
			id = id,
			score = score,
			userIds = userIds.ToList()
		};
		handle.leaderboard.entries.Add(item);
		Write(_leaderboards);
	}

	public async Task<List<LeaderboardEntry>> QueryLeaderboard(ILeaderboardHandle handleInterface, LeaderboardQueryType type, int startIndex, int count)
	{
		NullLeaderboardHandle handle = (NullLeaderboardHandle)handleInterface;
		ulong id = PlatformUtil.GetLocalPlayerId(PlatformType.None);
		await CheckRefreshLeaderboard(handle, id);
		switch (type)
		{
		case LeaderboardQueryType.Global:
		{
			NullLeaderboardStrategy nullLeaderboardStrategy2 = this;
			List<NullLeaderboardFileEntry> entries2 = handle.leaderboard.entries;
			int num2 = startIndex;
			return nullLeaderboardStrategy2.ToLeaderboardEntries(startIndex, entries2.Slice(num2, Math.Min(startIndex + count, handle.leaderboard.entries.Count) - num2));
		}
		case LeaderboardQueryType.AroundUser:
		{
			int num = handle.leaderboard.entries.FindIndex((NullLeaderboardFileEntry e) => e.id == id);
			if (num < 0)
			{
				return new List<LeaderboardEntry>();
			}
			NullLeaderboardStrategy nullLeaderboardStrategy = this;
			List<NullLeaderboardFileEntry> entries = handle.leaderboard.entries;
			int num2 = num + startIndex;
			return nullLeaderboardStrategy.ToLeaderboardEntries(startIndex, entries.Slice(num2, num + startIndex + count - num2));
		}
		case LeaderboardQueryType.FriendsOnly:
			throw new NotImplementedException();
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public async Task<List<LeaderboardEntry>> QueryLeaderboardForUsers(ILeaderboardHandle handleInterface, IReadOnlyList<ulong> userIds)
	{
		NullLeaderboardHandle handle = (NullLeaderboardHandle)handleInterface;
		ulong localPlayerId = PlatformUtil.GetLocalPlayerId(PlatformType.None);
		await CheckRefreshLeaderboard(handle, localPlayerId);
		List<LeaderboardEntry> list = new List<LeaderboardEntry>();
		for (int i = 0; i < handle.leaderboard.entries.Count; i++)
		{
			NullLeaderboardFileEntry nullLeaderboardFileEntry = handle.leaderboard.entries[i];
			if (userIds.Contains(nullLeaderboardFileEntry.id))
			{
				list.Add(ToLeaderboardEntry(i, nullLeaderboardFileEntry));
			}
		}
		return list;
	}

	public int GetLeaderboardEntryCount(ILeaderboardHandle handleInterface)
	{
		return ((NullLeaderboardHandle)handleInterface).leaderboard.entries.Count;
	}

	public void DebugAddEntry(ILeaderboardHandle handleInterface, LeaderboardEntry entry)
	{
		NullLeaderboardHandle nullLeaderboardHandle = (NullLeaderboardHandle)handleInterface;
		NullLeaderboardFileEntry item = new NullLeaderboardFileEntry
		{
			name = entry.name,
			id = entry.id,
			score = entry.score
		};
		nullLeaderboardHandle.leaderboard.entries.Add(item);
		nullLeaderboardHandle.leaderboard.entries.Sort((NullLeaderboardFileEntry x, NullLeaderboardFileEntry y) => y.score.CompareTo(x.score));
		Write(_leaderboards);
	}

	private List<LeaderboardEntry> ToLeaderboardEntries(int startIndex, List<NullLeaderboardFileEntry> nullEntries)
	{
		List<LeaderboardEntry> list = new List<LeaderboardEntry>();
		for (int i = 0; i < nullEntries.Count; i++)
		{
			list.Add(ToLeaderboardEntry(startIndex + i, nullEntries[i]));
		}
		return list;
	}

	private LeaderboardEntry ToLeaderboardEntry(int rank, NullLeaderboardFileEntry nullEntry)
	{
		return new LeaderboardEntry
		{
			name = nullEntry.name,
			score = nullEntry.score,
			rank = rank,
			id = 1uL,
			userIds = nullEntry.userIds
		};
	}

	private List<NullLeaderboard> Read()
	{
		if (TestMode.IsOn)
		{
			return new List<NullLeaderboard>();
		}
		string text = _fileIo.ReadFile("leaderboards.save");
		if (text == null)
		{
			return new List<NullLeaderboard>();
		}
		ReadSaveResult<NullLeaderboardFile> readSaveResult = JsonSerializationUtility.FromJson<NullLeaderboardFile>(text);
		if (readSaveResult.Status != ReadSaveStatus.Success)
		{
			return new List<NullLeaderboard>();
		}
		return readSaveResult.SaveData.leaderboards;
	}

	private void Write(List<NullLeaderboard> leaderboards)
	{
		if (!TestMode.IsOn)
		{
			string content = JsonSerializer.Serialize(new NullLeaderboardFile
			{
				leaderboards = leaderboards
			}, JsonSerializationUtility.GetTypeInfo<NullLeaderboardFile>());
			_fileIo.WriteFile("leaderboards.save", content);
		}
	}

	private async Task CheckRefreshLeaderboard(NullLeaderboardHandle? handle, ulong id)
	{
		if (!CommandLineHelper.HasArg("fastmp"))
		{
			return;
		}
		await Task.Delay((int)((float)id * 0.5f));
		_leaderboards = Read();
		if (handle != null)
		{
			handle.leaderboard = _leaderboards.First((NullLeaderboard l) => l.name == handle.leaderboard.name);
		}
	}
}
