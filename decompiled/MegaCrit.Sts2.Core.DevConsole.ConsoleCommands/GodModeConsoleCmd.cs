using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class GodModeConsoleCmd : AbstractConsoleCmd
{
	private bool _godModeActive;

	private Player? _godModePlayer;

	public override string CmdName => "godmode";

	public override string Args => "";

	public override string Description => "Become invincible!";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
		{
			return new CmdResult(success: false, "A run does not appear to be in progress");
		}
		Task task;
		if (_godModeActive)
		{
			_godModeActive = false;
			CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
			_godModePlayer = null;
			task = DisableGodMode(issuingPlayer);
			return new CmdResult(task, success: true, "Godmode deactivated!");
		}
		_godModeActive = true;
		_godModePlayer = issuingPlayer;
		CombatManager.Instance.CombatSetUp += OnCombatSetUp;
		task = EnableGodMode(issuingPlayer);
		return new CmdResult(task, success: true, "Godmode activated!");
	}

	private void OnCombatSetUp(CombatState combatState)
	{
		if (_godModePlayer != null && RunManager.Instance.IsInProgress)
		{
			Player me = LocalContext.GetMe(combatState.RunState);
			if (me == _godModePlayer)
			{
				TaskHelper.RunSafely(EnableGodMode(_godModePlayer));
			}
		}
	}

	private static async Task DisableGodMode(Player player)
	{
		Creature playerCreature = player.Creature;
		await PowerCmd.Remove<StrengthPower>(playerCreature);
		await PowerCmd.Remove<BufferPower>(playerCreature);
		await PowerCmd.Remove<RegenPower>(playerCreature);
	}

	private static async Task EnableGodMode(Player player)
	{
		Creature playerCreature = player.Creature;
		await PowerCmd.Apply<StrengthPower>(playerCreature, 9999m, playerCreature, null);
		await PowerCmd.Apply<BufferPower>(playerCreature, 9999m, playerCreature, null);
		await PowerCmd.Apply<RegenPower>(playerCreature, 9999m, playerCreature, null);
	}
}
