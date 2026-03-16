using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;

[ScriptPath("res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereCell.cs")]
public class NCrystalSphereCell : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnEntityHighlightUpdated = "OnEntityHighlightUpdated";

		public static readonly StringName EntityClicked = "EntityClicked";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _mask = "_mask";

		public static readonly StringName _hoveredFg = "_hoveredFg";

		public static readonly StringName _fadeTween = "_fadeTween";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private const string _scenePath = "res://scenes/events/custom/crystal_sphere/crystal_sphere_cell.tscn";

	private NCrystalSphereMask _mask;

	private Control _hoveredFg;

	private Tween? _fadeTween;

	public CrystalSphereCell Entity { get; private set; }

	public static NCrystalSphereCell? Create(CrystalSphereCell cell, NCrystalSphereMask mask)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCrystalSphereCell nCrystalSphereCell = PreloadManager.Cache.GetScene("res://scenes/events/custom/crystal_sphere/crystal_sphere_cell.tscn").Instantiate<NCrystalSphereCell>(PackedScene.GenEditState.Disabled);
		nCrystalSphereCell.Entity = cell;
		nCrystalSphereCell._mask = mask;
		return nCrystalSphereCell;
	}

	public override void _Ready()
	{
		_hoveredFg = GetNode<Control>("HoveredFg");
		base.Modulate = Colors.Transparent;
		base.MouseFilter = (MouseFilterEnum)(Entity.IsHidden ? 0 : 2);
		base.FocusMode = (FocusModeEnum)(Entity.IsHidden ? 2 : 0);
		_hoveredFg.Visible = false;
	}

	public override void _EnterTree()
	{
		Entity.HighlightUpdated += OnEntityHighlightUpdated;
		Entity.FogUpdated += EntityClicked;
	}

	public override void _ExitTree()
	{
		Entity.HighlightUpdated -= OnEntityHighlightUpdated;
		Entity.FogUpdated -= EntityClicked;
	}

	private void OnEntityHighlightUpdated()
	{
		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		Tween? fadeTween = _fadeTween;
		NodePath property = "modulate";
		CrystalSphereCell entity = Entity;
		fadeTween.TweenProperty(this, property, (entity != null && entity.IsHighlighted && entity.IsHidden) ? Colors.White : Colors.Transparent, 0.15000000596046448);
		_hoveredFg.Visible = Entity.IsHovered;
	}

	private void EntityClicked()
	{
		base.MouseFilter = (MouseFilterEnum)(Entity.IsHidden ? 0 : 2);
		base.FocusMode = (FocusModeEnum)(Entity.IsHidden ? 2 : 0);
		_mask.UpdateMat(Entity);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEntityHighlightUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EntityClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEntityHighlightUpdated && args.Count == 0)
		{
			OnEntityHighlightUpdated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EntityClicked && args.Count == 0)
		{
			EntityClicked();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnEntityHighlightUpdated)
		{
			return true;
		}
		if (method == MethodName.EntityClicked)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._mask)
		{
			_mask = VariantUtils.ConvertTo<NCrystalSphereMask>(in value);
			return true;
		}
		if (name == PropertyName._hoveredFg)
		{
			_hoveredFg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			_fadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._mask)
		{
			value = VariantUtils.CreateFrom(in _mask);
			return true;
		}
		if (name == PropertyName._hoveredFg)
		{
			value = VariantUtils.CreateFrom(in _hoveredFg);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			value = VariantUtils.CreateFrom(in _fadeTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoveredFg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._mask, Variant.From(in _mask));
		info.AddProperty(PropertyName._hoveredFg, Variant.From(in _hoveredFg));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._mask, out var value))
		{
			_mask = value.As<NCrystalSphereMask>();
		}
		if (info.TryGetProperty(PropertyName._hoveredFg, out var value2))
		{
			_hoveredFg = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value3))
		{
			_fadeTween = value3.As<Tween>();
		}
	}
}
