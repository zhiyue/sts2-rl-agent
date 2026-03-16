using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Saves;

public class SettingsSave : ISaveSchema
{
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("fps_limit")]
	public int FpsLimit { get; set; } = 60;

	[JsonPropertyName("language")]
	public string? Language { get; set; }

	[JsonPropertyName("window_position")]
	public Vector2I WindowPosition { get; set; } = new Vector2I(-1, -1);

	[JsonPropertyName("window_size")]
	public Vector2I WindowSize { get; set; } = new Vector2I(1920, 1080);

	[JsonPropertyName("fullscreen")]
	public bool Fullscreen { get; set; } = true;

	[JsonPropertyName("aspect_ratio")]
	public AspectRatioSetting AspectRatioSetting { get; set; } = AspectRatioSetting.SixteenByNine;

	[JsonPropertyName("target_display")]
	public int TargetDisplay { get; set; } = -1;

	[JsonPropertyName("resize_windows")]
	public bool ResizeWindows { get; set; } = true;

	[JsonPropertyName("vsync")]
	public VSyncType VSync { get; set; } = VSyncType.Adaptive;

	[JsonPropertyName("msaa")]
	public int Msaa { get; set; } = 2;

	[JsonPropertyName("volume_bgm")]
	public float VolumeBgm { get; set; } = 0.5f;

	[JsonPropertyName("volume_master")]
	public float VolumeMaster { get; set; } = 0.5f;

	[JsonPropertyName("volume_sfx")]
	public float VolumeSfx { get; set; } = 0.5f;

	[JsonPropertyName("volume_ambience")]
	public float VolumeAmbience { get; set; } = 0.5f;

	[JsonPropertyName("mod_settings")]
	public ModSettings? ModSettings { get; set; }

	[JsonPropertyName("skip_intro_logo")]
	public bool SkipIntroLogo { get; set; }

	[JsonPropertyName("keyboard_mapping")]
	public Dictionary<string, string> KeyboardMapping { get; set; } = new Dictionary<string, string>();

	[JsonPropertyName("controller_mapping_type")]
	public ControllerMappingType ControllerMappingType { get; set; }

	[JsonPropertyName("controller_mapping")]
	public Dictionary<string, string> ControllerMapping { get; set; } = new Dictionary<string, string>();

	[JsonPropertyName("limit_fps_in_background")]
	public bool LimitFpsInBackground { get; set; } = true;

	[JsonPropertyName("full_console")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool FullConsole { get; set; }

	[JsonPropertyName("seen_ea_disclaimer")]
	public bool SeenEaDisclaimer { get; set; }
}
