using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;

[ScriptPath("res://src/Core/Nodes/Screens/RelicCollection/NRelicCollectionEntry.cs")]
public class NRelicCollectionEntry : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName ModelVisibility = "ModelVisibility";

		public static readonly StringName _relicHolder = "_relicHolder";

		public static readonly StringName _relicNode = "_relicNode";

		public static readonly StringName _hoverTween = "_hoverTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("screens/relic_collection/relic_collection_entry");

	public static readonly string lockedIconPath = ImageHelper.GetImagePath("packed/common_ui/locked_model.png");

	public RelicModel relic;

	private Control _relicHolder;

	private Control _relicNode;

	private Tween? _hoverTween;

	private static LocString UnknownHoverTipTitle => new LocString("main_menu_ui", "COMPENDIUM_RELIC_COLLECTION.unknown.title");

	private static LocString UnknownHoverTipDescription => new LocString("main_menu_ui", "COMPENDIUM_RELIC_COLLECTION.unknown.description");

	private static HoverTip UnknownHoverTip => new HoverTip(UnknownHoverTipTitle, UnknownHoverTipDescription);

	private static LocString LockedHoverTipTitle => new LocString("main_menu_ui", "COMPENDIUM_RELIC_COLLECTION.locked.title");

	private static LocString LockedHoverTipDescription => new LocString("main_menu_ui", "COMPENDIUM_RELIC_COLLECTION.locked.description");

	private static HoverTip LockedHoverTip => new HoverTip(LockedHoverTipTitle, LockedHoverTipDescription);

	public ModelVisibility ModelVisibility { get; set; }

	public static NRelicCollectionEntry Create(RelicModel relic, ModelVisibility visibility)
	{
		NRelicCollectionEntry nRelicCollectionEntry = PreloadManager.Cache.GetScene(scenePath).Instantiate<NRelicCollectionEntry>(PackedScene.GenEditState.Disabled);
		nRelicCollectionEntry.relic = relic;
		nRelicCollectionEntry.ModelVisibility = visibility;
		return nRelicCollectionEntry;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_relicHolder = GetNode<Control>("RelicHolder");
		if (ModelVisibility == ModelVisibility.Locked)
		{
			TextureRect textureRect = new TextureRect();
			textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
			textureRect.Texture = PreloadManager.Cache.GetTexture2D(lockedIconPath);
			textureRect.Size = Vector2.One * 68f;
			textureRect.PivotOffset = textureRect.Size * 0.5f;
			textureRect.Modulate = StsColors.gray;
			_relicHolder.AddChildSafely(textureRect);
			_relicNode = textureRect;
		}
		else
		{
			NRelic nRelic = NRelic.Create(relic.ToMutable(), NRelic.IconSize.Small);
			_relicHolder.AddChildSafely(nRelic);
			if (ModelVisibility == ModelVisibility.NotSeen)
			{
				nRelic.Icon.SelfModulate = StsColors.ninetyPercentBlack;
				nRelic.Outline.SelfModulate = StsColors.halfTransparentWhite;
			}
			else
			{
				foreach (RelicPoolModel allCharacterRelicPool in ModelDb.AllCharacterRelicPools)
				{
					if (allCharacterRelicPool.AllRelicIds.Contains(relic.Id))
					{
						TextureRect outline = nRelic.Outline;
						Color labOutlineColor = allCharacterRelicPool.LabOutlineColor;
						labOutlineColor.A = 0.66f;
						outline.SelfModulate = labOutlineColor;
						break;
					}
				}
			}
			_relicNode = nRelic;
		}
		_relicNode.MouseFilter = MouseFilterEnum.Ignore;
		_relicNode.FocusMode = FocusModeEnum.None;
	}

	protected override void OnRelease()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relicNode, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	protected override void OnFocus()
	{
		_hoverTween?.Kill();
		_relicNode.Scale = Vector2.One * 1.25f;
		ModelVisibility modelVisibility = ModelVisibility;
		IEnumerable<IHoverTip> enumerable = default(IEnumerable<IHoverTip>);
		switch (modelVisibility)
		{
		case ModelVisibility.None:
			throw new ArgumentOutOfRangeException();
		case ModelVisibility.Visible:
			enumerable = relic.HoverTips;
			break;
		case ModelVisibility.NotSeen:
			enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(UnknownHoverTip);
			break;
		case ModelVisibility.Locked:
			enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(LockedHoverTip);
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(modelVisibility);
			break;
		}
		IEnumerable<IHoverTip> hoverTips = enumerable;
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTips, HoverTip.GetHoverTipAlignment(this));
		nHoverTipSet.SetFollowOwner();
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relicNode, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relicNode, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
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
		if (method == MethodName.OnRelease)
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
		if (method == MethodName.OnPress)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ModelVisibility)
		{
			ModelVisibility = VariantUtils.ConvertTo<ModelVisibility>(in value);
			return true;
		}
		if (name == PropertyName._relicHolder)
		{
			_relicHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._relicNode)
		{
			_relicNode = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName.ModelVisibility)
		{
			value = VariantUtils.CreateFrom<ModelVisibility>(ModelVisibility);
			return true;
		}
		if (name == PropertyName._relicHolder)
		{
			value = VariantUtils.CreateFrom(in _relicHolder);
			return true;
		}
		if (name == PropertyName._relicNode)
		{
			value = VariantUtils.CreateFrom(in _relicNode);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ModelVisibility, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ModelVisibility, Variant.From<ModelVisibility>(ModelVisibility));
		info.AddProperty(PropertyName._relicHolder, Variant.From(in _relicHolder));
		info.AddProperty(PropertyName._relicNode, Variant.From(in _relicNode));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ModelVisibility, out var value))
		{
			ModelVisibility = value.As<ModelVisibility>();
		}
		if (info.TryGetProperty(PropertyName._relicHolder, out var value2))
		{
			_relicHolder = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._relicNode, out var value3))
		{
			_relicNode = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value4))
		{
			_hoverTween = value4.As<Tween>();
		}
	}
}
