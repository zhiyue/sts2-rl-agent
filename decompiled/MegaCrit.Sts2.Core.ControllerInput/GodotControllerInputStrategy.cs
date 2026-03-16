using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.ControllerInput;

public class GodotControllerInputStrategy : IControllerInputStrategy
{
	private readonly Dictionary<StringName, StringName> _megaInputMap = new Dictionary<StringName, StringName>
	{
		{
			Controller.joystickUp,
			Controller.dPadNorth
		},
		{
			Controller.joystickDown,
			Controller.dPadSouth
		},
		{
			Controller.joystickLeft,
			Controller.dPadWest
		},
		{
			Controller.joystickRight,
			Controller.dPadEast
		}
	};

	private string? _currentControllerType;

	private ControllerConfig? _controllerConfig;

	private readonly InputEventAction _reusableEvent = new InputEventAction();

	public ControllerConfig? ControllerConfig
	{
		get
		{
			if (_controllerConfig == null)
			{
				UpdateControllerConfig();
			}
			return _controllerConfig;
		}
	}

	public Dictionary<StringName, StringName> GetDefaultControllerInputMap
	{
		get
		{
			if (ControllerConfig == null)
			{
				UpdateControllerConfig();
			}
			return ControllerConfig.DefaultControllerInputMap;
		}
	}

	public bool ShouldAllowControllerRebinding => true;

	public Task Init()
	{
		UpdateControllerConfig();
		return Task.FromResult(result: true);
	}

	public void ProcessInput()
	{
		StringName[] allControllerInputs = Controller.AllControllerInputs;
		foreach (StringName action in allControllerInputs)
		{
			if (Input.IsActionJustPressed(action))
			{
				UpdateControllerConfig();
			}
		}
		foreach (KeyValuePair<StringName, StringName> item in _megaInputMap)
		{
			if (Input.IsActionJustPressed(item.Key))
			{
				_reusableEvent.Action = item.Value;
				_reusableEvent.Pressed = true;
				Input.ParseInputEvent(_reusableEvent);
			}
			else if (Input.IsActionJustReleased(item.Key))
			{
				_reusableEvent.Action = item.Value;
				_reusableEvent.Pressed = false;
				Input.ParseInputEvent(_reusableEvent);
			}
		}
	}

	private void UpdateControllerConfig()
	{
		if (Input.GetConnectedJoypads().Count == 0)
		{
			_controllerConfig = new SteamControllerConfig();
			return;
		}
		string joyName = Input.GetJoyName(0);
		if (!(joyName == _currentControllerType))
		{
			_currentControllerType = joyName;
			if (_currentControllerType.Contains("Xbox One") || _currentControllerType.Contains("XInput"))
			{
				_controllerConfig = new XboxOneConfig();
			}
			else if (_currentControllerType.Contains("Xbox 360"))
			{
				_controllerConfig = new Xbox360Config();
			}
			else if (_currentControllerType.Contains("PS3"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("PS4") || _currentControllerType.Contains("DualSense"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("PS5"))
			{
				_controllerConfig = new Ps4Config();
			}
			else if (_currentControllerType.Contains("Switch"))
			{
				_controllerConfig = new SwitchConfig();
			}
			else
			{
				_controllerConfig = new SteamControllerConfig();
			}
			NControllerManager.Instance?.OnControllerTypeChanged();
		}
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		return _controllerConfig?.GetButtonIcon(hotkey);
	}
}
