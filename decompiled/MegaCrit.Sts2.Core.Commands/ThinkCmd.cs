using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Commands;

public static class ThinkCmd
{
	private const double _defaultTimePerCharacter = 0.08;

	private const double _minTimeToDisplay = 1.5;

	public static void Play(LocString line, Creature speaker, double secondsToDisplay = -1.0)
	{
		string formattedText = line.GetFormattedText();
		if (secondsToDisplay < 0.0)
		{
			secondsToDisplay = (double)formattedText.Length * 0.08;
		}
		if (secondsToDisplay < 1.5)
		{
			secondsToDisplay = 1.5;
		}
		NThoughtBubbleVfx nThoughtBubbleVfx = NThoughtBubbleVfx.Create(formattedText, speaker, secondsToDisplay);
		if (nThoughtBubbleVfx != null)
		{
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nThoughtBubbleVfx);
		}
	}
}
