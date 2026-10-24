using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

/// <summary>
/// Badge visual which appears on the game over screen.
/// Can be hovered to view its name and description.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/GameOverScreen/NBadge.cs")]
public class NBadge : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'GetRarityPrefix' method.
		/// </summary>
		public static readonly StringName GetRarityPrefix = "GetRarityPrefix";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'GetBadgeBaseTexture' method.
		/// </summary>
		public static readonly StringName GetBadgeBaseTexture = "GetBadgeBaseTexture";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_hoverNode' field.
		/// </summary>
		public static readonly StringName _hoverNode = "_hoverNode";

		/// <summary>
		/// Cached name for the '_selectionReticle' field.
		/// </summary>
		public static readonly StringName _selectionReticle = "_selectionReticle";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/game_over_screen/badge");

	private Tween? _tween;

	private HoverTip _hoverTip;

	private LocString _title;

	private LocString _description;

	private const string _table = "badges";

	private Control _hoverNode;

	private NSelectionReticle _selectionReticle;

	/// <summary>
	/// This creates an NBadge for the game over screen.
	/// </summary>
	public static NBadge? Create(Badge badgeModel)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBadge nBadge = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBadge>(PackedScene.GenEditState.Disabled);
		nBadge.GetNode<TextureRect>("%BadgeHolder").Texture = badgeModel.BadgeBase;
		nBadge.GetNode<TextureRect>("%Icon").Texture = badgeModel.BadgeIcon;
		if (LocString.Exists("badges", badgeModel.Id + "." + GetRarityPrefix(badgeModel.Rarity) + "Title"))
		{
			nBadge._title = new LocString("badges", badgeModel.Id + "." + GetRarityPrefix(badgeModel.Rarity) + "Title");
			nBadge._description = new LocString("badges", badgeModel.Id + "." + GetRarityPrefix(badgeModel.Rarity) + "Description");
		}
		else
		{
			nBadge._title = new LocString("badges", badgeModel.Id + ".title");
			nBadge._description = new LocString("badges", badgeModel.Id + ".description");
		}
		return nBadge;
	}

	/// <summary>
	/// This creates an NBadge for the run history screen :).
	/// TODO: And the statistics screen later!
	/// </summary>
	public static NBadge? Create(string id, BadgeRarity rarity)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBadge nBadge = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBadge>(PackedScene.GenEditState.Disabled);
		nBadge.GetNode<TextureRect>("%BadgeHolder").Texture = GetBadgeBaseTexture(rarity);
		nBadge.GetNode<TextureRect>("%Icon").Texture = PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_" + id.ToLowerInvariant() + ".png"));
		if (LocString.Exists("badges", id + "." + GetRarityPrefix(rarity) + "Title"))
		{
			nBadge._title = new LocString("badges", id + "." + GetRarityPrefix(rarity) + "Title");
			nBadge._description = new LocString("badges", id + "." + GetRarityPrefix(rarity) + "Description");
		}
		else
		{
			nBadge._title = new LocString("badges", id + ".title");
			nBadge._description = new LocString("badges", id + ".description");
		}
		return nBadge;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_hoverNode = this;
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_hoverTip = new HoverTip(_title, _description);
	}

	/// <summary>
	/// Maps badge rarity enums to prefix used for LocStrings
	/// </summary>
	private static string GetRarityPrefix(BadgeRarity rarity)
	{
		return rarity switch
		{
			BadgeRarity.Bronze => "bronze", 
			BadgeRarity.Silver => "silver", 
			BadgeRarity.Gold => "gold", 
			_ => "ERROR", 
		};
	}

	/// <summary>
	/// 0.4 second animation of a Badge animating in.
	/// Deferred due to how GridContainer lays out its elements after AddChildSafely().
	/// </summary>
	public async Task AnimateIn()
	{
		if (this.IsValid())
		{
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "modulate:a", 1f, 0.25);
			_tween.TweenProperty(this, "position:y", base.Position.Y, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring)
				.From(base.Position.Y + 40f);
			await _tween.AwaitFinished(this);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_tween?.Kill();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		NHoverTipSet.CreateAndShow(_hoverNode, _hoverTip)?.SetGlobalPosition(_hoverNode.GlobalPosition + new Vector2(10f, -132f));
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		NHoverTipSet.Remove(_hoverNode);
		_selectionReticle.OnDeselect();
	}

	private static Texture2D GetBadgeBaseTexture(BadgeRarity rarity)
	{
		return rarity switch
		{
			BadgeRarity.Bronze => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_bronze.png")), 
			BadgeRarity.Silver => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_silver.png")), 
			BadgeRarity.Gold => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_gold.png")), 
			_ => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("atlases/power_atlas.sprites/missing_power.tres")), 
		};
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetRarityPrefix, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetBadgeBaseTexture, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NBadge>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<BadgeRarity>(in args[1])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRarityPrefix && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRarityPrefix(VariantUtils.ConvertTo<BadgeRarity>(in args[0])));
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.GetBadgeBaseTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetBadgeBaseTexture(VariantUtils.ConvertTo<BadgeRarity>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NBadge>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<BadgeRarity>(in args[1])));
			return true;
		}
		if (method == MethodName.GetRarityPrefix && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetRarityPrefix(VariantUtils.ConvertTo<BadgeRarity>(in args[0])));
			return true;
		}
		if (method == MethodName.GetBadgeBaseTexture && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetBadgeBaseTexture(VariantUtils.ConvertTo<BadgeRarity>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.GetRarityPrefix)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
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
		if (method == MethodName.GetBadgeBaseTexture)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverNode)
		{
			_hoverNode = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._hoverNode)
		{
			value = VariantUtils.CreateFrom(in _hoverNode);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._hoverNode, Variant.From(in _hoverNode));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverNode, out var value2))
		{
			_hoverNode = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value3))
		{
			_selectionReticle = value3.As<NSelectionReticle>();
		}
	}
}
