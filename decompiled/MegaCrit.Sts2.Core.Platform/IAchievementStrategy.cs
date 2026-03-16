using MegaCrit.Sts2.Core.Achievements;

namespace MegaCrit.Sts2.Core.Platform;

public interface IAchievementStrategy
{
	void Unlock(Achievement achievement);

	void Revoke(Achievement achievement);

	bool IsUnlocked(Achievement achievement);
}
