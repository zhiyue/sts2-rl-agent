using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;

[ScriptPath("res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereItem.cs")]
public class NCrystalSphereItem : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _icon = "_icon";

		public static readonly StringName _material = "_material";

		public static readonly StringName _card = "_card";

		public static readonly StringName _cardFrame = "_cardFrame";

		public static readonly StringName _cardBanner = "_cardBanner";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public const string scenePath = "res://scenes/events/custom/crystal_sphere/crystal_sphere_item.tscn";

	private CrystalSphereItem _item;

	private TextureRect _icon;

	private Material _material;

	private Control _card;

	private TextureRect _cardFrame;

	private TextureRect _cardBanner;

	private Tween? _tween;

	public static NCrystalSphereItem? Create(CrystalSphereItem item)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCrystalSphereItem nCrystalSphereItem = PreloadManager.Cache.GetScene("res://scenes/events/custom/crystal_sphere/crystal_sphere_item.tscn").Instantiate<NCrystalSphereItem>(PackedScene.GenEditState.Disabled);
		nCrystalSphereItem._item = item;
		return nCrystalSphereItem;
	}

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("Icon");
		_card = GetNode<Control>("%Card");
		_cardFrame = GetNode<TextureRect>("%CardFrame");
		_cardBanner = GetNode<TextureRect>("%CardBanner");
		base.PivotOffset = base.Size / 2f;
		_icon.PivotOffset = base.Size / 2f;
		_card.PivotOffset = base.Size / 2f;
		_material = _icon.Material;
		if (_item is CrystalSphereCardReward crystalSphereCardReward)
		{
			_card.Visible = true;
			_icon.Visible = false;
			_cardBanner.Material = crystalSphereCardReward.BannerMaterial;
			_cardFrame.Material = crystalSphereCardReward.FrameMaterial;
		}
		else
		{
			_card.Visible = false;
			_icon.Visible = true;
			_icon.Texture = _item.Texture;
		}
	}

	public override void _EnterTree()
	{
		_item.Revealed += OnRevealed;
	}

	public override void _ExitTree()
	{
		_item.Revealed -= OnRevealed;
		if (_tween != null && _tween.IsRunning())
		{
			_tween.Kill();
		}
	}

	private void OnRevealed(CrystalSphereItem item)
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenInterval(0.25);
		_tween.Chain().TweenProperty(_material, "shader_parameter/val", 1f, 0.5).From(0);
		_tween.Parallel().TweenProperty(_icon, "scale", Vector2.One * 1.2f, 0.15000000596046448);
		_tween.Parallel().TweenProperty(_icon, "scale", Vector2.One, 0.5).SetDelay(0.15000000596046448);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (name == PropertyName._material)
		{
			_material = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._card)
		{
			_card = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cardFrame)
		{
			_cardFrame = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._cardBanner)
		{
			_cardBanner = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._material)
		{
			value = VariantUtils.CreateFrom(in _material);
			return true;
		}
		if (name == PropertyName._card)
		{
			value = VariantUtils.CreateFrom(in _card);
			return true;
		}
		if (name == PropertyName._cardFrame)
		{
			value = VariantUtils.CreateFrom(in _cardFrame);
			return true;
		}
		if (name == PropertyName._cardBanner)
		{
			value = VariantUtils.CreateFrom(in _cardBanner);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._material, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._card, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardBanner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._material, Variant.From(in _material));
		info.AddProperty(PropertyName._card, Variant.From(in _card));
		info.AddProperty(PropertyName._cardFrame, Variant.From(in _cardFrame));
		info.AddProperty(PropertyName._cardBanner, Variant.From(in _cardBanner));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._material, out var value2))
		{
			_material = value2.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._card, out var value3))
		{
			_card = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cardFrame, out var value4))
		{
			_cardFrame = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._cardBanner, out var value5))
		{
			_cardBanner = value5.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
