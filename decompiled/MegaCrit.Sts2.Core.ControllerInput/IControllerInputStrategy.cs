using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace MegaCrit.Sts2.Core.ControllerInput;

public interface IControllerInputStrategy
{
	ControllerConfig? ControllerConfig { get; }

	Dictionary<StringName, StringName> GetDefaultControllerInputMap { get; }

	bool ShouldAllowControllerRebinding { get; }

	Task Init();

	void ProcessInput();

	Texture2D? GetHotkeyIcon(string hotkey);
}
