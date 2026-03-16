using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class MigrationManager
{
	private readonly Dictionary<Type, int> _latestVersions = new Dictionary<Type, int>();

	private readonly Dictionary<Type, int> _minimumSupportedVersions = new Dictionary<Type, int>();

	private readonly MigrationRegistry _registry = new MigrationRegistry();

	private readonly ISaveStore _saveStore;

	public MigrationManager(ISaveStore saveStore)
	{
		_saveStore = saveStore;
		Initialize();
	}

	private void Initialize()
	{
		_registry.RegisterAllMigrations(this);
		DeriveAndSetLatestVersions();
		ValidateMigrationPaths();
		SetMinimumSupportedVersion<SerializableRun>(13);
		SetMinimumSupportedVersion<SerializableProgress>(21);
		SetMinimumSupportedVersion<SettingsSave>(4);
		SetMinimumSupportedVersion<RunHistory>(8);
		SetMinimumSupportedVersion<PrefsSave>(2);
		SetMinimumSupportedVersion<ProfileSave>(2);
	}

	private void DeriveAndSetLatestVersions()
	{
		foreach (Type key in _registry.Migrations.Keys)
		{
			if (_registry.Migrations[key].Count == 0)
			{
				_latestVersions[key] = 1;
				continue;
			}
			int value = _registry.Migrations[key].Select((IMigration m) => m.ToVersion).Max();
			_latestVersions[key] = value;
		}
		Log.Info("Current save versions: " + string.Join("; ", _latestVersions.Select<KeyValuePair<Type, int>, string>((KeyValuePair<Type, int> p) => $"{p.Key.Name} v{p.Value}")));
		EnsureVersionSet<SerializableRun>();
		EnsureVersionSet<SerializableProgress>();
		EnsureVersionSet<SettingsSave>();
		EnsureVersionSet<RunHistory>();
		EnsureVersionSet<PrefsSave>();
		EnsureVersionSet<ProfileSave>();
	}

	private void EnsureVersionSet<T>() where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		_latestVersions.TryAdd(typeFromHandle, 1);
	}

	private void ValidateMigrationPaths()
	{
		foreach (Type key in _registry.Migrations.Keys)
		{
			IsMigrationPathValid(key);
		}
	}

	public int GetLatestVersion<T>()
	{
		Type typeFromHandle = typeof(T);
		if (!_latestVersions.TryGetValue(typeFromHandle, out var value))
		{
			Log.Warn("No version found for " + typeFromHandle.Name + ", defaulting to 1");
			return 1;
		}
		return value;
	}

	private List<int> GetGapsInMigrationPath(Type saveType)
	{
		List<int> list = new List<int>();
		if (!_registry.Migrations.TryGetValue(saveType, out List<IMigration> value))
		{
			return list;
		}
		HashSet<int> hashSet = new HashSet<int>(value.Select((IMigration m) => m.FromVersion));
		HashSet<int> hashSet2 = new HashSet<int>(value.Select((IMigration m) => m.ToVersion));
		int value2;
		bool flag = _latestVersions.TryGetValue(saveType, out value2);
		foreach (int item in hashSet2)
		{
			if ((!flag || item != value2) && !hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private List<int> GetDuplicateMigrationSources(Type saveType)
	{
		if (!_registry.Migrations.TryGetValue(saveType, out List<IMigration> value))
		{
			return new List<int>();
		}
		return (from m in value
			group m by m.FromVersion into g
			where g.Count() > 1
			select g.Key).ToList();
	}

	private void IsMigrationPathValid(Type saveType)
	{
		List<int> duplicateMigrationSources = GetDuplicateMigrationSources(saveType);
		if (duplicateMigrationSources.Count > 0)
		{
			int value = duplicateMigrationSources[0];
			string message = $"Multiple migrations from version {value} for {saveType.Name}";
			throw new DuplicateMigrationException(message);
		}
		List<int> gapsInMigrationPath = GetGapsInMigrationPath(saveType);
		if (gapsInMigrationPath.Count > 0)
		{
			int value2 = gapsInMigrationPath[0];
			string message2 = $"Gap in migration path for {saveType.Name}: version {value2} is a target but not a source in any migration";
			throw new MigrationPathGapException(message2);
		}
	}

	public void RegisterMigration(IMigration migration)
	{
		Type saveType = migration.SaveType;
		if (!_registry.Migrations.TryGetValue(saveType, out List<IMigration> value))
		{
			value = new List<IMigration>();
			_registry.Migrations[saveType] = value;
		}
		value.Add(migration);
	}

	private void SetMinimumSupportedVersion<T>(int version) where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		_minimumSupportedVersions[typeFromHandle] = version;
	}

	private int GetCurrentVersion<T>() where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		if (!_latestVersions.TryGetValue(typeFromHandle, out var value))
		{
			throw new MigrationException("No migrations found for " + typeFromHandle.Name);
		}
		return value;
	}

	private int GetMinimumSupportedVersion<T>() where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		if (!_minimumSupportedVersions.TryGetValue(typeFromHandle, out var value))
		{
			throw new InvalidOperationException("No minimum supported version found for " + typeFromHandle.Name + ". Each save schema type should have a minimum supported version set and a base migration implemented.");
		}
		return value;
	}

	private IMigration? GetMigration<T>(int fromVersion, int toVersion) where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		if (!_registry.Migrations.TryGetValue(typeFromHandle, out List<IMigration> value))
		{
			return null;
		}
		return value.FirstOrDefault((IMigration m) => m.FromVersion == fromVersion && m.ToVersion == toVersion);
	}

	private int? GetNextVersion<T>(int currentVersion) where T : ISaveSchema
	{
		Type typeFromHandle = typeof(T);
		if (!_registry.Migrations.TryGetValue(typeFromHandle, out List<IMigration> value))
		{
			return null;
		}
		int versionToFind = currentVersion;
		return value.FirstOrDefault((IMigration m) => m.FromVersion == versionToFind)?.ToVersion;
	}

	public IEnumerable<Type> GetRegisteredSaveTypes()
	{
		return _registry.Migrations.Keys;
	}

	public IEnumerable<IMigration> GetMigrationsForType(Type saveType)
	{
		if (!_registry.Migrations.TryGetValue(saveType, out List<IMigration> value))
		{
			return Array.Empty<IMigration>();
		}
		return value;
	}

	private static int ExtractSchemaVersion(MigratingData json)
	{
		if (json.Has("schema_version"))
		{
			return json.GetInt("schema_version");
		}
		throw new MissingSchemaVersionException($"Schema version not found in JSON: {json}");
	}

	public T CreateNewSave<T>() where T : ISaveSchema, new()
	{
		return new T
		{
			SchemaVersion = GetLatestVersion<T>()
		};
	}

	private void PreserveCorruptFile(string savePath, ReadSaveStatus status)
	{
		try
		{
			if (savePath.EndsWith(".corrupt"))
			{
				Log.Warn("File '" + savePath + "' is already marked as corrupt, skipping rename");
				return;
			}
			string text = CorruptFileHandler.GenerateCorruptFilePath(savePath, status);
			_saveStore.RenameFile(savePath, text);
			Log.Error($"Corrupt save detected ({status}): Renamed '{savePath}' to '{text}'");
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to preserve corrupt save file '" + savePath + "': " + ex.Message);
		}
	}

	private static bool ShouldPreserveCorrupt(ReadSaveStatus status)
	{
		if (status != ReadSaveStatus.FileNotFound && status != ReadSaveStatus.Success)
		{
			return status != ReadSaveStatus.MigrationRequired;
		}
		return false;
	}

	public ReadSaveResult<T> LoadSave<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string filePath) where T : ISaveSchema, new()
	{
		ReadSaveResult<T> readSaveResult = LoadSaveFromPath<T>(filePath);
		if (readSaveResult.Success)
		{
			return readSaveResult;
		}
		string text = filePath + ".backup";
		if (_saveStore.FileExists(text))
		{
			Log.Warn($"Primary save failed ({readSaveResult.Status}), attempting .backup fallback: {text}");
			ReadSaveResult<T> readSaveResult2 = LoadSaveFromPath<T>(text);
			if (readSaveResult2.Success)
			{
				return readSaveResult2;
			}
		}
		return readSaveResult;
	}

	private ReadSaveResult<T> LoadSaveFromPath<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string filePath) where T : ISaveSchema, new()
	{
		if (!_saveStore.FileExists(filePath))
		{
			return new ReadSaveResult<T>(ReadSaveStatus.FileNotFound);
		}
		try
		{
			string text = _saveStore.ReadFile(filePath);
			if (string.IsNullOrWhiteSpace(text))
			{
				Log.Warn("Empty save file found at " + filePath);
				PreserveCorruptFile(filePath, ReadSaveStatus.FileEmpty);
				return new ReadSaveResult<T>(ReadSaveStatus.FileEmpty);
			}
			return LoadWithAggressiveRecovery<T>(filePath, text);
		}
		catch (Exception ex)
		{
			Log.Error("File access error loading " + filePath + ": " + ex.Message);
			PreserveCorruptFile(filePath, ReadSaveStatus.FileAccessError);
			return new ReadSaveResult<T>(ReadSaveStatus.FileAccessError, ex.Message);
		}
	}

	private ReadSaveResult<T> LoadWithAggressiveRecovery<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string filePath, string content) where T : ISaveSchema, new()
	{
		try
		{
			using JsonDocument document = JsonDocument.Parse(content);
			MigratingData migratingData = new MigratingData(document);
			bool flag = false;
			int num;
			try
			{
				num = ExtractSchemaVersion(migratingData);
			}
			catch (MissingSchemaVersionException)
			{
				Log.Warn("Missing schema version in " + filePath + ", attempting to infer...");
				flag = true;
				num = InferSchemaVersionFromStructure<T>(migratingData).GetValueOrDefault();
				migratingData.Set("schema_version", num);
			}
			int currentVersion = GetCurrentVersion<T>();
			int minimumSupportedVersion = GetMinimumSupportedVersion<T>();
			if (num > currentVersion)
			{
				Log.Warn($"Save version {num} is newer than current {currentVersion}, attempting recovery...");
				T val = RecoverPartialDataFromCorruptSave<T>(migratingData);
				if (val != null)
				{
					Log.Info($"Successfully recovered data from future save version {num}");
					return new ReadSaveResult<T>(val, ReadSaveStatus.RecoveredWithDataLoss, $"Data recovered from future version {num} but newer fields were discarded");
				}
				string text = $"Save file version {num} is newer than current version {currentVersion}";
				Log.Error(text + ": " + filePath);
				PreserveCorruptFile(filePath, ReadSaveStatus.FutureVersion);
				return new ReadSaveResult<T>(ReadSaveStatus.FutureVersion, text);
			}
			if (num < minimumSupportedVersion)
			{
				Log.Warn($"Save version {num} is below minimum {minimumSupportedVersion}, attempting data scavenging...");
				T val2 = RecoverPartialDataFromCorruptSave<T>(migratingData);
				if (val2 != null)
				{
					Log.Info($"Successfully scavenged data from old save version {num} (recovery data not persisted)");
					return new ReadSaveResult<T>(val2, ReadSaveStatus.RecoveredWithDataLoss, $"Data recovered from version {num} but some information may be lost");
				}
				string text2 = $"Save file version {num} is too old and couldn't be scavenged";
				Log.Error(text2 + ": " + filePath);
				PreserveCorruptFile(filePath, ReadSaveStatus.VersionTooOld);
				return new ReadSaveResult<T>(ReadSaveStatus.VersionTooOld, text2);
			}
			if (num < currentVersion)
			{
				try
				{
					MigratingData migratingData2 = MigrateDataSequentially<T>(migratingData);
					T data = migratingData2.ToObject<T>();
					Log.Info($"Successfully migrated {typeof(T).Name} from v{num} to v{data.SchemaVersion} (migration not persisted)");
					return new ReadSaveResult<T>(data, ReadSaveStatus.MigrationRequired, $"Save was migrated from version {num} to {data.SchemaVersion}");
				}
				catch (Exception ex2)
				{
					Log.Error($"Migration failed for {filePath} with exception: {ex2}");
					T val3 = RecoverPartialDataFromCorruptSave<T>(migratingData);
					if (val3 != null)
					{
						Log.Info("Migration failed but data scavenging succeeded");
						return new ReadSaveResult<T>(val3, ReadSaveStatus.RecoveredWithDataLoss, $"Migration failed, recovered partial data from version {num}");
					}
					PreserveCorruptFile(filePath, ReadSaveStatus.MigrationFailed);
					return new ReadSaveResult<T>(ReadSaveStatus.MigrationFailed, ex2.Message);
				}
			}
			ReadSaveResult<T> readSaveResult = JsonSerializationUtility.FromJson<T>(content);
			if (!readSaveResult.Success || readSaveResult.SaveData == null)
			{
				Log.Error("Failed to deserialize " + filePath + ": " + readSaveResult.ErrorMessage);
				T val4 = RecoverPartialDataFromCorruptSave<T>(migratingData);
				if (val4 != null)
				{
					Log.Info("Deserialization failed but data scavenging succeeded");
					return new ReadSaveResult<T>(val4, ReadSaveStatus.RecoveredWithDataLoss, "Save file was corrupt but partial data was recovered");
				}
				if (flag)
				{
					PreserveCorruptFile(filePath, ReadSaveStatus.MissingSchemaVersion);
					return new ReadSaveResult<T>(ReadSaveStatus.MissingSchemaVersion, "Save file is missing schema version and cannot be deserialized");
				}
				if (ShouldPreserveCorrupt(readSaveResult.Status))
				{
					PreserveCorruptFile(filePath, readSaveResult.Status);
				}
				return new ReadSaveResult<T>(readSaveResult.Status, readSaveResult.ErrorMessage);
			}
			return readSaveResult;
		}
		catch (JsonException ex3)
		{
			string value = ex3.Path ?? "unknown";
			Log.Error($"JSON parse error in {filePath} at path={value}, line={ex3.LineNumber}: {ex3.Message}");
			string text3 = RepairCommonJsonErrors(content);
			if (text3 != null)
			{
				Log.Info("JSON repair succeeded, retrying load...");
				ReadSaveResult<T> readSaveResult2 = LoadWithAggressiveRecovery<T>(filePath, text3);
				if (readSaveResult2.Success)
				{
					_saveStore.WriteFile(filePath + ".pre-repair", content);
					return new ReadSaveResult<T>(readSaveResult2.SaveData, ReadSaveStatus.JsonRepaired, "Save file had JSON errors that were automatically repaired");
				}
				return readSaveResult2;
			}
			PreserveCorruptFile(filePath, ReadSaveStatus.JsonParseError);
			return new ReadSaveResult<T>(ReadSaveStatus.JsonParseError, $"JSON error at {value} (line {ex3.LineNumber}): {ex3.Message}");
		}
		catch (Exception ex4)
		{
			Log.Error("Unexpected error loading " + filePath + ": " + ex4.Message);
			PreserveCorruptFile(filePath, ReadSaveStatus.Unrecoverable);
			return new ReadSaveResult<T>(ReadSaveStatus.Unrecoverable, ex4.Message);
		}
	}

	private int? InferSchemaVersionFromStructure<T>(MigratingData data) where T : ISaveSchema
	{
		return null;
	}

	private T? RecoverPartialDataFromCorruptSave<T>(MigratingData data) where T : ISaveSchema, new()
	{
		if (typeof(T) == typeof(SerializableRun))
		{
			return default(T);
		}
		int currentVersion = GetCurrentVersion<T>();
		try
		{
			data.Set("schema_version", currentVersion);
			T result = data.ToObject<T>();
			result.SchemaVersion = currentVersion;
			Log.Info("Data scavenging succeeded for " + typeof(T).Name);
			return result;
		}
		catch (Exception ex)
		{
			Log.Warn("Data scavenging failed for " + typeof(T).Name + ": " + ex.Message);
			return default(T);
		}
	}

	private string? RepairCommonJsonErrors(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}
		try
		{
			string input = json;
			input = Regex.Replace(input, ",\\s*([}\\]])", "$1");
			if (!input.EndsWith("}") && !input.EndsWith("]"))
			{
				int num = input.Count((char c) => c == '{') - input.Count((char c) => c == '}');
				int num2 = input.Count((char c) => c == '[') - input.Count((char c) => c == ']');
				if (num == 1 && num2 == 0)
				{
					input += "}";
				}
				else if (num2 == 1 && num == 0)
				{
					input += "]";
				}
			}
			using (JsonDocument.Parse(input))
			{
				return input;
			}
		}
		catch
		{
			return null;
		}
	}

	private MigratingData MigrateDataSequentially<T>(MigratingData jsonObj) where T : ISaveSchema
	{
		int num = ExtractSchemaVersion(jsonObj);
		int currentVersion = GetCurrentVersion<T>();
		while (num < currentVersion)
		{
			int? nextVersion = GetNextVersion<T>(num);
			if (!nextVersion.HasValue)
			{
				throw new MigrationException($"Missing migration path for {typeof(T).Name} from v{num} to latest version");
			}
			int value = nextVersion.Value;
			IMigration migration = GetMigration<T>(num, value);
			if (migration == null)
			{
				throw new MigrationException($"Missing migration implementation for {typeof(T).Name} from v{num} to v{value}");
			}
			Log.Info($"Migrating {typeof(T).Name} from v{num} to v{value}");
			try
			{
				jsonObj = migration.Migrate(jsonObj);
				num = value;
			}
			catch (Exception ex) when (!(ex is MigrationException))
			{
				throw new MigrationException($"Error migrating {typeof(T).Name} from v{num} to v{value}: {ex.Message}", ex);
			}
		}
		return jsonObj;
	}
}
