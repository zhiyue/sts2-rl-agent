using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Helpers;

public static class TaskHelper
{
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
			if (!(ex is TaskCanceledException))
			{
				Log.Error(ex.ToString());
				SentryService.CaptureException(ex);
			}
			throw;
		}
	}

	public static async Task WhenAny(params Task[] tasks)
	{
		await (await Task.WhenAny(tasks));
	}
}
