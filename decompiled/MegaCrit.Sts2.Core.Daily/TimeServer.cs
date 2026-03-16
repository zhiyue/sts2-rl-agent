using System;
using System.Net.Http;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Daily;

public static class TimeServer
{
	private const string _timeServerUrl = "https://time.megacrit.com";

	private const int _maxRetry = 2;

	private const int _waitTimeCap = 4;

	private static readonly Logger _logger = new Logger("TimeServer", LogType.Network);

	public static Task<TimeServerResult?>? RequestTimeTask { get; private set; }

	private static async Task<TimeServerResult?> RequestTime(string url)
	{
		using HttpClient client = new HttpClient();
		Exception exception = null;
		int retries = 0;
		while (retries < 2)
		{
			long? num;
			try
			{
				num = await RequestTimeInternal(client, url);
			}
			catch (Exception ex)
			{
				Log.Warn($"Caught exception while requesting server time of type {ex.GetType()}");
				exception = ex;
				num = null;
			}
			if (num.HasValue)
			{
				return new TimeServerResult
				{
					serverTime = DateTimeOffset.FromUnixTimeSeconds(num.Value),
					localReceivedTime = DateTimeOffset.UtcNow
				};
			}
			retries++;
			Log.Info($"Retries: {retries}/{2}");
			if (retries < 2)
			{
				long seconds = Math.Min((long)Math.Pow(2.0, retries), 4L);
				await Task.Delay(TimeSpan.FromSeconds(seconds));
			}
		}
		_logger.Warn("Gave up trying to retrieve server time. Will use local time instead");
		if (exception != null)
		{
			throw exception;
		}
		return null;
	}

	private static async Task<long?> RequestTimeInternal(HttpClient client, string url)
	{
		HttpResponseMessage response = await client.GetAsync(url);
		string text = await response.Content.ReadAsStringAsync();
		if (response.IsSuccessStatusCode)
		{
			_logger.Info("Successfully queried time server. Response: " + text);
			if (long.TryParse(text, out var result))
			{
				return result;
			}
		}
		else
		{
			_logger.Info($"Failed to retrieve server time. Status: {response.StatusCode} Response: {text}");
		}
		return null;
	}

	public static Task<TimeServerResult?> FetchDailyTime()
	{
		RequestTimeTask = RequestTime("https://time.megacrit.com");
		return RequestTimeTask;
	}
}
