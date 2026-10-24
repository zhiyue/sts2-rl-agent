using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Runs.Metrics;

public struct SettingsDataMetric
{
	[JsonPropertyName("buildId")]
	public required string BuildId { get; set; }

	[JsonPropertyName("os")]
	public string Os { get; set; }

	[JsonPropertyName("platform")]
	public string Platform { get; set; }

	[JsonPropertyName("systemRam")]
	public int SystemRam { get; set; }

	[JsonPropertyName("language")]
	public string LanguageCode { get; set; }

	[JsonPropertyName("combatSpeed")]
	public required FastModeType FastModeType { get; set; }

	[JsonPropertyName("screenshake")]
	public int Screenshake { get; set; }

	[JsonPropertyName("runTimer")]
	public bool ShowRunTimer { get; set; }

	[JsonPropertyName("phobiaMode")]
	public bool PhobiaMode { get; set; }

	[JsonPropertyName("cardIndices")]
	public bool ShowCardIndices { get; set; }

	[JsonPropertyName("displayCount")]
	public int DisplayCount { get; set; }

	[JsonPropertyName("displayResolution")]
	public Vector2I DisplayResolution { get; set; }

	[JsonPropertyName("fullscreen")]
	public bool Fullscreen { get; set; }

	[JsonPropertyName("aspectRatio")]
	public AspectRatioSetting AspectRatio { get; set; }

	[JsonPropertyName("resizeWindows")]
	public bool ResizeWindows { get; set; }

	[JsonPropertyName("vSync")]
	public VSyncType VSync { get; set; }

	[JsonPropertyName("fpsLimit")]
	public int FpsLimit { get; set; }

	[JsonPropertyName("msaa")]
	public int Msaa { get; set; }
}
