using System.Threading.Tasks;

namespace MegaCrit.Sts2.Core.DevConsole;

public struct CmdResult
{
	public readonly bool success;

	public readonly string msg;

	public readonly Task? task;

	public CmdResult(bool success, string? msg = null)
	{
		this.success = success;
		this.msg = msg ?? string.Empty;
		task = null;
	}

	public CmdResult(Task task, bool success, string? msg = null)
	{
		this.task = task;
		this.success = success;
		this.msg = msg ?? string.Empty;
	}
}
