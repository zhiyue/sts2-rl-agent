using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public class SteamLeaderboardStrategy : ILeaderboardStrategy
{
	private PacketWriter _writer = new PacketWriter();

	private int[] _cachedUserDetails = new int[64];

	private CSteamID[] _cachedUsers = new CSteamID[10];

	public PlatformType Platform => PlatformType.Steam;

	public async Task<ILeaderboardHandle> GetOrCreateLeaderboard(string name)
	{
		SteamAPICall_t call = SteamUserStats.FindOrCreateLeaderboard(name, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
		using SteamCallResult<LeaderboardFindResult_t> callResult = new SteamCallResult<LeaderboardFindResult_t>(call);
		LeaderboardFindResult_t leaderboardFindResult_t = await callResult.Task;
		if (leaderboardFindResult_t.m_bLeaderboardFound == 0)
		{
			throw new InvalidOperationException("Steam FindOrCreateLeaderboard returned 0 from leaderboard found!");
		}
		return new SteamLeaderboardHandle
		{
			leaderboard = leaderboardFindResult_t.m_hSteamLeaderboard
		};
	}

	public async Task<ILeaderboardHandle?> GetLeaderboard(string name)
	{
		SteamAPICall_t call = SteamUserStats.FindLeaderboard(name);
		using SteamCallResult<LeaderboardFindResult_t> callResult = new SteamCallResult<LeaderboardFindResult_t>(call);
		LeaderboardFindResult_t leaderboardFindResult_t = await callResult.Task;
		if (leaderboardFindResult_t.m_bLeaderboardFound == 0)
		{
			return null;
		}
		return new SteamLeaderboardHandle
		{
			leaderboard = leaderboardFindResult_t.m_hSteamLeaderboard
		};
	}

	public async Task UploadLocalScore(ILeaderboardHandle handleInterface, int score, IReadOnlyList<ulong> userIds)
	{
		int scoreDetailsCount;
		int[] pScoreDetails = PackScoreDetails(userIds, out scoreDetailsCount);
		SteamLeaderboardHandle steamLeaderboardHandle = (SteamLeaderboardHandle)handleInterface;
		SteamAPICall_t call = SteamUserStats.UploadLeaderboardScore(steamLeaderboardHandle.leaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, score, pScoreDetails, scoreDetailsCount);
		using SteamCallResult<LeaderboardScoreUploaded_t> callResult = new SteamCallResult<LeaderboardScoreUploaded_t>(call);
		if ((await callResult.Task).m_bSuccess == 0)
		{
			throw new IOException("Steam score upload failed!");
		}
	}

	public async Task<List<LeaderboardEntry>> QueryLeaderboard(ILeaderboardHandle handleInterface, LeaderboardQueryType type, int startIndex, int count)
	{
		SteamLeaderboardHandle steamLeaderboardHandle = (SteamLeaderboardHandle)handleInterface;
		int num = startIndex;
		if (type == LeaderboardQueryType.Global)
		{
			num++;
		}
		SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(steamLeaderboardHandle.leaderboard, LeaderboardDataRequestTypeFrom(type), num, num + count - 1);
		using SteamCallResult<LeaderboardScoresDownloaded_t> callResult = new SteamCallResult<LeaderboardScoresDownloaded_t>(call);
		LeaderboardScoresDownloaded_t leaderboardScoresDownloaded_t = await callResult.Task;
		List<LeaderboardEntry> list = new List<LeaderboardEntry>();
		int num2 = 0;
		int num3 = leaderboardScoresDownloaded_t.m_cEntryCount;
		if (type == LeaderboardQueryType.FriendsOnly)
		{
			num2 = startIndex;
			num3 = Math.Min(startIndex + count, leaderboardScoresDownloaded_t.m_cEntryCount);
		}
		for (int i = num2; i < num3; i++)
		{
			if (!SteamUserStats.GetDownloadedLeaderboardEntry(leaderboardScoresDownloaded_t.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, _cachedUserDetails, _cachedUserDetails.Length))
			{
				throw new InvalidOperationException($"Failed to download leaderboard entry at index {i}");
			}
			list.Add(LeaderboardEntryFromSteamType(pLeaderboardEntry));
		}
		return list;
	}

	public async Task<List<LeaderboardEntry>> QueryLeaderboardForUsers(ILeaderboardHandle handleInterface, IReadOnlyList<ulong> userIds)
	{
		SteamLeaderboardHandle steamLeaderboardHandle = (SteamLeaderboardHandle)handleInterface;
		for (int i = 0; i < userIds.Count; i++)
		{
			_cachedUsers[i] = new CSteamID(userIds[i]);
		}
		SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntriesForUsers(steamLeaderboardHandle.leaderboard, _cachedUsers, userIds.Count);
		using SteamCallResult<LeaderboardScoresDownloaded_t> callResult = new SteamCallResult<LeaderboardScoresDownloaded_t>(call);
		LeaderboardScoresDownloaded_t leaderboardScoresDownloaded_t = await callResult.Task;
		List<LeaderboardEntry> list = new List<LeaderboardEntry>();
		for (int j = 0; j < leaderboardScoresDownloaded_t.m_cEntryCount; j++)
		{
			if (!SteamUserStats.GetDownloadedLeaderboardEntry(leaderboardScoresDownloaded_t.m_hSteamLeaderboardEntries, j, out var pLeaderboardEntry, _cachedUserDetails, _cachedUserDetails.Length))
			{
				throw new InvalidOperationException($"Failed to download leaderboard entry at index {j}");
			}
			list.Add(LeaderboardEntryFromSteamType(pLeaderboardEntry));
		}
		return list;
	}

	public int GetLeaderboardEntryCount(ILeaderboardHandle handleInterface)
	{
		SteamLeaderboardHandle steamLeaderboardHandle = (SteamLeaderboardHandle)handleInterface;
		return SteamUserStats.GetLeaderboardEntryCount(steamLeaderboardHandle.leaderboard);
	}

	private LeaderboardEntry LeaderboardEntryFromSteamType(LeaderboardEntry_t entry)
	{
		return new LeaderboardEntry
		{
			id = entry.m_steamIDUser.m_SteamID,
			name = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser),
			rank = entry.m_nGlobalRank - 1,
			score = entry.m_nScore,
			userIds = UnpackScoreDetails(_cachedUserDetails, entry.m_cDetails)
		};
	}

	private ELeaderboardDataRequest LeaderboardDataRequestTypeFrom(LeaderboardQueryType type)
	{
		switch (type)
		{
		case LeaderboardQueryType.Global:
			return ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
		case LeaderboardQueryType.AroundUser:
			return ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
		case LeaderboardQueryType.FriendsOnly:
			return ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
		case LeaderboardQueryType.None:
			throw new InvalidOperationException("LeaderboardQueryType.None should never be passed to an API!");
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(type);
			ELeaderboardDataRequest result = default(ELeaderboardDataRequest);
			return result;
		}
		}
	}

	private int[] PackScoreDetails(IReadOnlyList<ulong> userIds, out int scoreDetailsCount)
	{
		_writer.Reset();
		foreach (ulong userId in userIds)
		{
			_writer.WriteULong(userId);
		}
		scoreDetailsCount = (int)Math.Ceiling((float)_writer.BitPosition / 32f);
		if (scoreDetailsCount > 64)
		{
			throw new InvalidOperationException($"Tried to write {userIds.Count} user IDs into {scoreDetailsCount} integers, exceeding the max limit ({64})!");
		}
		for (int i = 0; i < scoreDetailsCount; i++)
		{
			_cachedUserDetails[i] = BitConverter.ToInt32(_writer.Buffer, i * 4);
		}
		return _cachedUserDetails;
	}

	private List<ulong> UnpackScoreDetails(int[] scoreDetails, int scoreDetailsCount)
	{
		_writer.Reset();
		for (int i = 0; i < scoreDetailsCount; i++)
		{
			_writer.WriteInt(scoreDetails[i]);
		}
		PacketReader packetReader = new PacketReader();
		packetReader.Reset(_writer.Buffer);
		int num = (int)Math.Floor((float)_writer.BitPosition / 64f);
		List<ulong> list = new List<ulong>();
		for (int j = 0; j < num; j++)
		{
			list.Add(packetReader.ReadULong());
		}
		return list;
	}
}
