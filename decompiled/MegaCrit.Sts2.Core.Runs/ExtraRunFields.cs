using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

public class ExtraRunFields
{
	public bool StartedWithNeow { get; set; }

	public int TestSubjectKills { get; set; }

	public bool FreedRepy { get; set; }

	public SerializableExtraRunFields ToSerializable()
	{
		return new SerializableExtraRunFields
		{
			StartedWithNeow = StartedWithNeow,
			TestSubjectKills = TestSubjectKills,
			FreedRepy = FreedRepy
		};
	}

	public static ExtraRunFields FromSerializable(SerializableExtraRunFields save)
	{
		return new ExtraRunFields
		{
			StartedWithNeow = save.StartedWithNeow,
			TestSubjectKills = save.TestSubjectKills,
			FreedRepy = save.FreedRepy
		};
	}
}
