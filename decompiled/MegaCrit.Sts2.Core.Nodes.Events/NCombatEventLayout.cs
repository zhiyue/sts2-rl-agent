using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NCombatEventLayout.cs")]
public class NCombatEventLayout : NEventLayout
{
	public new class MethodName : NEventLayout.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetCombatRoomNode = "SetCombatRoomNode";

		public new static readonly StringName InitializeVisuals = "InitializeVisuals";

		public static readonly StringName HideEventVisuals = "HideEventVisuals";
	}

	public new class PropertyName : NEventLayout.PropertyName
	{
		public static readonly StringName EmbeddedCombatRoom = "EmbeddedCombatRoom";

		public static readonly StringName HasCombatStarted = "HasCombatStarted";

		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _combatRoomContainer = "_combatRoomContainer";
	}

	public new class SignalName : NEventLayout.SignalName
	{
	}

	public const string combatScenePath = "res://scenes/events/combat_event_layout.tscn";

	private Control _combatRoomContainer;

	public NCombatRoom? EmbeddedCombatRoom { get; private set; }

	public bool HasCombatStarted { get; private set; }

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (!HasCombatStarted)
			{
				return base.DefaultFocusedControl;
			}
			return EmbeddedCombatRoom?.DefaultFocusedControl;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		_combatRoomContainer = GetNode<Control>("%CombatRoomContainer");
	}

	public void SetCombatRoomNode(NCombatRoom? combatRoomNode)
	{
		if (combatRoomNode != null)
		{
			if (EmbeddedCombatRoom != null)
			{
				throw new InvalidOperationException("Combat room node was already set.");
			}
			EmbeddedCombatRoom = combatRoomNode;
			_combatRoomContainer.AddChildSafely(combatRoomNode);
		}
	}

	public override void SetEvent(EventModel eventModel)
	{
		IRunState runState = eventModel.Owner.RunState;
		ICombatRoomVisuals visuals = eventModel.CreateCombatRoomVisuals(runState.Players, runState.Act);
		NCombatRoom nCombatRoom = NCombatRoom.Create(visuals, CombatRoomMode.VisualOnly);
		SetCombatRoomNode(nCombatRoom);
		nCombatRoom?.SetUpBackground(runState);
		base.SetEvent(eventModel);
	}

	protected override void InitializeVisuals()
	{
	}

	public void HideEventVisuals()
	{
		if (_description != null)
		{
			_description.Visible = false;
		}
		if (_sharedEventLabel != null)
		{
			_sharedEventLabel.Visible = false;
		}
		_optionsContainer.Visible = false;
		HasCombatStarted = true;
		DefaultFocusedControl?.TryGrabFocus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCombatRoomNode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "combatRoomNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InitializeVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideEventVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetCombatRoomNode && args.Count == 1)
		{
			SetCombatRoomNode(VariantUtils.ConvertTo<NCombatRoom>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeVisuals && args.Count == 0)
		{
			InitializeVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideEventVisuals && args.Count == 0)
		{
			HideEventVisuals();
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
		if (method == MethodName.SetCombatRoomNode)
		{
			return true;
		}
		if (method == MethodName.InitializeVisuals)
		{
			return true;
		}
		if (method == MethodName.HideEventVisuals)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.EmbeddedCombatRoom)
		{
			EmbeddedCombatRoom = VariantUtils.ConvertTo<NCombatRoom>(in value);
			return true;
		}
		if (name == PropertyName.HasCombatStarted)
		{
			HasCombatStarted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._combatRoomContainer)
		{
			_combatRoomContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.EmbeddedCombatRoom)
		{
			value = VariantUtils.CreateFrom<NCombatRoom>(EmbeddedCombatRoom);
			return true;
		}
		if (name == PropertyName.HasCombatStarted)
		{
			value = VariantUtils.CreateFrom<bool>(HasCombatStarted);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._combatRoomContainer)
		{
			value = VariantUtils.CreateFrom(in _combatRoomContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatRoomContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EmbeddedCombatRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasCombatStarted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.EmbeddedCombatRoom, Variant.From<NCombatRoom>(EmbeddedCombatRoom));
		info.AddProperty(PropertyName.HasCombatStarted, Variant.From<bool>(HasCombatStarted));
		info.AddProperty(PropertyName._combatRoomContainer, Variant.From(in _combatRoomContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.EmbeddedCombatRoom, out var value))
		{
			EmbeddedCombatRoom = value.As<NCombatRoom>();
		}
		if (info.TryGetProperty(PropertyName.HasCombatStarted, out var value2))
		{
			HasCombatStarted = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._combatRoomContainer, out var value3))
		{
			_combatRoomContainer = value3.As<Control>();
		}
	}
}
