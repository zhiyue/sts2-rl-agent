using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class MapCmd
{
	public static void SetBossEncounter(IRunState runState, EncounterModel boss)
	{
		runState.Act.SetBossEncounter(boss);
		if (TestMode.IsOff)
		{
			NRun.Instance.GlobalUi.TopBar.BossIcon.RefreshBossIcon();
			NMapScreen.Instance?.SetMap(runState.Map, runState.Rng.Seed, clearDrawings: false);
		}
	}
}
