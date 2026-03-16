using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.ControllerInput;

public class SteamControllerInputStrategy : IControllerInputStrategy
{
	private Dictionary<EInputActionOrigin, StringName> _steamInputsToMegaInputs = new Dictionary<EInputActionOrigin, StringName>
	{
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_A,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_B,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_X,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_Y,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftTrigger_Pull,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftTrigger_Click,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_RightTrigger_Pull,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_RightTrigger_Click,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_DPadNorth,
			Controller.joystickUp
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_DPadSouth,
			Controller.joystickDown
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_DPadWest,
			Controller.joystickLeft
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_DPadEast,
			Controller.joystickRight
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_Click,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_Start,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamController_Back,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_X,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_Circle,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_Square,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_Triangle,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftTrigger_Pull,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftTrigger_Click,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_RightTrigger_Pull,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_RightTrigger_Click,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_DPadNorth,
			Controller.joystickUp
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_DPadSouth,
			Controller.joystickDown
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_DPadWest,
			Controller.joystickLeft
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_DPadEast,
			Controller.joystickRight
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_Click,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_Options,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_Share,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_CenterPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_RightPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS4_LeftPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_X,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_Circle,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_Square,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_Triangle,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftTrigger_Pull,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftTrigger_Click,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_RightTrigger_Pull,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_RightTrigger_Click,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_DPadNorth,
			Controller.joystickUp
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_DPadSouth,
			Controller.joystickDown
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_DPadWest,
			Controller.joystickLeft
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_DPadEast,
			Controller.joystickRight
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_Click,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_Option,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_Create,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_CenterPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_RightPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_PS5_LeftPad_Click,
			Controller.ps4Touchpad
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_A,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_B,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_X,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_Y,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftTrigger_Pull,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftTrigger_Click,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightTrigger_Pull,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightTrigger_Click,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_DPadNorth,
			Controller.joystickUp
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_DPadSouth,
			Controller.joystickDown
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_DPadWest,
			Controller.joystickLeft
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_DPadEast,
			Controller.joystickRight
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_Click,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_Menu,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBoxOne_View,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_A,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_B,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_X,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_Y,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Pull,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Click,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Pull,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Click,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_Start,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_XBox360_Back,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_A,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_B,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_X,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_Y,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_LeftBumper,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_RightBumper,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_Plus,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_Switch_Minus,
			Controller.selectButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_A,
			Controller.faceButtonSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_B,
			Controller.faceButtonEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_X,
			Controller.faceButtonWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_Y,
			Controller.faceButtonNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L1,
			Controller.leftBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R1,
			Controller.rightBumper
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L2,
			Controller.leftTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R2,
			Controller.rightTrigger
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L3,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R3,
			Controller.joystickPress
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_DPad_North,
			Controller.dPadNorth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_DPad_South,
			Controller.dPadSouth
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_DPad_West,
			Controller.dPadWest
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_DPad_East,
			Controller.dPadEast
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_Menu,
			Controller.startButton
		},
		{
			EInputActionOrigin.k_EInputActionOrigin_SteamDeck_View,
			Controller.selectButton
		}
	};

	private InputHandle_t? _currentControllerHandle;

	private InputActionSetHandle_t? _currentActionSetHandle;

	private ControllerConfig? _controllerConfig;

	private readonly List<string> _pressedInputs = new List<string>();

	private readonly Dictionary<StringName, InputEventAction> _inputEvents = new Dictionary<StringName, InputEventAction>();

	private readonly IControllerInputStrategy _fallbackStrategy = new GodotControllerInputStrategy();

	private double _nextControllerCheckTime;

	private readonly Dictionary<StringName, InputDigitalActionHandle_t> _digitalActionHandleCache = new Dictionary<StringName, InputDigitalActionHandle_t>();

	private Dictionary<EInputActionOrigin, Texture2D> _fallbackSteamGlyphs = new Dictionary<EInputActionOrigin, Texture2D>();

	private InputAnalogActionHandle_t _joystickActionHandle;

	private Vector2 _joystickPosition;

	private InputEventJoypadMotion _joystickXAxis;

	private InputEventJoypadMotion _joystickYAxis;

	private StringName _up;

	private StringName _down;

	private StringName _left;

	private StringName _right;

	public ControllerConfig? ControllerConfig => _controllerConfig ?? _fallbackStrategy.ControllerConfig;

	public Dictionary<StringName, StringName> GetDefaultControllerInputMap
	{
		get
		{
			if (ControllerConfig == null)
			{
				return _fallbackStrategy.ControllerConfig.DefaultControllerInputMap;
			}
			return ControllerConfig.DefaultControllerInputMap;
		}
	}

	public bool ShouldAllowControllerRebinding
	{
		get
		{
			if (!SteamInitializer.Initialized)
			{
				return _fallbackStrategy.ShouldAllowControllerRebinding;
			}
			return false;
		}
	}

	public async Task Init()
	{
		await NControllerManager.Instance.ToSignal(NControllerManager.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
		if (SteamInitializer.Initialized)
		{
			try
			{
				SteamInput.Init(bExplicitlyCallRunFrame: false);
				UpdateControllerConnections();
				_joystickXAxis = new InputEventJoypadMotion
				{
					Axis = JoyAxis.LeftX,
					Device = 0
				};
				_joystickYAxis = new InputEventJoypadMotion
				{
					Axis = JoyAxis.LeftY,
					Device = 0
				};
				_up = new StringName("Up");
				_down = new StringName("Down");
				_left = new StringName("Left");
				_right = new StringName("Right");
				_joystickActionHandle = SteamInput.GetAnalogActionHandle("Joystick");
				NInputManager.Instance.ResetToDefaultControllerMapping();
			}
			catch (InvalidOperationException ex)
			{
				Log.Error("Failed to initialize Steam Input: " + ex.Message);
			}
		}
		else
		{
			Log.Warn("Cannot initialize Steam Input because Steamworks is not initialized. Falling back to standard input.");
		}
		await _fallbackStrategy.Init();
	}

	public void ProcessInput()
	{
		if (!SteamInitializer.Initialized)
		{
			_fallbackStrategy.ProcessInput();
			return;
		}
		double num = (double)Time.GetTicksMsec() / 1000.0;
		if (num >= _nextControllerCheckTime)
		{
			_nextControllerCheckTime = num + 1.0;
			UpdateControllerConnections();
		}
		if (!_currentControllerHandle.HasValue)
		{
			_fallbackStrategy.ProcessInput();
			return;
		}
		try
		{
			SteamInput.RunFrame();
			ProcessDigitalInputs();
			ProcessAnalogInputs();
		}
		catch (InvalidOperationException ex)
		{
			Log.Error("Error running Steam Input frame: " + ex.Message);
			_currentControllerHandle = null;
			_fallbackStrategy.ProcessInput();
		}
	}

	private void ProcessDigitalInputs()
	{
		if (_controllerConfig == null)
		{
			return;
		}
		foreach (KeyValuePair<string, StringName> item in _controllerConfig.SteamInputControllerMap)
		{
			if (!_digitalActionHandleCache.TryGetValue(item.Key, out var value))
			{
				Log.Error("The input " + item.Key + " was not cached during initialization. Skipping...");
				continue;
			}
			bool flag = SteamInput.GetDigitalActionData(_currentControllerHandle.Value, value).bState == 1;
			bool flag2 = _pressedInputs.Contains(item.Key);
			if (flag && !flag2)
			{
				_pressedInputs.Add(item.Key);
			}
			else if (!flag && flag2)
			{
				_pressedInputs.Remove(item.Key);
			}
			if (flag && !flag2)
			{
				InputEventAction inputEventAction = _inputEvents[item.Key];
				inputEventAction.Pressed = true;
				Input.ParseInputEvent(inputEventAction);
			}
			else if (!flag && flag2)
			{
				InputEventAction inputEventAction2 = _inputEvents[item.Key];
				inputEventAction2.Pressed = false;
				Input.ParseInputEvent(inputEventAction2);
			}
		}
	}

	private void ProcessAnalogInputs()
	{
		InputAnalogActionData_t analogActionData = SteamInput.GetAnalogActionData(_currentControllerHandle.Value, _joystickActionHandle);
		Vector2 joystickPosition = new Vector2(analogActionData.x, analogActionData.y);
		if (joystickPosition.DistanceTo(_joystickPosition) > 0.05f)
		{
			InputEventJoypadMotion joystickXAxis = _joystickXAxis;
			joystickXAxis.AxisValue = joystickPosition.X;
			InputEventJoypadMotion joystickYAxis = _joystickYAxis;
			joystickYAxis.AxisValue = 0f - joystickPosition.Y;
			Input.ParseInputEvent(joystickXAxis);
			Input.ParseInputEvent(joystickYAxis);
		}
		if (joystickPosition.Y >= 0.5f && !_pressedInputs.Contains("Joy_Up"))
		{
			InputEventAction inputEventAction = _inputEvents[_up];
			inputEventAction.Pressed = true;
			Input.ParseInputEvent(inputEventAction);
			_pressedInputs.Add("Joy_Up");
		}
		else if (joystickPosition.Y < 0.5f && _pressedInputs.Contains("Joy_Up"))
		{
			InputEventAction inputEventAction2 = _inputEvents[_up];
			inputEventAction2.Pressed = false;
			Input.ParseInputEvent(inputEventAction2);
			_pressedInputs.Remove("Joy_Up");
		}
		if (joystickPosition.Y <= -0.5f && !_pressedInputs.Contains("Joy_Down"))
		{
			InputEventAction inputEventAction3 = _inputEvents[_down];
			inputEventAction3.Pressed = true;
			Input.ParseInputEvent(inputEventAction3);
			_pressedInputs.Add("Joy_Down");
		}
		else if (joystickPosition.Y > -0.5f && _pressedInputs.Contains("Joy_Down"))
		{
			InputEventAction inputEventAction4 = _inputEvents[_down];
			inputEventAction4.Pressed = false;
			Input.ParseInputEvent(inputEventAction4);
			_pressedInputs.Remove("Joy_Down");
		}
		if (joystickPosition.X <= -0.5f && !_pressedInputs.Contains("Joy_Left"))
		{
			InputEventAction inputEventAction5 = _inputEvents[_left];
			inputEventAction5.Pressed = true;
			Input.ParseInputEvent(inputEventAction5);
			_pressedInputs.Add("Joy_Left");
		}
		else if (joystickPosition.X > -0.5f && _pressedInputs.Contains("Joy_Left"))
		{
			InputEventAction inputEventAction6 = _inputEvents[_left];
			inputEventAction6.Pressed = false;
			Input.ParseInputEvent(inputEventAction6);
			_pressedInputs.Remove("Joy_Left");
		}
		if (joystickPosition.X >= 0.5f && !_pressedInputs.Contains("Joy_Right"))
		{
			InputEventAction inputEventAction7 = _inputEvents[_right];
			inputEventAction7.Pressed = true;
			Input.ParseInputEvent(inputEventAction7);
			_pressedInputs.Add("Joy_Right");
		}
		else if (joystickPosition.X < 0.5f && _pressedInputs.Contains("Joy_Right"))
		{
			InputEventAction inputEventAction8 = _inputEvents[_right];
			inputEventAction8.Pressed = false;
			Input.ParseInputEvent(inputEventAction8);
			_pressedInputs.Remove("Joy_Right");
		}
		_joystickPosition = joystickPosition;
	}

	private void UpdateControllerConnections()
	{
		try
		{
			InputHandle_t[] array = new InputHandle_t[16];
			if (SteamInput.GetConnectedControllers(array) == 0)
			{
				_currentControllerHandle = null;
				if (_controllerConfig == null)
				{
					UpdateControllerConfig(ESteamInputType.k_ESteamInputType_SteamDeckController);
				}
				return;
			}
			ESteamInputType eSteamInputType = ESteamInputType.k_ESteamInputType_Unknown;
			if (_currentControllerHandle.HasValue)
			{
				eSteamInputType = SteamInput.GetInputTypeForHandle(_currentControllerHandle.Value);
			}
			_currentControllerHandle = array[0];
			ESteamInputType inputTypeForHandle = SteamInput.GetInputTypeForHandle(_currentControllerHandle.Value);
			if (eSteamInputType != inputTypeForHandle)
			{
				UpdateControllerConfig(inputTypeForHandle);
				UpdateInputMap();
			}
			_currentActionSetHandle = SteamInput.GetActionSetHandle("Controls");
			SteamInput.ActivateActionSet(_currentControllerHandle.Value, _currentActionSetHandle.Value);
		}
		catch (InvalidOperationException ex)
		{
			Log.Error("Failed to connect to Steam controller: " + ex.Message);
			_currentControllerHandle = null;
		}
	}

	private void UpdateControllerConfig(ESteamInputType controllerType)
	{
		ControllerConfig controllerConfig = _controllerConfig;
		switch (controllerType)
		{
		case ESteamInputType.k_ESteamInputType_XBoxOneController:
			_controllerConfig = new XboxOneConfig();
			break;
		case ESteamInputType.k_ESteamInputType_XBox360Controller:
			_controllerConfig = new Xbox360Config();
			break;
		case ESteamInputType.k_ESteamInputType_PS4Controller:
		case ESteamInputType.k_ESteamInputType_PS3Controller:
		case ESteamInputType.k_ESteamInputType_PS5Controller:
			_controllerConfig = new Ps4Config();
			break;
		case ESteamInputType.k_ESteamInputType_SwitchJoyConPair:
		case ESteamInputType.k_ESteamInputType_SwitchJoyConSingle:
		case ESteamInputType.k_ESteamInputType_SwitchProController:
			_controllerConfig = new SwitchConfig();
			break;
		default:
			_controllerConfig = new SteamControllerConfig();
			break;
		}
		if (controllerConfig != null && controllerConfig.ControllerMappingType != _controllerConfig.ControllerMappingType)
		{
			NControllerManager.Instance?.OnControllerTypeChanged();
		}
	}

	private void UpdateInputMap()
	{
		if (_controllerConfig == null)
		{
			return;
		}
		_inputEvents.Clear();
		_digitalActionHandleCache.Clear();
		foreach (KeyValuePair<string, StringName> item in _controllerConfig.SteamInputControllerMap)
		{
			_inputEvents[item.Key] = new InputEventAction
			{
				Action = item.Value
			};
		}
		foreach (string key in _controllerConfig.SteamInputControllerMap.Keys)
		{
			StringName stringName = key;
			try
			{
				InputDigitalActionHandle_t digitalActionHandle = SteamInput.GetDigitalActionHandle(stringName);
				_digitalActionHandleCache[stringName] = digitalActionHandle;
			}
			catch (InvalidOperationException ex)
			{
				Log.Error($"Failed to cache digital action handle for {stringName}: {ex.Message}");
			}
		}
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		if (!SteamInitializer.Initialized || !_currentControllerHandle.HasValue)
		{
			return _fallbackStrategy.GetHotkeyIcon(hotkey);
		}
		Dictionary<string, StringName> source = ((ControllerConfig != null) ? ControllerConfig.SteamInputControllerMap : new SteamControllerConfig().SteamInputControllerMap);
		string key = source.FirstOrDefault((KeyValuePair<string, StringName> kvp) => kvp.Value == (StringName)hotkey).Key;
		if (key == null)
		{
			return ControllerConfig?.GetButtonIcon(hotkey);
		}
		EInputActionOrigin[] array = new EInputActionOrigin[8];
		if (!_digitalActionHandleCache.TryGetValue(key, out var value))
		{
			Log.Error("The input " + key + " was not cached during initialization.");
			return ControllerConfig?.GetButtonIcon(key);
		}
		SteamInput.GetDigitalActionOrigins(_currentControllerHandle.Value, _currentActionSetHandle.Value, value, array);
		if (array.Length != 0 && array[0] != EInputActionOrigin.k_EInputActionOrigin_None)
		{
			ESteamInputType inputTypeForHandle = SteamInput.GetInputTypeForHandle(_currentControllerHandle.Value);
			EInputActionOrigin eInputActionOrigin = SteamInput.TranslateActionOrigin(inputTypeForHandle, array[0]);
			if (_steamInputsToMegaInputs.TryGetValue(eInputActionOrigin, out StringName value2))
			{
				return ControllerConfig?.GetButtonIcon(value2);
			}
			if (!_fallbackSteamGlyphs.ContainsKey(eInputActionOrigin))
			{
				string glyphSVGForActionOrigin = SteamInput.GetGlyphSVGForActionOrigin(eInputActionOrigin, 0u);
				Image image = Image.LoadFromFile(glyphSVGForActionOrigin);
				_fallbackSteamGlyphs.Add(eInputActionOrigin, ImageTexture.CreateFromImage(image));
			}
			return _fallbackSteamGlyphs[eInputActionOrigin];
		}
		return ControllerConfig?.GetButtonIcon(hotkey);
	}
}
