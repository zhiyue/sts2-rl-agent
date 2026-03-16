using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;

[ScriptPath("res://src/Core/Nodes/Screens/PotionLab/NLabPotionHolder.cs")]
public class NLabPotionHolder : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _potionNode = "_potionNode";

		public static readonly StringName _potionHolder = "_potionHolder";

		public static readonly StringName _visibility = "_visibility";

		public static readonly StringName _hoverTween = "_hoverTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("screens/potion_lab/lab_potion_holder");

	public static readonly string lockedIconPath = ImageHelper.GetImagePath("packed/common_ui/locked_model.png");

	private PotionModel _model;

	private NPotion _potionNode;

	private Control _potionHolder;

	private ModelVisibility _visibility;

	private Tween? _hoverTween;

	private LocString UnknownHoverTipTitle => new LocString("main_menu_ui", "POTION_LAB_COLLECTION.unknown.title");

	private LocString UnknownHoverTipDescription => new LocString("main_menu_ui", "POTION_LAB_COLLECTION.unknown.description");

	private HoverTip UnknownHoverTip => new HoverTip(UnknownHoverTipTitle, UnknownHoverTipDescription);

	private static LocString LockedHoverTipTitle => new LocString("main_menu_ui", "POTION_LAB_COLLECTION.locked.title");

	private static LocString LockedHoverTipDescription => new LocString("main_menu_ui", "POTION_LAB_COLLECTION.locked.description");

	private static HoverTip LockedHoverTip => new HoverTip(LockedHoverTipTitle, LockedHoverTipDescription);

	public static NLabPotionHolder Create(PotionModel potion, ModelVisibility visibility)
	{
		NLabPotionHolder nLabPotionHolder = PreloadManager.Cache.GetScene(scenePath).Instantiate<NLabPotionHolder>(PackedScene.GenEditState.Disabled);
		nLabPotionHolder._model = potion;
		nLabPotionHolder._visibility = visibility;
		return nLabPotionHolder;
	}

	public override void _Ready()
	{
		_potionHolder = GetNode<Control>("PotionHolder");
		_potionNode = NPotion.Create(_model);
		_potionHolder.AddChildSafely(_potionNode);
		if (_visibility == ModelVisibility.Locked)
		{
			_potionNode.Image.Texture = PreloadManager.Cache.GetTexture2D(lockedIconPath);
			_potionNode.Outline.Visible = false;
			_potionNode.Modulate = StsColors.gray;
		}
		else if (_visibility == ModelVisibility.NotSeen)
		{
			_potionNode.Image.SelfModulate = StsColors.ninetyPercentBlack;
			_potionNode.Outline.Modulate = StsColors.halfTransparentWhite;
		}
		else
		{
			foreach (PotionPoolModel allCharacterPotionPool in ModelDb.AllCharacterPotionPools)
			{
				PotionModel potionModel = allCharacterPotionPool.AllPotions.FirstOrDefault((PotionModel p) => p.Id == _model.Id);
				if (potionModel != null)
				{
					TextureRect outline = _potionNode.Outline;
					Color labOutlineColor = allCharacterPotionPool.LabOutlineColor;
					labOutlineColor.A = 0.66f;
					outline.Modulate = labOutlineColor;
					break;
				}
			}
		}
		_potionNode.MouseFilter = MouseFilterEnum.Ignore;
		_potionNode.PivotOffset = _potionNode.Size * 0.5f;
		_potionNode.Position = Vector2.Zero;
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
	}

	private void OnFocus()
	{
		_hoverTween?.Kill();
		NHoverTipSet.Remove(this);
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_potionNode, "scale", Vector2.One * 1.2f, 0.05);
		ModelVisibility visibility = _visibility;
		IEnumerable<IHoverTip> enumerable = default(IEnumerable<IHoverTip>);
		switch (visibility)
		{
		case ModelVisibility.None:
			throw new ArgumentOutOfRangeException();
		case ModelVisibility.Visible:
			enumerable = _potionNode.Model.HoverTips;
			break;
		case ModelVisibility.NotSeen:
			enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(UnknownHoverTip);
			break;
		case ModelVisibility.Locked:
			enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(LockedHoverTip);
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(visibility);
			break;
		}
		IEnumerable<IHoverTip> hoverTips = enumerable;
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTips, HoverTip.GetHoverTipAlignment(this));
		nHoverTipSet.SetFollowOwner();
		nHoverTipSet.SetExtraFollowOffset(new Vector2(32f, 0f));
	}

	private void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_potionNode, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._potionNode)
		{
			_potionNode = VariantUtils.ConvertTo<NPotion>(in value);
			return true;
		}
		if (name == PropertyName._potionHolder)
		{
			_potionHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._visibility)
		{
			_visibility = VariantUtils.ConvertTo<ModelVisibility>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._potionNode)
		{
			value = VariantUtils.CreateFrom(in _potionNode);
			return true;
		}
		if (name == PropertyName._potionHolder)
		{
			value = VariantUtils.CreateFrom(in _potionHolder);
			return true;
		}
		if (name == PropertyName._visibility)
		{
			value = VariantUtils.CreateFrom(in _visibility);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._visibility, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._potionNode, Variant.From(in _potionNode));
		info.AddProperty(PropertyName._potionHolder, Variant.From(in _potionHolder));
		info.AddProperty(PropertyName._visibility, Variant.From(in _visibility));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._potionNode, out var value))
		{
			_potionNode = value.As<NPotion>();
		}
		if (info.TryGetProperty(PropertyName._potionHolder, out var value2))
		{
			_potionHolder = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._visibility, out var value3))
		{
			_visibility = value3.As<ModelVisibility>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value4))
		{
			_hoverTween = value4.As<Tween>();
		}
	}
}
