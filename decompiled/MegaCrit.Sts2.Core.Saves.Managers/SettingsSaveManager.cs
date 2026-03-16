using Godot;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Saves.Migrations;
using Steamworks;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class SettingsSaveManager
{
	public const string settingsSaveFileName = "settings.save";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	private string SettingsPath => "settings.save";

	public SettingsSave Settings { get; set; }

	public SettingsSaveManager(ISaveStore saveStore, MigrationManager migrationManager)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
	}

	public void SaveSettings()
	{
		Settings.SchemaVersion = _migrationManager.GetLatestVersion<SettingsSave>();
		string content = JsonSerializationUtility.ToJson(Settings);
		_saveStore.WriteFile(SettingsPath, content);
	}

	public ReadSaveResult<SettingsSave> LoadSettings()
	{
		ReadSaveResult<SettingsSave> readSaveResult = _migrationManager.LoadSave<SettingsSave>(SettingsPath);
		if (!readSaveResult.Success || readSaveResult.SaveData == null)
		{
			Settings = _migrationManager.CreateNewSave<SettingsSave>();
			ApplyPlatformDefaults(Settings);
			SaveSettings();
		}
		else
		{
			Settings = readSaveResult.SaveData;
		}
		return readSaveResult;
	}

	private void ApplyPlatformDefaults(SettingsSave settings)
	{
		if (SteamInitializer.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			settings.Fullscreen = true;
			settings.WindowPosition = new Vector2I(0, 0);
		}
	}
}
