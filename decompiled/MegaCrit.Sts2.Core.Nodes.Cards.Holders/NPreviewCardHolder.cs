using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Cards.Holders;

[ScriptPath("res://src/Core/Nodes/Cards/Holders/NPreviewCardHolder.cs")]
public class NPreviewCardHolder : NCardHolder
{
	public new class MethodName : NCardHolder.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Initialize = "Initialize";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName SetCardScale = "SetCardScale";

		public new static readonly StringName CreateHoverTips = "CreateHoverTips";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NCardHolder.PropertyName
	{
		public new static readonly StringName HoverScale = "HoverScale";

		public new static readonly StringName SmallScale = "SmallScale";

		public new static readonly StringName IsShowingUpgradedCard = "IsShowingUpgradedCard";

		public static readonly StringName _showHoverTips = "_showHoverTips";

		public static readonly StringName _scaleOnHover = "_scaleOnHover";

		public static readonly StringName _originalScale = "_originalScale";
	}

	public new class SignalName : NCardHolder.SignalName
	{
	}

	private bool _showHoverTips;

	private bool _scaleOnHover;

	private Vector2 _originalScale = Vector2.One;

	protected override Vector2 HoverScale => _originalScale * 1.1f;

	public override Vector2 SmallScale => _originalScale;

	public override bool IsShowingUpgradedCard
	{
		get
		{
			if (!base.IsShowingUpgradedCard)
			{
				if (CardModel != null)
				{
					return CardModel.UpgradePreviewType.IsPreview();
				}
				return false;
			}
			return true;
		}
	}

	private static string ScenePath => SceneHelper.GetScenePath("cards/holders/preview_card_holder");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NPreviewCardHolder? Create(NCard card, bool showHoverTips, bool scaleOnHover)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NPreviewCardHolder nPreviewCardHolder = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPreviewCardHolder>(PackedScene.GenEditState.Disabled);
		nPreviewCardHolder.Initialize(card, showHoverTips, scaleOnHover);
		return nPreviewCardHolder;
	}

	public override void _Ready()
	{
		ConnectSignals();
	}

	private void Initialize(NCard card, bool showHoverTips, bool scaleOnHover)
	{
		base.Name = $"UpgradePreviewCardHolder-{card.Model?.Id}";
		SetCard(card);
		base.Scale = SmallScale;
		_showHoverTips = showHoverTips;
		_scaleOnHover = scaleOnHover;
	}

	protected override void OnFocus()
	{
		_isHovered = true;
		if (_scaleOnHover)
		{
			_hoverTween?.Kill();
			base.Scale = HoverScale;
		}
		if (_showHoverTips)
		{
			CreateHoverTips();
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		if (_scaleOnHover)
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween();
			_hoverTween.TweenProperty(this, "scale", SmallScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		if (_showHoverTips)
		{
			ClearHoverTips();
		}
	}

	public void SetCardScale(Vector2 scale)
	{
		_originalScale = scale;
		base.Scale = _originalScale;
	}

	protected override void CreateHoverTips()
	{
		if (base.CardNode != null)
		{
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, base.CardNode.Model.HoverTips);
			nHoverTipSet.SetAlignmentForCardHolder(this);
		}
	}

	public override void _ExitTree()
	{
		if (base.CardNode != null)
		{
			if (IsAncestorOf(base.CardNode))
			{
				base.CardNode.QueueFreeSafely();
			}
			base.CardNode = null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "showHoverTips", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "scaleOnHover", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "showHoverTips", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "scaleOnHover", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCardScale, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NPreviewCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 3)
		{
			Initialize(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
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
		if (method == MethodName.SetCardScale && args.Count == 1)
		{
			SetCardScale(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateHoverTips && args.Count == 0)
		{
			CreateHoverTips();
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
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NPreviewCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

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
		if (method == MethodName.Initialize)
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
		if (method == MethodName.SetCardScale)
		{
			return true;
		}
		if (method == MethodName.CreateHoverTips)
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
		if (name == PropertyName._showHoverTips)
		{
			_showHoverTips = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._scaleOnHover)
		{
			_scaleOnHover = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._originalScale)
		{
			_originalScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Vector2 from;
		if (name == PropertyName.HoverScale)
		{
			from = HoverScale;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.SmallScale)
		{
			from = SmallScale;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsShowingUpgradedCard)
		{
			value = VariantUtils.CreateFrom<bool>(IsShowingUpgradedCard);
			return true;
		}
		if (name == PropertyName._showHoverTips)
		{
			value = VariantUtils.CreateFrom(in _showHoverTips);
			return true;
		}
		if (name == PropertyName._scaleOnHover)
		{
			value = VariantUtils.CreateFrom(in _scaleOnHover);
			return true;
		}
		if (name == PropertyName._originalScale)
		{
			value = VariantUtils.CreateFrom(in _originalScale);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.SmallScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._showHoverTips, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scaleOnHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgradedCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._showHoverTips, Variant.From(in _showHoverTips));
		info.AddProperty(PropertyName._scaleOnHover, Variant.From(in _scaleOnHover));
		info.AddProperty(PropertyName._originalScale, Variant.From(in _originalScale));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._showHoverTips, out var value))
		{
			_showHoverTips = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._scaleOnHover, out var value2))
		{
			_scaleOnHover = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._originalScale, out var value3))
		{
			_originalScale = value3.As<Vector2>();
		}
	}
}
