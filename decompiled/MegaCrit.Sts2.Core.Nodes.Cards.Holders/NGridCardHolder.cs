using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Pooling;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Cards.Holders;

[ScriptPath("res://src/Core/Nodes/Cards/Holders/NGridCardHolder.cs")]
public class NGridCardHolder : NCardHolder, IPoolable
{
	public new class MethodName : NCardHolder.MethodName
	{
		public static readonly StringName InitPool = "InitPool";

		public static readonly StringName Create = "Create";

		public static readonly StringName UpdateCardModel = "UpdateCardModel";

		public static readonly StringName OnInstantiated = "OnInstantiated";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName EnsureCardLibraryStatsExists = "EnsureCardLibraryStatsExists";

		public new static readonly StringName OnCardReassigned = "OnCardReassigned";

		public new static readonly StringName SetCard = "SetCard";

		public static readonly StringName UpdateName = "UpdateName";

		public new static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName SetIsPreviewingUpgrade = "SetIsPreviewingUpgrade";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnReturnedFromPool = "OnReturnedFromPool";

		public static readonly StringName OnFreedToPool = "OnFreedToPool";
	}

	public new class PropertyName : NCardHolder.PropertyName
	{
		public static readonly StringName CardLibraryStats = "CardLibraryStats";

		public new static readonly StringName IsShowingUpgradedCard = "IsShowingUpgradedCard";

		public static readonly StringName _isPreviewingUpgrade = "_isPreviewingUpgrade";
	}

	public new class SignalName : NCardHolder.SignalName
	{
	}

	private CardModel _baseCard;

	private CardModel? _upgradedCard;

	private bool _isPreviewingUpgrade;

	private static string ScenePath => SceneHelper.GetScenePath("cards/holders/grid_card_holder");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NCardLibraryStats? CardLibraryStats { get; private set; }

	public override CardModel CardModel => _baseCard;

	public override bool IsShowingUpgradedCard
	{
		get
		{
			if (!_isPreviewingUpgrade)
			{
				return base.IsShowingUpgradedCard;
			}
			return true;
		}
	}

	public static void InitPool()
	{
		NodePool.Init<NGridCardHolder>(ScenePath, 30);
	}

