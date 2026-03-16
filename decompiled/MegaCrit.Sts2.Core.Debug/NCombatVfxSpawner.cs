using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Ui;

namespace MegaCrit.Sts2.Core.Debug;

[ScriptPath("res://src/Core/Debug/NCombatVfxSpawner.cs")]
public class NCombatVfxSpawner : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName GetRandomColor = "GetRandomColor";

		public static readonly StringName TestFunctionA = "TestFunctionA";

		public static readonly StringName TestFunctionB = "TestFunctionB";

		public static readonly StringName TestFunctionC = "TestFunctionC";

		public static readonly StringName SpawnVfx = "SpawnVfx";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _backCombatVfxContainer = "_backCombatVfxContainer";

		public static readonly StringName _combatVfxContainer = "_combatVfxContainer";

		public static readonly StringName _env = "_env";

		public static readonly StringName _playerPosition = "_playerPosition";

		public static readonly StringName _playerGroundPosition = "_playerGroundPosition";

		public static readonly StringName _enemyPosition = "_enemyPosition";

		public static readonly StringName _enemyGroundPosition = "_enemyGroundPosition";

		public static readonly StringName _defectEyePosition = "_defectEyePosition";

		public static readonly StringName _lowHpBorderVfx = "_lowHpBorderVfx";

		public static readonly StringName _gaseousScreenVfx = "_gaseousScreenVfx";

		public static readonly StringName _shiftPressed = "_shiftPressed";
	}

	public new class SignalName : Control.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Node2D _backCombatVfxContainer;

	[Export(PropertyHint.None, "")]
	private Control _combatVfxContainer;

	private WorldEnvironment _env;

	[Export(PropertyHint.None, "")]
	private Node2D _playerPosition;

	[Export(PropertyHint.None, "")]
	private Node2D _playerGroundPosition;

	[Export(PropertyHint.None, "")]
	private Node2D _enemyPosition;

	[Export(PropertyHint.None, "")]
	private Node2D _enemyGroundPosition;

	[Export(PropertyHint.None, "")]
	private Node2D _defectEyePosition;

	[Export(PropertyHint.None, "")]
	private NLowHpBorderVfx _lowHpBorderVfx;

	[Export(PropertyHint.None, "")]
	private NGaseousScreenVfx _gaseousScreenVfx;

	private decimal _damage = 10m;

	private bool _shiftPressed;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		_shiftPressed = Input.IsKeyPressed(Key.Shift);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey inputEventKey && inputEventKey.Keycode == Key.Q && inputEventKey.Pressed)
		{
			TestFunctionA(_shiftPressed);
		}
		if (inputEvent is InputEventKey inputEventKey2 && inputEventKey2.Keycode == Key.W && inputEventKey2.Pressed)
		{
			TestFunctionB(_shiftPressed);
		}
		if (inputEvent is InputEventKey inputEventKey3 && inputEventKey3.Keycode == Key.E && inputEventKey3.Pressed)
		{
			TestFunctionC(_shiftPressed);
		}
	}

	private static Color GetRandomColor()
	{
		return new Color(Mathf.Lerp(0.35f, 1f, GD.Randf()), Mathf.Lerp(0.35f, 1f, GD.Randf()), Mathf.Lerp(0.35f, 1f, GD.Randf()));
	}

	private void TestFunctionA(bool shiftPressed)
	{
		_lowHpBorderVfx.Play();
	}

	private void TestFunctionB(bool shiftPressed)
	{
		_gaseousScreenVfx.Play();
	}

	private void TestFunctionC(bool shiftPressed)
	{
		NWormyImpactVfx child = NWormyImpactVfx.Create(_playerGroundPosition.GlobalPosition, _playerPosition.GlobalPosition);
		_combatVfxContainer.AddChildSafely(child);
	}

	private void SpawnVfx()
	{
		NGaseousImpactVfx child = NGaseousImpactVfx.Create(_enemyPosition.GlobalPosition, GetRandomColor());
		_combatVfxContainer.AddChildSafely(child);
	}

	private async Task SpawningVfx()
	{
		NItemThrowVfx child = NItemThrowVfx.Create(_playerPosition.GlobalPosition + Vector2.Up * 150f, _enemyPosition.GlobalPosition, null);
		_combatVfxContainer.AddChildSafely(child);
		await Cmd.Wait(0.55f);
		NSplashVfx child2 = NSplashVfx.Create(_enemyPosition.GlobalPosition, new Color(0.25f, 1f, 0.4f));
		_combatVfxContainer.AddChildSafely(child2);
	}

	private async Task Hyperbeaming()
	{
		NHyperbeamVfx child = NHyperbeamVfx.Create(_defectEyePosition.GlobalPosition, _enemyPosition.GlobalPosition);
		_combatVfxContainer.AddChildSafely(child);
		await Cmd.Wait(NHyperbeamVfx.hyperbeamAnticipationDuration);
		NHyperbeamImpactVfx child2 = NHyperbeamImpactVfx.Create(_defectEyePosition.GlobalPosition, _enemyPosition.GlobalPosition);
		_combatVfxContainer.AddChildSafely(child2);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetRandomColor, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.TestFunctionA, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "shiftPressed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TestFunctionB, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "shiftPressed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TestFunctionC, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "shiftPressed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SpawnVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRandomColor && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Color>(GetRandomColor());
			return true;
		}
		if (method == MethodName.TestFunctionA && args.Count == 1)
		{
			TestFunctionA(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TestFunctionB && args.Count == 1)
		{
			TestFunctionB(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TestFunctionC && args.Count == 1)
		{
			TestFunctionC(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnVfx && args.Count == 0)
		{
			SpawnVfx();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetRandomColor && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Color>(GetRandomColor());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.GetRandomColor)
		{
			return true;
		}
		if (method == MethodName.TestFunctionA)
		{
			return true;
		}
		if (method == MethodName.TestFunctionB)
		{
			return true;
		}
		if (method == MethodName.TestFunctionC)
		{
			return true;
		}
		if (method == MethodName.SpawnVfx)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backCombatVfxContainer)
		{
			_backCombatVfxContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._combatVfxContainer)
		{
			_combatVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._env)
		{
			_env = VariantUtils.ConvertTo<WorldEnvironment>(in value);
			return true;
		}
		if (name == PropertyName._playerPosition)
		{
			_playerPosition = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._playerGroundPosition)
		{
			_playerGroundPosition = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._enemyPosition)
		{
			_enemyPosition = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._enemyGroundPosition)
		{
			_enemyGroundPosition = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._defectEyePosition)
		{
			_defectEyePosition = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._lowHpBorderVfx)
		{
			_lowHpBorderVfx = VariantUtils.ConvertTo<NLowHpBorderVfx>(in value);
			return true;
		}
		if (name == PropertyName._gaseousScreenVfx)
		{
			_gaseousScreenVfx = VariantUtils.ConvertTo<NGaseousScreenVfx>(in value);
			return true;
		}
		if (name == PropertyName._shiftPressed)
		{
			_shiftPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._backCombatVfxContainer)
		{
			value = VariantUtils.CreateFrom(in _backCombatVfxContainer);
			return true;
		}
		if (name == PropertyName._combatVfxContainer)
		{
			value = VariantUtils.CreateFrom(in _combatVfxContainer);
			return true;
		}
		if (name == PropertyName._env)
		{
			value = VariantUtils.CreateFrom(in _env);
			return true;
		}
		if (name == PropertyName._playerPosition)
		{
			value = VariantUtils.CreateFrom(in _playerPosition);
			return true;
		}
		if (name == PropertyName._playerGroundPosition)
		{
			value = VariantUtils.CreateFrom(in _playerGroundPosition);
			return true;
		}
		if (name == PropertyName._enemyPosition)
		{
			value = VariantUtils.CreateFrom(in _enemyPosition);
			return true;
		}
		if (name == PropertyName._enemyGroundPosition)
		{
			value = VariantUtils.CreateFrom(in _enemyGroundPosition);
			return true;
		}
		if (name == PropertyName._defectEyePosition)
		{
			value = VariantUtils.CreateFrom(in _defectEyePosition);
			return true;
		}
		if (name == PropertyName._lowHpBorderVfx)
		{
			value = VariantUtils.CreateFrom(in _lowHpBorderVfx);
			return true;
		}
		if (name == PropertyName._gaseousScreenVfx)
		{
			value = VariantUtils.CreateFrom(in _gaseousScreenVfx);
			return true;
		}
		if (name == PropertyName._shiftPressed)
		{
			value = VariantUtils.CreateFrom(in _shiftPressed);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backCombatVfxContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatVfxContainer, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._env, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerPosition, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerGroundPosition, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enemyPosition, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enemyGroundPosition, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defectEyePosition, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lowHpBorderVfx, PropertyHint.NodeType, "ColorRect", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gaseousScreenVfx, PropertyHint.NodeType, "AspectRatioContainer", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._shiftPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backCombatVfxContainer, Variant.From(in _backCombatVfxContainer));
		info.AddProperty(PropertyName._combatVfxContainer, Variant.From(in _combatVfxContainer));
		info.AddProperty(PropertyName._env, Variant.From(in _env));
		info.AddProperty(PropertyName._playerPosition, Variant.From(in _playerPosition));
		info.AddProperty(PropertyName._playerGroundPosition, Variant.From(in _playerGroundPosition));
		info.AddProperty(PropertyName._enemyPosition, Variant.From(in _enemyPosition));
		info.AddProperty(PropertyName._enemyGroundPosition, Variant.From(in _enemyGroundPosition));
		info.AddProperty(PropertyName._defectEyePosition, Variant.From(in _defectEyePosition));
		info.AddProperty(PropertyName._lowHpBorderVfx, Variant.From(in _lowHpBorderVfx));
		info.AddProperty(PropertyName._gaseousScreenVfx, Variant.From(in _gaseousScreenVfx));
		info.AddProperty(PropertyName._shiftPressed, Variant.From(in _shiftPressed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backCombatVfxContainer, out var value))
		{
			_backCombatVfxContainer = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._combatVfxContainer, out var value2))
		{
			_combatVfxContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._env, out var value3))
		{
			_env = value3.As<WorldEnvironment>();
		}
		if (info.TryGetProperty(PropertyName._playerPosition, out var value4))
		{
			_playerPosition = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._playerGroundPosition, out var value5))
		{
			_playerGroundPosition = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._enemyPosition, out var value6))
		{
			_enemyPosition = value6.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._enemyGroundPosition, out var value7))
		{
			_enemyGroundPosition = value7.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._defectEyePosition, out var value8))
		{
			_defectEyePosition = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._lowHpBorderVfx, out var value9))
		{
			_lowHpBorderVfx = value9.As<NLowHpBorderVfx>();
		}
		if (info.TryGetProperty(PropertyName._gaseousScreenVfx, out var value10))
		{
			_gaseousScreenVfx = value10.As<NGaseousScreenVfx>();
		}
		if (info.TryGetProperty(PropertyName._shiftPressed, out var value11))
		{
			_shiftPressed = value11.As<bool>();
		}
	}
}
