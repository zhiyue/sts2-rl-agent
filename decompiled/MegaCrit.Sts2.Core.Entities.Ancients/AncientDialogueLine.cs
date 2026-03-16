using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogueLine
{
	public const string sfxFallbackPath = "event:/sfx/ui/enchant_simple";

	private readonly string _sfxPath;

	public LocString? LineText { get; set; }

	public LocString? NextButtonText { get; set; }

	public AncientDialogueSpeaker Speaker { get; set; }

	public AncientDialogueLine(string sfxPath)
	{
		_sfxPath = sfxPath;
	}

	public string GetSfxOrFallbackPath()
	{
		if (!string.IsNullOrEmpty(_sfxPath))
		{
			return _sfxPath;
		}
		return "event:/sfx/ui/enchant_simple";
	}
}
