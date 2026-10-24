namespace MegaCrit.Sts2.Core.Saves.Migrations.ProfileSaves;

/// <summary>
/// No-op migration for release save wipe. All pre-release saves are invalidated
/// via minimum supported version bump.
/// </summary>
[Migration(typeof(ProfileSave), 1, 2)]
public class ProfileSaveV1ToV2 : MigrationBase<ProfileSave>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
	}
}
