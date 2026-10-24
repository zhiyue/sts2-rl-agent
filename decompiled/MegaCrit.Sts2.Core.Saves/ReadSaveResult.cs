namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Result of a save file read operation.
/// </summary>
/// <typeparam name="T">The type of save data</typeparam>
public class ReadSaveResult<T> where T : ISaveSchema
{
	/// <summary>
	/// The save data, if the operation succeeded or was recreated.
	/// </summary>
	public T? SaveData { get; }

	/// <summary>
	/// The status of the operation.
	/// </summary>
	public ReadSaveStatus Status { get; }

	/// <summary>
	/// Whether the operation succeeded and resulted in usable save data.
	/// This includes successful loads, migrations, repairs, and partial recoveries.
	/// </summary>
	public bool Success
	{
		get
		{
			if (Status != ReadSaveStatus.Success && Status != ReadSaveStatus.MigrationRequired && Status != ReadSaveStatus.JsonRepaired)
			{
				return Status == ReadSaveStatus.RecoveredWithDataLoss;
			}
			return true;
		}
	}

	/// <summary>
	/// Error message, if the operation failed.
	/// </summary>
	public string? ErrorMessage { get; }

	/// <summary>
	/// Creates a new successful result with save data.
	/// </summary>
	/// <param name="data">The save data</param>
	public ReadSaveResult(T data)
	{
		SaveData = data;
		Status = ReadSaveStatus.Success;
	}

	/// <summary>
	/// Creates a new result with a status.
	/// </summary>
	/// <param name="status">The status</param>
	/// <param name="errorMessage">Optional error message</param>
	public ReadSaveResult(ReadSaveStatus status, string? errorMessage = null)
	{
		Status = status;
		ErrorMessage = errorMessage;
	}

	/// <summary>
	/// Creates a new result with a status and save data.
	/// </summary>
	/// <param name="data">The save data</param>
	/// <param name="status">The status</param>
	/// <param name="errorMessage">Optional error message</param>
	public ReadSaveResult(T data, ReadSaveStatus status, string? errorMessage = null)
	{
		SaveData = data;
		Status = status;
		ErrorMessage = errorMessage;
	}
}
