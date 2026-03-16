using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NAncientEventLayout.cs")]
public class NAncientEventLayout : NEventLayout
{
	public new class MethodName : NEventLayout.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName InitializeVisuals = "InitializeVisuals";

		public new static readonly StringName AnimateIn = "AnimateIn";

		public static readonly StringName ClearDialogue = "ClearDialogue";

		public new static readonly StringName OnSetupComplete = "OnSetupComplete";

		public new static readonly StringName AnimateButtonsIn = "AnimateButtonsIn";

		public static readonly StringName OnDialogueHitboxClicked = "OnDialogueHitboxClicked";

		public static readonly StringName SetDialogueLineAndAnimate = "SetDialogueLineAndAnimate";

		public static readonly StringName UpdateFakeNextButton = "UpdateFakeNextButton";

		public static readonly StringName HideNameBanner = "HideNameBanner";

		public static readonly StringName ShowNameBanner = "ShowNameBanner";

		public static readonly StringName UpdateBannerVisibility = "UpdateBannerVisibility";

		public static readonly StringName UpdateHotkeyDisplay = "UpdateHotkeyDisplay";
	}

	public new class PropertyName : NEventLayout.PropertyName
	{
		public static readonly StringName IsDialogueOnLastLine = "IsDialogueOnLastLine";

		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _currentDialogueLine = "_currentDialogueLine";

		public static readonly StringName _ancientBgContainer = "_ancientBgContainer";

		public static readonly StringName _ancientNameBanner = "_ancientNameBanner";

		public static readonly StringName _bannerTween = "_bannerTween";

		public static readonly StringName _contentContainer = "_contentContainer";

		public static readonly StringName _originalContentContainerHeight = "_originalContentContainerHeight";

		public static readonly StringName _content = "_content";

		public static readonly StringName _dialogueContainer = "_dialogueContainer";

		public static readonly StringName _dialogueHitbox = "_dialogueHitbox";

		public static readonly StringName _fakeNextButtonContainer = "_fakeNextButtonContainer";

		public static readonly StringName _fakeNextButton = "_fakeNextButton";

		public static readonly StringName _fakeNextButtonControllerIcon = "_fakeNextButtonControllerIcon";

		public static readonly StringName _fakeNextButtonLabel = "_fakeNextButtonLabel";

		public static readonly StringName _contentTween = "_contentTween";
	}

	public new class SignalName : NEventLayout.SignalName
	{
	}

	public const string ancientScenePath = "res://scenes/events/ancient_event_layout.tscn";

	private const double _contentTweenDuration = 1.0;

	private AncientEventModel _ancientEvent;

	private readonly List<AncientDialogueLine> _dialogue = new List<AncientDialogueLine>();

	private int _currentDialogueLine;

	private NAncientBgContainer _ancientBgContainer;

	private Control? _ancientNameBanner;

	private Tween? _bannerTween;

	private Control _contentContainer;

	private float _originalContentContainerHeight;

	private VBoxContainer _content;

	private VBoxContainer _dialogueContainer;

	private NAncientDialogueHitbox _dialogueHitbox;

	private Control _fakeNextButtonContainer;

	private Control _fakeNextButton;

	private TextureRect _fakeNextButtonControllerIcon;

	private MegaLabel _fakeNextButtonLabel;

	private Tween? _contentTween;

	private bool IsDialogueOnLastLine => _currentDialogueLine >= _dialogue.Count - 1;

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (!IsDialogueOnLastLine)
			{
				return null;
			}
			return base.OptionButtons.FirstOrDefault();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		_ancientBgContainer = GetNode<NAncientBgContainer>("%AncientBgContainer");
		_contentContainer = GetNode<Control>("%ContentContainer");
		_content = GetNode<VBoxContainer>("%Content");
		_dialogueContainer = GetNode<VBoxContainer>("%DialogueContainer");
		_dialogueHitbox = GetNode<NAncientDialogueHitbox>("%DialogueHitbox");
		_dialogueHitbox.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(OnDialogueHitboxClicked));
		_dialogueHitbox.Visible = false;
		_dialogueHitbox.Disable();
		_fakeNextButtonContainer = GetNode<Control>("%FakeNextButtonContainer");
		_fakeNextButton = _fakeNextButtonContainer.GetNode<Control>("FakeNextButton");
		_fakeNextButtonLabel = _fakeNextButton.GetNode<MegaLabel>("Label");
		_fakeNextButtonControllerIcon = _fakeNextButton.GetNode<TextureRect>("ControllerIcon");
		_originalContentContainerHeight = _contentContainer.Size.Y;
		_contentContainer.Size = new Vector2(_contentContainer.Size.X, _fakeNextButtonContainer.GlobalPosition.Y - _contentContainer.GlobalPosition.Y);
		UpdateHotkeyDisplay();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateHotkeyDisplay));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateHotkeyDisplay));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateHotkeyDisplay));
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		ActiveScreenContext.Instance.Updated += UpdateBannerVisibility;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_ancientEvent.HasAmbientBgm)
		{
			SfxCmd.StopLoop(_ancientEvent.AmbientBgm);
		}
		ActiveScreenContext.Instance.Updated -= UpdateBannerVisibility;
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateHotkeyDisplay));
		NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateHotkeyDisplay));
		NInputManager.Instance.Disconnect(NInputManager.SignalName.InputRebound, Callable.From(UpdateHotkeyDisplay));
	}

	protected override void InitializeVisuals()
	{
		_ancientEvent = (AncientEventModel)_event;
		_ancientNameBanner = NAncientNameBanner.Create(_ancientEvent);
		this.AddChildSafely(_ancientNameBanner);
		UpdateBannerVisibility();
		AncientEventModel ancientEvent = _ancientEvent;
		if (ancientEvent != null && ancientEvent.Owner != null && ancientEvent.HealedAmount > 0)
		{
			TaskHelper.RunSafely(PlayHealVfxAfterFadeIn(_ancientEvent.Owner, _ancientEvent.HealedAmount));
		}
		foreach (Node child in _ancientBgContainer.GetChildren())
		{
			_ancientBgContainer.RemoveChildSafely(child);
		}
		_ancientBgContainer.AddChildSafely(_ancientEvent.CreateBackgroundScene().Instantiate<Control>(PackedScene.GenEditState.Disabled));
		if (_ancientEvent.HasAmbientBgm)
		{
			SfxCmd.PlayLoop(_ancientEvent.AmbientBgm);
		}
	}

	protected override void AnimateIn()
	{
		if (_description != null)
		{
			_description.Modulate = Colors.Transparent;
			_descriptionTween?.Kill();
			_descriptionTween = CreateTween().SetParallel();
			if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast)
			{
				_descriptionTween.TweenInterval(0.2);
			}
			else
			{
				_descriptionTween.TweenInterval(0.5);
			}
			_descriptionTween.Chain();
			_descriptionTween.TweenProperty(_description, "modulate", Colors.White, 1.0);
		}
	}

	public void SetDialogue(IReadOnlyList<AncientDialogueLine> lines)
	{
		_dialogue.Clear();
		_dialogue.AddRange(lines);
		_currentDialogueLine = 0;
		foreach (AncientDialogueLine line in lines)
		{
			NAncientDialogueLine child = NAncientDialogueLine.Create(line, _ancientEvent, _ancientEvent.Owner.Character);
			_dialogueContainer.AddChildSafely(child);
		}
	}

	public void ClearDialogue()
	{
		_dialogue.Clear();
		_dialogueContainer.FreeChildren();
	}

	public override void OnSetupComplete()
	{
		_dialogueContainer.ResetSize();
		_optionsContainer.ResetSize();
		_content.ResetSize();
		_content.Position = new Vector2(_content.Position.X, _contentContainer.Size.Y);
		SetDialogueLineAndAnimate(0);
	}

	protected override void AnimateButtonsIn()
	{
		foreach (NEventOptionButton optionButton in base.OptionButtons)
		{
			optionButton.Modulate = Colors.White;
			optionButton.EnableButton();
		}
	}

	private async Task PlayHealVfxAfterFadeIn(Player player, decimal healAmount)
	{
		await Cmd.Wait(0.2f);
		PlayerFullscreenHealVfx.Play(player, healAmount, base.VfxContainer);
	}

	private void OnDialogueHitboxClicked(NClickableControl _)
	{
		SetDialogueLineAndAnimate(_currentDialogueLine + 1);
	}

	private void SetDialogueLineAndAnimate(int lineIndex)
	{
		_currentDialogueLine = lineIndex;
		if (_contentTween != null)
		{
			_contentTween.Pause();
			_contentTween.CustomStep(1.0);
			_contentTween.Kill();
			_contentTween = null;
		}
		UpdateFakeNextButton();
		NAncientDialogueLine childOrNull = _dialogueContainer.GetChildOrNull<NAncientDialogueLine>(_currentDialogueLine);
		childOrNull?.PlaySfx();
		float num = ((childOrNull != null) ? (childOrNull.Position.Y + childOrNull.Size.Y) : 0f);
		if (IsDialogueOnLastLine)
		{
			_fakeNextButtonContainer.Visible = false;
			_contentContainer.Size = new Vector2(_contentContainer.Size.X, _originalContentContainerHeight);
			_dialogueHitbox.Visible = false;
			_dialogueHitbox.Disable();
			foreach (NEventOptionButton optionButton in base.OptionButtons)
			{
				optionButton.EnableButton();
			}
			num += _optionsContainer.Size.Y + 10f;
		}
		else
		{
			_fakeNextButtonContainer.Visible = true;
			_dialogueHitbox.Visible = true;
			_dialogueHitbox.Enable();
		}
		if (_dialogueContainer.GetChildCount() > _currentDialogueLine)
		{
			_dialogueContainer.GetChild<NAncientDialogueLine>(_currentDialogueLine).SetSpeakerIconVisible();
		}
		foreach (NEventOptionButton optionButton2 in base.OptionButtons)
		{
			optionButton2.FocusMode = FocusModeEnum.None;
		}
		_contentTween = CreateTween();
		_contentTween.TweenProperty(_content, "position", new Vector2(_content.Position.X, _contentContainer.Size.Y - num), 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		if (IsDialogueOnLastLine)
		{
			_contentTween.Parallel().TweenCallback(Callable.From(delegate
			{
				foreach (NEventOptionButton optionButton3 in base.OptionButtons)
				{
					optionButton3.FocusMode = FocusModeEnum.All;
				}
				DefaultFocusedControl?.TryGrabFocus();
			})).SetDelay(0.8);
		}
		for (int num2 = 0; num2 < _currentDialogueLine; num2++)
		{
			NAncientDialogueLine child = _dialogueContainer.GetChild<NAncientDialogueLine>(num2);
			child.SetTransparency((num2 != _currentDialogueLine) ? 0.25f : 1f);
		}
	}

	private void UpdateFakeNextButton()
	{
		LocString locString = ((_dialogue.Count > _currentDialogueLine) ? _dialogue[_currentDialogueLine].NextButtonText : null);
		if (locString != null)
		{
			_fakeNextButtonLabel.SetTextAutoSize(locString.GetFormattedText() ?? "");
		}
		else
		{
			_fakeNextButtonLabel.Visible = false;
		}
	}

	private void HideNameBanner()
	{
		if (_ancientNameBanner != null)
		{
			_bannerTween?.Kill();
			_bannerTween = CreateTween();
			_bannerTween.TweenProperty(_ancientNameBanner, "modulate:a", 0f, 0.25);
		}
	}

	private void ShowNameBanner()
	{
		if (_ancientNameBanner != null)
		{
			_bannerTween?.Kill();
			_bannerTween = CreateTween();
			_bannerTween.TweenProperty(_ancientNameBanner, "modulate:a", 1f, 0.5);
		}
	}

	private void UpdateBannerVisibility()
	{
		if (NEventRoom.Instance != null)
		{
			if (ActiveScreenContext.Instance.IsCurrent(NEventRoom.Instance))
			{
				ShowNameBanner();
			}
			else
			{
				HideNameBanner();
			}
		}
	}

	private void UpdateHotkeyDisplay()
	{
		_fakeNextButtonControllerIcon.Visible = NControllerManager.Instance.IsUsingController;
		string hotkey = _dialogueHitbox.GetHotkey();
		if (hotkey != null)
		{
			_fakeNextButtonControllerIcon.Texture = NInputManager.Instance.GetHotkeyIcon(hotkey);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearDialogue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSetupComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateButtonsIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDialogueHitboxClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetDialogueLineAndAnimate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "lineIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateFakeNextButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideNameBanner, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowNameBanner, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateBannerVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHotkeyDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.InitializeVisuals && args.Count == 0)
		{
			InitializeVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearDialogue && args.Count == 0)
		{
			ClearDialogue();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSetupComplete && args.Count == 0)
		{
			OnSetupComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateButtonsIn && args.Count == 0)
		{
			AnimateButtonsIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDialogueHitboxClicked && args.Count == 1)
		{
			OnDialogueHitboxClicked(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDialogueLineAndAnimate && args.Count == 1)
		{
			SetDialogueLineAndAnimate(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateFakeNextButton && args.Count == 0)
		{
			UpdateFakeNextButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideNameBanner && args.Count == 0)
		{
			HideNameBanner();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowNameBanner && args.Count == 0)
		{
			ShowNameBanner();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateBannerVisibility && args.Count == 0)
		{
			UpdateBannerVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHotkeyDisplay && args.Count == 0)
		{
			UpdateHotkeyDisplay();
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
		if (method == MethodName.InitializeVisuals)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.ClearDialogue)
		{
			return true;
		}
		if (method == MethodName.OnSetupComplete)
		{
			return true;
		}
		if (method == MethodName.AnimateButtonsIn)
		{
			return true;
		}
		if (method == MethodName.OnDialogueHitboxClicked)
		{
			return true;
		}
		if (method == MethodName.SetDialogueLineAndAnimate)
		{
			return true;
		}
		if (method == MethodName.UpdateFakeNextButton)
		{
			return true;
		}
		if (method == MethodName.HideNameBanner)
		{
			return true;
		}
		if (method == MethodName.ShowNameBanner)
		{
			return true;
		}
		if (method == MethodName.UpdateBannerVisibility)
		{
			return true;
		}
		if (method == MethodName.UpdateHotkeyDisplay)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._currentDialogueLine)
		{
			_currentDialogueLine = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._ancientBgContainer)
		{
			_ancientBgContainer = VariantUtils.ConvertTo<NAncientBgContainer>(in value);
			return true;
		}
		if (name == PropertyName._ancientNameBanner)
		{
			_ancientNameBanner = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bannerTween)
		{
			_bannerTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._contentContainer)
		{
			_contentContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._originalContentContainerHeight)
		{
			_originalContentContainerHeight = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._content)
		{
			_content = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._dialogueContainer)
		{
			_dialogueContainer = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._dialogueHitbox)
		{
			_dialogueHitbox = VariantUtils.ConvertTo<NAncientDialogueHitbox>(in value);
			return true;
		}
		if (name == PropertyName._fakeNextButtonContainer)
		{
			_fakeNextButtonContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._fakeNextButton)
		{
			_fakeNextButton = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._fakeNextButtonControllerIcon)
		{
			_fakeNextButtonControllerIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._fakeNextButtonLabel)
		{
			_fakeNextButtonLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._contentTween)
		{
			_contentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsDialogueOnLastLine)
		{
			value = VariantUtils.CreateFrom<bool>(IsDialogueOnLastLine);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._currentDialogueLine)
		{
			value = VariantUtils.CreateFrom(in _currentDialogueLine);
			return true;
		}
		if (name == PropertyName._ancientBgContainer)
		{
			value = VariantUtils.CreateFrom(in _ancientBgContainer);
			return true;
		}
		if (name == PropertyName._ancientNameBanner)
		{
			value = VariantUtils.CreateFrom(in _ancientNameBanner);
			return true;
		}
		if (name == PropertyName._bannerTween)
		{
			value = VariantUtils.CreateFrom(in _bannerTween);
			return true;
		}
		if (name == PropertyName._contentContainer)
		{
			value = VariantUtils.CreateFrom(in _contentContainer);
			return true;
		}
		if (name == PropertyName._originalContentContainerHeight)
		{
			value = VariantUtils.CreateFrom(in _originalContentContainerHeight);
			return true;
		}
		if (name == PropertyName._content)
		{
			value = VariantUtils.CreateFrom(in _content);
			return true;
		}
		if (name == PropertyName._dialogueContainer)
		{
			value = VariantUtils.CreateFrom(in _dialogueContainer);
			return true;
		}
		if (name == PropertyName._dialogueHitbox)
		{
			value = VariantUtils.CreateFrom(in _dialogueHitbox);
			return true;
		}
		if (name == PropertyName._fakeNextButtonContainer)
		{
			value = VariantUtils.CreateFrom(in _fakeNextButtonContainer);
			return true;
		}
		if (name == PropertyName._fakeNextButton)
		{
			value = VariantUtils.CreateFrom(in _fakeNextButton);
			return true;
		}
		if (name == PropertyName._fakeNextButtonControllerIcon)
		{
			value = VariantUtils.CreateFrom(in _fakeNextButtonControllerIcon);
			return true;
		}
		if (name == PropertyName._fakeNextButtonLabel)
		{
			value = VariantUtils.CreateFrom(in _fakeNextButtonLabel);
			return true;
		}
		if (name == PropertyName._contentTween)
		{
			value = VariantUtils.CreateFrom(in _contentTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentDialogueLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientBgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientNameBanner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bannerTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._contentContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._originalContentContainerHeight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._content, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueHitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fakeNextButtonContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fakeNextButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fakeNextButtonControllerIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fakeNextButtonLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._contentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsDialogueOnLastLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._currentDialogueLine, Variant.From(in _currentDialogueLine));
		info.AddProperty(PropertyName._ancientBgContainer, Variant.From(in _ancientBgContainer));
		info.AddProperty(PropertyName._ancientNameBanner, Variant.From(in _ancientNameBanner));
		info.AddProperty(PropertyName._bannerTween, Variant.From(in _bannerTween));
		info.AddProperty(PropertyName._contentContainer, Variant.From(in _contentContainer));
		info.AddProperty(PropertyName._originalContentContainerHeight, Variant.From(in _originalContentContainerHeight));
		info.AddProperty(PropertyName._content, Variant.From(in _content));
		info.AddProperty(PropertyName._dialogueContainer, Variant.From(in _dialogueContainer));
		info.AddProperty(PropertyName._dialogueHitbox, Variant.From(in _dialogueHitbox));
		info.AddProperty(PropertyName._fakeNextButtonContainer, Variant.From(in _fakeNextButtonContainer));
		info.AddProperty(PropertyName._fakeNextButton, Variant.From(in _fakeNextButton));
		info.AddProperty(PropertyName._fakeNextButtonControllerIcon, Variant.From(in _fakeNextButtonControllerIcon));
		info.AddProperty(PropertyName._fakeNextButtonLabel, Variant.From(in _fakeNextButtonLabel));
		info.AddProperty(PropertyName._contentTween, Variant.From(in _contentTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._currentDialogueLine, out var value))
		{
			_currentDialogueLine = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._ancientBgContainer, out var value2))
		{
			_ancientBgContainer = value2.As<NAncientBgContainer>();
		}
		if (info.TryGetProperty(PropertyName._ancientNameBanner, out var value3))
		{
			_ancientNameBanner = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bannerTween, out var value4))
		{
			_bannerTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._contentContainer, out var value5))
		{
			_contentContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._originalContentContainerHeight, out var value6))
		{
			_originalContentContainerHeight = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._content, out var value7))
		{
			_content = value7.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._dialogueContainer, out var value8))
		{
			_dialogueContainer = value8.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._dialogueHitbox, out var value9))
		{
			_dialogueHitbox = value9.As<NAncientDialogueHitbox>();
		}
		if (info.TryGetProperty(PropertyName._fakeNextButtonContainer, out var value10))
		{
			_fakeNextButtonContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._fakeNextButton, out var value11))
		{
			_fakeNextButton = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._fakeNextButtonControllerIcon, out var value12))
		{
			_fakeNextButtonControllerIcon = value12.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._fakeNextButtonLabel, out var value13))
		{
			_fakeNextButtonLabel = value13.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._contentTween, out var value14))
		{
			_contentTween = value14.As<Tween>();
		}
	}
}
