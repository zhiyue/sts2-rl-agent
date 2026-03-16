using System;
using Godot;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class SentryConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sentry";

	public override string Args => "<test|message|exception|crash|status> [text]";

	public override string Description => "Test Sentry error reporting. 'test' sends a test message and exception, 'message <text>' sends a custom message, 'exception' throws a test exception, 'crash confirm' triggers a native crash (terminates game!), 'status' shows Sentry status.";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "Usage: sentry <test|message|exception|crash|status> [text]");
		}
		string text = args[0].ToLowerInvariant();
		return text switch
		{
			"status" => GetStatus(), 
			"test" => RunTest(), 
			"message" => SendMessage(args), 
			"exception" => ThrowException(), 
			"crash" => TriggerNativeCrash(args), 
			_ => new CmdResult(success: false, "Unknown subcommand: " + text + ". Use test, message, exception, crash, or status."), 
		};
	}

	private static CmdResult GetStatus()
	{
		bool isEnabled = SentryService.IsEnabled;
		bool value = ReleaseInfoManager.Instance.ReleaseInfo != null;
		bool uploadData = SaveManager.Instance.PrefsSave.UploadData;
		string value2 = ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? "N/A";
		string value3 = ReleaseInfoManager.Instance.ReleaseInfo?.Branch ?? "N/A";
		string msg = $"Sentry Status:\n  .NET SDK Enabled: {isEnabled}\n  Has release_info.json: {value}\n  User consent (UploadData): {uploadData}\n  Version: {value2}\n  Branch: {value3}";
		return new CmdResult(success: true, msg);
	}

	private static CmdResult RunTest()
	{
		bool isEnabled = SentryService.IsEnabled;
		if (!isEnabled)
		{
			ForceInitializeForTesting();
		}
		if (!SentryService.IsEnabled)
		{
			return new CmdResult(success: false, "Failed to initialize Sentry for testing. Check logs for details.");
		}
		SentryService.CaptureMessage("Test message from STS2 dev console");
		try
		{
			throw new InvalidOperationException("Test exception from STS2 dev console");
		}
		catch (Exception ex)
		{
			SentryService.CaptureException(ex);
		}
		string text = "Sent test message and exception to Sentry. Check your Sentry dashboard.";
		if (!isEnabled)
		{
			text += "\n(Sentry was temporarily enabled for this test)";
		}
		return new CmdResult(success: true, text);
	}

	private static CmdResult SendMessage(string[] args)
	{
		if (args.Length < 2)
		{
			return new CmdResult(success: false, "Usage: sentry message <text>");
		}
		string text = string.Join(" ", args, 1, args.Length - 1);
		if (!SentryService.IsEnabled)
		{
			ForceInitializeForTesting();
		}
		if (!SentryService.IsEnabled)
		{
			return new CmdResult(success: false, "Failed to initialize Sentry for testing.");
		}
		SentryService.CaptureMessage("[DevConsole] " + text);
		return new CmdResult(success: true, "Sent message to Sentry: " + text);
	}

	private static CmdResult ThrowException()
	{
		if (!SentryService.IsEnabled)
		{
			ForceInitializeForTesting();
		}
		if (!SentryService.IsEnabled)
		{
			return new CmdResult(success: false, "Failed to initialize Sentry for testing.");
		}
		try
		{
			throw new InvalidOperationException("Test exception triggered via 'sentry exception' command");
		}
		catch (Exception ex)
		{
			SentryService.CaptureException(ex);
			return new CmdResult(success: true, "Captured test exception and sent to Sentry.");
		}
	}

	private static CmdResult TriggerNativeCrash(string[] args)
	{
		if (OS.HasFeature("editor"))
		{
			return new CmdResult(success: false, "Native crash testing only works in builds, not the editor.");
		}
		if (args.Length < 2 || !args[1].Equals("confirm", StringComparison.OrdinalIgnoreCase))
		{
			return new CmdResult(success: false, "WARNING: This will crash the game!\nTo confirm, run: sentry crash confirm");
		}
		OS.Crash("Sentry test crash triggered via dev console");
		return new CmdResult(success: true, "Crash triggered");
	}

	private static void ForceInitializeForTesting()
	{
		SentryService.InitializeForTesting();
	}
}
