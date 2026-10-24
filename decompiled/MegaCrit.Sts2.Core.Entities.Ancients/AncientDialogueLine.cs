using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogueLine
{
	public const string sfxFallbackPath = "event:/sfx/ui/enchant_simple";

	/// <summary>
	/// The path to the SFX that should be played when this line of dialogue is shown.
	/// Empty string means no SFX.
	/// </summary>
	private readonly string _sfxPath;

	/// <summary>
	/// The text that should be shown for this line of dialogue.
	/// </summary>
	public LocString? LineText { get; set; }

	/// <summary>
	/// The text that should be shown on the "Next" button that goes with this line of dialogue.
	/// Null for the last line in a dialogue.
	/// </summary>
	public LocString? NextButtonText { get; set; }

	/// <summary>
	/// The speaker of this line of dialogue.
	/// </summary>
	public AncientDialogueSpeaker Speaker { get; set; }

	public AncientDialogueLine(string sfxPath)
	{
		_sfxPath = sfxPath;
	}

	/// <summary>
	/// Get the path to the SFX to play for this line of dialogue.
	/// Returns a fallback if the SFX path is empty.
	/// </summary>
	public string GetSfxOrFallbackPath()
	{
		if (!string.IsNullOrEmpty(_sfxPath))
		{
			return _sfxPath;
		}
		return "event:/sfx/ui/enchant_simple";
	}
}
