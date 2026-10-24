using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Helpers;

public static class TaskHelper
{
	/// <summary>
	/// Runs a task without awaiting it.
	/// Prefer using this over calling the task-returning method and then discarding the task, as that causes exceptions
	/// not to be logged.
	/// </summary>
	public static Task RunSafely(Task task)
	{
		return LogTaskExceptions(task);
	}

	private static async Task LogTaskExceptions(Task task)
	{
		try
		{
			await task;
		}
		catch (Exception ex)
		{
			if (!(ex is OperationCanceledException))
			{
				Log.Error(ex.ToString());
				SentryService.CaptureException(ex);
			}
			throw;
		}
	}

	/// <summary>
	/// Runs all the tasks at once and returns a task whose result is equivalent to the first task completed.
	/// Prefer using this over Task.WhenAny, as that returns a Task that is always successful.
	/// </summary>
	/// <param name="tasks"></param>
	/// <returns></returns>
	public static async Task WhenAny(params Task[] tasks)
	{
		await (await Task.WhenAny(tasks));
	}
}
