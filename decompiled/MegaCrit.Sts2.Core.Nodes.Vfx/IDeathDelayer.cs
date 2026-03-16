using System.Threading.Tasks;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

public interface IDeathDelayer
{
	Task GetDelayTask();
}
