using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NCommandHistory.cs")]
public class NCommandHistory : Panel
{
	public new class MethodName : Panel.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName SetBackgroundColor = "SetBackgroundColor";

		public static readonly StringName ShowConsole = "ShowConsole";

		public static readonly StringName HideConsole = "HideConsole";

		public static readonly StringName GetHistory = "GetHistory";

		public static readonly StringName Refresh = "Refresh";

		public static readonly StringName GetText = "GetText";
	}

	public new class PropertyName : Panel.PropertyName
	{
		public static readonly StringName _outputBuffer = "_outputBuffer";
	}

	public new class SignalName : Panel.SignalName
	{
	}

	private static NCommandHistory? _instance;

	private RichTextLabel _outputBuffer;

	public override void _Ready()
	{
		if (_instance != null)
		{
			this.QueueFreeSafely();
			return;
		}
		_instance = this;
		HideConsole();
		_outputBuffer = GetNode<RichTextLabel>("OutputContainer/OutputBuffer");
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!(inputEvent is InputEventKey { Pressed: not false } inputEventKey))
		{
			return;
		}
		if (inputEventKey.Keycode == Key.Plus)
		{
			if (base.Visible)
			{
				HideConsole();
			}
			else
			{
				ShowConsole();
			}
		}
		if (base.Visible && inputEventKey.Keycode == Key.Escape)
		{
			HideConsole();
		}
	}

	public void SetBackgroundColor(Color color)
	{
		base.Modulate = color;
	}

	public void ShowConsole()
	{
		base.Visible = true;
		CombatManager.Instance.History.Changed += Refresh;
		Refresh();
	}

	public void HideConsole()
	{
		CombatManager.Instance.History.Changed -= Refresh;
		base.Visible = false;
	}

	public static string GetHistory()
	{
		if (_instance != null)
		{
			return GetText();
		}
		return string.Empty;
	}

	private void Refresh()
	{
		_outputBuffer.Text = GetText();
	}

	private static string GetText()
	{
		return string.Join('\n', CombatManager.Instance.History.Entries.Select((CombatHistoryEntry e) => e.HumanReadableString));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetBackgroundColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowConsole, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideConsole, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetHistory, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.Refresh, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetText, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
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
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetBackgroundColor && args.Count == 1)
		{
			SetBackgroundColor(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowConsole && args.Count == 0)
		{
			ShowConsole();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideConsole && args.Count == 0)
		{
			HideConsole();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHistory && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetHistory());
			return true;
		}
		if (method == MethodName.Refresh && args.Count == 0)
		{
			Refresh();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetText && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetText());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetHistory && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetHistory());
			return true;
		}
		if (method == MethodName.GetText && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetText());
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.SetBackgroundColor)
		{
			return true;
		}
		if (method == MethodName.ShowConsole)
		{
			return true;
		}
		if (method == MethodName.HideConsole)
		{
			return true;
		}
		if (method == MethodName.GetHistory)
		{
			return true;
		}
		if (method == MethodName.Refresh)
		{
			return true;
		}
		if (method == MethodName.GetText)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._outputBuffer)
		{
			_outputBuffer = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._outputBuffer)
		{
			value = VariantUtils.CreateFrom(in _outputBuffer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outputBuffer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outputBuffer, Variant.From(in _outputBuffer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outputBuffer, out var value))
		{
			_outputBuffer = value.As<RichTextLabel>();
		}
	}
}
