using Godot;

namespace MegaCrit.Sts2.Core.ControllerInput;

public static class Controller
{
	public static readonly StringName leftTrigger = "controller_left_trigger";

	public static readonly StringName rightTrigger = "controller_right_trigger";

	public static readonly StringName leftBumper = "controller_left_bumper";

	public static readonly StringName rightBumper = "controller_right_bumper";

	public static readonly StringName dPadNorth = "controller_d_pad_north";

	public static readonly StringName dPadSouth = "controller_d_pad_south";

	public static readonly StringName dPadEast = "controller_d_pad_east";

	public static readonly StringName dPadWest = "controller_d_pad_west";

	public static readonly StringName faceButtonNorth = "controller_face_button_north";

	public static readonly StringName faceButtonSouth = "controller_face_button_south";

	public static readonly StringName faceButtonEast = "controller_face_button_east";

	public static readonly StringName faceButtonWest = "controller_face_button_west";

	public static readonly StringName startButton = "controller_start_button";

	public static readonly StringName selectButton = "controller_select_button";

	public static readonly StringName joystickPress = "controller_joystick_press";

	public static readonly StringName joystickLeft = "controller_joystick_left";

	public static readonly StringName joystickRight = "controller_joystick_right";

	public static readonly StringName joystickUp = "controller_joystick_up";

	public static readonly StringName joystickDown = "controller_joystick_down";

	public static readonly StringName ps4Touchpad = "ui_controller_touch_pad";

	public static StringName[] AllControllerInputs => new StringName[20]
	{
		dPadEast, dPadNorth, dPadSouth, dPadWest, faceButtonEast, faceButtonNorth, faceButtonSouth, faceButtonWest, joystickDown, joystickLeft,
		joystickPress, joystickRight, joystickUp, leftBumper, leftTrigger, rightBumper, rightTrigger, selectButton, startButton, ps4Touchpad
	};
}
