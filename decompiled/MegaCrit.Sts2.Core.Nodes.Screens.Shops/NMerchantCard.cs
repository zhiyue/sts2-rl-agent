using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantCard.cs")]
public class NMerchantCard : NMerchantSlot
{
	public new class MethodName : NMerchantSlot.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName UpdateVisual = "UpdateVisual";

		public static readonly StringName OnInventoryOpened = "OnInventoryOpened";

		public new static readonly StringName OnPreview = "OnPreview";

		public new static readonly StringName CreateHoverTip = "CreateHoverTip";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NMerchantSlot.PropertyName
	{
		public static readonly StringName IsShowingUpgradedCard = "IsShowingUpgradedCard";

		public new static readonly StringName Visual = "Visual";

		public static readonly StringName _saleVisual = "_saleVisual";

		public static readonly StringName _cardHolder = "_cardHolder";

		public static readonly StringName _cardNode = "_cardNode";

		public new static readonly StringName _hoverTween = "_hoverTween";
	}

	public new class SignalName : NMerchantSlot.SignalName
	{
	}

	private Node2D _saleVisual;

	private Control _cardHolder;

	private NCard? _cardNode;

	private Tween? _hoverTween;

	private MerchantCardEntry _cardEntry;

	public bool IsShowingUpgradedCard => _cardNode?.Model?.IsUpgraded == true;

	public override MerchantEntry Entry => _cardEntry;

	protected override CanvasItem Visual => _cardHolder;

	public override void _Ready()
	{
		ConnectSignals();
		_cardHolder = GetNode<Control>("%CardHolder");
		_saleVisual = GetNode<Node2D>("%SaleVisual");
	}

	public void FillSlot(MerchantCardEntry cardEntry)
	{
		_cardEntry = cardEntry;
		cardEntry.EntryUpdated += UpdateVisual;
		cardEntry.PurchaseFailed += base.OnPurchaseFailed;
		cardEntry.PurchaseCompleted += OnSuccessfulPurchase;
		UpdateVisual();
	}

	protected override void UpdateVisual()
	{
		base.UpdateVisual();
		if (_cardEntry.CreationResult == null)
		{
			base.Visible = false;
			base.MouseFilter = MouseFilterEnum.Ignore;
			ClearHoverTip();
			return;
		}
		if (_cardNode != null && _cardNode.Model != _cardEntry.CreationResult.Card)
		{
			_cardNode.QueueFreeSafely();
			_cardNode = null;
		}
		if (_cardNode == null)
		{
			_cardNode = NCard.Create(_cardEntry.CreationResult.Card);
			_cardHolder.AddChildSafely(_cardNode);
			_cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		}
		_costLabel.SetTextAutoSize(_cardEntry.Cost.ToString());
		_saleVisual.Visible = _cardEntry.IsOnSale;
		if (!_cardEntry.EnoughGold)
		{
			_costLabel.Modulate = StsColors.red;
		}
		else
		{
			_costLabel.Modulate = (_cardEntry.IsOnSale ? StsColors.green : StsColors.cream);
		}
	}

	public void OnInventoryOpened()
	{
		CardCreationResult? creationResult = _cardEntry.CreationResult;
		if (creationResult != null && creationResult.HasBeenModified)
		{
			TaskHelper.RunSafely(DoRelicFlash());
		}
	}

	private async Task DoRelicFlash()
	{
		SceneTreeTimer sceneTreeTimer = GetTree().CreateTimer(0.4);
		await sceneTreeTimer.ToSignal(sceneTreeTimer, SceneTreeTimer.SignalName.Timeout);
		foreach (RelicModel modifyingRelic in _cardEntry.CreationResult.ModifyingRelics)
		{
			modifyingRelic.Flash();
			_cardNode?.FlashRelicOnCard(modifyingRelic);
		}
	}

	protected override async Task OnTryPurchase(MerchantInventory? inventory)
	{
		await _cardEntry.OnTryPurchaseWrapper(inventory);
	}

	protected void OnSuccessfulPurchase(PurchaseStatus _, MerchantEntry __)
	{
		TriggerMerchantHandToPointHere();
		NRun.Instance?.GlobalUi.ReparentCard(_cardNode);
		NRun.Instance?.GlobalUi.TopBar.TrailContainer.AddChildSafely(NCardFlyVfx.Create(_cardNode, PileType.Deck.GetTargetPosition(_cardNode), isAddingToPile: true, _cardNode.Model.Owner.Character.TrailPath));
		_cardNode = null;
		UpdateVisual();
	}

	protected override void OnPreview()
	{
		ClearHoverTip();
		NInspectCardScreen inspectCardScreen = NGame.Instance.GetInspectCardScreen();
		int num = 1;
		List<CardModel> list = new List<CardModel>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CardModel> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = _cardNode.Model;
		inspectCardScreen.Open(list, 0);
	}

	protected override void CreateHoverTip()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _cardEntry.CreationResult.Card.HoverTips);
		nHoverTipSet.SetAlignment(_hitbox, HoverTip.GetHoverTipAlignment(this));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_cardNode?.QueueFreeSafely();
		_cardEntry.EntryUpdated -= UpdateVisual;
		_cardEntry.PurchaseFailed -= base.OnPurchaseFailed;
		_cardEntry.PurchaseCompleted -= OnSuccessfulPurchase;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisual, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnInventoryOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.UpdateVisual && args.Count == 0)
		{
			UpdateVisual();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInventoryOpened && args.Count == 0)
		{
			OnInventoryOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPreview && args.Count == 0)
		{
			OnPreview();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateHoverTip && args.Count == 0)
		{
			CreateHoverTip();
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
		if (method == MethodName.UpdateVisual)
		{
			return true;
		}
		if (method == MethodName.OnInventoryOpened)
		{
			return true;
		}
		if (method == MethodName.OnPreview)
		{
			return true;
		}
		if (method == MethodName.CreateHoverTip)
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
		if (name == PropertyName._saleVisual)
		{
			_saleVisual = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._cardHolder)
		{
			_cardHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
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
		if (name == PropertyName.IsShowingUpgradedCard)
		{
			value = VariantUtils.CreateFrom<bool>(IsShowingUpgradedCard);
			return true;
		}
		if (name == PropertyName.Visual)
		{
			value = VariantUtils.CreateFrom<CanvasItem>(Visual);
			return true;
		}
		if (name == PropertyName._saleVisual)
		{
			value = VariantUtils.CreateFrom(in _saleVisual);
			return true;
		}
		if (name == PropertyName._cardHolder)
		{
			value = VariantUtils.CreateFrom(in _cardHolder);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._saleVisual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgradedCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visual, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._saleVisual, Variant.From(in _saleVisual));
		info.AddProperty(PropertyName._cardHolder, Variant.From(in _cardHolder));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._saleVisual, out var value))
		{
			_saleVisual = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._cardHolder, out var value2))
		{
			_cardHolder = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value3))
		{
			_cardNode = value3.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value4))
		{
			_hoverTween = value4.As<Tween>();
		}
	}
}
