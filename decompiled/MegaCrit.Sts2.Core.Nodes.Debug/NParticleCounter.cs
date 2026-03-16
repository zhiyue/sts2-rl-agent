using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NParticleCounter.cs")]
public class NParticleCounter : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName CheckForHotkey = "CheckForHotkey";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _icon = "_icon";

		public static readonly StringName _label = "_label";

		public static readonly StringName _secondsSinceLastUpdate = "_secondsSinceLastUpdate";

		public static readonly StringName _totalParticles = "_totalParticles";

		public static readonly StringName _updateCount = "_updateCount";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _toggleParticleCounter = new StringName("toggle_particle_counter");

	private const float _secondsPerUpdate = 5f;

	private TextureRect _icon;

	private Label? _label;

	private double _secondsSinceLastUpdate;

	private int _totalParticles;

	private int _updateCount;

	public override void _Ready()
	{
		if (!OS.HasFeature("editor"))
		{
			this.QueueFreeSafely();
			return;
		}
		_icon = GetNode<TextureRect>("%Icon");
		_label = GetNode<Label>("%Label");
		base.Visible = false;
	}

	public override void _Input(InputEvent inputEvent)
	{
		CheckForHotkey(inputEvent);
	}

	private void CheckForHotkey(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(_toggleParticleCounter) && !NDevConsole.Instance.Visible)
		{
			base.Visible = !base.Visible;
		}
	}

	public override void _Process(double delta)
	{
		if (!base.Visible || _label == null)
		{
			return;
		}
		_secondsSinceLastUpdate += delta;
		if ((_totalParticles > 1 || _updateCount > 500) && _secondsSinceLastUpdate < 5.0)
		{
			return;
		}
		_updateCount++;
		_secondsSinceLastUpdate = 0.0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (Node item in GetChildrenRecursive(GetTree().Root))
		{
			if (!(item is CpuParticles2D cpuParticles2D))
			{
				if (item is GpuParticles2D gpuParticles2D)
				{
					num3 += gpuParticles2D.Amount;
					num4++;
				}
			}
			else
			{
				num += cpuParticles2D.Amount;
				num2++;
			}
		}
		_totalParticles = num + num3;
		_label.Text = $"All particles: {_totalParticles}\nCPU particles: {num} in {num2} node{((num2 == 1) ? "" : "s")}\nGPU particles: {num3} in {num4} node{((num4 == 1) ? "" : "s")}";
	}

	private static List<Node> GetChildrenRecursive(Node root)
	{
		int num = 1;
		List<Node> list = new List<Node>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Node> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = root;
		List<Node> list2 = list;
		foreach (Node child in root.GetChildren())
		{
			list2.AddRange(GetChildrenRecursive(child));
		}
		return list2;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckForHotkey, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.CheckForHotkey && args.Count == 1)
		{
			CheckForHotkey(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
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
		if (method == MethodName.CheckForHotkey)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._secondsSinceLastUpdate)
		{
			_secondsSinceLastUpdate = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._totalParticles)
		{
			_totalParticles = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._updateCount)
		{
			_updateCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._secondsSinceLastUpdate)
		{
			value = VariantUtils.CreateFrom(in _secondsSinceLastUpdate);
			return true;
		}
		if (name == PropertyName._totalParticles)
		{
			value = VariantUtils.CreateFrom(in _totalParticles);
			return true;
		}
		if (name == PropertyName._updateCount)
		{
			value = VariantUtils.CreateFrom(in _updateCount);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._secondsSinceLastUpdate, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._totalParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._updateCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._secondsSinceLastUpdate, Variant.From(in _secondsSinceLastUpdate));
		info.AddProperty(PropertyName._totalParticles, Variant.From(in _totalParticles));
		info.AddProperty(PropertyName._updateCount, Variant.From(in _updateCount));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._secondsSinceLastUpdate, out var value3))
		{
			_secondsSinceLastUpdate = value3.As<double>();
		}
		if (info.TryGetProperty(PropertyName._totalParticles, out var value4))
		{
			_totalParticles = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._updateCount, out var value5))
		{
			_updateCount = value5.As<int>();
		}
	}
}
