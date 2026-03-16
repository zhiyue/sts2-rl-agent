using System.Threading.Tasks;
using Godot;

namespace MegaCrit.Sts2.Core.Extensions;

public static class SignalAwaiterExtensions
{
	public static async Task ToTask(this SignalAwaiter awaiter)
	{
		await awaiter;
	}
}
