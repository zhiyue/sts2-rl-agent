using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NEpochInspectScreen.cs")]
public class NEpochInspectScreen : NClickableControl, IScreenContext
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName HidePaginators = "HidePaginators";

		public static readonly StringName Close = "Close";

		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnMouseReleased = "OnMouseReleased";

		public static readonly StringName SpeedUpTextAnimation = "SpeedUpTextAnimation";

		public static readonly StringName NextChapter = "NextChapter";

		public static readonly StringName PrevChapter = "PrevChapter";

		public static readonly StringName RefreshChapterPaginators = "RefreshChapterPaginators";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _closeButton = "_closeButton";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _portraitFlash = "_portraitFlash";

		public static readonly StringName _mask = "_mask";

		public static readonly StringName _chains = "_chains";

		public static readonly StringName _portraitHsv = "_portraitHsv";

		public static readonly StringName _fancyText = "_fancyText";

		public static readonly StringName _storyLabel = "_storyLabel";

		public static readonly StringName _chapterLabel = "_chapterLabel";

		public static readonly StringName _closeLabel = "_closeLabel";

		public static readonly StringName _placeholderLabel = "_placeholderLabel";

		public static readonly StringName _nextChapterButton = "_nextChapterButton";

		public static readonly StringName _prevChapterButton = "_prevChapterButton";

		public static readonly StringName _unlockInfo = "_unlockInfo";

		public static readonly StringName _hasStory = "_hasStory";

		public static readonly StringName _wasRevealed = "_wasRevealed";

		public static readonly StringName _prevChapterButtonOffsetX = "_prevChapterButtonOffsetX";

		public static readonly StringName _nextChapterButtonOffsetX = "_nextChapterButtonOffsetX";

		public static readonly StringName _maskOffsetX = "_maskOffsetX";

		public static readonly StringName _maskOffsetY = "_maskOffsetY";

		public static readonly StringName _closeButtonY = "_closeButtonY";

		public static readonly StringName _unlockTween = "_unlockTween";

		public static readonly StringName _buttonTween = "_buttonTween";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _textTween = "_textTween";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly LocString placeholderLoc = new LocString("timeline", "PLACEHOLDER_PORTRAIT");

	public static readonly string lockedImagePath = ImageHelper.GetImagePath("packed/timeline/epoch_slot_locked.png");

	private NButton _closeButton;

	private TextureRect _portrait;

	private TextureRect _portraitFlash;

	private TextureRect _mask;

	private NEpochChains _chains;

	private ShaderMaterial _portraitHsv;

	private MegaRichTextLabel _fancyText;

	private MegaLabel _storyLabel;

	private MegaLabel _chapterLabel;

	private MegaLabel _closeLabel;

	private MegaLabel _placeholderLabel;

	private NEpochPaginateButton _nextChapterButton;

	private NEpochPaginateButton _prevChapterButton;

	private NUnlockInfo _unlockInfo;

	private List<SerializableEpoch> _allEpochs;

	private EpochModel _epoch;

	private EpochModel? _prevChapterEpoch;

	private EpochModel? _nextChapterEpoch;

	private LocString _chapterLoc;

	private bool _hasStory;

	private bool _wasRevealed;

	private float _prevChapterButtonOffsetX;

	private float _nextChapterButtonOffsetX;

	private float _maskOffsetX;

	private float _maskOffsetY;

	private float _closeButtonY;

	private Tween? _unlockTween;

	private Tween? _buttonTween;

	private Tween? _tween;

	private Tween? _textTween;

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	public Control? DefaultFocusedControl => null;

	public override void _Ready()
	{
		_storyLabel = GetNode<MegaLabel>("%StoryLabel");
		_chapterLabel = GetNode<MegaLabel>("%ChapterLabel");
		_placeholderLabel = GetNode<MegaLabel>("%PlaceholderLabel");
		_portrait = GetNode<TextureRect>("%Portrait");
		_portraitFlash = GetNode<TextureRect>("%PortraitFlash");
		_mask = GetNode<TextureRect>("%Mask");
		_maskOffsetY = _mask.OffsetTop;
		_maskOffsetX = _mask.OffsetLeft;
		_chains = GetNode<NEpochChains>("%Chains");
		_portraitHsv = (ShaderMaterial)_portrait.Material;
		_closeButton = GetNode<NButton>("%CloseButton");
		_fancyText = GetNode<MegaRichTextLabel>("%FancyText");
		_closeLabel = GetNode<MegaLabel>("%CloseLabel");
		_chapterLoc = new LocString("timeline", "EPOCH_INSPECT.chapterFormat");
		_unlockInfo = GetNode<NUnlockInfo>("%UnlockInfo");
		_nextChapterButton = GetNode<NEpochPaginateButton>("%NextChapterButton");
		_prevChapterButton = GetNode<NEpochPaginateButton>("%PrevChapterButton");
		_prevChapterButtonOffsetX = _prevChapterButton.OffsetLeft;
		_nextChapterButtonOffsetX = _nextChapterButton.OffsetLeft;
		_nextChapterButton.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
		{
			NextChapter();
		}));
		_prevChapterButton.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
		{
			PrevChapter();
		}));
		Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(OnMouseReleased));
		_closeButton.Disable();
	}

	public async Task Open(NEpochSlot slot, EpochModel epoch, bool wasRevealed)
	{
		_buttonTween?.FastForwardToCompletion();
		_wasRevealed = wasRevealed;
		_epoch = epoch;
		if (_epoch.IsArtPlaceholder)
		{
			_placeholderLabel.Visible = true;
			_placeholderLabel.Text = placeholderLoc.GetRawText();
		}
		else
		{
			_placeholderLabel.Visible = false;
		}
		base.Modulate = Colors.White;
		_portrait.Texture = epoch.BigPortrait;
		_fancyText.Modulate = StsColors.transparentWhite;
		_fancyText.Text = epoch.Description;
		_hasStory = epoch.StoryTitle != null;
		SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_open_epoch");
		if (_hasStory)
		{
			_storyLabel.SetTextAutoSize(epoch.StoryTitle ?? string.Empty);
			_chapterLoc.Add("ChapterIndex", epoch.ChapterIndex);
			_chapterLoc.Add("ChapterName", epoch.Title);
			_chapterLabel.SetTextAutoSize(_chapterLoc.GetFormattedText());
			_chapterLabel.VerticalAlignment = VerticalAlignment.Center;
		}
		else
		{
			_storyLabel.SetTextAutoSize(string.Empty);
			_chapterLabel.SetTextAutoSize(epoch.Title.GetFormattedText());
			_chapterLabel.VerticalAlignment = VerticalAlignment.Bottom;
			_nextChapterButton.Disable();
			_prevChapterButton.Disable();
		}
		_closeButton.MouseFilter = MouseFilterEnum.Stop;
		_closeButton.Scale = Vector2.One;
		_closeButtonY = _closeButton.Position.Y + 180f;
		_closeButton.Position = new Vector2(_closeButton.Position.X, _closeButtonY);
		_closeButton.Modulate = StsColors.transparentWhite;
		_closeButton.Disable();
		NTimelineScreen.Instance.ShowBackstopAndHideUi();
		base.Visible = true;
		Vector2 size = _mask.Size;
		_mask.GlobalPosition = slot.GlobalPosition;
		_mask.SetDeferred(Control.PropertyName.Size, slot.Size * slot.GetGlobalTransform().Scale);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_mask, "offset_left", _maskOffsetX, 0.4).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_mask, "offset_top", _maskOffsetY, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_mask, "size", size, 0.4).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_storyLabel.Modulate = StsColors.transparentWhite;
		_chapterLabel.Modulate = StsColors.transparentWhite;
		_nextChapterButton.Modulate = StsColors.transparentWhite;
		_prevChapterButton.Modulate = StsColors.transparentWhite;
		_tween.TweenProperty(_storyLabel, "modulate:a", 1f, 0.5).SetDelay(0.4);
		_tween.TweenProperty(_chapterLabel, "modulate:a", 1f, 0.5).SetDelay(0.2);
		_tween.TweenProperty(_prevChapterButton, "offset_left", _prevChapterButtonOffsetX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_prevChapterButtonOffsetX + 100f)
			.SetDelay(0.25);
		_tween.TweenProperty(_prevChapterButton, "modulate:a", 1f, 0.25).SetDelay(0.25);
		_tween.TweenProperty(_nextChapterButton, "offset_left", _nextChapterButtonOffsetX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_nextChapterButtonOffsetX - 100f)
			.SetDelay(0.25);
		_tween.TweenProperty(_nextChapterButton, "modulate:a", 1f, 0.25).SetDelay(0.25);
		if (wasRevealed)
		{
			await TaskHelper.RunSafely(UnlockAnimation(epoch));
			return;
		}
		_closeLabel.SetTextAutoSize(new LocString("timeline", "EPOCH_INSPECT.closeButton").GetRawText());
		_fancyText.Text = epoch.Description;
		_textTween?.Kill();
		_textTween = CreateTween().SetParallel();
		_textTween.TweenProperty(_fancyText, "modulate:a", 1f, 0.5).SetDelay(0.1);
		_fancyText.VisibleRatio = 1f;
		_buttonTween?.Kill();
		_buttonTween = CreateTween().SetParallel();
		_buttonTween.TweenProperty(_closeButton, "modulate:a", 1f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(0.1);
		_buttonTween.TweenProperty(_closeButton, "position:y", _closeButtonY - 180f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.SetDelay(0.1);
		_buttonTween.TweenCallback(Callable.From(_closeButton.Enable));
		RefreshChapterPaginators();
		_unlockInfo.AnimIn(epoch.UnlockText);
	}

	private void HidePaginators()
	{
		_hasStory = false;
		_nextChapterButton.Visible = false;
		_prevChapterButton.Visible = false;
	}

	private void OpenViaPaginator(EpochModel epoch)
	{
		_epoch = epoch;
		base.Modulate = Colors.White;
		_fancyText.Text = epoch.Description;
		_portrait.Texture = epoch.BigPortrait;
		_hasStory = epoch.StoryTitle != null;
		_storyLabel.Modulate = Colors.White;
		_chapterLabel.Modulate = Colors.White;
		if (_hasStory)
		{
			_storyLabel.SetTextAutoSize(epoch.StoryTitle ?? string.Empty);
			_chapterLoc.Add("ChapterIndex", epoch.ChapterIndex);
			_chapterLoc.Add("ChapterName", epoch.Title);
			_chapterLabel.SetTextAutoSize(_chapterLoc.GetFormattedText());
			_chapterLabel.VerticalAlignment = VerticalAlignment.Center;
			_nextChapterButton.Modulate = Colors.White;
			_prevChapterButton.Modulate = Colors.White;
		}
		else
		{
			_storyLabel.SetTextAutoSize(string.Empty);
			_chapterLabel.SetTextAutoSize(epoch.Title.GetFormattedText());
			_chapterLabel.VerticalAlignment = VerticalAlignment.Bottom;
			_nextChapterButton.Visible = false;
			_prevChapterButton.Visible = false;
		}
		_fancyText.Modulate = StsColors.transparentWhite;
		NTimelineScreen.Instance.ShowBackstopAndHideUi();
		base.Visible = true;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_textTween?.Kill();
		_textTween = CreateTween().SetParallel();
		_tween.TweenProperty(_mask, "offset_top", _maskOffsetY, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_textTween.TweenProperty(_fancyText, "modulate:a", 1f, 0.5).SetDelay(0.1);
		_fancyText.VisibleRatio = 1f;
		TaskHelper.RunSafely(_unlockInfo.AnimInViaPaginator(epoch.UnlockText));
	}

	public void Close()
	{
		if (NTimelineScreen.Instance.IsScreenQueued())
		{
			NTimelineScreen.Instance.OpenQueuedScreen();
		}
		else
		{
			NTimelineScreen.Instance.EnableInput();
			NTimelineScreen.Instance.HideBackstopAndShowUi(showBackButton: true);
		}
		base.FocusMode = FocusModeEnum.None;
		_buttonTween?.FastForwardToCompletion();
		_unlockTween?.FastForwardToCompletion();
		_tween?.FastForwardToCompletion();
		_textTween?.FastForwardToCompletion();
		_buttonTween = CreateTween().SetParallel();
		_buttonTween.TweenProperty(_closeButton, "scale", new Vector2(3f, 0.1f), 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_buttonTween.TweenProperty(_closeButton, "modulate:a", 0f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_buttonTween.TweenProperty(this, "modulate", new Color(0f, 0f, 0f, 0f), 0.5);
		_buttonTween.TweenCallback(Callable.From(delegate
		{
			base.Visible = false;
			_closeButton.Disable();
			if (_wasRevealed)
			{
				AchievementsHelper.CheckTimelineComplete();
			}
		}));
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.right, NextChapter);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.left, PrevChapter);
	}

	public async Task UnlockAnimation(EpochModel epoch)
	{
		HidePaginators();
		epoch.QueueUnlocks();
		SaveManager.Instance.SaveProgressFile();
		_unlockInfo.HideImmediately();
		_closeLabel.SetTextAutoSize(new LocString("timeline", "EPOCH_INSPECT.continueButton").GetRawText());
		_fancyText.VisibleRatio = 0f;
		_fancyText.Modulate = StsColors.transparentWhite;
		_portraitHsv.SetShaderParameter(_s, 0f);
		_portraitHsv.SetShaderParameter(_v, 0.75f);
		_chains.Texture = PreloadManager.Cache.GetTexture2D(lockedImagePath);
		_chains.Visible = true;
		_chains.Modulate = Colors.White;
		_chains.SelfModulate = Colors.White;
		_portraitFlash.Modulate = new Color(1f, 1f, 1f, 0f);
		_unlockTween?.Kill();
		_unlockTween = CreateTween().SetParallel();
		_unlockTween.TweenProperty(_chains, "scale", Vector2.One * 0.98f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.SetDelay(0.5);
		await ToSignal(_unlockTween, Tween.SignalName.Finished);
		_chains.Unlock();
		await ToSignal(_chains, NEpochChains.SignalName.OnAnimationFinished);
		_portraitFlash.Modulate = Colors.White;
		_unlockTween = CreateTween().SetParallel();
		_unlockTween.TweenProperty(_portraitFlash, "modulate:a", 0f, 0.5);
		_unlockTween.TweenMethod(Callable.From<float>(UpdateShaderS), _portraitHsv.GetShaderParameter(_s), 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_unlockTween.TweenMethod(Callable.From<float>(UpdateShaderV), _portraitHsv.GetShaderParameter(_v), 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_textTween?.Kill();
		_textTween = CreateTween().SetParallel();
		_textTween.TweenProperty(_fancyText, "modulate:a", 1f, 2.0).SetDelay(0.25);
		_textTween.TweenProperty(_fancyText, "visible_ratio", 1f, (double)_fancyText.GetTotalCharacterCount() * 0.015).SetDelay(0.5);
		_buttonTween?.Kill();
		_buttonTween = CreateTween().SetParallel();
		_buttonTween.TweenProperty(_closeButton, "modulate:a", 1f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(1.0);
		_buttonTween.TweenProperty(_closeButton, "position:y", _closeButtonY - 180f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.SetDelay(1.0);
		_buttonTween.TweenCallback(Callable.From(_closeButton.Enable));
		await ToSignal(_unlockTween, Tween.SignalName.Finished);
	}

	private void UpdateShaderS(float value)
	{
		_portraitHsv.SetShaderParameter(_s, value);
	}

	private void UpdateShaderV(float value)
	{
		_portraitHsv.SetShaderParameter(_v, value);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed(MegaInput.select) || inputEvent.IsActionPressed(MegaInput.accept))
		{
			SpeedUpTextAnimation();
		}
	}

	private void OnMouseReleased(InputEvent obj)
	{
		SpeedUpTextAnimation();
	}

	private void SpeedUpTextAnimation()
	{
		if (_textTween != null && _textTween.IsRunning())
		{
			_textTween.Kill();
			_fancyText.Modulate = Colors.White;
			_fancyText.VisibleRatio = 1f;
		}
	}

	private void NextChapter()
	{
		OpenViaPaginator(_nextChapterEpoch);
		RefreshChapterPaginators();
	}

	private void PrevChapter()
	{
		OpenViaPaginator(_prevChapterEpoch);
		RefreshChapterPaginators();
	}

	private void RefreshChapterPaginators()
	{
		if (_hasStory)
		{
			_nextChapterEpoch = StoryModel.NextChapter(_epoch);
			_prevChapterEpoch = StoryModel.PrevChapter(_epoch);
			if (_nextChapterEpoch != null)
			{
				NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.right, NextChapter);
				_nextChapterButton.Visible = true;
			}
			else
			{
				_nextChapterButton.Visible = false;
			}
			if (_prevChapterEpoch != null)
			{
				NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.left, PrevChapter);
				_prevChapterButton.Visible = true;
			}
			else
			{
				_prevChapterButton.Visible = false;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HidePaginators, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderS, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SpeedUpTextAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.NextChapter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PrevChapter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshChapterPaginators, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.HidePaginators && args.Count == 0)
		{
			HidePaginators();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderS && args.Count == 1)
		{
			UpdateShaderS(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseReleased && args.Count == 1)
		{
			OnMouseReleased(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpeedUpTextAnimation && args.Count == 0)
		{
			SpeedUpTextAnimation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NextChapter && args.Count == 0)
		{
			NextChapter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PrevChapter && args.Count == 0)
		{
			PrevChapter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshChapterPaginators && args.Count == 0)
		{
			RefreshChapterPaginators();
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
		if (method == MethodName.HidePaginators)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderS)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnMouseReleased)
		{
			return true;
		}
		if (method == MethodName.SpeedUpTextAnimation)
		{
			return true;
		}
		if (method == MethodName.NextChapter)
		{
			return true;
		}
		if (method == MethodName.PrevChapter)
		{
			return true;
		}
		if (method == MethodName.RefreshChapterPaginators)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._closeButton)
		{
			_closeButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._portraitFlash)
		{
			_portraitFlash = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._mask)
		{
			_mask = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._chains)
		{
			_chains = VariantUtils.ConvertTo<NEpochChains>(in value);
			return true;
		}
		if (name == PropertyName._portraitHsv)
		{
			_portraitHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._fancyText)
		{
			_fancyText = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._storyLabel)
		{
			_storyLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._chapterLabel)
		{
			_chapterLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._closeLabel)
		{
			_closeLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._placeholderLabel)
		{
			_placeholderLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._nextChapterButton)
		{
			_nextChapterButton = VariantUtils.ConvertTo<NEpochPaginateButton>(in value);
			return true;
		}
		if (name == PropertyName._prevChapterButton)
		{
			_prevChapterButton = VariantUtils.ConvertTo<NEpochPaginateButton>(in value);
			return true;
		}
		if (name == PropertyName._unlockInfo)
		{
			_unlockInfo = VariantUtils.ConvertTo<NUnlockInfo>(in value);
			return true;
		}
		if (name == PropertyName._hasStory)
		{
			_hasStory = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._wasRevealed)
		{
			_wasRevealed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._prevChapterButtonOffsetX)
		{
			_prevChapterButtonOffsetX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._nextChapterButtonOffsetX)
		{
			_nextChapterButtonOffsetX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maskOffsetX)
		{
			_maskOffsetX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maskOffsetY)
		{
			_maskOffsetY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._closeButtonY)
		{
			_closeButtonY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._unlockTween)
		{
			_unlockTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._buttonTween)
		{
			_buttonTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._textTween)
		{
			_textTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._closeButton)
		{
			value = VariantUtils.CreateFrom(in _closeButton);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
		if (name == PropertyName._portraitFlash)
		{
			value = VariantUtils.CreateFrom(in _portraitFlash);
			return true;
		}
		if (name == PropertyName._mask)
		{
			value = VariantUtils.CreateFrom(in _mask);
			return true;
		}
		if (name == PropertyName._chains)
		{
			value = VariantUtils.CreateFrom(in _chains);
			return true;
		}
		if (name == PropertyName._portraitHsv)
		{
			value = VariantUtils.CreateFrom(in _portraitHsv);
			return true;
		}
		if (name == PropertyName._fancyText)
		{
			value = VariantUtils.CreateFrom(in _fancyText);
			return true;
		}
		if (name == PropertyName._storyLabel)
		{
			value = VariantUtils.CreateFrom(in _storyLabel);
			return true;
		}
		if (name == PropertyName._chapterLabel)
		{
			value = VariantUtils.CreateFrom(in _chapterLabel);
			return true;
		}
		if (name == PropertyName._closeLabel)
		{
			value = VariantUtils.CreateFrom(in _closeLabel);
			return true;
		}
		if (name == PropertyName._placeholderLabel)
		{
			value = VariantUtils.CreateFrom(in _placeholderLabel);
			return true;
		}
		if (name == PropertyName._nextChapterButton)
		{
			value = VariantUtils.CreateFrom(in _nextChapterButton);
			return true;
		}
		if (name == PropertyName._prevChapterButton)
		{
			value = VariantUtils.CreateFrom(in _prevChapterButton);
			return true;
		}
		if (name == PropertyName._unlockInfo)
		{
			value = VariantUtils.CreateFrom(in _unlockInfo);
			return true;
		}
		if (name == PropertyName._hasStory)
		{
			value = VariantUtils.CreateFrom(in _hasStory);
			return true;
		}
		if (name == PropertyName._wasRevealed)
		{
			value = VariantUtils.CreateFrom(in _wasRevealed);
			return true;
		}
		if (name == PropertyName._prevChapterButtonOffsetX)
		{
			value = VariantUtils.CreateFrom(in _prevChapterButtonOffsetX);
			return true;
		}
		if (name == PropertyName._nextChapterButtonOffsetX)
		{
			value = VariantUtils.CreateFrom(in _nextChapterButtonOffsetX);
			return true;
		}
		if (name == PropertyName._maskOffsetX)
		{
			value = VariantUtils.CreateFrom(in _maskOffsetX);
			return true;
		}
		if (name == PropertyName._maskOffsetY)
		{
			value = VariantUtils.CreateFrom(in _maskOffsetY);
			return true;
		}
		if (name == PropertyName._closeButtonY)
		{
			value = VariantUtils.CreateFrom(in _closeButtonY);
			return true;
		}
		if (name == PropertyName._unlockTween)
		{
			value = VariantUtils.CreateFrom(in _unlockTween);
			return true;
		}
		if (name == PropertyName._buttonTween)
		{
			value = VariantUtils.CreateFrom(in _buttonTween);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._textTween)
		{
			value = VariantUtils.CreateFrom(in _textTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portraitFlash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chains, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portraitHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fancyText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._storyLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chapterLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._placeholderLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nextChapterButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._prevChapterButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlockInfo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hasStory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._wasRevealed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._prevChapterButtonOffsetX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._nextChapterButtonOffsetX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maskOffsetX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maskOffsetY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._closeButtonY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unlockTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._closeButton, Variant.From(in _closeButton));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._portraitFlash, Variant.From(in _portraitFlash));
		info.AddProperty(PropertyName._mask, Variant.From(in _mask));
		info.AddProperty(PropertyName._chains, Variant.From(in _chains));
		info.AddProperty(PropertyName._portraitHsv, Variant.From(in _portraitHsv));
		info.AddProperty(PropertyName._fancyText, Variant.From(in _fancyText));
		info.AddProperty(PropertyName._storyLabel, Variant.From(in _storyLabel));
		info.AddProperty(PropertyName._chapterLabel, Variant.From(in _chapterLabel));
		info.AddProperty(PropertyName._closeLabel, Variant.From(in _closeLabel));
		info.AddProperty(PropertyName._placeholderLabel, Variant.From(in _placeholderLabel));
		info.AddProperty(PropertyName._nextChapterButton, Variant.From(in _nextChapterButton));
		info.AddProperty(PropertyName._prevChapterButton, Variant.From(in _prevChapterButton));
		info.AddProperty(PropertyName._unlockInfo, Variant.From(in _unlockInfo));
		info.AddProperty(PropertyName._hasStory, Variant.From(in _hasStory));
		info.AddProperty(PropertyName._wasRevealed, Variant.From(in _wasRevealed));
		info.AddProperty(PropertyName._prevChapterButtonOffsetX, Variant.From(in _prevChapterButtonOffsetX));
		info.AddProperty(PropertyName._nextChapterButtonOffsetX, Variant.From(in _nextChapterButtonOffsetX));
		info.AddProperty(PropertyName._maskOffsetX, Variant.From(in _maskOffsetX));
		info.AddProperty(PropertyName._maskOffsetY, Variant.From(in _maskOffsetY));
		info.AddProperty(PropertyName._closeButtonY, Variant.From(in _closeButtonY));
		info.AddProperty(PropertyName._unlockTween, Variant.From(in _unlockTween));
		info.AddProperty(PropertyName._buttonTween, Variant.From(in _buttonTween));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._textTween, Variant.From(in _textTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._closeButton, out var value))
		{
			_closeButton = value.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value2))
		{
			_portrait = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._portraitFlash, out var value3))
		{
			_portraitFlash = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._mask, out var value4))
		{
			_mask = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._chains, out var value5))
		{
			_chains = value5.As<NEpochChains>();
		}
		if (info.TryGetProperty(PropertyName._portraitHsv, out var value6))
		{
			_portraitHsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._fancyText, out var value7))
		{
			_fancyText = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._storyLabel, out var value8))
		{
			_storyLabel = value8.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._chapterLabel, out var value9))
		{
			_chapterLabel = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._closeLabel, out var value10))
		{
			_closeLabel = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._placeholderLabel, out var value11))
		{
			_placeholderLabel = value11.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._nextChapterButton, out var value12))
		{
			_nextChapterButton = value12.As<NEpochPaginateButton>();
		}
		if (info.TryGetProperty(PropertyName._prevChapterButton, out var value13))
		{
			_prevChapterButton = value13.As<NEpochPaginateButton>();
		}
		if (info.TryGetProperty(PropertyName._unlockInfo, out var value14))
		{
			_unlockInfo = value14.As<NUnlockInfo>();
		}
		if (info.TryGetProperty(PropertyName._hasStory, out var value15))
		{
			_hasStory = value15.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._wasRevealed, out var value16))
		{
			_wasRevealed = value16.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._prevChapterButtonOffsetX, out var value17))
		{
			_prevChapterButtonOffsetX = value17.As<float>();
		}
		if (info.TryGetProperty(PropertyName._nextChapterButtonOffsetX, out var value18))
		{
			_nextChapterButtonOffsetX = value18.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maskOffsetX, out var value19))
		{
			_maskOffsetX = value19.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maskOffsetY, out var value20))
		{
			_maskOffsetY = value20.As<float>();
		}
		if (info.TryGetProperty(PropertyName._closeButtonY, out var value21))
		{
			_closeButtonY = value21.As<float>();
		}
		if (info.TryGetProperty(PropertyName._unlockTween, out var value22))
		{
			_unlockTween = value22.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._buttonTween, out var value23))
		{
			_buttonTween = value23.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value24))
		{
			_tween = value24.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._textTween, out var value25))
		{
			_textTween = value25.As<Tween>();
		}
	}
}
