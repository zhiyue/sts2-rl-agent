using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug.Multiplayer;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class MultiplayerConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "multiplayer";

	public override string Args => "";

	public override string Description => "Opens the multiplayer menu, or the test scene if test is the first argument";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length != 0 && args[0] == "test")
		{
			NMultiplayerTest currentScene = SceneHelper.Instantiate<NMultiplayerTest>("debug/multiplayer_test");
			NGame.Instance.RootSceneContainer.SetCurrentScene(currentScene);
			TaskHelper.RunSafely(NGame.Instance.Transition.FadeIn());
			return new CmdResult(success: true, "Opened multiplayer test scene");
		}
		NGame.Instance.MainMenu.OpenMultiplayerSubmenu(null);
		return new CmdResult(success: true, "Opened multiplayer submenu");
	}
}
