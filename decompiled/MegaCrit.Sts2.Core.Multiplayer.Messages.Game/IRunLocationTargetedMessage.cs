using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game;

public interface IRunLocationTargetedMessage
{
	RunLocation Location { get; }
}
