using System.Threading.Tasks;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// Implement this on a node under NCreature to stop the NCreature from being freed until the task returned from GetDelayTask is complete
/// </summary>
public interface IDeathDelayer
{
	Task GetDelayTask();
}