	public static NGridCardHolder? Create(NCard cardNode)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NGridCardHolder nGridCardHolder = NodePool.Get<NGridCardHolder>();
		nGridCardHolder.SetCard(cardNode);
		nGridCardHolder.UpdateCardModel();
		nGridCardHolder.UpdateName();
		nGridCardHolder.Scale = nGridCardHolder.SmallScale;
		return nGridCardHolder;
	}

	private void UpdateCardModel()
	{
		CardModel cardModel = (_baseCard = base.CardNode.Model);
		if (cardModel.IsUpgradable)
		{
			_upgradedCard = (CardModel)cardModel.MutableClone();
			_upgradedCard.UpgradeInternal();
			if (IsNodeReady())
			{
				bool isPreviewingUpgrade = _isPreviewingUpgrade;
				_isPreviewingUpgrade = false;
				SetIsPreviewingUpgrade(isPreviewingUpgrade);
			}
		}
	}

	public void OnInstantiated()
	{
	}

	public override void _Ready()
	{
		bool isPreviewingUpgrade = _isPreviewingUpgrade;
		_isPreviewingUpgrade = false;
		SetIsPreviewingUpgrade(isPreviewingUpgrade);
		ConnectSignals();
	}

	public void EnsureCardLibraryStatsExists()
	{
		if (CardLibraryStats == null)
		{
			CardLibraryStats = NCardLibraryStats.Create();
			this.AddChildSafely(CardLibraryStats);
		}
	}

	protected override void OnCardReassigned()
	{
		UpdateCardModel();
		UpdateName();
	}

	protected override void SetCard(NCard node)
	{
		base.SetCard(node);
		if (CardLibraryStats != null)
		{
			MoveChild(CardLibraryStats, GetChildCount() - 1);
		}
	}

	private void UpdateName()
	{
		base.Name = $"GridCardHolder-{base.CardNode.Model.Id}";
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		MoveToFront();
	}

	public void SetIsPreviewingUpgrade(bool showUpgradePreview)
	{
		if (!base.Visible)
		{
			return;
		}
		if (!_baseCard.IsUpgradable && showUpgradePreview)
		{
			throw new InvalidExpressionException($"{_baseCard.Id} is not upgradable.");
		}
		if (_isPreviewingUpgrade != showUpgradePreview)
		{
			if (showUpgradePreview && _upgradedCard != null)
			{
				base.CardNode.Model = _upgradedCard;
				base.CardNode.ShowUpgradePreview();
			}
			else
			{
				base.CardNode.Model = _baseCard;
				base.CardNode.UpdateVisuals(base.CardNode.DisplayingPile, CardPreviewMode.Normal);
			}
			_isPreviewingUpgrade = showUpgradePreview;
		}
	}

	public override void _ExitTree()
	{
		if (IsAncestorOf(base.CardNode))
		{
			base.CardNode?.QueueFreeSafely();
		}
		base.CardNode = null;
	}

	public void OnReturnedFromPool()
	{
		if (IsNodeReady())
		{
			base.Position = Vector2.Zero;
			base.Rotation = 0f;
			base.Scale = Vector2.One;
			base.Modulate = Colors.White;
			base.Visible = true;
			SetClickable(isClickable: true);
			base.Hitbox.MouseDefaultCursorShape = CursorShape.Arrow;
			_isPreviewingUpgrade = false;
			if (CardLibraryStats != null)
			{
				CardLibraryStats.Visible = false;
				CardLibraryStats.Modulate = Colors.White;
			}
		}
	}

	public void OnFreedToPool()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName.InitPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCardModel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnInstantiated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnsureCardLibraryStatsExists, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardReassigned, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateName, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsPreviewingUpgrade, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "showUpgradePreview", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnReturnedFromPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFreedToPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.InitPool && args.Count == 0)
		{
			InitPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NGridCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0])));
			return true;
		}
		if (method == MethodName.UpdateCardModel && args.Count == 0)
		{
			UpdateCardModel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInstantiated && args.Count == 0)
		{
			OnInstantiated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnsureCardLibraryStatsExists && args.Count == 0)
		{
			EnsureCardLibraryStatsExists();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardReassigned && args.Count == 0)
		{
			OnCardReassigned();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCard && args.Count == 1)
		{
			SetCard(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateName && args.Count == 0)
		{
			UpdateName();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIsPreviewingUpgrade && args.Count == 1)
		{
			SetIsPreviewingUpgrade(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReturnedFromPool && args.Count == 0)
		{
			OnReturnedFromPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFreedToPool && args.Count == 0)
		{
			OnFreedToPool();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.InitPool && args.Count == 0)
		{
			InitPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NGridCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.InitPool)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.UpdateCardModel)
		{
			return true;
		}
		if (method == MethodName.OnInstantiated)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.EnsureCardLibraryStatsExists)
		{
			return true;
		}
		if (method == MethodName.OnCardReassigned)
		{
			return true;
		}
		if (method == MethodName.SetCard)
		{
			return true;
		}
		if (method == MethodName.UpdateName)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.SetIsPreviewingUpgrade)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnReturnedFromPool)
		{
			return true;
		}
		if (method == MethodName.OnFreedToPool)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CardLibraryStats)
		{
			CardLibraryStats = VariantUtils.ConvertTo<NCardLibraryStats>(in value);
			return true;
		}
		if (name == PropertyName._isPreviewingUpgrade)
		{
			_isPreviewingUpgrade = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CardLibraryStats)
		{
			value = VariantUtils.CreateFrom<NCardLibraryStats>(CardLibraryStats);
			return true;
		}
		if (name == PropertyName.IsShowingUpgradedCard)
		{
			value = VariantUtils.CreateFrom<bool>(IsShowingUpgradedCard);
			return true;
		}
		if (name == PropertyName._isPreviewingUpgrade)
		{
			value = VariantUtils.CreateFrom(in _isPreviewingUpgrade);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardLibraryStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPreviewingUpgrade, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgradedCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CardLibraryStats, Variant.From<NCardLibraryStats>(CardLibraryStats));
		info.AddProperty(PropertyName._isPreviewingUpgrade, Variant.From(in _isPreviewingUpgrade));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CardLibraryStats, out var value))
		{
			CardLibraryStats = value.As<NCardLibraryStats>();
		}
		if (info.TryGetProperty(PropertyName._isPreviewingUpgrade, out var value2))
		{
			_isPreviewingUpgrade = value2.As<bool>();
		}
	}
}
