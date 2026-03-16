namespace MegaCrit.Sts2.Core.Saves;

public class StaticProfileIdProvider : IProfileIdProvider
{
	private int _profileId;

	public int CurrentProfileId => _profileId;

	public StaticProfileIdProvider(int profileId)
	{
		_profileId = profileId;
	}
}
