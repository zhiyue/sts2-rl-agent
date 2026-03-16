using System;
using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class MigrationRegistry
{
	public Dictionary<Type, List<IMigration>> Migrations { get; } = new Dictionary<Type, List<IMigration>>();

	public void RegisterAllMigrations(MigrationManager manager)
	{
		try
		{
			Assembly assembly = typeof(IMigration).Assembly;
			Type typeFromHandle = typeof(IMigration);
			for (int i = 0; i < IMigrationSubtypes.Count; i++)
			{
				Type type = IMigrationSubtypes.Get(i);
				try
				{
					object obj = Activator.CreateInstance(type);
					if (obj is IMigration migration)
					{
						manager.RegisterMigration(migration);
						Log.Debug($"Registered migration for {migration.SaveType.Name} from v{migration.FromVersion} to v{migration.ToVersion}");
					}
					else
					{
						Log.Error("Failed to instantiate migration " + type.Name + ": Created instance is not an IMigration");
					}
				}
				catch (Exception ex)
				{
					Log.Error("Failed to instantiate migration " + type.Name + ": " + ex.Message);
				}
			}
			Log.Info($"Registered {IMigrationSubtypes.Count} migrations");
		}
		catch (Exception ex2)
		{
			Log.Error("Error registering migrations: " + ex2.Message);
		}
	}
}
