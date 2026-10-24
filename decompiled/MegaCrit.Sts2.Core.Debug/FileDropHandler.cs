using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Debug;

public static class FileDropHandler
{
	public static void OnFilesDropped(string[] files)
	{
		if ((OS.HasFeature("editor") || ModManager.IsRunningModded() || SaveManager.Instance.SettingsSave.FullConsole) && files.Length <= 1)
		{
			string text = files[0];
			if (!text.EndsWith(".run"))
			{
				Log.Error("We only support dropping .run files. You dropped " + text + ".");
			}
			else
			{
				TaskHelper.RunSafely(OnRunHistoryDropped(text));
			}
		}
	}

	private static async Task OnRunHistoryDropped(string file)
	{
		if (!RunManager.Instance.IsInProgress)
		{
			Log.Error("Can only load run history while run is in progress.");
			return;
		}
		RunState runState = RunManager.Instance.DebugOnlyGetState();
		if (runState.Players.Count > 1)
		{
			Log.Error("Only singleplayer supported for now.");
			return;
		}
		using FileAccess fileAccess = FileAccess.Open(file, FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			Log.Error($"Couldn't open file {file}: {FileAccess.GetOpenError()}");
			return;
		}
		ReadSaveResult<RunHistory> readSaveResult = JsonSerializationUtility.FromJson<RunHistory>(fileAccess.GetAsText());
		if (!readSaveResult.Success)
		{
			Log.Error($"Couldn't read {file}: {readSaveResult.ErrorMessage} ({readSaveResult.Status})");
			return;
		}
		RunHistory saveData = readSaveResult.SaveData;
		if (saveData.Players.Count > 1)
		{
			Log.Error("Only singleplayer supported for now.");
			return;
		}
		RunHistoryPlayer runHistoryPlayer = saveData.Players[0];
		Log.Info("Successfully loaded file " + file);
		SerializableRun savedRun = RunManager.Instance.ToSave(null);
		SerializablePlayer serializablePlayer = savedRun.Players[0];
		serializablePlayer.Deck.Clear();
		serializablePlayer.Deck.AddRange(runHistoryPlayer.Deck);
		serializablePlayer.Relics.Clear();
		serializablePlayer.Relics.AddRange(runHistoryPlayer.Relics);
		serializablePlayer.Potions.Clear();
		serializablePlayer.Potions.AddRange(runHistoryPlayer.Potions);
		RunManager.Instance.CleanUp();
		RunState loadedState = RunState.FromSerializable(savedRun);
		await RunManager.Instance.SetUpSavedSingleplayer(loadedState, savedRun);
		NGame.Instance.ReactionContainer.InitializeNetworking(new NetSingleplayerGameService());
		await NGame.Instance.LoadRun(loadedState, savedRun.PreFinishedRoom);
	}
}
