using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class BestiaryConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "bestiary";

	public override string Args => "";

	public override string Description => "Opens the bestiary (WIP)";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		NCompendiumSubmenu nCompendiumSubmenu = NGame.Instance.MainMenu.OpenCompendiumSubmenu();
		nCompendiumSubmenu.OpenBestiary();
		return new CmdResult(success: true, "Opened bestiary submenu");
	}
}
