using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Runs;

/// <summary>
/// Extra fields that are used for specific pieces of content within a run.
/// </summary>
public class ExtraRunFields
{
	/// <summary>
	/// Whether or not the player started the run at the <see cref="T:MegaCrit.Sts2.Core.Models.Events.Neow" /> Ancient.
	/// This is used to determine whether or not to show the first node in Act 1 as Neow when loading from a save.
	/// If the player starts a singleplayer run, then unlocks the Neow epoch in multiplayer and places it, we don't want
	/// to generate the map upon load with Neow in it.
	/// </summary>
	public bool StartedWithNeow { get; set; }

	/// <summary>
	/// Number of times the Test Subject was killed in this run. We need to save this so that it gets saved to the
	/// SerializableProgress at the end of the run.
	/// </summary>
	public int TestSubjectKills { get; set; }

	/// <summary>
	/// Whether or not the player went to the <see cref="T:MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy" /> event and freed Repy in this run.
	/// This is then used to determine wither to show Repy in the background of the <see cref="T:MegaCrit.Sts2.Core.Models.Encounters.QueenBoss" /> encounter.
	/// </summary>
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
