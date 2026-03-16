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
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NCardRewardSelectionScreen.cs")]
public class NCardRewardSelectionScreen : Control, IOverlayScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnAlternateRewardSelected = "OnAlternateRewardSelected";

		public static readonly StringName SelectCard = "SelectCard";

		public static readonly StringName InspectCard = "InspectCard";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName PowerCardFtueCheck = "PowerCardFtueCheck";

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

		public static readonly StringName _ui = "_ui";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _cardRow = "_cardRow";

		public static readonly StringName _rewardAlternativesContainer = "_rewardAlternativesContainer";

		public static readonly StringName _inspectPrompt = "_inspectPrompt";

		public static readonly StringName _cardTween = "_cardTween";

		public static readonly StringName _buttonTween = "_buttonTween";

		public static readonly StringName _lastFocusedControl = "_lastFocusedControl";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const ulong _noSelectionTimeMsec = 350uL;

	private Control _ui;

	private NCommonBanner _banner;

	private Control _cardRow;

	private IReadOnlyList<CardCreationResult> _options;

	private IReadOnlyList<CardRewardAlternative> _extraOptions;

	private Control _rewardAlternativesContainer;

	private Control _inspectPrompt;

	private TaskCompletionSource<Tuple<IEnumerable<NCardHolder>, bool>>? _completionSource;

	private Tween? _cardTween;

	private Tween? _buttonTween;

	private const float _cardXOffset = 350f;

	private static readonly Vector2 _bannerAnimPosOffset = new Vector2(0f, 50f);

	private Control? _lastFocusedControl;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/card_reward_selection_screen");

	public static IEnumerable<string> AssetPaths => new string[1] { ScenePath }.Concat(NCardRewardAlternativeButton.AssetPaths);

	public NetScreenType ScreenType => NetScreenType.CardSelection;

	public bool UseSharedBackstop => true;

	public Control DefaultFocusedControl
	{
		get
		{
			if (_lastFocusedControl != null)
			{
				return _lastFocusedControl;
			}
			List<NGridCardHolder> list = _cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
			return list[list.Count / 2];
		}
	}

	public static NCardRewardSelectionScreen? ShowScreen(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> extraOptions)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardRewardSelectionScreen nCardRewardSelectionScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardRewardSelectionScreen>(PackedScene.GenEditState.Disabled);
		nCardRewardSelectionScreen.Name = "NCardRewardSelectionScreen";
		nCardRewardSelectionScreen._options = options;
		nCardRewardSelectionScreen._extraOptions = extraOptions;
		NOverlayStack.Instance.Push(nCardRewardSelectionScreen);
		return nCardRewardSelectionScreen;
	}

	public override void _Ready()
	{
		_ui = GetNode<Control>("UI");
		_cardRow = GetNode<Control>("UI/CardRow");
		_banner = GetNode<NCommonBanner>("UI/Banner");
		_rewardAlternativesContainer = GetNode<Control>("UI/RewardAlternatives");
		_inspectPrompt = GetNode<Control>("%InspectPrompt");
		_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_CARD_HEADER").GetRawText());
		_banner.AnimateIn();
		RefreshOptions(_options, _extraOptions);
		UpdateControllerIcons();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerIcons));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerIcons));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerIcons));
	}

	public void RefreshOptions(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> extraOptions)
	{
		_options = options;
		_extraOptions = extraOptions;
		Vector2 vector = Vector2.Left * (_options.Count - 1) * 350f * 0.5f;
		_lastFocusedControl = null;
		foreach (NGridCardHolder item in _cardRow.GetChildren().OfType<NGridCardHolder>())
		{
			item.QueueFreeSafely();
		}
		foreach (NCardRewardAlternativeButton item2 in _rewardAlternativesContainer.GetChildren().OfType<NCardRewardAlternativeButton>())
		{
			item2.QueueFreeSafely();
		}
		_cardTween = CreateTween().SetParallel();
		for (int i = 0; i < _options.Count; i++)
		{
			CardCreationResult cardCreationResult = _options[i];
			NCard nCard = NCard.Create(cardCreationResult.Card);
			NGridCardHolder holder = NGridCardHolder.Create(nCard);
			_cardRow.AddChildSafely(holder);
			holder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(SelectCard));
			holder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(InspectCard));
			holder.Connect(Control.SignalName.FocusEntered, Callable.From(() => _lastFocusedControl = holder));
			nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			holder.Scale = holder.SmallScale;
			_cardTween.TweenProperty(holder, "position", vector + Vector2.Right * 350f * i, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_cardTween.TweenProperty(holder, "modulate", Colors.White, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
				.From(Colors.Black);
			nCard.ActivateRewardScreenGlow();
			foreach (RelicModel modifyingRelic in cardCreationResult.ModifyingRelics)
			{
				modifyingRelic.Flash();
				nCard.FlashRelicOnCard(modifyingRelic);
			}
		}
		foreach (CardRewardAlternative rewardOption in _extraOptions)
		{
			NCardRewardAlternativeButton nCardRewardAlternativeButton = NCardRewardAlternativeButton.Create(rewardOption.Title.GetFormattedText(), rewardOption.Hotkey);
			_rewardAlternativesContainer.AddChildSafely(nCardRewardAlternativeButton);
			nCardRewardAlternativeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
			{
				OnAlternateRewardSelected(rewardOption.AfterSelected);
				TaskHelper.RunSafely(rewardOption.OnSelect());
			}));
		}
		List<NGridCardHolder> list = _cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
		for (int num = 0; num < _cardRow.GetChildCount(); num++)
		{
			Control child = _cardRow.GetChild<Control>(num);
			child.FocusNeighborBottom = child.GetPath();
			child.FocusNeighborTop = child.GetPath();
			child.FocusNeighborLeft = ((num > 0) ? _cardRow.GetChild(num - 1).GetPath() : _cardRow.GetChild(_cardRow.GetChildCount() - 1).GetPath());
			child.FocusNeighborRight = ((num < _cardRow.GetChildCount() - 1) ? _cardRow.GetChild(num + 1).GetPath() : _cardRow.GetChild(0).GetPath());
		}
		if (NControllerManager.Instance.IsUsingController)
		{
			list[list.Count / 2].TryGrabFocus();
		}
	}

	public override void _ExitTree()
	{
		TaskCompletionSource<Tuple<IEnumerable<NCardHolder>, bool>> completionSource = _completionSource;
		if (completionSource != null)
		{
			Task<Tuple<IEnumerable<NCardHolder>, bool>> task = completionSource.Task;
			if (task != null && !task.IsCompleted)
			{
				_completionSource.SetResult(new Tuple<IEnumerable<NCardHolder>, bool>(Array.Empty<NCardHolder>(), item2: false));
			}
		}
		foreach (NGridCardHolder item in _cardRow.GetChildren().OfType<NGridCardHolder>())
		{
			item.QueueFreeSafely();
		}
	}

	private void OnAlternateRewardSelected(PostAlternateCardRewardAction afterSelected)
	{
		if ((afterSelected != PostAlternateCardRewardAction.None && afterSelected != PostAlternateCardRewardAction.DoNothing) || 1 == 0)
		{
			_completionSource?.SetResult(new Tuple<IEnumerable<NCardHolder>, bool>(Array.Empty<NCardHolder>(), afterSelected == PostAlternateCardRewardAction.DismissScreenAndRemoveReward));
		}
	}

	private void SelectCard(NCardHolder cardHolder)
	{
		if (_completionSource == null)
		{
			throw new InvalidOperationException("CardsSelected must be awaited before a card is selected!");
		}
		_completionSource.SetResult(new Tuple<IEnumerable<NCardHolder>, bool>(new global::_003C_003Ez__ReadOnlySingleElementList<NCardHolder>(cardHolder), item2: true));
	}

	private void InspectCard(NCardHolder cardHolder)
	{
		if (!_completionSource.Task.IsCompleted)
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
	}

	public async Task<Tuple<IEnumerable<NCardHolder>, bool>> CardsSelected()
	{
		_completionSource = new TaskCompletionSource<Tuple<IEnumerable<NCardHolder>, bool>>();
		return await _completionSource.Task;
	}

	public void AfterOverlayOpened()
	{
		PowerCardFtueCheck();
		_banner.AnimateIn();
		_buttonTween = CreateTween();
		_buttonTween.SetParallel();
		_buttonTween.TweenProperty(_rewardAlternativesContainer, "position", _rewardAlternativesContainer.Position, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_rewardAlternativesContainer.Position - _bannerAnimPosOffset);
		TaskHelper.RunSafely(DisableCardsForShortTimeAfterOpening());
	}

	private async Task DisableCardsForShortTimeAfterOpening()
	{
		foreach (NGridCardHolder item in _cardRow.GetChildren().OfType<NGridCardHolder>())
		{
			item.SetClickable(isClickable: false);
		}
		await Cmd.Wait(0.35f);
		if (!_cardRow.IsValid())
		{
			return;
		}
		foreach (NGridCardHolder item2 in _cardRow.GetChildren().OfType<NGridCardHolder>())
		{
			item2.SetClickable(isClickable: true);
		}
	}

	private void PowerCardFtueCheck()
	{
		if (!SaveManager.Instance.SeenFtue("power_card_ftue"))
		{
			IEnumerable<NGridCardHolder> source = _cardRow.GetChildren().OfType<NGridCardHolder>();
			NGridCardHolder nGridCardHolder = source.FirstOrDefault((NGridCardHolder h) => h.CardModel.Type == CardType.Power);
			if (nGridCardHolder != null)
			{
				NModalContainer.Instance.Add(NPowerCardFtue.Create(nGridCardHolder));
				SaveManager.Instance.MarkFtueAsComplete("power_card_ftue");
			}
		}
	}

	public void AfterOverlayClosed()
	{
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		base.Visible = true;
	}

	public void AfterOverlayHidden()
	{
		base.Visible = false;
	}

	private void UpdateControllerIcons()
	{
		_inspectPrompt.Visible = NControllerManager.Instance.IsUsingController;
		_inspectPrompt.GetNode<TextureRect>("ControllerIcon").Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.accept);
		_inspectPrompt.GetNode<MegaLabel>("Label").SetTextAutoSize(new LocString("gameplay_ui", "TO_INSPECT_PROMPT").GetFormattedText());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAlternateRewardSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "afterSelected", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InspectCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PowerCardFtueCheck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAlternateRewardSelected && args.Count == 1)
		{
			OnAlternateRewardSelected(VariantUtils.ConvertTo<PostAlternateCardRewardAction>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectCard && args.Count == 1)
		{
			SelectCard(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InspectCard && args.Count == 1)
		{
			InspectCard(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PowerCardFtueCheck && args.Count == 0)
		{
			PowerCardFtueCheck();
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
		if (method == MethodName.OnAlternateRewardSelected)
		{
			return true;
		}
		if (method == MethodName.SelectCard)
		{
			return true;
		}
		if (method == MethodName.InspectCard)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.PowerCardFtueCheck)
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
		if (name == PropertyName._ui)
		{
			_ui = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
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
		if (name == PropertyName._rewardAlternativesContainer)
		{
			_rewardAlternativesContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._inspectPrompt)
		{
			_inspectPrompt = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._buttonTween)
		{
			_buttonTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._lastFocusedControl)
		{
			_lastFocusedControl = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._ui)
		{
			value = VariantUtils.CreateFrom(in _ui);
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
		if (name == PropertyName._rewardAlternativesContainer)
		{
			value = VariantUtils.CreateFrom(in _rewardAlternativesContainer);
			return true;
		}
		if (name == PropertyName._inspectPrompt)
		{
			value = VariantUtils.CreateFrom(in _inspectPrompt);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
			return true;
		}
		if (name == PropertyName._buttonTween)
		{
			value = VariantUtils.CreateFrom(in _buttonTween);
			return true;
		}
		if (name == PropertyName._lastFocusedControl)
		{
			value = VariantUtils.CreateFrom(in _lastFocusedControl);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ui, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rewardAlternativesContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inspectPrompt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ui, Variant.From(in _ui));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._cardRow, Variant.From(in _cardRow));
		info.AddProperty(PropertyName._rewardAlternativesContainer, Variant.From(in _rewardAlternativesContainer));
		info.AddProperty(PropertyName._inspectPrompt, Variant.From(in _inspectPrompt));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
		info.AddProperty(PropertyName._buttonTween, Variant.From(in _buttonTween));
		info.AddProperty(PropertyName._lastFocusedControl, Variant.From(in _lastFocusedControl));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ui, out var value))
		{
			_ui = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value2))
		{
			_banner = value2.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._cardRow, out var value3))
		{
			_cardRow = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._rewardAlternativesContainer, out var value4))
		{
			_rewardAlternativesContainer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._inspectPrompt, out var value5))
		{
			_inspectPrompt = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value6))
		{
			_cardTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._buttonTween, out var value7))
		{
			_buttonTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._lastFocusedControl, out var value8))
		{
			_lastFocusedControl = value8.As<Control>();
		}
	}
}
