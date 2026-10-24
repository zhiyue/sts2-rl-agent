using MegaCrit.Sts2.Core.Saves.Migrations;

namespace MegaCrit.Sts2.Core.Saves.Managers;

public class ProfileSaveManager
{
	public const int maxProfileCount = 3;

	public const string profileSaveFileName = "profile.save";

	private readonly ISaveStore _saveStore;

	private readonly MigrationManager _migrationManager;

	public static string ProfilePath => "profile.save";

	public ProfileSave Profile { get; private set; }

	public ProfileSaveManager(ISaveStore saveStore, MigrationManager migrationManager)
	{
		_saveStore = saveStore;
		_migrationManager = migrationManager;
	}

	public void SaveProfile()
	{
		Profile.SchemaVersion = _migrationManager.GetLatestVersion<ProfileSave>();
		string content = JsonSerializationUtility.ToJson(Profile);
		_saveStore.WriteFile(ProfilePath, content);
	}

	public ReadSaveResult<ProfileSave> LoadProfile()
	{
		ReadSaveResult<ProfileSave> readSaveResult = _migrationManager.LoadSave<ProfileSave>(ProfilePath);
		if (!readSaveResult.Success || readSaveResult.SaveData == null)
		{
			Profile = _migrationManager.CreateNewSave<ProfileSave>();
		}
		else
		{
			Profile = readSaveResult.SaveData;
		}
		return readSaveResult;
	}
}
