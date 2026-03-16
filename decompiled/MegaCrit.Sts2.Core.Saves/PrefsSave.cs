using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Saves;

public class PrefsSave : ISaveSchema
{
	[JsonPropertyName("schema_version")]
	public int SchemaVersion { get; set; }

	[JsonPropertyName("fast_mode")]
	public FastModeType FastMode { get; set; } = FastModeType.Normal;

	[JsonPropertyName("screenshake")]
	public int ScreenShakeOptionIndex { get; set; } = 2;

	[JsonPropertyName("show_run_timer")]
	public bool ShowRunTimer { get; set; }

	[JsonPropertyName("show_card_indices")]
	public bool ShowCardIndices { get; set; }

	[JsonPropertyName("upload_data")]
	public bool UploadData { get; set; } = true;

	[JsonPropertyName("mute_in_background")]
	public bool MuteInBackground { get; set; } = true;

	[JsonPropertyName("long_press")]
	public bool IsLongPressEnabled { get; set; }

	[JsonPropertyName("text_effects_enabled")]
	public bool TextEffectsEnabled { get; set; } = true;
}
