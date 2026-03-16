using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

public interface ICharacterSelectButtonDelegate
{
	StartRunLobby Lobby { get; }

	void SelectCharacter(NCharacterSelectButton charSelectButton, CharacterModel characterModel);
}
