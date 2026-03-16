namespace MegaCrit.Sts2.Core.Saves;

public interface ICloudSaveStore : ISaveStore
{
	bool HasCloudFiles();

	void ForgetFile(string path);

	bool IsFilePersisted(string path);

	void BeginSaveBatch();

	void EndSaveBatch();
}
