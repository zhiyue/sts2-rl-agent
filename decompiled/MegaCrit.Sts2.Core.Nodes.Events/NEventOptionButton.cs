using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NEventOptionButton.cs")]
public class NEventOptionButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetVisuallyLocked = "SetVisuallyLocked";

		public static readonly StringName AnimateIn = "AnimateIn";

		public static readonly StringName EnableButton = "EnableButton";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName GrayOut = "GrayOut";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";

		public static readonly StringName RefreshVotes = "RefreshVotes";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName WillKillPlayer = "WillKillPlayer";

		public static readonly StringName ShowPersistentKillGlow = "ShowPersistentKillGlow";

		public static readonly StringName PulseKillGlow = "PulseKillGlow";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName Index = "Index";

		public static readonly StringName VoteContainer = "VoteContainer";

		public static readonly StringName _label = "_label";

		public static readonly StringName _image = "_image";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _killGlow = "_killGlow";

		public static readonly StringName _confirmFlash = "_confirmFlash";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _playerVoteContainer = "_playerVoteContainer";

		public static readonly StringName _animInTween = "_animInTween";

		public static readonly StringName _flashTween = "_flashTween";

		public static readonly StringName _killGlowTween = "_killGlowTween";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _buttonColor = "_buttonColor";

		public static readonly StringName _deathPreventionVfx = "_deathPreventionVfx";

		public static readonly StringName _deathPreventionVfxPosition = "_deathPreventionVfxPosition";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private static readonly string _voteIconPath = SceneHelper.GetScenePath("ui/multiplayer_vote_icon");

	private MegaRichTextLabel _label;

	private NinePatchRect _image;

	private NinePatchRect _outline;

	private NinePatchRect _killGlow;

	private NinePatchRect _confirmFlash;

	private ShaderMaterial? _hsv;

	private NMultiplayerVoteContainer _playerVoteContainer;

	private Tween? _animInTween;

	private Tween? _flashTween;

	private Tween? _killGlowTween;

	private Tween? _tween;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.01f;

	private static readonly Vector2 _pressScale = Vector2.One * 0.99f;

	private const float _defaultV = 0.9f;

	private const float _hoverV = 1.2f;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private Color _buttonColor;

	private NThoughtBubbleVfx? _deathPreventionVfx;

	private CancellationTokenSource? _deathPreventionCancellation;

	private Vector2 _deathPreventionVfxPosition;

	public EventModel Event { get; private set; }

	public EventOption Option { get; private set; }

	private int Index { get; set; }

	public NMultiplayerVoteContainer VoteContainer => _playerVoteContainer;

	private static string ScenePath => SceneHelper.GetScenePath("events/event_option_button");

	private static string AncientScenePath => SceneHelper.GetScenePath("events/ancient_event_option_button");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { ScenePath, AncientScenePath, _voteIconPath });

	public static NEventOptionButton Create(EventModel eventModel, EventOption option, int index)
	{
		NEventOptionButton nEventOptionButton = ((eventModel is AncientEventModel) ? PreloadManager.Cache.GetScene(AncientScenePath).Instantiate<NEventOptionButton>(PackedScene.GenEditState.Disabled) : PreloadManager.Cache.GetScene(ScenePath).Instantiate<NEventOptionButton>(PackedScene.GenEditState.Disabled));
		nEventOptionButton.Event = eventModel;
		nEventOptionButton.Option = option;
		nEventOptionButton.Index = index;
		nEventOptionButton._buttonColor = eventModel.ButtonColor;
		return nEventOptionButton;
	}

	public override void _Ready()
	{
		ConnectSignals();
		Event.DynamicVars.AddTo(Option.Description);
		Event.DynamicVars.AddTo(Option.Title);
		string formattedText = Option.Title.GetFormattedText();
		string formattedText2 = Option.Description.GetFormattedText();
		_label = GetNode<MegaRichTextLabel>("%Text");
		if (string.IsNullOrEmpty(formattedText))
		{
			_label.Text = formattedText2;
		}
		else
		{
			string value = (Option.IsLocked ? "red" : "gold");
			_label.Text = $"[{value}][b]{formattedText}[/b][/{value}]\n{formattedText2}";
		}
		_image = GetNode<NinePatchRect>("Image");
		_image.Modulate = _buttonColor;
		_hsv = (ShaderMaterial)_image.Material;
		_outline = GetNode<NinePatchRect>("Outline");
		_killGlow = GetNode<NinePatchRect>("RedFlash");
		_confirmFlash = GetNode<NinePatchRect>("BlueFlash");
		_playerVoteContainer = GetNode<NMultiplayerVoteContainer>("PlayerVoteContainer");
		_playerVoteContainer.Initialize(ShouldDisplayPlayerVote, Event.Owner.RunState.Players);
		if (Event is AncientEventModel && Option.Relic != null)
		{
			TextureRect node = GetNode<TextureRect>("%RelicIcon");
			node.SetTexture(Option.Relic.Icon);
			node.GetNode<TextureRect>("%Outline").SetTexture(Option.Relic.IconOutline);
			node.Visible = true;
		}
		if (Option.IsLocked)
		{
			SetVisuallyLocked();
		}
		else if (WillKillPlayer())
		{
			ShowPersistentKillGlow();
		}
	}

	private void SetVisuallyLocked()
	{
		_label.Modulate = new Color(_label.Modulate.R, _label.Modulate.G, _label.Modulate.B, 0.7f);
		_hsv?.SetShaderParameter(_h, 0.48f);
		_hsv?.SetShaderParameter(_s, 0.2f);
		_hsv?.SetShaderParameter(_v, 0.65f);
	}

	public void AnimateIn()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
			{
				EnableButton();
				return;
			}
			base.Modulate = StsColors.transparentWhite;
			bool flag = SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast;
			_animInTween = CreateTween().SetParallel();
			_animInTween.TweenInterval(flag ? 0.25 : 0.5);
			_animInTween.Chain();
			_animInTween.TweenInterval((flag ? 0.1 : 0.2) * (double)Index);
			_animInTween.Chain();
			_animInTween.TweenProperty(this, "position", base.Position, flag ? 0.25 : 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
				.From(base.Position + new Vector2(-60f, 0f));
			_animInTween.TweenProperty(this, "modulate", Colors.White, flag ? 0.25 : 0.5);
			_animInTween.Finished += EnableButton;
		}
	}

	public void EnableButton()
	{
		base.MouseFilter = MouseFilterEnum.Stop;
	}

	protected override void OnRelease()
	{
		if (Option.IsLocked)
		{
			return;
		}
		if (WillKillPlayer())
		{
			if (Event.Owner.RunState.Players.Count > 1)
			{
				if (_deathPreventionVfx == null)
				{
					LocString eventDeathPreventionLine = Event.Owner.Character.EventDeathPreventionLine;
					_deathPreventionVfx = NThoughtBubbleVfx.Create(eventDeathPreventionLine.GetFormattedText(), DialogueSide.Right, null);
					NEventRoom.Instance?.VfxContainer?.AddChildSafely(_deathPreventionVfx);
					_deathPreventionVfxPosition = base.GlobalPosition + new Vector2(-50f, -15f);
					_deathPreventionVfx.GlobalPosition = _deathPreventionVfxPosition;
				}
				else
				{
					_deathPreventionCancellation?.Cancel();
				}
				TaskHelper.RunSafely(RumbleDeathVfx());
				TaskHelper.RunSafely(ExpireDeathPreventionVfx());
				return;
			}
			ShowPersistentKillGlow();
		}
		NEventRoom.Instance.OptionButtonClicked(Option, Index);
	}

	private async Task RumbleDeathVfx()
	{
		ScreenRumbleInstance rumble = new ScreenRumbleInstance(20f, 0.30000001192092896, 10f, RumbleStyle.Rumble);
		while (!rumble.IsDone)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			Vector2 vector = rumble.Update(GetProcessDeltaTime());
			_deathPreventionVfx.GlobalPosition = _deathPreventionVfxPosition + vector;
		}
	}

	private async Task ExpireDeathPreventionVfx()
	{
		_deathPreventionCancellation = new CancellationTokenSource();
		await Cmd.Wait(2.5f, _deathPreventionCancellation.Token, ignoreCombatEnd: true);
		if (!_deathPreventionCancellation.IsCancellationRequested)
		{
			_deathPreventionVfx?.GoAway();
			_deathPreventionVfx = null;
		}
	}

	protected override void OnPress()
	{
		if (!Option.IsLocked)
		{
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "scale", _pressScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_tween.TweenMethod(Callable.From<float>(UpdateShaderParam), 1.2f, 0.9f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			base.OnPress();
		}
	}

	protected override void OnFocus()
	{
		if (!Option.IsLocked)
		{
			base.OnFocus();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "scale", _hoverScale, 0.05);
			_tween.TweenProperty(_outline, "modulate", StsColors.blueGlow, 0.05);
			_hsv?.SetShaderParameter(_v, 1.2f);
			NHoverTipSet.CreateAndShow(this, Option.HoverTips, (Event.LayoutType != EventLayoutType.Combat) ? HoverTipAlignment.Left : HoverTipAlignment.Right);
			if (WillKillPlayer())
			{
				PulseKillGlow();
			}
		}
	}

	protected override void OnUnfocus()
	{
		if (!Option.IsLocked)
		{
			base.OnUnfocus();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_tween.TweenMethod(Callable.From<float>(UpdateShaderParam), 1.2f, 0.9f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_tween.TweenProperty(_outline, "modulate:a", 0f, 0.3);
			NHoverTipSet.Remove(this);
			if (WillKillPlayer())
			{
				ShowPersistentKillGlow();
			}
		}
	}

	public async Task FlashConfirmation()
	{
		_flashTween?.Kill();
		NinePatchRect confirmFlash = _confirmFlash;
		Color modulate = _confirmFlash.Modulate;
		modulate.A = 0f;
		confirmFlash.Modulate = modulate;
		_flashTween = CreateTween();
		_flashTween.TweenProperty(_confirmFlash, "modulate:a", 0.8f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_flashTween.Parallel().TweenProperty(_confirmFlash, "scale", new Vector2(1.03f, 1.1f), 0.25).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		_flashTween.TweenProperty(_confirmFlash, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_flashTween.TweenProperty(_confirmFlash, "modulate:a", 0f, 0.3);
		await ToSignal(_flashTween, Tween.SignalName.Finished);
		await Cmd.Wait(0.5f);
	}

	public void GrayOut()
	{
		_flashTween?.Kill();
		_flashTween = CreateTween();
		Tween? flashTween = _flashTween;
		NodePath property = "modulate";
		Color lightGray = StsColors.lightGray;
		lightGray.A = 0.5f;
		flashTween.TweenProperty(this, property, lightGray, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderParam(float newV)
	{
		_hsv?.SetShaderParameter(_v, newV);
	}

	private bool ShouldDisplayPlayerVote(Player player)
	{
		return RunManager.Instance.EventSynchronizer.GetPlayerVote(player) == Index;
	}

	public void RefreshVotes()
	{
		_playerVoteContainer.RefreshPlayerVotes();
	}

	public override void _ExitTree()
	{
		_cancelToken.Cancel();
		if (_animInTween != null)
		{
			_animInTween.Finished -= EnableButton;
		}
	}

	private bool WillKillPlayer()
	{
		if (Event.Owner != null)
		{
			return Option.WillKillPlayer?.Invoke(Event.Owner) ?? false;
		}
		return false;
	}

	private void ShowPersistentKillGlow()
	{
		_killGlowTween?.Kill();
		NinePatchRect killGlow = _killGlow;
		Color modulate = _killGlow.Modulate;
		modulate.A = 0.5f;
		killGlow.Modulate = modulate;
	}

	private void PulseKillGlow()
	{
		_killGlowTween?.Kill();
		NinePatchRect killGlow = _killGlow;
		Color modulate = _killGlow.Modulate;
		modulate.A = 0.25f;
		killGlow.Modulate = modulate;
		_killGlowTween = CreateTween();
		_killGlowTween.TweenProperty(_killGlow, "modulate:a", 1f, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_killGlowTween.TweenProperty(_killGlow, "modulate:a", 0.25f, 0.8);
		_killGlowTween.SetLoops();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetVisuallyLocked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GrayOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "newV", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.WillKillPlayer, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowPersistentKillGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PulseKillGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetVisuallyLocked && args.Count == 0)
		{
			SetVisuallyLocked();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableButton && args.Count == 0)
		{
			EnableButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
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
		if (method == MethodName.GrayOut && args.Count == 0)
		{
			GrayOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderParam && args.Count == 1)
		{
			UpdateShaderParam(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshVotes && args.Count == 0)
		{
			RefreshVotes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WillKillPlayer && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(WillKillPlayer());
			return true;
		}
		if (method == MethodName.ShowPersistentKillGlow && args.Count == 0)
		{
			ShowPersistentKillGlow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PulseKillGlow && args.Count == 0)
		{
			PulseKillGlow();
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
		if (method == MethodName.SetVisuallyLocked)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.EnableButton)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnPress)
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
		if (method == MethodName.GrayOut)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		if (method == MethodName.RefreshVotes)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.WillKillPlayer)
		{
			return true;
		}
		if (method == MethodName.ShowPersistentKillGlow)
		{
			return true;
		}
		if (method == MethodName.PulseKillGlow)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			Index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName._killGlow)
		{
			_killGlow = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName._confirmFlash)
		{
			_confirmFlash = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._playerVoteContainer)
		{
			_playerVoteContainer = VariantUtils.ConvertTo<NMultiplayerVoteContainer>(in value);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			_animInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._flashTween)
		{
			_flashTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._killGlowTween)
		{
			_killGlowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._buttonColor)
		{
			_buttonColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._deathPreventionVfx)
		{
			_deathPreventionVfx = VariantUtils.ConvertTo<NThoughtBubbleVfx>(in value);
			return true;
		}
		if (name == PropertyName._deathPreventionVfxPosition)
		{
			_deathPreventionVfxPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Index)
		{
			value = VariantUtils.CreateFrom<int>(Index);
			return true;
		}
		if (name == PropertyName.VoteContainer)
		{
			value = VariantUtils.CreateFrom<NMultiplayerVoteContainer>(VoteContainer);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._killGlow)
		{
			value = VariantUtils.CreateFrom(in _killGlow);
			return true;
		}
		if (name == PropertyName._confirmFlash)
		{
			value = VariantUtils.CreateFrom(in _confirmFlash);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._playerVoteContainer)
		{
			value = VariantUtils.CreateFrom(in _playerVoteContainer);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			value = VariantUtils.CreateFrom(in _animInTween);
			return true;
		}
		if (name == PropertyName._flashTween)
		{
			value = VariantUtils.CreateFrom(in _flashTween);
			return true;
		}
		if (name == PropertyName._killGlowTween)
		{
			value = VariantUtils.CreateFrom(in _killGlowTween);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._buttonColor)
		{
			value = VariantUtils.CreateFrom(in _buttonColor);
			return true;
		}
		if (name == PropertyName._deathPreventionVfx)
		{
			value = VariantUtils.CreateFrom(in _deathPreventionVfx);
			return true;
		}
		if (name == PropertyName._deathPreventionVfxPosition)
		{
			value = VariantUtils.CreateFrom(in _deathPreventionVfxPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._killGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmFlash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerVoteContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flashTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._killGlowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VoteContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._buttonColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathPreventionVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._deathPreventionVfxPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Index, Variant.From<int>(Index));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._killGlow, Variant.From(in _killGlow));
		info.AddProperty(PropertyName._confirmFlash, Variant.From(in _confirmFlash));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._playerVoteContainer, Variant.From(in _playerVoteContainer));
		info.AddProperty(PropertyName._animInTween, Variant.From(in _animInTween));
		info.AddProperty(PropertyName._flashTween, Variant.From(in _flashTween));
		info.AddProperty(PropertyName._killGlowTween, Variant.From(in _killGlowTween));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._buttonColor, Variant.From(in _buttonColor));
		info.AddProperty(PropertyName._deathPreventionVfx, Variant.From(in _deathPreventionVfx));
		info.AddProperty(PropertyName._deathPreventionVfxPosition, Variant.From(in _deathPreventionVfxPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Index, out var value))
		{
			Index = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value3))
		{
			_image = value3.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value4))
		{
			_outline = value4.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName._killGlow, out var value5))
		{
			_killGlow = value5.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName._confirmFlash, out var value6))
		{
			_confirmFlash = value6.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._playerVoteContainer, out var value8))
		{
			_playerVoteContainer = value8.As<NMultiplayerVoteContainer>();
		}
		if (info.TryGetProperty(PropertyName._animInTween, out var value9))
		{
			_animInTween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._flashTween, out var value10))
		{
			_flashTween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._killGlowTween, out var value11))
		{
			_killGlowTween = value11.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value12))
		{
			_tween = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._buttonColor, out var value13))
		{
			_buttonColor = value13.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._deathPreventionVfx, out var value14))
		{
			_deathPreventionVfx = value14.As<NThoughtBubbleVfx>();
		}
		if (info.TryGetProperty(PropertyName._deathPreventionVfxPosition, out var value15))
		{
			_deathPreventionVfxPosition = value15.As<Vector2>();
		}
	}
}
