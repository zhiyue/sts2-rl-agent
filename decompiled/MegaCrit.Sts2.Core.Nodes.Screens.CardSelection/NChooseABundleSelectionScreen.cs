using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NChooseABundleSelectionScreen.cs")]
public class NChooseABundleSelectionScreen : Control, IOverlayScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnBundleClicked = "OnBundleClicked";

		public static readonly StringName OpenPreviewScreen = "OpenPreviewScreen";

		public static readonly StringName CancelSelection = "CancelSelection";

		public static readonly StringName ConfirmSelection = "ConfirmSelection";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _bundleRow = "_bundleRow";

		public static readonly StringName _bundlePreviewContainer = "_bundlePreviewContainer";

		public static readonly StringName _bundlePreviewCards = "_bundlePreviewCards";

		public static readonly StringName _previewCancelButton = "_previewCancelButton";

		public static readonly StringName _previewConfirmButton = "_previewConfirmButton";

		public static readonly StringName _selectedBundle = "_selectedBundle";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _peekButton = "_peekButton";

		public static readonly StringName _fadeTween = "_fadeTween";

		public static readonly StringName _cardTween = "_cardTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _bundleRow;

	private IReadOnlyList<IReadOnlyList<CardModel>> _bundles;

	private Control _bundlePreviewContainer;

	private Control _bundlePreviewCards;

	private NBackButton _previewCancelButton;

	private NConfirmButton _previewConfirmButton;

	private NCardBundle? _selectedBundle;

	private NCommonBanner _banner;

	private readonly TaskCompletionSource<IEnumerable<IReadOnlyList<CardModel>>> _completionSource = new TaskCompletionSource<IEnumerable<IReadOnlyList<CardModel>>>();

	private NPeekButton _peekButton;

	private Tween? _fadeTween;

	private const float _cardXSpacing = 400f;

	private Tween? _cardTween;

	private static string ScenePath => SceneHelper.GetScenePath("/screens/card_selection/choose_a_bundle_selection_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NetScreenType ScreenType => NetScreenType.CardSelection;

	public bool UseSharedBackstop => true;

	public Control DefaultFocusedControl
	{
		get
		{
			if (!_bundlePreviewContainer.Visible)
			{
				return _bundleRow.GetChild<NCardBundle>(0).Hitbox;
			}
			return _bundlePreviewCards.GetChild<Control>(_bundlePreviewCards.GetChildCount() - 1);
		}
	}

	public override void _Ready()
	{
		_bundleRow = GetNode<Control>("%BundleRow");
		_bundlePreviewContainer = GetNode<Control>("%BundlePreviewContainer");
		_bundlePreviewCards = GetNode<Control>("%Cards");
		_previewCancelButton = GetNode<NBackButton>("%Cancel");
		_previewConfirmButton = GetNode<NConfirmButton>("%Confirm");
		_banner = GetNode<NCommonBanner>("Banner");
		_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_A_PACK").GetRawText());
		_banner.AnimateIn();
		_previewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_previewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		Vector2 vector = Vector2.Left * (_bundles.Count - 1) * 400f * 0.5f;
		for (int i = 0; i < _bundles.Count; i++)
		{
			NCardBundle nCardBundle = NCardBundle.Create(_bundles[i]);
			_bundleRow.AddChildSafely(nCardBundle);
			nCardBundle.Connect(NCardBundle.SignalName.Clicked, Callable.From<NCardBundle>(OnBundleClicked));
			nCardBundle.Scale = nCardBundle.smallScale;
			nCardBundle.Position += vector + Vector2.Right * 400f * i;
		}
		for (int j = 0; j < _bundleRow.GetChildCount(); j++)
		{
			NCardBundle child = _bundleRow.GetChild<NCardBundle>(j);
			child.Hitbox.FocusNeighborLeft = ((j > 0) ? _bundleRow.GetChild<NCardBundle>(j - 1).Hitbox.GetPath() : _bundleRow.GetChild<NCardBundle>(_bundleRow.GetChildCount() - 1).Hitbox.GetPath());
			child.Hitbox.FocusNeighborRight = ((j < _bundleRow.GetChildCount() - 1) ? _bundleRow.GetChild<NCardBundle>(j + 1).Hitbox.GetPath() : _bundleRow.GetChild<NCardBundle>(0).Hitbox.GetPath());
			child.Hitbox.FocusNeighborTop = child.Hitbox.GetPath();
			child.Hitbox.FocusNeighborBottom = child.Hitbox.GetPath();
		}
		_bundlePreviewContainer.Visible = false;
		_bundlePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_peekButton = GetNode<NPeekButton>("%PeekButton");
		_peekButton.AddTargets(_banner, _bundleRow, _bundlePreviewContainer);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.SetCanceled();
		}
	}

	public static NChooseABundleSelectionScreen ShowScreen(IReadOnlyList<IReadOnlyList<CardModel>> bundles)
	{
		NChooseABundleSelectionScreen nChooseABundleSelectionScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NChooseABundleSelectionScreen>(PackedScene.GenEditState.Disabled);
		nChooseABundleSelectionScreen.Name = "NChooseABundleSelectionScreen";
		nChooseABundleSelectionScreen._bundles = bundles;
		NOverlayStack.Instance.Push(nChooseABundleSelectionScreen);
		return nChooseABundleSelectionScreen;
	}

	private void OnBundleClicked(NCardBundle bundleNode)
	{
		_banner.AnimateOut();
		_selectedBundle = bundleNode;
		_bundlePreviewContainer.Visible = true;
		_bundlePreviewContainer.MouseFilter = MouseFilterEnum.Stop;
		_bundleRow.Visible = false;
		_previewCancelButton.Enable();
		_previewConfirmButton.Enable();
		Vector2 vector = Vector2.Right * (bundleNode.Bundle.Count - 1) * 400f * 0.5f;
		IReadOnlyList<NCard> readOnlyList = bundleNode.RemoveCardNodes();
		_cardTween?.Kill();
		_cardTween = CreateTween().SetParallel();
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			Vector2 globalPosition = readOnlyList[i].GlobalPosition;
			NPreviewCardHolder nPreviewCardHolder = NPreviewCardHolder.Create(readOnlyList[i], showHoverTips: true, scaleOnHover: true);
			_bundlePreviewCards.AddChildSafely(nPreviewCardHolder);
			nPreviewCardHolder.GlobalPosition = globalPosition;
			nPreviewCardHolder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(OpenPreviewScreen));
			readOnlyList[i].UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			_cardTween.TweenProperty(nPreviewCardHolder, "position", vector + Vector2.Left * 400f * i, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		for (int j = 0; j < _bundlePreviewCards.GetChildCount(); j++)
		{
			NPreviewCardHolder child = _bundlePreviewCards.GetChild<NPreviewCardHolder>(j);
			child.FocusNeighborLeft = ((j < _bundlePreviewCards.GetChildCount() - 1) ? _bundlePreviewCards.GetChild(j + 1).GetPath() : _bundlePreviewCards.GetChild(0).GetPath());
			child.FocusNeighborRight = ((j > 0) ? _bundlePreviewCards.GetChild(j - 1).GetPath() : _bundlePreviewCards.GetChild(_bundlePreviewCards.GetChildCount() - 1).GetPath());
			child.FocusNeighborTop = child.Hitbox.GetPath();
			child.FocusNeighborBottom = child.Hitbox.GetPath();
		}
		_bundlePreviewCards.GetChild<Control>(_bundlePreviewCards.GetChildCount() - 1).TryGrabFocus();
	}

	private void OpenPreviewScreen(NCardHolder cardHolder)
	{
		NInspectCardScreen inspectCardScreen = NGame.Instance.GetInspectCardScreen();
		int num = 1;
		List<CardModel> list = new List<CardModel>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CardModel> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = cardHolder.CardNode.Model;
		inspectCardScreen.Open(list, 0);
	}

	private void CancelSelection(NButton _)
	{
		_banner.AnimateIn();
		_bundlePreviewContainer.Visible = false;
		_bundlePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_cardTween?.Kill();
		_selectedBundle?.ReAddCardNodes();
		_selectedBundle?.Hitbox.TryGrabFocus();
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		_selectedBundle = null;
		_bundleRow.Visible = true;
	}

	private void ConfirmSelection(NButton _)
	{
		IReadOnlyList<NCard> cardNodes = _selectedBundle.CardNodes;
		foreach (NCard item in cardNodes)
		{
			NRun.Instance.GlobalUi.ReparentCard(item);
			Vector2 targetPosition = PileType.Deck.GetTargetPosition(item);
			NCardFlyVfx child = NCardFlyVfx.Create(item, targetPosition, isAddingToPile: true, item.Model.Owner.Character.TrailPath);
			NRun.Instance.GlobalUi.TopBar.TrailContainer.AddChildSafely(child);
		}
		_completionSource.SetResult(new global::_003C_003Ez__ReadOnlySingleElementList<IReadOnlyList<CardModel>>(_selectedBundle.Bundle));
	}

	public async Task<IEnumerable<IReadOnlyList<CardModel>>> CardsSelected()
	{
		IEnumerable<IReadOnlyList<CardModel>> result = await _completionSource.Task;
		NOverlayStack.Instance.Remove(this);
		return result;
	}

	public void AfterOverlayOpened()
	{
		base.Modulate = Colors.Transparent;
		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenProperty(this, "modulate:a", 1f, 0.4);
	}

	public void AfterOverlayClosed()
	{
		_fadeTween?.Kill();
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		base.Visible = true;
		if (_bundlePreviewContainer.Visible)
		{
			_previewCancelButton.Enable();
			_previewConfirmButton.Enable();
		}
	}

	public void AfterOverlayHidden()
	{
		base.Visible = false;
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBundleClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "bundleNode", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenPreviewScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CancelSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ConfirmSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBundleClicked && args.Count == 1)
		{
			OnBundleClicked(VariantUtils.ConvertTo<NCardBundle>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPreviewScreen && args.Count == 1)
		{
			OpenPreviewScreen(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelSelection && args.Count == 1)
		{
			CancelSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfirmSelection && args.Count == 1)
		{
			ConfirmSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnBundleClicked)
		{
			return true;
		}
		if (method == MethodName.OpenPreviewScreen)
		{
			return true;
		}
		if (method == MethodName.CancelSelection)
		{
			return true;
		}
		if (method == MethodName.ConfirmSelection)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._bundleRow)
		{
			_bundleRow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bundlePreviewContainer)
		{
			_bundlePreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bundlePreviewCards)
		{
			_bundlePreviewCards = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._previewCancelButton)
		{
			_previewCancelButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._previewConfirmButton)
		{
			_previewConfirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._selectedBundle)
		{
			_selectedBundle = VariantUtils.ConvertTo<NCardBundle>(in value);
			return true;
		}
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			_peekButton = VariantUtils.ConvertTo<NPeekButton>(in value);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			_fadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._bundleRow)
		{
			value = VariantUtils.CreateFrom(in _bundleRow);
			return true;
		}
		if (name == PropertyName._bundlePreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _bundlePreviewContainer);
			return true;
		}
		if (name == PropertyName._bundlePreviewCards)
		{
			value = VariantUtils.CreateFrom(in _bundlePreviewCards);
			return true;
		}
		if (name == PropertyName._previewCancelButton)
		{
			value = VariantUtils.CreateFrom(in _previewCancelButton);
			return true;
		}
		if (name == PropertyName._previewConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _previewConfirmButton);
			return true;
		}
		if (name == PropertyName._selectedBundle)
		{
			value = VariantUtils.CreateFrom(in _selectedBundle);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			value = VariantUtils.CreateFrom(in _peekButton);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			value = VariantUtils.CreateFrom(in _fadeTween);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bundleRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bundlePreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bundlePreviewCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedBundle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._peekButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bundleRow, Variant.From(in _bundleRow));
		info.AddProperty(PropertyName._bundlePreviewContainer, Variant.From(in _bundlePreviewContainer));
		info.AddProperty(PropertyName._bundlePreviewCards, Variant.From(in _bundlePreviewCards));
		info.AddProperty(PropertyName._previewCancelButton, Variant.From(in _previewCancelButton));
		info.AddProperty(PropertyName._previewConfirmButton, Variant.From(in _previewConfirmButton));
		info.AddProperty(PropertyName._selectedBundle, Variant.From(in _selectedBundle));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._peekButton, Variant.From(in _peekButton));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bundleRow, out var value))
		{
			_bundleRow = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bundlePreviewContainer, out var value2))
		{
			_bundlePreviewContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bundlePreviewCards, out var value3))
		{
			_bundlePreviewCards = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._previewCancelButton, out var value4))
		{
			_previewCancelButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._previewConfirmButton, out var value5))
		{
			_previewConfirmButton = value5.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._selectedBundle, out var value6))
		{
			_selectedBundle = value6.As<NCardBundle>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value7))
		{
			_banner = value7.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._peekButton, out var value8))
		{
			_peekButton = value8.As<NPeekButton>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value9))
		{
			_fadeTween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value10))
		{
			_cardTween = value10.As<Tween>();
		}
	}
}
