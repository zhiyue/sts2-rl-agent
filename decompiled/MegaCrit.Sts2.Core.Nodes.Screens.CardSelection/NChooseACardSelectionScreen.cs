using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ControllerInput;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NChooseACardSelectionScreen.cs")]
public class NChooseACardSelectionScreen : Control, IOverlayScreen, IScreenContext, ICardSelector
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName SelectHolder = "SelectHolder";

		public static readonly StringName OpenPreviewScreen = "OpenPreviewScreen";

		public static readonly StringName OnSkipButtonReleased = "OnSkipButtonReleased";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";

		public static readonly StringName UpdateControllerIcons = "UpdateControllerIcons";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _cardRow = "_cardRow";

		public static readonly StringName _skipButton = "_skipButton";

		public static readonly StringName _combatPiles = "_combatPiles";

		public static readonly StringName _inspectPrompt = "_inspectPrompt";

		public static readonly StringName _peekButton = "_peekButton";

		public static readonly StringName _openedTicks = "_openedTicks";

		public static readonly StringName _screenComplete = "_screenComplete";

		public static readonly StringName _cardSelected = "_cardSelected";

		public static readonly StringName _canSkip = "_canSkip";

		public static readonly StringName _cardTween = "_cardTween";

		public static readonly StringName _fadeTween = "_fadeTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _cardXSpacing = 340f;

	private const ulong _noSelectionTimeMsec = 350uL;

	private NCommonBanner _banner;

	private Control _cardRow;

	private NChoiceSelectionSkipButton _skipButton;

	private NCombatPilesContainer _combatPiles;

	private Control _inspectPrompt;

	private NPeekButton _peekButton;

	private readonly TaskCompletionSource<IEnumerable<CardModel>> _completionSource = new TaskCompletionSource<IEnumerable<CardModel>>();

	private ulong _openedTicks;

	private bool _screenComplete;

	private bool _cardSelected;

	private bool _canSkip;

	private Tween? _cardTween;

	private Tween? _fadeTween;

	private IReadOnlyList<CardModel> _cards;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/choose_a_card_selection_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NetScreenType ScreenType => NetScreenType.CardSelection;

	public bool UseSharedBackstop => true;

	public Control DefaultFocusedControl
	{
		get
		{
			if (_peekButton.IsPeeking)
			{
				return NCombatRoom.Instance.DefaultFocusedControl;
			}
			List<NGridCardHolder> list = _cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
			return list[list.Count / 2];
		}
	}

	public static NChooseACardSelectionScreen? ShowScreen(IReadOnlyList<CardModel> cards, bool canSkip)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NChooseACardSelectionScreen nChooseACardSelectionScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NChooseACardSelectionScreen>(PackedScene.GenEditState.Disabled);
		nChooseACardSelectionScreen.Name = "NChooseACardSelectionScreen";
		nChooseACardSelectionScreen._cards = cards;
		nChooseACardSelectionScreen._canSkip = canSkip;
		NOverlayStack.Instance.Push(nChooseACardSelectionScreen);
		return nChooseACardSelectionScreen;
	}

	public override void _Ready()
	{
		_banner = GetNode<NCommonBanner>("Banner");
		_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_CARD_HEADER").GetRawText());
		_banner.AnimateIn();
		_cardRow = GetNode<Control>("CardRow");
		_combatPiles = GetNode<NCombatPilesContainer>("%CombatPiles");
		if (CombatManager.Instance.IsInProgress)
		{
			_combatPiles.Initialize(_cards.First().Owner);
		}
		_combatPiles.Disable();
		_combatPiles.Visible = false;
		_inspectPrompt = GetNode<Control>("%InspectPrompt");
		Vector2 vector = Vector2.Left * (_cards.Count - 1) * 340f * 0.5f;
		_cardTween = CreateTween().SetParallel();
		for (int i = 0; i < _cards.Count; i++)
		{
			CardModel card = _cards[i];
			NCard nCard = NCard.Create(card);
			NGridCardHolder nGridCardHolder = NGridCardHolder.Create(nCard);
			_cardRow.AddChildSafely(nGridCardHolder);
			nGridCardHolder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(SelectHolder));
			nGridCardHolder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(OpenPreviewScreen));
			nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			nGridCardHolder.Scale = nGridCardHolder.SmallScale;
			_cardTween.TweenProperty(nGridCardHolder, "position", vector + Vector2.Right * 340f * i, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_cardTween.TweenProperty(nGridCardHolder, "modulate", Colors.White, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
				.From(Colors.Black);
			nCard.ActivateRewardScreenGlow();
		}
		_skipButton = GetNode<NChoiceSelectionSkipButton>("SkipButton");
		if (_canSkip)
		{
			_skipButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSkipButtonReleased));
			_skipButton.AnimateIn();
		}
		else
		{
			_skipButton.Disable();
			_skipButton.Visible = false;
		}
		_peekButton = GetNode<NPeekButton>("%PeekButton");
		_peekButton.AddTargets(_banner, _cardRow, _skipButton, _inspectPrompt);
		_peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(delegate
		{
			if (_peekButton.IsPeeking)
			{
				base.MouseFilter = MouseFilterEnum.Ignore;
				_combatPiles.Visible = true;
				_combatPiles.Enable();
				_skipButton.Disable();
			}
			else
			{
				base.MouseFilter = MouseFilterEnum.Stop;
				_combatPiles.Visible = false;
				_combatPiles.Disable();
				ActiveScreenContext.Instance.Update();
				if (_canSkip)
				{
					_skipButton.Enable();
				}
			}
		}));
		for (int num = 0; num < _cardRow.GetChildCount(); num++)
		{
			Control child = _cardRow.GetChild<Control>(num);
			child.FocusNeighborBottom = child.GetPath();
			child.FocusNeighborTop = child.GetPath();
			child.FocusNeighborLeft = ((num > 0) ? _cardRow.GetChild(num - 1).GetPath() : _cardRow.GetChild(_cardRow.GetChildCount() - 1).GetPath());
			child.FocusNeighborRight = ((num < _cardRow.GetChildCount() - 1) ? _cardRow.GetChild(num + 1).GetPath() : _cardRow.GetChild(0).GetPath());
		}
		UpdateControllerIcons();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerIcons));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerIcons));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerIcons));
	}

	public override void _ExitTree()
	{
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.SetCanceled();
		}
		foreach (NGridCardHolder item in _cardRow.GetChildren().OfType<NGridCardHolder>())
		{
			item.QueueFreeSafely();
		}
	}

	private void SelectHolder(NCardHolder cardHolder)
	{
		if (_completionSource == null)
		{
			throw new InvalidOperationException("CardsSelected must be awaited before a card is selected!");
		}
		if (Time.GetTicksMsec() - _openedTicks > 350)
		{
			CardModel cardModel = cardHolder.CardModel;
			_screenComplete = true;
			_cardSelected = true;
			_completionSource.SetResult(new CardModel[1] { cardModel });
		}
	}

	private void OpenPreviewScreen(NCardHolder cardHolder)
	{
		NInspectCardScreen inspectCardScreen = NGame.Instance.GetInspectCardScreen();
		int num = 1;
		List<CardModel> list = new List<CardModel>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CardModel> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = cardHolder.CardModel;
		inspectCardScreen.Open(list, 0);
	}

	public async Task<IEnumerable<CardModel>> CardsSelected()
	{
		IEnumerable<CardModel> result = await _completionSource.Task;
		NOverlayStack.Instance.Remove(this);
		return result;
	}

	private void OnSkipButtonReleased(NButton _)
	{
		_screenComplete = true;
		_completionSource.SetResult(Array.Empty<CardModel>());
	}

	public void AfterOverlayOpened()
	{
		base.Modulate = Colors.Transparent;
		_openedTicks = Time.GetTicksMsec();
		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenProperty(this, "modulate:a", 1f, 0.2);
	}

	public void AfterOverlayClosed()
	{
		_fadeTween?.Kill();
		_peekButton.SetPeeking(isPeeking: false);
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		base.Visible = true;
		_peekButton.Enable();
		if (_canSkip && !_peekButton.IsPeeking)
		{
			_skipButton.Enable();
		}
	}

	public void AfterOverlayHidden()
	{
		_peekButton.Disable();
		_skipButton.Disable();
		base.Visible = false;
	}

	private void UpdateControllerIcons()
	{
		_inspectPrompt.Modulate = (NControllerManager.Instance.IsUsingController ? Colors.White : Colors.Transparent);
		_inspectPrompt.GetNode<TextureRect>("ControllerIcon").Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.accept);
		_inspectPrompt.GetNode<MegaLabel>("Label").SetTextAutoSize(new LocString("gameplay_ui", "TO_INSPECT_PROMPT").GetFormattedText());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SelectHolder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenPreviewScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSkipButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateControllerIcons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SelectHolder && args.Count == 1)
		{
			SelectHolder(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPreviewScreen && args.Count == 1)
		{
			OpenPreviewScreen(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSkipButtonReleased && args.Count == 1)
		{
			OnSkipButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName.UpdateControllerIcons && args.Count == 0)
		{
			UpdateControllerIcons();
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
		if (method == MethodName.SelectHolder)
		{
			return true;
		}
		if (method == MethodName.OpenPreviewScreen)
		{
			return true;
		}
		if (method == MethodName.OnSkipButtonReleased)
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
		if (method == MethodName.UpdateControllerIcons)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._cardRow)
		{
			_cardRow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._skipButton)
		{
			_skipButton = VariantUtils.ConvertTo<NChoiceSelectionSkipButton>(in value);
			return true;
		}
		if (name == PropertyName._combatPiles)
		{
			_combatPiles = VariantUtils.ConvertTo<NCombatPilesContainer>(in value);
			return true;
		}
		if (name == PropertyName._inspectPrompt)
		{
			_inspectPrompt = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			_peekButton = VariantUtils.ConvertTo<NPeekButton>(in value);
			return true;
		}
		if (name == PropertyName._openedTicks)
		{
			_openedTicks = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._screenComplete)
		{
			_screenComplete = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cardSelected)
		{
			_cardSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._canSkip)
		{
			_canSkip = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._cardRow)
		{
			value = VariantUtils.CreateFrom(in _cardRow);
			return true;
		}
		if (name == PropertyName._skipButton)
		{
			value = VariantUtils.CreateFrom(in _skipButton);
			return true;
		}
		if (name == PropertyName._combatPiles)
		{
			value = VariantUtils.CreateFrom(in _combatPiles);
			return true;
		}
		if (name == PropertyName._inspectPrompt)
		{
			value = VariantUtils.CreateFrom(in _inspectPrompt);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			value = VariantUtils.CreateFrom(in _peekButton);
			return true;
		}
		if (name == PropertyName._openedTicks)
		{
			value = VariantUtils.CreateFrom(in _openedTicks);
			return true;
		}
		if (name == PropertyName._screenComplete)
		{
			value = VariantUtils.CreateFrom(in _screenComplete);
			return true;
		}
		if (name == PropertyName._cardSelected)
		{
			value = VariantUtils.CreateFrom(in _cardSelected);
			return true;
		}
		if (name == PropertyName._canSkip)
		{
			value = VariantUtils.CreateFrom(in _canSkip);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._skipButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatPiles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inspectPrompt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._peekButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._openedTicks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._screenComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._cardSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._canSkip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._cardRow, Variant.From(in _cardRow));
		info.AddProperty(PropertyName._skipButton, Variant.From(in _skipButton));
		info.AddProperty(PropertyName._combatPiles, Variant.From(in _combatPiles));
		info.AddProperty(PropertyName._inspectPrompt, Variant.From(in _inspectPrompt));
		info.AddProperty(PropertyName._peekButton, Variant.From(in _peekButton));
		info.AddProperty(PropertyName._openedTicks, Variant.From(in _openedTicks));
		info.AddProperty(PropertyName._screenComplete, Variant.From(in _screenComplete));
		info.AddProperty(PropertyName._cardSelected, Variant.From(in _cardSelected));
		info.AddProperty(PropertyName._canSkip, Variant.From(in _canSkip));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._banner, out var value))
		{
			_banner = value.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._cardRow, out var value2))
		{
			_cardRow = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._skipButton, out var value3))
		{
			_skipButton = value3.As<NChoiceSelectionSkipButton>();
		}
		if (info.TryGetProperty(PropertyName._combatPiles, out var value4))
		{
			_combatPiles = value4.As<NCombatPilesContainer>();
		}
		if (info.TryGetProperty(PropertyName._inspectPrompt, out var value5))
		{
			_inspectPrompt = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._peekButton, out var value6))
		{
			_peekButton = value6.As<NPeekButton>();
		}
		if (info.TryGetProperty(PropertyName._openedTicks, out var value7))
		{
			_openedTicks = value7.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._screenComplete, out var value8))
		{
			_screenComplete = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardSelected, out var value9))
		{
			_cardSelected = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._canSkip, out var value10))
		{
			_canSkip = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value11))
		{
			_cardTween = value11.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value12))
		{
			_fadeTween = value12.As<Tween>();
		}
	}
}
