using System;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Multiplayer.Replay;

public class CombatReplayWriter : IDisposable
{
	private CombatReplay? _replay;

	private readonly PacketWriter _writer = new PacketWriter();

	private readonly ActionQueueSet _actionQueueSet;

	private readonly ActionQueueSynchronizer _actionQueueSynchronizer;

	private readonly PlayerChoiceSynchronizer _playerChoiceSynchronizer;

	private readonly ChecksumTracker _checksumTracker;

	public bool IsEnabled { get; set; } = true;

	public bool IsRecordingReplay => _replay != null;

	public CombatReplayWriter(PlayerChoiceSynchronizer playerChoiceSynchronizer, ActionQueueSet actionQueueSet, ActionQueueSynchronizer actionQueueSynchronizer, ChecksumTracker checksumTracker)
	{
		_actionQueueSet = actionQueueSet;
		_actionQueueSynchronizer = actionQueueSynchronizer;
		_playerChoiceSynchronizer = playerChoiceSynchronizer;
		_checksumTracker = checksumTracker;
		actionQueueSet.ActionEnqueued += RecordGameAction;
		actionQueueSet.ActionResumed += RecordActionResume;
		playerChoiceSynchronizer.PlayerChoiceReceived += RecordPlayerChoice;
		checksumTracker.ChecksumGenerated += RecordChecksum;
	}

	public void Dispose()
	{
		_actionQueueSet.ActionEnqueued -= RecordGameAction;
		_actionQueueSet.ActionResumed -= RecordActionResume;
		_playerChoiceSynchronizer.PlayerChoiceReceived -= RecordPlayerChoice;
		_checksumTracker.ChecksumGenerated -= RecordChecksum;
	}

	public void RecordInitialState(SerializableRun serializableRun)
	{
		if (IsEnabled)
		{
			_replay = new CombatReplay
			{
				version = (ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "UNRELEASED"),
				gitCommit = (ReleaseInfoManager.Instance.ReleaseInfo?.Commit ?? GitHelper.ShortCommitId ?? "UNKNOWN"),
				modelIdHash = ModelIdSerializationCache.Hash,
				choiceIds = _playerChoiceSynchronizer.ChoiceIds.ToList(),
				nextActionId = _actionQueueSet.NextActionId,
				nextChecksumId = _checksumTracker.NextId,
				nextHookId = _actionQueueSynchronizer.NextHookId,
				serializableRun = serializableRun
			};
			_replay.events.Clear();
		}
	}

	private void RecordGameAction(GameAction gameAction)
	{
		if (!IsEnabled || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (_replay == null)
		{
			throw new InvalidOperationException("RecordInitialState must be called first");
		}
		if (gameAction is GenericHookGameAction genericHookGameAction)
		{
			_replay.events.Add(new CombatReplayEvent
			{
				playerId = gameAction.OwnerId,
				eventType = CombatReplayEventType.HookAction,
				hookId = genericHookGameAction.HookId,
				gameActionType = genericHookGameAction.ActionType
			});
			return;
		}
		if (!gameAction.RecordableToReplay)
		{
			throw new InvalidOperationException($"Found unrecordable game action: {gameAction}");
		}
		_replay.events.Add(new CombatReplayEvent
		{
			playerId = gameAction.OwnerId,
			eventType = CombatReplayEventType.GameAction,
			action = gameAction.ToNetAction()
		});
	}

	private void RecordActionResume(uint actionId)
	{
		if (IsEnabled && CombatManager.Instance.IsInProgress)
		{
			if (_replay == null)
			{
				throw new InvalidOperationException("RecordInitialState must be called first");
			}
			_replay.events.Add(new CombatReplayEvent
			{
				eventType = CombatReplayEventType.ResumeAction,
				actionId = actionId
			});
		}
	}

	private void RecordPlayerChoice(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		if (IsEnabled && CombatManager.Instance.IsInProgress)
		{
			if (_replay == null)
			{
				throw new InvalidOperationException("RecordInitialState must be called first");
			}
			_replay.events.Add(new CombatReplayEvent
			{
				eventType = CombatReplayEventType.PlayerChoice,
				playerId = player.NetId,
				choiceId = choiceId,
				playerChoiceResult = result
			});
		}
	}

	private void RecordChecksum(NetChecksumData checksum, string context, NetFullCombatState fullCombatState)
	{
		if (IsEnabled && CombatManager.Instance.IsInProgress)
		{
			if (_replay == null)
			{
				throw new InvalidOperationException("RecordInitialState must be called first");
			}
			_replay.checksumData.Add(new ReplayChecksumData
			{
				checksumData = checksum,
				context = context,
				fullState = fullCombatState
			});
		}
	}

	public void WriteReplay(string filePath, bool stopRecording)
	{
		if (!IsEnabled)
		{
			return;
		}
		if (_replay == null)
		{
			throw new InvalidOperationException("RecordInitialState must be called first");
		}
		_writer.Reset();
		_writer.Write(_replay.Anonymized());
		DirAccess.MakeDirRecursiveAbsolute(filePath.Substring(0, filePath.LastIndexOf('/')));
		using FileAccessStream fileAccessStream = new FileAccessStream(filePath, FileAccess.ModeFlags.Write);
		fileAccessStream.Write(_writer.Buffer.AsSpan().Slice(0, _writer.BytePosition));
		if (stopRecording)
		{
			StopRecording();
		}
	}

	public void StopRecording()
	{
		_replay = null;
	}
}
